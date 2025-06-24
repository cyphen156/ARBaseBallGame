using UnityEngine;

/// <summary>
/// 플레이어의 배트의 물리력을 제어하는 컴포넌트
/// 배트는 중력을 사용하지 않으며, Rigidbody를 통해 물리적 상호작용을 처리합니다.
/// 배트의 움직임을 제어합니다.
/// 공과의 충돌을 감지하고 공에게 배트 힘을 전달합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Bat : MonoBehaviour
{
    private Rigidbody rb;
    public Collider[] batColliders; // 혹시 몰라서 인스펙터 열었음
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // 배트는 중력을 사용하지 않도록 설정
        batColliders = GetComponentsInChildren<Collider>(); // 캡슐 콜라이더 전부 가져옴
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 공과 충돌 시, 공에게 배트의 힘을 전달
        if (collision.gameObject.CompareTag("Ball"))
        {
            collision.gameObject.TryGetComponent<Ball>(out Ball ball);

            if ( ball == null)
            {
                return;
            }
            Collider hitCollider = collision.contacts[0].thisCollider;

            for (int i = 0; i < batColliders.Length; ++i)
            {
                if (hitCollider == batColliders[i])
                {
                    float t = i / (float)(batColliders.Length - 1);

                    ball.Reflect();
                    break;
                }
            }
        }
    }

    public void Swing(Vector3 direction, float force)
    {
        // 배트의 스윙 방향과 힘을 적용
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    public void ResetRB()
    {
        rb.linearVelocity = Vector3.zero; // 배트의 속도를 초기화
    }

}
