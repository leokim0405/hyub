using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillUIManager : MonoBehaviour
{
    public static SkillUIManager instance;

    //[Header("쿨타임 오버레이 이미지들")]
    public Image[] cooldownOverlays; // 0: 표창, 1: 대시 등 순서대로 연결

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // 시작 시 쿨타임 0으로 초기화
        foreach (var img in cooldownOverlays)
        {
            img.fillAmount = 0;
        }
    }

    // 스킬 사용 시 호출: index(몇 번 스킬인지), cooldownTime(몇 초인지)
    public void TriggerCooldown(int skillIndex, float cooldownTime)
    {
        if (skillIndex < 0 || skillIndex >= cooldownOverlays.Length) return;
        
        StartCoroutine(CooldownRoutine(cooldownOverlays[skillIndex], cooldownTime));
    }

    IEnumerator CooldownRoutine(Image overlay, float time)
    {
        overlay.fillAmount = 1f; // 어둡게 시작
        float timer = 0f;

        while (timer < time)
        {
            timer += Time.deltaTime;
            // 시간이 지날수록 1 -> 0으로 줄어듦
            overlay.fillAmount = 1f - (timer / time);
            yield return null;
        }

        overlay.fillAmount = 0f; // 쿨타임 끝
    }
}