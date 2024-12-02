using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;

public class DialogueManager : BaseDialogueManager
{
    public FluencyDataSaver dataSaver; // FluencyDataSaver ���� �߰�//////////////////////////
    public GameObject Canvas1;
    public GameObject Canvas2;
    private string recordedFilePath; // ������ ���� ��� ����
    public SpeechRecognitionManager speechRecognitionManager; // SpeechRecognitionManager ���� �߰�

    protected override async Task SetupInitialDialogue()
    {
        await WaitForSpeechSynthesisInitialization();

        // ù ��° ���� ���� �� ����
        string question1 = "���� ������ � �� ����?";///////////////////
        dataSaver.SaveQuestion(question1); // ���� ����//////////////////////

        uiManager.SetupQuestion(
            question1,///////////////////////
            "�ذ� ¸¸�ϰ� ���� �� ����",
            "������ ������ ���Ŀ� �� �´�",
            "���� ��ħ�� �ʰ� �Ͼ��"
        );

        await speechSynthesisManager.SpeakText(question1);
        conversationStage = 1;
    }

    public override string GetExpectedResponse(string userResponse)
    {
        if (conversationStage == 1)
        {
            if (userResponse == "�ذ� ¸¸�ϰ� ���� �� ����")
            {
                dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����////////////////////////
                currentFlow = 1; // ���� ���� �帧 ����
                return userResponse;
            }
            else if (userResponse == "������ ������ ���Ŀ� �� �´�")
            {
                dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����////////////////////////
                currentFlow = 2; // ���� ���� �帧 ����
                return userResponse;
            }
            else if (userResponse == "���� ��ħ�� �ʰ� �Ͼ��")
            {
                dataSaver.SaveAnswer(userResponse, false); // Ʋ�� ��� �������� ����////////////////////
                return userResponse; // Ʋ�� ��
            }
        }
        else if (conversationStage == 2)
        {
            if (currentFlow == 1)
            {
                if (userResponse == "������ ���Ƽ� �ɾ�Ծ�")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return userResponse;
                }
                else if (userResponse == "�ʾ �� Ÿ�� �Ծ�")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return userResponse;
                }
                else if (userResponse == "������ ����ö �߿� ������ Ż�� ����̾�")
                {
                    dataSaver.SaveAnswer(userResponse, false); //  Ʋ�� ��� �������� ����
                    return userResponse; // Ʋ�� ������ ����
                }
            }
            else if (currentFlow == 2)
            {
                if (userResponse == "���� �� ��� ì���")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return userResponse;
                }
                else if (userResponse == "���ϰ� �������� ����� �� ì���")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return userResponse;
                }
                else if (userResponse == "�츮 ���� ����� ����")
                {
                    dataSaver.SaveAnswer(userResponse, false); // Ʋ�� ��� �������� ����
                    return userResponse; // Ʋ�� ������ ����
                }
            }
        }
        return "";
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
                    HandleSecondStageForWeatherQuestion(userResponse);
                else if (currentFlow == 2)
                    HandleSecondStageForUmbrellaQuestion(userResponse);
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
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupWalkingToSchoolQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            currentFlow = 2;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupUmbrellaQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            uiManager.questionText.text = "��ħ�� �ʰ� �Ͼ�� ���� ����! �׷��� ���� ������ ���� �˰� �;�. �ذ� ¸¸�ߴ���, �� �� �� ���Ҵ��� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForWeatherQuestion(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // TTS ���� �߰� ����///////////////////////////////
            string question3 = "������ ���� �� �ȴ� �� �ְ���!";///////////
            dataSaver.SaveQuestion(question3); // TTS ���� ����///////////////////

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            uiManager.questionText.text = "������ ����ö ������ ������� ����. �׷��� ���� �б��� ��� �Դ����� �ñ���. �ɾ �Ծ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);

            string question3 = "�ʾ��� �� ���� �ְ���!";//////////
            dataSaver.SaveQuestion(question3); // TTS ���� ����//////////////////

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForUmbrellaQuestion(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // question3 ����////////////////////////////////
            string question3 = "���߾�! �� ���� ����� �� �ʿ�����.";
            dataSaver.SaveQuestion(question3); // TTS ���� ����

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            string question3 = "�׷� �̵� �� ���� ���� ��� ���� ���� ����!";
            dataSaver.SaveQuestion(question3); // TTS ���� ����

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, EndDialogue));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            uiManager.questionText.text = "����� ���� �ֱ���! �׷��� �ʰ� ����� ì����� �ñ���.";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void SetupWalkingToSchoolQuestion()
    {
        string question2 = "�״ϱ�! ���� ����. ���� �б��� �ɾ �Ծ�?";////////////////
        dataSaver.SaveQuestion(question2); // �� ��° ���� ����///////////////////

        uiManager.SetupQuestion(
            question2,
            "������ ���Ƽ� �ɾ�Ծ�",
            "������ ����ö �߿� ������ Ż�� ����̾�",
            "�ʾ �� Ÿ�� �Ծ�"
        );
        await speechSynthesisManager.SpeakText(question2);
        conversationStage = 2;
    }

    private async void SetupUmbrellaQuestion()
    {
        string question2 = "���Ŀ� �� �´ٰ�? �� ��� ���� �Ծ�?";////////////////
        dataSaver.SaveQuestion(question2); // �� ��° ���� ����///////////////////

        uiManager.SetupQuestion(
            question2,
            "���� �� ��� ì���",
            "���ϰ� �������� ����� �� ì���",
            "�츮 ���� ����� ����"
        );
        await speechSynthesisManager.SpeakText(question2);
        conversationStage = 2;
    }

    private void EndDialogue()
    {
        // ���� ���� �� ����
        dataSaver.GenerateRandomScores(); // ��â��, ����, ��� ���� ����
        dataSaver.SaveScoresToPlayerPrefs(); // ���� ����

        int canvasIndex = (Canvas1.activeSelf) ? 1 : 2; // ���� Canvas�� �ε���
        dataSaver.SaveToPlayerPrefs(canvasIndex);
        dataSaver.ClearData(); // ���� ������ �ʱ�ȭ

        Canvas1.SetActive(false);
        Canvas2.SetActive(true);
    }

    private bool dialogueDataIsComplete()
    {
        return dataSaver.GetDialogueDataCount() >= 5; // ��: ���� 3���� �ùٸ� ��� 2���� ����Ǿ����� Ȯ��
    }


    protected string RemovePunctuation(string text)
    {
        return text.Replace("?", "").Replace(".", "").Replace("!", "");
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