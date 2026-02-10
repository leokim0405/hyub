using UnityEngine;

public class DieAnimation : StateMachineBehaviour
{
    private SpriteRenderer spriteRenderer;

    // 상태에 진입할 때 처음 한 번 호출됩니다.
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 애니메이터가 붙은 오브젝트에서 SpriteRenderer를 가져옵니다.
        spriteRenderer = animator.GetComponent<SpriteRenderer>();
    }

    // 상태가 유지되는 동안 매 프레임 호출됩니다.
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (spriteRenderer != null)
        {
            // stateInfo.normalizedTime은 애니메이션의 진행도(0.0 ~ 1.0)를 나타냅니다.
            // 진행도가 1.0에 가까워질수록 Alpha를 0에 가깝게 만듭니다.
            float alpha = 1f - stateInfo.normalizedTime;

            Color color = spriteRenderer.color;
            color.a = Mathf.Clamp01(alpha); // 0과 1 사이로 고정
            spriteRenderer.color = color;
        }

    }

    // 상태가 끝날 때(애니메이션 종료 시) 호출됩니다.
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 애니메이션이 끝나면 오브젝트를 파괴합니다.
        Destroy(animator.gameObject);
    }
}