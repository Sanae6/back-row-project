using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject buttonPrefab;
    public float distanceBetweenButtons = 0.5f;

    [NonSerialized] public readonly List<PressableButton> Levels = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var list = LevelSystem.Instance.levelPrefabs;
        var left = -(list.Count - 1) * distanceBetweenButtons / 2;
        for (var i = 0; i < list.Count; i++)
        {
            var prefab = list[i];
            var button = Instantiate(buttonPrefab, new Vector3(left + i * distanceBetweenButtons, 0, 0),
                Quaternion.identity, new InstantiateParameters { parent = transform, worldSpace = false });
            button.GetComponentInChildren<TMP_Text>().text = $"LEVEL {i + 1}";
            var pressable = button.GetComponentInChildren<PressableButton>();
            pressable.ButtonPressed.AddListener(() => SwitchLevel(prefab));
            Levels.Add(pressable);
        }
    }

    private void SwitchLevel(LevelManager prefab)
    {
        LevelSystem.Instance.TrySwitchLevel(prefab);
    }
}