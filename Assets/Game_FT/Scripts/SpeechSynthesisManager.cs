using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using System.Threading.Tasks;

public class SpeechSynthesisManager : MonoBehaviour
{
    private SpeechSynthesizer synthesizer;
    public bool isInitialized = false; // �ʱ�ȭ ���θ� Ȯ���ϴ� �÷���
    public bool isSpeaking = false; // TTS ���� ���� ����

    async void Start()
    {
        // Azure Speech SDK ���� �� �ʱ�ȭ
        await InitializeSpeechSynthesizer();
    }

    // �񵿱������� SpeechSynthesizer �ʱ�ȭ
    private async Task InitializeSpeechSynthesizer()
    {
        var config = SpeechConfig.FromSubscription("1ZpqHQla3FjGYigKSPQIdYfK0e3r34BgRM0I6p2oVMVVzrrm4vroJQQJ99AKACNns7RXJ3w3AAAYACOGeY1y", "koreacentral");
        synthesizer = new SpeechSynthesizer(config);

        if (synthesizer != null)
        {
            Debug.Log("SpeechSynthesizer �ʱ�ȭ ����");
            isInitialized = true; // �ʱ�ȭ �Ϸ�
        }
        else
        {
            Debug.LogError("SpeechSynthesizer �ʱ�ȭ ����");
        }
    }

    // �ؽ�Ʈ�� �������� ��ȯ�ϴ� �Լ�
    public async Task SpeakText(string text)
    {
        if (!isInitialized)
        {
            Debug.LogError("SpeechSynthesizer�� ���� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        isSpeaking = true;  // TTS ���� �� ���¸� true�� ����
        var result = await synthesizer.SpeakTextAsync(text);
        isSpeaking = false; // TTS �Ϸ� �� ���¸� false�� ����

        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            Debug.Log("TTS �Ϸ�: " + text);
        }
        else
        {
            Debug.LogError("TTS ����: " + result.Reason);
        }
    }
}