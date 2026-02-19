using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void StartGame()
    {
        SceneManager.LoadScene("stage_1");
    }

    // 게임 종료 버튼에 연결할 함수
    public void QuitGame()
    {
        Application.Quit();
    }
}
