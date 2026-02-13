using UnityEngine;

public class QuestionMarkFix : MonoBehaviour
{
    private Vector3 initialScale; // 처음에 설정한 예쁜 크기 저장용

    void Start()
    {
        // 1. 게임 시작 시, 인스펙터에서 정해준 '원래 크기'를 딱 저장해둡니다.
        initialScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (transform.parent != null)
        {
            // 2. 부모의 스케일 수치(크기)는 무시하고, '방향(부호)'만 가져옵니다.
            // 오른쪽이면 1, 왼쪽이면 -1이 나옵니다.
            float parentDirection = Mathf.Sign(transform.parent.localScale.x);

            // 3. 원래 크기(initialScale)에 부모의 방향만 곱합니다.
            // 부모가 -1이면 나도 -1이 되어, 최종적으로 화면에는 +1(정방향)로 보입니다.
            transform.localScale = new Vector3(
                initialScale.x * parentDirection,
                initialScale.y,
                initialScale.z
            );
        }
    }
}