using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내에 존재하는 UI 요소들을 관리하는 클래스입니다.
/// 게임 매니저와 상호작용하여 UI를 업데이트
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UIPrefab")]
    [SerializeField] private List<GameObject> UIPrefabs;
    private Dictionary<string, GameObject> uiElements = new Dictionary<string, GameObject>();

    [Header("UIControll")]
    [SerializeField] private GameObject currentPlayModeUI; // 현재 활성화된 UI 요소

    private void Awake()
    {
        // UI 프리팹을 초기화합니다.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        // 게임 매니저의 이벤트에 구독합니다.
        GameManager.Instance.OnGameStateChanged += ApplyGameStateInUI;
        GameManager.Instance.OnPlayModeChanged += ApplyPlayModeUI;
        GameManager.Instance.OnRestTimeChanged += UpdateSystemTimerUI;
    }

    private void OnDisable()
    {
        // 게임 매니저의 이벤트 구독을 해제합니다.
        GameManager.Instance.OnGameStateChanged -= ApplyGameStateInUI;
        GameManager.Instance.OnPlayModeChanged -= ApplyPlayModeUI;
        GameManager.Instance.OnRestTimeChanged -= UpdateSystemTimerUI;
    }

    /// <summary>
    /// UI 프리팹을 로드하고 초기화합니다.
    // 각 UI 요소를 초기화하는 로직을 여기에 작성합니다.
    /// </summary>
    public void InitializeUI()
    {
        foreach (var uiPrefab in UIPrefabs)
        {
            if (uiPrefab != null)
            {
                var instance = Instantiate(uiPrefab, transform);
                instance.name = uiPrefab.name;
                uiElements[uiPrefab.name] = instance;
                instance.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 게임 상태에 따라 UI를 스위칭합니다.
    /// </summary>
    private void ApplyPlayModeUI(PlayMode playMode)
    {
        foreach (var uiElement in uiElements.Values)
        {
            if (uiElement.name == "SystemUI")
            {
                uiElement.SetActive(true); // 시스템 UI는 항상 활성화합니다.
            }
            else if (uiElement.name == playMode.ToString() + "UI")
            {
                uiElement.SetActive(true); // 선택된 플레이 모드의 UI 요소를 활성화합니다.
                currentPlayModeUI = uiElement; // 현재 활성화된 UI 요소를 저장합니다.
            }
            else
            {
                uiElement.SetActive(false); // 다른 UI 요소는 비활성화합니다.
            }
        }
    }

    /// <summary>
    /// 현재 남은 시간을 시스템 UI에 업데이트합니다.
    /// </summary>
    /// <param name="currentTime"></param>
    private void UpdateSystemTimerUI(int currentTime)
    {
        if (uiElements.TryGetValue("SystemUI", out GameObject systemUI))
        {
            uiElements["SystemUI"].GetComponent<SystemUI>().UpdateTextTimer(currentTime);
        }
    }

    private void UpdateUIForInitComplete(int sequenceNumber)
    {
        string target;

        switch (sequenceNumber)
        {
            case 0:
                target = "TextARInput";
                break;

            case 1:
                target = "PlayModePanel";
                break;

            case 2:
                target = "ButtonPanel";
                break;
            default:
                target = "null";
                Debug.Log("그런 시퀀스 넘버는 없습니다.");
                break;
        }
        SetOnlyActiveInSystemUI(target);
    }

    private void SetOnlyActiveInSystemUI(string targetName)
    {
        if (!uiElements.TryGetValue("SystemUI", out GameObject systemUI))
        {
            return;
        }

        if (targetName == "")
        {
            Debug.Log("그런 UI요소는 없습니다.");
            return;
        }

        foreach (Transform child in systemUI.transform)
        {
            bool isTarget = child.name == targetName;
            child.gameObject.SetActive(isTarget);
        }
    }

    /// <summary>
    /// 턴의 결과를 시스템 UI에 적용합니다.
    /// </summary>
    /// <param name="result"></param>
    public void ApplyTurnResult(TurnResult result, int count)
    {
        if (uiElements.TryGetValue("SystemUI", out GameObject systemUI))
        {
            uiElements["SystemUI"].GetComponent<SystemUI>().PlayTurnResultTextAnimation(result.ToString(), 0.1f);

            uiElements["SystemUI"].GetComponent<SystemUI>().ApplyTurnResult(result.ToString(), count);
        }
    }

    public void ApplyTurnSessionResult(TurnSessionResult result, int count)
    {
        if (uiElements.TryGetValue("SystemUI", out GameObject systemUI))
        {
            uiElements["SystemUI"].GetComponent<SystemUI>().PlayTurnResultTextAnimation(result.ToString(), 0.1f);
        }

        if (result == TurnSessionResult.Out)
        {
            uiElements["SystemUI"].GetComponent<SystemUI>().ApplyTurnResult(result.ToString(), count);
        }
    }

    public void RequestCommand(PitchType pitchType)
    {
        switch (pitchType)
        {
            case PitchType.None:
                GameManager.Instance.TryChangePitchType(PitchType.None);
                break;
            case PitchType.Fastball:
                GameManager.Instance.TryChangePitchType(PitchType.Fastball);
                break;
            case PitchType.Curve:
                GameManager.Instance.TryChangePitchType(PitchType.Curve);
                break;
            default:
                Debug.LogWarning("[UIManager] 지원하지 않는 피칭 모드입니다: " + pitchType);
                break;
        }
    }

    public void RequestUpdateUIForInitComplete(int sequenceNumber)
    {
        UpdateUIForInitComplete(sequenceNumber);
    }

    public void RequestCommand(PlayMode playMode)
    {
        switch (playMode)
        {
            case PlayMode.PitcherMode:
                GameManager.Instance.TryChangePlayMode(PlayMode.PitcherMode);
                break;
            case PlayMode.BatterMode:
                GameManager.Instance.TryChangePlayMode(PlayMode.BatterMode);
                break;
            default:
                Debug.LogWarning("[UIManager] 지원하지 않는 플레이 모드입니다: " + playMode);
                break;
        }
    }

    public void RequestCommand(Command command, object parameter = null)
    {
        switch (command)
        {
            case Command.Initialize:
                GameManager.Instance.TryChangeGameState(GameState.Initializing);
                break;
            case Command.ReadyGame:
                GameManager.Instance.TryChangeGameState(GameState.Ready);
                break;
            case Command.PlayGame:
                GameManager.Instance.TryChangeGameState(GameState.Play);
                break;
            case Command.EndGame:
                GameManager.Instance.TryChangeGameState(GameState.End);
                break;
            case Command.Exit:
                GameManager.Instance.TryChangeApplicationState("Exit");
                break;
            default:
                Debug.LogWarning("Unknown command: " + command);
                break;
        }
    }

    private void ApplyGameStateInUI(GameState state)
    {
        bool activeFlag = true;

        if (!uiElements.TryGetValue("SystemUI", out GameObject systemUI))
            return;

        var system = systemUI.GetComponent<SystemUI>();

        switch (state)
        {
            case GameState.Initializing:
                system.SetArInput(activeFlag);
                break;

            case GameState.Ready:
                system.SetArInput(!activeFlag);
                system.SetPlayModePanel(!activeFlag);
                system.SetButtonPanel(activeFlag);
                break;

            case GameState.Play:
                system.SetButtonPanel(!activeFlag);
                system.SetGamePlayUI(activeFlag);
                break;
        }
    }
}
