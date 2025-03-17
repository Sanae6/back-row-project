using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum LevelState
{
    Valid,
    Invalid,
    Completed,
}

public class LevelManager : MonoBehaviour
{
    [HideInInspector]
    public static LevelManager Instance;

    [SerializeField]
    private BoxCollider m_BoxCollider;

    [HideInInspector]
    public UnityEvent<LevelState> LevelStateUpdated;

    // A legal level state is one where no domino has collided
    // with a domino that was not valid.
    private LevelState m_LevelState = LevelState.Valid;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public bool PositionIsInStartingArea(Vector3 pos)
    {
        return m_BoxCollider.bounds.Contains(pos);
    }

    private Dictionary<GoalDomino, bool> m_GoalDominosToppled = new();

    public void RegisterGoalDomino(GoalDomino goalDomino)
    {
        m_GoalDominosToppled.Add(goalDomino, false);
    }

    // Assumes the domino has been registered
    public void RegisterGoalToppled(GoalDomino goalDomino)
    {
        if (m_LevelState == LevelState.Completed)
        {
            throw new Exception("Goal was toppled but level already in completed state...");
        }

        m_GoalDominosToppled[goalDomino] = true;

        if (!(m_LevelState == LevelState.Valid))
            return;

        // Every time we hear that a goal domino has been toppled,
        // we want to loop through our map and check if they've all
        // been toppled. if they have, do stuff
        foreach (var kv in m_GoalDominosToppled)
        {
            if (kv.Value == false)
                return;
        }

        Debug.Log("Level complete!!!");
        m_LevelState = LevelState.Completed;
        LevelStateUpdated.Invoke(m_LevelState);
    }

    public void RegisterInvalidTopple(Vector3 position)
    {
        if (m_LevelState != LevelState.Invalid)
        {
            m_LevelState = LevelState.Invalid;
            LevelStateUpdated.Invoke(m_LevelState);
        }
    }

    public void ResetLevel()
    {
        // Clear Dominos
        DominoManager.Instance.ClearDominos();

        // Reset the goal domino
        foreach (var key in m_GoalDominosToppled.Keys.ToList())
        {
            m_GoalDominosToppled[key] = false;
            key.Reset();
        }

        // Reset state
        m_LevelState = LevelState.Valid;
        LevelStateUpdated.Invoke(m_LevelState);
    }
}
