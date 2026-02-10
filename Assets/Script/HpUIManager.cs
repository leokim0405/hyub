using UnityEngine;
using UnityEngine.UI;

public class HpUIManager : MonoBehaviour
{
    public static HpUIManager hpUI;
    //[Header("UI 컴포넌트")]
    public Image[] hearts; // 하트 이미지 배열 (순서대로 0, 1, 2, 3)

    //[Header("스프라이트 리소스")]
    public Sprite fullHeart;  // 꽉 찬 하트 이미지
    public Sprite emptyHeart; // 빈 하트 이미지

    // 체력이 변할 때마다 호출할 함수

    void Awake()
    {
        // 게임 시작 시, 이 스크립트 자신을 static 변수에 할당
        if (hpUI == null)
        {
            hpUI = this;
        }
    }
    public void HeartUI(int currentHP)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            // 인덱스(i)가 현재 체력보다 작으면 "꽉 찬 하트"
            // 예: 체력이 3이면 -> 0, 1, 2번 하트는 Full / 3번 하트는 Empty
            if (i < currentHP)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }
}