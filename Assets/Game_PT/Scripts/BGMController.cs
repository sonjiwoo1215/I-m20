using UnityEngine;

public class BGMController : MonoBehaviour
{
    private AudioSource audioSource;

    // �г� ������Ʈ (Inspector���� panel2, panel3�� �Ҵ��մϴ�)
    public GameObject panel2;
    public GameObject panel3;

    // ������ ��� ������ ���θ� üũ�� ����
    private bool isRecordingPlaying = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();  // ���� ���� �� ������� ���
        }
    }

    void Update()
    {
        // ������ ��� ���̰ų�, panel2 �Ǵ� panel3�� Ȱ��ȭ�Ǿ��� �� ������� �Ͻ� ����
        if (isRecordingPlaying || (panel2 != null && panel2.activeSelf) || (panel3 != null && panel3.activeSelf))
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();  // ������� �Ͻ� ����
            }
        }
        else
        {
            // �г��� ��Ȱ��ȭ�ǰ� ������ ������ ������� �簳
            if (!audioSource.isPlaying)
            {
                audioSource.UnPause();  // ������� �簳
            }
        }
    }

    // �ܺο��� ȣ���� �� �ֵ��� isRecordingPlaying�� �����ϴ� �޼���
    public void SetRecordingPlayingStatus(bool isPlaying)
    {
        isRecordingPlaying = isPlaying;
    }
}
