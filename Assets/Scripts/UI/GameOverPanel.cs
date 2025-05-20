using TMPro;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    public GameOverUI successUI;
    public GameOverUI defeatUI;

    private void Awake()
    {
        CloseGameOverUI();
    }

    public void CloseGameOverUI()
    {
        successUI.gameObject.SetActive(false);
        defeatUI.gameObject.SetActive(false);
    }

    public void ShowGameOverUI(bool is_success)
    {
        if (is_success)
        {
            successUI.gameObject.SetActive(true);
            defeatUI.gameObject.SetActive(false);
        }
        else
        {
            successUI.gameObject.SetActive(false);
            defeatUI.gameObject.SetActive(true);
        }
    }
}
