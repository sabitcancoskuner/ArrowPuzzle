using UnityEngine;

public static class BoardUtility 
{
    public static Vector2Int ToIntVector(Vector2 position)
    {
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
}
