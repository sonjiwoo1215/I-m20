using UnityEngine;
using UnityEngine.UI;

public class WebcamDisplay : MonoBehaviour
{
    public RawImage rawImage; // RawImage UI ��Ҹ� Inspector���� �����ؾ� �մϴ�.

    void Start()
    {
        // ��ķ�� ���� ���� ȭ���� ǥ���� WebCamTexture�� �����մϴ�.
        WebCamTexture webcamTexture = new WebCamTexture();

        // RawImage UI ��ҿ� WebCamTexture�� �Ҵ��Ͽ� ȭ���� ǥ���մϴ�.
        rawImage.texture = webcamTexture;

        // ��ķ ȭ���� �����մϴ�.
        webcamTexture.Play();

        // ��ķ ȭ���� �¿� ������ ���� RawImage�� UV ��ǥ�� �����մϴ�.
        rawImage.uvRect = new Rect(1, 0, -1, 1); // UV ��ǥ�� �����Ͽ� �¿� ���� ȿ���� �ݴϴ�.

        float zoomFactor = 2.0f;
        Rect currentUVRect = rawImage.uvRect;
        float zoomWidth = currentUVRect.width / zoomFactor;
        float zoomHeight = currentUVRect.height / zoomFactor;
        rawImage.uvRect = new Rect(
            currentUVRect.x + (currentUVRect.width - zoomWidth) / 2,
            currentUVRect.y + (currentUVRect.height - zoomHeight) / 2,
            zoomWidth,
            zoomHeight
        );
    }
}