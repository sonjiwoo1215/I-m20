using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class DialogueManager4 : BaseDialogueManager
{
    public FluencyDataSaver dataSaver;
    public SpeechRecognitionManager4 speechRecognitionManager; // SpeechRecognitionManager ���� �߰�
    public GameObject Canvas4;
    public GameObject Canvas5;
    protected override async Task SetupInitialDialogue()
    {
        await WaitForSpeechSynthesisInitialization();

        // ù ��° ���� ���� �� ����
        string question1 = "���� ���������� ��� �ð��� ������ �� ������?";///////////////////
        dataSaver.SaveQuestion(question1); // ���� ����//////////////////////

        // ù ��° ������ ������ ����
        uiManager.SetupQuestion(
            "���� ���������� ��� �ð��� ������ �� ������?",
            "�а� ���� å�� ��� ���� �͵� ���� �� ����",
            "�������� �ʹ� ������ �� ����", // Ʋ�� ������
            "���θ� �ϸ鼭 �ð��� ������ �͵� ���� �� ����"
        );

        // TTS�� ���� ���
        await speechSynthesisManager.SpeakText("���� ���������� ��� �ð��� ������ �� ������?");
        conversationStage = 1; // ù ��° ��ȭ �ܰ�
    }

    public override string GetExpectedResponse(string userResponse)
    {
        if (conversationStage == 1)
        {
            if (userResponse == "�а� ���� å�� ��� ���� �͵� ���� �� ����")
            {
                dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����/////
                currentFlow = 1;
                return "�а� ���� å�� ��� ���� �͵� ���� �� ����";
            }
            else if (userResponse == "���θ� �ϸ鼭 �ð��� ������ �͵� ���� �� ����")
            {
                dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����/////
                currentFlow = 2;
                return "���θ� �ϸ鼭 �ð��� ������ �͵� ���� �� ����";
            }
            else if (userResponse == "�������� �ʹ� ������ �� ����")
            {
                dataSaver.SaveAnswer(userResponse, false); // Ʋ�� ��� �������� ����/////
                return "�������� �ʹ� ������ �� ����"; // Ʋ�� ��
            }
        }
        else if (conversationStage == 2)
        {
            if (currentFlow == 1)
            {
                if (userResponse == "���� �ٰŸ��� �ִ� �Ҽ�å�� ������")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����/
                    return "���� �ٰŸ��� �ִ� �Ҽ�å�� ������";
                }
                else if (userResponse == "���� �������� ������")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����/
                    return "���� �������� ������";
                }
                else if (userResponse == "���� å�� ������ ���� ������")
                {
                    dataSaver.SaveAnswer(userResponse, false); // Ʋ�� ��� �������� ����/////
                    return userResponse; // Ʋ�� ��
                }
            }
            else if (currentFlow == 2)
            {
                if (userResponse == "���� ���� ��� ���߾�")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����/
                    return "���� ���� ��� ���߾�";
                }
                else if (userResponse == "���� å�� �б��� �ΰ� �ͼ� ���߾�")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����/
                    return "���� å�� �б��� �ΰ� �ͼ� ���߾�";
                }
                else if (userResponse == "���θ� �ϸ� ���� �� ����")
                {
                    dataSaver.SaveAnswer(userResponse, false); // Ʋ�� ��� �������� ����///
                    return userResponse; // Ʋ�� ��
                }
            }
        }
        return ""; // ������� ���� �亯
    }

    protected override void ProcessResponse(string userResponse)
    {
        switch (conversationStage)
        {
            case 1:
                HandleFirstStage(userResponse);
                break;
            case 2:
                if (currentFlow == 1)
                    HandleSecondStageForBookType(userResponse);
                else if (currentFlow == 2)
                    HandleSecondStageForStudy(userResponse);
                break;
        }
        // ��ȭ�� ����Ǿ��� ��� ������ ����///////////////////////////////////////
        if (conversationStage == 2 && dialogueDataIsComplete())
        {
            dataSaver.SaveToPlayerPrefs(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator WaitAndProcessNextQuestion(float delay, System.Action nextQuestionAction)
    {
        Debug.Log($"��� ��... {delay}�� �� ���� �������� �̵�");
        yield return new WaitForSeconds(delay);
        nextQuestionAction?.Invoke();
        Debug.Log("���� �������� �̵� �Ϸ�");
    }

    private async Task WaitForSpeechSynthesisInitialization()
    {
        while (!speechSynthesisManager.isInitialized)
        {
            Debug.Log("SpeechSynthesisManager �ʱ�ȭ ��� ��...");
            await Task.Delay(100);
        }
    }

    private bool IsAnswerCorrect(string response, string choice)
    {
        return response.Trim().Equals(choice, System.StringComparison.OrdinalIgnoreCase);
    }

    private async void HandleFirstStage(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            currentFlow = 1;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupBookQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            currentFlow = 2;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupStudyQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            uiManager.questionText.text = "�������� �����ؼ� ������ �� �Ǵ� �� ����! �׷��� ���� ���� ���������� ��� �ð��� ������ ����̾�.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForBookType(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // TTS ���� �߰� ����///////////////////////////////
            string question3 = "�Ҽ��� ������ �� �Ǳ� ����!�׷� ������ �Ҽ�å�� �����߰ڴ�!";///////////
            dataSaver.SaveQuestion(question3); // TTS ���� ����///////////////////

            uiManager.questionText.text = "�Ҽ��� ������ �� �Ǳ� ����! �׷� ������ �Ҽ�å�� �����߰ڴ�!";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            // TTS ���� �߰� ����///////////////////////////////
            string question3 = "�������� ����� �ι��� �˾ư��� �����!";///////////
            dataSaver.SaveQuestion(question3); // TTS ���� ����///////////////////

            uiManager.questionText.text = "�������� ����� �ι��� �˾ư��� �����!";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            uiManager.questionText.text = "å�� ������ ���� �����ϴٴ�! ������ ����ϴ±���! �׷��� ���� �ʰ� � ������ å�� �����ϴ��� �ñ���";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForStudy(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            // question3 ����////////////////////////////////
            string question3 = "���� ���� �ǰ��߱���! �ǰ��� �� ���� �ڴ°� �ְ���!";
            dataSaver.SaveQuestion(question3); // TTS ���� ����

            uiManager.questionText.text = "���� ���� �ǰ��߱���! �ǰ��� �� ���� �ڴ°� �ְ���!";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            // question3 ����////////////////////////////////
            string question3 = "å�� ì��� �� �����߱���.";
            dataSaver.SaveQuestion(question3); // TTS ���� ����

            uiManager.questionText.text = "å�� ì��� �� �����߱���.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            uiManager.questionText.text = "������ �� �ǰ��ϸ� �ڰ� �Ͼ�� �ϴ� �� �? �׷��� ���� �ʰ� �� ���θ� ���ߴ��� �ñ���.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void SetupBookQuestion()
    {
        string question2 = "�ʴ� � ������ å�� ������?";////////////////
        dataSaver.SaveQuestion(question2); // �� ��° ���� ����///

        uiManager.SetupQuestion(
            "�ʴ� � ������ å�� ������?",
            "���� �ٰŸ��� �ִ� �Ҽ�å�� ������",
            "���� �������� ������",
            "���� å�� ������ ���� ������" // Ʋ�� ������
        );
        await speechSynthesisManager.SpeakText("�ʴ� � ������ å�� ������?");
        conversationStage = 2;
    }

    private async void SetupStudyQuestion()
    {
        string question2 = "�׷��߰ڴ�! �� ���� ���� ���� �߾�?";////////////////
        dataSaver.SaveQuestion(question2); // �� ��° ���� ����//

        uiManager.SetupQuestion(
            "�׷��߰ڴ�! �� ���� ���� ���� �߾�?",
            "���θ� �ϸ� ���� �� ����", // Ʋ�� ������
            "���� ���� ��� ���߾�",
            "���� å�� �б��� �ΰ� �ͼ� ���߾�"
        );
        await speechSynthesisManager.SpeakText("�׷��߰ڴ�! �� ���� ���� ���� �߾�?");
        conversationStage = 2;
    }
    protected string RemovePunctuation(string text)
    {
        return text.Replace("?", "").Replace(".", "").Replace("!", "");
    }
    private void EndDialogue()
    {
        // ���� ���� �� ����
        dataSaver.GenerateRandomScores(); // ��â��, ����, ��� ���� ����
        dataSaver.SaveScoresToPlayerPrefs(); // ���� ����

        Debug.Log("��ȭ�� �������ϴ�.");
        if (Canvas4 != null && Canvas5 != null)
        {
            int canvasIndex = (Canvas4.activeSelf) ? 3 : 4; // ���� Canvas�� �ε���
            dataSaver.SaveToPlayerPrefs(canvasIndex);

            // ���� Canvas�� ��Ȱ��ȭ�ϰ�, ���ο� Canvas�� Ȱ��ȭ
            Canvas4.SetActive(false);  // ���� Canvas ��Ȱ��ȭ
            Canvas5.SetActive(true);  // Scene02_Board�� �ش��ϴ� Canvas Ȱ��ȭ
        }
        else
        {
            Debug.LogError("Canvas ������Ʈ�� �������� �ʾҽ��ϴ�. �ν����Ϳ��� mainCanvas �� boardCanvas�� Ȯ�����ּ���.");
        }
    }

    private bool dialogueDataIsComplete()
    {
        return dataSaver.GetDialogueDataCount() >= 5; // ��: ���� 3���� �ùٸ� ��� 2���� ����Ǿ����� Ȯ��
    }



    public void SaveCurrentDialogueData()
    {
        if (dataSaver != null)
        {
            // ���� ���� ���� (�̹� ����Ǿ����� Ȯ��)
            if (!dataSaver.IsQuestionSaved(uiManager.questionText.text))
            {
                dataSaver.SaveQuestion(uiManager.questionText.text);
            }

            // ���� ������� �亯 ����
            string userResponse = speechRecognitionManager?.GetRecognizedText();
            if (!string.IsNullOrEmpty(userResponse))
            {
                bool isCorrect = IsAnswerCorrect(userResponse, uiManager.choice1Text.text) ||
                                 IsAnswerCorrect(userResponse, uiManager.choice2Text.text);
                dataSaver.SaveAnswer(userResponse, isCorrect);
            }

            // PlayerPrefs�� ����
            dataSaver.SaveToPlayerPrefs(SceneManager.GetActiveScene().buildIndex);

            Debug.Log("���� ���� ���� �����͸� �����߽��ϴ�.");
        }
        else
        {
            Debug.LogWarning("DataSaver�� ������� �ʾҽ��ϴ�.");
        }
    }
}