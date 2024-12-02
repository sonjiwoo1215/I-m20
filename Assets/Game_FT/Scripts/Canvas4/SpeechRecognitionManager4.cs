using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.CognitiveServices.Speech;
using System.Threading.Tasks;

public class SpeechRecognitionManager4: MonoBehaviour
{
    private SpeechRecognizer recognizer;
    public UIManager uiManager; // UIManager�� ����
    public BaseDialogueManager dialogueManager; // �������� ���� BaseDialogueManager�� ����

    public static SpeechRecognitionManager4 Instance; // Singleton Instance
    private DialogueManager currentDialogueManager;

    private bool isRecognizing = false;

    private string recognizedText;

    private void Awake()
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

    // ���� DialogueManager�� �����ϴ� �޼���
    public void SetDialogueManager(DialogueManager dialogueManager)
    {
        currentDialogueManager = dialogueManager;
    }

    // ���� �ν��� ����� ���⿡ �����Ѵٰ� ����
    public void StoreRecognitionResult(string result)
    {
        recognizedText = result;
    }

    // recognizedText�� ��ȯ�ϴ� �޼��� �߰�
    public string GetRecognizedText()
    {
        return recognizedText;
    }

    private void Start()
    {
        var config = SpeechConfig.FromSubscription("1ZpqHQla3FjGYigKSPQIdYfK0e3r34BgRM0I6p2oVMVVzrrm4vroJQQJ99AKACNns7RXJ3w3AAAYACOGeY1y", "koreacentral");
        recognizer = new SpeechRecognizer(config, "ko-KR");

        // ���� Scene�� ���� DialogueManager�� �������� ����
        dialogueManager = FindCurrentDialogueManager();
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager�� ã�� �� �����ϴ�.");
        }
    }
    public void UpdateDialogueManager(GameObject activeCanvas)
    {
        dialogueManager = activeCanvas.GetComponentInChildren<BaseDialogueManager>();
        if (dialogueManager != null)
        {
            Debug.Log($"���ο� DialogueManager�� �����Ǿ����ϴ�: {dialogueManager.name}");
        }
        else
        {
            Debug.LogError("Ȱ��ȭ�� Canvas���� DialogueManager�� ã�� �� �����ϴ�.");
        }
    }

    private BaseDialogueManager FindCurrentDialogueManager()
    {
        // FCanvas�� ��� Canvas�� �θ� ������Ʈ
        var fCanvas = GameObject.Find("FT_Canvas");
        if (fCanvas == null)
        {
            Debug.LogError("FCanvas�� ã�� �� �����ϴ�.");
            return null;
        }

        // FCanvas ������ Ȱ��ȭ�� Canvas���� DialogueManager ã��
        foreach (Transform child in fCanvas.transform)
        {
            if (child.gameObject.activeSelf) // Ȱ��ȭ�� Canvas�� �˻�
            {
                var dialogueManager = child.GetComponentInChildren<BaseDialogueManager>();
                if (dialogueManager != null)
                {
                    Debug.Log($"���� Ȱ��ȭ�� Canvas�� DialogueManager�� ã�ҽ��ϴ�: {dialogueManager.name}");
                    return dialogueManager;
                }
            }
        }

        Debug.LogError("Ȱ��ȭ�� Canvas���� DialogueManager�� ã�� �� �����ϴ�.");
        return null; // Ȱ��ȭ�� Canvas�� ���ų� DialogueManager�� ���� ���
    }

    // ���� �ν� ����
    public async Task StartRecognition()
    {
        if (isRecognizing)
        {
            Debug.Log("�̹� ���� �ν� ���Դϴ�.");
            return;
        }

        isRecognizing = true;
        Debug.Log("���� �ν� ��...");

        var result = await recognizer.RecognizeOnceAsync();
        isRecognizing = false;

        if (result.Reason == ResultReason.RecognizedSpeech)
        {
            recognizedText = result.Text; // �νĵ� �ؽ�Ʈ�� ����
            Debug.Log($"�νĵ� �ؽ�Ʈ: {recognizedText}");
            uiManager.DisplaySTTResult(recognizedText);

            // DialogueManager�� ����
            if (dialogueManager != null)
            {
                dialogueManager.OnUserResponse(recognizedText);
            }
            else
            {
                Debug.LogError("���� DialogueManager�� �������� �ʾҽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning($"���� �ν� ����: {result.Reason}");
        }
    }

    // ���� �ν� ����
    public void StopRecognition()
    {
        if (isRecognizing)
        {
            isRecognizing = false;
            Debug.Log("���� �ν��� ������ ����Ǿ����ϴ�.");
        }
    }
}