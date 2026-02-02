using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("시야 설정")]
    public float viewDistance = 5f;        // 감지 거리
    public float stealthViewMultiplier = 0.6f;  // 은신시 감지 거리 비율

    [Range(0, 360)]
    public float viewAngle = 120f;         // 시야각
    public LayerMask targetMask;           // 플레이어 레이어
    public LayerMask obstacleMask;         // 장애물 레이어

    private PlayerMove _player;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.GetComponent<PlayerMove>();
        }

    }
    // Update is called once per frame
    void Update()
    {

    }

    public bool IsPlayerVisible()
    {

        float currentMaxDistance = viewDistance;

        if (_player != null && _player.isStealth)
        {
            currentMaxDistance = viewDistance * stealthViewMultiplier;
        }

        Collider2D target = Physics2D.OverlapCircle(transform.position, currentMaxDistance, targetMask);

        if (target != null)
        {
            Vector2 dirToTarget = (target.transform.position - transform.position).normalized;

            // 2. 적이 바라보는 방향 계산 (localScale.x 기준)
            Vector2 forward = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            // 3. 두 벡터 사이의 각도가 시야각의 절반보다 작은지 확인
            if (Vector2.Angle(forward, dirToTarget) < viewAngle / 2f)
            {
                float distToTarget = Vector2.Distance(transform.position, target.transform.position);

                // 4. 레이캐스트를 쏴서 장애물 확인 (장애물에 걸리지 않아야 함)
                if (!Physics2D.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    return true; // 플레이어 발견
                }
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.white;
        // Gizmos.DrawWireSphere(transform.position, viewDistance);

        float debugDistance = viewDistance;

        // 플레이어 상태를 실시간으로 가져와서 선의 길이를 조절합니다.
        if (Application.isPlaying && _player != null && _player.isStealth)
        {
            debugDistance *= stealthViewMultiplier;
            Gizmos.color = Color.yellow; // 은신 감지 범위는 노란색으로 표시
        }
        else
        {
            Gizmos.color = Color.red;    // 평소 감지 범위는 빨간색으로 표시
        }

        Vector3 forward = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, viewAngle / 2f) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -viewAngle / 2f) * forward;

        Gizmos.DrawWireSphere(transform.position, debugDistance);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * debugDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * debugDistance);
    }
}

