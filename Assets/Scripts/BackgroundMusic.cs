using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager instance;
    public AudioSource backgroundMusic;
    public Toggle musicToggle;

    public Canvas ptCanvas;
    public Canvas btCanvas;
    public Canvas ftCanvas;
    public Canvas btFeedback;
    public Canvas ptFeedback;
    public Canvas ftFeedback;

    private string[] gameScenes = { "BreathGame", "PronunciationGame", "FluencyGame" };

    private bool isMusicPausedByCanvas = false; // ĵ������ ���� ������ �Ͻ������Ǿ����� ����

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (!backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }

        if (musicToggle != null)
        {
            musicToggle.isOn = backgroundMusic.isPlaying;
            musicToggle.onValueChanged.RemoveAllListeners();
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsGameScene(scene.name))
        {
            if (backgroundMusic.isPlaying)
            {
                backgroundMusic.Pause();
                isMusicPausedByCanvas = true;
            }
        }
        else
        {
            if (!backgroundMusic.isPlaying && !isMusicPausedByCanvas)
            {
                backgroundMusic.UnPause();
            }
        }
    }

    public void ToggleMusic(bool isOn)
    {
        if (isOn)
        {
            if (!isMusicPausedByCanvas) // ĵ������ ������ �Ͻ������� ��� ����� ������ ������� �ʵ��� ��
            {
                backgroundMusic.Play();
            }
        }
        else
        {
            backgroundMusic.Pause();
        }
    }

    // Ư�� ĵ���� Ȱ��ȭ �� ���� �Ͻ�����
    public void CheckCanvasState()
    {
        if (ptCanvas.isActiveAndEnabled ||
            btCanvas.isActiveAndEnabled ||
            ftCanvas.isActiveAndEnabled ||
            btFeedback.isActiveAndEnabled ||
            ptFeedback.isActiveAndEnabled ||
            ftFeedback.isActiveAndEnabled)
        {
            if (backgroundMusic.isPlaying)
            {
                backgroundMusic.Pause();
                isMusicPausedByCanvas = true; // ĵ������ ���� ������ �����Ǿ����� ǥ��
            }
        }
        else
        {
            if (!backgroundMusic.isPlaying && musicToggle.isOn)
            {
                backgroundMusic.UnPause();
                isMusicPausedByCanvas = false;
            }
        }
    }

    void Update()
    {
        // ĵ���� ���¸� �� ������ Ȯ��
        CheckCanvasState();
    }

    private bool IsGameScene(string sceneName)
    {
        foreach (string gameScene in gameScenes)
        {
            if (sceneName == gameScene)
            {
                return true;
            }
        }
        return false;
    }
}
