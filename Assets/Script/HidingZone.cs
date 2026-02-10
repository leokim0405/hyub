using UnityEngine;

public class HidingZone : MonoBehaviour
{
    private Collider2D zoneCollider;

    void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
    }

    // 플레이어가 영역 안에 머무는 동안 계속 체크
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어의 Collider와 Stealth 컴포넌트 가져오기
            Collider2D playerCollider = other;
            PlayerMove player = other.GetComponent<PlayerMove>();

            if (player != null)
            {
                // [핵심 로직] 구역(Bounds)이 플레이어(Bounds)를 완전히 포함하는지 체크
                bool isFullyInside = IsBoundsContained(zoneCollider.bounds, playerCollider.bounds);

                // 결과 적용
                player.SetHidingState(isFullyInside);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMove player = other.GetComponent<PlayerMove>();
            if (player != null)
            {
                player.SetHidingState(false);
            }
        }
    }

    // A가 B를 완전히 포함하는지 확인하는 함수
    private bool IsBoundsContained(Bounds container, Bounds target)
    {
        return container.Contains(target.min) && container.Contains(target.max);
    }
}