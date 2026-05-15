using UnityEngine;

public enum ArrowVisualType
{
    Body,
    Head,
    Tail,
    Cross
}

public class VisualManager : MonoBehaviour
{
    public static VisualManager Instance;

    [Header("Arrow Visual Pieces")]
    [SerializeField] private GameObject arrowBody;
    [SerializeField] private GameObject arrowHead;
    [SerializeField] private GameObject arrowTail;
    [SerializeField] private GameObject arrowCross;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject SpawnVisualPiece(ArrowVisualType type, Vector2Int pos)
    {
        switch (type)
        {
            case ArrowVisualType.Body:
                return Instantiate(arrowBody, new Vector3(pos.x, pos.y), Quaternion.identity);
            
            case ArrowVisualType.Head:
                return Instantiate(arrowHead, new Vector3(pos.x, pos.y), Quaternion.identity);
            
            case ArrowVisualType.Tail:
                return Instantiate(arrowTail, new Vector3(pos.x, pos.y), Quaternion.identity);

            case ArrowVisualType.Cross:
                return Instantiate(arrowCross, new Vector3(pos.x, pos.y), Quaternion.identity);
            
            default:
                return null;
        }
    }

}
