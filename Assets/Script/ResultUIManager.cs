using TMPro;
using UnityEngine;
using UnityEngine.UI; // UI 기본 (Text 등)

public class ResultUIManager : MonoBehaviour
{
    public static ResultUIManager instance;

    [Header("UI 패널 연결")]
    public GameObject gameOverPanel;
    public GameObject gameClearPanel;
    public TMPro.TextMeshProUGUI clearTimeText; // 만약 TextMeshPro를 쓰신다면 TMPro.TextMeshProUGUI 로 변경하세요!

    void Start()
    {
        // 시작할 때는 둘 다 안 보이게 꺼둡니다.
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
    }

    // GameManager가 호출할 게임오버 함수
    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    // GameManager가 호출할 게임클리어 함수
    public void ShowGameClear(float finalTime)
    {
        if (gameClearPanel != null) gameClearPanel.SetActive(true);
        
        // 초(float)로 되어있는 시간을 분:초 포맷으로 예쁘게 바꿉니다.
        int minutes = Mathf.FloorToInt(finalTime / 60f);
        int seconds = Mathf.FloorToInt(finalTime % 60f);
        string niceTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (clearTimeText != null) 
        {
            clearTimeText.text = "Clear Time: " + niceTime;
        }

        Time.timeScale = 0f; // 화면이 뜨면 게임 일시정지!
    }

    // 메인화면 버튼을 눌렀을 때 실행될 함수
    public void OnClickMainMenuButton()
    {
        GameManager.instance.GoToMainMenu();
    }
}