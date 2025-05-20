using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VSX.AI;
using VSX.SpaceCombatKit;

public class RaceGamePlay : Gameplay<RacePlayer>
{
    public int totalLaps;
    public PatrolRoute raceRoute;
    public int completedPlayer = 0;
    public string raceTime = "0:00.00";

    private List<RacePlayer> racingRank = new List<RacePlayer>();
    private float raceStartTime = -1f;
    private Coroutine raceTimerCoroutine = null;

    public override void SetGame()
    {
        //Player + AI
        foreach(var p in players)
        {
            p.totalLaps = totalLaps;
            p.raceRoute = raceRoute;
            p.OnInit();
        }
    }

    public override bool CheckGameOver(RacePlayer player)
    {
        // 避免重复计数
        if (!completedPlayers.Contains(player))
        {
            completedPlayers.Add(player);
            completedPlayer++;
        }

        return completedPlayer >= players.Count;
    }

    public void RankPlayer(RacePlayer rp)
    {
        if (!racingRank.Contains(rp))
        {
            racingRank.Add(rp);
            rp.RaceRank = racingRank.Count;
            rp.RaceTime = raceTime;
            rp.IsSuccess = true;
        }
    }

    public void StartRaceTimer()
    {
        raceStartTime = Time.time;  // 记录比赛开始的时间
        raceTimerCoroutine = StartCoroutine(RaceTimerCoroutine());
    }

    public void StopRaceTimer()
    {
        if(raceTimerCoroutine != null)
        {
            StopCoroutine(raceTimerCoroutine);
            raceTimerCoroutine = null;
        }
    }

    private IEnumerator RaceTimerCoroutine()
    {
        while (true)
        {
            if (raceStartTime < 0f)
            {
                raceTime = "0:00.00";
            }
            else
            {
                float elapsedTime = Time.time - raceStartTime;
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                float seconds = elapsedTime % 60f;
                raceTime = string.Format("{0}:{1:00.00}", minutes, seconds);
                foreach(var p in players)
                {
                    p.RaceTime = raceTime;
                }
            }

            yield return null;  // 每帧更新
        }
    }

    public override void OnPlayerDestroyed(RacePlayer player)
    {
        CheckGameOver(player);
    }
}
