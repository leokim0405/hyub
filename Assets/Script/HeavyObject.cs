using UnityEngine;

public class HeavyObject : MonoBehaviour, ITeleportable
{
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
}
