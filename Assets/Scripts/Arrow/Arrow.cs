using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Animation Settings")]
    public float stepDuration = 0.08f;

    [Header("Escape Settings")]
    public int escapeDistance = 15;

    private GameObject headVisual;
    private List<GameObject> bodySegments = new List<GameObject>();

    private Vector2Int currentHeadPos;
    private Vector2Int escapeDirection;
    private int remainingEscapeSteps;

    public void BuildVisuals(ArrowData arrowData)
    {
        currentHeadPos = arrowData.cells[arrowData.cells.Count - 1];

        // 3. Spawn Middle Segments (Bodies and Elbows)
        for (int i = 0; i < arrowData.cells.Count - 1; i++)
        {
            if (i == 0)
            {
                GameObject tailPiece = VisualManager.Instance.SpawnVisualPiece(ArrowVisualType.Tail, arrowData.cells[i], transform);
                Vector2Int dir = arrowData.cells[i + 1] - arrowData.cells[i];
                tailPiece.transform.up = new Vector3(dir.x, dir.y, 0);
                bodySegments.Add(tailPiece);
            }
            else
            {
                ArrowVisualType type = GetMiddleVisualType(arrowData, i);
                GameObject piece = VisualManager.Instance.SpawnVisualPiece(type, arrowData.cells[i], transform);

                if (type == ArrowVisualType.Body)
                {
                    Vector2Int dir = arrowData.cells[i + 1] - arrowData.cells[i];
                    piece.transform.up = new Vector3(dir.x, dir.y, 0);
                }
                else if (type == ArrowVisualType.ElbowJoint)
                {
                    Vector2Int dirIn = arrowData.cells[i] - arrowData.cells[i - 1];
                    Vector2Int dirOut = arrowData.cells[i + 1] - arrowData.cells[i];
                    piece.transform.eulerAngles = new Vector3(0, 0, GetElbowRotation(dirIn, dirOut));
                }
                bodySegments.Add(piece);
            }
        }

        // 4. Spawn Head
        headVisual = VisualManager.Instance.SpawnVisualPiece(ArrowVisualType.Head, currentHeadPos, transform);
        Vector2Int headDir = DirectionUtility.ToVector(arrowData.direction);
        headVisual.transform.up = new Vector3(headDir.x, headDir.y, 0);
    }

    public void StartEscape(Vector2Int direction)
    {
        escapeDirection = direction;
        remainingEscapeSteps = escapeDistance;
        AnimateStep();
    }

    private void AnimateStep()
    {
        if (remainingEscapeSteps <= 0)
        {
            // It has flown far enough off-screen. Clean it up!
            Destroy(gameObject);
            return;
        }

        remainingEscapeSteps--;

        Vector2Int nextHeadPos = currentHeadPos + escapeDirection;
        Vector3 worldNextHead = new Vector3(nextHeadPos.x, nextHeadPos.y, 0);

        // 1. Move the Head into the void
        Tween.Position(headVisual.transform, worldNextHead, stepDuration);

        // 2. Spawn a new body segment behind the head
        GameObject newBody = VisualManager.Instance.SpawnVisualPiece(ArrowVisualType.Body, currentHeadPos, transform);
        newBody.transform.up = new Vector3(escapeDirection.x, escapeDirection.y, 0);
        bodySegments.Add(newBody); 

        currentHeadPos = nextHeadPos;

        // 3. Shrink the back of the snake so the tail pulls forward
        if (bodySegments.Count > 0) ShrinkOldestSegment();

        Tween.Delay(stepDuration).OnComplete(() => AnimateStep());
    }

    private void ShrinkOldestSegment()
    {
        if (bodySegments.Count == 0) return;

        GameObject oldest = bodySegments[0];

        if (oldest.name.Contains("Cross") || oldest.name.Contains("Elbow"))
        {
            bodySegments.RemoveAt(0);
            Destroy(oldest);
            ShrinkOldestSegment(); 
            return;
        }

        // It is a straight body. Shrink it by moving its base forward.
        bodySegments.RemoveAt(0);
        Vector3 targetWorld = oldest.transform.position + oldest.transform.up;
        Sequence.Create()
            .Group(Tween.Position(oldest.transform, targetWorld, stepDuration, Ease.Linear))
            .Group(Tween.ScaleY(oldest.transform, 0f, stepDuration, Ease.Linear))
            .OnComplete(() =>
            {
               Destroy(oldest); 
            });
    }

    private ArrowVisualType GetMiddleVisualType(ArrowData arrow, int index)
    {
        Vector2Int incoming = arrow.cells[index] - arrow.cells[index - 1];
        Vector2Int outgoing = arrow.cells[index + 1] - arrow.cells[index];
        return incoming == outgoing ? ArrowVisualType.Body : ArrowVisualType.ElbowJoint;
    }

    private float GetElbowRotation(Vector2Int directionIn, Vector2Int directionOut)
    {
        if (directionIn == Vector2Int.up && directionOut == Vector2Int.left) return -90f;
        if (directionIn == Vector2Int.up && directionOut == Vector2Int.right) return 0f;
        if (directionIn == Vector2Int.down && directionOut == Vector2Int.left) return 180f;
        if (directionIn == Vector2Int.down && directionOut == Vector2Int.right) return 90f;
        if (directionIn == Vector2Int.right && directionOut == Vector2Int.up) return 180f;
        if (directionIn == Vector2Int.right && directionOut == Vector2Int.down) return -90f;
        if (directionIn == Vector2Int.left && directionOut == Vector2Int.up) return 90f;
        if (directionIn == Vector2Int.left && directionOut == Vector2Int.down) return 0f;
        return 0f;
    }
}
