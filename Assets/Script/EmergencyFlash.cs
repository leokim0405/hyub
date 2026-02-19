using UnityEngine;
using UnityEngine.UI;

public class EmergencyFlash : MonoBehaviour
{
    private Image redPanel;

    [Header("점멸 설정")]
    public float flashSpeed = 3f;
    public float maxAlpha = 0.4f;

    void Awake()
    {
        // Image 컴포넌트를 미리 찾아둡니다.
        redPanel = GetComponent<Image>();
    }

    void Update()
    {
        float wave = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
        float currentAlpha = wave * maxAlpha;

        Color c = redPanel.color;
        c.a = currentAlpha;
        redPanel.color = c;
    }
}