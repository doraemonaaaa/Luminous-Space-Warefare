using UnityEngine;
using VSX.SpaceCombatKit;

public class SpawnManager : SingletonMono<SpawnManager>
{
    protected override bool isDontDestroyOnLoad => false;

    public void OnSpawn(Transform spawn_parent)
    {
        if (GameplayManager.Instance.GetGameplayMode() == GameplayMode.Racing)
        {
            SetRace(spawn_parent);
        }
    }

    private void SetRace(Transform spawn_parent)
    {
        var race_player = spawn_parent.GetComponentInChildren<RacePlayer>();
        var race_behavior = spawn_parent.GetComponentInChildren<SpaceshipRaceBehaviour>();
        race_player.raceBehaviour = race_behavior;
        race_behavior.racePlayer = race_player;
    }
}
