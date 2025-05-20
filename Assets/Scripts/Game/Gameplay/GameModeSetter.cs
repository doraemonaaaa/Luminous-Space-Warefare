using UnityEngine;

/// <summary>
/// ������ UI ��ť��������Ϸģʽ������/���ˣ����淨���ͣ�����/����ս����
/// </summary>
public class GameModeSetter : MonoBehaviour
{
    // ������Ϸģʽ��GameMode��
    public void SetToSingleGame()
    {
        GameplayManager.Instance.SetGameMode(GameMode.SingleGame);
    }

    public void SetToMultiplayerGame()
    {
        GameplayManager.Instance.SetGameMode(GameMode.MultiPlayerGame);
    }

    // �����淨ģʽ��GameplayMode��
    public void SetToRacingMode()
    {
        GameplayManager.Instance.SetGameplayMode(GameplayMode.Racing);
    }

    public void SetToPlanetaryWarMode()
    {
        GameplayManager.Instance.SetGameplayMode(GameplayMode.PlanetaryWar);
    }
}
