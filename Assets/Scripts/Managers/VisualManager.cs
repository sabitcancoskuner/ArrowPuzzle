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
    [SerializeField] private GameObject arrowBody;
    [SerializeField] private GameObject arrowHead;
    [SerializeField] private GameObject arrowTail;
    [SerializeField] private GameObject arrowElbow;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject SpawnArrowPrefab(Vector2Int pos)
    {
        return Instantiate(arrowPrefab, new Vector3(pos.x, pos.y), Quaternion.identity);
    }

    public GameObject SpawnVisualPiece(ArrowVisualType type, Vector2Int pos, Transform parent)
    {
        GameObject newBodyPart;
        switch (type)
        {
            case ArrowVisualType.Body:
                newBodyPart = Instantiate(arrowBody, new Vector3(pos.x, pos.y), Quaternion.identity);
                newBodyPart.transform.SetParent(parent);
                return newBodyPart;

            case ArrowVisualType.Head:
                newBodyPart = Instantiate(arrowHead, new Vector3(pos.x, pos.y), Quaternion.identity);
                newBodyPart.transform.SetParent(parent);
                return newBodyPart;

            case ArrowVisualType.Tail:
                newBodyPart = Instantiate(arrowTail, new Vector3(pos.x, pos.y), Quaternion.identity);
                newBodyPart.transform.SetParent(parent);
                return newBodyPart;

            case ArrowVisualType.ElbowJoint:
                newBodyPart = Instantiate(arrowElbow, new Vector3(pos.x, pos.y), Quaternion.identity);
                newBodyPart.transform.SetParent(parent);
                return newBodyPart;

            default:
                 return null;
        }
    }

}
