using TMPro;
using UnityEngine;

public class FPSUI : MonoBehaviour
{
    public TextMeshProUGUI FPSText;

    private float timer;
    private int frames;
    private float updateInterval = 1.0f; // 每1秒更新一次显示
    private float currentFPS;

    void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            currentFPS = frames / timer;
            UpdateFPS(currentFPS);

            // 重置计数器
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
