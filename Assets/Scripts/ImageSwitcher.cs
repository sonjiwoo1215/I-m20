using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageSwitcher : MonoBehaviour
{
    public Canvas informationCanvas; // Information Canvas ����
    public Image[] images; // ��ȯ�� �̹�����
    public float switchInterval = 3f; // �̹��� ��ȯ ���� (��)

    private int currentImageIndex = 0;
    private Coroutine switchRoutine;

    private void OnEnable()
    {
        // ĵ������ Ȱ��ȭ�� �� ��ȯ ����
        if (informationCanvas != null && informationCanvas.gameObject.activeSelf)
        {
            StartImageSwitching();
        }
    }

    private void OnDisable()
    {
        // ĵ������ ��Ȱ��ȭ�� �� ��ȯ �ߴ�
        StopImageSwitching();
    }

    private void StartImageSwitching()
    {
        if (images.Length > 0 && switchRoutine == null)
        {
            // ��� �̹����� �ʱ�ȭ (��Ȱ��ȭ)
            foreach (var img in images)
            {
                img.gameObject.SetActive(false);
            }

            // ù ��° �̹����� Ȱ��ȭ
            images[currentImageIndex].gameObject.SetActive(true);

            // ��ȯ �ڷ�ƾ ����
            switchRoutine = StartCoroutine(SwitchImages());
        }
    }

    private void StopImageSwitching()
    {
        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;
        }
    }

    private IEnumerator SwitchImages()
    {
        while (true)
        {
            yield return new WaitForSeconds(switchInterval);

            // ���� �̹����� ��Ȱ��ȭ
            images[currentImageIndex].gameObject.SetActive(false);

            // ���� �̹����� �ε��� ����
            currentImageIndex = (currentImageIndex + 1) % images.Length;

            // ���� �̹����� Ȱ��ȭ
            images[currentImageIndex].gameObject.SetActive(true);
        }
    }
}
