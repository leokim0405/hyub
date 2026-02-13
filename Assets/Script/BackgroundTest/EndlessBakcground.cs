using UnityEngine;

public class EndlessBackground : MonoBehaviour
{
    [Header("필수 세팅")]
    public Transform cam; // 이동하는 메인 카메라
    public float parallaxSpeed; // 0 = 하늘(안 움직임), 0.5 = 먼 산, 0.9 = 가까운 나무

    private float length; // 배경의 가로 길이
    private float startPosX; // 배경이 시작된 원래 위치

    void Start()
    {
        startPosX = transform.position.x;
        // 내 배경 이미지의 가로 길이를 유니티가 스스로 계산해서 저장함
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        // 1. 카메라가 원래 위치에서 얼마나 멀어졌는지 계산 (패럴랙스 속도 곱하기)
        float distance = cam.position.x * parallaxSpeed;
        
        // 2. 배경 이동 (카메라가 가는 방향으로 시차를 두고 따라감)
        transform.position = new Vector3(startPosX + distance, transform.position.y, transform.position.z);

        // --- 여기가 무한 스크롤(순간이동) 마술의 핵심 ---

        // 3. 카메라가 배경을 얼마나 앞질러 갔는지 체크하는 수치
        // (만약 parallaxSpeed가 1이면 이 수치는 0이 되어서 무한 스크롤이 고장납니다!)
        float checkPos = cam.position.x * (1 - parallaxSpeed);

        // 4. 카메라가 오른쪽으로 너무 많이 가서 배경 끝(length)을 벗어나려 하면?
        if (checkPos > startPosX + length)
        {
            // 배경의 시작 위치를 오른쪽으로 한 칸(length) 순간이동!
            startPosX += length; 
        }
        // 5. 카메라가 왼쪽으로 너무 많이 가서 벗어나려 하면?
        else if (checkPos < startPosX - length)
        {
            // 배경의 시작 위치를 왼쪽으로 한 칸 순간이동!
            startPosX -= length; 
        }
    }
}