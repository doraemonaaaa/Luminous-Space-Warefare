using Mirror.Examples.BilliardsPredicted;
using MyAudio;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using VSX.Vehicles;

public class GameFlowHandle : MonoBehaviour
{
    public bool isShowStartVideo = false;
    public bool isShowStartTimer = false;

    public Canvas videoPlayCanvas;
    public VideoPlayer startVideoPlayer;
    private bool _startVideoPlayerDone = false;

    public Canvas startTimerCanvas;
    public TextMeshProUGUI startTimerText;
    private Coroutine _lockShipCoroutine;

    public GameOverPanel gameOverPanel;
    public GameObject playerInput;


    public void OnGameEnter()
    {
        Debug.Log("播放開場動畫");
        if(isShowStartVideo) StartCoroutine(PlayVideoToEnd(startVideoPlayer, videoPlayCanvas));
        AudioManager.Instance.PlayMusic("fly me to the moon", true);
    }

    public void OnGameEnterCompleted()
    {
        if (isShowStartVideo)
        {
            StartCoroutine(StartTime());
        }
        else
        {
            _lockShipCoroutine = StartCoroutine(LockShipPositionCoroutine());  // 锁定飞船位置
            LockTime(false);
            GameStateManager.Instance.SetState(GameState.GameStart);
            UnlockShipPosition();
        }
    }

    public void OnGameStart()
    {
        if (isShowStartTimer)
        {
            StartCoroutine(GameStartRoutine());
        }
    }

    public void OnGameOver()
    {
        GameplayManager.Instance.OnGameOver();

        //UIControlActions[] uIControlActions = Object.FindObjectsByType<UIControlActions>(FindObjectsSortMode.None);
        //foreach (var uca in uIControlActions)
        //{
        //    uca.OpenPanel("Game Over Panel");
        //}
        playerInput.SetActive(false);
        RacePlayer[] race_players = Object.FindObjectsByType<RacePlayer>(FindObjectsSortMode.None);
        foreach(var rp in race_players)
        {
            //GameOverUI[] game_over_ui = rp.playerParent.GetComponentsInChildren<GameOverUI>(true);  // 未激活的物体也要寻找
            //foreach (var ui in game_over_ui)
            //{
            //    ui.SetRank(rp.RaceRank.ToString());
            //    ui.SetScore(rp.RaceScore.ToString());
            //    ui.SetTime(rp.RaceTime);
            //}
            if(rp.isAI == false)
            {
                GameOverUI[] game_over_ui = new GameOverUI[] {
                    gameOverPanel.successUI,
                    gameOverPanel.defeatUI
                };

                foreach (var ui in game_over_ui)
                {
                    ui.SetRank(rp.RaceRank.ToString());
                    ui.SetScore(rp.RaceScore.ToString());
                    ui.SetTime(rp.RaceTime);
                }
            }
        }

        gameOverPanel.gameObject.SetActive(true);
        gameOverPanel.ShowGameOverUI(true);

        Debug.Log("游戏结束");

    }

    public void HandleStateChange(GameState game_state)
    {
        switch (game_state) 
        {
            case GameState.GameEnter: OnGameEnter(); break;
            case GameState.GameEnterCompleted: OnGameEnterCompleted(); break;
            case GameState.GameStart: OnGameStart(); break;
            case GameState.GameOver: OnGameOver(); break;
        }

    }

    private IEnumerator PlayVideoToEnd(VideoPlayer video_palyer, Canvas video_palyer_canvas)
    {
        video_palyer_canvas.gameObject.SetActive(true);
        video_palyer.Play();
        // 等待视频播放开始
        while (!video_palyer.isPlaying)
        {
            yield return null;
        }
        // 等待视频播放结束
        while (video_palyer.isPlaying)
        {
            yield return null;
        }

        video_palyer_canvas.gameObject.SetActive(false);

        _startVideoPlayerDone = true;

    }

    private IEnumerator StartTime()
    {
        while (!_startVideoPlayerDone)
        {
            yield return null;
        }
        Debug.Log("开场动画播放完毕");
        yield return new WaitForSecondsRealtime(1f);  // 不依赖time scale
        // 视频播放完成后恢复时间流动
        _lockShipCoroutine = StartCoroutine(LockShipPositionCoroutine());  // 锁定飞船位置
        LockTime(false);
        Debug.Log("开始时间流逝");
        GameStateManager.Instance.SetState(GameState.GameStart);
    }

    private IEnumerator GameStartRoutine()
    {
        startTimerCanvas.gameObject.SetActive(true);

        for (int i = 5; i > 0; i--)
        {
            startTimerText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        startTimerText.text = "Go!";
        Debug.Log("Go!");
        yield return new WaitForSecondsRealtime(1f);

        startTimerCanvas.gameObject.SetActive(false);
        UnlockShipPosition();

        GameplayManager.Instance.OnGameStart();
    }

    public void LockTime(bool state)
    {
        if (state)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public IEnumerator LockShipPositionCoroutine()
    {
        // 查找所有的 Ship 对象
        Vehicle[] ships = Object.FindObjectsByType<Vehicle>(FindObjectsSortMode.None);

        Vector3[] initialPositions = new Vector3[ships.Length];
        for (int i = 0; i < ships.Length; i++)
        {
            initialPositions[i] = ships[i].transform.position;
        }
        while (true)  // 每帧执行
        {
            //Debug.Log("锁定飞船位移");
            for (int i = 0; i < ships.Length; i++)
            {
                ships[i].transform.position = initialPositions[i];
                Rigidbody rb = ships[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                }
            }
            // 等待下一帧继续执行
            yield return null;
        }
    }


    public void UnlockShipPosition()
    {
        // 停止锁定飞船位置
        if (_lockShipCoroutine != null)
        {
            StopCoroutine(_lockShipCoroutine);
            _lockShipCoroutine = null;
        }
    }
}
