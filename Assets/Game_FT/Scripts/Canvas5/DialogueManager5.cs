using UnityEngine;
using System.Threading.Tasks;

public class DialogueManager5 : MonoBehaviour
{
    public SpeechSynthesisManager speechSynthesisManager; // TTS �Ŵ��� ����
    public UIManager uiManager; // UIManager ���� �߰�

    async void Start()
    {
        await WaitForSpeechSynthesisInitialization();
        DisplayGoodbyeMessage();
        PlayGoodbyeMessage();
    }

    private async Task WaitForSpeechSynthesisInitialization()
    {
        while (!speechSynthesisManager.isInitialized)
        {
            Debug.Log("SpeechSynthesisManager �ʱ�ȭ ��� ��...");
            await Task.Delay(100); // 100ms ��� �� �ٽ� Ȯ��
        }
    }

    private void DisplayGoodbyeMessage()
    {
        string goodbyeMessage = "�߰�~ ���� �Բ��ؼ� ��ſ���~";
        uiManager.questionText.text = goodbyeMessage; // UI�� �޽��� ǥ��
                                                      // Goodbye �޽����� PlayerPrefs�� ����
        PlayerPrefs.SetString("GoodbyeMessage", goodbyeMessage);
        PlayerPrefs.Save(); // ������ ������ ��� ����
    }

    private async void PlayGoodbyeMessage()
    {
        string goodbyeMessage = "�߰�~ ���� �Բ��ؼ� ��ſ���~";
        await speechSynthesisManager.SpeakText(goodbyeMessage);

        ShowCompletionPopup();  // TTS�� ������ �Ϸ� �˾� ǥ��
    }

    private void ShowCompletionPopup()
    {
        uiManager.ShowCompletionPopup();  // UIManager���� �˾� ǥ��
    }
}