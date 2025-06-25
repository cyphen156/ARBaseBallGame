using System.Data.Common;
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
    [SerializeField] private float magnusStrength = 0.1f;

    private Vector3 _startPosition;
    private Vector3 _direction;
    private float _force;
    private float _startTime;
    private PitchType _pitchType;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Physics.gravity = new Vector3(0, -9.81f * 0.3f, 0);    // 중력값 보정
    }

    private void Start()
    {
        _startTime = Time.time;
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

        ApplySpin(_pitchType);
    }

    private void ApplySpin(PitchType type)
    {
        switch (type)
        {
            case PitchType.Fastball:
                rb.angularVelocity = transform.right * -30f; // Backspin
                break;
            case PitchType.Curve:
                Vector3 cross = Vector3.Cross(transform.forward, _direction);
                float side = Vector3.Dot(cross, Vector3.up); // 좌/우 판별

                float directionSign = Mathf.Sign(side); // +1 또는 -1

                float verticalFlip = Mathf.Sign(_direction.y); // 위로 던지면 +1, 아래면 -1
                Vector3 spinAxis = transform.up;

                // X축 기준 플립
                if (verticalFlip < 0)
                    spinAxis = Vector3.Reflect(spinAxis, transform.right);

                float baseSpin = 3f;
                float forceFactor = Mathf.InverseLerp(10f, 25f, _force); 
                float finalSpin = Mathf.Lerp(1f, 8f, forceFactor); 

                rb.angularVelocity = spinAxis * directionSign * -finalSpin; break;
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
        Debug.Log("충돌햇음");
    }

    public void Reflect(Vector3 direction, float force)
    {
        Debug.Log("반사 2");
    }
}
