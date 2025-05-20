using UnityEngine;

public class GameSettingManager : SingletonMono<GameSettingManager>
{
    protected override bool isDontDestroyOnLoad => false;

    public GameSettingsSO gameSettingsSO;
}
