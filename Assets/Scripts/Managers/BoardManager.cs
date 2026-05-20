using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    // Board Settings
    private int boardWidth;
    private int boardHeight;
    private ArrowData[,] board;

    private Dictionary<ArrowData, LineArrow> arrowVisuals;
    private List<ArrowData> wrongMovedArrows = new List<ArrowData>();

    public event Action OnWrongMove;

    public event Action<int, int> OnBoardCreated;
    public event Action OnBoardCleared;
    public event Action<bool> OnShowGuideLine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        arrowVisuals = new Dictionary<ArrowData, LineArrow>();
    }

    private void OnEnable()
    {
        TouchManager.Instance.OnScreenTouched += TryMoveArrow;
        LevelManager.Instance.OnLevelLoaded += InitializeBoard;
    }

    private void OnDisable()
    {
        TouchManager.Instance.OnScreenTouched -= TryMoveArrow;
        LevelManager.Instance.OnLevelLoaded -= InitializeBoard;
    }

    private void InitializeBoard(LevelData level)
    {
        boardWidth = level.width;
        boardHeight = level.height;
        board = new ArrowData[boardWidth, boardHeight];
        
        foreach (ArrowData data in level.arrows)
        {
            Vector2Int head = data.cells[data.cells.Count - 1];
            GameObject newArrowVisual = VisualManager.Instance.SpawnArrowPrefab(head); 

            for(int i = 0; i < data.cells.Count; i++)
            {
                board[data.cells[i].x, data.cells[i].y] = data;
            }

            LineArrow newArrowScript = newArrowVisual.GetComponent<LineArrow>();
            VisualManager.Instance.SpawnDot(data);
            newArrowScript.BuildVisuals(data);
            newArrowScript.OnEscapeSuccesful += CheckWinSituation;

            arrowVisuals.Add(data, newArrowScript);
        }

        OnBoardCreated?.Invoke(boardWidth, boardHeight);
    }

    private void TryMoveArrow(Vector2 screenPosition)
    {
        Vector2 worldPosition = CameraUtility.ScreenToWorld(screenPosition);
        Vector2Int gridPosition = BoardUtility.ToIntVector(worldPosition);

        if (IsCellEmpty(gridPosition)) return;

        ArrowData currentArrow = board[gridPosition.x, gridPosition.y];
        Vector2Int headPosition = GetHeadPosition(currentArrow);
        Vector2Int arrowDirection = GetArrowDirection(currentArrow);

        var (canMove, blockedPos) = MoveArrow(headPosition, arrowDirection);

        if (canMove)
        {
            ToggleGuideLines(false);
            arrowVisuals[currentArrow].Move(arrowDirection);
            ClearArrowData(currentArrow);
        }
        else
        {
            arrowVisuals[currentArrow].PlayBlockedMove(arrowDirection, blockedPos.Value);
            arrowVisuals[board[blockedPos.Value.x, blockedPos.Value.y]].Blink();

            if (wrongMovedArrows.Contains(currentArrow)) return;

            wrongMovedArrows.Add(currentArrow);
            OnWrongMove?.Invoke();
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

    public void ToggleGuideLines(bool toggle)
    {
        foreach(LineArrow arrow in arrowVisuals.Values)
        {
            arrow.ToggleGuideline(toggle);
        }

        OnShowGuideLine?.Invoke(toggle);
    }

    private void ClearArrowData(ArrowData arrowData)
    {
        foreach (Vector2Int pos in arrowData.cells)
        {
            board[pos.x, pos.y] = null;
        }

        arrowVisuals.Remove(arrowData);
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

    private void CheckWinSituation()
    {
        // if there are no arrow visuals left, level is cleared
        if (arrowVisuals.Count == 0)
        {
            OnBoardCleared?.Invoke();
        }
    }
}
