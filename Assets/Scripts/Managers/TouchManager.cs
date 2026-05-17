using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance;

    private PlayerInput playerInput;

    // Touch Actions
    private InputAction touchPositionAction;
    private InputAction touchPressAction;

    // Pinch Zoom Actions
    private InputAction primaryFingerPositionAction;
    private InputAction secondaryFingerPositionAction;
    private InputAction secondaryTouchContactAction;
    private Coroutine zoomCoroutine;

    // Swipe Actions and settings
    private InputAction primarySwipeContactAction;
    private InputAction primarySwipePositionAction;
    private Coroutine swipeCoroutine;

    [Header("Swipe Settings")]
    [SerializeField] private float minSwipePixelDelta = 0.1f;
    [SerializeField] private float swipeStartPixelDistance = 20f;
    [SerializeField] private float pinchSwipeSensitivity = 1.5f;

    [Header("Tap Settings")]
    [SerializeField] private float tapMaxPixelDistance = 15f;

    private Vector2 touchStartPosition;
    private bool isSwiping;
    private bool isZooming;

    // Event
    public event Action<Vector2> OnScreenTouched;
    public event Action<float, Vector2> OnPinchZoom;
    public event Action<Vector2> OnScreenSwipe;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        playerInput = GetComponent<PlayerInput>();

        touchPositionAction = playerInput.actions["TouchPosition"];
        touchPressAction = playerInput.actions["TouchPress"];

        primaryFingerPositionAction = playerInput.actions["PrimaryFingerPosition"];
        secondaryFingerPositionAction = playerInput.actions["SecondaryFingerPosition"];
        secondaryTouchContactAction = playerInput.actions["SecondaryTouchContact"];

        primarySwipeContactAction = playerInput.actions["PrimarySwipeContact"];
        primarySwipePositionAction = playerInput.actions["PrimarySwipePosition"];
    }

    private void OnEnable()
    {
        touchPressAction.started += TouchStarted;
        touchPressAction.canceled += TouchReleased;

        secondaryTouchContactAction.started += ZoomStart;
        secondaryTouchContactAction.canceled += ZoomEnd;

        primarySwipeContactAction.started += StartTouchPrimary;
        primarySwipeContactAction.canceled += EndTouchPrimary;
    }

    private void OnDisable()
    {
        touchPressAction.started -= TouchStarted;
        touchPressAction.canceled -= TouchReleased;

        secondaryTouchContactAction.started -= ZoomStart;
        secondaryTouchContactAction.canceled -= ZoomEnd;

        primarySwipeContactAction.started -= StartTouchPrimary;
        primarySwipeContactAction.canceled -= EndTouchPrimary;

        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            zoomCoroutine = null;
        }

        if (swipeCoroutine != null)
        {
            StopCoroutine(swipeCoroutine);
            swipeCoroutine = null;
        }
    }

    private void TouchStarted(InputAction.CallbackContext ctx)
    {
        touchStartPosition = touchPositionAction.ReadValue<Vector2>();
        isSwiping = false;
    }

    private void TouchReleased(InputAction.CallbackContext ctx)
    {
        Vector2 touchEndPosition = touchPositionAction.ReadValue<Vector2>();
        float touchDistance = Vector2.Distance(touchStartPosition, touchEndPosition);

        if (!isSwiping && touchDistance <= tapMaxPixelDistance)
        {
            OnScreenTouched?.Invoke(touchEndPosition);
        }
    }

    private void ZoomStart(InputAction.CallbackContext ctx)
    {
        isZooming = true;
        isSwiping = true;
        zoomCoroutine = StartCoroutine(ZoomDetection());
    }

    private void ZoomEnd(InputAction.CallbackContext ctx)
    {
        isZooming = false;
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            zoomCoroutine = null;
        }
    }

    private IEnumerator ZoomDetection()
    {
        Vector2 firstFingerPosition = primaryFingerPositionAction.ReadValue<Vector2>();
        Vector2 secondaryFingerPosition = secondaryFingerPositionAction.ReadValue<Vector2>();
        float previousDistance = Vector2.Distance(firstFingerPosition, secondaryFingerPosition);;
        Vector2 previousPinchCenter = (firstFingerPosition + secondaryFingerPosition) * 0.5f;

        float distance = 0f;

        while (true)
        {
            firstFingerPosition = primaryFingerPositionAction.ReadValue<Vector2>();
            secondaryFingerPosition = secondaryFingerPositionAction.ReadValue<Vector2>();

            distance = Vector2.Distance(firstFingerPosition, secondaryFingerPosition);
            Vector2 pinchCenter = (firstFingerPosition + secondaryFingerPosition) * 0.5f;

            float delta = distance - previousDistance;
            Vector2 pinchCenterDelta = (previousPinchCenter - pinchCenter) * pinchSwipeSensitivity;

            if (Mathf.Abs(delta) > 0f)
            {
                OnPinchZoom?.Invoke(-delta, pinchCenter);
            }

            if (pinchCenterDelta.sqrMagnitude >= minSwipePixelDelta * minSwipePixelDelta)
            {
                OnScreenSwipe?.Invoke(pinchCenterDelta);
            }

            previousDistance = distance;
            previousPinchCenter = pinchCenter;
            yield return null;
        }
    }

    private void StartTouchPrimary(InputAction.CallbackContext ctx)
    {
        if (swipeCoroutine != null)
        {
            StopCoroutine(swipeCoroutine);
        }

        swipeCoroutine = StartCoroutine(SwipeDetection());
    }

    private void EndTouchPrimary(InputAction.CallbackContext ctx)
    {
        if (swipeCoroutine != null)
        {
            StopCoroutine(swipeCoroutine);
            swipeCoroutine = null;
        }
    }

    private IEnumerator SwipeDetection()
    {
        yield return null;

        Vector2 startPosition = primarySwipePositionAction.ReadValue<Vector2>();
        Vector2 previousPosition = primarySwipePositionAction.ReadValue<Vector2>();

        while (true)
        {
            Vector2 currentPosition = primarySwipePositionAction.ReadValue<Vector2>();
            if (isZooming)
            {
                previousPosition = currentPosition;
                yield return null;
                continue;
            }

            Vector2 screenDelta = previousPosition - currentPosition;
            float totalDragDistance = Vector2.Distance(startPosition, currentPosition);

            if (!isSwiping && totalDragDistance >= swipeStartPixelDistance)
            {
                isSwiping = true;
            }

            if (isSwiping && screenDelta.sqrMagnitude >= minSwipePixelDelta * minSwipePixelDelta)
            {
                OnScreenSwipe?.Invoke(screenDelta);
            }

            previousPosition = currentPosition;
            yield return null;
        }
    }
}
