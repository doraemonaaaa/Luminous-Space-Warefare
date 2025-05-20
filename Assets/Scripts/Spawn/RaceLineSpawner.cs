using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VSX.AI;
using VSX.Loadouts;
using VSX.SpaceCombatKit;
using VSX.UniversalVehicleCombat;
using VSX.VehicleCombatKits;

public class RaceLineSpawner : MonoBehaviour
{
    [Header("Setup")]
    public LoadoutVehicleSpawner playerSpawner;                        // ��Ҷ���
    [SerializeField]
    protected List<PilotedVehicleSpawn> spawners = new List<PilotedVehicleSpawn>();
    public int numberOfShips => spawners.Count ;                    // Ҫ���ɵ� AI �ɴ�����

    private void Start()
    {
        foreach(var s in spawners)
        {
            s.onSpawned.AddListener(() => SpawnManager.Instance.OnSpawn(s.transform));
            s.onSpawned.AddListener(() => s.Vehicle.onDestroyed.AddListener(() => GameplayManager.Instance.OnPlayerDestroyed<RacePlayer>(s.Vehicle.GetComponentInChildren<RacePlayer>())));  // ����AI �ݻٺ���Ϊ
        }
        playerSpawner.onSpawned.AddListener(SpawnShips);  // ������ɺ�����AI
        // ��Ҵݻٺ���Ϊ
        playerSpawner.onSpawned.AddListener(() => playerSpawner.spawnedVehicle.onDestroyed.AddListener(() => GameplayManager.Instance.OnPlayerDestroyed<RacePlayer>(playerSpawner.spawnedVehicle.GetComponentInChildren<RacePlayer>())));
        playerSpawner.onSpawned.AddListener(() => playerSpawner.spawnedVehicle.onDestroyed.AddListener(() => GameStateManager.Instance.SetState(GameState.GameOver)));

        playerSpawner.Spawn();
    }

    void SpawnShips()
    {
        for (int i = 0; i < numberOfShips; i++)
        {
            Spawn(i);
        }
    }

    public void Spawn(int listIndex)
    {
        spawners[listIndex].Spawn();
    }
}
