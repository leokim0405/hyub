using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Resolvers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MarkerManager : MonoBehaviour
{
    public List<Dagger> activeDaggers = new List<Dagger>();
    [SerializeField] private GameObject player;
    private ITeleportable playerTeleport;

    [Header("사운드 설정")]
    public AudioSource audioSource; // 효과음 전용 오디오 소스
    public AudioClip swapSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }


    void Awake()
    {
        playerTeleport = player.GetComponent<ITeleportable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ExecuteSwap();
        }
    }

    public void AddDagger(Dagger newDagger)
    {
        activeDaggers.RemoveAll(d => d == null);

        if (activeDaggers.Count >= 2)
        {
            // 가장 오래된 단검(0번)을 리스트에서 제거하고 파괴
            Dagger oldest = activeDaggers[0];
            activeDaggers.RemoveAt(0);
            Destroy(oldest.gameObject);
            UnityEngine.Debug.Log("oldest dagger destroy.");
        }

        activeDaggers.Add(newDagger);
    }

    private void ExecuteSwap()
    {
        activeDaggers.RemoveAll(d => d == null);

        int count = activeDaggers.Count;

        if (count == 0) return;

        if (count == 1)
        {
            ITeleportable target = activeDaggers[0].stuckTarget ?? activeDaggers[0].GetComponent<ITeleportable>();
            UnityEngine.Debug.Log("swap with player");
            Swap(playerTeleport, target);
        }
        else if (count == 2)
        {
            ITeleportable targetA = activeDaggers[0].stuckTarget ?? activeDaggers[0].GetComponent<ITeleportable>();
            ITeleportable targetB = activeDaggers[1].stuckTarget ?? activeDaggers[1].GetComponent<ITeleportable>();

            UnityEngine.Debug.Log("dagger swap dagger");

            Swap(targetA, targetB);
        }

        ClearAllDaggers();
    }

    private void Swap(ITeleportable a, ITeleportable b)
    {
        if (a == null || b == null) return;

        Transform transA = a.GetTransform();
        Transform transB = b.GetTransform();

        // 위치 교환 핵심 로직
        Vector3 tempPos = transA.position;
        transA.position = transB.position;
        transB.position = tempPos;

        // 순간이동 후 피드백 효과
        a.OnTeleport();
        b.OnTeleport();

        if(swapSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(swapSound);
        }
    }

    private void ClearAllDaggers()
    {
        foreach (var dagger in activeDaggers)
        {
            if (dagger != null) Destroy(dagger.gameObject);
        }

        activeDaggers.Clear();
    }

    public void RemoveDaggerFromList(Dagger dagger)
    {
        if (activeDaggers.Contains(dagger))
        {
            activeDaggers.Remove(dagger);
        }
    }

    


}
