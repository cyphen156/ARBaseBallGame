using UnityEngine;

/// <summary>
/// 투수용 UI 컴포넌트입니다.
/// </summary>
public class PitcherUI : MonoBehaviour
{ 
    [SerializeField] private PitchType currentPitchType;
    [SerializeField] private PitchTypeSelector pitchTypeSelector;

    private void Awake()
    {
        pitchTypeSelector = GetComponentInChildren<PitchTypeSelector>();
        // 초기 피치 유형 설정
        currentPitchType = PitchType.None;
        pitchTypeSelector.OnPitchTypeChanged += OnPitchTypeChanged;
    }

    private void OnPitchTypeChanged(PitchType pitchType)
    {
        currentPitchType = pitchType;
    }
}
