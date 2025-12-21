using System.Collections;
// using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, ITeleportable
{
  public enum EnemyState { Idel, Patrol, Alert, Chase }
  public EnemyState currentState = EnemyState.Patrol;
  public float speed;
  public float patrolTime = 2f;
  public float waitTime = 0.2f;

  private Rigidbody2D rb;
  private Coroutine _PatrolRoutine;
  private Coroutine _currentBehaviorRoutine;
  private EnemyVision _vision;
  private float _chaseLossTimer = 0f; // 플레이어를 놓친 시간을 기록
  public float chaseLossThreshold = 1.0f; // 놓친 후 대기할 시간 (1초)

  Animator anim;

  [Header("추격 설정")]
  public float chaseSpeed = 4f;
  public float jumpForce = 20f;
  public float wallCheckDist = 1.2f;
  public float jumpThresholdY = 1.5f;
  public LayerMask groundLayer;

  private Transform _playerTransform;
  private bool _isGrounded;
  private Vector2 _lastHeardPos;

  [Header("청각 설정")]
  public float hearingDistance = 7f; // 소리가 들리는 최대 거리
  public float alertDuration = 3f;   // 경계 상태 유지 

  [Header("바닥 체크 설정")]
  public Transform groundCheck;
  public float groundCheckRadius = 0.2f;

  void Awake()
  {
    speed = 3f;
    rb = GetComponent<Rigidbody2D>();
    _vision = GetComponent<EnemyVision>();
    _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    anim = GetComponent<Animator>();
    // groundCheck = GetComponent<Transform>();
  }

  // void Onable()
  // {
  //   _PatrolRoutine = StartCoroutine(PatrolRoutine()); 
  // }
  void OnDisable()
  {
    if (_PatrolRoutine != null) StopCoroutine(PatrolRoutine());
  }

  void Start()
  {
    TransitionToState(EnemyState.Patrol);

    // _PatrolRoutine = StartCoroutine(PatrolRoutine());
  }

  void Update()
  {
    _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    Debug.DrawRay(transform.position, Vector2.down * 1.1f, Color.red);

    if (currentState != EnemyState.Chase)
    {
      if (_vision != null && _vision.IsPlayerVisible())
      {
        Debug.Log("set to chase");
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

  private void UpdateAnimation()
  {
    if (anim == null) return;

    // 2. 좌우 이동(X축) 속도가 거의 0보다 큰지 확인
    // Mathf.Abs를 사용하는 이유는 왼쪽(-속도)으로 갈 때도 true가 되게 하기 위함입니다.
    // 점프 중일 때 걷는 애니메이션이 나오는 걸 방지하려면 _isGrounded 체크도 추가하면 좋습니다.
    bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f && _isGrounded;

    // // 1. 추격 상태(Chase)이면서 움직이고 있다면 Running = true
    bool isRunning = (currentState == EnemyState.Chase) && isMoving;

    // // 2. 추격 상태가 아니면서(Patrol 등) 움직이고 있다면 Walking = true
    bool isWalking = (currentState != EnemyState.Chase) && isMoving;

    // 애니메이터 파라미터 업데이트
    anim.SetBool("Running", isRunning);
    anim.SetBool("Walking", isWalking);
    anim.SetBool("IsGround", _isGrounded);

    // if (anim == null) return;

    // float moveX = Mathf.Abs(rb.linearVelocity.x);

    // // 바닥에 있고, 속도가 0.01보다 클 때만 걷는 것으로 판정
    // bool walking = moveX > 0.01f && _isGrounded; 

    anim.SetBool("Walking", isWalking);
    anim.SetBool("IsGround", _isGrounded);
  }

  private void CheckAndJump(float direction)
  {
    if (!_isGrounded) return;

    // 2. 레이캐스트 시작 지점 다양화 (발밑과 허리 높이 두 곳 체크)
    Vector2 rayOriginLow = (Vector2)transform.position + Vector2.up * 0.2f;
    Vector2 rayOriginMid = (Vector2)transform.position + Vector2.up * 0.8f;
    Vector2 moveDir = new Vector2(direction, 0);

    // 하단 레이캐스트로 장애물 감지
    RaycastHit2D wallHit = Physics2D.Raycast(rayOriginLow, moveDir, wallCheckDist, groundLayer);

    // 디버그용 레이 (에디터 뷰에서 파란색으로 보임)
    Debug.DrawRay(rayOriginLow, moveDir * wallCheckDist, Color.blue);

    if (wallHit.collider != null)
    {
      // 장애물을 발견하면 점프! 
      // 기존 속도를 유지하면서 Y축만 변경 (Force 방식 추천)
      rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
      Debug.Log("Jump Triggered!");
    }
  }

  IEnumerator PatrolRoutine()
  {
    while (true) // 무한 루프 (순찰 계속)
    {
      // 1. 오른쪽으로 이동
      UnityEngine.Debug.Log("move start");
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

    while (timer < duration)
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
  public Transform GetTransform() => transform;
  public void OnTeleport()
  {
    // 텔레포트 시 물리적 관성 제거
    rb.linearVelocity = Vector2.zero;
  }

  public void TransitionToState(EnemyState newState)
  {
    if (currentState == newState && newState != EnemyState.Patrol) return;

    Debug.Log($"{gameObject.name}: state change {currentState} -> {newState}");
    currentState = newState;
    _chaseLossTimer = 0f;

    if (_currentBehaviorRoutine != null)
    {
      StopCoroutine(_currentBehaviorRoutine);
      _currentBehaviorRoutine = null;
    }

    switch (newState)
    {
      case EnemyState.Patrol:
        _currentBehaviorRoutine = StartCoroutine(PatrolRoutine());
        break;
      case EnemyState.Alert:
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        break;
      case EnemyState.Chase:
        Debug.Log("chasing ... ");
        // HandleChaseAction();
        // 추격 시작 시 별도의 코루틴 없이 Update의 HandleChaseAction이 주도함
        break;
    }
  }

  private void HandleChaseAction()
  {
    if (_playerTransform == null) return;

    chaseSpeed = speed;

    // 플레이어 방향으로 속도 설정
    float direction = _playerTransform.position.x > transform.position.x ? 1f : -1f;
    transform.localScale = new Vector3(direction, 1, 1);
    rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);


    CheckAndJump(direction);
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
        Debug.Log("플레이어를 놓쳤다. 주변을 수색하자.");
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
    // 이미 추격 중이라면 소리에 반응하지 않음 (선택 사항)
    if (currentState == EnemyState.Chase) return;

    // 소리와의 거리 계산
    float distance = Vector2.Distance(transform.position, soundPosition);

    if (distance <= hearingDistance)
    {
      Debug.Log($"{gameObject.name}: 무슨 소리지?");

      // 소리 난 방향으로 고개 돌리기
      float dir = soundPosition.x > transform.position.x ? 1f : -1f;
      transform.localScale = new Vector3(dir, 1, 1);

      _lastHeardPos = soundPosition;


      // 일정 시간 후 다시 순찰로 돌아가게 설정
      StopAllCoroutines(); // 기존 순찰/대기 코루틴 정지
      StartCoroutine(AlertRoutine());
    }

  }
  IEnumerator AlertRoutine()
  {
    float stopDistance = 0.5f; // 목표 지점 도달 판정 거리 (0.5m 이내면 도착)

    // 목표 지점에 도착할 때까지 반복
    while (Mathf.Abs(transform.position.x - _lastHeardPos.x) > stopDistance)
    {
      // 방향 계산 및 고개 돌리기
      float dir = _lastHeardPos.x > transform.position.x ? 1f : -1f;
      transform.localScale = new Vector3(dir, 1, 1);

      // 이동 속도 적용
      rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

      CheckAndJump(dir);

      yield return null; // 다음 프레임까지 대기
    }

    // 2. 도착 후 정지
    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    Debug.Log($"{gameObject.name}: 도착했다. 주변을 살펴보자.");

    // 3. 제자리에서 잠시 두리번거리거나 대기 (경계 시간)
    yield return new WaitForSeconds(alertDuration);

    // 4. 다시 순찰 상태로 복귀 (그 사이 추격 상태가 되지 않았다면)
    if (currentState != EnemyState.Chase)
    {
      TransitionToState(EnemyState.Patrol);
    }
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.collider.CompareTag("floor"))
    {
      _isGrounded = true;
      anim.SetBool("IsGround", true);
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

}
