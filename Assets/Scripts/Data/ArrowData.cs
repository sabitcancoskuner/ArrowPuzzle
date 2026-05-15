using System.Collections.Generic;
using UnityEngine;

public enum ArrowDirection
{
    Up,
    Down,
    Left,
    Right
}

[System.Serializable]
public class ArrowData
{
    public int id;
    // ordered cells from tail to head
    public List<Vector2Int> cells;
    public ArrowDirection direction;
}
