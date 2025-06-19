using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 게임의 전반적인 상태를 관리합니다.
/// 사용자의 입력에 따라 게임의 진행 상태를 업데이트하고, 게임 오브젝트를 관리합니다.
/// AI와의 상호작용을 처리하며, 게임의 시작과 종료를 관리합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    // 테스트용
    public GameObject ballPrefab;  // 공 프리팹

    /// <summary>
    /// todos : 
    /// 1. 플레이어 입력에 따라 게임 상태를 업데이트합니다.
    /// 2. AI와의 상호작용을 처리합니다.
    /// 3. 게임 오브젝트를 관리합니다.
    /// 4. 게임의 시작과 종료를 관리합니다.
    /// </summary>
    [Header("GameState")]
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameState currentGameState;  // 현재 게임 상태

    [SerializeField] private PlayMode currentPlayMode;  // 플레이 모드 선택 여부

    public event Action<GameState> OnGameStateChanged;
    public event Action<PlayMode> OnPlayModeChanged;
    public event Action<int> OnRestTimeChanged;
    private float defaultRestTime = 30f;
    private float currentRestTime;
    private float elapsedTime = 0f;
    //[Header("PlayerControll")]
    //public PlayerController PlayerController { get; private set; }

    //[Header("AIControll")]
    //public AIController AIController { get; private set; }


    //[Header("UIControll")]
    //public UIController UIController { get; private set; }


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

    private void Start()
    {
        UIManager.Instance.InitializeUI();
        InitializeGame();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            GameObject go = Instantiate(ballPrefab, Camera.main.transform.position, Quaternion.identity);
            go.GetComponent<Ball>().Shoot(Camera.main.transform.position, Camera.main.transform.forward, 10f, PitchType.Fastball);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            GameObject go = Instantiate(ballPrefab, Camera.main.transform.position, Quaternion.identity);
            go.GetComponent<Ball>().Shoot(Camera.main.transform.position, Camera.main.transform.forward, 10f, PitchType.Curve);
        }
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
                            }
                            OnRestTimeChanged?.Invoke((int)currentRestTime);
                        }
                    }
                    else if (currentPlayMode == PlayMode.BatterMode)
                    {
                        // 배터 모드에서의 게임 로직을 처리합니다.
                        // 예: 플레이어가 공을 치는 로직을 처리합니다.
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
    private void InitializeGame()
    {
        // 게임 초기화 로직을 여기에 작성합니다.
        // 예: 플레이어와 AI 컨트롤러 초기화, UI 설정 등
        Debug.Log("게임 초기화 중...");
        ChangeGameState(GameState.Initializing);
        ChangePlayMode(PlayMode.None); // 초기 플레이 모드 설정
        ChangePlayMode(PlayMode.PitcherMode);
        // 로직 1
        // AR을 통해 플레이어와 AI의 위치를 설정하는 로직을 여기에 작성합니다.
        //yield return new WaitUntil(() => arIsReady);

        // 로직 2
        // UI를 통해 플레이 모드를 선택하도록 안내합니다.

        ChangeGameState(GameState.Play);
        Debug.Log("게임 플레이 준비 완료. 현재 상태: " + currentGameState);
        OnGameStateChanged?.Invoke(currentGameState);
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
        if (currentGameState == GameState.Play && currentPlayMode == PlayMode.PitcherMode)
        {
            ResetRestTime(); // 게임이 시작되면 타이머를 초기화합니다.
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
        Debug.Log("플레이 모드가 변경되었습니다: " + currentPlayMode);
        OnPlayModeChanged?.Invoke(currentPlayMode);
    }

    private void ResetRestTime()
    {
        currentRestTime = defaultRestTime;
    }

    internal void TryChangeGameState(GameState state)
    {
        ChangeGameState(state);
    }
}
