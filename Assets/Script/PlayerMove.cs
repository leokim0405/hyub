using System.Data;
using UnityEngine;

public class PlayerMove : MonoBehaviour, ITeleportable
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float dir;
    private Rigidbody2D _rigidBody;
    // private float fireRate = 3f;
    // private float nextFireTime = 0f;

    public float speed;
    public float jumpFoce;
    public int jumpCount;
    private float noiseRange;

    [SerializeField] private GameObject daggerPrefab;
    [SerializeField] private MarkerManager markerManager;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        jumpCount = 2;
        // noiseRange = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        dir = Input.GetAxisRaw("Horizontal");

        Vector2 moveVector = new Vector2(dir * speed, 0);
        gameObject.transform.Translate(moveVector * Time.deltaTime, 0);

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount != 0)
        {
            // _rigidBody.AddForce(new Vector2(0, 6), ForceMode2D.Impulse);
            _rigidBody.linearVelocity = new Vector2(0, jumpFoce);
            jumpCount = jumpCount - 1;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
            // nextFireTime = Time.time + fireRate;
        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("floor"))
        {
            jumpCount = 2;
            if (collision.relativeVelocity.y > 3f)
            {
                MakeNoise(transform.position, 10f); // 10만큼의 범위로 소음 전파
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
