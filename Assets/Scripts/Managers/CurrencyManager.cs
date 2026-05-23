using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [SerializeField] private int totalHealth = 3;

    public event Action OnHealthZeroed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        BoardManager.Instance.OnWrongMove += DecreaseHealth;
    }

    private void OnDisable()
    {
        BoardManager.Instance.OnWrongMove -= DecreaseHealth;
    }

    private void DecreaseHealth()
    {
        totalHealth--;

        if (totalHealth == 0)
        {
            OnHealthZeroed?.Invoke();
        }
    }
}
