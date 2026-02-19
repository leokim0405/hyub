using UnityEngine;

public class EnemyMapManager : MonoBehaviour
{
    [Header("적 그룹 연결")]
    public GameObject normalGroup; 
    public GameObject escapeGroup; 

    // 🌟 GameManager가 이 함수를 호출해서 모드를 바꿔줄 것입니다!
    public void SetEscapeMode(bool isEscape)
    {
        if (isEscape)
        {
            if (normalGroup != null) normalGroup.SetActive(false);
            if (escapeGroup != null) escapeGroup.SetActive(true);
        }
        else
        {
            if (normalGroup != null) normalGroup.SetActive(true);
            if (escapeGroup != null) escapeGroup.SetActive(false);
        }
    }
}