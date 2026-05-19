using UnityEngine;

public enum ArrowVisualType
{
    Body,
    Head,
    Tail,
    ElbowJoint
}

public class VisualManager : MonoBehaviour
{
    public static VisualManager Instance;

    [Header("Arrow Pieces")]
    [SerializeField] private GameObject arrowPrefab;
    private Transform arrowParent;
    [SerializeField] private GameObject dotPrefab;
    private Transform dotParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        arrowParent = new GameObject("Arrow Parent").transform;
        dotParent = new GameObject("Dot Parent").transform;
    }

    public GameObject SpawnArrowPrefab(Vector2Int pos)
    {
        GameObject arrow = Instantiate(arrowPrefab, new Vector3(pos.x, pos.y), Quaternion.identity);
        arrow.transform.SetParent(arrowParent);
        return arrow;
    }

    public void SpawnDot(ArrowData data)
    {
        foreach (Vector2Int pos in data.cells)
        {
            GameObject dot = Instantiate(dotPrefab, new Vector3(pos.x, pos.y), Quaternion.identity);
            dot.transform.SetParent(dotParent);
        }
    }

}
