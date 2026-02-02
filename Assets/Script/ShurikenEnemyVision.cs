using UnityEngine;

public class ShurikenEnemyVision : MonoBehaviour
{
    [Header("시야 설정 (공격용)")]
    public float viewDistance = 5f;        // 공격 가능한 시야 거리
    public float stealthMultiplier = 0.6f;

    [Range(0, 360)]
    public float viewAngle = 120f;         // 공격 시야각

    [Header("감지 설정 (경계용)")]
    public float alertDistance = 7f;       // 인기척을 느끼는 거리 (보통 시야보다 멂)

    [Header("레이어 설정")]
    public LayerMask targetMask;           // 플레이어 레이어
    public LayerMask obstacleMask;         // 장애물 레이어

    private PlayerMove _playerMove;

    void Awake()
    {
        // 플레이어 태그로 찾아서 스크립트를 미리 가져옵니다.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerMove = playerObj.GetComponent<PlayerMove>();
        }
    }

    // 1. 공격 조건: 시야각 내에 있고 장애물이 없는가?
    public bool IsPlayerVisible()
    {
        float currentMaxDistance = viewDistance;

        if (_playerMove != null && _playerMove.isStealth)
        {
            currentMaxDistance = viewDistance * stealthMultiplier;
        }
        // 먼저 거리 내에 있는지 체크
        Collider2D target = Physics2D.OverlapCircle(transform.position, currentMaxDistance, targetMask);

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
        float currentAlertDistance = alertDistance;
        if (_playerMove != null && _playerMove.isStealth)
        {
            currentAlertDistance = alertDistance * stealthMultiplier;
        }

        return Physics2D.OverlapCircle(transform.position, currentAlertDistance, targetMask);
    }

    private void OnDrawGizmos()
    {
        // Alert 범위 (노란색 원)
        float currentD = viewDistance;
        float currentA = alertDistance;

        if (Application.isPlaying && _playerMove != null && _playerMove.isStealth)
        {
            currentD *= stealthMultiplier;
            currentA *= stealthMultiplier;
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f); // 은신 시 노란색 반투명
        }

        // Alert 범위 (노란색 원)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currentA);

        // Attack 범위 (빨간색 부채꼴)
        Vector3 forward = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, viewAngle / 2f) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -viewAngle / 2f) * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * currentD);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * currentD);
        Gizmos.DrawWireSphere(transform.position, currentD);
    }
}