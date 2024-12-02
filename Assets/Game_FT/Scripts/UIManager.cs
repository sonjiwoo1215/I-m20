using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{
    public Text questionText;  // ���� �ؽ�Ʈ
    public Text choice1Text;   // ������ 1 �ؽ�Ʈ
    public Text choice2Text;   // ������ 2 �ؽ�Ʈ
    public Text choice3Text;   // ������ 3 �ؽ�Ʈ
    public Text feedbackText;  // ���� �ν� ��� �Ǵ� �ǵ�� �ؽ�Ʈ

    public GameObject completionPopup;  // �Ϸ� �˾� UI ��� �߰�
                                        // �� ��� ��ư Ŭ�� �� ȣ��� �޼���
    
    public GameObject FCanvas; // Fĵ���� ���� �߰�
    public GameObject currentCanvas; // ���� ĵ���� ���� �߰�

    // ������ �ؽ�Ʈ�� ���� ���� �Լ�
    public void HighlightChoice(int choiceIndex)
    {
        // ��� �������� ������ �ʱ�ȭ
        choice1Text.color = Color.black;
        choice2Text.color = Color.black;
        choice3Text.color = Color.black;

        // ���õ� �������� ���� ����
        switch (choiceIndex)
        {
            case 1:
                choice1Text.color = Color.blue;
                break;
            case 2:
                choice2Text.color = Color.blue;
                break;
            case 3:
                choice3Text.color = Color.blue;
                break;
        }
    }

    // ���� �� ������ �ؽ�Ʈ ���� �Լ�
    public void SetupQuestion(string question, string choice1, string choice2, string choice3)
    {
        // ������ �������� ����
        questionText.text = question;
        choice1Text.text = choice1;
        choice2Text.text = choice2;
        choice3Text.text = choice3;

        // �������� ������ ��� ���������� �ʱ�ȭ
        choice1Text.color = Color.black;
        choice2Text.color = Color.black;
        choice3Text.color = Color.black;

        feedbackText.text = "";  // �����ν� ��� �ʱ�ȭ
    }

    // STT ����� ȭ�鿡 ǥ���ϴ� �Լ�
    public void DisplaySTTResult(string result)
    {
        feedbackText.text = $"���� ���: {result}";
    }

    // ���� ���¸� Console�� ���
    public void LogSaveStatus(string statusMessage)
    {
        Debug.Log($"���� ����: {statusMessage}");
    }
    
    // Ȩ ��ư ������ training �������� ��ȯ
    public void OnHomeButtonClick() 
    {
        Debug.Log("Training Canvas�� ��ȯ");

        // ���� ���� ���� ������ �亯 ����
        var dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.SaveCurrentDialogueData(); // ���� ���� �����͸� ����
        }

        // ���� �����͸� TrainingFetcher�� �ٷ� �ε�
        TrainingFetcher.instance.LoadFluencyTrainingData(); // ��â������ ����.

        // FCanvas Ȱ��ȭ
        if (FCanvas != null)
        {
            FCanvas.SetActive(true);
        }

        // ���� ĵ���� ��Ȱ��ȭ
        if (currentCanvas != null)
        {
            currentCanvas.SetActive(false);
        }
    }

    // �ٽ��ϱ� ��ư ������ canvas1���� �����
    public void replayButtonClick()
    {
        ////����ä��� ���÷���
    }

    // �Ϸ� �˾� ǥ�� �Լ�
    public void ShowCompletionPopup()
    {
        completionPopup.SetActive(true);  // �Ϸ� �˾��� Ȱ��ȭ
    }

    // �Ϸ� �˾��� ����� �Լ�
    public void HideCompletionPopup()
    {
        completionPopup.SetActive(false);  // �Ϸ� �˾��� ��Ȱ��ȭ
    }

    // ������ ��ư�� Ŭ������ �� ȣ��Ǵ� �Լ�
    public void OnExitButtonClick()
    {
        Application.Quit();  // ���� ����
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // �����Ϳ��� �׽�Ʈ�� ���� �÷��� ��� ����
#endif
    }//////////////�� �ڵ�� ���� �ȿ�����

}