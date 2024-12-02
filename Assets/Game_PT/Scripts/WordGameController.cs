using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class WordGameController : MonoBehaviour
{
    public static WordGameController Instance;


    // WordDataManager�� �ν��Ͻ��� ���
    public WordDataManager wordDataManager;

    public GameObject panel1;
    public RawImage imagePanel1;
    public Button nextButton;
    public Button playButton;
    public GameObject Panel;
    public GameObject cuteBird;
    public GameObject panel2;
    public Slider progressSlider;
    public Text gameOverText;
    public Text wordNameText;
    public GameObject WordNameTextimage;

    public Text buttonText;

    public VideoPlayer videoPlayer;
    public GameObject videopanel; // videopanel ������Ʈ �߰�

    public RenderTexture newRenderTexture;
    private int currentStage = 1;  // �������� ��ȣ�� �����ϴ� ����

    private int currentWordIndex = 0;
    private List<string> selectedWords;

    public GameObject FCanvas; // Fĵ���� ���� �߰�
    public GameObject currentCanvas; // ���� ĵ���� ���� �߰�


    private string imagesPath = "Images/";
    private string videosPath = "Videos/";
    private List<string> words = new List<string> { "�����", "����", "���ö�", "�丶��", "�佺Ʈ", "���̺�", "������", "û�ұ�", "������", "������", "�����", "����", "��Ƽ��", "�ٳ���", "�ܹ���", "������", "�ʹ��", "�߿���", "������" };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

        selectedWords = GetRandomWords(3);
        //st<string> videoPaths = new List<string>();   // ���� ��� ���
        List<string> recognizedTexts = new List<string>(); // �� ����Ʈ�� �ʱ�ȭ
        //st<AudioClip> audioClips = new List<AudioClip>(); // �� ����� Ŭ�� ����Ʈ (����� �������)
        List<float> accuracyScores = new List<float>();
        List<string> feedbacks = new List<string>();
        foreach (var word in selectedWords)
        {
            //tring videoPath = videosPath + word + ".mp4"; // ������ ��� ����
            //ideoPaths.Add(videoPath);
        }

        // ���õ� �ܾ�� ������ ��θ� JSON�� ���� (recognizedTexts�� audioClips�� �� ����Ʈ�� ����)
        WordDataManager.SaveData(selectedWords, recognizedTexts, /*audioClips, videoPaths,*/ accuracyScores, feedbacks); // Ŭ���� �̸����� ȣ��

        Debug.Log("Data save Success: " + string.Join(",", selectedWords));

        ShowImageAndWord(selectedWords[currentWordIndex]);

        nextButton.onClick.AddListener(OnNextButtonClick);
        nextButton.gameObject.SetActive(false);

        playButton.onClick.AddListener(OnPlayButtonClick);

        buttonText.text = "���� �ܾ��";

        Panel.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        cuteBird.gameObject.SetActive(false);

        RawImage rawImage = panel2.GetComponentInChildren<RawImage>();
        rawImage.texture = newRenderTexture;

        videoPlayer.targetTexture = newRenderTexture;

        PlayVideo(selectedWords[currentWordIndex]);

        UpdateProgressText();

        // VideoPlayer�� ����� ������ �� ȣ��� �̺�Ʈ ���
        videoPlayer.loopPointReached += OnVideoEnd;

        // ó���� videopanel�� Ȱ��ȭ (������ ��� ������ �ʴٰ� ����)
        videopanel.SetActive(true);
    }

    void Update()
    {
        // SpeechToText ��ũ��Ʈ�� finalFailPanel �Ǵ� panelToActivate�� Ȱ��ȭ�� ��� WordNameTextimage�� ��� ��Ȱ��ȭ
        if (SpeechToText.Instance != null &&
            (SpeechToText.Instance.finalFailPanel.activeSelf || SpeechToText.Instance.panelToActivate.activeSelf))
        {
            WordNameTextimage.SetActive(false);
        }
        else
        {
            WordNameTextimage.SetActive(true);
        }

        // VideoPlayer�� ��� ������ Ȯ���Ͽ� videopanel�� Ȱ��ȭ/��Ȱ��ȭ
        if (videoPlayer.isPlaying)
        {
            videopanel.SetActive(false); // ���� ��� ���� �� ��Ȱ��ȭ
        }
        else
        {
            videopanel.SetActive(true); // ���� ��� ���� �ƴ� �� Ȱ��ȭ
        }

    }
    // ���� ����� ������ �� ȣ��Ǵ� �޼���
    private void OnVideoEnd(VideoPlayer vp)
    {
        videopanel.SetActive(true); // ������ ������ videopanel Ȱ��ȭ
    }

    // ���õ� �ܾ ��ȯ�ϴ� �޼���
    public List<string> GetSelectedWords()
    {
        return selectedWords;
    }

    // ���� ��� ����� ��ȯ�ϴ� �޼���
    /*ublic List<string> GetVideoPaths()
     {
         List<string> videoPaths = new List<string>();
         foreach (var word in selectedWords)
         {
             string videoPath = videosPath + word + ".mp4"; // ������ ��� ����
             videoPaths.Add(videoPath);
         }
         return videoPaths;
     }*/

    // ���� �������� ��ȣ�� ��ȯ�ϴ� �޼���
    public int GetCurrentStage()
    {
        return currentStage;  // �������� ��ȣ ��ȯ
    }

    // �������� ��ȣ�� ������Ű�� �޼��� (�ʿ�� ȣ��)
    public void IncrementStage()
    {
        currentStage++;
    }

    List<string> GetRandomWords(int count)
    {
        List<string> selectedWords = new List<string>();
        while (selectedWords.Count < count)
        {
            string randomWord = words[Random.Range(0, words.Count)];
            if (!selectedWords.Contains(randomWord))
            {
                selectedWords.Add(randomWord);
            }
        }
        return selectedWords;
    }

    void ShowImageAndWord(string word)
    {
        Texture2D texture = Resources.Load<Texture2D>(imagesPath + word);
        if (texture != null)
        {
            imagePanel1.texture = texture;
            wordNameText.text = word;
        }
        else
        {
            Debug.LogWarning("Image not found for word: " + word);
        }
    }

    // ���� ����� ���� ������ ���� ������ PlayVideo �޼���
    void PlayVideo(string word)
    {
        string videoPath = videosPath + word; // Ȯ���� ����

        // ���� ���
        VideoClip videoClip = Resources.Load<VideoClip>(videoPath);
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
            videopanel.SetActive(false);
            videoPlayer.Play();

            // ���õ� ���� ������ ������ ����
            List<string> recognizedTexts = new List<string>(); // ���÷� �� �ؽ�Ʈ ���
            List<float> accuracyScores = new List<float>();
            List<string> feedbacks = new List<string>();

            // selectedWords�� null�� �ƴ��� Ȯ��
            if (selectedWords == null)
            {
                Debug.LogError("selectedWords is null!");
                return;
            }

            wordDataManager.SaveDataLocally(selectedWords, recognizedTexts, accuracyScores, feedbacks);
        }
        else
        {
            Debug.LogWarning("Video not found for word: " + word);
        }
    }


    void OnNextButtonClick()
    {
        currentWordIndex++;

        if (currentWordIndex < selectedWords.Count)
        {
            ShowImageAndWord(selectedWords[currentWordIndex]);

            if (currentWordIndex == selectedWords.Count - 1)
            {
                buttonText.text = "���� ����";
            }

            UpdateProgressText();
            PlayVideo(selectedWords[currentWordIndex]);
        }
        else
        {
            EndGame();
        }

        nextButton.gameObject.SetActive(false);
    }

    void EndGame()
    {
        Debug.Log("���� ����");

        panel1.SetActive(false);
        nextButton.gameObject.SetActive(false);
        Panel.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(true);
        wordNameText.gameObject.SetActive(false);
        WordNameTextimage.gameObject.SetActive(false);
        cuteBird.gameObject.SetActive(true);
    }

    void OnPlayButtonClick()
    {
        if (videoPlayer.clip != null)
        {
            videoPlayer.Play();
        }
    }

    void UpdateProgressText()
    {
        progressSlider.value = (float)(currentWordIndex) / selectedWords.Count;
    }

    public string GetCurrentWord()
    {
        return selectedWords[currentWordIndex];
    }
    // ���� �ܾ��� �ε����� ��ȯ�ϴ� �޼��� �߰�
    public int GetCurrentWordIndex()
    {
        return currentWordIndex;
    }
    public void IncrementSlider()
    {
        // �����̴��� ���� �� ĭ ������Ŵ
        progressSlider.value += 1.0f / selectedWords.Count;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ReturnToMain()
    {
        // PlayerPrefs�� Ȱ��ȭ�� ĵ���� �̸� ����
        PlayerPrefs.SetString("TargetCanvasName", "Training");

        // main �� �ε�
        SceneManager.LoadScene("Main");
    }

    // Main ������ ���ư��� �޼���
    public void ReturnMainHome()
    {
        SceneManager.LoadScene("Main");
    }
    public void ExitGame()
    {
        Debug.Log("ExitGame ��ư�� ���Ƚ��ϴ�.");

        // FCanvas Ȱ��ȭ
        FCanvas.SetActive(true);

        // ���� �����͸� TrainingFetcher�� �ٷ� �ε�
        TrainingFetcher.instance.LoadPronunciationTrainingData(); // �ٷ� �����͸� �ε�

        // ���� ĵ������ ��Ȱ��ȭ
        if (currentCanvas != null)
        {
            currentCanvas.SetActive(false);
        }
    }

}