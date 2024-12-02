using UnityEngine;
using UnityEngine.UI; // ��ư�� ����ϱ� ���� �ʿ�

public class FirstCanvas : MonoBehaviour
{
    public Button startButton; // ��ư�� �ν����Ϳ��� ����
    public PanelController panelController; // PanelController�� �ν����Ϳ��� ����

    void Start()
    {
        // ��ư Ŭ�� �� PanelController�� OnButtonClick ȣ��
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClick);
        }
    }

    void OnStartButtonClick()
    {
        if (panelController != null)
        {
            panelController.OnButtonClick(); // PanelController�� OnButtonClick �޼��� ȣ��
        }

        // FirstCanvas ��Ȱ��ȭ
        gameObject.SetActive(false); // �� ��ũ��Ʈ�� �پ� �ִ� ���� ������Ʈ�� ��Ȱ��ȭ
    }
}
