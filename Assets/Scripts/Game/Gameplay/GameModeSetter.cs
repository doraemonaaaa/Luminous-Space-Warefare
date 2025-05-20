using UnityEngine;

/// <summary>
/// 用于在 UI 按钮中设置游戏模式（单人/多人）和玩法类型（竞速/星球战争）
/// </summary>
public class GameModeSetter : MonoBehaviour
{
    // 设置游戏模式（GameMode）
    public void SetToSingleGame()
    {
        GameplayManager.Instance.SetGameMode(GameMode.SingleGame);
    }

    public void SetToMultiplayerGame()
    {
        GameplayManager.Instance.SetGameMode(GameMode.MultiPlayerGame);
    }

    // 设置玩法模式（GameplayMode）
    public void SetToRacingMode()
    {
        GameplayManager.Instance.SetGameplayMode(GameplayMode.Racing);
    }

    public void SetToPlanetaryWarMode()
    {
        GameplayManager.Instance.SetGameplayMode(GameplayMode.PlanetaryWar);
    }
}
