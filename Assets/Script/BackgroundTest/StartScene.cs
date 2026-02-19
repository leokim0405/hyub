using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void StartGame()
    {
        SceneManager.LoadScene("stage_1");
    }

    public void ReStartGame()
    {
        SceneManager.LoadScene("EndScene");
    }

    public void ClearRestart()
    {
        SceneManager.LoadScene("StartScene");
    }

    // 게임 종료 버튼에 연결할 함수
    public void QuitGame()
    {
#if UNITY_EDITOR
        // 유니티 에디터에서 실행 중일 때
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 실제 빌드된 게임에서 실행 중일 때
        Application.Quit();
#endif

        Debug.Log("게임 종료 버튼이 클릭되었습니다.");
    }
}
