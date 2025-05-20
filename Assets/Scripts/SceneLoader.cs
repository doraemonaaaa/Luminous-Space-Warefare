using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByGameMode()
    {
        if (GameplayManager.Instance.GetGameMode() == GameMode.SingleGame)
        {
            switch (GameplayManager.Instance.GetGameplayMode())
            {
                case GameplayMode.Racing: LoadSceneByNameAsync("Racing Mode"); break;
                case GameplayMode.PlanetaryWar: LoadSceneByNameAsync("Planetary War Mode"); break;
            }
        }

    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneByNameAsync(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
