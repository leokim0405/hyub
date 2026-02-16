using UnityEngine;
using UnityEngine.UI; // UI를 다루기 위해 필수!

public class EmergencyFlash : MonoBehaviour
{
    private Image redPanel;

    [Header("점멸 설정")]
    [Tooltip("점멸하는 속도입니다.")]
    public float flashSpeed = 3f;
    [Tooltip("가장 진해질 때의 투명도(0~1)입니다. 0.3~0.5 정도가 눈이 덜 아픕니다.")]
    public float maxAlpha = 0.4f;
    public float lowAlpha = 0.1f;

    void Start()
    {
        // 1. 만약 메모지에 "비급서 먹음(true)"이라고 안 적혀있다면?
        if (Book.hasBook == false)
        {
            // 아직 평화로운 상태이므로 이 빨간 패널을 아예 꺼버립니다.
            gameObject.SetActive(false);
            return;
        }

        // 2. 비급서를 먹고 왔다면 패널의 Image 컴포넌트를 가져옵니다.
        redPanel = GetComponent<Image>();
    }

    void Update()
    {
        // 비급서를 안 먹었으면 아예 꺼져있으므로 이 Update는 실행되지 않습니다.

        // 3. Mathf.Sin을 이용해 부드러운 파동 만들기 (-1 ~ 1)
        // 파동을 0 ~ 1 사이로 바꾸기 위해 1을 더하고 반으로 나눕니다.
        float wave = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;

        // 4. 최대 투명도(maxAlpha)를 곱해서 실제 적용할 투명도를 계산합니다.
        float currentAlpha = lowAlpha + wave * maxAlpha;

        // 5. 색상에 적용!
        Color c = redPanel.color;
        c.a = currentAlpha;
        redPanel.color = c;
    }
}