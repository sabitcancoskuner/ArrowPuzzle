using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle;
using UnityEngine;

public class LineArrow : MonoBehaviour
{
    private const float MinSegmentDistance = 0.001f;

    [Header("Animation Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float escapeTrackDistance = 30f;
    [SerializeField] private float blockedImpactPause = 0.05f;
    [SerializeField] private float blockedReturnDuration = 0.08f;

    [Header("Color Settings")]
    [SerializeField] private Color moveColor;
    [SerializeField] private Color blockColor;

    private LineRenderer lineRenderer;
    private List<Vector3> linePoints = new List<Vector3>();
    private Transform headTransform;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        headTransform = transform.GetChild(0);
    }

    public void BuildVisuals(ArrowData data)
    {
        linePoints.Clear();
        foreach (Vector2Int cell in data.cells)
        {
            linePoints.Add(new Vector3(cell.x, cell.y, 0f));
        }

        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());

        headTransform.position = linePoints.Last();
        headTransform.eulerAngles = GetHeadRotation(data.direction);
    }


    public void Move(Vector2Int direction)
    {
        if (moveCoroutine != null) return;
        moveCoroutine = StartCoroutine(MoveAlongLineRoutine(new Vector3(direction.x, direction.y, 0f)));
        ChangeArrowColor(moveColor);
    }

    public void PlayBlockedMove(Vector2Int direction, Vector2Int blockedPosition)
    {
        if (moveCoroutine != null) return;
        moveCoroutine = StartCoroutine(PlaySimpleBumpRoutine(new Vector3(direction.x, direction.y, 0f), new Vector3(blockedPosition.x, blockedPosition.y)));
        ChangeArrowColor(blockColor);
    }

