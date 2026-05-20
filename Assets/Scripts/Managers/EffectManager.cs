using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class EffectManager : MonoBehaviour
{
    [Header("Win Effect Settings")]
    [SerializeField] private GameObject confettiFX;
    [SerializeField] private int confettiToSpawn;

    private void OnEnable()
    {
        BoardManager.Instance.OnBoardCleared += PlayWinEffect;
    }

    private void OnDisable()
    {
        BoardManager.Instance.OnBoardCleared -= PlayWinEffect;
    }

    private void PlayWinEffect()
    {
        StartCoroutine(WinEffectCoroutine());
    }

    private IEnumerator WinEffectCoroutine()
    {
        for (int i = 0; i < confettiToSpawn; i++)
        {
            Vector3 randomPoint = new Vector3(Random.value, Random.value, 0);
            Vector3 worldPos = Camera.main.ViewportToWorldPoint(randomPoint);

            Instantiate(confettiFX, worldPos, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.3f));
        }
    }
}
