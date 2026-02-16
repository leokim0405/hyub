using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    void Start()
    {
        // 1. 이전 씬에서 포탈이 남긴 메모(targetSpawnName)와 '내 이름'이 똑같은지 확인합니다.
        if (StageTransition.targetSpawnName == gameObject.name)
        {
            // 2. 씬 전체에서 "Player" 태그를 가진 오브젝트를 찾습니다.
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                // 3. 플레이어의 위치를 내 위치(스폰 포인트)로 강제 이동시킵니다!
                player.transform.position = transform.position;
                
                // 4. (안전장치) 일을 끝냈으니 메모지를 깨끗하게 지웁니다.
                StageTransition.targetSpawnName = ""; 
                
                Debug.Log(gameObject.name + " 위치로 플레이어를 소환했습니다!");
            }
            else
            {
                Debug.LogWarning("씬에 Player 태그를 가진 오브젝트가 없습니다!");
            }
        }
    }
}