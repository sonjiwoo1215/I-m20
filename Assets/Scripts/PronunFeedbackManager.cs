using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PronunFeedbackManager : MonoBehaviour
{
    public Text wordText;
    public Text childWordText;
    public Text accuracyText;
    public Text feedbackText;
    public Button nextButton;
    public Button previousButton;
    public Button playTeacherVoiceButton; // ������ �Ҹ� ��� ��ư
    public Button playChildVoiceButton;  // �Ƶ� �Ҹ� ��� ��ư
    public AudioSource audioSource;       // ����� ����� ���� AudioSource
    public Image accuracyBar;             // ��Ȯ�� ������ ǥ���� �׷��� �̹���

    private int currentWordIndex = 0;     // ���� ǥ�� ���� �ܾ��� �ε���
    private TrainingFetcher trainingFetcher; // TrainingFetcher �ν��Ͻ� ����

    private string teacherAudioFolderPath = "Game_PT/Resources/Mp3/"; // ������ �Ҹ� ���
    private string childAudioBasePath;    // �Ƶ� �Ҹ� �⺻ ���
    private string sessionFolderName;     // ���� ���� ���� �̸� (e.g., Session_9)

    void Start()
    {// Unity ��Ÿ�ӿ��� persistentDataPath �ʱ�ȭ
        childAudioBasePath = Path.Combine(Application.persistentDataPath, "AudioFiles");

        // PlayerPrefs���� �ùٸ� ���� ��ȣ ��������
        int sessionNumber = PlayerPrefs.GetInt("sessionNumber", -1);
        if (sessionNumber == -1)
        {
            Debug.LogError("���� ��ȣ�� ������ �� �����ϴ�. ����Ʈ ���� -1�� ��ȯ�Ǿ����ϴ�.");
        }

        sessionFolderName = $"Session_{sessionNumber}";
        Debug.Log($"���� ���� ��ȣ: {sessionNumber}");
        Debug.Log($"���� ���� ���: {Path.Combine(childAudioBasePath, sessionFolderName)}");

        trainingFetcher = TrainingFetcher.instance;

        if (trainingFetcher != null)
        {
            UpdateWordUI(); // ù ��° �ܾ�� UI ������Ʈ
        }
        else
        {
            Debug.LogError("TrainingFetcher instance is null.");
        }

        nextButton.onClick.AddListener(GoToNextWord);
        previousButton.onClick.AddListener(GoToPreviousWord);
        playTeacherVoiceButton.onClick.AddListener(PlayTeacherVoice); // ������ �Ҹ� ���
        playChildVoiceButton.onClick.AddListener(PlayChildVoice);    // �Ƶ� �Ҹ� ���
    }

    // UI�� ���� �ܾ� �ε����� ���� ������Ʈ
    private void UpdateWordUI()
    {
        if (trainingFetcher != null && currentWordIndex < trainingFetcher.GetWords().Length)
        {
            wordText.text = trainingFetcher.GetWords()[currentWordIndex];
            childWordText.text = trainingFetcher.GetChildWords()[currentWordIndex];

            UpdateGraph();

            feedbackText.text = trainingFetcher.GetFeedback()[currentWordIndex];
            previousButton.gameObject.SetActive(currentWordIndex > 0);
            nextButton.gameObject.SetActive(currentWordIndex < trainingFetcher.GetWords().Length - 1);
        }
    }

    public void UpdateGraph()
    {
        float accuracy = trainingFetcher.GetScores()[currentWordIndex];

        if (accuracy > 0)
        {
            accuracyBar.fillAmount = Mathf.Clamp01(accuracy / 100f); // 100.0 �������� ����ȭ
            accuracyText.text = accuracy.ToString("F1") + "%"; // �Ҽ��� 1�ڸ����� ǥ��
        }
        else
        {
            accuracyBar.fillAmount = 0f;
            accuracyText.text = "������ ����";
            Debug.LogWarning("��Ȯ�� �����Ͱ� ��ȿ���� �ʽ��ϴ�.");
        }
    }

    public void GoToNextWord()
    {
        if (currentWordIndex < trainingFetcher.GetWords().Length - 1)
        {
            currentWordIndex++;
            UpdateWordUI();
        }
    }

    public void GoToPreviousWord()
    {
        if (currentWordIndex > 0)
        {
            currentWordIndex--;
            UpdateWordUI();
        }
    }

    // �Ʒ� �ܾ �´� mp3 ���� ��� (������ �Ҹ�)
    private void PlayTeacherVoice()
    {
        string currentWord = trainingFetcher.GetWords()[currentWordIndex];
        string audioFilePath = Path.Combine(Application.dataPath, teacherAudioFolderPath, $"{currentWord}.mp3");

        if (!File.Exists(audioFilePath))
        {
            Debug.LogError($"����� ������ �������� �ʽ��ϴ�: {audioFilePath}");
            return;
        }

        StartCoroutine(LoadAudioClipFromPath(audioFilePath));
    }

    private void PlayChildVoice()
    {
        string currentWord = trainingFetcher.GetWords()[currentWordIndex];
        int trainingOrder = currentWordIndex + 1; // �Ʒ� ���� (1���� ����)
        string childAudioFilePath = Path.Combine(childAudioBasePath, sessionFolderName, $"recordedAudio_{currentWord}_{trainingOrder}.wav");

        Debug.Log($"����Ϸ��� ���� ���: {childAudioFilePath}");

        if (!File.Exists(childAudioFilePath))
        {
            Debug.LogError($"�Ƶ� �Ҹ� ������ �������� �ʽ��ϴ�: {childAudioFilePath}");
            return;
        }

        StartCoroutine(LoadAudioClipFromPath(childAudioFilePath));
    }


    private System.Collections.IEnumerator LoadAudioClipFromPath(string path)
    {
        using (var www = new WWW("file://" + path))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                audioSource.clip = www.GetAudioClip(false, true);
                audioSource.Play();
                Debug.Log("����� ���: " + path);
            }
            else
            {
                Debug.LogError("����� �ε� ����: " + www.error);
            }
        }
    }
}
