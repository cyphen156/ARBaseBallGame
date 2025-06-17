using System.Collections;
using UnityEngine;

/// <summary>
/// 게임의 전반적인 상태를 관리합니다.
/// 사용자의 입력에 따라 게임의 진행 상태를 업데이트하고, 게임 오브젝트를 관리합니다.
/// AI와의 상호작용을 처리하며, 게임의 시작과 종료를 관리합니다.
/// </summary>

public class GameManager : MonoBehaviour
{
    [Header("GameState")]
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameState currentGameState;  // 현재 게임 상태

    [SerializeField] private PlayMode currentPlayMode;  // 플레이 모드 선택 여부

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

    private IEnumerator Start()
    {
        yield return StartCoroutine(InitializeGame());
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
                // 게임이 진행 중입니다.
                // 예: 플레이어와 AI의 턴을 처리합니다.
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
    private IEnumerator InitializeGame()
    {
        // 게임 초기화 로직을 여기에 작성합니다.
        // 예: 플레이어와 AI 컨트롤러 초기화, UI 설정 등
        Debug.Log("게임 초기화 중...");
        ChangeGameState(GameState.Initializing);
        ChangePlayMode(PlayMode.None); // 초기 플레이 모드 설정

        // 로직 1
        // AR을 통해 플레이어와 AI의 위치를 설정하는 로직을 여기에 작성합니다.
        //yield return new WaitUntil(() => arIsReady);

        // 로직 2
        // UI를 통해 플레이 모드를 선택하도록 안내합니다.
        yield return new WaitUntil(() => currentPlayMode != PlayMode.None);

        ChangeGameState(GameState.Ready);
        Debug.Log("게임 플레이 준비 완료. 현재 상태: " + currentGameState);
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
        Debug.Log("게임 상태가 변경되었습니다: " + currentGameState);
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
    }
}
