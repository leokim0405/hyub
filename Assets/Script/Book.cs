using UnityEngine;
using UnityEngine.Playables; // 🚀 타임라인(PlayableDirector)을 쓰기 위해 필수!
using UnityEngine.SceneManagement;

public class Book : MonoBehaviour
{
    [Header("컷신 및 이동 설정")]
    public PlayableDirector cutsceneDirector; // 아까 만든 CutsceneManager를 연결할 칸
    public string escapeStageName = "Stage2"; // 탈출할 씬 이름

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 비급서를 먹었으니 화면에서 숨김 (안 지우고 투명하게만)
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;

            // (선택) 플레이어가 움직이지 못하게 PlayerMove 스크립트 끄기
            // collision.GetComponent<PlayerMove>().enabled = false;

            // 2. 컷신 재생 시작!
            if (cutsceneDirector != null)
            {
                GameManager.instance.StopBGM();
                cutsceneDirector.Play();
                // 컷신이 끝나는 순간을 감지해서 아래 OnCutsceneEnded 함수를 실행시킴
                cutsceneDirector.stopped += OnCutsceneEnded; 
            }
        }
    }

    // 컷신이 다 끝나면 자동으로 실행되는 함수
    private void OnCutsceneEnded(PlayableDirector director)
    {
        GameManager.instance.hasSecretBook = true;
        StageTransition.targetSpawnName = "SpawnPoint_B"; 
        
        // 4. 2스테이지로 씬 로드
        SceneManager.LoadScene(escapeStageName);
    }
}