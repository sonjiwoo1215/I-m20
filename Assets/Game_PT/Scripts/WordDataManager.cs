using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class WordData
{
    public List<string> pt_word;          // ���õ� �ܾ�
    public List<string> pt_text;          // �νĵ� �ؽ�Ʈ ���
    public List<float> pt_score;          // ��Ȯ�� ���� ���
    public List<string> pt_feedback;      // �ǵ�� ���
}

public class WordDataManager : MonoBehaviour
{
    private static string jsonFilePath = Path.Combine(Application.persistentDataPath, "SaveData.json");

    // PlayerPrefs�� �����͸� �����ϴ� �޼���
    public static void SaveData(List<string> selectedWords, List<string> recognizedTexts, List<float> accuracyScores, List<string> feedbacks)
    {
        // WordData ��ü�� �����Ͽ� �ʿ��� �����͸� ���
        WordData wordData = new WordData
        {
            pt_word = selectedWords,
            pt_text = recognizedTexts,
            pt_score = accuracyScores,
            pt_feedback = feedbacks
        };

        // PlayerPrefs�� ������ ����
        PlayerPrefs.SetString("pt_word", JsonUtility.ToJson(new StringListWrapper { items = wordData.pt_word }));
        PlayerPrefs.SetString("pt_text", JsonUtility.ToJson(new StringListWrapper { items = wordData.pt_text }));
        PlayerPrefs.SetString("pt_score", JsonUtility.ToJson(new FloatListWrapper { items = wordData.pt_score }));
        PlayerPrefs.SetString("pt_feedback", JsonUtility.ToJson(new StringListWrapper { items = wordData.pt_feedback }));
        PlayerPrefs.Save();

        // PlayerPrefs�� ����� �����͸� ����� �޽����� ���
        Debug.Log("PlayerPrefs ���� ����:");
        Debug.Log("pt_word: " + PlayerPrefs.GetString("pt_word"));
        Debug.Log("pt_text: " + PlayerPrefs.GetString("pt_text"));
        Debug.Log("pt_score: " + PlayerPrefs.GetString("pt_score"));
        Debug.Log("pt_feedback: " + PlayerPrefs.GetString("pt_feedback"));

        // JSON �����͸� ���Ͽ� ����
        string json = JsonUtility.ToJson(wordData);
        File.WriteAllText(jsonFilePath, json);

        Debug.Log("���õ� �ܾ�, �νĵ� �ؽ�Ʈ, ��Ȯ�� ����, �ǵ�� JSON���� ���� �Ϸ�: " + json);
    }

    [System.Serializable]
    public class StringListWrapper
    {
        public List<string> items;
    }

    [System.Serializable]
    public class FloatListWrapper
    {
        public List<float> items;
    }

    // ���ÿ� ������ �����ϴ� �޼���
    public void SaveDataLocally(List<string> selectedWords, List<string> recognizedTexts, List<float> accuracyScores, List<string> feedbacks)
    {
        // ���ÿ� JSON ���� ����
        SaveData(selectedWords, recognizedTexts, accuracyScores, feedbacks);
    }
}