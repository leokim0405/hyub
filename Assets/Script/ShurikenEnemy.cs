using System.Collections;
using UnityEngine;

public class ShurikenEnemy : MonoBehaviour, ITeleportable
{
    public enum EnemyState { Patrol, Alert, Attack }
    public EnemyState currentState = EnemyState.Patrol;

    [Header("이동 설정")]
    public float moveSpeed = 2f;
    public float patrolWaitTime = 2.0f;
    private int _facingDir = 1;

    [Header("지형 감지")]
    public LayerMask groundLayer;
    public float wallCheckDist = 0.5f;
    public float cliffCheckDist = 0.5f;
    private bool _isGrounded;

    [Header("공격/감지")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackCooldown = 2.0f;
    private float _lastAttackTime;
    private ShurikenEnemyVision _vision;
    private Transform _playerTransform;
    private float _chaseLossTimer = 0f;
    public float chaseLossThreshold = 2.0f;
    private Vector2 _lastHeardPos;

    private Rigidbody2D rb;
    private Animator anim;
    private Coroutine _currentBehaviorRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        _vision = GetComponent<ShurikenEnemyVision>();

        if (_playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _playerTransform = playerObj.transform;
        }
    }

    void Start()
    {
        Debug.Log("🚀 게임 시작! Patrol 상태로 전환 시도...");
        TransitionToState(EnemyState.Patrol);
    }

    void Update()
    {
        // 바닥 체크 (녹색 선)
        float legLength = GetComponent<Collider2D>().bounds.extents.y + 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, legLength, groundLayer);
        _isGrounded = hit.collider != null;
        Debug.DrawRay(transform.position, Vector2.down * legLength, _isGrounded ? Color.green : Color.red);

        UpdateAnimation();

        // 상태 감지 로직
        if (currentState == EnemyState.Attack)
        {
            HandleAttackState();
        }
        else
        {
            if (_vision != null && _vision.IsPlayerVisible())
            {
                // [디버그] 시야 감지 로그
                Debug.Log("👁️ 플레이어 발견! Attack 모드로 전환!");
                TransitionToState(EnemyState.Attack);
            }
        }
    }

    public void TransitionToState(EnemyState newState)
    {
        if (currentState == newState && _currentBehaviorRoutine != null) return;

        // [디버그] 상태 변경 로그
        Debug.Log($"🔄 상태 변경: {currentState} -> {newState}");

        currentState = newState;
        _chaseLossTimer = 0f;

        if (_currentBehaviorRoutine != null) StopCoroutine(_currentBehaviorRoutine);

        rb.linearVelocity = Vector2.zero;

        switch (newState)
        {
            case EnemyState.Patrol:
                _currentBehaviorRoutine = StartCoroutine(PatrolRoutine());
                break;
            case EnemyState.Alert:
                _currentBehaviorRoutine = StartCoroutine(AlertRoutine());
                break;
            case EnemyState.Attack:
                break;
        }
    }

    IEnumerator PatrolRoutine()
    {
        Debug.Log("🚶 순찰(Patrol) 루틴 시작됨"); // 루틴 진입 확인

        while (currentState == EnemyState.Patrol)
        {
            // 지형 체크
            if (CheckWall() || CheckCliff())
            {
                rb.linearVelocity = Vector2.zero;
                yield return new WaitForSeconds(patrolWaitTime);
                Flip();
            }
            else
            {
                // 이동
                rb.linearVelocity = new Vector2(_facingDir * moveSpeed, rb.linearVelocity.y);
            }
            yield return null;
        }
    }

    private bool CheckCliff()
    {
        Collider2D col = GetComponent<Collider2D>();
        Vector2 origin = new Vector2(transform.position.x + (_facingDir * cliffCheckDist), col.bounds.center.y);
        float rayLength = col.bounds.extents.y + 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, groundLayer);
        
        // [중요] 빨간 선이 그려져야 이 함수가 실행되고 있는 것임
        Debug.DrawRay(origin, Vector2.down * rayLength, hit.collider == null ? Color.red : Color.green);

        return hit.collider == null;
    }

    private bool CheckWall()
    {
        Collider2D col = GetComponent<Collider2D>();
        Vector2 origin = col.bounds.center;
        float rayLength = col.bounds.extents.x + wallCheckDist;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * _facingDir, rayLength, groundLayer);
        Debug.DrawRay(origin, Vector2.right * _facingDir * rayLength, hit.collider != null ? Color.red : Color.blue);
        return hit.collider != null;
    }

    private void Flip()
    {
        _facingDir *= -1;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * _facingDir; // 절대값 * 방향
        transform.localScale = scale;
    }

    // ... (나머지 Alert, Attack, UpdateAnimation 등은 기존과 동일) ...
    // 코드가 너무 길어지니 생략된 부분은 기존 코드를 유지해주세요.
    
    // [Attack, Alert, Animation 코드 유지 필요]
    private void UpdateAnimation()
    {
        if (anim == null) return;
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f && currentState == EnemyState.Patrol;
        anim.SetBool("Walking", isMoving);
        anim.SetBool("IsGround", _isGrounded);
    }

    private void HandleAttackState()
    {
        rb.linearVelocity = Vector2.zero;
        if (_playerTransform == null) return;

        float dir = _playerTransform.position.x - transform.position.x;
        if (dir != 0)
        {
            _facingDir = dir > 0 ? 1 : -1;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * _facingDir;
            transform.localScale = scale;
        }

        if (!_vision.IsPlayerVisible())
        {
            _chaseLossTimer += Time.deltaTime;
            if (_chaseLossTimer >= chaseLossThreshold) TransitionToState(EnemyState.Alert);
        }
        else
        {
            _chaseLossTimer = 0f;
            if (Time.time >= _lastAttackTime + attackCooldown) Attack();
        }
    }

    private void Attack()
    {
        _lastAttackTime = Time.time;
        anim.SetTrigger("IsAttack");
        StartCoroutine(ShootDelay(0.2f));
    }

    IEnumerator ShootDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Vector2 dir = (_playerTransform.position - firePoint.position).normalized;
            projectile.GetComponent<Projectile>().Launch(dir);
        }
    }
    
    IEnumerator AlertRoutine()
    {
        float dir = _lastHeardPos.x - transform.position.x;
        if (dir != 0) 
        { 
            _facingDir = dir > 0 ? 1 : -1; 
            
            // [수정] 기존 스케일 유지
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * _facingDir;
            transform.localScale = scale;
        }
        rb.linearVelocity = Vector2.zero; 
        yield return new WaitForSeconds(3f);
        if (currentState == EnemyState.Alert) TransitionToState(EnemyState.Patrol);
    }

    public void OnHeardSound(Vector2 soundPosition)
    {
        if (currentState == EnemyState.Attack) return;
        _lastHeardPos = soundPosition;
        TransitionToState(EnemyState.Alert);
    }
    public Transform GetTransform() => transform;
    public void OnTeleport() => rb.linearVelocity = Vector2.zero;
}