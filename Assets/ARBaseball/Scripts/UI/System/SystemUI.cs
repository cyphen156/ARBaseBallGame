using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 항상 활성화 되어 있는 
/// 게임 시스템을 UI를 관리하는 클래스입니다.
/// </summary>
public class SystemUI : MonoBehaviour
{
    public GameObject turnResultPrefab;

    private TextMeshProUGUI _textTimer;
    
    [SerializeField] private Transform ballContainer;
    [SerializeField] private Transform strikeContainer;
    [SerializeField] private Transform outContainer;
    private List<Transform> _ball = new List<Transform>();
    private List<Transform> _strike = new List<Transform>();
    private List<Transform> _out = new List<Transform>();

    private int[] _gOCounts = { 4, 3, 3 };

    [SerializeField] private Color ballColor = Color.yellow;
    [SerializeField] private Color strikeColor = new Color(1f, 0.5f, 0f); // orange
    [SerializeField] private Color outColor = Color.red;
    private Color[] _colors;

    private static int colorIndex = 0; // Static index to cycle through colors
   
    private void Awake()
    {
        _textTimer = GetComponent<TextMeshProUGUI>();
        GameObject turnSessionResult = GameObject.Find("PanelTurnSessionResult");
        _colors = new Color[] { ballColor, strikeColor, outColor };

        InitResultIcons(ballContainer, _gOCounts[0], _ball);
        InitResultIcons(strikeContainer, _gOCounts[1], _strike);
        InitResultIcons(outContainer, _gOCounts[2], _out);
    }

    private void InitResultIcons(Transform parent, int count, List<Transform> list)
    {
        list.Clear();
        for (int i = 0; i < count; i++)
        {
            var icon = Instantiate(turnResultPrefab, parent);
            icon.name = $"{parent.name}_Icon_{i}";
            var image = icon.GetComponent<Image>();
            if (image != null)
            {
                image.color = _colors[colorIndex];
            }
            list.Add(icon.transform);
            icon.gameObject.SetActive(false); // Initially hide the icons
        }
        colorIndex += 1; // Increment color index for next icon type
    }

    public void ApplyTurnResult(string name, int count)
    {
        int index = 0;
        List<Transform> targetList = null;
        switch (name)
        {
            case "Ball":
                targetList = _ball;
                index = count - 1;
                break;
            case "Strike":
                targetList = _strike;
                index = count - 1;
                break;
            case "Out":
                targetList = _out;
                index = count - 1;
                break;
        }
        for (int i = 0; i < targetList.Count; i++)
        {
            if (i < count)
            {
                targetList[i].gameObject.SetActive(true);
            }
            else
            {
                targetList[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateTextTimer(float time)
    {
        int currentTime = (int)time;
        _textTimer.text = currentTime.ToString();
    }
}
