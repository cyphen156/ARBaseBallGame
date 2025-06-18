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
        GameManager.Instance.OnPlayModeChanged += ApplyPlayModeUI;
        GameManager.Instance.OnRestTimeChanged += UpdateSystemTimerUI;
    }

    private void OnDisable()
    {
        // 게임 매니저의 이벤트 구독을 해제합니다.
        GameManager.Instance.OnPlayModeChanged -= ApplyPlayModeUI;
    }
    private void Start()
    {
        // 게임 시작 시 UI를 초기화합니다.
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

    private void UpdateSystemTimerUI(int currentTime)
    {
        if (uiElements.TryGetValue("SystemUI", out GameObject systemUI))
        {
            uiElements["SystemUI"].GetComponent<SystemUI>().UpdateTextTimer(currentTime);
        }
    }
}
