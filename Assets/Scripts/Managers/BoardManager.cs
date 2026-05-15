using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    // Board Settings
    private int boardWidth;
    private int boardHeight;
    private BoardCellData[,] board;

    [SerializeField] private LevelData testLevel;

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
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        boardWidth = testLevel.width;
        boardHeight = testLevel.height;
        board = new BoardCellData[boardWidth, boardHeight];
        
        foreach (ArrowData arrow in testLevel.arrows)
        {
            BoardCellData newBoardCell = new BoardCellData();
            newBoardCell.arrow = arrow;
            newBoardCell.arrowId = arrow.id;

            for(int i = 0; i < arrow.cells.Count; i++)
            {
                ArrowVisualType visualType = GetArrowVisualType(arrow, i);
                newBoardCell.visualPiece = VisualManager.Instance.SpawnVisualPiece(visualType, arrow.cells[i]);
            }
        }
    }

    private ArrowVisualType GetArrowVisualType(ArrowData arrow, int index)
    {
        // Tail 
        if (index == 0)
        {
            return ArrowVisualType.Tail;
        }
        // Head
        else if (index == arrow.cells.Count - 1)
        {
            return ArrowVisualType.Head;
        }

        Vector2Int previous = arrow.cells[index - 1];
        Vector2Int current = arrow.cells[index];
        Vector2Int next = arrow.cells[index + 1];

        Vector2Int incomingDirection = current - previous;
        Vector2Int outgoingDirection = next - current;

        return incomingDirection == outgoingDirection ?
               ArrowVisualType.Body :
               ArrowVisualType.Cross;
    }
}
