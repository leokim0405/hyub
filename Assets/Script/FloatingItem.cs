using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    [Header("둥실거림 설정")]
    [Tooltip("위아래로 움직이는 폭(높이)입니다. 클수록 많이 오르내립니다.")]
    public float amplitude = 0.2f; // 움직임 폭 (진폭)

    [Tooltip("움직이는 속도입니다. 클수록 빨리 까딱거립니다.")]
    public float frequency = 1.5f; // 움직임 속도 (진동수)

    // 아이템이 처음에 놓여있던 기준 위치를 기억할 변수
    private Vector3 startPos;

    void Start()
    {
        // 게임 시작 시점의 원래 위치를 기억해둡니다.
        startPos = transform.position;
    }

    void Update()
    {
        // --- 수학 시간(싸인파) 등장! ---
        // Mathf.Sin(Time.time * 속도) -> 시간이 흐름에 따라 -1 ~ 1 사이의 값을 부드럽게 오가는 파도 모양을 만듭니다.
        // 여기에 amplitude(높이)를 곱해서 위아래 움직임 폭을 결정합니다.
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;

        // 계산된 새로운 Y 높이를 적용합니다. (X랑 Z는 원래 위치 그대로 유지)
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}