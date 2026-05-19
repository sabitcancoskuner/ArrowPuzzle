using UnityEngine;

public static class CameraUtility
{
    public static Camera main = Camera.main;

    public static Vector2 ScreenToWorld(Vector2 position)
    {
        return main.ScreenToWorldPoint(position);
    }

    public static bool IsPointOffScreen(Vector3 worldPosition)
    {
        Vector3 viewportPos = main.WorldToViewportPoint(worldPosition);
        
        return viewportPos.x < -0.1f || viewportPos.x > 1.1f || viewportPos.y < -0.1f || viewportPos.y > 1.1f;
    }
}
