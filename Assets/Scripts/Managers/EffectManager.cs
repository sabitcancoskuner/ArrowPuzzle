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
        if (confettiFX == null) yield break;

        for (int i = 0; i < confettiToSpawn; i++)
        {
            if (!TryGetRandomWorldPointFromScreenArea(out Vector2 spawnPoint)) yield break;

            Vector3 worldPos = new Vector3(spawnPoint.x, spawnPoint.y, 0f);

            Instantiate(confettiFX, worldPos, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.3f));
        }
    }

    private bool TryGetRandomWorldPointFromScreenArea(out Vector2 worldPoint)
    {
        Vector2 viewportPoint = new Vector2(
            Random.Range(0.1f, 0.8f),
            Random.Range(0.1f, 0.8f)
        );

        Vector2 screenPoint = new Vector2(
            viewportPoint.x * Screen.width,
            viewportPoint.y * Screen.height
        );

        return CameraUtility.TryScreenToWorld(screenPoint, out worldPoint);
    }
}
