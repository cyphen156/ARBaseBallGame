using UnityEngine;


/// <summary>
/// 마그누스 효과 구현하기
/// 공의 물리적 특성을 제어하는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Pitch Settings")]
    [SerializeField] private float _lifetime = 15f;
    [SerializeField] private float _resistance = 0.1f;
    [SerializeField] private float magnusStrength = 0.005f;

    private Vector3 _startPosition;
    private Vector3 _direction;
    private float _force;
    private PitchType _pitchType;
    private Vector3 _defalutGravity = new Vector3(0, -9.81f * 0.3f, 0);
    private Vector3 _flippedGravity = new Vector3(0, -9.81f * -0.2f, 0);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Physics.gravity = _defalutGravity;   // 중력값 보정
    }

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    public void Shoot(Vector3 startPosition, Vector3 direction, float force, PitchType type)
    {
        _startPosition = startPosition;
        _direction = direction.normalized;
        _force = force;
        _pitchType = type;

        transform.position = _startPosition;

        rb.linearVelocity = _direction * _force;

        Debug.Log($"[Ball] 던짐: 시작위치 {_startPosition}, 방향 {_direction}, 힘 {_force}, 종류 {_pitchType}");
        ApplySpin(_pitchType);
    }

    private void ApplySpin(PitchType type)
    {
        switch (type)
        {
            case PitchType.Fastball:
                rb.angularVelocity = transform.right * -30f; // Backspin
                rb.useGravity = false;
                break;
            case PitchType.Curve:
                Vector3 cross = Vector3.Cross(transform.forward, _direction);
                float side = Vector3.Dot(cross, Vector3.up); // 좌/우 판별

                float directionSign = Mathf.Sign(side); // +1이면 Curve Left, -1이면 Curve Right

                float verticalFlip = Mathf.Sign(_direction.y); // 위로 던지면 +1, 아래면 -1
                Vector3 spinAxis = transform.up;

                // 만약 아래로 던졌다면
                // X축 기준 플립
                if (verticalFlip < 0)
                {
                    spinAxis = Vector3.Reflect(spinAxis, transform.right);
                    // 중력값 보정 새로 적용
                    Physics.gravity = _flippedGravity;
                }

                float forceFactor = Mathf.InverseLerp(1f, 10f, _force); 
                float finalSpin = Mathf.Lerp(1f, 2.5f, forceFactor);

                rb.angularVelocity = spinAxis * directionSign * -finalSpin * 0.8f;
                break;
        }
    }

    private void FixedUpdate()
    {
        if (_pitchType == PitchType.Curve)
        {
            Vector3 w = rb.angularVelocity;
            Vector3 v = rb.linearVelocity;

            if (w != Vector3.zero && v != Vector3.zero)
            {
                Vector3 magnusForce = Vector3.Cross(w, v).normalized * magnusStrength;
                rb.AddForce(magnusForce, ForceMode.Acceleration);
            }
        }

        rb.linearVelocity *= (1f - _resistance * Time.fixedDeltaTime);
        Debug.DrawRay(transform.position, rb.linearVelocity, Color.green, 0.1f);

    }

    private void OnTriggerEnter(Collider other)
    {
        TurnResult turnResult = TurnResult.Ball;

        int layer = other.gameObject.layer;
        PlayMode pMod = GameManager.Instance.GetPlayMode();

        if (pMod == PlayMode.PitcherMode)
        {
            if (layer == 14)
            {
                turnResult = TurnResult.Strike; // 스트라이크
            }
        }
        else if (pMod == PlayMode.BatterMode)
        {
            switch (layer)
            {
                case 11:    // homerun
                    turnResult = TurnResult.HomeRun;
                    break;
                case 12:    // foul
                    turnResult = TurnResult.Foul;
                    break;
                case 13:    // inner
                    turnResult = TurnResult.Hit;
                    break;
                case 14:    // strike
                    turnResult = TurnResult.Strike;
                    break;
                default:
                    break;
            }
        }
            
        Debug.Log($"[Ball] 충돌됨: {other.gameObject.name}, 결과: {turnResult}");
        GameManager.Instance.RequestTurnResult(turnResult);

        Destroy(gameObject);
    }

    public void Reflect(Vector3 direction, float force)
    {
        rb.useGravity = true;
        Vector3 incoming = rb.linearVelocity;
        Vector3 reflectDirection = Vector3.Reflect(incoming.normalized, direction.normalized);

        float finalSpeed = Mathf.Clamp(force + incoming.magnitude, 0f, 80f);
        rb.linearVelocity = reflectDirection * finalSpeed;

        Physics.gravity = _defalutGravity;
        
        Debug.Log($"[Ball] 반사됨1: 배트방향 {direction}, 전달된 힘 {force}");
        Debug.Log($"[Ball] 반사됨2: 방향 {reflectDirection}, 속도 {finalSpeed}");
    }
}
