// using System.Collections;
// using UnityEngine;

// public class Enemy : MonoBehaviour, ITeleportable
// {
//     public enum EnemyState { Idel, Patrol, Alert, Chase }
//     public EnemyState currentState = EnemyState.Patrol;

//     [Header("기본 설정")]
//     public float speed = 3f; // 이제 인스펙터에서 수정 가능
//     public float patrolTime = 2f;
//     public float waitTime = 1f;

//     private Rigidbody2D rb;
//     private Coroutine _currentBehaviorRoutine; // 현재 실행 중인 행동(순찰/경계) 코루틴 저장
//     private EnemyVision _vision;
//     private float _chaseLossTimer = 0f;
//     public float chaseLossThreshold = 1.0f;

//     Animator anim;

//     [Header("추격 설정")]
//     public float chaseSpeed = 4f;
//     public float jumpForce = 10f; // 값 조정 (20은 너무 클 수 있음)
//     public float wallCheckDist = 1.0f;
//     public LayerMask groundLayer;

//     private Transform _playerTransform;
//     private bool _isGrounded;
//     private Vector2 _lastHeardPos;

//     [Header("점프 설정")]
//     private float _jumpCooldown = 0.1f; // 점프 쿨타임 타이머
//     private float _jumpCooldownTime = 0.5f; // 점프 후 0.5초간 재점프 금지

//     [Header("청각 설정")]
//     public float hearingDistance = 7f;
//     public float alertDuration = 3f;

//     [Header("바닥 체크 설정")]
//     public Transform groundCheck;
//     public float groundCheckRadius = 0.2f;

//     void Awake()
//     {
//         // speed = 3f; // [삭제됨] 인스펙터 값을 덮어쓰지 않음
//         rb = GetComponent<Rigidbody2D>();
//         _vision = GetComponent<EnemyVision>();
        
//         // Unity 6 권장: FindAnyObjectByType (물론 FindGameObjectWithTag도 사용 가능)
//         if (_playerTransform == null)
//         {
//             GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
//             if (playerObj != null) _playerTransform = playerObj.transform;
//         }

//         anim = GetComponent<Animator>();
//     }

//     void OnDisable()
//     {
//         StopBehaviorCoroutine();
//     }

//     void Start()
//     {
//         TransitionToState(EnemyState.Patrol);
//     }

//     void Update()
//     {
//         // 바닥 체크 (이것만 사용)
//         if (groundCheck != null)
//         {
//             _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
//         }
        
//         Debug.DrawRay(transform.position, Vector2.down * 1.1f, Color.red);

//         // 상태별 로직 수행
//         if (currentState != EnemyState.Chase)
//         {
//             if (_vision != null && _vision.IsPlayerVisible())
//             {
//                 Debug.Log("Player Detected -> Chase Start");
//                 TransitionToState(EnemyState.Chase);
//             }
//         }
//         else
//         {
//             HandleChaseAction();
//             CheckChaseTimeout();
//         }

//         UpdateAnimation();
//     }

//     // 행동 관련 코루틴만 안전하게 종료하는 헬퍼 함수
//     private void StopBehaviorCoroutine()
//     {
//         if (_currentBehaviorRoutine != null)
//         {
//             StopCoroutine(_currentBehaviorRoutine);
//             _currentBehaviorRoutine = null;
//         }
//     }

//     private void UpdateAnimation()
//     {
//         if (anim == null) return;

//         bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
//         bool isRunning = (currentState == EnemyState.Chase) && isMoving;
//         bool isWalking = (currentState != EnemyState.Chase) && isMoving;

//         anim.SetBool("Running", isRunning);
//         anim.SetBool("Walking", isWalking);
//         anim.SetBool("IsGround", _isGrounded);
//     }
//     private void CheckAndJump(float direction)
//     {
//         if (!_isGrounded || Time.time < _jumpCooldown) return;

//         // 1. 내 콜라이더 정보 가져오기
//         Collider2D col = GetComponent<Collider2D>();
//         if (col == null) return;

//         // 2. 높이 자동 계산 (가장 중요한 부분)
//         // - Low: 발바닥(bounds.min.y)에서 아주 살짝(0.1f) 위
//         // - High: 몸통 중심(bounds.center.y)
//         float yLow = col.bounds.min.y + 0.1f;
//         float yHigh = col.bounds.center.y;

