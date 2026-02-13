using UnityEngine;

public class HeavyObject : MonoBehaviour, ITeleportable
{
    [Header("사운드 설정")]
    public AudioSource AudioSource;
    public AudioClip boxColidekSound;
    public float volume = 0.4f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Transform GetTransform() => transform;

    public void OnTeleport()
    {
        UnityEngine.Debug.Log("tp success");
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // if(other.collider.CompareTag("Enemy"))
        // {
        //     Destroy(gameObject);


        // }

        playSound();        
    }

    public void playSound()
    {
        if (boxColidekSound != null)
        {
            AudioSource.PlayOneShot(boxColidekSound, volume);
        }
    }
}
