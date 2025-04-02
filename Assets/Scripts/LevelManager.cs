using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum LevelState
{
    Valid,
    Invalid,
    Toppling,
    Completed,
}

public class LevelManager : MonoBehaviour
{

    [SerializeField]
    private BoxCollider m_BoxCollider;

    [SerializeField]
    private List<ActivationPad> m_ActivationPads;

    [NonSerialized]
    public int? LevelIndex = null;

    [HideInInspector]
    public UnityEvent<LevelState> LevelStateUpdated;

    [HideInInspector]
    public static LevelManager Instance;

    // A legal level state is one where no domino has collided
    // with a domino that was not valid.
    private LevelState m_LevelState = LevelState.Valid;

    private List<CannonGravity> m_CannonBalls = new();

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

    public LevelState GetLevelState()
    {
        return m_LevelState;
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

    public void RegisterCannonBall(CannonGravity cannonball)
    {
        m_CannonBalls.Add(cannonball);
    }

    public void RegisterActivationPad(ActivationPad pad)
    {
        m_ActivationPads.Add(pad);
    }

    public void RegisterBeginTopple()
    {
        m_LevelState = LevelState.Toppling;
        LevelStateUpdated.Invoke(m_LevelState);
    }

    // Assumes the domino has been registered
    public void RegisterGoalToppled(GoalDomino goalDomino)
    {
        if (m_LevelState == LevelState.Completed)
        {
            throw new Exception("Goal was toppled but level already in completed state...");
        }

        m_GoalDominosToppled[goalDomino] = true;

        if (m_LevelState != LevelState.Toppling)
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

        for (int i = 0; i < m_CannonBalls.Count; i++)
        {
            Destroy(m_CannonBalls[i].gameObject);
        }
        m_CannonBalls.Clear();

        // Reset activation pads so cannons can fire, etc etc
        foreach (ActivationPad pad in m_ActivationPads)
            pad.Reset();
    }
}
