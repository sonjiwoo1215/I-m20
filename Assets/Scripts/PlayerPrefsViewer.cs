using UnityEngine;

/// <summary>
/// ///////////////////���߿� �����ҰԿ�!!!!!!!!
/// </summary>
public class PlayerPrefsViewer : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.Label("PlayerPrefs Data:");

        for (int i = 0; i < 5; i++) // ��: 5���� Scene ������
        {
            string key = $"Scene_{i}_Dialogue";
            if (PlayerPrefs.HasKey(key))
            {
                GUILayout.Label($"{key}: {PlayerPrefs.GetString(key)}");
            }
            else
            {
                GUILayout.Label($"{key}: Not Found");
            }
        }
    }
}
