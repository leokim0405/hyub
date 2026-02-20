using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스

public class KatanaZeroSpeech : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private RectTransform bubbleContainer; // 말풍선 배경(BubblePanel)

// --- [추가됨] 오디오 소스 컴포넌트 ---
    [SerializeField] private AudioSource audioSource; 

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed = 0.04f;
    
    // --- [추가됨] 효과음 설정 ---
    [SerializeField] private AudioClip typeSound; // 타자 소리 파일
    [SerializeField] private float minPitch = 0.9f; // 최소 피치 (낮은 소리)
    [SerializeField] private float maxPitch = 1.1f; // 최대 피치 (높은 소리)

    [Header("Wobble Settings")]
    [SerializeField] private bool isShaking = true;
    [SerializeField] private float shakeAmount = 2.0f; // 떨림 강도
    [SerializeField] private float shakeSpeed = 5.0f;  // 떨림 속도

    private Coroutine typingCoroutine;

    private void Update()
    {
        // 1. 텍스트가 흔들리는 효과 (매 프레임 갱신)
        if (isShaking)
        {
            ApplyJitterEffect();
        }
    }

    // 외부에서 호출: 대화 시작
    public void SetText(string text)
    {
        textComponent.text = text;
        
        // 레이아웃이 텍스트 변경을 즉시 인지하도록 강제 업데이트 (Unity 6 UI 갱신 이슈 방지)
        Canvas.ForceUpdateCanvases();
        bubbleContainer.gameObject.SetActive(true);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypewriterRoutine());
    }

    private IEnumerator TypewriterRoutine()
    {
        textComponent.ForceMeshUpdate(); // 텍스트 정보 갱신
        int totalChars = textComponent.textInfo.characterCount;
        
        // 처음엔 0글자만 보임
        textComponent.maxVisibleCharacters = 0;

        for (int i = 0; i <= totalChars; i++)
        {
            textComponent.maxVisibleCharacters = i;
            
            // 여기에 오디오 재생 코드 추가 (예: PlayBlipSound())
            // --- [추가됨] 소리 재생 로직 ---
            // 마지막 글자가 아니고, 공백이 아닐 때만 소리 재생
            if (i < totalChars && !char.IsWhiteSpace(textComponent.text[i])) 
            {
                PlayTypeSound();
            }

            // 구두점(., ?, !)에서는 조금 더 쉬기
            if (i > 0 && IsPunctuation(textComponent.textInfo.characterInfo[i - 1].character))
            {
                yield return new WaitForSeconds(typingSpeed * 5f);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }
    
    // --- [추가됨] 피치 랜덤 재생 함수 ---
    private void PlayTypeSound()
    {
        if (audioSource != null && typeSound != null)
        {
            // 매번 음정(Pitch)을 약간씩 다르게 설정하여 기계적인 느낌 제거
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(typeSound);
        }
    }

    // 버텍스 애니메이션 핵심 로직
    private void ApplyJitterEffect()
    {
        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            // 공백이나 안 보이는 글자는 스킵
            if (!charInfo.isVisible) continue;

            // 해당 글자의 Material 인덱스와 Vertex 인덱스 가져오기
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            // 원본 버텍스 배열 참조
            Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

            // 꿀렁거리는 오프셋 계산 (PerlinNoise나 Sin/Cos 활용)
            // Time.time에 인덱스(i)를 섞어 글자마다 다르게 움직이게 함
            float offsetX = (Mathf.PerlinNoise(Time.time * shakeSpeed, i) - 0.5f) * shakeAmount;
            float offsetY = (Mathf.PerlinNoise(i, Time.time * shakeSpeed) - 0.5f) * shakeAmount;

            Vector3 jitter = new Vector3(offsetX, offsetY, 0);

            // 사각형(Quad)을 구성하는 4개의 점(Vertex) 모두 이동
            sourceVertices[vertexIndex + 0] += jitter; // 좌하단
            sourceVertices[vertexIndex + 1] += jitter; // 좌상단
            sourceVertices[vertexIndex + 2] += jitter; // 우상단
            sourceVertices[vertexIndex + 3] += jitter; // 우하단
        }

        // 변경된 버텍스를 실제 메시에 적용
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textComponent.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    private bool IsPunctuation(char c)
    {
        return c == '.' || c == ',' || c == '!' || c == '?';
    }
    // 기존 클래스 안에 추가
    [ContextMenu("Test Dialogue")] // <- 이 한 줄이 핵심입니다.
    public void TestDialogue()
    {
        // 테스트하고 싶은 문구를 여기에 적으세요.
        SetText("시스템 가동.\n목표를 <color=red>제거</color>하라.");
    }

    private void OnEnable()
    {
        Debug.Log("UI가 켜졌습니다! 이벤트를 발생시킵니다.");
        
        // 인스펙터에 연결해둔 이벤트들을 전부 실행해라!
        SetText("쥐가 숨어들어왔구나.\n교인들은 당장 이를 찾아내 죽이라.");
    }
    
}