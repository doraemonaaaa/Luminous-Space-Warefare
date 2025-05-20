using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game Settings", order = 1)]
public class GameSettingsSO : ScriptableObject
{
    [Header("Display")]
    public bool showFPS = true;

    [Range(0, 1)]
    [Header("Audio")]
    public float globalVolume = 1f;

    [Range(0.2f, 2f)]
    [Header("Brightness")]
    public float brightness = 1f;
}
