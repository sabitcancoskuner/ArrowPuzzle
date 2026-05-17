using UnityEngine;

public static class DirectionUtility
{
    public static Vector2Int ToVector(ArrowDirection direction)
    {
        switch(direction)
        {
            case ArrowDirection.Up:
                return Vector2Int.up;
            
            case ArrowDirection.Down:
                return Vector2Int.down;
            
            case ArrowDirection.Left:
                return Vector2Int.left;
            
            case ArrowDirection.Right:
                return Vector2Int.right;
            
            default:
                return Vector2Int.zero;
        }
    }

    public static ArrowDirection ToArrowDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
            return ArrowDirection.Up;

        else if (direction == Vector2Int.down)
            return ArrowDirection.Down;

        else if (direction == Vector2Int.left)
            return ArrowDirection.Left;

        return ArrowDirection.Right;
    }
}
