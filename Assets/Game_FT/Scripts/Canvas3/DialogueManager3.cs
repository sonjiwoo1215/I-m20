using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class DialogueManager3 : BaseDialogueManager
{
    public FluencyDataSaver dataSaver;
    public SpeechRecognitionManager3 speechRecognitionManager; // SpeechRecognitionManager ���� �߰�
    public GameObject Canvas3;
    public GameObject Canvas5;
    protected override async Task SetupInitialDialogue()
    {
        await WaitForSpeechSynthesisInitialization();

        // ù ��° ������ ������ ����
        string question1 = "���� ü�� �ð��� � ��� �ϸ� �������?";///////////////////
        dataSaver.SaveQuestion(question1); // ���� ����//////////////////////

        uiManager.SetupQuestion(
            question1,
            "���� �̿��� ��� �ϸ� ���� �� ���ƿ�",
            "�׸��� �׸��� ���� �� ���ƿ�", // Ʋ�� ������
            "���� ������ �Բ� �� �� �ִ� ��� �ϸ� ���� �� ���ƿ�"
        );

        // ù ��° ������ TTS�� ���
        await speechSynthesisManager.SpeakText(question1);
        conversationStage = 1; // ù ��° ��ȭ �ܰ�
    }

    public override string GetExpectedResponse(string userResponse)
    {
        if (conversationStage == 1)
        {
            if (userResponse == "���� �̿��� ��� �ϸ� ���� �� ���ƿ�")
            {
                dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����/////////////
                currentFlow = 1;
                return "���� �̿��� ��� �ϸ� ���� �� ���ƿ�";
            }
            else if (userResponse == "���� ������ �Բ� �� �� �ִ� ��� �ϸ� ���� �� ���ƿ�")
            {
                dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����/////////////
                currentFlow = 2;
                return "���� ������ �Բ� �� �� �ִ� ��� �ϸ� ���� �� ���ƿ�";
            }
            else if (userResponse == "�׸��� �׸��� ���� �� ���ƿ�")
            {
                dataSaver.SaveAnswer(userResponse, false); //  Ʋ�� ��� �������� ����
                return "�׸��� �׸��� ���� �� ���ƿ�"; // Ʋ�� ��
            }
        }
        else if (conversationStage == 2)
        {
            if (currentFlow == 1)
            {
                if (userResponse == "���� �豸�� ������� �� ���ƿ�")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return "���� �豸�� ������� �� ���ƿ�";
                }
                else if (userResponse == "���� �Ǳ��� �ϰ� �;��")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return "���� �Ǳ��� �ϰ� �;��";
                }
                else if (userResponse == "���� ������ ��� �;��")
                {
                    dataSaver.SaveAnswer(userResponse, false); //  Ʋ�� ��� �������� ����
                    return userResponse; // Ʋ�� ��
                }
            }
            else if (currentFlow == 2)
            {
                if (userResponse == "�̾�޸��⸦ �ϸ� ���� �� ���ƿ�")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return "�̾�޸��⸦ �ϸ� ���� �� ���ƿ�";
                }
                else if (userResponse == "�������� �⸦ �� �ִ� �Ǳ��� �ϸ� ���� �� ���ƿ�")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return "�������� �⸦ �� �ִ� �Ǳ��� �ϸ� ���� �� ���ƿ�";
                }
                else if (userResponse == "�� ���� �丮�� �ϸ� ���� �� ���ƿ�")
                {
                    dataSaver.SaveAnswer(userResponse, false); //  Ʋ�� ��� �������� ����
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
                    HandleSecondStageForBallGame(userResponse);
                else if (currentFlow == 2)
                    HandleSecondStageForTeamwork(userResponse);
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
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupBallQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            uiManager.questionText.text = "�׸� �׸��� �͵� �������! ������ ������ ü�� �ð��̾�. � ��� �ϸ� ������?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            currentFlow = 2;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupTeamworkQuestion));
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForBallGame(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // TTS ���� �߰� ����///////////////////////////////
            string question3 = "�豸�� ���� �������! ���� �豸 �⺻�⸦ �����غ���!";///////////
            dataSaver.SaveQuestion(question3); // TTS ���� ����///////////////////

            uiManager.questionText.text = "�豸�� ���� �������! ���� �豸 �⺻�⸦ �����غ���!";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            uiManager.questionText.text = "������ �����鼭 ��� �ϴ� �͵� ������, � ��� �ϰ� ������ ������.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            // TTS ���� �߰� ����///////////////////////////////
            string question3 = "�Ǳ��� ���� �ų��� ��������! ���� ������ ������ �����غ���?";///////////
            dataSaver.SaveQuestion(question3); // TTS ���� ����///////////////////

            uiManager.questionText.text = "�Ǳ��� ���� �ų��� ��������! ���� ������ ������ �����غ���?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForTeamwork(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            // question3 ����////////////////////////////////
            string question3 = "�̾�޸���� �������� �⸣�� ���� �������! �׷� ���� ��������?";
            dataSaver.SaveQuestion(question3); // TTS ���� ����

            uiManager.questionText.text = "�̾�޸���� �������� �⸣�� ���� �������! �׷� ���� ��������?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            // question3 ����////////////////////////////////
            string question3 = "�Ǳ��� �������� �⸣�⿡ ���� �����! ���� ��̰� �غ���.";
            dataSaver.SaveQuestion(question3); // TTS ���� ����

            uiManager.questionText.text = "�Ǳ��� �������� �⸣�⿡ ���� �����! ���� ��̰� �غ���.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            uiManager.questionText.text = "�丮�� �������� �⸣����, ������ ü���ð��̾�. � ��� ���� �����غ�.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void SetupBallQuestion()
    {
        string question2 = "���� �����̾�! �׷� ���� �̿��� � �߿� � ��� �ϰ� �;�?";////////////////
        dataSaver.SaveQuestion(question2); // �� ��° ���� ����////////////////

        uiManager.SetupQuestion(
            "���� �����̾�! �׷� ���� �̿��� � �߿� � ��� �ϰ� �;�?",
            "���� �豸�� ������� �� ���ƿ�",
            "���� �Ǳ��� �ϰ� �;��",
            "���� ������ ��� �;��" // Ʋ�� ������
        );
        await speechSynthesisManager.SpeakText("���� �����̾�! �׷� ���� �̿��� � �߿� � ��� �ϰ� �;�?");
        conversationStage = 2;
    }

    private async void SetupTeamworkQuestion()
    {
        string question2 = "���� ������ �Բ� �ϴ� � �߿� �� �ϸ� ������?";////////////////
        dataSaver.SaveQuestion(question2); // �� ��° ���� ����////////////////

        uiManager.SetupQuestion(
            "���� ������ �Բ� �ϴ� � �߿� �� �ϸ� ������?",
            "�� ���� �丮�� �ϸ� ���� �� ���ƿ�", // Ʋ�� ������
            "�̾�޸��⸦ �ϸ� ���� �� ���ƿ�",
            "�������� �⸦ �� �ִ� �Ǳ��� �ϸ� ���� �� ���ƿ�"
        );
        await speechSynthesisManager.SpeakText("���� ������ �Բ� �ϴ� � �߿� �� �ϸ� ������?");
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
        if (Canvas3 != null && Canvas5 != null)
        {
            int canvasIndex = (Canvas3.activeSelf) ? 3 : 4; // ���� Canvas�� �ε���
            dataSaver.SaveToPlayerPrefs(canvasIndex);
            

            // ���� Canvas�� ��Ȱ��ȭ�ϰ�, ���ο� Canvas�� Ȱ��ȭ
            Canvas3.SetActive(false);  // ���� Canvas ��Ȱ��ȭ
            Canvas5.SetActive(true);  // Scene05�� �ش��ϴ� Canvas Ȱ��ȭ
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