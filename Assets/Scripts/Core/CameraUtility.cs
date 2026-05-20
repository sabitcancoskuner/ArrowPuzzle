using UnityEngine;

public static class CameraUtility
{
    private static Camera main;

    private static Camera Main
    {
        get
        {
            if (main == null)
            {
                main = Camera.main;
            }

            return main;
        }
    }

    public static bool TryScreenToWorld(Vector2 position, out Vector2 worldPosition)
    {
        Camera camera = Main;
        if (camera == null)
        {
            worldPosition = Vector2.zero;
            return false;
        }

        worldPosition = camera.ScreenToWorldPoint(position);
        return true;
    }

    public static Vector2 ScreenToWorld(Vector2 position)
    {
        TryScreenToWorld(position, out Vector2 worldPosition);
        return worldPosition;
    }

    public static bool IsPointOffScreen(Vector3 worldPosition)
    {
        Camera camera = Main;
        if (camera == null) return false;

        Vector3 viewportPos = camera.WorldToViewportPoint(worldPosition);
        
        return viewportPos.x < -0.1f || viewportPos.x > 1.1f || viewportPos.y < -0.1f || viewportPos.y > 1.1f;
    }
}
