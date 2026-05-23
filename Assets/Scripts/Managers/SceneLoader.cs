using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(2);
    }
}
