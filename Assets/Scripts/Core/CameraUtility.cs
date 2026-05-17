using UnityEngine;

public static class CameraUtility
{
    public static Vector2 ScreenToWorld(Vector2 position)
    {
        return Camera.main.ScreenToWorldPoint(position);
    }
}
