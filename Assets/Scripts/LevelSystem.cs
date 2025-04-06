using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class LevelSystem : MonoBehaviour
{
    [SerializeField] [FormerlySerializedAs("menuLevelPrefab")]
    private LevelManager menuPrefab;

    public List<LevelManager> levelPrefabs = new();
    [SerializeField] private int transitionSeconds = 1;
    private const float LowestPoint = -1.5f;

    [NonSerialized] public LevelManager currentLevel;
    [NonSerialized] private bool inTransition = false;
    [NonSerialized] public static LevelSystem Instance;
    [NonSerialized] public UnityEvent<LevelManager> LevelStarted = new();

    private void Awake()
    {
        Instance = this;
        for (var i = 0; i < levelPrefabs.Count; i++) levelPrefabs[i].LevelIndex = i;
    }

    void Start()
    {
        currentLevel = Instantiate(menuPrefab);
        currentLevel.gameObject.SetActive(true);
    }

    public LevelManager GetNextPrefab(LevelManager current)
    {
        if (current.LevelIndex is { } index && index < levelPrefabs.Count)
        {
            return levelPrefabs[index];
        }

        return menuPrefab;
    }

    public void SwitchToMainMenu()
    {
        TrySwitchLevel(menuPrefab);
    }

    public void TrySwitchLevel(LevelManager toPrefab)
    {
        if (inTransition) return;

        StartCoroutine(SwitchLevel(toPrefab));
        inTransition = true;
    }

    private IEnumerator SwitchLevel(LevelManager toPrefab)
    {
        inTransition = true;

        // Otherwise dominos on tabletop seem to persist between levels??
        // Better solution likely needed
        DominoManager.Instance.ClearDominos();

        Time.timeScale = 0;

        float t = 0;
        while (t < 1)
        {
            t += Time.fixedDeltaTime * transitionSeconds;
            t = Mathf.Min(t, 1f);
            var prev = currentLevel.transform.position;
            currentLevel.transform.position = new Vector3(prev.x, Mathf.Lerp(0f, LowestPoint, t), prev.z);
            yield return null;
        }

        Destroy(currentLevel.gameObject);
        yield return null;
        currentLevel = Instantiate(toPrefab);
        currentLevel.gameObject.SetActive(true);
        var cameras = Camera.allCameras.ToArray();
        foreach (var camera in cameras) camera.cullingMask = 1 << 3;
        yield return null;
        var prevFirst = currentLevel.transform.position;
        currentLevel.transform.position = new Vector3(prevFirst.x, LowestPoint, prevFirst.z);
        yield return null;
        foreach (var camera in cameras) camera.cullingMask = int.MaxValue;

        while (t > 0)
        {
            t -= Time.fixedDeltaTime * transitionSeconds;
            t = Mathf.Max(t, 0f);
            var prev = currentLevel.transform.position;
            currentLevel.transform.position = new Vector3(prev.x, Mathf.Lerp(0f, LowestPoint, t), prev.z);
            yield return null;
        }

        Time.timeScale = 1f;
        LevelStarted.Invoke(currentLevel);

        inTransition = false;
    }
}