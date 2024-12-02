using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class TrainingFetcher : MonoBehaviour
{
    public static TrainingFetcher instance;

    // ȣ�� �Ʒ� ������
    public float length;                   // ȣ�� �Ʒ�: ��� ���ӽð�
    public float[] bt_levels = new float[6]; // ȣ�� �Ʒ�: ���� �� (6�� ����)
    public int bt_success_cnt;             // ȣ�� �Ʒ�: ǳ�� ����

    // ���� �Ʒ� ������
    public string[] pt_words = new string[3];           // ���� �Ʒ�: �Ʒ� �ܾ�
    public string[] pt_text = new string[3];            // ���� �Ʒ�: �Ƶ� ���� �ؽ�Ʈ
    public float[] pt_score = new float[3];             // ���� �Ʒ�: ��Ȯ�� ����
    public string[] pt_feedback = new string[3];        // ���� �Ʒ�: �ǵ��
    public AudioClip[] pt_teacher_voice = new AudioClip[3];  // ���� �Ʒ�: ������ �Ҹ�
    public AudioClip[] pt_child_voice = new AudioClip[3];    // ���� �Ʒ�: �Ƶ� �Ҹ�

    // ��â�� �Ʒ� ������
    public string[][] ft_texts = new string[3][];      // ��â�� �Ʒ�: �� ���� ��ȭ 5��
    // ��â�� �Ʒ� ���� ������
    public float fluencyScore; // ��â�� ����
    public float pronunciationScore; // ���� ����
    public float intonationScore; // ��� ����

    private string resourcesPath = "Game_PT/Resources/";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadBreathTrainingData();
        LoadPronunciationTrainingData();
        LoadFluencyTrainingData(); // ��â�� �Ʒ� ������ �ε�
        Debug.Log("[DEBUG] TrainingFetcher Start ȣ���");
    }

    // ȣ�� �Ʒ� ������ �ε�
    public void LoadBreathTrainingData()
    {
        length = PlayerPrefs.GetFloat("length", 0.0f);
        Debug.Log($"Loaded length: {length}");

        for (int i = 0; i < bt_levels.Length; i++)
        {
            bt_levels[i] = PlayerPrefs.GetFloat($"bt_level{i + 1}", 0.0f); // bt_level1 ~ bt_level6
            Debug.Log($"Loaded bt_level{i + 1}: {bt_levels[i]}");
        }

        bt_success_cnt = PlayerPrefs.GetInt("bt_success_cnt", 0);
        Debug.Log($"Loaded bt_success_cnt: {bt_success_cnt}");
    }

    // ���� �Ʒ� ������ �ε�
    public void LoadPronunciationTrainingData()
    {
        pt_words = LoadStringArrayFromPrefs("pt_word", new string[] { "", "", "" });
        pt_text = LoadStringArrayFromPrefs("pt_text", new string[] { "", "", "" });

        float[] rawScores = LoadFloatArrayFromPrefs("pt_score", new float[] { 0.0f, 0.0f, 0.0f });
        for (int i = 0; i < pt_score.Length; i++)
        {
            pt_score[i] = Mathf.Round(rawScores[i] * 10f) / 10f;
        }

        pt_feedback = LoadStringArrayFromPrefs("pt_feedback", new string[] { "", "", "" });

        for (int i = 0; i < 3; i++)
        {
            pt_teacher_voice[i] = Resources.Load<AudioClip>($"{resourcesPath}TeacherVoice_{i}") ?? null;
            pt_child_voice[i] = Resources.Load<AudioClip>($"{resourcesPath}ChildVoice_{i}") ?? null;
        }
    }

    // ��â�� �Ʒ� ������ �ε�
    public void LoadFluencyTrainingData()
    {
        // ft_texts �迭 �ʱ�ȭ
        if (ft_texts == null || ft_texts.Length != 3)
        {
            ft_texts = new string[3][];
            for (int i = 0; i < ft_texts.Length; i++)
            {
                ft_texts[i] = new string[5]; // ������ ��ȭ�� 5��
            }
        }

        for (int i = 1; i < 3; i++) // �� ���� ���� �ݺ�
        {
            string key = $"Canvas_{i}_Dialogue";
            string jsonData = PlayerPrefs.GetString(key, "");

            // [DEBUG] JSON ������ Ȯ��
            Debug.Log($"[DEBUG] Stored JSON Data for Canvas {i}: {jsonData}");

            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.Log($"[PlayerPrefs ������ Ȯ��]Canvas_{i}_Dialogue Not Found.");
                continue;
            }

            var dialogueData = JsonUtility.FromJson<DialogueData>(jsonData);
            if (dialogueData != null)
            {
                Debug.Log($"[PlayerPrefs ������ Ȯ��] Scene {i} Dialogue Loaded: {jsonData}");
                Debug.Log($"Questions: {string.Join(", ", dialogueData.Questions)}");
                Debug.Log($"Answers: {string.Join(", ", dialogueData.Answers)}");
                // ���⿡ �����͸� �ǵ�� �������� �����ϰų� ����
            }
        }

    }
    // TrainingFetcher���� �����͸� ������ UI ������Ʈ///
    public string[] GetDialogueForScene(int canvasIndex)
    {
        string key = $"Canvas_{canvasIndex}_Dialogue";
        string jsonData = PlayerPrefs.GetString(key, "");

        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.Log($"[PlayerPrefs �ǵ�� Ȯ��]Canvas_{canvasIndex}_Dialogue Not Found.");
            return null;
        }

        var dialogueData = JsonUtility.FromJson<DialogueData>(jsonData);
        if (dialogueData != null)
        {
            Debug.Log($"[PlayerPrefs �ǵ�� Ȯ��] Canvas {canvasIndex} Dialogue Found: {jsonData}");
            Debug.Log($"Questions: {string.Join(", ", dialogueData.Questions)}");
            Debug.Log($"Answers: {string.Join(", ", dialogueData.Answers)}");
            return dialogueData.Questions.ToArray();
        }

        return null;
    } ///

    public void LoadScoresFromPlayerPrefs()
    {
        // PlayerPrefs���� ������ �ҷ�����
        fluencyScore = PlayerPrefs.GetFloat("FluencyScore", 0f);
        pronunciationScore = PlayerPrefs.GetFloat("PronunciationScore", 0f);
        intonationScore = PlayerPrefs.GetFloat("IntonationScore", 0f);

        Debug.Log($"[PlayerPrefs ���� �ҷ�����] ��â��: {fluencyScore:F1}, ����: {pronunciationScore:F1}, ���: {intonationScore:F1}");
    }


    // Helper to load string array from PlayerPrefs
    private string[] LoadStringArrayFromPrefs(string key, string[] defaultValue)
    {
        string jsonData = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(jsonData)) return defaultValue;

        var wrapper = JsonUtility.FromJson<StringArrayWrapper>(jsonData);
        return wrapper?.items ?? defaultValue;
    }

    // Helper to load float array from PlayerPrefs
    private float[] LoadFloatArrayFromPrefs(string key, float[] defaultValue)
    {
        string jsonData = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(jsonData)) return defaultValue;

        var wrapper = JsonUtility.FromJson<FloatArrayWrapper>(jsonData);
        return wrapper?.items ?? defaultValue;
    }

    [System.Serializable]
    private class StringArrayWrapper { public string[] items; }

    [System.Serializable]
    private class FloatArrayWrapper { public float[] items; }

    [System.Serializable]
    public class DialogueData
    {
        public List<string> Questions; // ���� ����Ʈ
        public List<string> Answers;   // �亯 ����Ʈ

        public DialogueData(List<string> questions, List<string> answers)
        {
            Questions = questions;
            Answers = answers;
        }
    }

    // Getters for ȣ�� �Ʒ� ������
    public float GetAverageDuration() => length;
    public float[] GetVolumeLevels() => bt_levels;
    public int GetBalloonCount() => bt_success_cnt;

    // Getters for ���� �Ʒ� ������
    public string[] GetWords() => pt_words;
    public string[] GetChildWords() => pt_text;
    public float[] GetScores() => pt_score;
    public string[] GetFeedback() => pt_feedback;
    public AudioClip[] GetTeacherVoices() => pt_teacher_voice;
    public AudioClip[] GetChildVoices() => pt_child_voice;

    // Getters for ��â�� �Ʒ� ������
    public string[][] GetFluencyTexts() => ft_texts;
    // Getters for ���� ������
    public float GetFluencyScore() => fluencyScore;
    public float GetPronunciationScore() => pronunciationScore;
    public float GetIntonationScore() => intonationScore;
}