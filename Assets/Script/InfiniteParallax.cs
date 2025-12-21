using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    private float _length, _startPos;
    public GameObject cam; // 메인 카메라를 할당하세요
    public float parallaxEffect; // 0~1 사이의 값 (0: 고정, 1: 카메라와 함께 이동)

    void Start()
    {
        _startPos = transform.position.x;
        // 배경 이미지의 가로 길이를 자동으로 가져옵니다.
        _length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        // 1. 카메라가 이동한 거리 (패럴랙스용)
        float distance = (cam.transform.position.x * parallaxEffect);
        
        // 2. 루프를 위한 임시 위치 계산
        // 카메라가 배경의 길이를 넘었는지 확인하기 위한 용도입니다.
        float temp = (cam.transform.position.x * (1 - parallaxEffect));

        // 실제 배경 위치 이동
        transform.position = new Vector3(_startPos + distance, transform.position.y, transform.position.z);

        // 3. 무한 루프 로직 (재배치)
        // 카메라가 현재 배경 이미지의 오른쪽 끝을 넘어가면
        if (temp > _startPos + _length) 
        {
            _startPos += _length;
        }
        // 카메라가 현재 배경 이미지의 왼쪽 끝을 넘어가면
        else if (temp < _startPos - _length) 
        {
            _startPos -= _length;
        }
    }
}