    private IEnumerator MoveAlongLineRoutine(Vector3 direction)
    {
        if (linePoints.Count == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        direction = direction.normalized;
        if (direction.sqrMagnitude <= MinSegmentDistance)
        {
            moveCoroutine = null;
            yield break;
        }

        float arrowLength = GetArrowLength();
        List<Vector3> track = new List<Vector3>(linePoints);
        ExtendTrack(track, direction, GetEscapeTrackDistance(arrowLength));

        float currentHeadDistance = arrowLength;
        float moveSpeed = Mathf.Max(MinSegmentDistance, speed);

        while (!IsTailOffScreen())
        {
            currentHeadDistance += moveSpeed * Time.deltaTime;

            if (currentHeadDistance + arrowLength >= GetTrackLength(track) - MinSegmentDistance)
            {
                ExtendTrack(track, direction, GetEscapeTrackDistance(arrowLength));
            }

            linePoints = SampleTrack(track, currentHeadDistance, arrowLength);
            ApplyPositionsToLineRenderer();
            headTransform.position = linePoints.Last();
            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator PlaySimpleBumpRoutine(Vector3 direction, Vector3 blockedPosition)
    {
        float arrowLength = GetArrowLength();

        // The track is the current snake's body + the extra distance it will travel before hitting the wall
        List<Vector3> track = new List<Vector3>(linePoints);
        Vector3 originalHeadPos = headTransform.position;

        float distanceToObstacle = Vector3.Distance(originalHeadPos, blockedPosition) - 0.5f;
        if (distanceToObstacle <= MinSegmentDistance) distanceToObstacle = 0.15f;
        
        Vector3 impactPos = originalHeadPos + (direction * distanceToObstacle);
        track.Add(impactPos); 

        float startHeadDist = arrowLength;
        float endHeadDist = arrowLength + distanceToObstacle;

        float forwardDuration = distanceToObstacle / speed;
        float elapsed = 0f;

        // --- PHASE 1: Sither Forward (Tail goes away) ---
        while (elapsed < forwardDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / forwardDuration);
            
            float currentHeadDist = Mathf.Lerp(startHeadDist, endHeadDist, t);

            linePoints = SampleTrack(track, currentHeadDist, arrowLength);
            ApplyPositionsToLineRenderer();
            headTransform.position = linePoints.Last();

            yield return null;
        }

        // --- PHASE 2: Impact Pause ---
        if (blockedImpactPause > 0f) yield return new WaitForSeconds(blockedImpactPause);

        // --- PHASE 3: Slither Backward (Head goes away, tail spawns back) ---
        elapsed = 0f;
        while (elapsed < blockedReturnDuration)
        {
            elapsed += Time.deltaTime;
            // You can use a custom curve here or stick to Lerp
            float t = Mathf.Clamp01(elapsed / blockedReturnDuration); 
            
            // Move the head's position backwards along the track
            float currentHeadDist = Mathf.Lerp(endHeadDist, startHeadDist, t);

            linePoints = SampleTrack(track, currentHeadDist, arrowLength);
            ApplyPositionsToLineRenderer();
            headTransform.position = linePoints.Last();

            yield return null;
        }

        // --- PHASE 4: Perfect Reset ---
        linePoints = SampleTrack(track, startHeadDist, arrowLength);
        ApplyPositionsToLineRenderer();
        headTransform.position = linePoints.Last();

        moveCoroutine = null;
    }

    private float GetArrowLength()
    {
        float length = 0f;
        for (int i = 0; i < linePoints.Count - 1; i++)
        {
            length += Vector3.Distance(linePoints[i], linePoints[i + 1]);
        }
        return length;
    }

    private float GetEscapeTrackDistance(float arrowLength)
    {
        Camera camera = Camera.main;
        if (camera == null) return Mathf.Max(escapeTrackDistance, arrowLength);

        float cameraBasedDistance = camera.orthographicSize * 4f + arrowLength + 20f;
        return Mathf.Max(escapeTrackDistance, cameraBasedDistance);
    }

    private float GetTrackLength(List<Vector3> track)
    {
        float length = 0f;
        for (int i = 0; i < track.Count - 1; i++)
        {
            length += Vector3.Distance(track[i], track[i + 1]);
        }

        return length;
    }

    private void ExtendTrack(List<Vector3> track, Vector3 direction, float distance)
    {
        if (track.Count == 0) return;

        track.Add(track[track.Count - 1] + direction * distance);
    }

    // --- HELPER 2: The Magic Algorithm ---
    // Takes a 1D distance and walks backward along the 2D path, perfectly placing corners!
    private List<Vector3> SampleTrack(List<Vector3> track, float targetHeadDist, float targetLength)
    {
        List<Vector3> sampledPoints = new List<Vector3>();
        float currentDist = 0f;
        Vector3 headPos = track[0];
        int headSegmentIdx = 0;

        // Step A: Walk forward along the track to find exactly where the Head is
        for (int i = 0; i < track.Count - 1; i++)
        {
            float segLen = Vector3.Distance(track[i], track[i+1]);
            if (currentDist + segLen >= targetHeadDist)
            {
                float t = (targetHeadDist - currentDist) / segLen;
                headPos = Vector3.Lerp(track[i], track[i+1], t);
                headSegmentIdx = i;
                break;
            }
            currentDist += segLen;
            
            if (i == track.Count - 2) // Fallback for floating point limits
            {
                headPos = track[i+1];
                headSegmentIdx = i;
            }
        }

        // Step B: Walk backwards from the Head, collecting corners until we reach targetLength
        sampledPoints.Add(headPos);
        float remainingLength = targetLength;
        Vector3 currPoint = headPos;
        int currIdx = headSegmentIdx;

        while (remainingLength > 0.001f && currIdx >= 0)
        {
            float distToCorner = Vector3.Distance(currPoint, track[currIdx]);

            if (remainingLength > distToCorner)
            {
                // We consumed this straight segment entirely, log the corner and keep walking back
                if (distToCorner > 0.001f) 
                {
                    sampledPoints.Add(track[currIdx]);
                }
                remainingLength -= distToCorner;
                currPoint = track[currIdx]; // Shift our reference to the corner
                currIdx--;
            }
            else
            {
                // The tail ends halfway down this specific segment!
                Vector3 tailPos = Vector3.MoveTowards(currPoint, track[currIdx], remainingLength);
                sampledPoints.Add(tailPos);
                break; // We have fully constructed the snake
            }
        }

        sampledPoints.Reverse(); // Reverse so index 0 is the tail again
        return sampledPoints;
    }

    private void ApplyPositionsToLineRenderer()
    {
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
    }

    private bool IsTailOffScreen()
    {
        return linePoints.Count == 0 || CameraUtility.IsPointOffScreen(linePoints[0]);
    }

    private void ChangeArrowColor(Color newColor)
    {
        lineRenderer.startColor = newColor;
        lineRenderer.endColor = newColor;
        headTransform.GetComponent<SpriteRenderer>().color = newColor;
    }

    private Vector3 GetHeadRotation(ArrowDirection direction)
    {
        switch (direction)
        {
            case ArrowDirection.Up:    return new Vector3(0, 0, 90);
            case ArrowDirection.Down:  return new Vector3(0, 0, -90);
            case ArrowDirection.Left:  return new Vector3(0, 0, 180);
            default:                   return new Vector3(0, 0, 0);
        }
    }
}
