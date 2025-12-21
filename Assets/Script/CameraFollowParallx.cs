using UnityEngine;

public class CameraFollowParallax : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public float oppositeFactor = 0f; // 반대 이동 비율

    private float offsetDifference; // 플레이어와 카메라의 초기 간격 차이

    void Start()
    {
        // 플레이어와 카메라 사이의 초기 X축 거리 차이를 저장
        if (target != null)
            offsetDifference = transform.position.x + target.position.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 플레이어의 X 위치에 -1을 곱한 위치를 목표로 잡음 (반대칭)
        // offsetDifference를 더해주는 이유는 초기 위치 보정을 위해
        float targetX = (target.position.x * -oppositeFactor) + offsetDifference;

        Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}