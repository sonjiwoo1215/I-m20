using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("FT_PlayerPrefs �����Ͱ� ��� �����Ǿ����ϴ�.");
    }
}
