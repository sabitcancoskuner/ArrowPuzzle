using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("All Levels")]
    [SerializeField] private List<LevelData> levels;

    private static int levelIndex = 0;

    public event Action<LevelData> OnLevelLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        LoadCurrentLevel();
    }

    private void LoadCurrentLevel()
    {
        SetLevelIndex(SaveManager.Instance.GetCurrentLevelIndex() - 1);
        OnLevelLoaded?.Invoke(levels[levelIndex]);
    }

    public void SetLevelIndex(int index)
    {
        levelIndex = index;
    }

    public int GetCurrentLevelIndex()
    {
        return levelIndex;
    }

}
