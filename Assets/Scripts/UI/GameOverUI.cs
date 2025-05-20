using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;



    public void SetRank(string t)
    {
        rankText.text = t;
    }

    public void SetScore(string t)
    {
        scoreText.text = t;
    }

    public void SetTime(string t)
    {
        timeText.text = t;
    }
}
