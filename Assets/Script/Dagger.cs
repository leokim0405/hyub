using System.Collections;
using UnityEngine;

public class  Dagger : MonoBehaviour, ITeleportable
{
  private Rigidbody2D rb;
  private bool isStuck = false;
  private Coroutine destroyRoutine;

  public float lifeTime;
  public float speed;
  public ITeleportable stuckTarget;

  Animator anim;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Awake()
  {
    speed = 8f;
    lifeTime = 2f;
    rb = GetComponent<Rigidbody2D>();
    anim = GetComponent<Animator>();
    // Destroy(gameObject, lifeTime);
  }
  
  void Start()
  {
    destroyRoutine = StartCoroutine(DestroyRoutine());
  }
    // Update is called once per frame
    void Update()
    {
      // if(Time.time )
        
    }

  IEnumerator DestroyRoutine()
  {
    yield return new WaitForSeconds(lifeTime);

    if(!isStuck)
    {
      MarkerManager manager = Object.FindFirstObjectByType<MarkerManager>();
      if (manager != null)
      {
          manager.RemoveDaggerFromList(this);
      }

      Destroy(gameObject);
    }
  }

  public void Launch(Vector2 direction)
  {
      rb.linearVelocity = direction * speed;
      
      float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
      transform.rotation = Quaternion.Euler(0,0,angle);
  }

  void OnTriggerEnter2D(Collider2D other)
  {
    if(isStuck) return;

    ITeleportable target = other.GetComponent<ITeleportable>();

    if(target != null)
    {
      StickToTarget(target, other.transform);
    }
    else if (other.CompareTag("floor"))
    {
      StopMovement();
    }
  }

  private void StickToTarget(ITeleportable target, Transform targetTransform)
  {
    isStuck = true;
    stuckTarget = target;

    if (destroyRoutine != null)
    {
        StopCoroutine(destroyRoutine);
    }

    StopMovement();

    transform.SetParent(targetTransform);

    UnityEngine.Debug.Log($"marker set to : {targetTransform.name}");
  }

  private void StopMovement()
  {
    anim.SetBool("isLock", true);
    isStuck = true;
    rb.linearVelocity = Vector2.zero;
    rb.bodyType = RigidbodyType2D.Kinematic;

    if(destroyRoutine != null)
    {
      StopCoroutine(destroyRoutine);
    }
  }

  public Transform GetTransform() => transform;

  public void OnTeleport()
  {
      UnityEngine.Debug.Log("tp success");
  }

}
