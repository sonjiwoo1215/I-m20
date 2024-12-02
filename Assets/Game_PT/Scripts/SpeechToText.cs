using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class SpeechToText : MonoBehaviour
{
    public Button startButton;  // ���� �ν��� �����ϴ� ��ư
    public Button playRecordingButtonPanelToActivate; // panelToActivate�� �ִ� PlayRecordingButton
    public Button playRecordingButtonFinalFailPanel; // finalFailPanel�� �ִ� PlayRecordingButton

    private bool isPanelToActivateButtonClicked = false; // panelToActivate ��ư Ŭ�� ���� ����
    private bool isFinalFailPanelButtonClicked = false;  // finalFailPanel ��ư Ŭ�� ���� ����
    public static SpeechToText Instance;

    public Image playRecordingButtonImagePanelToActivate; // panelToActivate ��ư�� �̹���
    public Image playRecordingButtonImageFinalFailPanel;  // finalFailPanel ��ư�� �̹���
    private Vector3 originalScalePanelToActivate;
    private Vector3 originalScaleFinalFailPanel;

    public Button stopRecordButton; // StopRecordButton �߰�
    public Button nextButton;
    //public Text resultText;     // �νĵ� �ؽ�Ʈ�� ǥ���ϴ� UI �ؽ�Ʈ
    //public Text assessmentText; // ���� �� ����� ǥ���ϴ� UI �ؽ�Ʈ

    public GameObject panel1;
    public GameObject panel2;  // Ư�� UI �г� (panel2)
    public GameObject panel3;  // Ư�� UI �г� (panel3)
    public GameObject panelToActivate;  // Ư�� �̺�Ʈ �� Ȱ��ȭ�� �г�
    public GameObject againPanel; // 3�� ���� ������ �� Ȱ��ȭ�� �г�
    public GameObject finalFailPanel; //3��° ������ �� Ȱ��ȭ�� �г�
    public GameObject image1;
    public GameObject defaultImage;  // �⺻ �̹���
    public GameObject recordImage;   // ���� �� �̹���
    public GameObject successImage;  // �ܾ� ���� �̹���
    public GameObject failImage;     // �ܾ� ���� �̹���

    public GameObject text1;     // PanelToActivate ���� text1
    public GameObject text2;      // PanelToActivate ���� text2
    public GameObject ftext1;     // FinalFailPanel ���� ftext1
    public GameObject ftext2;     // FinalFailPanel ���� ftext2
    public GameObject text3;     // 3��° �ܾ��� �� Ȱ��ȭ�� text3
    public GameObject ftext3;
    public ParticleSystem confettiParticle;  // ��ƼŬ �ý��� ����
    private int currentWordIndex = 0; // ���� �ܾ��� �ε����� �����ϱ� ���� ����
    private BGMController bgmController;  // BGMController ����

    private SpeechRecognizer recognizer;  // ���� �νı⸦ ���� ����
    private string subscriptionKey = "1ZpqHQla3FjGYigKSPQIdYfK0e3r34BgRM0I6p2oVMVVzrrm4vroJQQJ99AKACNns7RXJ3w3AAAYACOGeY1y"; // Azure ���� Ű (���� Ű�� ��ü�ؾ� ��)
    private string region = "koreacentral"; // Azure ���� (���� �������� ��ü�ؾ� ��)
    private string language = "ko-KR";     // ���� �ν� ��� (�ѱ���)
    private SpeechConfig config;           // ���� �ν� ���� ����


    private SynchronizationContext unityContext; // Unity ���� �����忡�� UI ������Ʈ�� ���� ���ؽ�Ʈ

    //private bool stopButtonClicked = false;  // ���� ��ư�� Ŭ���Ǿ����� ���θ� ����
    private int attemptCount = 0;  // ���� �õ� Ƚ��
    private string audioFilePath;  // ���� ���� ��� ����
    private AudioClip recordedClip; // ������ ����� Ŭ��
    private string microphoneName = null; // �⺻ ����ũ ��ġ �̸�
    private bool isRecording = false;
    //private int currentRecordingIndex = 0;  // ������ �ε���
    //private string resourcesPath = "Resources/Audio/";
    private string sessionFolderPath;
    private int recordingCount = 1;  // ���� Ƚ���� �����ϱ� ���� ����
    private SpeechRecognitionResult recognizedResult;  // �νĵ� ��� ���� ����
    private string recognizedText;  // �νĵ� �ؽ�Ʈ�� �����ϴ� ����

    // recognizedText�� ������ ����Ʈ �߰�
    List<string> recognizedTexts = new List<string>(); // �� ����Ʈ�� �ʱ�ȭ
    List<string> videoPaths = new List<string>();
    private List<string> selectedWords;
    private List<AudioClip> audioClips = new List<AudioClip>(); // ������ AudioClip ����Ʈ
    List<float> accuracyScores = new List<float>(); // accuracyScore�� ������ ����Ʈ
    private List<string> feedbacks = new List<string>();
    void Start()
    {
        bgmController = FindObjectOfType<BGMController>();  // ������ BGMController ã��
        // Unity�� SynchronizationContext�� ĸó�Ͽ� ���� �����忡�� UI�� ������Ʈ�� �� �ֵ��� ��
        unityContext = SynchronizationContext.Current;

        // SpeechConfig �ʱ�ȭ: ���� Ű�� ������ ����Ͽ� ����
        config = SpeechConfig.FromSubscription(subscriptionKey, region);
        config.SpeechRecognitionLanguage = language; // ���� �ν� ��� ����

        // startButton�� Ŭ�� �̺�Ʈ ������ �߰�
        startButton.onClick.AddListener(() =>
        {
            // successImage �Ǵ� recordImage�� Ȱ��ȭ�Ǿ� �ִ� ���, �ƹ� ���۵� ���� ����
            if (recordImage.activeSelf || successImage.activeSelf || failImage.activeSelf)
            {
                Debug.Log("���� ���¿����� startButton�� ������ �������� ����.");
                return; // Ŭ���ص� �ƹ� ���� �� ��
            }

            // �� ������ ����ϸ� ���� �ν� ����
            StartRecognition(); // ���� �ν� �� ���� ����
        });
        // stopRecordButton�� Ŭ�� �̺�Ʈ������ �߰�
        stopRecordButton.onClick.AddListener(() =>
        {
            // ������ �����ϴ� �Լ� ȣ��
            StopRecording();
        });

        // ��ư �̹����� �ʱ� ũ�⸦ ����
        originalScalePanelToActivate = playRecordingButtonPanelToActivate.transform.localScale;
        originalScaleFinalFailPanel = playRecordingButtonFinalFailPanel.transform.localScale;

        // PlayRecordingButton Ŭ�� �� ����� ��� �� �̹��� �ִϸ��̼� ����
        playRecordingButtonPanelToActivate.onClick.AddListener(() =>
        {
            if (!isPanelToActivateButtonClicked) // ��ư�� �� ���� Ŭ������ �ʾ��� ���� ����
            {
                PlayRecording(playRecordingButtonImagePanelToActivate);  // �̹��� �ִϸ��̼� �߰�
                text1.SetActive(true);
                text2.SetActive(false);
                isPanelToActivateButtonClicked = true; // ��ư Ŭ�� �÷��� ����
            }
            else
            {
                Debug.Log("playRecordingButtonPanelToActivate�� �� ���� ���� �� �ֽ��ϴ�.");
            }
        });

        playRecordingButtonFinalFailPanel.onClick.AddListener(() =>
        {
            if (!isFinalFailPanelButtonClicked) // ��ư�� �� ���� Ŭ������ �ʾ��� ���� ����
            {
                PlayRecording(playRecordingButtonImageFinalFailPanel);  // �̹��� �ִϸ��̼� �߰�
                ftext1.SetActive(true);
                ftext2.SetActive(false);
                isFinalFailPanelButtonClicked = true; // ��ư Ŭ�� �÷��� ����
            }
            else
            {
                Debug.Log("playRecordingButtonFinalFailPanel�� �� ���� ���� �� �ֽ��ϴ�.");
            }
        });
        // �г� �ʱ�ȭ �� �ʱ�ȭ �Լ��� ��ư �÷��� ���� ����
        ResetPanelToActivate();
        ResetFinalFailPanel();
        stopRecordButton.onClick.AddListener(StopRecording);  // StopRecordButton �ڵ� Ŭ�� ����

        // ��� ������ ����ũ ��ġ �̸� ����
        if (Microphone.devices.Length > 0)
        {
            microphoneName = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("����ũ�� ã�� �� �����ϴ�.");
        }

        // ���ο� ���� ������ ���۵� ��, ������ ���� ������ ����
        CreateNewSessionFolder();
        // �ʱ� ��Ȱ��ȭ
        text2.SetActive(false);
        ftext2.SetActive(false);
        text3.SetActive(false);
        ftext3.SetActive(false);
        startButton.gameObject.SetActive(false);
        finalFailPanel.gameObject.SetActive(false);
        againPanel.gameObject.SetActive(false);
        panelToActivate.SetActive(false);
        image1.SetActive(false);
        recordImage.SetActive(false);
        successImage.SetActive(false);
        failImage.SetActive(false);
        stopRecordButton.gameObject.SetActive(false);  // �ʱ⿡�� ��Ȱ��ȭ
        confettiParticle.Stop();
    }
    // ���ο� ���� ���� ���� �޼���
    void CreateNewSessionFolder()
    {
        string baseFolder = Path.Combine(Application.persistentDataPath, "AudioFiles");

        if (!Directory.Exists(baseFolder))
        {
            Directory.CreateDirectory(baseFolder);
        }

        // ������ ���� ���� �̸� ���� (Session_1, Session_2, ...)
        int sessionNumber = 1;
        do
        {
            sessionFolderPath = Path.Combine(baseFolder, $"Session_{sessionNumber}");
            sessionNumber++;
        } while (Directory.Exists(sessionFolderPath));

        // ���� ���� ����
        Directory.CreateDirectory(sessionFolderPath);

        // PlayerPrefs�� ���� �ѹ� ����
        PlayerPrefs.SetInt("sessionNumber", sessionNumber - 1);
        PlayerPrefs.Save(); // ���� ���� ����

        Debug.Log($"���ο� ���� ���� ������: {sessionFolderPath}");
        Debug.Log($"����� ���� �ѹ�: {PlayerPrefs.GetInt("sessionNumber")}");
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensures only one instance exists
        }

    }
    /// �����νİ� �������� ���� �޼���
    async void StartRecognition()
    {
        if (recognizer != null)
        {
            // �̹� recognizer�� ���� ���̸� ���� �����ϰ� ���ҽ��� ����
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            recognizer.Recognized -= RecognizedHandler;
            recognizer.Canceled -= CanceledHandler;
            recognizer.Dispose();
            recognizer = null;
        }

        // ���� ���� �̹����� ��ȯ
        successImage.SetActive(false);
        defaultImage.SetActive(false);
        recordImage.SetActive(true);

        // ���� �ν� ����
        var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        recognizer = new SpeechRecognizer(config, audioConfig);
        recognizer.Recognized += RecognizedHandler;
        recognizer.Canceled += CanceledHandler;

        // ���� ����
        StartRecording(); // ���⼭ ���� ����
        stopRecordButton.gameObject.SetActive(true);  // StopRecordButton Ȱ��ȭ
        Debug.Log("�����ν� ����");
        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
    }

    // ���� ���� �޼���
    public void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("������ ����ũ ��ġ�� �����ϴ�.");
            return;
        }

        if (isRecording || Microphone.IsRecording(null))
        {
            Debug.LogWarning("�̹� ���� ���Դϴ�.");
            return;
        }

        isRecording = true;
        recordedClip = Microphone.Start(null, false, 3, 44100);
        if (Microphone.IsRecording(null))
        {
            Debug.Log("���� ��...");
        }
        else
        {
            Debug.LogError("������ ���۵��� �ʾҽ��ϴ�.");
        }
    }


    /// �����νİ���� ó���ϴ� �̺�Ʈ �ڵ鷯
    private async void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        // �����ν��� ������ ���
        if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {
            // ����ڰ� ������ ���� ������ �ν�
            recognizedText = e.Result.Text.TrimEnd('.');
            Debug.Log($"���� �ν� ����! �ν� ���: '{recognizedText}'");

            // ���� �ܾ��� �ε����� ������Ʈ
            currentWordIndex = WordGameController.Instance.GetCurrentWordIndex(); // �ܾ� �ε��� ��������

            // Step 1: ���� �ν� ����� ��ǥ �ܾ� ��
            string targetWord = WordGameController.Instance.GetCurrentWord(); // ��ǥ �ܾ�
            Debug.Log($"��ǥ �ܾ�: '{targetWord}', �νĵ� �ؽ�Ʈ: '{recognizedText}'");



            // �νĵ� �ؽ�Ʈ�� ���� ������ �ܾ�� ��ġ�ϴ��� Ȯ��
            if (recognizedText.Equals(targetWord, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("�ܾ� ��ġ");

                StopRecognition();

                unityContext.Post(_ =>
                {
                    defaultImage.gameObject.SetActive(false);
                    recordImage.gameObject.SetActive(false);
                    successImage.gameObject.SetActive(true);
                }, null);


                // 1�� ��� �� Ȱ��ȭ,��Ȱ��ȭ
                await Task.Delay(1500);
                unityContext.Post(_ =>
                {
                    panel1.gameObject.SetActive(false);
                    panel2.gameObject.SetActive(false);          // panel2 ��Ȱ��ȭ
                    panel3.gameObject.SetActive(false);          // panel3 ��Ȱ��ȭ
                    startButton.gameObject.SetActive(false);

                    // �г��� Ȱ��ȭ�ϱ� ���� ���� �ʱ�ȭ
                    ResetPanelToActivate();  // text1�� Ȱ��ȭ�ϰ� text2�� ��Ȱ��ȭ

                    panelToActivate.gameObject.SetActive(true);  // panelToActivate Ȱ��ȭ
                    WordGameController.Instance.IncrementSlider(); // �����̴� ������Ʈ
                    defaultImage.gameObject.SetActive(true);
                    successImage.gameObject.SetActive(false);
                }, null);


                // ���� �� ����
                //PerformPronunciationAssessment(audioFilePath);

            }
            else // �νĵ� �ؽ�Ʈ�� ���õ� �ܾ ��ġ���� ������
            {
                if (attemptCount < 2)
                {
                    //�õ� Ƚ�� ����
                    attemptCount++;
                    Debug.Log($"�ܾ ��ġ���� ����. �õ� Ƚ��: {attemptCount}/3");
                    StopRecognition(); // �����νİ� ���� �ߴ�

                    //���� �̹����� ��ȯ
                    unityContext.Post(_ =>
                    {
                        defaultImage.gameObject.SetActive(false);
                        recordImage.gameObject.SetActive(false);
                        failImage.gameObject.SetActive(true);
                    }, null);

                    // 1�� ��� �� �ٽ� �غ��ڴ� �г� Ȱ��ȭ
                    await Task.Delay(1000);
                    unityContext.Post(_ =>
                    {
                        againPanel.SetActive(true);  // againpanel Ȱ��ȭ
                    }, null);

                    // 2.5�� ��� �� �г� ��Ȱ��ȭ�ϰ� �ٽ� ���� ����
                    await Task.Delay(2500);
                    unityContext.Post(_ =>
                    {
                        againPanel.SetActive(false);  // panelToActivate ��Ȱ��ȭ
                        defaultImage.gameObject.SetActive(true);
                        failImage.gameObject.SetActive(false);
                    }, null);
                }


                //if (attemptCount == 3) //3��° Ʋ���� ��
                else if (attemptCount == 2)
                {
                    //�õ� Ƚ�� ����
                    attemptCount++;

                    Debug.Log("3��° �õ� ����");

                    StopRecognition();

                    //���� �̹����� ��ȯ
                    unityContext.Post(_ =>
                    {
                        defaultImage.gameObject.SetActive(false);
                        recordImage.gameObject.SetActive(false);
                        successImage.gameObject.SetActive(false);
                        failImage.gameObject.SetActive(true);
                    }, null);



                    // 1�� ��� �� Ȱ��ȭ,��Ȱ��ȭ
                    await Task.Delay(1500);
                    unityContext.Post(_ =>
                    {
                        panel1.gameObject.SetActive(false);
                        panel2.gameObject.SetActive(false);          // panel2 ��Ȱ��ȭ
                        panel3.gameObject.SetActive(false);          // panel3 ��Ȱ��ȭ
                        startButton.gameObject.SetActive(false);

                        // �г��� Ȱ��ȭ�ϱ� ���� ���� �ʱ�ȭ
                        ResetFinalFailPanel();  // ftext1�� Ȱ��ȭ�ϰ� ftext2�� ��Ȱ��ȭ

                        finalFailPanel.gameObject.SetActive(true);  // panelToActivate Ȱ��ȭ

                        WordGameController.Instance.IncrementSlider(); // �����̴� ������Ʈ
                        defaultImage.gameObject.SetActive(true);
                        successImage.gameObject.SetActive(false);
                        failImage.gameObject.SetActive(false);
                        recordImage.gameObject.SetActive(false);
                    }, null);

                    // ���� �� ����
                    //PerformPronunciationAssessment(audioFilePath);
                }

            }


        }
        else
        {
            Debug.LogWarning($"�����ν� ���� ����: {e.Result.Reason}");
        }
    }

    private async void PerformPronunciationAssessment(string audioFilePath, string targetWord, string recognizedText)
    {
        var pronunciationConfig = new PronunciationAssessmentConfig(
            targetWord,  // ��ǥ �ܾ� ����
            GradingSystem.HundredMark,
            Granularity.Phoneme,  // ���� ���� �򰡸� Ȱ��ȭ
            true);

        var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);  // ������ ������ �Է����� ���

        using (var recognizer = new SpeechRecognizer(config, audioConfig))
        {
            pronunciationConfig.ApplyTo(recognizer);  // ���� �� ������ ����

            var result = await recognizer.RecognizeOnceAsync();  // ������ ���Ϸ� ���� �� ����

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                var pronunciationResult = PronunciationAssessmentResult.FromResult(result);
                if (pronunciationResult != null)
                {
                    // 1. �ܾ� ��ü�� ���� ��Ȯ�� ���� ���
                    var accuracyScore = pronunciationResult.AccuracyScore;
                    accuracyScores.Add((float)accuracyScore);  // ���� �� ������ accuracyScores ����Ʈ�� �߰�
                    Debug.Log($"���� �� - �ܾ� ��ü ��Ȯ�� ����: {accuracyScore}");

                    // 2. ���� ������ �򰡵� ��� ���
                    List<string> targetPhonemes = KoreanPhonemeSplitter.SplitIntoPhonemes(targetWord); // ��ǥ �ܾ ���ҷ� �и�
                    List<string> recognizedPhonemes = KoreanPhonemeSplitter.SplitIntoPhonemes(recognizedText); // �νĵ� �ܾ ���ҷ� �и�
                    float lowestPhonemeScore = 100.0f;  // ���� ���� ������ ������ ����
                    string lowestPhoneme = "";  // ���� ���� ������ ���� ����
                    string feedback = "";  // �ǵ���� ������ ����
                    string lowestPhonemePosition = "";  // �ش� ������ ��ġ ���� (�ʼ�, �߼�, ����)
                    int lowestPhonemeIndex = -1;  // ���� ���� ������ ���� ������ �ε���

                    // 3. ���� ������ �� �� �ǵ�� ����
                    int phonemeIndex = 0;
                    foreach (var wordDetail in pronunciationResult.Words)
                    {
                        foreach (var phoneme in wordDetail.Phonemes)
                        {
                            if (phonemeIndex < targetPhonemes.Count)
                            {
                                string targetPhoneme = targetPhonemes[phonemeIndex];
                                string recognizedPhoneme = (phonemeIndex < recognizedPhonemes.Count) ? recognizedPhonemes[phonemeIndex] : "����";  // �νĵ� ���Ұ� ������ "����"
                                string syllable;  // out �Ű������� ���� ���� ����
                                string phonemePosition = KoreanPhonemeSplitter.GetPhonemePosition(targetWord, phonemeIndex, out syllable);

                                Debug.Log($"����: {targetPhoneme} / �񱳵� ����: {recognizedPhoneme} / ��Ȯ��: {phoneme.AccuracyScore} / ��ġ: {phonemePosition}");

                                // ���� ���� ������ ���� ã��
                                if (phoneme.AccuracyScore < lowestPhonemeScore)
                                {
                                    lowestPhonemeScore = (float)phoneme.AccuracyScore;
                                    lowestPhoneme = targetPhoneme;
                                    lowestPhonemePosition = phonemePosition;
                                    lowestPhonemeIndex = phonemeIndex;
                                }

                                phonemeIndex++;
                            }
                        }
                    }

                    // 4. ���� ���� ���Ұ� ���� ������ ã��, �ʼ�, �߼�, ���� ��ġ�� ã�� �ǵ�鿡 ����
                    if (lowestPhonemeIndex >= 0)
                    {
                        string syllable = "";  // ���Ұ� ���� ������ ������ ����
                        string phonemePosition = KoreanPhonemeSplitter.GetPhonemePosition(targetWord, lowestPhonemeIndex, out syllable); // ���� ��ġ�� ���� ã��

                        // �ǵ�� ����
                        feedback = $"\r\n<b><size=42><color=#8388FF>'{targetWord}'���� {syllable}�� {phonemePosition} {lowestPhoneme}�� ������ �����մϴ�.</color></size></b>";

                        // 5. ���� ���� ���ҿ� ���� �ǵ�� ����
                        switch (lowestPhoneme)
                        {
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b> \r\n���� ���� �Ŀ�������, ���밡 �������� �ʰ� ���� �޺κ�(������)�� ��õ�� �����̿� ��� �Ҹ��� ���ϴ�. ���Ⱑ ���ܵǾ��ٰ� Ǯ���鼭 �Ҹ��� ���� �ϸ�, ���� ���� ������ ������ �ε巴�� �Ҹ��� �̾������� �մϴ�. \r\n\r\n <b><size=40><������ �� ���� ����� ������ ���� ����> -</size></b> \r\n���� ��õ�忡 �ʹ� ���ϰ� ��� �Ҹ��� �����ϰų� ���� �� �ֽ��ϴ�. �̷� ���� ���� �� ��� ������ ������ �Ͽ� ���� ������ Ǯ��, �Ҹ��� �ڿ������� �̾������� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -</size></b> \r\n�� ���� ��� ������ �����ų�, �ſ��� ����� ���� ��ġ�� �ð������� Ȯ���� �� �ְ� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -</size></b> \r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��ؼ� ����, ���� ��ġ�� ������ �����ϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� ������ �ݺ��� �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b> \r\n���� ��������, ���밡 �����ϸ� ������ ���� ����(ġ��)�� ��� �Ͽ� �Ҹ��� ���ϴ�. ���Ⱑ �ڸ� ���� ��Ȱ�ϰ� �������� �ؾ� �ϸ�, ���� �������� �ڿ��������� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -</size></b> \r\n���� �ʹ� ���ϰ� ��ų� ���Ⱑ �ڸ� ���� �� ���������� ������ �Ҹ��� �����ų� �帴�ϰ� �鸱 �� �ֽ��ϴ�. �ڸ� ���� ������ ���� �Ͽ� ���� �� ������ �帧�� ������ �ϰ�, ���� ��ġ�� ������ �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -</size></b>\r\n�ڸ� ���� �����ϸ鼭 �� �ȿ��� ���Ⱑ �������� ������ ���������� �ϰ�, �ſ��� ����� ���� ��ġ�� �ð������� Ȯ���ϰ� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -</size></b> \r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� ������ �ùٸ� ��ġ�� ��� �ִ��� �������ϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� ª�� �Ҹ��� �ݺ��ϸ� ������ �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b> \r\n���� ���� �Ŀ�������, ���밡 �������� �ʰ� ������ ���� ����(ġ��)�� ������ ��� ���鼭 �Ҹ��� ���ϴ�. ���Ⱑ ���ܵǾ��ٰ� �������� ���;� �ϸ�, ���� �������� �ε巴�� �����մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b>\r\n���� �ʹ� ���� ��� �Ҹ��� �����ϰ� �鸮�ų�, ���� �� �������� �ʾ� �Ҹ��� �帴�� �� �ֽ��ϴ�. ���� ������ Ǯ��, ���Ⱑ �ڿ������� �귯�������� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b>\r\n�ſ��� ���鼭 ���� �������� �����ϰ�, ������ �� �տ��� ������ ������ �帧�� ���������� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b>\r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��ϸ� ���� ��ġ�� ���� �帧�� �����ϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� ������ �Ҹ��� ������ �ε巴�� �����ϵ��� �մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ġ�� ����������, ������ ���� �ٷ� ���ʿ� ��� ���� ���� �Ҹ��� ���ϴ�. ���� �ε巴�� ���� �ʰ� ��Ȯ�ϰ� ��ƾ� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b>\r\n���� �ʹ� ���� ���ų�, ������ �����ؼ� �Ҹ��� �Ҹ�Ȯ�� �� �ֽ��ϴ�. ���� ���� ������ ���� �ʵ��� �ϸ�, �Ҹ��� �ڿ������� �̾������� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b>\r\n������ ���� �ڿ� ��� ������ ���������� �ϰ�, �ſ��� ����� ���� ��ġ�� Ȯ���� �� �ְ� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -</size></b> \r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��ϸ鼭 ���� �ùٸ� ��ġ�� �ִ��� Ȯ���մϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� �Ҹ��� �ݺ��մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ��������, ���밡 �����ϸ� �Լ��� �ݰ� �ڷ� ���⸦ �������鼭 �Ҹ��� ���ϴ�. �ڸ� ���� ���� �帧�� ��Ȱ�ؾ� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -</size></b> \r\n�Լ��� �ʹ� �� �ݰų�, �ڸ� ���� ���Ⱑ �� ������ ������ �Ҹ��� ���ϰų� ���� �� �ֽ��ϴ�. �ڸ� ���� ������ ���� �Ͽ� ���� �帧�� ������ �ϰ�, �Լ��� ������ Ǯ���ݴϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -</size></b> \r\n�ڸ� ������ ���� ������ ���鼭 ���� �� ���� �帧�� �ν��ϰ� �ϰ�, �ڿ��� ������ ���������� Ȯ���� �ּ���.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -</size></b> \r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� �ڷ� ���Ⱑ �������� Ȯ���մϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� ª�� �Ҹ��� �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ���� �Ŀ�������, ���밡 �������� ������ �Լ��� ������ �ݾҴٰ� ���� �Ҹ��� ���ϴ�. ���Ⱑ ������ ������ ���� Ư¡�Դϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b>\r\n�Լ��� �ʹ� ���� �ݰų�, �Ҹ��� ���ϰ� �鸮�� ������ ����Ȯ���� �� �ֽ��ϴ�. �Լ��� ���� ������ ������ �ε巴�� �Ҹ��� �̾������� �մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b>\r\n�ſ��� ���� �Լ��� ������ ������ ����� Ȯ���ϰ�, ������ �� �տ��� ������ �帧�� ������ �� �ּ���.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -</size></b> \r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� �Լ��� �������� Ȯ���մϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� �Ҹ��� �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ���� ����������, ���밡 �������� ������ ������ �Ʒ��� ��ó�� ������ �ΰ� ���⸦ ��������鼭 �Ҹ��� ���ϴ�. ���Ⱑ �ε巴�� �帣���� �ؾ� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b>\r\n���� �ʹ� ������ ���аų�, ���Ⱑ ����ġ�� ���� ���� �� �ֽ��ϴ�. ���� ��ġ�� �����Ͽ� ���Ⱑ �ε巴�� �帣���� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -</size></b> \r\n�ſ��� ����� ���� ��ġ�� �ð������� Ȯ���ϰ�, ������ �� �տ��� ������ ������ �帧�� ���������� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -</size></b> \r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� ���� ��ġ�� ���� �帧�� �����մϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� �Ҹ��� �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ���� ����������, ���밡 �������� �ʰ� ��Ҹ������� �����ϸ�, ���� �Լ��� �������� �ּ�ȭ�ؾ� �մϴ�. ���Ⱑ �ڿ������� �񿡼� �������� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b>\r\n���� �Լ��� ����ġ�� �����̰ų� ��Ҹ��θ� �Ҹ��� ���� ���� ���� �ֽ��ϴ�. �̷� ���� ���� �� ��� ������ ������ �ϰ�, �Ҹ��� �񱸸ۿ��� ������ ���� �ν��ϰ� �մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -</size></b> \r\n���� �� ��� ������ ������ �Ҹ��� �ڿ������� �������� Ȯ���ϰ�, �ſ��� ���� ���� �Լ��� �������� �ʴ��� Ȯ���մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -</size></b> \r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� �񿡼� �Ҹ��� �ε巴�� �������� �������ϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� ������ �Ҹ��� �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ���� ����������, ���밡 �������� �ʰ� ������ ���� �ڿ� ������ ��� �����մϴ�. ���⸦ �ε巴�� ��������� �Ҹ��� ������ �ؾ� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b>\r\n�Ҹ��� �ʹ� ���� �����ų� ���� ��ġ�� ��Ȯ���� �ʾƼ� �Ҹ��� �Ҹ�Ȯ�� �� �ֽ��ϴ�. ���� ��ġ�� �ſ�� ���� �����ϰ�, �Ҹ��� �ε巴�� ������ �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b>\r\n�ſ��� ���� ���� ��ġ�� �ð������� Ȯ���ϰ�, ������ �� �տ��� ������ ������ �帧�� ���������� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b>\r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� ���� �ùٸ� ��ġ�� �ִ��� Ȯ���մϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� �Ҹ��� �ݺ��մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ���� ����������, ���밡 �������� ������ ������ ���� �ڿ� ������ ��� ���⸦ �Ͷ߸��� �Ҹ��� ���ϴ�. �Ҹ��� �ε巴�� ���;� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -</size></b> \r\n���� �ʹ� ���ϰ� ��� �Ҹ��� ��ĥ�� ���� �� �ֽ��ϴ�. ���� ���� ���� Ǯ�� �ε巴�� �����ϵ��� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -</size></b> \r\n�ſ��� ���� ���� ��ġ�� ���� �����ϰ�, ������ �� �տ��� ������ �帧�� ���������� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b>\r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� ������ ��ġ�� ������ �帧�� �������ϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� �Ҹ��� �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ���� �Ŀ�������, ���밡 �������� �ʰ� ���� �޺κ��� ��õ�忡 ��� �Ҹ��� ���ϴ�. ���⸦ ���ϰ� �Ͷ߸��� �����ؾ� �ϸ�, �Ҹ��� �ε巴�� ���;� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -</size></b> \r\n���� �ʹ� ���ϰ� ��� �Ҹ��� �����ϰų� ���� �� �ֽ��ϴ�. ���� ������ Ǯ��, ���Ⱑ �ڿ������� �귯�������� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b>\r\n���� �� ��� ������ ������ �Ҹ��� ��Ȱ�ϰ� �������� Ȯ���ϰ�, �ſ��� ���� ���� ��ġ�� �ð������� Ȯ���մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -</size></b> \r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� ���� �������� �������ϴ�.\r\n������ ���� ���� : \"ī\", \"��\" ���� �Ҹ��� �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ���� �Ŀ�������, ���밡 �������� �ʰ� ������ ���� ���ʿ� ������ ��� ���⸦ �Ͷ߸��� �Ҹ��� ���ϴ�. �Ҹ��� �ε巴�� �������� �ؾ� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b>\r\n���� �ʹ� ���ϰ� ��� �Ҹ��� �����ϰų� ���� �� �ֽ��ϴ�. ���� ������ Ǯ�� �ε巴�� �Ҹ��� ������ �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b>\r\n�ſ��� ���� ���� ��ġ�� �ð������� Ȯ���ϰ�, ������ ������ �帧�� ������ �� �ּ���.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b>\r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��ϸ� ���� �ùٸ� ��ġ�� �ִ��� Ȯ���մϴ�.\r\n������ ���� ���� : \"Ÿ\", \"��\" ���� �Ҹ��� �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ���� �Ŀ�������, ���밡 �������� �ʰ� �Լ��� �ݾҴٰ� ���鼭 �Ҹ��� ���ϴ�. ���Ⱑ �ڿ������� ������ �������� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -</size></b> \r\n�Լ��� �ʹ� ���� �ݰų� �Ҹ��� ���ϰ� ���� �� �ֽ��ϴ�. �Լ��� ������ Ǯ�� �ε巴�� �����ϵ��� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -</size></b> \r\n�ſ��� ���� �Լ��� �������� ���� �ϰ�, ������ ������ �帧�� ���������� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b>\r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� �Լ��� �������� Ȯ���մϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� �Ҹ��� �����մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ���� ����������, ���밡 �������� �ʰ� �񱸸ۿ��� �ε巴�� ���⸦ �������� �����մϴ�. ���� ������ �ʵ��� �����ؾ� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -</size></b> \r\n���� ����ġ�� ���̸� �Ҹ��� �����ų� ���ϰ� ���� �� �ֽ��ϴ�. �̶��� ���� ����ϰ� �ϰ� �ڿ������� ���⸦ ���񵵷� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -</size></b> \r\n���� �� ��� ������ ������ �Ҹ��� ��Ȱ�ϰ� �������� Ȯ���ϰ�, �ſ��� ���� ������ �帧�� �ð������� Ȯ���մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -</size></b> \r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� ���Ⱑ �ε巴�� �������� �������ϴ�.\r\n������ ���� ���� : \"��\", \"ȣ\" ���� �Ҹ��� �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b> \r\n���� �ȼҸ�(����) �Ŀ�������, ���밡 �������� �ʰ� ���� �޺κ�(������)�� ��õ�忡 ���ϰ� ��� �Ҹ��� ���ϴ�. ���Ⱑ ª�� �ܴ��ϰ� ���� ���;� �ϸ�, ������ �� ���� ������ ������ �����ϴ� ���� �߿��մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b>\r\n���� ������ ���� �ָ� �Ҹ��� ����ġ�� �����ϰ� �鸱 �� �ְ�, �ʹ� ���ϸ� �ȼҸ� Ư���� �ܴ����� �������� �� �ֽ��ϴ�. �̷� ���� ���� ���� ������ �����ϰ�, ���� �� ��� ������ ������ �Ͽ� ������ �ε巴�� �̾���� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b>\r\n���� �� ��� ������ ��������, �ſ��� ����� ���� ��ġ�� �ð������� Ȯ���ϸ鼭 �Ҹ��� �ܴ��ϰ� ������ üũ�մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b>\r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� ���� ���� ��ġ�� �����ϸ� �ܴ��ϰ� �Ҹ��� �������� Ȯ���մϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� ª�� �Ҹ��� �ݺ��ϸ� ������ ��Ȯ���� ���Դϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� �ȼҸ�(����) �Ŀ�������, ������ ���� ����(ġ��)�� ���ϰ� ��� �Ҹ��� ���ϴ�. ���Ⱑ ª�� �ܴ��ϰ� ���� ���;� �ϸ�, ������ �� ������ ������ ���� �ݴϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -</size></b> \r\n���� �ʹ� ���� �и� �Ҹ��� �����ϰų� ���� �� ������, �ݴ�� ���� �����ϸ� �ȼҸ� Ư���� ���� ������ ������ �ʽ��ϴ�. ���� ������ ������ �����ϰ�, ������ �帧�� �ν��ϸ鼭 �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b>\r\n�ſ��� ���� ������ ���� �ڿ� ��Ȯ�ϰ� ��� �ִ��� �ð������� Ȯ���ϰ�, ������ ������ �帧�� �������� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b>\r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� ���� ���� ��ġ�� �����մϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� ª�� �Ҹ��� �ݺ��ϸ� ��Ȯ�� ������ �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� �ȼҸ�(����) �Ŀ�������, �Լ��� ���ϰ� �ݾҴٰ� ���� ���⸦ ª�� �ܴ��ϰ� �Ͷ߸��� �����մϴ�. �Ҹ��� ª�� ��Ȯ�ϰ� ����� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b>\r\n�Լ��� ����ġ�� ���ϰ� ������ ������ �����ϰ� �鸮��, �ݴ�� �ʹ� ���ϸ� �ȼҸ��� Ư¡�� �� ��Ÿ���� ���� �� �ֽ��ϴ�. �Լ��� ���� ������ �����Ͽ� �ε巴�� �ܴ��ϰ� �Ҹ��� �̾���� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b>\r\n�ſ��� ���� �Լ��� ������ ������ ����� �ð������� Ȯ���ϰ�, ������ �� �տ��� ������ ������ �帧�� �������ϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -</size></b> \r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� �Լ��� ���� �������� �����մϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� ª�� �Ҹ��� �ݺ��Ͽ� ��Ȯ�� ������ �����մϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� �ȼҸ�(����) ����������, ���밡 �������� ������ ������ �Ʒ��� ��ó�� �� ������ ��� ���ϰ� ���⸦ �������� �Ҹ��� ���ϴ�. �Ҹ��� ��Ȯ�ϰ� ���ϰ� ���;� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b>\r\n���� �ʹ� ������ ���аų�, �Ҹ��� ����ġ�� ������ �� �ֽ��ϴ�. ���� ��ġ�� �����ϸ� ���Ⱑ �ε巴�� �帣���� �����ϰ�, �Ҹ��� ���� ���� ������ ������ �帧�� ������ �մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b>\r\n�ſ��� ���� ���� ��ġ�� �ð������� Ȯ���ϸ�, ������ �� �տ��� ������ ������ �帧�� �������� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -</size></b> \r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��ϸ� ���� ��ġ�� �Ҹ��� ������ �����մϴ�.\r\n������ ���� ���� : \"��\", \"��\" ���� ª�� �Ҹ��� �ݺ��Ͽ� ������ ��Ȯ���� ���Դϴ�.\r\n";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� �ȼҸ�(����) ����������, ���밡 �������� �ʰ� ������ ���� �ٷ� �ڿ� ���ϰ� ��� �����մϴ�. �Ҹ��� ª�� ���ϰ� ���� ���;� �ϸ�, ���� ���� ������ �����ؾ� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -</size></b> \r\n���� �ʹ� ���� ��� �Ҹ��� ��ĥ�� ���� �� ������, �ݴ�� ���ϰ� ��� ������ ������ ������ �� �ֽ��ϴ�. ���� ���� �����Ͽ� �ε巴�� �ܴ��ϰ� �����ϵ��� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b>\r\n�ſ��� ����� ���� ��ġ�� �ð������� Ȯ���ϰ�, �Ҹ��� �� �� ������ ������ �帧�� �������ϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b>\r\n�Ҹ� ���� ���� : \"��\" �Ҹ��� �ݺ��Ͽ� ������ ���� �ڿ� ��Ȯ�� ��� �ִ��� Ȯ���մϴ�.\r\n������ ���� ���� : \"¥\", \"��\" ���� ª�� �Ҹ��� �ݺ��Ͽ� ������ ��Ȯ���� ���Դϴ�.\r\n";
                                break;

                            // ���� �ǵ��
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ���� ��������, ���� ũ�� ������ ��Ҹ��� �ڿ������� ���鼭 �����մϴ�. ���� ���� ��ġ�ϸ�, ��õ����� �Ÿ��� �־�� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -  </size></b>\r\n���� �ʹ� ���� ������ �Ҹ��� ���� �� �ֽ��ϴ�. �̷� ���� �ſ��� ���� ���� ũ�� ������ ������ �ϵ��� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b>\r\n������ ���� ������ �ε帮�� �Ҹ��� ���� ������ ������ �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -  </size></b>\r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� \"��\"�� �����ϳ�, ������ �� \"��\" �Ҹ��� ��Ұ� ���Ե˴ϴ�. ���� �� ������ ��ġ�ϸ� ���� �����ϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n���� �ڷ� ���� �Ҹ��� �帴���� �� �ֽ��ϴ�. ���� ��ġ�� �����ϸ� �ſ��� ���� �ð������� Ȯ���ϰ� �մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b>\r\n���� ��ġ�� ������ �Ҹ��� ���� ���� ���� �Ծտ� ��� ������ �帧�� �������� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -  </size></b>\r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n���� ���� �а� �����鼭 �����ϴ� �߰� �����Դϴ�. ���� �ణ ������ ��ġ�ϰ� �Լ��� �ڿ������� ������ �մϴ�. �Ҹ��� ���� ���� �ε巴�� ���������� �����ϴ� ���� �߿��մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -  </size></b>\r\n\"��\" �Ҹ��� ������ �� ���� �߻��ϴ� ������ �Ҹ��� \"��\"�� \"��\"�� ȥ���Ǵ� ���Դϴ�. �̷� ���, \"��\"�� ������ �ڿ� \"��\"�� �����Ͽ� ������ ����, �� �Ҹ��� ���̸� ��Ȯ�ϰ� �ν��� �� �ֵ��� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b> </size></b>\r\n���� ������ ������ �Ծ��� ����� ������ �Ͽ� �ùٸ� ��ġ�� Ȯ���մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - \r\n�Ҹ� ���� ����: \"�� �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�´� \"��\"�� ����������, \"��\"�� ��Ұ� �߰��Ǿ� �� �ε巴�� �����˴ϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -  </size></b>\r\n\"��\"�� ������ �� �Ҹ��� ����Ȯ�ϰ� ���� �� �ֽ��ϴ�. �̷� ���, \"��\"�� \"��\"�� ���� ������ �� ��, �� ������ �߰� �Ҹ��� �ǽ������� �����Ͽ� �ùٸ� �������� ������ �� �ֵ��� �մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b>\r\n������ �� ������ ��� �����ϸ鼭 ������ ���������� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -  </size></b>\r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�ô� ���� ����� ��������, ���� �ణ ������ ���� �߰� ��ġ�� �ξ� �����մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -  </size></b>\r\n�Ҹ��� �ʹ� ���� ������ ������ �� �ֽ��ϴ�. �̷� ���� ��Ҹ��� ���߾� �����ϰ� �մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b>\r\n������ ���� ������ �ε帮�� ������ ���� ������ ���������� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -  </size></b>\r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�Ŵ� \"��\"�� ������ �� ���� �������� ��¦ �̵����� �����ϴ� �Ҹ��Դϴ�. ���� �ణ ���� ���¿��� �ε巴�� �����մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n���� ��ġ�� ����Ȯ�ϸ� �Ҹ��� ����Ȯ���� �� �ֽ��ϴ�. �ſ��� ����� ������ �� ���� ���ʿ� ��Ȯ�� ��ġ��Ű���� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b>\r\n���� ��ġ�� ������ �Ҹ��� ���鼭 ������ �Ծտ� ��� ������ �帧�� Ȯ���մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b> \r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�Ĵ� \"��\"�� ����������, ������ �� ���� �ణ ���߰� �Լ��� ������ �־� �Ҹ��� ��Ȯ�ϰ� �����մϴ�. ���� �Ʒ��ʿ� ��ġ�ϰ�, ���� ������ �ణ �� ������ �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n\"��\"�� ������ �� �Ҹ��� �Ҹ�Ȯ�ϰ� �鸱 �� �ֽ��ϴ�. �̶��� \"��\"�� ������ �� \"��\"�� ��ȯ�� ������ �Ͽ� �� �Ҹ��� ���̸� Ȯ���� ������ �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b> \r\n���� ��ġ�� ������ ������ ���� ������ �ε帮�� �Ҹ��� ���� ������ ������ �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b> \r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�ƴ� \"��\"�� ���� �պκ��� �÷� �����ϴ� �Ҹ��Դϴ�. ���� �ణ ������ �ε巴�� �����ϸ�, ���� ��ġ�� �� �����Ͽ� ��Ȯ�� �Ҹ��� ���� ���� �߿��մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n\"��\"�� ������ �� �Ҹ��� �Ҹ�Ȯ�ϰ� ���� �� �ֽ��ϴ�. �� ���, \"��\"�� \"��\"�� ���� �����Ͽ� ������ ��, �� �Ҹ��� �ڿ������� �̾ \"��\"�� ������ ������ �մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b> \r\n���� ���� ���¿��� �Ҹ��� ���� ������ �Ծտ��� ������ �帧�� ���������� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b> \r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�Ǵ� �ձ۰� ���� ������ �����ϴ� �Ҹ��Դϴ�. ���� �߰� ��ġ���� �ణ �ö󰩴ϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n\"��\"�� ������ �� �Լ��� �ʹ� ���� �����ϸ� �Ҹ��� �帴�ϰ� �鸱 �� �ֽ��ϴ�. �� ���, �Լ��� �ε巴�� �ձ۰� �Ͽ� �ڿ������� �����ϵ��� �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b> \r\n�Լ��� ������ ������ �ε巴�� �����̰� �ϰ� �Ҹ��� ���� ������ ������ �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -  </size></b>\r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>r\n�˴� \"��\"�� ������ \"��\"�� ��Ұ� ���յ� �Ҹ��Դϴ�. �Լ��� �ձ۰� �ϸ鼭 �ε巴�� �����ϰ�, ���� ���� ��ġ���� �ڿ������� �Ҹ��� ���� ���� �߿��մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n�Լ��� ������ ������ ��� �Ҹ��� ��ȣ���� �� �ֽ��ϴ�. �̷� ���� �Ҹ��� ������ \"��\"�� ��Ȯ�ϰ� �غ��� �մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b>\r\n�Ҹ��� ���鼭 �Լ��� �������� ������ Ȯ���ϰ� �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -  </size></b>\r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�ʴ� \"��\"�� \"��\"�� ��Ұ� ���յ� �Ҹ��Դϴ�. ������ �� �Լ��� ó���� �ձ۰� �Ͽ� �ణ ������, ������ ����ʿ� ���� �ڿ������� \"��\"�� ������ ������ �Լ��� ��¦ �о����� ���°� �˴ϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n�Ҹ��� ����Ȯ�� �� ������, �̶��� \"��\"�� \"��\"�� �и��� �����ϰ� �մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b> \r\n�Ҹ��� ���鼭 ������ �Լ��� ����� �������� Ȯ���Ͽ� ���� �� �Լ��� ��ġ�� ���¸� ������ �ùٸ� ������ �����մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -  </size></b>\r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�ȴ� \"��\"�� \"��\"�� ��������, �Լ��� ó���� �ձ۰� �����Ͽ� \"��\"�� ������ �� ��, �Լ��� ����� �ε巴�� ��鼭 \"��\"�� ��ȯ�ؾ� �մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -  </size></b>\r\n\"��\"�� ������ �� �Ҹ��� �Ҹ�Ȯ�ϰ� �鸱 �� �ֽ��ϴ�. �̷� ���, \"��\"�� \"��\"�� ���� ��Ȯ�ϰ� �����Ͽ� �� �Ҹ��� ���̸� Ȯ���� �������� �����ϴ� ���� �����ϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b> \r\n�Ҹ��� ���鼭 ������ �Լ��� ����� �������� Ȯ���Ͽ� ���� �� �Լ��� ��ġ�� ���¸� ������ �ùٸ� ������ �����մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b> \r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�ɴ� \"��\"�� \"��\"�� �������� �̷���� �Ҹ��Դϴ�. ������ �� �Լ��� ó���� �ձ۰� �����ϸ鼭 �ణ ������ �˴ϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n\"��\"�� ������ �� �Ҹ��� �Ҹ�Ȯ�ϰ� �鸱 �� �ֽ��ϴ�. �̷� ���, \"��\"�� \"��\"�� ���� �и��� �����Ͽ� �� ������ ���̸� Ȯ���� �������� �����ϴ� ���� �����ϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b>\r\n�Ҹ��� ���鼭 ������ �Լ��� ����� �������� Ȯ���Ͽ� ���� �� �Լ��� ��ġ�� ���¸� ������ �ùٸ� ������ �����մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b> \r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�̴� �Լ��� �ձ۰� �ϰ� ���� �߰� ��ġ�� �ξ� �����մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -  </size></b>\r\n�Լ��� �ʹ� ���� �����ϸ� �Ҹ��� ���� �� �ֽ��ϴ�. �̷� ���� �Լ��� �ε巴�� �����Ͽ� �Ҹ��� ���� �մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b> \r\n�Լ��� �������� ������ ������ �Ҹ��� ���� ������ ������ �մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -  </size></b>\r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�д� �Լ��� �ձ۰� �ϰ� �����ϴ� �Ҹ��Դϴ�. ������ �� �Լ��� �ձ۰� �����ϸ�, ���� ���� ���ʿ� ��ġ���Ѿ� �մϴ�. �̶� �Լ��� �ձ۰� �� ���¿��� �Ҹ��� �ε巴�� ������ �����ϴ� ���� �߿��մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n\"��\"�� ������ �� �Ҹ��� ����Ȯ�ϰ� ���� �� �ֽ��ϴ�. �̷� ���, \"��\"�� �и��ϰ� ������ ��, �Լ��� �ձ۰� �����ϸ鼭 ���� �պκ��� ���� �÷� \"��\"�� �����ϵ��� �����ϴ� ���� �����ϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b>\r\n�Լ��� ��ġ�� ������ �Ҹ��� ���� ������ ������ �帧�� Ȯ���մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b> \r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�ϴ� \"��\"�� \"��\"�� �������� �̷���� �Ҹ��Դϴ�. ������ ��, ó������ �Լ��� �ձ۰� �Ͽ� \"��\"ó�� ������ ��, �Լ��� �� ������ �ø��� \"��\"�� ������ ���մϴ�. \r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n\"��\"�� ������ �� �Ҹ��� �帴�ϰ� ���� �� �ֽ��ϴ�. �̷� ���, \"��\"�� \"��\"�� ���� �и��ϰ� �����Ͽ� �� ������ ���̸� ��Ȯ�� �ν��ϵ��� �����ϴ� ���� �߿��մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b> \r\n�Ҹ��� ���鼭 ������ �Լ��� ����� �������� Ȯ���Ͽ� ���� �� �Լ��� ��ġ�� ���¸� ������ �ùٸ� ������ �����մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -  </size></b>\r\n�Ҹ� ���� ����: \"��\"�Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�ʹ� \"��\"�� \"��\"�� �������� �̷���� �Ҹ��Դϴ�. ������ �� �Լ��� �ձ۰� �Ͽ� �Ҹ��� ����, ���� �ణ ������ �˴ϴ�. ���� �߰� ���̿� ��ġ���� �ڿ������� �����ϴ� ���� �߿��մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -  </size></b>\r\n\"��\"�� ������ �� �Ҹ��� �Һи��ϰ� �鸱 �� �ֽ��ϴ�. �̷� ���� \"��\"�� \"��\"�� ���� ��Ȯ�ϰ� �����Ͽ� �����ϴ� ������ �ʿ��մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b> \r\n�Ҹ��� ���� ���� ������ �Լ��� ����� Ȯ���ϰ� ������ ������, �ùٸ� ������ ���� �Լ��� ��ġ�� �����մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b> \r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�δ� \"��\"�� \"��\"�� �������� �̷���� �Ҹ��Դϴ�. ������ ��, ó������ �Լ��� �ձ۰� �Ͽ� \"��\"ó�� �Ҹ��� ����, �� ���¿��� �Լ��� �� ������ �ε巴�� �����鼭 \"��\"�� �������� ��ȯ�մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -  </size></b>\r\n\"��\"�� ������ �� �Ҹ��� �帴�ϰ� �鸱 �� �ֽ��ϴ�. �̷� ���� \"��\"�� \"��\"�� �и��Ͽ� ���� ��Ȯ�ϰ� �����ϴ� ������ �ʿ��մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b>\r\n�Լ��� �������� ������ ������ �Ҹ��� ���� ������ Ȯ���ϸ�, ������ �� �Լ��� ��Ȯ�� ��ġ�� ����� �ν��մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b> \r\n�Ҹ� ���� ����: \"�� �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�Ѵ� ���� �������� ��� ������, ���� �߰� ��ġ���� �����ϰ� �����մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> -  </size></b>\r\n\"��\"�� ������ �� �Ҹ��� ���ϰ� �鸱 �� �ֽ��ϴ�. �̷� ���, ���� ��ġ�� ���߰� �Լ��� ������ ������ �����Ͽ� �Ҹ��� �� �и��ϰ� ������ �����ϴ� ���� �߿��մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> - </size></b> \r\n�Լ��� ���� ������ ������ ������ �Ҹ��� ����, ���� �� �߻��ϴ� ������ �����鼭 �ùٸ� ���� ��ġ�� ����� �ν��մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b> \r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�Ӵ� ���� �� ������ ������ ���� ���� ��ġ���� �����ϴ� �Ҹ��Դϴ�. �̶� �Ҹ��� �ε巴�� ���� ���� �߿��մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n\"��\"�� ������ �� �Ҹ��� ����Ȯ�ϰ� ���� �� �ֽ��ϴ�. �̷� ���, ���� �պκ��� ������ ���� ���ʿ��� ������ ������ ���� �����̿� ��ġ�ϵ��� �����Ͽ� ��Ȯ�� �Ҹ��� ������ �����մϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b>\r\n���� ��ġ�� ������ �Ҹ��� ����, ������ ���� ������ Ȯ���մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> - </size></b> \r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;
                            case "��":
                                feedback += " \r\n\r\n<b><size=40><���� �� �Ҹ��� Ư¡�� �ùٸ� ���� ���> - </size></b>\r\n�Ҵ� \"��\"�� \"��\"�� �������� �̷���� �Ҹ��Դϴ�. ������ �� �ణ�� ������ �־� ���� ���� ��ġ��Ű��, �Լ��� �ڿ������� ������ �մϴ�. �Ҹ��� �� ��, \"��\"�� ���� ������ �� \"��\"�� �ε巴�� �̾����� ���� �߿��մϴ�.\r\n\r\n<b><size=40><������ �� ���� ����� ������ ���� ����> - </size></b> \r\n\"��\"�� ������ �� �Ҹ��� ����Ȯ�ϰ� ���� �� �ֽ��ϴ�. �̷� ���, \"��\"�� \"��\"�� ���� ��Ȯ�ϰ� �����Ͽ� �� ������ ���̸� �и��� �ν��ϵ��� �����ϴ� ���� �����ϴ�.\r\n\r\n<b><size=40><������ Ȱ���� ���� ����> -  </size></b>\r\n�Լ��� ���� �������� ������ ������ �Ҹ��� ����, ������ ���� ������ Ȯ���Ͽ� �ùٸ� ������ �ν��մϴ�.\r\n\r\n<b><size=40><�������� ���� �ϴ� ���� ����> -  </size></b>\r\n�Ҹ� ���� ����: \"��\" �Ҹ��� �ݺ��մϴ�.";
                                break;


                        }
                        feedbacks.Add(feedback);
                        Debug.Log($"���� ���� ������ ���� ����: {lowestPhoneme} / ��Ȯ��: {lowestPhonemeScore} / �ǵ��: {feedback}");

                    }
                    else
                    {
                        Debug.LogWarning("���Ҹ� ã�� �� �����ϴ�.");
                    }

                    // WordGameController���� selectedWords�� videoPaths ��������
                    List<string> selectedWords = WordGameController.Instance.GetSelectedWords();
                    //        List<string> videoPaths = WordGameController.Instance.GetVideoPaths();

                    // ���õ� �ܾ�� ������ ��� �� recognizedTexts�� JSON�� ����
                    WordDataManager.SaveData(selectedWords, recognizedTexts, /*audioClips, videoPaths,*/ accuracyScores, feedbacks);
                    Debug.Log("���� �� ������ �Բ� ������ ���� �Ϸ�.");

                }
            }
            else
            {
                Debug.LogWarning($"���� �� ���� ����: {result.Reason}");
            }
        }
    }

    public class KoreanPhonemeSplitter
    {
        private static readonly char[] InitialConsonants =  // �ʼ� �迭
        {
        '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��'
    };

        private static readonly char[] Vowels =  // �߼� �迭
        {
        '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��'
    };

        private static readonly char[] FinalConsonants =  // ���� �迭
        {
        '\0', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��'
    };

        // �ܾ ���ҷ� �и��ϴ� �޼���
        public static List<string> SplitIntoPhonemes(string word)
        {
            List<string> phonemes = new List<string>();

            foreach (char syllable in word)
            {
                if (syllable >= 0xAC00 && syllable <= 0xD7A3)  // �ѱ� ���� ���� ���� ���� ���
                {
                    int unicodeIndex = syllable - 0xAC00;
                    int initialIndex = unicodeIndex / (21 * 28);  // �ʼ� �ε��� ���
                    int vowelIndex = (unicodeIndex % (21 * 28)) / 28;  // �߼� �ε��� ���
                    int finalIndex = unicodeIndex % 28;  // ���� �ε��� ���

                    phonemes.Add(InitialConsonants[initialIndex].ToString());  // �ʼ� �߰�
                    phonemes.Add(Vowels[vowelIndex].ToString());  // �߼� �߰�

                    if (finalIndex > 0)  // ������ ���� ��쿡�� �߰�
                    {
                        phonemes.Add(FinalConsonants[finalIndex].ToString());
                    }
                }
                else  // �ѱ� ������ �ƴ� ��� �״�� �߰�
                {
                    phonemes.Add(syllable.ToString());
                    Debug.Log($"�ѱ� ������ �ƴ�: {syllable}");
                }
            }

            return phonemes;
        }


        public static string GetSyllableAtPhonemeIndex(string word, int phonemeIndex)
        {
            List<string> phonemes = SplitIntoPhonemes(word);  // �ܾ ���ҷ� �и�
            int currentPhonemeIndex = 0;

            for (int i = 0; i < word.Length; i++)
            {
                char syllable = word[i];
                if (syllable >= 0xAC00 && syllable <= 0xD7A3)  // �ѱ� ���� ���� ���� ���� ���
                {
                    int unicodeIndex = syllable - 0xAC00;
                    int phonemesInSyllable = (unicodeIndex % 28 > 0) ? 3 : 2;  // ������ ������ 3��, ������ 2��

                    // phonemeIndex�� ���� ���� ���� �ִ��� Ȯ��
                    if (phonemeIndex >= currentPhonemeIndex && phonemeIndex < currentPhonemeIndex + phonemesInSyllable)
                    {
                        return syllable.ToString();  // �ش� ������ ��ȯ
                    }

                    currentPhonemeIndex += phonemesInSyllable;
                }
            }

            return "�� �� ����";
        }

        public static string GetPhonemePosition(string word, int phonemeIndex, out string syllable)
        {
            List<string> phonemes = SplitIntoPhonemes(word); // �ܾ ���ҷ� �и�
            int currentPhonemeIndex = 0;

            // ������ ���� ������ �� �������� ���� ��ġ�� ����
            for (int i = 0; i < word.Length; i++)
            {
                char currentSyllable = word[i];
                if (currentSyllable >= 0xAC00 && currentSyllable <= 0xD7A3)  // �ѱ� ���� ���� ���� ���� ���
                {
                    int unicodeIndex = currentSyllable - 0xAC00;
                    int phonemesInSyllable = (unicodeIndex % 28 > 0) ? 3 : 2;  // ������ ������ 3��, ������ 2��

                    // phonemeIndex�� ���� ���� ���� ���ϴ��� Ȯ��
                    if (phonemeIndex >= currentPhonemeIndex && phonemeIndex < currentPhonemeIndex + phonemesInSyllable)
                    {
                        int positionInSyllable = phonemeIndex - currentPhonemeIndex;

                        // �ش� ������ ��ȯ
                        syllable = currentSyllable.ToString();

                        // ���� ��ġ�� ���� �ʼ�, �߼�, ������ ��ȯ
                        if (positionInSyllable == 0)
                            return "�ʼ�";
                        else if (positionInSyllable == 1)
                            return "�߼�";
                        else if (positionInSyllable == 2 && phonemesInSyllable == 3)  // ������ �ִ� ��츸 ���� ��ȯ
                            return "����";
                    }

                    currentPhonemeIndex += phonemesInSyllable;  // ���� �ε����� �̵�
                }
            }

            syllable = "�� �� ����";  // ������ ã�� �� ���� ��
            return "�� �� ����";
        }


    }


    /// �����νİ� �������� �ߴ� �޼���
    async void StopRecognition()
    {
        //�����ν� �ߴ�
        if (recognizer != null)
        {
            Debug.Log("�����ν� �ߴ�");
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

            // ���� �ν� �ߴ� �� StopRecordButton�� �ڵ� Ŭ�� (���� �ߴ�)
            unityContext.Post(_ =>
            {
                Debug.Log("StopRecordButton �ڵ� Ŭ��");
                stopRecordButton.onClick.Invoke();  // StopRecordButton �ڵ� Ŭ��
            }, null);
            recognizer.Recognized -= RecognizedHandler;
            recognizer.Canceled -= CanceledHandler;
            recognizer.Dispose();
            recognizer = null;
        }
        else
        {
            Debug.LogWarning("�����νı� �ʱ�ȭ���� ����.");
        }

        // ���� �ν� �ߴ� �� ��� �ؽ�Ʈ �ʱ�ȭ
        //resultText.text = "";
        // recognizedText = null;
    }

    // ���� ���� �޼���
    public void StopRecording()
    {


        if (isRecording || Microphone.IsRecording(null))
        {
            Microphone.End(null);
            isRecording = false;
            Debug.Log("���� �ߴ�!");

            // ������ ����� Ŭ���� ���̸� ��� (���� �Ϸ� ��)
            if (recordedClip != null)
            {
                // WordGameController���� ���� �ܾ ������
                string currentWord = WordGameController.Instance.GetCurrentWord();

                // �ùٸ��� ������ ��� ���� ����
                if (recognizedText.Equals(WordGameController.Instance.GetCurrentWord(), StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log("���!! (��Ȯ�� ����)");

                    // ���� ���� ���� (currentWord�� �Ѱ���)
                    SaveRecording(recordedClip, currentWord);

                    // ������ ����� Ŭ���� ����Ʈ�� �߰�
                    audioClips.Add(recordedClip);

                    // �ش� recognizedText ����
                    recognizedTexts.Add(recognizedText);
                    Debug.Log("���� ����� recognizedTexts: " + string.Join(", ", recognizedTexts));
                    attemptCount = 0;
                    Debug.Log("attemptCount �ʱ�ȭ��");
                }
                //3��° �õ��� ��� ���� ����
                if (attemptCount == 3)
                {
                    Debug.Log("���!! (3��° �õ� ����~~!)");

                    // ���� ���� ���� (currentWord�� �Ѱ���)
                    SaveRecording(recordedClip, currentWord);

                    // ������ ����� Ŭ���� ����Ʈ�� �߰�
                    audioClips.Add(recordedClip);

                    // �ش� recognizedText ����
                    recognizedTexts.Add(recognizedText);
                    Debug.Log("���� ����� recognizedTexts: " + string.Join(", ", recognizedTexts));
                    attemptCount = 0;
                    Debug.Log("attemptCount �ʱ�ȭ��");

                }



            }
            else
            {
                Debug.LogWarning("������ ����� Ŭ���� �����ϴ�.");
            }
        }
        else
        {
            // Debug.LogError("������ ������� �ʾҽ��ϴ�. ���� ���� ����.");
        }
    }

    public void PlayRecording(Image buttonImage)
    {
        if (recordedClip != null)
        {
            Debug.Log("���� ���� ��� ��...");
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // ����� �ҽ� ���� Ȯ��
            audioSource.volume = 1.0f;  // ������ 100%�� ����
            audioSource.mute = false;   // ���Ұ� ����
            audioSource.clip = recordedClip;
            audioSource.Play();

            // ������� ���� ó��
            if (bgmController != null)
            {
                bgmController.SetRecordingPlayingStatus(true);  // ������ ��� ������ BGMController�� �˸�
            }

            // ����� ��� �� �̹��� �ִϸ��̼� ����
            StartCoroutine(AnimateButtonWhilePlaying(audioSource, buttonImage));

            Debug.Log("���� ���� ��� �Ϸ�");
            if (panelToActivate.activeSelf)
            {
                StartCoroutine(WaitForAudioToEnd(audioSource, panelToActivate, text1, text2));
            }
            else if (finalFailPanel.activeSelf)
            {
                StartCoroutine(WaitForAudioToEnd(audioSource, finalFailPanel, ftext1, ftext2));
            }

            // ���� ������ ������ ��������� �ٽ� �����ϵ��� ����
            StartCoroutine(WaitForAudioToEnd(audioSource));
        }
        else
        {
            Debug.LogError("����� ���� ������ �����ϴ�.");
        }
    }

    private IEnumerator WaitForAudioToEnd(AudioSource audioSource)
    {
        // ������ ���� ������ ��ٸ� ��, ��������� �簳�ϵ��� ó��
        yield return new WaitWhile(() => audioSource.isPlaying);

        if (bgmController != null)
        {
            bgmController.SetRecordingPlayingStatus(false);  // ������ �������Ƿ� ������� �簳
        }
    }
    // �̹��� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator AnimateButtonWhilePlaying(AudioSource audioSource, Image buttonImage)
    {
        Vector3 originalScale = buttonImage.transform.localScale;
        Vector3 enlargedScale = originalScale * 1.2f;  // ��¦ Ȯ��� ũ��

        while (audioSource.isPlaying)
        {
            // 0.5�� ���� �̹����� Ŀ���ٰ� ���� ũ��� ���ƿ� (Ȯ��)
            yield return StartCoroutine(ScaleButton(buttonImage, enlargedScale, 0.5f));
            // 0.5�� ���� �̹����� �ٽ� ���� ũ��� ���ƿ� (���)
            yield return StartCoroutine(ScaleButton(buttonImage, originalScale, 0.5f));
        }

        // ����� ����� ������ �̹����� ���� ũ��� ����
        buttonImage.transform.localScale = originalScale;
    }
    // ��ư ũ�⸦ �ε巴�� ��ȭ��Ű�� �ڷ�ƾ
    private IEnumerator ScaleButton(Image buttonImage, Vector3 targetScale, float duration)
    {
        Vector3 initialScale = buttonImage.transform.localScale;
        float time = 0f;

        while (time < duration)
        {
            buttonImage.transform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        buttonImage.transform.localScale = targetScale;
    }
    // ����� ��� �Ϸ� �� ������ ó���ϴ� �ڷ�ƾ
    private IEnumerator WaitForAudioToEnd(AudioSource audioSource, GameObject panelToDeactivate, GameObject textToDeactivate, GameObject textToActivate)
    {
        while (audioSource.isPlaying)
        {
            yield return null;
        }

        // ����� ��� �Ϸ� �� 1�� ���
        yield return new WaitForSeconds(0.5f);

        // ����� ��� �Ϸ� �� text ��ü
        textToDeactivate.SetActive(false);
        textToActivate.SetActive(true);
        // �ܾ 3��°�� ���� text3 �Ǵ� ftext3 Ȱ��ȭ
        if (currentWordIndex == 2)
        {
            text3.SetActive(true);   // 3��° �ܾ��� �� text3 Ȱ��ȭ
            text2.SetActive(false);  // text2 ��Ȱ��ȭ
            ftext3.SetActive(true);  // 3��° �ܾ��� �� ftext3 Ȱ��ȭ
            ftext2.SetActive(false); // ftext2 ��Ȱ��ȭ
        }
        else
        {
            text3.SetActive(false);  // 3��° �ܾ �ƴϸ� text3 ��Ȱ��ȭ
            text2.SetActive(true);   // text2 Ȱ��ȭ
            ftext3.SetActive(false); // 3��° �ܾ �ƴϸ� ftext3 ��Ȱ��ȭ
            ftext2.SetActive(true);  // ftext2 Ȱ��ȭ
        }
        // text2 �Ǵ� ftext2�� Ȱ��ȭ�� ���� ��ƼŬ ���
        if (textToActivate == text2 || textToActivate == ftext2)
        {
            confettiParticle.Play();  // ��ƼŬ ���
            Debug.Log("��ƼŬ ��� ����!");
        }
        else
        {
            confettiParticle.Stop();  // �ٸ� ��� ��ƼŬ ��� ����
            Debug.Log("��ƼŬ ������.");
        }
        Debug.Log("����� ��� �Ϸ� �� ��� �� ���� ����");

        // 2�� ��� �� panelToDeactivate ��Ȱ��ȭ �� ���� ��ư Ŭ��
        yield return new WaitForSeconds(4);
        panelToDeactivate.SetActive(false);  // �г� ��Ȱ��ȭ
        nextButton.gameObject.SetActive(true);
        nextButton.onClick.Invoke();

        // panelToActivate �ʱ�ȭ (panelToActivate ��Ȱ��ȭ �� text1, text2 ���¸� �ʱ�ȭ)
        ResetPanelToActivate();  // text1 Ȱ��ȭ, text2 ��Ȱ��ȭ

        // finalFailPanel �ʱ�ȭ (finalFailPanel ��Ȱ��ȭ �� ftext1, ftext2 ���¸� �ʱ�ȭ)
        ResetFinalFailPanel();  // ftext1 Ȱ��ȭ, ftext2 ��Ȱ��ȭ

        attemptCount = 0;
        Debug.Log("attemptCount �ʱ�ȭ��");
    }

    // panelToActivate�� �ʱ� ���·� �����ϴ� �޼���
    private void ResetPanelToActivate()
    {
        text1.SetActive(true);  // text1 Ȱ��ȭ
        text2.SetActive(false); // text2 ��Ȱ��ȭ
        text3.SetActive(false); // text3 ��Ȱ��ȭ
        // ��ư Ŭ�� �÷��� ����
        isPanelToActivateButtonClicked = false;

        Debug.Log("panelToActivate �ʱ�ȭ �Ϸ�");
    }

    // finalFailPanel�� �ʱ� ���·� �����ϴ� �޼���
    private void ResetFinalFailPanel()
    {
        ftext1.SetActive(true);  // ftext1 Ȱ��ȭ
        ftext2.SetActive(false); // ftext2 ��Ȱ��ȭ
        ftext3.SetActive(false); // ftext3 ��Ȱ��ȭ
        // ��ư Ŭ�� �÷��� ����
        isFinalFailPanelButtonClicked = false;

        Debug.Log("finalFailPanel �ʱ�ȭ �Ϸ�");
    }



    private void SaveRecording(AudioClip clip, string word)
    {
        if (clip == null || clip.length == 0)
        {
            Debug.LogWarning("������ ������ ���ų� ��� �ֽ��ϴ�.");
            return;
        }

        // ���� ���� �̸��� �����ϰ� ���� (recordedAudio_1.wav, recordedAudio_2.wav, ...)
        string filename = $"recordedAudio_{word}_{recordingCount}.wav";
        string filePath = Path.Combine(sessionFolderPath, filename);
        Debug.Log("���� ���: " + filePath);

        try
        {
            byte[] wavData = WavUtility.FromAudioClip(clip);  // AudioClip�� WAV �����ͷ� ��ȯ
            File.WriteAllBytes(filePath, wavData);  // ���� ����

            Debug.Log("���� ���� ���� �Ϸ�: " + filePath);

            recordingCount++;  // ���� Ƚ�� ���� (���� ���� ���� �̸� �����ϰ� ����)
        }
        catch (System.Exception ex)
        {
            Debug.LogError("���� ���� ���� �� ���� �߻�: " + ex.Message);
        }
        // ������ ���Ϸ� ���� �� ����
        PerformPronunciationAssessment(filePath, word, recognizedText);  // ����� ���Ϸ� ���� ��
    }



    /// ���� �ν��� ��ҵǰų� �������� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯
    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        Debug.LogError($"�����ν� ���&���� ����: {e.ErrorDetails}");
    }

    void Update()
    {

        // panel2�� panel3�� Ȱ��ȭ�Ǿ��� �� ��ư Ȱ��ȭ
        if (panel2.activeSelf && panel3.activeSelf)
        {

            startButton.gameObject.SetActive(true);
        }
    }




}