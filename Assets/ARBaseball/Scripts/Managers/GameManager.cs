using System;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

using Random = UnityEngine.Random;
/// <summary>
/// 게임의 전반적인 상태를 관리합니다.
/// 사용자의 입력에 따라 게임의 진행 상태를 업데이트하고, 게임 오브젝트를 관리합니다.
/// AI와의 상호작용을 처리하며, 게임의 시작과 종료를 관리합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public GameObject ballPrefab;                       // 공 프리팹
    public GameObject batterPrefab;                     // 타자 프리팹
    public GameObject pitcherPrefab;                    // 투수 프리팹
    private BaseballGameCreator baseballGameCreator;    // 야구 게임 생성기 
    private GameObject simulationCamera;                    // SimulationCamera

    /// <summary>
    /// todos : 
    /// 1. 플레이어 입력에 따라 게임 상태를 업데이트합니다.
    /// 2. AI와의 상호작용을 처리합니다.
    /// 3. 게임 오브젝트를 관리합니다.
    /// 4. 게임의 시작과 종료를 관리합니다.
    /// </summary>
    [Header("GameState")]
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameState currentGameState;// 현재 게임 상태
    [SerializeField] private PlayMode currentPlayMode;  // 플레이 모드 선택 여부
    [SerializeField] private PitchType currentPitchType;

    [Header("플레이어블 인스턴스 관련 헤더")]
    [SerializeField] private GameObject playerGameObject;
    [SerializeField] private GameObject aIGameObject;
    [SerializeField] private GameObject bat;
    [SerializeField] private GameObject baseballField;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private Transform pitcherPosition;
    [SerializeField] private Transform batterPosition;
    [SerializeField] private Transform StrikeZone; 
    [SerializeField] private Vector3 defaultPitcherPosition;

    private Vector3 cameraOffset;
    private Vector3 pitchOffset = new Vector3(-2f, -1f, 8f); 
    private Vector3 batOffset = new Vector3(3f, 0f, -12f); 

    public event Action<GameState> OnGameStateChanged;
    public event Action<PlayMode> OnPlayModeChanged;
    public event Action<int> OnRestTimeChanged;

    private float defaultRestTime = 30f;
    private float currentRestTime;
    private float elapsedTime = 0f;
    public float minForce = 10f;
    public float maxForce = 25f;
    private Vector3 pitchAnimationCameraPosition;

    private TurnSession turnSession;
    private bool _isInputLocked = false;
    private bool _isAIPitching = false;
    private float _AIPitchingTime = 3f;
    private float _accumulatedTime = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        turnSession.OnTurnResultAccepted -= ApplyTrunResult;
        turnSession.NoticeTurnSessionResult -= ApplyTurnSessionResult;
    }
    private void Start()
    {
        baseballGameCreator = GetComponent<BaseballGameCreator>();
        UIManager.Instance.InitializeUI();
        StartCoroutine(C_InitializeGame());
    }

    private void Update()
    {
        // 게임 상태에 따라 업데이트 로직을 처리합니다.
        switch (currentGameState)
        {
            case GameState.None:
                // 초기 상태, 게임이 시작되지 않았습니다.
                // 게임 로직이 비정상적인 상태일 때 처리할 로직을 여기에 작성합니다.
                return;
            case GameState.Initializing:
                // 게임이 초기화 중입니다.
                // AR을 통해 플레이어와 AI의 위치를 설정하고, 
                // 유저는 게임 모드를 선택해야 합니다.
                break;
            case GameState.Ready:
                // 플레이어가 게임을 시작할 준비가 되었습니다.
                // 예: UI를 통해 플레이어가 게임을 시작할 수 있도록 안내합니다.
                break;
            case GameState.Play:
                {
                    if (currentPlayMode == PlayMode.PitcherMode)
                    {
                        // 피처 모드에서의 게임 로직을 처리합니다.
                        // 플레이어 공 던지기 타이머 작동
                        elapsedTime += Time.deltaTime;

                        // 타이머가 1초 이상 경과하면 현재 남은 시간을 감소시킵니다.
                        if (elapsedTime >= 1f)
                        {
                            currentRestTime -= 1f;
                            elapsedTime = 0f; // 타이머 초기화

                            if (currentRestTime < 0)
                            {
                                // 타이머가 0 이하로 내려가면 다음 턴으로 넘어갑니다.
                                turnSession.AcceptTurnResult(TurnResult.Ball);
                                ResetRestTime();
                            }
                        }
                    }
                    else if (currentPlayMode == PlayMode.BatterMode)
                    {

                        // 배터 모드에서의 게임 로직을 처리합니다.
                        // AI의 공 던지기를 결정합니다.
                        if (!_isAIPitching)
                        // AI가 피칭중이 아닐때
                        {
                            _accumulatedTime += Time.deltaTime;

                            if (_AIPitchingTime < _accumulatedTime)
                            // AI가 공을 던질 시간인지 확인합니다.
                            {
                                _isAIPitching = true;
                                aIGameObject.GetComponent<AnimationContoller>().PlayAnimation("Shoot");

                                Vector3 newPosition = aIGameObject.transform.position;
                                newPosition.y = 1f; // 공이 바닥에 닿지 않도록 약간 위로 설정
                                GameObject ball = Instantiate(ballPrefab, newPosition, aIGameObject.transform.rotation);


                                Vector3 targetPos = StrikeZone.position + new Vector3(
                                    Random.Range(-1f, 1.1f),
                                    Random.Range(-1f, 1.1f),
                                    0f
                                );

                                Vector3 direction = (targetPos - newPosition).normalized;
                                Debug.Log($"AI가 공을 던집니다. 방향: {direction}");
                                ball.GetComponent<Ball>().Shoot(newPosition, direction, 10f, PitchType.Fastball);

                                _AIPitchingTime = Random.Range(2f, 5f); // AI가 공을 던지는 시간 간격을 랜덤으로 설정
                                _accumulatedTime = 0f; // 누적 시간 초기화
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("현재 플레이 모드가 설정되지 않았습니다.");
                        // 게임이 진행 중입니다.
                        // 예: 플레이어와 AI의 턴을 처리합니다.
                    }
                }
                break;
            case GameState.End:
                // 게임이 종료되었습니다.
                // 예: 승패를 결정하고, 결과를 표시합니다.
                break;
            default:
                // Critical error handling or logging
                break;
        }
    }

    /// <summary>
    /// // AR을 통해 플레이어와 AI의 위치를 설정하고, 게임 오브젝트를 초기화합니다.
    /// 위치 설정이 완료되면 플레이어는 플레이 모드를 선택할 수 있습니다.
    /// 플레이 모드가 선택되면 게임 상태를 Ready로 변경합니다.
    /// </summary>
    private IEnumerator C_InitializeGame()
    {
        // 게임 초기화 로직을 여기에 작성합니다.
        // 예: 플레이어와 AI 컨트롤러 초기화, UI 설정 등
        Debug.Log("게임 초기화 중...");
        ChangeGameState(GameState.Initializing);
        ChangePlayMode(PlayMode.None); // 초기 플레이 모드 설정

        // 초기 1회 강제 UI 업데이트
        OnPlayModeChanged?.Invoke(currentPlayMode);
        UIManager.Instance.RequestUpdateUIForInitComplete(0);

        // 로직 1
        // AR을 통해 플레이어와 AI의 위치를 설정하는 로직을 여기에 작성합니다.
        yield return new WaitUntil(()=> baseballGameCreator.isCreated);
        simulationCamera = Camera.main.gameObject;

        // 캐릭터 생성 좌표 초기화
        pitcherPosition = GameObject.Find("PitcherPosition").transform;
        batterPosition = GameObject.Find("BatterPosition").transform;
        Debug.Log("AR BaseballGameSetUP Complete");
        UIManager.Instance.RequestUpdateUIForInitComplete(1);

        // 로직 2
        // UI를 통해 플레이 모드를 선택하도록 안내합니다.
        yield return new WaitUntil(() => currentPlayMode != PlayMode.None);
        UIManager.Instance.RequestUpdateUIForInitComplete(2);

        Debug.Log("게임 플레이 준비 완료. 현재 상태: " + currentGameState);
        OnGameStateChanged?.Invoke(currentGameState);

        turnSession = GetComponent<TurnSession>();
        turnSession.OnTurnResultAccepted += ApplyTrunResult;
        turnSession.NoticeTurnSessionResult += ApplyTurnSessionResult;
    }

    /// <summary>
    /// 게임 상태를 변경합니다.
    /// </summary>
    /// <param name="newGameState"></param>
    private void ChangeGameState(GameState newGameState)
    {
        if (currentGameState == newGameState)
        {
            return; // 현재 상태와 동일하면 변경하지 않음
        }
        currentGameState = newGameState;

        if (currentGameState == GameState.Play)
        {
            try
            {
                // 캐릭터를 생성하고
                SpawnCharacters();

            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to create player characeter: {e},{e.Message}, {e.StackTrace} ");
                return; // 오류 발생 시 게임 상태 변경을 중단
            }

            try
            {
                baseballField = GameObject.Find("BaseballField");
                if (baseballField == null)
                {
                    throw new NullReferenceException("BaseballField 오브젝트가 존재하지 않습니다. ARBaseballGameCreator가 올바르게 설정되었는지 확인하세요.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Faield to get baseballfield: {e},{e.Message}, {e.StackTrace} ");
                return; // 오류 발생 시 게임 상태 변경을 중단
            }

            try
            {
                // 카메라의 현재 위치 캐싱
                cameraTransform = simulationCamera.transform;
                targetTransform = simulationCamera.transform;

                if (cameraTransform == null ||targetTransform == null)
                {
                 throw new NullReferenceException("CAm or target transform is null입니다. ARBaseballGameCreator가 올바르게 설정되었는지 확인하세요.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"camera pos exception: {e},{e.Message}, {e.StackTrace} ");
                return; // 오류 발생 시 게임 상태 변경을 중단
            }



            // 플레이 모드에 따라위치를 옮긴 다음 서로 마주보게 해야함
            if (currentPlayMode == PlayMode.PitcherMode)
            {
                playerGameObject.transform.position = pitcherPosition.position;
                playerGameObject.transform.rotation = pitcherPosition.rotation;
                aIGameObject.transform.position = batterPosition.position;
                aIGameObject.transform.rotation = batterPosition.rotation;
                defaultPitcherPosition = new Vector3(pitcherPosition.position.x, 0, pitcherPosition.position.z);
                targetTransform = pitcherPosition;
                cameraOffset = pitchOffset;
                ResetRestTime(); // 게임이 시작되면 타이머를 초기화합니다.
            }
            else if (currentPlayMode == PlayMode.BatterMode)
            {
                playerGameObject.transform.position = batterPosition.position;
                playerGameObject.transform.rotation = batterPosition.rotation;
                aIGameObject.transform.position = pitcherPosition.position;
                StrikeZone = GameObject.Find("StrikeZone").transform;
                aIGameObject.transform.LookAt(StrikeZone);
                targetTransform = batterPosition;
                cameraOffset = batOffset;
                bat = GameObject.Find("Bat");
            }

            playerGameObject.transform.SetParent(baseballField.transform);
            aIGameObject.transform.SetParent(baseballField.transform);

            // 회전 변환 먼저 (M = TRS)
            baseballField.transform.rotation = Quaternion.LookRotation(targetTransform.forward, Vector3.up);

            // 이동 변환 후처리
            Vector3 newPosition = cameraTransform.position - targetTransform.position + baseballField.transform.rotation * cameraOffset;
            newPosition.y = cameraOffset.y;

            if (currentPlayMode == PlayMode.PitcherMode)
            {
                pitchAnimationCameraPosition = newPosition;
                newPosition = defaultPitcherPosition;
                playerGameObject.SetActive(false);
                aIGameObject .SetActive(false);
            }

            baseballField.transform.position = newPosition;
            Physics.SyncTransforms();
        }

        Debug.Log("게임 상태가 변경되었습니다: " + currentGameState);
        OnGameStateChanged?.Invoke(currentGameState);
    }

    /// <summary>
    /// 플레이 모드를 변경합니다.
    /// </summary>
    /// <param name="newMode"></param>
    private void ChangePlayMode(PlayMode newMode)
    {
        if (newMode == currentPlayMode)
        {
            return;
        }

        currentPlayMode = newMode;
        if (currentPlayMode == PlayMode.PitcherMode)
        {
            playerGameObject = pitcherPrefab;
            aIGameObject = batterPrefab;
        }
        else if (currentPlayMode == PlayMode.BatterMode)
        {
            playerGameObject = batterPrefab;
            aIGameObject = pitcherPrefab;
        }

        Debug.Log("플레이 모드가 변경되었습니다: " + currentPlayMode);
        OnPlayModeChanged?.Invoke(currentPlayMode);
        ChangeGameState(GameState.Ready); // 플레이 모드가 변경되면 게임 상태를 Ready로 변경합니다.
    }

    private void ChangePitchType(PitchType newType)
    {
        if (newType == currentPitchType)
        {
            return;
        }

        currentPitchType = newType;
        Debug.Log("피칭 모드가 변경되었습니다: " + currentPitchType);
    }

    /// <summary>
    /// 게임 상태가 Play가 되었을 때 호출될 함수
    /// 게임 모드에 따라 플레이어 또는 AI를 각기 할당된 포지션에 생성함
    /// </summary>
    private void SpawnCharacters()
    {
        playerGameObject = Instantiate(playerGameObject);
        aIGameObject = Instantiate(aIGameObject);
        playerGameObject.name = "Player";
        aIGameObject.name = "AI";
    }

    private void ResetRestTime()
    {
        currentRestTime = defaultRestTime;
        OnRestTimeChanged?.Invoke((int)currentRestTime);
    }

    public void TryChangeGameState(GameState state)
    {
        ChangeGameState(state);
    }

    public void TryChangeApplicationState(string command)
    {
        switch (command)
        {
            case "Exit":
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
                break;
            default:
                Debug.LogWarning("알 수 없는 명령어입니다: " + command);
                break;
        }
    }

    public void TryChangePlayMode(PlayMode playMode)
    {   
        ChangePlayMode(playMode);
    }

    public void TryChangePitchType(PitchType pitchType)
    {
        ChangePitchType(pitchType);
    }

    public void ProcessInput(Vector2 StartPosition, Vector2 EndPosition, double elapsedDraggingTime)
    {
        if (_isInputLocked)
        {
            return;
        }

        if (currentGameState == GameState.Initializing)
        {
            // 스트라이크 존 생성 로직을 여기에 작성합니다.
            if (baseballGameCreator.isCreated)
            {
                return;
            }

            baseballGameCreator.GenerateBaseballGame(EndPosition);
        }

        else if (currentGameState == GameState.Play)
        {
            _isInputLocked = true;
            Vector2 drag = EndPosition - StartPosition;
            float force = Mathf.Clamp((float)elapsedDraggingTime * 10f, minForce, maxForce);

            Vector3 spawnPosition = cameraTransform.position + cameraTransform.forward * 0.5f;
            Vector3 baseDirection = cameraTransform.forward;

            float screenRatio = (float)Screen.height / Screen.width;

            float horizontalOffset = Mathf.Clamp(drag.x / Screen.width, -0.3f, 0.3f);
            float verticalOffset = Mathf.Clamp((drag.y / Screen.height) * screenRatio, -0.3f, 0.3f);

            Vector3 direction = (baseDirection
                                + cameraTransform.right * horizontalOffset
                                + cameraTransform.up * verticalOffset).normalized;

            if (currentPlayMode == PlayMode.PitcherMode)
            {
                StartCoroutine(C_Shoot(spawnPosition, direction, force));
            }
            else if (currentPlayMode == PlayMode.BatterMode)
            {
                if (bat == null)
                {
                    Debug.LogWarning("[GameManager] Player에 Bat 컴포넌트가 없습니다.");
                    return;
                }

                bat.GetComponent<Bat>().Swing(direction, force);
                playerGameObject.GetComponent<AnimationContoller>().PlayAnimation("Swing");
                Debug.Log("배트를 휘둘럿다!");
            }
        }
    }

    private IEnumerator C_Shoot(Vector3 spawnPosition, Vector3 direction, float force)
    {
        playerGameObject.SetActive(true);
        baseballField.transform.position = pitchAnimationCameraPosition;

        Animator animator = playerGameObject.GetComponent<Animator>();
        animator.SetTrigger("Shoot");

        // 애니메이션 대기 - 클립 길이를 아는 경우
        float shootAnimDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(shootAnimDuration);

        // 필드 원위치 복귀
        baseballField.transform.position = defaultPitcherPosition;
        playerGameObject.SetActive(false);

        // 공 던지기
        GameObject go = Instantiate(ballPrefab, spawnPosition, cameraTransform.rotation);
        go.GetComponent<Ball>().Shoot(spawnPosition, direction, force, currentPitchType);
    }

    /// <summary>
    /// 공이 배트에 맞았을 때 재생되는 거
    /// 1, 게임을 멈춤
    /// 2. 카메라를 배트와 공이 맞은 것을 볼 수 있도록 위치 이동
    /// 3. 0.4초 뒤에 게임을 다시 재생
    /// 4. 0.6초 동안 공이 날아가는 것을 카메라가 트래킹
    /// 5. 이 후 다시 카메라가 원래 위치로 복귀
    /// </summary>
    private void PlayHitSequence()
    {

    }

    public void OnBallHit()
    {
        PlayHitSequence();
    }

    private void ApplyTrunResult(TurnResult turnResult, int count)
    {
        UIManager.Instance.ApplyTurnResult(turnResult, count);
        _isInputLocked = false;
    }

    private void ApplyTurnSessionResult(TurnSessionResult result, int count)
    {
        UIManager.Instance.ApplyTurnSessionResult(result, count);

        if (result == TurnSessionResult.SessionEnd)
        {
            ChangeGameState(GameState.End);
        }
        _isInputLocked = false;
    }

    public void RequestTurnResult(TurnResult result)
    {
        turnSession.AcceptTurnResult(result);

        if (currentPlayMode == PlayMode.BatterMode)
        {
            _isAIPitching = false;
        }
    }

    public PlayMode GetPlayMode()
    {
        return currentPlayMode;
    }
}
