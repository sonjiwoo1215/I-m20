using UnityEngine;
using UnityEngine.UI;
using static TrainingFetcher;

public class FluencyUIController : MonoBehaviour
{
    public Text dialogueText; // �ǵ�� �����͸� ǥ���� UI
    public Text fluencyScoreText; // ��â�� ���� UI
    public Text pronunciationScoreText; // ���� ���� UI
    public Text intonationScoreText; // ��� ���� UI

    public Image fluencyBar; // ��â�� ���� �׷���
    public Image pronunciationBar; // ���� ���� �׷���
    public Image intonationBar; // ��� ���� �׷���

    private int currentCanvasIndex = 0;

    void Start()
    {
        LoadAndDisplayAllFeedback(); // �ǵ�� �ε� �� ������Ʈ
        UpdateScores();
    }

    public void LoadAndDisplayAllFeedback()
    {
        string fullDialogue = "";
        var trainingFetcher = TrainingFetcher.instance;

        for (int canvasIndex = 1; canvasIndex <= 3; canvasIndex++) // Canvas 1, 2, 3�� ������
        {
            string key = $"Canvas_{canvasIndex}_Dialogue";
            string jsonData = PlayerPrefs.GetString(key, "");

            if (!string.IsNullOrEmpty(jsonData))
            {
                var dialogueData = JsonUtility.FromJson<DialogueData>(jsonData);

                fullDialogue += $"<b><size=42><color=#8388FF>-- [��� {canvasIndex}] --\n</color></size></b>";
                fullDialogue += "\n";
                for (int i = 0; i < dialogueData.Questions.Count; i++)
                {
                    fullDialogue += $"<b>��Ű :</b>  {dialogueData.Questions[i]}\n";
                    if (i < dialogueData.Answers.Count)
                    {
                        fullDialogue += $"<b>���� �亯 :</b>  {dialogueData.Answers[i]}\n";
                    }
                }

                fullDialogue += "\n";
            }
            else
            {
                fullDialogue += $"---[��� {canvasIndex}]---\n������ ����\n\n";
            }
        }

        // ��ȭ ���� ������Ʈ
        dialogueText.text = fullDialogue;
    }
    // ���� ������ ������Ʈ
    public void UpdateScores()
    {
        var trainingFetcher = TrainingFetcher.instance;
        if (trainingFetcher != null)
        {
            // PlayerPrefs���� ���� �ҷ�����
            trainingFetcher.LoadScoresFromPlayerPrefs();

            // ���� �ؽ�Ʈ ������Ʈ
            float fluencyScore = trainingFetcher.GetFluencyScore();
            float pronunciationScore = trainingFetcher.GetPronunciationScore();
            float intonationScore = trainingFetcher.GetIntonationScore();

            fluencyScoreText.text = $"{fluencyScore:F1}��";
            pronunciationScoreText.text = $"{pronunciationScore:F1}��";
            intonationScoreText.text = $"{intonationScore:F1}��";

            // ������ ���� �׷��� �� fillAmount ������Ʈ (0~1 ����ȭ)
            fluencyBar.fillAmount = Mathf.Clamp01(fluencyScore / 100f);
            pronunciationBar.fillAmount = Mathf.Clamp01(pronunciationScore / 100f);
            intonationBar.fillAmount = Mathf.Clamp01(intonationScore / 100f);

            Debug.Log($"[UI ������Ʈ] ���� ��â��: {fluencyScore:F1}, ���� ������: {pronunciationScore:F1}, ���� ��Ȯ��: {intonationScore:F1}");
        }
        else
        {
            Debug.LogError("TrainingFetcher�� �������� �ʽ��ϴ�.");
            fluencyScoreText.text = "������ �ҷ��� �� �����ϴ�.";
            pronunciationScoreText.text = "������ �ҷ��� �� �����ϴ�.";
            intonationScoreText.text = "������ �ҷ��� �� �����ϴ�.";
        }
    }
}