//         // 3. 가로 시작 위치 계산 (몸통 폭 + 0.1f 앞)
//         // - 이렇게 하면 내 몸을 찌르지 않고 바로 앞에서 시작함
//         float xOffset = (col.bounds.extents.x + 0.1f) * direction;
//         float xOrigin = col.bounds.center.x + xOffset;

//         // 최종 시작점 벡터
//         Vector2 rayOriginLow = new Vector2(xOrigin, yLow);
//         Vector2 rayOriginHigh = new Vector2(xOrigin, yHigh);
//         Vector2 moveDir = new Vector2(direction, 0);

//         // 4. 레이 발사
//         RaycastHit2D hitLow = Physics2D.Raycast(rayOriginLow, moveDir, wallCheckDist, groundLayer);
//         RaycastHit2D hitHigh = Physics2D.Raycast(rayOriginHigh, moveDir, wallCheckDist, groundLayer);

//         // [디버그] 레이 확인 (파란색: 허공 / 초록색: 벽 감지)
//         bool isHit = (hitLow.collider != null) || (hitHigh.collider != null);
//         Debug.DrawRay(rayOriginLow, moveDir * wallCheckDist, isHit ? Color.green : Color.blue);
//         Debug.DrawRay(rayOriginHigh, moveDir * wallCheckDist, isHit ? Color.green : Color.blue);

//         // 5. 점프 실행
//         if (isHit)
//         {
//             Debug.Log("🚧 낮은 장애물 감지! 점프!");
//             rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
//             _jumpCooldown = Time.time + _jumpCooldownTime;
//         }
//     }

//     public void TransitionToState(EnemyState newState)
//     {
//         if (currentState == newState && newState != EnemyState.Patrol) return;

//         Debug.Log($"{gameObject.name}: State Change {currentState} -> {newState}");
//         currentState = newState;
//         _chaseLossTimer = 0f;

//         // 이전 행동 코루틴 종료
//         StopBehaviorCoroutine();

//         switch (newState)
//         {
//             case EnemyState.Patrol:
//                 _currentBehaviorRoutine = StartCoroutine(PatrolRoutine());
//                 break;

//             case EnemyState.Alert:
//                 rb.linearVelocity = Vector2.zero; // 멈춤
//                 // [수정된 부분] 추격하다 놓쳤을 때: 제자리에서 두리번거리다 순찰로 복귀하는 코루틴 시작
//                 _currentBehaviorRoutine = StartCoroutine(AlertWaitRoutine());
//                 break;

//             case EnemyState.Chase:
//                 // Chase는 Update에서 처리됨
//                 break;
//         }
//     }

//     IEnumerator PatrolRoutine()
//     {
//         while (currentState == EnemyState.Patrol)
//         {
//             // 1. 오른쪽
//             yield return StartCoroutine(MoveInDirection(Vector2.right, patrolTime));
//             // 2. 대기
//             yield return StartCoroutine(WaitAtEdge());
//             // 3. 왼쪽
//             yield return StartCoroutine(MoveInDirection(Vector2.left, patrolTime));
//             // 4. 대기
//             yield return StartCoroutine(WaitAtEdge());
//         }
//     }
//     // 추격 실패 후 제자리에서 경계하다가 순찰로 복귀하는 코루틴
//     IEnumerator AlertWaitRoutine()
//     {
//         Debug.Log("Target Lost. Searching area...");
        
//         // 설정된 경계 시간(alertDuration) 만큼 대기
//         yield return new WaitForSeconds(alertDuration);

//         // 여전히 Alert 상태라면(그 사이 다시 플레이어를 발견하지 않았다면) 순찰로 복귀
//         if (currentState == EnemyState.Alert)
//         {
//             Debug.Log("Nothing found. Return to Patrol.");
//             TransitionToState(EnemyState.Patrol);
//         }
//     }

//     IEnumerator MoveInDirection(Vector2 direction, float duration)
//     {
//         float timer = 0f;
//         transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);

//         while (timer < duration && currentState == EnemyState.Patrol)
//         {
//             rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
//             CheckAndJump(direction.x);
//             timer += Time.deltaTime;
//             yield return null;
//         }
//     }

//     IEnumerator WaitAtEdge()
//     {
//         rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
//         yield return new WaitForSeconds(waitTime);
//     }
//     private void HandleChaseAction()
//     {
//         if (_playerTransform == null) return;

