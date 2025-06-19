using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    public Rigidbody rb;

    private float _speed = 80f;
    private Vector3 _direction;
    private Vector3 _startPosition;
    private Vector3 _targetPosition = new Vector3(0, 1, 1);
    private Vector3 _startPositionOffSet = new Vector3(0, 0f, 5f);
    private float _force = 5f;
    private float _lifetime = 3f;
    private float _resistance = 0.1f;
    private float _velocity;
    private PitchType _pitchType;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    public void Shoot(Vector3 startPosition, Vector3 direction, float force, PitchType type)
    {
        _startPosition = startPosition + _startPositionOffSet;
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
                rb.angularVelocity = transform.right * 30f; // Topspin
                break;
        }
    }

    private void FixedUpdate()
    {
        if (_pitchType == PitchType.Curve)
        {
            Vector3 curveForce = Vector3.right * Mathf.Sin(Time.time * 6f) * 2f;
            rb.AddForce(curveForce, ForceMode.Acceleration);
        }

        rb.linearVelocity *= (1f - _resistance * Time.fixedDeltaTime);
    }
}