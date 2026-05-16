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

            for(int i = 0; i < arrow.cells.Count; i++)
            {
                ArrowVisualType bodyType = GetArrowVisualType(arrow, i);
                newBoardCell.visualPiece = VisualManager.Instance.SpawnVisualPiece(bodyType, arrow.cells[i]);

                board[arrow.cells[i].x, arrow.cells[i].y] = newBoardCell;
            }
        }
    }

    private ArrowVisualType GetArrowVisualType(ArrowData arrow, int index)
    {
        if (index == 0) return ArrowVisualType.Tail;  
        else if (index == arrow.cells.Count - 1) return ArrowVisualType.Head;
 
        Vector2Int previous = arrow.cells[index - 1];
        Vector2Int current = arrow.cells[index];
        Vector2Int next = arrow.cells[index + 1];

        Vector2Int incoming = current - previous;
        Vector2Int outgoing = next - current;

        return incoming == outgoing ? ArrowVisualType.Body : ArrowVisualType.Cross;
    }

    private bool IsInsideGrid(int x, int y)
    {
        if (x < 0 || x >= boardWidth || y < 0 || y >= boardHeight) return false;

        return true;
    }

    private bool IsInsideGrid(Vector2Int pos)
    {
        return IsInsideGrid(pos.x, pos.y);
    }
    
    private bool IsCellEmpty(int x, int y)
    {
        if (!IsInsideGrid(x, y)) return false;

        return board[x, y] == null;
    }

    private bool IsCellEmpty(Vector2Int pos)
    {
        return IsCellEmpty(pos.x, pos.y);
    }
}
