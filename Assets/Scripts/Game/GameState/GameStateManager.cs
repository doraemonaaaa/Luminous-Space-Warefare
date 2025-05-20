using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;



public enum PlayerState 
{ 
    None,
    Settings,
    Ready,
    Victory,
    Defeat
}


public class GameStateManager : SingletonMono<GameStateManager>
{
    protected override bool isDontDestroyOnLoad => false;

    public GameState CurrentState;

    public UnityEvent<GameState> onGameStateChanged;

    [SerializeField] private List<GameStateConfigSO> stateConfigs;

    private Coroutine stateChangeCoroutine;
    private bool isChangingState = false;


    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public void SetState(GameState newState)
    {
        if (isChangingState || newState == CurrentState) return;

        GameStateConfigSO config = GetConfig(newState);
        if (config != null && config.allowedFrom.Count > 0 && !config.allowedFrom.Contains(CurrentState))
        {
            Debug.LogWarning($"State change from {CurrentState} to {newState} not allowed.");
            return;
        }

        if (stateChangeCoroutine != null) StopCoroutine(stateChangeCoroutine);
        stateChangeCoroutine = StartCoroutine(ChangeStateCoroutine(newState, config));
    }

    private IEnumerator ChangeStateCoroutine(GameState newState, GameStateConfigSO config)
    {
        isChangingState = true;

        if (config != null && config.delayBeforeEnter > 0)
            yield return new WaitForSeconds(config.delayBeforeEnter);

        CurrentState = newState;

        if (config != null)
        {
            Time.timeScale = config.freezeTimeOnEnter ? 0 : 1;
            AudioListener.pause = config.freezeSound ? true : false;
            GlobalInputBlocker.InputBlocked = config.blockGlobalInput;
            Cursor.visible = config.showCursor;
            Cursor.lockState = config.lockCursor ? CursorLockMode.Locked : CursorLockMode.None;

            if (config.centerCursor)
                CenterCursor();
        }
        else
        {
            Debug.LogError("Game State Manager: config not found");
        }

            onGameStateChanged.Invoke(CurrentState);
        isChangingState = false;
    }


    private void CenterCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.None;
    }

    private GameStateConfigSO GetConfig(GameState type)
    {
        return stateConfigs.Find(c => c.state == type);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1;
        AudioListener.pause = false;
    }

}
