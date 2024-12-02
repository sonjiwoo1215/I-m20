using UnityEngine;
using System.Threading.Tasks;

public abstract class BaseDialogueManager : MonoBehaviour
{
    public UIManager uiManager;
    public SpeechSynthesisManager speechSynthesisManager;
    public PronunciationEvaluationManager pronunciationEvaluationManager;

    protected int conversationStage = 0; // ��ȭ �ܰ�
    protected int currentFlow = 0; // ��ȭ �帧 (�� ���� ���� �ٸ�)

    // ��ȭ�� ���� �ؽ�Ʈ�� ��ȯ�ϴ� �߻� �޼��� �߰�
    private string recordedFilePath; // ������ ���� ��� ����

    // ������ ���� ��θ� �����ϴ� �޼��� (����)
    public virtual void SetRecordedFilePath(string path)
    {
        recordedFilePath = path;
        Debug.Log($"������ ���� ��ΰ� �����Ǿ����ϴ�: {path}");
    }
    protected abstract Task SetupInitialDialogue(); // �� ������ ������ �ʱ� ����
    protected abstract void ProcessResponse(string userResponse); // ��ȭ ���� ó��

    // �� ������ ����Ǵ� �ؽ�Ʈ�� ��ȯ�ϴ� �߻� �޼���
    public abstract string GetExpectedResponse(string userResponse);

    protected virtual async void Start()
    {
        await SetupInitialDialogue();
    }

    public async void OnUserResponse(string userResponse)
    {
        userResponse = RemovePunctuation(userResponse);
        uiManager.DisplaySTTResult(userResponse);

        string expectedText = GetExpectedResponse(userResponse); // �� ������ ������ ���� �ؽ�Ʈ ��������

        if (!string.IsNullOrEmpty(expectedText) && userResponse.Equals(expectedText, System.StringComparison.OrdinalIgnoreCase))
        {
            // ���� �򰡸� ���� ���� ���� ���
            if (!string.IsNullOrEmpty(recordedFilePath))
            {
                pronunciationEvaluationManager.EvaluatePronunciation(recordedFilePath, expectedText);
                ClearRecordedFilePath(); // ���� �� �Ϸ� �� ���� ��� �ʱ�ȭ
            }

            pronunciationEvaluationManager.SaveUserResponse(expectedText, userResponse);
            Debug.Log("���� �� �����Ͱ� ����Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning($"���� ����ġ: ���� = {expectedText}, ���� = {userResponse}. ������� ����.");
        }

        ProcessResponse(userResponse);
    }
    private void ClearRecordedFilePath()
    {
        recordedFilePath = string.Empty;
        Debug.Log("������ ���� ��ΰ� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    protected string RemovePunctuation(string text)
    {
        return text.Replace("?", "").Replace(".", "").Replace("!", "");
    }
}