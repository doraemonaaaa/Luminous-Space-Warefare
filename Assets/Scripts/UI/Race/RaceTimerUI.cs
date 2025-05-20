using TMPro;
using UnityEngine;

public class RaceTimerUI : MonoBehaviour
{
    public TextMeshProUGUI timerText;


    public void SetTime(string t)
    {
        timerText.text = t;
    }
}
