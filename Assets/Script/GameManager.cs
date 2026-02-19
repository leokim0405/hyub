using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("게임 전역 데이터")]
    public bool hasSecretBook = false; 
    public int playerCurrentHP = 4; // 현재 체력 (기본값 3)
    public int playerMaxHP = 4;     // 최대 체력 (기본값 3)

    [Header("BGM 설정")]
    public AudioClip normalBGM; // 평화로운 잠입 BGM
    public AudioClip escapeBGM; // 긴박한 탈출 BGM
    private AudioSource bgmPlayer; // 음악을 재생할 플레이어 기기

    [Header("게임 상태 및 타이머")]
    public float playtime = 0f;
    public bool isTimerRunning = false;
    public bool isGameOver = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 

            // 🎵 1. 내 몸(GameManager)에 오디오 플레이어가 있는지 확인하고, 없으면 달아줍니다.
            bgmPlayer = GetComponent<AudioSource>();
            if (bgmPlayer == null)
            {
                bgmPlayer = gameObject.AddComponent<AudioSource>();
            }
            bgmPlayer.loop = true; // BGM이니까 무한 반복 켜기!
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 현재 활성화된 씬을 가져와서 OnSceneLoaded 함수에 억지로 집어넣습니다.
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void Update()
    {
        // 🌟 타이머가 켜져 있으면 매 초마다 시간을 증가시킵니다.
        if (isTimerRunning)
        {
            playtime += Time.deltaTime;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"==== [{scene.name}] 씬 로드 완료 ====");

        if (scene.name == "stage_1" && !hasSecretBook)
        {
            playtime = 0f;
            isTimerRunning = true;
            isGameOver = false;
            Time.timeScale = 1f; // 혹시 정지되었던 시간 정상화
        }

        if (scene.name == "stage_1" || scene.name == "stage_2")
        {
            if (hasSecretBook)
            {
                SetupEscapeMode();
            }
            else
            {
                SetupNormalMode();
            }
        }
    }

    public void GameOver()
    {
        // 이미 게임오버 상태면 중복 실행 방지
        if (isGameOver) return;
        
        isGameOver = true;
        isTimerRunning = false; 

        // 🌟 대기 없이 즉시 씬에서 UI 매니저를 찾아서 켭니다!
        ResultUIManager ui = Object.FindFirstObjectByType<ResultUIManager>(FindObjectsInactive.Include);
        
        if (ui != null) 
        {
            ui.ShowGameOver(); // 게임오버 창 켜기
        }
        else
        {
            // 만약 UI를 못 찾았다면 콘솔창에 빨간 글씨로 경고를 띄웁니다!
            Debug.LogError("🚨 ResultUIManager를 찾을 수 없습니다! 씬에 스크립트가 배치되어 있나요?");
        }
    }

    public void GameClear()
    {
        isTimerRunning = false; // 타이머 정지
        
        ResultUIManager ui = Object.FindFirstObjectByType<ResultUIManager>(FindObjectsInactive.Include);
        if (ui != null) ui.ShowGameClear(playtime);
    }

    public void GoToMainMenu()
    {
        // 모든 상태 초기화
        hasSecretBook = false;
        playerCurrentHP = playerMaxHP;
        playtime = 0f;
        isGameOver = false;
        Time.timeScale = 1f; // 정지된 시간 다시 흐르게

        // "MainMenu" 라는 이름의 씬으로 이동 (실제 메인 씬 이름으로 바꿔주세요!)
        SceneManager.LoadScene("MainMenu"); 
    }

    // 🔴 [탈출 모드] 세팅 명령
    private void SetupEscapeMode()
    {
        EnemyMapManager mapManager = Object.FindFirstObjectByType<EnemyMapManager>();
        if (mapManager != null) mapManager.SetEscapeMode(true);

        EmergencyFlash flashUI = Object.FindFirstObjectByType<EmergencyFlash>(FindObjectsInactive.Include);
        if (flashUI != null) flashUI.gameObject.SetActive(true);

        // 🎵 탈출 BGM 틀기!
        ChangeBGM(escapeBGM);
    }

    // 🟢 [잠입 모드] 세팅 명령
    private void SetupNormalMode()
    {
        EnemyMapManager mapManager = Object.FindFirstObjectByType<EnemyMapManager>();
        if (mapManager != null) mapManager.SetEscapeMode(false);

        EmergencyFlash flashUI = Object.FindFirstObjectByType<EmergencyFlash>(FindObjectsInactive.Include);
        if (flashUI != null) flashUI.gameObject.SetActive(false);

        // 🎵 평소 BGM 틀기!
        ChangeBGM(normalBGM);
    }

    // 🎵 음악 교체 전용 함수 (핵심 기술)
    private void ChangeBGM(AudioClip newClip)
    {
        if (newClip == null) return; // 넣은 음악이 없으면 무시
        
        // 🌟 이미 똑같은 노래가 나오고 있다면? 처음부터 다시 틀지 않고 그냥 둡니다.
        // (1스테이지에서 2스테이지로 넘어갈 때 노래가 뚝 끊기고 다시 시작되는 걸 방지!)
        if (bgmPlayer.clip == newClip && bgmPlayer.isPlaying) return;

        bgmPlayer.clip = newClip;
        bgmPlayer.Play();
    }
}