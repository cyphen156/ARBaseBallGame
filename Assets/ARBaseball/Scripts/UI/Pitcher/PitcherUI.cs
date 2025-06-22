using UnityEngine;

/// <summary>
/// 투수용 UI 컴포넌트입니다.
/// </summary>
public class PitcherUI : UIBehaviour
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

    /// <summary>
    /// 토글 이벤트 발생으로 인한 데이터 갱신
    /// </summary>
    /// <param name="pitchType"></param>
    private void OnPitchTypeChanged(PitchType pitchType)
    {
        currentPitchType = pitchType;
        UIManager.Instance.RequestCommand(currentPitchType);
    }

}
