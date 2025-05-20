using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Gameplay<T> : MonoBehaviour
{
    public List<T> players;
    protected HashSet<RacePlayer> completedPlayers = new HashSet<RacePlayer>();

    public abstract void SetGame();
    public abstract bool CheckGameOver(T player);

    public virtual void RegisterPlayer(T player)
    {
        players.Add(player);
    }

    public virtual void OnPlayerDestroyed(T player)
    {

    }
}
