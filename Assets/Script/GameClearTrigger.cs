using UnityEngine;

public class GameClearTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && GameManager.instance.hasSecretBook)
        {
            Debug.Log("탈출 성공! 클리어 화면을 띄웁니다.");
            
            GameManager.instance.GameClear();
        }
    }
}