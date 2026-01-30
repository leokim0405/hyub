using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("설정")]
    public float speed = 10f;       // 날아가는 속도
    public float lifeTime = 3f;     // 3초 뒤 자동 삭제
    public int damage = 1;          // 데미지

    [Header("충돌 설정")]
    // ★ 핵심: 인스펙터에서 충돌할 레이어를 다중 선택할 수 있음
    public LayerMask collisionLayer; 

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime); // 수명 지나면 파괴
    }

    public void Launch(Vector2 direction)
    {
        // Unity 6: linearVelocity
        rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 플레이어 충돌 처리 (태그 사용)
        if (collision.CompareTag("Player"))
        {
            Debug.Log("💥 플레이어 피격!");
            // TODO: 데미지 처리
            // collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        // 2. 장애물/땅 충돌 처리 (LayerMask 사용)
        // 설명: 부딪힌 물체의 레이어가 collisionLayer에 포함되어 있는지 비트 연산으로 확인
        else if (((1 << collision.gameObject.layer) & collisionLayer) != 0)
        {
            // 여기에 걸린 레이어는 "막히는" 물체이므로 수리검 삭제
            Destroy(gameObject);
        }
        
        // * 참고: 여기에 포함되지 않은 레이어(예: 배경 벽)는 
        // 그냥 통과(무시)하고 지나갑니다.
    }
}