using UnityEngine;
using UnityEngine.SceneManagement;

public class StageTransition : MonoBehaviour
{
    [Header("이동할 씬과 스폰 위치")]
    public string nextStageName = "Stage2"; // 다음 씬 이름
    public string spawnPointName = "SpawnPoint_A"; // 다음 씬에서 태어날 위치(오브젝트)의 이름

    // 🌟 핵심: static을 붙이면 씬이 넘어가도 이 변수(메모)는 파괴되지 않고 유지됩니다!
    public static string targetSpawnName; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 씬을 넘어가기 전에, 우리가 가야 할 목표 지점의 이름을 static 메모지에 적어둡니다.
            targetSpawnName = spawnPointName;
            
            // 2. 씬 이동!
            SceneManager.LoadScene(nextStageName);
        }
    }
}