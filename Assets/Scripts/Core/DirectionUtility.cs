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
}
