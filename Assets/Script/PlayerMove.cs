using System.Data;
// using System.Drawing;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMove : MonoBehaviour, ITeleportable
{

    Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float dir;
    private Rigidbody2D _rigidBody;
    private SpriteRenderer spriteRenderer;
    // private Transform transform;
    private float noiseRange;
    private Color originalColor;
    // private float fireRate = 3f;
    // private float nextFireTime = 0f;
    public int playerHP = 4;
    public float speed;
    public float walkspeed;
    public float runSpeed;
    public float jumpFoce;
    public int jumpCount;
    public bool IsGround;
    public bool isStealth;
    public bool isInHidingZone;
    public bool isCrouching;


    [Header("은신 설정")]
    public float stealthAlpha = 0.5f; // 은신 시 투명도 (0: 투명, 1: 불투명)
    public float stealthSpeed = 1.0f; // 은신 시 이동 속도

    [Header("공격 설정")]
    public float attackRange = 1.5f;       // 공격 사거리
    public Vector2 attackOffset = new Vector2(0.5f, 0f); // 공격 중심점 오프셋
    public LayerMask enemyLayer;           // 적 오브젝트의 레이어
    public float attackCooldown = 1f;
    private float nextAttackTime = 0f;
    public float shootCooldown = 0.5f;
    private float nextShootTime = 0f;



    [SerializeField] private GameObject daggerPrefab;
    [SerializeField] private MarkerManager markerManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody2D>();
        jumpCount = 2;
        walkspeed = 3f;
        runSpeed = 6f;
        noiseRange = 10f;

        originalColor = spriteRenderer.color;
        if (HpUIManager.hpUI != null)
        {
            HpUIManager.hpUI.HeartUI(playerHP);
        }
    }

    // Update is called once per frame
    void Update()
    {
        dir = Input.GetAxisRaw("Horizontal");

        if (dir > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (dir < 0)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            dir = 0;
        }

        if (dir != 0)
        {
            anim.SetBool("Walking", true);
        }
        else
        {
            anim.SetBool("Walking", false);
        }

        Vector2 moveVector = new Vector2(dir * speed, 0);
        gameObject.transform.Translate(moveVector * Time.deltaTime, 0);


        if (Input.GetKey(KeyCode.LeftShift))
        {
            anim.SetBool("Running", true);
            speed = runSpeed;
        }
        else
        {
            anim.SetBool("Running", false);
            speed = walkspeed;
        }

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount != 0)
        {
            // _rigidBody.AddForce(new Vector2(0, 6), ForceMode2D.Impulse);
            _rigidBody.linearVelocity = new Vector2(0, jumpFoce);
            jumpCount = jumpCount - 1;
            anim.SetBool("Jumping", true);
        }

        // if (_rigidBody.linearVelocity.y < 0)
        // {
        //     anim.SetBool("Falling", true);
        // }

        if (Input.GetMouseButtonDown(0) && Time.time >= nextShootTime)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Throw"))
            {
                anim.SetTrigger("Throw");
                Shoot();
                nextAttackTime = Time.time + shootCooldown;
            }
        }

        if (Input.GetKeyDown(KeyCode.S) && Time.time >= nextAttackTime)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                anim.SetTrigger("Attack");
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            speed = stealthSpeed;
            isCrouching = true;
        }
        else
        {
            speed = walkspeed;
            isCrouching = false;
        }

        isStealth = isCrouching||isInHidingZone;

        SetStealth(isStealth);
    }

    public void SetHidingState(bool isInZone)
    {
        isInHidingZone = isInZone;
        // Update에서 자동으로 isStealth가 갱신됨
    }

    void SetStealth(bool isStealth)
    {
        if (isStealth)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, stealthAlpha);
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("floor") || collision.collider.CompareTag("Obstacle"))
        {
            jumpCount = 2;
            IsGround = true;
            anim.SetBool("Jumping", false);
            anim.SetBool("Falling", false);
            anim.SetBool("IsGround", true);
            if (collision.relativeVelocity.y > 3f)
            {
                MakeNoise(transform.position, 10f); // 10만큼의 범위로 소음 전파
            }
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            playerHP-=1;
            if (HpUIManager.hpUI != null)
            {
                HpUIManager.hpUI.HeartUI(playerHP);
            }
            if(playerHP<=0){
                anim.SetTrigger("Die");
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("floor") || collision.gameObject.CompareTag("Obstacle"))
        {
            IsGround = false;
            anim.SetBool("Falling", true);
            anim.SetBool("IsGround", false);
            // _rigidBody.linearVelocity.y = 0; 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            playerHP-=1;
            if (HpUIManager.hpUI != null)
            {
                HpUIManager.hpUI.HeartUI(playerHP);
            }
            if(playerHP<=0){
                anim.SetTrigger("Die");
            }
        }
    }

    void Shoot()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 finalPos = new Vector2(worldPos.x, worldPos.y);

        Vector2 playerPos = transform.position;
        Vector2 shootDir = (finalPos - playerPos).normalized;

        GameObject dagger = Instantiate(daggerPrefab, playerPos, Quaternion.identity);
        Dagger daggerScript = dagger.GetComponent<Dagger>();

        daggerScript.Launch(shootDir);

        markerManager.AddDagger(daggerScript);

        if (SkillUIManager.instance != null)
        {
            SkillUIManager.instance.TriggerCooldown(0, shootCooldown);
        }
    }

    public Transform GetTransform() => transform;

    public void OnTeleport()
    {
        UnityEngine.Debug.Log("tp success");
    }

    public void MakeNoise(Vector2 position, float noiseRange)
    {
        // 지정된 범위 내의 모든 적(Enemy 레이어)을 찾음
        Collider2D[] enemies = Physics2D.OverlapCircleAll(position, noiseRange);

        foreach (Collider2D enemyCollider in enemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && isStealth)
            {
                enemy.OnHeardSound(position);
            }
        }
    }

    void Attack()
    {
        Debug.Log("attack");

        float direction = spriteRenderer.flipX ? -1f : 1f;
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(attackOffset.x * direction, attackOffset.y);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(spawnPos, attackRange, enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyBase enemyScript = enemyCollider.GetComponent<EnemyBase>();

            if (enemyScript != null)
            {
                enemyScript.OnAssassinated();
            }

        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float direction = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
        Vector3 spawnPos = transform.position + new Vector3(attackOffset.x * direction, attackOffset.y, 0);
        Gizmos.DrawWireSphere(spawnPos, attackRange);
    }



}
