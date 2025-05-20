using UnityEngine;

public enum GameplayMode 
{
    None = 0,
    Racing = 1,
    PlanetaryWar = 2,
}

public enum GameMode 
{ 
    None = 0,
    SingleGame = 1,
    MultiPlayerGame = 2,
}


public class GameplayManager : SingletonMono<GameplayManager>
{
    protected override bool isDontDestroyOnLoad => false;

    [SerializeField] protected GameMode curGameMode;
    [SerializeField] protected GameplayMode curGameplayMode;

    [SerializeField] protected RaceGamePlay raceGameplay;

    public int requiredPlayerCount = 0;
    private int _playerCount = 0;  // contain player and AI

    public bool isEnterGame = true;

    private void Start()
    {
        if (isEnterGame)
        {
            GameStateManager.Instance.SetState(GameState.GameEnter);
            Debug.Log("进入游戏");
        }

    }

    public GameMode GetGameMode()
    {
        return curGameMode;
    }

    public GameplayMode GetGameplayMode()
    {
        return curGameplayMode;
    }

    public void SetGameMode(GameMode game_mode)
    {
        curGameMode = game_mode;
    }

    public void SetGameplayMode(GameplayMode game_mode)
    {
        curGameplayMode = game_mode;
    }

    public void SetPlayerNum(int player_num)
    {
        requiredPlayerCount = player_num;
    }

    public void RegisterPlayer<T>(GameplayMode mode, T player)
    {
        if(player is RacePlayer)
        {
            Debug.Log("GameplayManager: Registered a player");
            RacePlayer race_player = player as RacePlayer;
            _playerCount++;
            raceGameplay.RegisterPlayer(race_player);
            if(_playerCount >= requiredPlayerCount)
            {
                SetGameplayRule();
                GameStateManager.Instance.SetState(GameState.GameEnterCompleted);
            }
        }
    }

    public void SetGameplayRule()
    {
        Debug.Log("GameplayManager: Set GameplayRule");
        if (curGameplayMode == GameplayMode.Racing)
        {
            raceGameplay.SetGame();
        }
        else
        {

        }
    }

    public void GameOver<T>(T player)
    {
        if (curGameplayMode == GameplayMode.Racing)
        {
            if (player is RacePlayer)
            {
                RacePlayer race_player = player as RacePlayer;
                raceGameplay.RankPlayer(race_player);
                if (raceGameplay.CheckGameOver(race_player))
                {
                    GameStateManager.Instance.SetState(GameState.GameOver);
                    Debug.Log("游戏结束");
                }
            }
        }
    }

    public void OnGameStart()
    {
        if (curGameplayMode == GameplayMode.Racing)
        {
            raceGameplay.StartRaceTimer();
        }
    }

    public void OnGameOver()
    {
        if (curGameplayMode == GameplayMode.Racing)
        {
            raceGameplay.StopRaceTimer();
        }
    }

    public void OnPlayerDestroyed<T>(T player)
    {
        if (curGameplayMode == GameplayMode.Racing)
        {
            if (player is RacePlayer)
            {
                RacePlayer race_player = player as RacePlayer;
                raceGameplay.OnPlayerDestroyed(race_player);
            }
        }
    }
}
