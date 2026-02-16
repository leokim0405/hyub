using UnityEngine;

public class LiquidReaction : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    [Header("섞임 설정")]
    public float mixSpeed = 0.1f; // 0.1이면 10%씩 천천히 섞임 (낮을수록 자연스러움)
    public float reactionCooldown = 0.1f; // 너무 자주 계산하지 않도록 쿨타임
    private float lastReactionTime;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }
/*
    // 물리적 충돌이 일어났을 때 호출됨
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 쿨타임 체크 (성능 최적화)
        if (Time.time < lastReactionTime + reactionCooldown) return;

        // 상대방이 액체 입자인지 확인 (태그나 컴포넌트로 확인)
        LiquidReaction otherParticle = collision.gameObject.GetComponent<LiquidReaction>();

        if (otherParticle != null)
        {
            MixAttributes(otherParticle);
            lastReactionTime = Time.time;
        }
    }
*/
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 쿨타임 체크
        if (Time.time < lastReactionTime + reactionCooldown) return;

        LiquidReaction otherParticle = collision.gameObject.GetComponent<LiquidReaction>();

        if (otherParticle != null)
        {
            MixAttributes(otherParticle);
            lastReactionTime = Time.time;
        }
    }

    void MixAttributes(LiquidReaction other)
    {
        // 1. 색상 가져오기
        Color myColor = spriteRenderer.color;
        Color otherColor = other.spriteRenderer.color;
        

        // [핵심 수정] 한쪽으로 치우친 Lerp 대신, 둘의 '평균값'을 구합니다.
        // (A + B) / 2 = 정확히 반반 섞인 색
        Color averageColor = (myColor + otherColor) / 2f;

        // 2. 이제 공평한 중간색을 향해 '각자' 조금씩 변해야 합니다.
        // 바로 averageColor로 바꾸면 너무 빠르니까, 
        // 현재 내 색에서 -> 평균색으로 mixSpeed만큼 이동합니다.
        
        this.spriteRenderer.color = averageColor;
        other.spriteRenderer.color = averageColor;

        // ---------------------------------------------------------

        // 3. 물리 속성(질량)도 똑같은 논리로 적용
        float myMass = rb.mass;
        float otherMass = other.GetComponent<Rigidbody2D>().mass;
        
        float averageMass = (myMass + otherMass) / 2f; // 질량 평균

        this.rb.mass = averageMass;
        other.GetComponent<Rigidbody2D>().mass = averageMass;

        // B. 선형 저항 (Linear Damping) - 끈적임(점성)의 평균
        // * Unity 2022 이하는 'linearDamping' 대신 'drag' 라고 쓰세요.
        float averageLinearDamping = (this.rb.linearDamping + other.GetComponent<Rigidbody2D>().linearDamping) / 2f;
        this.rb.linearDamping = averageLinearDamping;
        other.GetComponent<Rigidbody2D>().linearDamping = averageLinearDamping;

        // C. 중력 계수 (Gravity Scale) - 위로 뜨는 성질과 가라앉는 성질의 중간
        float averageGravityScale = (this.rb.gravityScale + other.GetComponent<Rigidbody2D>().gravityScale) / 2f;
        this.rb.gravityScale = averageGravityScale;
        other.GetComponent<Rigidbody2D>().gravityScale = averageGravityScale;
    }
}