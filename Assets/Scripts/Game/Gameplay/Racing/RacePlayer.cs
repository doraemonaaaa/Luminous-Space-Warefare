using Michsky.UI.Heat;
using System.Collections;
using TMPro;
using Unity.Transforms;
using UnityEngine;
using VSX.AI;
using VSX.SpaceCombatKit;

public class RacePlayer : MonoBehaviour
{
    [SerializeField]
    public PatrolRoute raceRoute;
    public Transform curTargetWaypoint => raceRoute.Waypoints[_checkpointIndex % raceRoute.Waypoints.Count];
    public Transform nextTargetWaypoint => raceRoute.Waypoints[(_checkpointIndex + 1) % raceRoute.Waypoints.Count];

    private int _checkpointIndex;
    public int totalLaps = 3;
    public int currentLap = 0;
    private bool raceEnded = false;

    public GameObject playerParent;

    [Header("AI")]
    public bool isAI = false;
    public SpaceshipRaceBehaviour raceBehaviour;

    [Header("游戏结果")]
    public int RaceRank { get; set; } = -1;
    public int RaceScore { get; set; } = -1;
    public bool IsSuccess { get; set; } = false;

    private string _raceTime = "0:00.00";
    public string RaceTime
    {
        get => _raceTime;
        set
        {
            _raceTime = value;
            if (raceTimerUI != null)
                raceTimerUI.SetTime(value);
        }
    }


    [Header("UI")]
    public RaceTimerUI raceTimerUI;
    public LapUI lapUI;

    private void Start()
    {
        GameplayManager.Instance.RegisterPlayer<RacePlayer>(GameplayMode.Racing, this);
    }

    public void OnInit()
    {
        _checkpointIndex = 0;

        if (isAI)
        {
            
        }
        else
        {
            InitWaypointsForPlayer();
        }
    }


    private void InitWaypointsForPlayer()
    {
        if (raceRoute == null || raceRoute.Waypoints.Count == 0) return;

        HideAllOtherTargets();
        SetTargetDisplay(curTargetWaypoint, true);
    }

    private void HideAllOtherTargets()
    {
        Target[] allTargets = Object.FindObjectsByType<Target>(FindObjectsSortMode.None);
        foreach (var target in allTargets)
        {
            if (target.transform != curTargetWaypoint)
            {
                target.IsDisplay = false;
            }
        }
    }

    /// <summary>
    /// 更新目标检查点，防止越界，控制显示。
    /// </summary>
    protected void UpdateCheckPoint(bool add_index)
    {
        if (_checkpointIndex < 0 || _checkpointIndex >= raceRoute.Waypoints.Count) return;

        if (!isAI)
        {
            SetTargetDisplay(curTargetWaypoint, false);
        }

        if (add_index)
        {
            _checkpointIndex++;
            if (!isAI)
            {
                SetTargetDisplay(curTargetWaypoint, true);
            }
        }
    }

    public void OnReachWaypoint(Transform waypoint)
    {
        if (raceEnded) return;
        if (_checkpointIndex >= raceRoute.Waypoints.Count) _checkpointIndex = 0;
        if (raceRoute.Waypoints[_checkpointIndex] != waypoint) return;

        UpdateCheckPoint(true);

        if (_checkpointIndex >= raceRoute.Waypoints.Count)
        {
            currentLap++;
            if(lapUI != null)
            {
                lapUI.ShowLap(currentLap.ToString() + '/' + totalLaps.ToString());
            }
            if (currentLap >= totalLaps)
            {
                _checkpointIndex = 0;
                OnEndRace();
                return;
            }
            _checkpointIndex = 0;
        }

        ShowNotification(waypoint.gameObject.name);
        if (isAI)
        {
            raceBehaviour?.TryActivateBoost();
        }
    }

    private void OnEndRace()
    {
        Debug.Log($"Race Over! Total Laps: " + totalLaps);

        raceEnded = true;
        if (!isAI)
        {
            ShowNotification("Race Ended");
            SetTargetDisplay(curTargetWaypoint, false);
        }
        GameplayManager.Instance.GameOver(this);
    }

    private void SetTargetDisplay(Transform t, bool isVisible)
    {
        if (t == null) return;
        var target = t.GetComponent<Target>();
        if (target != null) target.IsDisplay = isVisible;
    }

    private void ShowNotification(string text)
    {
        NotificationManager nm = playerParent.GetComponentInChildren<NotificationManager>(true);
        if(nm != null)
        {
            nm.defaultState = NotificationManager.DefaultState.Minimized;
            nm.notificationText = text;
            nm.AnimateNotification();
        }
    }
}
