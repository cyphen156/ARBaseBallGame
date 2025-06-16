using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 토글 버튼이 있는 UI 요소로, 사용자가 피치 유형을 선택할 수 있도록 합니다.
/// 피치 유형 토글 버튼을 클릭하면 해당 버튼이 활성화되고, 다른 버튼은 비활성화됩니다.
/// 외부로 이벤트를 발생시킵니다.
/// </summary>
public class PitchTypeSelector : MonoBehaviour
{
    public enum PitchType 
    { 
        Fastball, 
        Curve,

        None
    }


    private Dictionary<Toggle, PitchType> toggleToPitchType;
    public Toggle[] toggles;

    public event Action<PitchType> OnPitchTypeChanged;

    private void Awake()
    {
        toggleToPitchType = new Dictionary<Toggle, PitchType>();
        toggles = GetComponentsInChildren<Toggle>();

        foreach (var toggle in toggles)
        {
            var type = GetPitchTypeFromName(toggle.name);
            toggleToPitchType[toggle] = type;

            toggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    Debug.Log($"[PitchTypeSelector] 선택된 구종: {type}");
                    OnPitchTypeChanged?.Invoke(type);
                }
                UpdateToggleColors();
            });
        }
    }

    private void Start()
    {
        // 첫 번째 토글 활성화
        if (toggles.Length > 0)
        {
            toggles[0].isOn = true;
        }
    }
    private PitchType GetPitchTypeFromName(string name)
    {
        foreach (PitchType type in Enum.GetValues(typeof(PitchType)))
        {
            if (name.Contains(type.ToString(), StringComparison.OrdinalIgnoreCase))
                return type;
        }

        Debug.LogWarning($"Toggle 이름에서 PitchType을 유추할 수 없습니다: {name}, 기본값 {PitchType.None} 반환");
        return PitchType.None;
    }

    private void UpdateToggleColors()
    {
        foreach (var pair in toggleToPitchType)
        {
            var toggle = pair.Key;
            var graphic = toggle.targetGraphic;

            if (graphic != null)
            {
                graphic.color = toggle.isOn ? Color.green : Color.white;
            }
        }
    }
}