//         // 1. 거리 계산 (이 부분이 있어야 높이 차이를 알 수 있습니다)
//         float xDistance = _playerTransform.position.x - transform.position.x;
//         float yDistance = _playerTransform.position.y - transform.position.y;
        
//         // 2. 방향 설정 (플레이어 쪽 바라보기)
//         float direction = xDistance > 0 ? 1f : -1f;
//         transform.localScale = new Vector3(direction, 1, 1);
        
//         // 3. 이동 (좌우 이동)
//         rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

//         // 4. 점프 로직 (두 가지 상황 체크)
        
//         // A. 앞에 벽/상자가 막고 있을 때 (레이캐스트)
//         CheckAndJump(direction);

//         // B. [수직 점프] 플레이어가 머리 위에 있을 때 (높이 차이 계산)
//         // 조건: 높이 차이가 1.5m 이상이고, 수평 거리가 2m 이내일 때
//         if (yDistance > 1.5f && Mathf.Abs(xDistance) < 2.0f && _isGrounded)
//         {
//             // 쿨타임 체크 (연속 점프 방지)
//             if (Time.time > _jumpCooldown)
//             {
//                 rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
//                 _jumpCooldown = Time.time + 0.5f; // 점프 쿨타임 갱신
//                 Debug.Log("플레이어가 위에 있어 점프!");
//             }
//         }
//     }
//     private void CheckChaseTimeout()
//     {
//         if (_vision == null) return;

//         if (!_vision.IsPlayerVisible())
//         {
//             _chaseLossTimer += Time.deltaTime;
//             if (_chaseLossTimer >= chaseLossThreshold)
//             {
//                 Debug.Log("Lost Player -> Alert Mode");
//                 TransitionToState(EnemyState.Alert); // 상태 전환
//                 // AlertRoutine 시작 (주변 두리번거리기 로직이 필요하다면 여기서 호출)
//             }
//         }
//         else
//         {
//             _chaseLossTimer = 0f;
//         }
//     }

//     // 소리를 들었을 때 호출
//     public void OnHeardSound(Vector2 soundPosition)
//     {
//         if (currentState == EnemyState.Chase) return;

//         float distance = Vector2.Distance(transform.position, soundPosition);
//         if (distance <= hearingDistance)
//         {
//             Debug.Log($"{gameObject.name}: Sound heard at {soundPosition}");
//             _lastHeardPos = soundPosition;

//             // 기존 행동 멈추고 Alert 상태로 전환
//             StopBehaviorCoroutine();
            
//             // 상태 값 변경 (Alert 상태로 표시)
//             currentState = EnemyState.Alert;
            
//             // 소리난 곳으로 가는 코루틴 시작
//             _currentBehaviorRoutine = StartCoroutine(AlertRoutine());
//         }
//     }

//     IEnumerator AlertRoutine()
//     {
//         float stopDistance = 0.5f;

//         // 1. 소리난 곳으로 이동
//         while (Mathf.Abs(transform.position.x - _lastHeardPos.x) > stopDistance)
//         {
//             // 추격 등으로 상태가 바뀌면 즉시 종료
//             if (currentState != EnemyState.Alert) yield break;

//             float dir = _lastHeardPos.x > transform.position.x ? 1f : -1f;
//             transform.localScale = new Vector3(dir, 1, 1);
//             rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
//             CheckAndJump(dir);

//             yield return null;
//         }

//         // 2. 도착 후 정지 및 대기
//         rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
//         Debug.Log($"{gameObject.name}: Arrived at sound location. Searching...");
        
//         yield return new WaitForSeconds(alertDuration);

//         // 3. 다시 순찰로 복귀 (여전히 Alert 상태라면)
//         if (currentState == EnemyState.Alert)
//         {
//             TransitionToState(EnemyState.Patrol);
//         }
//     }

//     // ITeleportable 구현
//     public Transform GetTransform() => transform;
//     public void OnTeleport() => rb.linearVelocity = Vector2.zero;

//     void OnCollisionEnter2D(Collision2D collision)
//     {
//         // 바닥 체크 로직은 Update의 OverlapCircle로 통합했으므로 제거
//         // 공격 피격 등 다른 로직만 남김
//         if (collision.collider.CompareTag("Player"))
//         {
//             anim.SetTrigger("IsAttack");
//         }
//     }
// }