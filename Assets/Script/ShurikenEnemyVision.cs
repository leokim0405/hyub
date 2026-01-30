using UnityEngine;

public class ShurikenEnemyVision : MonoBehaviour
{
    [Header("시야 설정 (공격용)")]
    public float viewDistance = 5f;        // 공격 가능한 시야 거리
    [Range(0, 360)]
    public float viewAngle = 120f;         // 공격 시야각

    [Header("감지 설정 (경계용)")]
    public float alertDistance = 7f;       // 인기척을 느끼는 거리 (보통 시야보다 멂)

    [Header("레이어 설정")]
    public LayerMask targetMask;           // 플레이어 레이어
    public LayerMask obstacleMask;         // 장애물 레이어

    // 1. 공격 조건: 시야각 내에 있고 장애물이 없는가?
    public bool IsPlayerVisible()
    {
        // 먼저 거리 내에 있는지 체크
        Collider2D target = Physics2D.OverlapCircle(transform.position, viewDistance, targetMask);

        if (target != null)
        {
            Vector2 dirToTarget = (target.transform.position - transform.position).normalized;
            
            // 적이 바라보는 방향 (X축 스케일 기준)
            Vector2 forward = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            // 시야각 체크
            if (Vector2.Angle(forward, dirToTarget) < viewAngle / 2f)
            {
                float distToTarget = Vector2.Distance(transform.position, target.transform.position);

                // 레이캐스트로 장애물 확인
                if (!Physics2D.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    return true; // 공격 가능!
                }
            }
        }
        return false;
    }

    // 2. 경계 조건: 그냥 주변(원형 범위)에 플레이어가 있는가?
    public bool IsPlayerInAlertRange()
    {
        // 각도 상관없이 거리만 체크 (청각이나 육감)
        return Physics2D.OverlapCircle(transform.position, alertDistance, targetMask);
    }

    private void OnDrawGizmos()
    {
        // Alert 범위 (노란색 원)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertDistance);

        // Attack 범위 (빨간색 부채꼴)
        Vector3 forward = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, viewAngle / 2f) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -viewAngle / 2f) * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewDistance);
        Gizmos.DrawWireSphere(transform.position, viewDistance); // 시야 거리 원
    }
}