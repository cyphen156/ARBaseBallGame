using UnityEngine;


/// <summary>
/// 스트라이크존을 지나는 공의 위치를 알려주기 위한 UI
/// </summary>
public class StrikeZonePanel : MonoBehaviour
{
    [SerializeField]
    private GameObject ballIndicatorUI;

    private Canvas _canvas;
    private Collider _collider;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //ball object 판별
        //if (other.TryGetComponent<BallController>(out BallController ball))
        {
            GameObject indicatorObj = Instantiate(ballIndicatorUI);
            indicatorObj.transform.SetParent(_canvas.transform, false);
            indicatorObj.transform.position = _collider.ClosestPoint(other.transform.position);
        }
    }
}
