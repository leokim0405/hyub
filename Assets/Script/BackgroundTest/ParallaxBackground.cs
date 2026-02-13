using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("패럴랙스 설정")]
    [Tooltip("0이면 카메라와 함께 멈춰있고(하늘), 1이면 일반 사물처럼 지나갑니다.")]
    public float parallaxEffect; // 이동 속도 비율

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate() // 카메라가 이동한 후에 배경을 움직이기 위해 LateUpdate 사용
    {
        // 카메라가 이동한 거리 계산
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // 배경 이동 (카메라 이동량 * 효과 비율)
        transform.position += new Vector3(deltaMovement.x * parallaxEffect, deltaMovement.y * parallaxEffect, 0);

        lastCameraPosition = cameraTransform.position;
    }
}