using UnityEngine;
using System.Collections;

public class PanelController : MonoBehaviour
{
    public GameObject panel1;
    public GameObject panel2;
    public GameObject panel3;
    public GameObject button;
    private Animator panel1Animator;
    private int repeatCount = 0; // �ִϸ��̼� �ݺ� Ƚ��

    void Start()
    {
        // �ʱ⿡�� panel1�� Ȱ��ȭ�Ǿ� �־�� ��
        panel1.SetActive(true);
        panel2.SetActive(false);
        panel3.SetActive(false);
        button.SetActive(false);

        // Animator ������Ʈ ��������
        panel1Animator = panel1.GetComponent<Animator>();

        // �ִϸ��̼� �̸� Ȯ�� (����� �α�)
        Debug.Log("Panel1 Animation Name: " + panel1Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    }


    // panel1 �ִϸ��̼� �Ϸ� �� ȣ��� �Լ� (��ư Ȱ��ȭ ���� ���ŵ�)
    public void OnPanel1AnimationEnd()
    {
        // panel1�� Ȱ��ȭ�ϰ�
        panel1.SetActive(true);

        // panel2�� panel3�� Ȱ��ȭ�ϰ�
        panel2.SetActive(true);
        panel3.SetActive(true);
    }

    // ��ư Ŭ�� �� ȣ��� �Լ�
    public void OnButtonClick()
    {
        if (repeatCount < 2)
        {
            // ���� Ȱ��ȭ�� panel2�� panel3�� ��Ȱ��ȭ
            panel1.SetActive(false);
            panel2.SetActive(false);
            panel3.SetActive(false);
            button.SetActive(false);


            // panel1�� �ٽ� Ȱ��ȭ�ϰ� �ִϸ��̼� ���
            panel1.SetActive(true);
            panel1Animator.Play("Panel1Animation"); // "Panel1Animation"�� ���� �ִϸ��̼� �̸����� ����
        }
    }
}