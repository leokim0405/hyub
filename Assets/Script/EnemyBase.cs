using UnityEngine;
using System.Collections;

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
    public GameObject curiousMark;

    [Header("사운드 설정")]
    public AudioSource AudioSource;
    public AudioClip attackSound;
    public AudioClip boxColidekSound;
    public float volume = 0.4f;


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
                // 현재 상태가 Chase일 때만 true, 나머지는 false
                detectionMark.SetActive(_currentState == EnemyState.Chase);
            }

            if (curiousMark != null)
            {
                // 현재 상태가 Alert일 때만 true, 나머지는 false
                curiousMark.SetActive(_currentState == EnemyState.Alert);
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
        // Destroy(gameObject);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        StartCoroutine(FadeOutAndDestroy(1.0f));    //1초동안 사라짐 숫자 바꿔주면 시간 변경가능함
    }

    private IEnumerator FadeOutAndDestroy(float duration)
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        float elapsed = 0f;

        // 1. 시작 위치와 목표 위치 설정
        Vector3 startPosition = transform.position;
        float floatHeight = 0.5f; // 위로 떠오를 높이 (원하는 만큼 조절)
        Vector3 targetPosition = startPosition + new Vector3(0, floatHeight, 0);

        if (sr != null)
        {
            Color startColor = sr.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration; // 0에서 1로 변하는 진행률

                // 2. 투명도 조절 (1 -> 0)
                float newAlpha = Mathf.Lerp(1f, 0f, t);
                sr.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

                // 3. 위치 조절 (시작점 -> 목표점)
                // 직선적으로 올라가게 하려면 Lerp를 사용합니다.
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);

                yield return null;
            }
        }

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
                AudioSource.PlayOneShot(boxColidekSound, volume);
                Destroy(collision.gameObject);
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

            if (attackSound != null)
            {
                AudioSource.PlayOneShot(attackSound, volume);
            }
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
