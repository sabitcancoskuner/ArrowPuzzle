using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    // Board Settings
    private int boardWidth;
    private int boardHeight;
    private ArrowData[,] board;

    private Dictionary<ArrowData, Arrow> arrowVisuals;

    [SerializeField] private LevelData testLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        arrowVisuals = new Dictionary<ArrowData, Arrow>();
    }

    private void OnEnable()
    {
        TouchManager.Instance.OnScreenTouched += TryMoveArrow;
    }

    private void OnDisable()
    {
        TouchManager.Instance.OnScreenTouched -= TryMoveArrow;
    }

    private void Start()
    {
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        boardWidth = testLevel.width;
        boardHeight = testLevel.height;
        board = new ArrowData[boardWidth, boardHeight];
        
        foreach (ArrowData data in testLevel.arrows)
        {
            Vector2Int head = data.cells[data.cells.Count - 1];
            GameObject newArrowVisual = VisualManager.Instance.SpawnArrowPrefab(head); 

            for(int i = 0; i < data.cells.Count; i++)
            {
                board[data.cells[i].x, data.cells[i].y] = data;
            }

            Arrow newArrowScript = newArrowVisual.GetComponent<Arrow>();
            newArrowScript.BuildVisuals(data);

            arrowVisuals.Add(data, newArrowScript);
        }
    }

    private void TryMoveArrow(Vector2 screenPosition)
    {
        Vector2 worldPosition = CameraUtility.ScreenToWorld(screenPosition);
        Vector2Int gridPosition = BoardUtility.ToIntVector(worldPosition);

        if (IsCellEmpty(gridPosition)) return;

        ArrowData currentCell = board[gridPosition.x, gridPosition.y];
        Vector2Int headPosition = GetHeadPosition(currentCell);
        Vector2Int arrowDirection = GetArrowDirection(currentCell);

        var (canMove, blockedPos) = MoveArrow(headPosition, arrowDirection);

        if (canMove)
        {
            Debug.Log($"Arrow with id {currentCell.id} escaped");
            arrowVisuals[currentCell].StartEscape(arrowDirection);
            ClearArrowData(currentCell);
        }
        else
        {
            Debug.Log($"Arrow with id {currentCell.id} is blocked at position {blockedPos}");
        }
    }

    private Vector2Int GetHeadPosition(ArrowData boardData)
    {
        Vector2Int head = boardData.cells[boardData.cells.Count - 1];
        return head;
    }

    private Vector2Int GetArrowDirection(ArrowData boardData)
    {
        return DirectionUtility.ToVector(boardData.direction);
    }

    private (bool canMove, Vector2Int? blockedPosition) MoveArrow(Vector2Int head, Vector2Int direction)
    {
        Vector2Int checkPosition = head + direction;
        while (IsInsideGrid(checkPosition))
        {
            if (!IsCellEmpty(checkPosition)) return (false, checkPosition);
            checkPosition += direction;
        }

        return (true, null);
    }

    private void ClearArrowData(ArrowData boardData)
    {
        foreach (Vector2Int pos in boardData.cells)
        {
            board[pos.x, pos.y] = null;
        }
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
        if (!IsInsideGrid(x, y)) return true;

        return board[x, y] == null;
    }

    private bool IsCellEmpty(Vector2Int pos)
    {
        return IsCellEmpty(pos.x, pos.y);
    }
}
