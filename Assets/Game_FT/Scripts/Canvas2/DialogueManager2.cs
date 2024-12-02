using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;

public class DialogueManager2 : BaseDialogueManager
{
    public FluencyDataSaver dataSaver;
    public SpeechRecognitionManager2 speechRecognitionManager; // SpeechRecognitionManager ���� �߰�
    public GameObject Canvas2;
    public GameObject Canvas3;
    public GameObject Canvas4;

    protected override async Task SetupInitialDialogue()
    {
        await WaitForSpeechSynthesisInitialization();

        // ù ��° ���� ���� �� ����
        string question1 = "�ȳ�! ���� ��ħ�̾�! Ȥ�� ���� ���� ���� �������� �ƴ�?";///////////////////
        dataSaver.SaveQuestion(question1); // ���� ����//////////////////////

        // ù ��° ������ ������ ����
        uiManager.SetupQuestion(
            question1,
            "���� ĥ���þ�",  // Ʋ�� ������
            "ü���ð��̾�",
            "�ڽ��ð��̾�"
        );

        // ù ��° ������ TTS�� ���
        await speechSynthesisManager.SpeakText(question1);
        conversationStage = 1; // ù ��° ��ȭ �ܰ�
    }

    public override string GetExpectedResponse(string userResponse)
    {
        if (conversationStage == 1)
        {
            if (userResponse == "ü���ð��̾�")
            {
                dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����////////////////////////
                currentFlow = 1;
                return userResponse;
            }
            else if (userResponse == "�ڽ��ð��̾�")
            {
                dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����////////////////////////
                currentFlow = 2;
                return userResponse;
            }
            else if (userResponse == "���� ĥ���þ�")
            {
                dataSaver.SaveAnswer(userResponse, false); // Ʋ�� ��� �������� ����/////
                return userResponse; // Ʋ�� ��
            }
        }
        else if (conversationStage == 2)
        {
            if (currentFlow == 1)
            {
                if (userResponse == "�����ϰ� ü������ ���� �ΰ� �Ծ�")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return userResponse;
                }
                else if (userResponse == "ü���� ���� �Ծ�")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return userResponse;
                }
                else if (userResponse == "ü���ð��� ������ ���� �Ծ�� �� �� ����")
                {
                    dataSaver.SaveAnswer(userResponse, false); //  Ʋ�� ��� �������� ����
                    return userResponse; // Ʋ�� ��
                }
            }
            else if (currentFlow == 2)
            {
                if (userResponse == "���� ���� ������ ���ؼ� ���� ������ �ž�")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return userResponse;
                }
                else if (userResponse == "���� å �д� ���� ������")
                {
                    dataSaver.SaveAnswer(userResponse, false); //  Ʋ�� ��� �������� ����
                    return userResponse; // Ʋ�� ��
                }
                else if (userResponse == "���� ��������� �־ ���� ������ �ž�")
                {
                    dataSaver.SaveAnswer(userResponse, true); // �ùٸ� ��� ����
                    return userResponse;
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
                    HandleSecondStageForGymClothesQuestion(userResponse);
                else if (currentFlow == 2)
                    HandleSecondStageForStudyQuestion(userResponse);
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
        if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            currentFlow = 1;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupGymClothesQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            currentFlow = 2;
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, SetupStudyQuestion));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            uiManager.questionText.text = "���� 7���ñ���. �׷��� ���� ���� ���ð� ���� �ñ���. ���� ���� ���� �����̾�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForGymClothesQuestion(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // TTS ���� �߰� ����///////////////////////////////
            string question3 = "ü���� �� �� ���� �ִµ�, ���� ü���� �����ٰ�!";///////////
            dataSaver.SaveQuestion(question3); // TTS ���� ����///////////////////

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, () => EndDialogue("Scene03_Gym")));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            // TTS ���� �߰� ����///////////////////////////////
            string question3 = "ü������ ���� �Ա���!";
            dataSaver.SaveQuestion(question3); // TTS ���� ����///////////////////

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, () => EndDialogue("Scene03_Gym")));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            uiManager.questionText.text = "ü�� �ϴ� ���� ���� ���� ��������. �ٵ� ���� �װ� ü������ ���� �Դ����� �� �ñ���. ü���� ���� �Ծ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void HandleSecondStageForStudyQuestion(string userResponse)
    {
        if (IsAnswerCorrect(userResponse, uiManager.choice1Text.text))
        {
            uiManager.HighlightChoice(1);
            // question3 ����////////////////////////////////
            string question3 = "���� ���� �־�����. ���� �����ٰ�. ��������.";
            dataSaver.SaveQuestion(question3); // TTS ���� ����

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, () => EndDialogue("Scene04_Library")));
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice2Text.text))
        {
            uiManager.HighlightChoice(2);
            uiManager.questionText.text = "������ �����ϴ±���. �׷��� ���� ���� �ڽ��ð��� �װ� � ���� ���������� �ñ���. � �� ������ �ž�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
        else if (IsAnswerCorrect(userResponse, uiManager.choice3Text.text))
        {
            uiManager.HighlightChoice(3);
            string question3 = "���� ���� ���� �غ�� �ܾ� �ܿ��߰ڴ�.";
            dataSaver.SaveQuestion(question3); // TTS ���� ����

            uiManager.questionText.text = question3;
            await speechSynthesisManager.SpeakText(question3);
            StartCoroutine(WaitAndProcessNextQuestion(2.5f, () => EndDialogue("Scene04_Library")));
        }
        else
        {
            uiManager.questionText.text = "�� �� �����, �ٽ� �ѹ� �����ٷ�?";
            await speechSynthesisManager.SpeakText(uiManager.questionText.text);
        }
    }

    private async void SetupGymClothesQuestion()
    {
        string question2 = "ü���� ���� �Ծ�?";////////////////
        dataSaver.SaveQuestion(question2); // �� ��° ���� ����///////////////////

        uiManager.SetupQuestion(
            question2,
            "�����ϰ� ü������ ���� �ΰ� �Ծ�",
            "ü���� ���� �Ծ�",
            "ü���ð��� ������ ���� �Ծ�� �� �� ����" // Ʋ�� ������
        );
        await speechSynthesisManager.SpeakText(question2);
        conversationStage = 2;
    }

    private async void SetupStudyQuestion()
    {
        string question2 = "���� ���������� �ڽ��Ѵٰ� �߾���! � �� �����Ұž�?";////////////////
        dataSaver.SaveQuestion(question2); // �� ��° ���� ����///////////////////

        uiManager.SetupQuestion(
            question2,
            "���� ���� ������ ���ؼ� ���� ������ �ž�",
            "���� å �д� ���� ������", // Ʋ�� ������
            "���� ��������� �־ ���� ������ �ž�"
        );
        await speechSynthesisManager.SpeakText(question2);
        conversationStage = 2;
    }

    private void EndDialogue(string nextScene)
    {
        // ���� ���� �� ����
        dataSaver.GenerateRandomScores(); // ��â��, ����, ��� ���� ����
        dataSaver.SaveScoresToPlayerPrefs(); // ���� ����

        int canvasIndex = (Canvas2.activeSelf) ? 2 : (nextScene == "Scene03_Gym") ? 3 : 4;
        if (!dialogueDataIsComplete())
        {
            Debug.LogWarning($"Canvas {canvasIndex}: ��ȭ �����Ͱ� �������� �ʽ��ϴ�. ��ȯ�� �� �����ϴ�.");
            return;
        }

        dataSaver.SaveToPlayerPrefs(canvasIndex);
        dataSaver.ClearData(); // ���� ������ �ʱ�ȭ

        Canvas2.SetActive(false);
        if (nextScene == "Scene03_Gym")
        {
            Canvas3.SetActive(true);
        }
        else if (nextScene == "Scene04_Library")
        {
            Canvas4.SetActive(true);
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