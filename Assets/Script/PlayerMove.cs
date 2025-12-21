using System.Data;
using UnityEngine;

public class PlayerMove : MonoBehaviour, ITeleportable
{

    Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float dir;
    private Rigidbody2D _rigidBody;
    private SpriteRenderer spriteRenderer;
    // private Transform transform;
    private float noiseRange;
    // private float fireRate = 3f;
    // private float nextFireTime = 0f;

    public float speed;
    public float walkspeed;
    public float runSpeed;
    public float jumpFoce;
    public int jumpCount;
    public bool IsGround;


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
        // noiseRange = 10f;
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

        if (_rigidBody.linearVelocity.y < 0)
        {
            anim.SetBool("Falling", true);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
            anim.SetTrigger("Throw");
            // nextFireTime = Time.time + fireRate;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                anim.SetTrigger("Attack");
            }
        }

        Debug.Log(_rigidBody.linearVelocity.y);

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

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
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
            if (enemy != null)
            {
                enemy.OnHeardSound(position);
            }
        }
    }

}
