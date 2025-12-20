using System.Collections;
using UnityEditorInternal;
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

  [Header("추격 설정")]
  public float chaseSpeed = 4f;
  public float jumpForce = 6f;
  public float wallCheckDist = 0.7f;
  public float jumpThresholdY = 1.5f;
  public LayerMask groundLayer;

  private Transform _playerTransform;
  private bool _isGrounded;
  private Vector2 _lastHeardPos;

  [Header("청각 설정")]
  public float hearingDistance = 7f; // 소리가 들리는 최대 거리
  public float alertDuration = 3f;   // 경계 상태 유지 시간

  void Awake()
  {
    speed = 3f;
    rb = GetComponent<Rigidbody2D>();
    _vision = GetComponent<EnemyVision>();
    _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
  }

  // void Onable()
  // {
  //   _PatrolRoutine = StartCoroutine(PatrolRoutine()); 
  // }
  void ODisable()
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
    // _isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);
    Debug.DrawRay(transform.position, Vector2.down * 1.1f, Color.red);

    if (currentState != EnemyState.Chase)
    {
      if (_vision != null && _vision.IsPlayerVisible())
      {
        Debug.Log("set to chase");
        TransitionToState(EnemyState.Chase);
      }
    }

    if (currentState == EnemyState.Chase)
    {
      HandleChaseAction();
    }

  }

  private void CheckAndJump(float direction)
  {
    Debug.Log(_isGrounded);
    if (!_isGrounded) return;

    // 시작 지점을 위로 올림 (반드시 rayOrigin을 사용해야 함!)
    Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.5f;

    // 레이캐스트 (rayOrigin 사용)
    RaycastHit2D wallHit = Physics2D.Raycast(rayOrigin, new Vector2(direction, 0), wallCheckDist, groundLayer);

    // 시각적 확인 (파란 선)
    Debug.DrawRay(rayOrigin, new Vector2(direction, 0) * wallCheckDist, Color.blue);

    if (wallHit.collider != null)
    {
      rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
    if(collision.collider.CompareTag("floor"))
    {
      _isGrounded = true;
    }

    if (_isGrounded && collision.collider.CompareTag("Obstacle"))
    {
      rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
  }

}
