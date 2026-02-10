using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, ITeleportable
{
    protected Rigidbody2D rb;
    protected bool _isGrounded;
    protected Animator anim;
    
    public float jumpForce = 10f;


    public enum EnemyState { Idle, Patrol, Alert, Chase }
    public EnemyState _currentState = EnemyState.Patrol;

    [Header("능력치 설정")]
    [SerializeField] protected float maxHealth = 1f; // 기본 체력 1
    protected float currentHealth;

    [Header("낙하 데미지 설정")]
    [SerializeField] protected float minFallSpeed = 3f; // 데미지를 입는 최소 속도
    [SerializeField] protected float fallDamage = 1f;  // 낙하물 충돌 시 입는 데미지

    [Header("UI 설정")]
    public GameObject detectionMark;

    public EnemyState currentState
    {
        get => _currentState;
        set
        {
            // 값이 같으면 실행하지 않음 (최적화)
            if (_currentState == value) return;

            _currentState = value;

            // 상태가 바뀔 때 자동으로 '!' 활성화 여부 결정
            if (detectionMark != null)
            {
                detectionMark.SetActive(_currentState == EnemyState.Chase);
            }
        }
    }

    protected virtual void Awake()
    {
        // 시작 시 현재 체력을 최대 체력으로 초기화
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        Debug.Log($"{gameObject.name}현재 체력: {currentHealth}");
        currentHealth -= damage;
        Debug.Log($"{gameObject.name}에 {damage}의 피해를 입힘. 남은 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void OnAssassinated()
    {
        if (currentState != EnemyState.Chase)
        {
            Debug.Log($"{gameObject.name} 암살됨");
            TakeDamage(1f);
        }
        else
        {
            // 추격 중일 때는 암살이 되지 않음을 알림 (필요 시 작성)
            Debug.Log($"{gameObject.name}이 플레이어를 발견한 상태라 암살에 실패했습니다.");
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} 사망");
        // 사망 애니메이션이나 이펙트 처리를 위해 가상 메서드로 분리
        Destroy(gameObject);
    }

    public abstract Transform GetTransform();
    public abstract void OnTeleport();

    protected virtual void OnValidate()
    {
        if (detectionMark != null)
        {
            detectionMark.SetActive(_currentState == EnemyState.Chase);
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 떨어지는 물체인지 태그로 확인
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log($"obstacle collision speed : {collision.relativeVelocity.y}");
            // 2. 충돌 시점의 속도가 충분히 빠른지 확인
            if (collision.relativeVelocity.y < -minFallSpeed) // 위에서 아래로 떨어지는 경우
            {
                TakeDamage(fallDamage);
            }
        }

        if (collision.collider.CompareTag("floor"))
        {
            _isGrounded = true;
        }

        if (_isGrounded && collision.collider.CompareTag("Obstacle"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (collision.collider.CompareTag("Player"))
        {
            anim.SetTrigger("IsAttack");
        }
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("floor") || collision.gameObject.CompareTag("Obstacle"))
        {
            _isGrounded = false;
        }
    }
}
