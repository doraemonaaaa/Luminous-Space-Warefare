using UnityEngine;


public class InteractionPromptData
{
    public string promptText;
    public float promptDuration;
    public Vector3 worldPosition;

    public InteractionPromptData(string prompt_text, float prompt_duration, Vector3 pos)
    {
        promptText = prompt_text;
        promptDuration = prompt_duration;
        worldPosition = pos;
    }
}
