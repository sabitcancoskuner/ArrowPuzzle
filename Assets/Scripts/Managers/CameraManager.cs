using PrimeTween;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 0.01f;
    [SerializeField] private float minCameraSize = 4f;
    [SerializeField] private float maxCameraSize = 20f;
    [SerializeField] private float zoomDuration = 0.1f;

    [Header("Movement Settings")]
    [SerializeField] private float maxSwipeWorldDelta = 0.5f;

    private Camera mainCamera;
    private float targetZoom;
    private Tween zoomTween;
    private Tween zoomPositionTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        mainCamera = Camera.main;
        targetZoom = mainCamera.orthographicSize;
    }

    private void OnEnable()
    {
        TouchManager.Instance.OnPinchZoom += ApplyZoom;
        TouchManager.Instance.OnScreenSwipe += MoveCamera;

        BoardManager.Instance.OnBoardCreated += FrameGrid;
    }

    private void OnDisable()
    {
        TouchManager.Instance.OnPinchZoom -= ApplyZoom;
        TouchManager.Instance.OnScreenSwipe -= MoveCamera;

        BoardManager.Instance.OnBoardCreated -= FrameGrid;
    }

    private void ApplyZoom(float zoomAmount, Vector2 zoomCenterScreen)
    {
        float currentZoom = mainCamera.orthographicSize;
        targetZoom += zoomAmount * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minCameraSize, maxCameraSize);

        if (zoomTween.isAlive)
        {
            zoomTween.Stop();
        }

        if (zoomPositionTween.isAlive)
        {
            zoomPositionTween.Stop();
        }

        Vector3 targetPosition = GetCameraPositionForZoomCenter(zoomCenterScreen, currentZoom, targetZoom);

        zoomTween = Tween.CameraOrthographicSize(mainCamera, targetZoom, zoomDuration, Ease.OutQuad);
        zoomPositionTween = Tween.Position(mainCamera.transform, targetPosition, zoomDuration, Ease.OutQuad);
    }

    private void MoveCamera(Vector2 screenDelta)
    {
        float worldUnitsPerPixel = (mainCamera.orthographicSize * 2f) / Screen.height;

        Vector3 cameraDelta = new Vector3(screenDelta.x * worldUnitsPerPixel,
                                          screenDelta.y * worldUnitsPerPixel,
                                          0f);

        cameraDelta = Vector3.ClampMagnitude(cameraDelta, maxSwipeWorldDelta);

        mainCamera.transform.position += cameraDelta;
    }

    private Vector3 GetCameraPositionForZoomCenter(Vector2 zoomCenterScreen, float currentZoom, float nextZoom)
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 offsetFromCenter = zoomCenterScreen - screenCenter;

        float currentWorldUnitsPerPixel = (currentZoom * 2f) / Screen.height;
        float nextWorldUnitsPerPixel = (nextZoom * 2f) / Screen.height;
        float worldUnitsPerPixelDelta = currentWorldUnitsPerPixel - nextWorldUnitsPerPixel;

        Vector3 targetPosition = mainCamera.transform.position;
        targetPosition.x += offsetFromCenter.x * worldUnitsPerPixelDelta;
        targetPosition.y += offsetFromCenter.y * worldUnitsPerPixelDelta;

        return targetPosition;
    }

    public void FrameGrid(int gridWidth, int gridHeight)
    {  
        float padding = 1f;
        float centerX = gridWidth / 2f - 0.5f;
        float centerY = gridHeight / 2f;

        mainCamera.transform.position = new Vector3(centerX, centerY, mainCamera.transform.position.z);

        float requiredVerticalSize = (gridHeight / 2f) + padding;
        float requiredHorizontalSize = ((gridWidth / 2f) + padding) / mainCamera.aspect;
        float maxSize = Mathf.Max(requiredVerticalSize, requiredHorizontalSize);
        maxCameraSize = maxSize;
        mainCamera.orthographicSize = maxSize;
    }
}
