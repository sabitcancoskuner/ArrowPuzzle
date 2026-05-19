using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGenerator
{
    public int minLen = 2;
    public int maxLen = 4;
    private int gridW, gridH;
    private LevelDifficulty difficulty;
    private System.Random rng = new System.Random();

    private readonly int[] dx = { 1, -1, 0, 0 };
    private readonly int[] dy = { 0, 0, 1, -1 };

    private List<ArrowData> placedArrows = new List<ArrowData>();

    public LevelGenerator(int w, int h, LevelDifficulty wantedDifficulty)
    {
        gridW = w;
        gridH = h;
        difficulty = wantedDifficulty;

        SetArrowLengths(wantedDifficulty);
    }

    public List<ArrowData> Generate()
    {
        List<int> zoneIndices = Enumerable.Range(0, gridW * gridH).ToList();
        if (zoneIndices == null || zoneIndices.Count < minLen) return null;

        placedArrows.Clear();

        HashSet<int> occupied = new HashSet<int>();
        HashSet<int> zone = new HashSet<int>(zoneIndices);

        bool addedInPass = true;

        while (addedInPass)
        {
            addedInPass = false;
            List<int> freeCells = zone.Where(idx => !occupied.Contains(idx)).ToList();

            if (freeCells.Count < minLen) break;

            ShuffleList(freeCells);

            foreach (int startCell in freeCells)
            {
                for (int targetLen = maxLen; targetLen >= minLen; targetLen--)
                {
                    ArrowData bestArrow = null;

                    int attemptsToFindHardArrow = 4;
                    for (int attempt = 0; attempt < attemptsToFindHardArrow; attempt++)
                    {
                        ArrowData potentialArrow = TryCreateSnakeArrow(startCell, targetLen, occupied, zone);

                        if (potentialArrow != null)
                        {
                            placedArrows.Add(potentialArrow);

                            if (CanSolveLevel())
                            {
                                bestArrow = potentialArrow;

                                if (IsExitBlocked(potentialArrow, occupied))
                                {
                                    placedArrows.RemoveAt(placedArrows.Count - 1);
                                    break;
                                }
                            }

                            placedArrows.RemoveAt(placedArrows.Count - 1);
                        }
                    }

                    if (bestArrow != null)
                    {
                        placedArrows.Add(bestArrow);
                        foreach (Vector2Int pos in bestArrow.cells)
                        {
                            occupied.Add(CellToIndex(pos));
                        }
                        addedInPass = true;
                        break;
                    }
                }

                if (addedInPass) break;
            }
        }

        TryFillEmptyCells(zone, occupied);

        return placedArrows;
    }

    public bool CanSolveLevel()
    {
        if (placedArrows.Count == 0) return true;

        int totalCells = gridW * gridH;
        int[] board = new int[totalCells];
        System.Array.Fill(board, -1);

        for (int i = 0; i < placedArrows.Count; i++)
        {
            foreach (Vector2Int pos in placedArrows[i].cells)
            {
                board[CellToIndex(pos)] = i;
            }
        }

        bool[] removed = new bool[placedArrows.Count];
        int removedCount = 0;
        bool changed;

        do
        {
            changed = false;
            for (int i = 0; i < placedArrows.Count; i++)
            {
                if (removed[i]) continue;

                if (CanExit(placedArrows[i], board))
                {
                    foreach (Vector2Int pos in placedArrows[i].cells)
                    {
                        board[CellToIndex(pos)] = -1;
                    }

                    removed[i] = true;
                    removedCount++;
                    changed = true;
                }
            }
        } while (changed);

        return removedCount == placedArrows.Count;
    }

    private bool CanExit(ArrowData arrow, int[] board)
    {
        int lastIndex = arrow.cells.Count - 1;
        int headIdx = CellToIndex(arrow.cells[lastIndex]);
        int neckIdx = CellToIndex(arrow.cells[lastIndex - 1]);

        int hx = headIdx % gridW;
        int hy = headIdx / gridW;
        int nx = neckIdx % gridW;
        int ny = neckIdx / gridW;

        int dx = hx - nx;
        int dy = hy - ny;

        int cx = hx + dx;
        int cy = hy + dy;

        while (cx >= 0 && cx < gridW && cy >= 0 && cy < gridH)
        {
            int checkIdx = cy * gridW + cx;
            if (board[checkIdx] != -1) return false;
            cx += dx;
            cy += dy;
        }

        return true;
    }

    private bool IsExitBlocked(ArrowData arrow, HashSet<int> occupied)
    {
        int lastIndex = arrow.cells.Count - 1;
        int headIdx = CellToIndex(arrow.cells[lastIndex]);
        int neckIdx = CellToIndex(arrow.cells[lastIndex - 1]);

        int hx = headIdx % gridW;
        int hy = headIdx / gridW;
        int nx = neckIdx % gridW;
        int ny = neckIdx / gridW;

        int dx = hx - nx;
        int dy = hy - ny;

        int cx = hx + dx;
        int cy = hy + dy;

        while (cx >= 0 && cx < gridW && cy >= 0 && cy < gridH)
        {
            int checkIdx = cy * gridW + cx;

            if (occupied.Contains(checkIdx)) return true;

            cx += dx;
            cy += dy;
        }

        return false;
    }

    private void TryFillEmptyCells(HashSet<int> zone, HashSet<int> occupied)
    {
        bool addedInPass;
        do
        {
            addedInPass = false;
            List<int> emptyCells = zone.Where(idx => !occupied.Contains(idx)).ToList();
            ShuffleList(emptyCells);

            foreach (int emptyIndex in emptyCells)
            {
                if (TryAttachEmptyCellToTail(emptyIndex, occupied) ||
                    TryAttachEmptyCellToHead(emptyIndex, occupied))
                {
                    addedInPass = true;
                    break;
                }
            }
        } while (addedInPass);
    }

    private bool TryAttachEmptyCellToTail(int emptyIndex, HashSet<int> occupied)
    {
        foreach (int neighborIndex in GetNeighborIndices(emptyIndex))
        {
            foreach (ArrowData arrow in placedArrows)
            {
                if (arrow.cells == null || arrow.cells.Count == 0) continue;

                int tailIndex = CellToIndex(arrow.cells[0]);
                if (tailIndex != neighborIndex) continue;

                Vector2Int emptyCell = IndexToCell(emptyIndex);
                arrow.cells.Insert(0, emptyCell);
                occupied.Add(emptyIndex);

                if (CanSolveLevel())
                {
                    return true;
                }

                arrow.cells.RemoveAt(0);
                occupied.Remove(emptyIndex);
            }
        }

        return false;
    }

    private bool TryAttachEmptyCellToHead(int emptyIndex, HashSet<int> occupied)
    {
        Vector2Int emptyCell = IndexToCell(emptyIndex);

        foreach (int neighborIndex in GetNeighborIndices(emptyIndex))
        {
            foreach (ArrowData arrow in placedArrows)
            {
                if (arrow.cells == null || arrow.cells.Count == 0) continue;

                int lastIndex = arrow.cells.Count - 1;
                Vector2Int headCell = arrow.cells[lastIndex];
                int headIndex = CellToIndex(headCell);
                if (headIndex != neighborIndex) continue;

                Vector2Int direction = DirectionUtility.ToVector(arrow.direction);
                if (headCell + direction != emptyCell) continue;

                arrow.cells.Add(emptyCell);
                occupied.Add(emptyIndex);

                if (CanSolveLevel())
                {
                    return true;
                }

                arrow.cells.RemoveAt(arrow.cells.Count - 1);
                occupied.Remove(emptyIndex);
            }
        }

        return false;
    }

    private List<int> GetNeighborIndices(int index)
    {
        List<int> neighbors = new List<int>();
        int x = index % gridW;
        int y = index / gridW;

        for (int i = 0; i < dx.Length; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx >= 0 && nx < gridW && ny >= 0 && ny < gridH)
            {
                neighbors.Add(ny * gridW + nx);
            }
        }

        return neighbors;
    }

    private ArrowData TryCreateSnakeArrow(int startIdx, int targetLen, HashSet<int> occupied, HashSet<int> zone)
    {
        List<int> currentPath = new List<int>();
        HashSet<int> pathVisited = new HashSet<int>();

        if (DFS_Snake(startIdx, targetLen, occupied, zone, currentPath, pathVisited))
        {
            currentPath.Reverse();
            return new ArrowData { id = 99,
                                   cells = ToVectorList(currentPath),
                                   direction = GetDirectionFromIndices(currentPath)};
        }

        return null;
    }

    private bool DFS_Snake(int currIdx, int targetLen, HashSet<int> occupied, HashSet<int> zone, List<int> currentPath, HashSet<int> pathVisited)
    {
        currentPath.Add(currIdx);
        pathVisited.Add(currIdx);

        if (currentPath.Count == targetLen) return true;

        int[] dIndices = { 0, 1, 2, 3 };
        Shuffle(dIndices);

        int cx = currIdx % gridW;
        int cy = currIdx / gridW;

        foreach (int i in dIndices)
        {
            int nx = cx + dx[i];
            int ny = cy + dy[i];
            int nextIdx = ny * gridW + nx;

            if (nx >= 0 && nx < gridW && ny >= 0 && ny < gridH &&
                zone.Contains(nextIdx) &&
                !occupied.Contains(nextIdx) &&
                !pathVisited.Contains(nextIdx))
            {
                if (DFS_Snake(nextIdx, targetLen, occupied, zone, currentPath, pathVisited))
                    return true;
            }
        }

        currentPath.RemoveAt(currentPath.Count - 1);
        pathVisited.Remove(currIdx);
        return false;
    }

    private ArrowDirection GetDirectionFromIndices(List<int> indices)
    {
        int headIndex = indices[indices.Count - 1];
        int neckIndex = indices[indices.Count - 2];

        int headX = headIndex % gridW;
        int headY = headIndex / gridW;

        int neckX = neckIndex % gridW;
        int neckY = neckIndex / gridW;

        int dx = headX - neckX;
        int dy = headY - neckY;

        if (dx == 1 && dy == 0) return ArrowDirection.Right;
        if (dx == -1 && dy == 0) return ArrowDirection.Left;
        if (dx == 0 && dy == 1) return ArrowDirection.Up;
        if (dx == 0 && dy == -1) return ArrowDirection.Down;

        throw new System.Exception("Invalid arrow direction");
    }

    private List<Vector2Int> ToVectorList(List<int> path)
    {
        List<Vector2Int> currentVectorPath = new List<Vector2Int>();

        for (int i = 0; i < path.Count; i++)
        {
            currentVectorPath.Add(IndexToCell(path[i]));
        }
        
        return currentVectorPath;
    }

    private void SetArrowLengths(LevelDifficulty difficulty)
    {
        if (difficulty == LevelDifficulty.Easy) maxLen = 4;
        else if (difficulty == LevelDifficulty.Medium) maxLen = 6;
        else if (difficulty == LevelDifficulty.Hard) maxLen = 9;
        else if (difficulty == LevelDifficulty.Expert) maxLen = 12;
    }
    
    private Vector2Int IndexToCell(int index)
    {
        return new Vector2Int(index % gridW, index / gridW);
    }

    private int CellToIndex(Vector2Int cell)
    {
        return cell.y * gridW + cell.x;
    }

    private void Shuffle(int[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            int value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }

    private void ShuffleList<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
