using TMPro;
using UnityEngine;

public class FPSUI : MonoBehaviour
{
    public TextMeshProUGUI FPSText;

    private float timer;
    private int frames;
    private float updateInterval = 1.0f; // ÿ1�����һ����ʾ
    private float currentFPS;

    void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            currentFPS = frames / timer;
            UpdateFPS(currentFPS);

            // ���ü�����
            frames = 0;
            timer = 0f;
        }
    }

    private void UpdateFPS(float fps)
    {
        if (FPSText != null)
        {
            FPSText.text = $"FPS: {Mathf.RoundToInt(fps)}";
        }
    }
}
