using System.Collections;
// using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : EnemyBase //MonoBehaviour, ITeleportable
{
  // public enum EnemyState { Idle, Patrol, Alert, Chase }
  // public EnemyState currentState = EnemyState.Patrol;

  [Header("기본 설정")]
  public float speed = 3f;
  public float patrolTime = 2f;
  public float waitTime = 0.2f;

  // private Rigidbody2D rb;
  // private Coroutine _PatrolRoutine;  // edit
  private Coroutine _currentBehaviorRoutine;
  private EnemyVision _vision;
  private float _chaseLossTimer = 0f; // 플레이어를 놓친 시간을 기록
  public float chaseLossThreshold = 1.0f; // 놓친 후 대기할 시간 (1초)

  // Animator anim;

  [Header("추격 설정")]
  public float chaseSpeed = 4f;
  // public float jumpForce = 10f;
  public float wallCheckDist = 1.0f;
  // public float jumpThresholdY = 1.5f;  // edit
  public LayerMask groundLayer;

  private Transform _playerTransform;
  // private bool _isGrounded;
  private Vector2 _lastHeardPos;

  [Header("점프 설정")]
  private float _jumpCooldown = 0.1f; // 점프 쿨타임 타이머
  private float _jumpCooldownTime = 0.5f; // 점프 후 0.5초간 재점프 금지

  [Header("청각 설정")]
  public float hearingDistance = 7f; // 소리가 들리는 최대 거리
  public float alertDuration = 3f;   // 경계 상태 유지 

  [Header("바닥 체크 설정")]
  public Transform groundCheck;
  public float groundCheckRadius = 0.2f;

  void Awake()
  {
    base.Awake();
    // speed = 3f;
    rb = GetComponent<Rigidbody2D>();
    _vision = GetComponent<EnemyVision>();

    if (_playerTransform == null)
    {
      GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
      if (playerObj != null) _playerTransform = playerObj.transform;
    }

    anim = GetComponent<Animator>();
    // groundCheck = GetComponent<Transform>();
  }

  void OnDisable()
  {
    StopBehaviorCoroutine();
  }

  void Start()
  {
    TransitionToState(EnemyState.Patrol);

    // _PatrolRoutine = StartCoroutine(PatrolRoutine());
  }

  void Update()
  {
    // 바닥 체크 (이것만 사용)
    if (groundCheck != null)
    {
      _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    Debug.DrawRay(transform.position, Vector2.down * 1.1f, Color.red);

    // 상태별 로직 수행
    if (currentState != EnemyState.Chase)
    {
      if (_vision != null && _vision.IsPlayerVisible())
      {
        // Debug.Log("Player Detected -> Chase Start");
        TransitionToState(EnemyState.Chase);
      }
    }
    else
    {
      HandleChaseAction();
      CheckChaseTimeout();
    }

    UpdateAnimation();

  }
  private void StopBehaviorCoroutine()
  {
    if (_currentBehaviorRoutine != null)
    {
      StopCoroutine(_currentBehaviorRoutine);
      _currentBehaviorRoutine = null;
    }
  }

  private void UpdateAnimation()
  {
    if (anim == null) return;

    bool isWalking = Mathf.Abs(rb.linearVelocity.x) > 0.1;

    anim.SetBool("Walking", isWalking);

    if (_isGrounded)
    {
      anim.SetBool("IsGround", true);
    }
    else
    {
      anim.SetBool("IsGround", false);
    }
  }

  private void CheckAndJump(float direction)
  {
    if (!_isGrounded || Time.time < _jumpCooldown) return;

    // 1. 내 콜라이더 정보 가져오기
    Collider2D col = GetComponent<Collider2D>();
    if (col == null) return;

    // 2. 높이 자동 계산 (가장 중요한 부분)
    // - Low: 발바닥(bounds.min.y)에서 아주 살짝(0.1f) 위
    // - High: 몸통 중심(bounds.center.y)
    float yLow = col.bounds.min.y + 0.1f;
    float yHigh = col.bounds.center.y;

    // 3. 가로 시작 위치 계산 (몸통 폭 + 0.1f 앞)
    // - 이렇게 하면 내 몸을 찌르지 않고 바로 앞에서 시작함
    float xOffset = (col.bounds.extents.x + 0.1f) * direction;
    float xOrigin = col.bounds.center.x + xOffset;

    // 최종 시작점 벡터
    Vector2 rayOriginLow = new Vector2(xOrigin, yLow);
    Vector2 rayOriginHigh = new Vector2(xOrigin, yHigh);
    Vector2 moveDir = new Vector2(direction, 0);

    // 4. 레이 발사
    RaycastHit2D hitLow = Physics2D.Raycast(rayOriginLow, moveDir, wallCheckDist, groundLayer);
    RaycastHit2D hitHigh = Physics2D.Raycast(rayOriginHigh, moveDir, wallCheckDist, groundLayer);

    // [디버그] 레이 확인 (파란색: 허공 / 초록색: 벽 감지)
    bool isHit = (hitLow.collider != null) || (hitHigh.collider != null);
    Debug.DrawRay(rayOriginLow, moveDir * wallCheckDist, isHit ? Color.green : Color.blue);
    Debug.DrawRay(rayOriginHigh, moveDir * wallCheckDist, isHit ? Color.green : Color.blue);

    // 5. 점프 실행
    if (isHit)
    {
      //  Debug.Log("🚧 낮은 장애물 감지! 점프!");
      rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
      _jumpCooldown = Time.time + _jumpCooldownTime;
    }
  }

  IEnumerator PatrolRoutine()
  {
    while (currentState == EnemyState.Patrol) // 무한 루프 (순찰 계속)
    {
      // 1. 오른쪽으로 이동
      // UnityEngine.Debug.Log("move start");
      yield return StartCoroutine(MoveInDirection(Vector2.right, patrolTime));

      // 2. 잠시 대기
      yield return StartCoroutine(WaitAtEdge());

      // 3. 왼쪽으로 이동
      yield return StartCoroutine(MoveInDirection(Vector2.left, patrolTime));

      // 4. 잠시 대기
      yield return StartCoroutine(WaitAtEdge());
    }
  }

  IEnumerator MoveInDirection(Vector2 direction, float duration)
  {
    float timer = 0f;

    // 방향에 맞춰 캐릭터 고개 돌리기
    transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);

    while (timer < duration && currentState == EnemyState.Patrol)
    {
      // Rigidbody를 사용해 이동 적용
      rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
      CheckAndJump(direction.x);
      timer += Time.deltaTime;
      yield return null; // 다음 프레임까지 대기
    }
  }

  IEnumerator WaitAtEdge()
  {
    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // 정지
    yield return new WaitForSeconds(waitTime); // 지정된 시간만큼 대기
  }


  // ITeleportable 구현 (마커 시스템 연동)
  public override Transform GetTransform() => transform;
  public override void OnTeleport()
  {
    // 텔레포트 시 물리적 관성 제거
    rb.linearVelocity = Vector2.zero;
  }

  public void TransitionToState(EnemyState newState)
  {
    if (currentState == newState && newState != EnemyState.Patrol) return;

    // Debug.Log($"{gameObject.name}: State Change {currentState} -> {newState}");
    currentState = newState;
    _chaseLossTimer = 0f;

    // 이전 행동 코루틴 종료
    StopBehaviorCoroutine();

    switch (newState)
    {
      case EnemyState.Patrol:
        _currentBehaviorRoutine = StartCoroutine(PatrolRoutine());
        break;

      case EnemyState.Alert:
        rb.linearVelocity = Vector2.zero; // 멈춤
                                          // [수정된 부분] 추격하다 놓쳤을 때: 제자리에서 두리번거리다 순찰로 복귀하는 코루틴 시작
        _currentBehaviorRoutine = StartCoroutine(AlertWaitRoutine());
        break;

      case EnemyState.Chase:
        // Chase는 Update에서 처리됨
        break;
    }
  }

  private void HandleChaseAction()
  {
    if (_playerTransform == null) return;

    // 1. 거리 계산 (이 부분이 있어야 높이 차이를 알 수 있습니다)
    float xDistance = _playerTransform.position.x - transform.position.x;
    float yDistance = _playerTransform.position.y - transform.position.y;

    // 2. 방향 설정 (플레이어 쪽 바라보기)
    float direction = xDistance > 0 ? 1f : -1f;
    transform.localScale = new Vector3(direction, 1, 1);

    // 3. 이동 (좌우 이동)
    rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

    // 4. 점프 로직 (두 가지 상황 체크)

    // A. 앞에 벽/상자가 막고 있을 때 (레이캐스트)
    CheckAndJump(direction);

    // B. [수직 점프] 플레이어가 머리 위에 있을 때 (높이 차이 계산)
    // 조건: 높이 차이가 1.5m 이상이고, 수평 거리가 2m 이내일 때
    if (yDistance > 1.5f && Mathf.Abs(xDistance) < 2.0f && _isGrounded)
    {
      // 쿨타임 체크 (연속 점프 방지)
      if (Time.time > _jumpCooldown)
      {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        _jumpCooldown = Time.time + 0.5f; // 점프 쿨타임 갱신
        // Debug.Log("플레이어가 위에 있어 점프!");
      }
    }
  }

  private void CheckChaseTimeout()
  {
    if (_vision == null) return;

    if (!_vision.IsPlayerVisible())
    {
      // 플레이어가 안 보이면 타이머 증가
      _chaseLossTimer += Time.deltaTime;

      if (_chaseLossTimer >= chaseLossThreshold)
      {
        Debug.Log("Lost Player -> Alert Mode");
        TransitionToState(EnemyState.Alert); // 1초가 지나면 Alert 상태로 전환
      }
    }
    else
    {
      // 플레이어가 다시 보이면 타이머 초기화
      _chaseLossTimer = 0f;
    }
  }

  public void OnHeardSound(Vector2 soundPosition)
  {
    if (currentState == EnemyState.Chase) return;

    float distance = Vector2.Distance(transform.position, soundPosition);
    if (distance <= hearingDistance)
    {
      Debug.Log($"{gameObject.name}: Sound heard at {soundPosition}");
      _lastHeardPos = soundPosition;

      // 기존 행동 멈추고 Alert 상태로 전환
      StopBehaviorCoroutine();

      // 상태 값 변경 (Alert 상태로 표시)
      currentState = EnemyState.Alert;

      // 소리난 곳으로 가는 코루틴 시작
      _currentBehaviorRoutine = StartCoroutine(AlertRoutine());
    }
  }

  IEnumerator AlertWaitRoutine()
  {
    Debug.Log("Target Lost. Searching area...");

    // 설정된 경계 시간(alertDuration) 만큼 대기
    yield return new WaitForSeconds(alertDuration);

    // 여전히 Alert 상태라면(그 사이 다시 플레이어를 발견하지 않았다면) 순찰로 복귀
    if (currentState == EnemyState.Alert)
    {
      Debug.Log("Nothing found. Return to Patrol.");
      TransitionToState(EnemyState.Patrol);
    }
  }

  IEnumerator AlertRoutine()
  {
    float stopDistance = 0.5f;

    // 1. 소리난 곳으로 이동
    while (Mathf.Abs(transform.position.x - _lastHeardPos.x) > stopDistance)
    {
      // 추격 등으로 상태가 바뀌면 즉시 종료
      if (currentState != EnemyState.Alert) yield break;

      float dir = _lastHeardPos.x > transform.position.x ? 1f : -1f;
      transform.localScale = new Vector3(dir, 1, 1);
      rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
      CheckAndJump(dir);

      yield return null;
    }

    // 2. 도착 후 정지 및 대기
    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    Debug.Log($"{gameObject.name}: Arrived at sound location. Searching...");

    yield return new WaitForSeconds(alertDuration);

    // 3. 다시 순찰로 복귀 (여전히 Alert 상태라면)
    if (currentState == EnemyState.Alert)
    {
      TransitionToState(EnemyState.Patrol);
    }
  }

  // void OnCollisionEnter2D(Collision2D collision)
  // {
  //   if (collision.collider.CompareTag("floor"))
  //   {
  //     _isGrounded = true;
  //   }

  //   if (_isGrounded && collision.collider.CompareTag("Obstacle"))
  //   {
  //     rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
  //   }

  //   if (collision.collider.CompareTag("Player"))
  //   {
  //     anim.SetTrigger("IsAttack");
  //   }
  // }

  // private void OnCollisionExit2D(Collision2D collision)
  // {
  //   if (collision.gameObject.CompareTag("floor") || collision.gameObject.CompareTag("Obstacle"))
  //   {
  //     _isGrounded = false;
  //   }
  // }

}
