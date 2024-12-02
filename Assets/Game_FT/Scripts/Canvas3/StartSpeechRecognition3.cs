using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections;
using System;
using System.IO;
using UnityEngine.SceneManagement;


public class StartSpeechRecognition3: MonoBehaviour
{
    public SpeechRecognitionManager3 speechManager;
    public Button startButton;
    public SpeechSynthesisManager ttsManager; // SpeechSynthesisManager ���� �߰�
    public PronunciationEvaluationManager pronunciationEvaluationManager;

    private bool isEvaluatingPronunciation = false; // ���� �� �� ����
    private bool isSceneTransitioning = false; // ��� ��ȯ �� ����
    private AudioClip recordedClip;
    private string microphoneDevice;

    void Start()
    {
        // ��ư Ŭ�� �� ���� �ν� ���� �Լ� ȣ��
        startButton.onClick.AddListener(() => StartRecognitionWithoutTimeout().ConfigureAwait(false));
    }
    public void OnSpeechButtonPressed()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
            StartRecording(); // ���� ����

            // ���� �ν��� ���� SpeechRecognitionManager�� �޼��带 ȣ��
            StartRecognitionAndProcess(); // ����� �޼��� ȣ��
        }
        else
        {
            Debug.LogError("����ũ ��ġ�� �������� �ʾҽ��ϴ�.");
        }
    }
    private void StartRecording()
    {
        recordedClip = Microphone.Start(microphoneDevice, false, 15, 16000); // �ִ� 15�� ����
        Debug.Log("������ ���۵Ǿ����ϴ�.");
    }

    private void StopRecording()
    {
        if (Microphone.IsRecording(microphoneDevice))
        {
            Microphone.End(microphoneDevice);
            Debug.Log("������ ����Ǿ����ϴ�.");

            // ������ ������ �����մϴ�.
            string filePath = SaveRecording(); // ���� ������ �����ϰ� ���� ��θ� ��ȯ����

            // ���� ���� DialogueManager�� ������ ���� ��� ����
            var dialogueManager = FindCurrentDialogueManager();
            if (dialogueManager != null)
            {
                dialogueManager.SetRecordedFilePath(filePath);
            }
            else
            {
                Debug.LogError("DialogueManager�� ã�� �� �����ϴ�.");
            }
        }
    }
    private BaseDialogueManager FindCurrentDialogueManager()
    {
        // ���� ���� ���� DialogueManager�� �������� �����մϴ�.
        if (SceneManager.GetActiveScene().name == "Scene01_SchoolOutside")
            return FindObjectOfType<DialogueManager>(); // ���÷� �� ���� DialogueManager Ŭ������ �ٸ��� ����
        else if (SceneManager.GetActiveScene().name == "Scene02_Board")
            return FindObjectOfType<DialogueManager2>();
        else if (SceneManager.GetActiveScene().name == "Scene03_Gym")
            return FindObjectOfType<DialogueManager3>();
        else if (SceneManager.GetActiveScene().name == "Scene04_Library")
            return FindObjectOfType<DialogueManager4>();
        else
            return null; // �ش� ���� �´� DialogueManager�� ���� ���
    }
    private string SaveRecording()
    {
        // ������ ���� �̸� ���� (UUID ��� �Ǵ� ��¥/�ð� ���)
        string fileName = $"recordedAudio_{System.DateTime.Now:yyyyMMdd_HHmmss}.wav";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        if (recordedClip == null)
        {
            Debug.LogError("������ ����� Ŭ���� �����ϴ�.");
            return string.Empty;
        }

        // ������ ������� WAV ���Ϸ� �����ϴ� ����
        var samples = new float[recordedClip.samples];
        recordedClip.GetData(samples, 0);

        byte[] wavFile = WavUtility.FromAudioClip(recordedClip); // WAV ��ƿ��Ƽ�� ����Ͽ� ��ȯ
        File.WriteAllBytes(filePath, wavFile);
        Debug.Log($"������ ������ ����Ǿ����ϴ�: {filePath}");

        return filePath; // ����� ���� ��� ��ȯ
    }

    // ���� �ν��� �����ϴ� �Լ�, TTS �߿��� ���� �ν��� �������� ����
    public async Task StartRecognitionWithoutTimeout()
    {
        // TTS�� ���� ���̸� ���� ��ư�� ����
        if (ttsManager.isSpeaking || isEvaluatingPronunciation || isSceneTransitioning)
        {
            Debug.Log("���� ��ư�� ��Ȱ��ȭ�Ǿ����ϴ�.");
            return;
        }

        Debug.Log("���� �ν� ��ư�� Ŭ���Ǿ����ϴ�.");
        StartRecording(); // ���� ����
        await StartRecognitionAndProcess(); // �񵿱� �۾��� ��ٸ��ϴ�
    }

    // ���� �߰��� �񵿱� ���� �ν� �� ���� �� �޼���
    private async Task StartRecognitionAndProcess()
    {
        try
        {
            await speechManager?.StartRecognition();  // ���� �ν� ���� �� ���

            StopRecording();

            // ���� �� ����: ����� ���� ��ο� ���� �ؽ�Ʈ ����
            string userResponse = speechManager.GetRecognizedText();  // ���� �ν� ��� ��������

            if (string.IsNullOrEmpty(userResponse))
            {
                Debug.LogError("���� �ν� ����� ��� �ֽ��ϴ�.");
                return;
            }

            Debug.Log($"�νĵ� �ؽ�Ʈ: {userResponse}");

            string expectedText = GetExpectedTextFromDialogueManager(userResponse);
            if (string.IsNullOrEmpty(expectedText))
            {
                Debug.LogError("���� �ؽ�Ʈ�� ��� �ֽ��ϴ�.");
                return;
            }

            string audioFilePath = SaveRecording();

            if (!IsFileReady(audioFilePath))
            {
                Debug.LogError("����� ���� ��ΰ� �ùٸ��� �ʰų� ������ �������� �ʽ��ϴ�.");
                return;
            }

            StartPronunciationEvaluation(audioFilePath, expectedText);
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� �ν� ó�� �� ���� �߻�: {ex.Message}");
        }
    }

    private bool IsFileReady(string filePath)
    {
        int maxAttempts = 10;
        int attempt = 0;
        while (!File.Exists(filePath) && attempt < maxAttempts)
        {
            System.Threading.Thread.Sleep(100); // 100ms ����
            attempt++;
        }
        return File.Exists(filePath);
    }
    // ���� �ؽ�Ʈ�� �������� �޼���
    private string GetExpectedTextFromDialogueManager(string userResponse)
    {
        var dialogueManager = FindCurrentDialogueManager();
        if (dialogueManager != null)
        {
            // ���� ��ȭ���� ����Ǵ� �ؽ�Ʈ �������� (������� ��Ȳ�� �°� �޼��� ���� �ʿ�)
            return dialogueManager.GetExpectedResponse(userResponse);
        }
        else
        {
            Debug.LogError("DialogueManager�� ã�� �� �����ϴ�.");
            return string.Empty;
        }
    }


    // ���� �򰡰� ���۵� �� ȣ���ϴ� �Լ�
    public void StartPronunciationEvaluation(string audioFilePath, string expectedText)
    {
        isEvaluatingPronunciation = true;

        // ���� �򰡸� ���� �� ���� �Ű����� (����� ���� ��ο� ���� �ؽ�Ʈ) ����
        pronunciationEvaluationManager.EvaluatePronunciation(audioFilePath, expectedText);
    }


    // ���� �򰡰� ����� �� ȣ���ϴ� �Լ�
    public void EndPronunciationEvaluation()
    {
        isEvaluatingPronunciation = false;
    }

    // ��� ��ȯ�� ���۵� �� ȣ���ϴ� �Լ�
    public void StartSceneTransition()
    {
        isSceneTransitioning = true;
    }

    // ��� ��ȯ�� �Ϸ�� �� ȣ���ϴ� �Լ�
    public void EndSceneTransition()
    {
        isSceneTransitioning = false;
    }
}