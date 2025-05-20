using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public enum GameState
{
    MainMenu,
    Loading,
    Ready,
    GameEnter,
    Playing,
    Paused,
    Victory,
    Defeat,
    Result,
    Settings,
    Cutscene,
    Exit,
    GameEnterCompleted,
    GameOver,
    GameStart
}

[CreateAssetMenu(fileName = "NewGameStateConfig", menuName = "Game/Game State Config")]
public class GameStateConfigSO : ScriptableObject
{
    public GameState state;
    public bool freezeTimeOnEnter;
    public bool freezeSound;
    public bool showCursor;
    public bool lockCursor;
    public bool centerCursor;
    public bool blockGlobalInput;
    public float delayBeforeEnter;
    public List<GameState> allowedFrom;
}
