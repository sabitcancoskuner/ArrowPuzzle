using System.Collections.Generic;
using UnityEngine;

public enum LevelDifficulty
{
    Easy,
    Medium,
    Hard,
    Expert
}

[CreateAssetMenu(fileName = "New Level Data", menuName = "Arrow Puzzle/Level Data")]
public class LevelData : ScriptableObject
{
    public int width;
    public int height;
    public LevelDifficulty difficulty;
    public List<ArrowData> arrows = new List<ArrowData>();
}
