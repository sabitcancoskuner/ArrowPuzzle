using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;

public class LineArrow : MonoBehaviour
{
    private const float MinSegmentDistance = 0.001f;

    [Header("Animation Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float escapeTrackDistance = 30f;
    [SerializeField] private float blockedImpactPause = 0.05f;
    [SerializeField] private float blockedReturnDuration = 0.08f;
    [SerializeField] private int blinkCount = 1;
    [SerializeField] private float blinkInterval = 0.15f;

    [Header("Color Settings")]
    [SerializeField] private Color moveColor;
    [SerializeField] private Color blockColor;
    private Color arrowBaseColor;

    private LineRenderer lineRenderer;
    private List<Vector3> linePoints = new List<Vector3>();

    // Arrow Head
    private Transform headTransform;
    private SpriteRenderer headRenderer;

    // Guide Line
    private LineRenderer guideLine;
    
    // Tweens and Sequences
    private Tween moveTween;
    private Sequence blockedSequence;
    private Sequence blinkSequence;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        headTransform = transform.GetChild(0);
        headRenderer = headTransform.GetComponent<SpriteRenderer>();

        guideLine = transform.GetChild(1).GetComponent<LineRenderer>();

        arrowBaseColor = lineRenderer.startColor;
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

        Vector2Int direction = DirectionUtility.ToVector(data.direction);
        Vector3 endpointOffset = new Vector3(direction.x, direction.y) * 100;
        guideLine.SetPosition(0, linePoints.Last());
        guideLine.SetPosition(1, linePoints.Last() + endpointOffset);
    }

    public void Move(Vector2Int direction)
    {
        if (moveTween.isAlive || blockedSequence.isAlive) return;

        Vector3 moveDirection = new Vector3(direction.x, direction.y, 0f).normalized;
        if (moveDirection.sqrMagnitude <= MinSegmentDistance) return;

        ChangeArrowColor(moveColor);

        float arrowLength = GetArrowLength();

        List<Vector3> track = new List<Vector3>(linePoints);
        float escapeDistance = GetEscapeTrackDistance(arrowLength);
        ExtendTrack(track, moveDirection, escapeDistance);

        float startHeadDistance = arrowLength;
        float endHeadDistance = arrowLength + escapeDistance;
        float duration = escapeDistance / Mathf.Max(MinSegmentDistance, speed);

        moveTween = Tween.Custom(
            startHeadDistance,
            endHeadDistance,
            duration,
            currentHeadDistance =>
            {
                linePoints = SampleTrack(track, currentHeadDistance, arrowLength);
                ApplyPositionsToLineRenderer();
                headTransform.position = linePoints.Last();
            },
            Ease.Linear
        ).OnComplete(this, target =>
        {
            Destroy(target.gameObject);
        });
    }

    public void PlayBlockedMove(Vector2Int direction, Vector2Int blockedPosition)
    {
        if (moveTween.isAlive || blockedSequence.isAlive) return;

        Vector3 moveDirection = new Vector3(direction.x, direction.y, 0f);
        Vector3 blockedWorldPosition = new Vector3(blockedPosition.x, blockedPosition.y, 0f);

        ChangeArrowColor(blockColor);

        float arrowLength = GetArrowLength();

        List<Vector3> track = new List<Vector3>(linePoints);
        Vector3 originalHeadPos = headTransform.position;

        float distanceToObstacle = Vector3.Distance(originalHeadPos, blockedWorldPosition) - 0.5f;
        if (distanceToObstacle <= MinSegmentDistance) distanceToObstacle = 0.15f;

        Vector3 impactPos = originalHeadPos + moveDirection * distanceToObstacle;
        track.Add(impactPos);

        float startHeadDistance = arrowLength;
        float endHeadDistance = arrowLength + distanceToObstacle;

        float forwardDuration = distanceToObstacle / Mathf.Max(MinSegmentDistance, speed);

        blockedSequence = Sequence.Create()
            .Chain(Tween.Custom(
                startHeadDistance,
                endHeadDistance,
                forwardDuration,
                currentHeadDistance =>
                {
                    linePoints = SampleTrack(track, currentHeadDistance, arrowLength);
                    ApplyPositionsToLineRenderer();
                    headTransform.position = linePoints.Last();
                },
                Ease.Linear
            ))
            .Chain(Tween.Delay(blockedImpactPause))
            .Chain(Tween.Custom(
                endHeadDistance,
                startHeadDistance,
                blockedReturnDuration,
                currentHeadDistance =>
                {
                    linePoints = SampleTrack(track, currentHeadDistance, arrowLength);
                    ApplyPositionsToLineRenderer();
                    headTransform.position = linePoints.Last();
                },
                Ease.OutQuad
            ))
            .ChainCallback(() =>
            {
                linePoints = SampleTrack(track, startHeadDistance, arrowLength);
                ApplyPositionsToLineRenderer();
                headTransform.position = linePoints.Last();
            });
    }

    public void Blink()
    {
        if (blinkSequence.isAlive)
            blinkSequence.Stop();

        blinkSequence = Sequence.Create(cycles: blinkCount)
            .Group(Tween.Custom(arrowBaseColor, blockColor, blinkInterval, color =>
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
                headRenderer.color = color;
            },
            Ease.OutQuad))
            .Chain(Tween.Custom(blockColor, arrowBaseColor, blinkInterval, color =>
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
                headRenderer.color = color;
            },
            Ease.InQuad));
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

    private void ExtendTrack(List<Vector3> track, Vector3 direction, float distance)
    {
        if (track.Count == 0) return;

        track.Add(track[track.Count - 1] + direction * distance);
    }

    private List<Vector3> SampleTrack(List<Vector3> track, float targetHeadDist, float targetLength)
    {
        List<Vector3> sampledPoints = new List<Vector3>();
        float currentDist = 0f;
        Vector3 headPos = track[0];
        int headSegmentIdx = 0;

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
            
            if (i == track.Count - 2)
            {
                headPos = track[i+1];
                headSegmentIdx = i;
            }
        }
        sampledPoints.Add(headPos);
        float remainingLength = targetLength;
        Vector3 currPoint = headPos;
        int currIdx = headSegmentIdx;

        while (remainingLength > 0.001f && currIdx >= 0)
        {
            float distToCorner = Vector3.Distance(currPoint, track[currIdx]);

            if (remainingLength > distToCorner)
            {
                if (distToCorner > 0.001f) 
                {
                    sampledPoints.Add(track[currIdx]);
                }
                remainingLength -= distToCorner;
                currPoint = track[currIdx];
                currIdx--;
            }
            else
            {
                Vector3 tailPos = Vector3.MoveTowards(currPoint, track[currIdx], remainingLength);
                sampledPoints.Add(tailPos);
                break;
            }
        }

        sampledPoints.Reverse();
        return sampledPoints;
    }

    private void ApplyPositionsToLineRenderer()
    {
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
    }


    private void ChangeArrowColor(Color newColor)
    {
        lineRenderer.startColor = newColor;
        lineRenderer.endColor = newColor;
        headRenderer.color = newColor;
    }

    public void ShowGuideLine(bool show)
    {
        guideLine.gameObject.SetActive(show);
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
