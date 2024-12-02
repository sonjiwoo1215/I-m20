using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;  // ���� �� ���� ���ӽ����̽�

public class PronunciationEvaluationManager : MonoBehaviour
{
    private string subscriptionKey = "1ZpqHQla3FjGYigKSPQIdYfK0e3r34BgRM0I6p2oVMVVzrrm4vroJQQJ99AKACNns7RXJ3w3AAAYACOGeY1y";
    private string serviceRegion = "koreacentral";

    // ���� �򰡸� �����ϴ� �޼���
    public async Task EvaluatePronunciation(string audioFilePath, string expectedText)
    {
        if (string.IsNullOrEmpty(audioFilePath) || string.IsNullOrEmpty(expectedText))
        {
            Debug.LogError("����� ���� ��� �Ǵ� ���� �ؽ�Ʈ�� �������� �ʾҽ��ϴ�.");
            return;
        }

        if (!System.IO.File.Exists(audioFilePath))
        {
            Debug.LogError($"������ ����� ������ �������� �ʽ��ϴ�: {audioFilePath}");
            return;
        }

        // Azure Speech Config ����
        var config = SpeechConfig.FromSubscription(subscriptionKey, serviceRegion);

        // ����� ������ ����� �Է� ����
        var audioInput = AudioConfig.FromWavFileInput(audioFilePath);

        // ���� �� ���� (���� �ؽ�Ʈ, ���� ���, �� ����)
        var pronunciationAssessmentConfig = new PronunciationAssessmentConfig(expectedText, GradingSystem.HundredMark, Granularity.Phoneme);

        using (var recognizer = new SpeechRecognizer(config, audioInput))
        {
            // ���� �򰡸� �����ϵ��� ����
            pronunciationAssessmentConfig.ApplyTo(recognizer);

            Debug.Log("���� �򰡸� ���� ���Դϴ�...");

            // ������ �ν��ϰ� ���� �� ����
            var result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                // ���� �� ��� ����
                var pronunciationResult = PronunciationAssessmentResult.FromResult(result);

                Debug.Log($"��Ȯ�� ����: {pronunciationResult.AccuracyScore} / 100");

                // �߰������� ������ UI�� �ݿ��ϰų� ����ڿ��� �ǵ���� ������ �� �ֽ��ϴ�.
                // ��: uiManager.DisplayPronunciationScore(pronunciationResult.AccuracyScore);
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                Debug.LogWarning("���� �򰡸� ���� ��ġ�ϴ� ������ ã�� ���߽��ϴ�.");
            }
            /*else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechRecognitionCancellationDetails.FromResult(result);
                Debug.LogError($"���� �򰡰� ��ҵǾ����ϴ�. ����: {cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Debug.LogError($"���� �ڵ�: {cancellation.ErrorCode}");
                    Debug.LogError($"���� �޽���: {cancellation.ErrorDetails}");
                }
            }*/
        }
    }

    // ����� ���� �����͸� ���Ͽ� �����ϴ� �޼���
    public void SaveUserResponse(string expectedText, string userResponse)
    {
        // ���� �̸� ���� (���� ID ���)
        string fileName = $"{Application.persistentDataPath}/Response_{System.Guid.NewGuid()}.json";

        // ���� ������ ����
        var data = new ResponseData
        {
            expectedText = expectedText,
            userResponse = userResponse
        };

        // JSON���� ����ȭ �� ���� ����
        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(fileName, json);

        Debug.Log($"����� ���� �����Ͱ� ����Ǿ����ϴ�: {fileName}");
    }

    // ����� ���� ������ ����
    [System.Serializable]
    private class ResponseData
    {
        public string expectedText;  // ���� �ؽ�Ʈ
        public string userResponse;  // ����� ���� �ؽ�Ʈ
    }
}
