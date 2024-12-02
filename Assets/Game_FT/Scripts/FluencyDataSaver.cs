using System.Collections.Generic;
using UnityEngine;

public class FluencyDataSaver : MonoBehaviour
{
    private List<string> savedQuestions = new List<string>(); // ����ڰ� ������ ���� �����͸� ��� ����Ʈ
    private List<string> savedAnswers = new List<string>(); // ����ڰ� ������ ��� �����͸� ��� ����Ʈ

    // ���� ������ ������ ���� �߰�
    public float fluencyScore; // ��â�� ����
    public float pronunciationScore; // ���� ����
    public float intonationScore; // ��� ����

    public void ClearData()
    {
        savedQuestions.Clear();
        savedAnswers.Clear();
        fluencyScore = 0;
        pronunciationScore = 0;
        intonationScore = 0;
        Debug.Log("����, �亯 �� ���� �����Ͱ� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    public void SaveQuestion(string question)  // ���޹��� question�� savedquestions ����Ʈ�� ����
    {
        if (!savedQuestions.Contains(question))         // �̹� ����Ʈ�� �ִ� �������� Ȯ��(�ߺ��˻�)
        {
            savedQuestions.Add(question);        // ���� ����
            Debug.Log($"���� ����: {question}");
        }
        else
        {
            Debug.LogWarning($"�ߺ��� ������ ������� �ʾҽ��ϴ�: {question}");
        }
    }

    public void SaveAnswer(string answer, bool isCorrect) // ����� saveAnswer����Ʈ�� ����
    {
        if (isCorrect && !savedAnswers.Contains(answer)) // �ùٸ� ��丸 ����
        {
            savedAnswers.Add(answer);
            Debug.Log($"�亯 ����: {answer}");
        }
        else if (!isCorrect)
        {
            Debug.LogWarning($"�ùٸ��� ���� �亯�� ������� �ʽ��ϴ�: {answer}");
        }
    }

    public bool IsQuestionSaved(string question) => savedQuestions.Contains(question); // Ư�� ������ ����ƴ��� Ȯ��
    public bool IsAnswerSaved(string answer) => savedAnswers.Contains(answer); // Ư�� ����� ����ƴ��� Ȯ��

    public void GenerateRandomScores()
    {
        fluencyScore = Random.Range(85f, 100f); // ��â�� ����: 85 ~ 99
        pronunciationScore = Random.Range(85f, 100f); // ���� ����: 85 ~ 99
        intonationScore = Random.Range(80f, 100f); // ��� ����: 80 ~ 99

        Debug.Log($"[���� ���� ����] ��â��: {fluencyScore:F1}, ����: {pronunciationScore:F1}, ���: {intonationScore:F1}");
    }

    public void SaveScoresToPlayerPrefs()
    {
        // ���� ������ PlayerPrefs�� ����
        PlayerPrefs.SetFloat("FluencyScore", fluencyScore);
        PlayerPrefs.SetFloat("PronunciationScore", pronunciationScore);
        PlayerPrefs.SetFloat("IntonationScore", intonationScore);
        PlayerPrefs.Save();

        Debug.Log($"[PlayerPrefs ���� Ȯ��] ��â��: {fluencyScore:F1}, ����: {pronunciationScore:F1}, ���: {intonationScore:F1}");
    }

    public void LoadScoresFromPlayerPrefs()
    {
        // PlayerPrefs���� ������ �ҷ�����
        fluencyScore = PlayerPrefs.GetFloat("FluencyScore", 0f);
        pronunciationScore = PlayerPrefs.GetFloat("PronunciationScore", 0f);
        intonationScore = PlayerPrefs.GetFloat("IntonationScore", 0f);

        Debug.Log($"[PlayerPrefs �ҷ�����] ��â��: {fluencyScore:F1}, ����: {pronunciationScore:F1}, ���: {intonationScore:F1}");
    }

    public void SaveToPlayerPrefs(int canvasIndex)    // savedQuestions�� savedanswers �����͸� playerprefs�� json �������� ����
    {
        string key = $"Canvas_{canvasIndex}_Dialogue";
        string jsonData = JsonUtility.ToJson(new DialogueData(savedQuestions, savedAnswers));
        PlayerPrefs.SetString(key, jsonData);  // ������ ����
        PlayerPrefs.Save();
        // Debug �޽��� 
        Debug.Log($"[PlayerPrefs ���� Ȯ��] Scene {canvasIndex} ������ ���� �Ϸ�: {jsonData}");
    }

    public void LoadFluencyTrainingData(int canvasIndex) // playerprefs���� json �����͸� �ҷ��� savedquestions�� savedanswers ����Ʈ�� ����
    {
        string key = $"Canvas_{canvasIndex}_Dialogue";// �� Ű�� ������ �˻�
        string jsonData = PlayerPrefs.GetString(key, "");
        if (!string.IsNullOrEmpty(jsonData))
        {
            DialogueData loadedData = JsonUtility.FromJson<DialogueData>(jsonData);
            savedQuestions = loadedData.Questions ?? new List<string>();
            savedAnswers = loadedData.Answers ?? new List<string>();

            Debug.Log($"[PlayerPrefs �ҷ����� Ȯ��]Canvas {canvasIndex} ������ ���� �Ϸ�: {jsonData}");
        }
        else
        {
            Debug.Log($"[PlayerPrefs �ҷ����� ����]Canvas {canvasIndex}�� ����� �����Ͱ� �����ϴ�.");
        }
    }

    public int GetDialogueDataCount() // ����� ������ ����� �� ������ ��ȯ
    {
        return savedQuestions.Count + savedAnswers.Count;
    }
    // ���� �����͸� ��ȯ�ϴ� �޼��� �߰�
    public string[] GetSavedQuestions()
    {
        return savedQuestions.ToArray();
    }

    // �亯 �����͸� ��ȯ�ϴ� �޼��� �߰�
    public string[] GetSavedAnswers()
    {
        return savedAnswers.ToArray();
    }

    [System.Serializable]
    private class DialogueData
    {
        public List<string> Questions;
        public List<string> Answers;

        public DialogueData(List<string> questions, List<string> answers)
        {
            Questions = questions ?? new List<string>();
            Answers = answers ?? new List<string>();
        }
    }
}