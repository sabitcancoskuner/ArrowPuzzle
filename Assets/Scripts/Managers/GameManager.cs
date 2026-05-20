using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private int totalHealth = 3;

    public event Action OnGameLose;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        Application.targetFrameRate = 120;
    }

    private void Start()
    {
        BoardManager.Instance.OnWrongMove += DecreaseHealth;
    }

    private void OnDisable()
    {
        BoardManager.Instance.OnWrongMove -= DecreaseHealth;
    }
    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(1);
    }

    private void DecreaseHealth()
    {
        totalHealth--;

        if (totalHealth == 0)
        {
            OnGameLose?.Invoke();
        }
    }
}
