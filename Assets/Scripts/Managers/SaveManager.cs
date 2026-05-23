using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("CurrentLevel"))
        {
            PlayerPrefs.SetInt("CurrentLevel", 1);
        }
    }

    public int GetCurrentLevelIndex()
    {
        return PlayerPrefs.GetInt("CurrentLevel");
    }

    public void SetCurrentLevel(int levelToSet)
    {
        PlayerPrefs.SetInt("CurrentLevel", levelToSet);
    }
}
