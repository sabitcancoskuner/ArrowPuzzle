using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelBlueprintEditor : EditorWindow
{
    private LevelData loadedLevelAsset;
    
    // Board Settings
    private int boardWidth = 5;
    private int boardHeight = 5;
    private LevelDifficulty difficulty;
    private List<ArrowData> arrows = new List<ArrowData>();

    // Paint State
    private enum PaintMode { PaintArrow, EraseArrow }
    private PaintMode paintMode;

    // Board Constants
    private const float CELL = 50f;

    // Textures
    private Texture2D headTexture;
    private Texture2D bodyTexture;
    private Texture2D elbowTexture;
    
    private int arrowId = 1;
    private Vector2Int lastPaintedCell = new Vector2Int(-1, -1);
    private List<Vector2Int> currentDrawingPath = new List<Vector2Int>();
    private bool isDrawing = false;

    private void OnEnable()
    {
        headTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Editor/editor_arrow_head.png");
        bodyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Editor/editor_arrow_body.png");
        elbowTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Editor/editor_arrow_elbow.png");

        if (headTexture == null || bodyTexture == null || elbowTexture == null)
        {
            Debug.LogWarning("Level Editor: Could not find one or more textures. Check your file paths in OnEnable!");
        }
    }

    [MenuItem("Window/Level Layout Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelBlueprintEditor>("Level Layout Editor");
    }

    private void OnGUI()
    {
        GUILayout.Space(5);
        DrawAssetPanel();
        GUILayout.Space(10);
        DrawSettingsPanel();
        GUILayout.Space(10);
        DrawPaintToolbar();
        GUILayout.Space(10);
        DrawGrid();
    }

    private void DrawAssetPanel()
    {
        EditorGUILayout.LabelField("Level Asset", EditorStyles.whiteBoldLabel);
        loadedLevelAsset = (LevelData) EditorGUILayout.ObjectField("Asset", loadedLevelAsset, typeof(LevelData), false);
        GUILayout.Space(3);

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = loadedLevelAsset != null;

        GUIContent loadContent = new GUIContent("Load", "Load Imported Level");
        if (GUILayout.Button(loadContent, GUILayout.Height(30)))
        {
            LoadFromAsset(loadedLevelAsset);
        }

        GUIContent saveContent = new GUIContent("Save", "Save Current Level Layout");
        if (GUILayout.Button(saveContent, GUILayout.Height(30)))
        {
            SaveToAsset(loadedLevelAsset);
        }

        GUI.enabled = true;

        GUIContent createContent = new GUIContent("Create", "Create New Level Data with the current Level layout.");
        if (GUILayout.Button(createContent, GUILayout.Height(30)))
        {
            CreateNewAsset();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void SaveToAsset(LevelData asset)
    {
        asset.width = boardWidth;
        asset.height = boardHeight;
        asset.difficulty = difficulty;

        asset.arrows = new List<ArrowData>();

        foreach (ArrowData arrow in arrows)
        {
            ArrowData copy = new ArrowData();
            copy.id = arrow.id;
            copy.cells = new List<Vector2Int>(arrow.cells);
            copy.direction = arrow.direction;
            asset.arrows.Add(copy);
        }
        
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LevelBlueprintEditor] Saved -> {asset.name}");
    }

    private void LoadFromAsset(LevelData asset)
    {
        boardWidth = asset.width;
        boardHeight = asset.height;
        difficulty = asset.difficulty;
        arrows = new List<ArrowData>();
        if (asset.arrows != null)
        {
            foreach (ArrowData arrow in asset.arrows)
            {
                ArrowData copy = new ArrowData();
                copy.id = arrow.id;
                copy.cells = new List<Vector2Int>(arrow.cells);
                copy.direction = arrow.direction;
                arrows.Add(copy);
            }
        }
        Debug.Log($"[LevelBlueprintEdiot] Loaded -> {asset.name}");
    }

    private void CreateNewAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create Level Data", "New Level", "asset", "Choose Save Location");

        if (string.IsNullOrEmpty(path)) return;

        LevelData newLevelAsset = CreateInstance<LevelData>();

        newLevelAsset.width = boardWidth;
        newLevelAsset.height = boardHeight;
        newLevelAsset.difficulty = difficulty;

        newLevelAsset.arrows = new List<ArrowData>();
        foreach (ArrowData arrow in arrows)
        {
            ArrowData copy = new ArrowData();
            copy.id = arrow.id;
            copy.cells = new List<Vector2Int>(arrow.cells);
            copy.direction = arrow.direction;
            newLevelAsset.arrows.Add(copy);
        }

        AssetDatabase.CreateAsset(newLevelAsset, path);
        AssetDatabase.SaveAssets();
        
        loadedLevelAsset = newLevelAsset;
        Debug.Log($"[LevelBlueprintEdiot] Created -> {path}");
    }

    private void DrawSettingsPanel()
    {
        EditorGUILayout.LabelField("Board Settings", EditorStyles.whiteBoldLabel);
        GUILayout.Space(3);

        EditorGUI.BeginChangeCheck();

        int width = EditorGUILayout.IntField("Width", boardWidth);
        int height = EditorGUILayout.IntField("Height", boardHeight);
        LevelDifficulty newDifficulty = (LevelDifficulty)EditorGUILayout.EnumPopup("Level Difficulty", difficulty);

        if (EditorGUI.EndChangeCheck())
        {
            boardWidth = Mathf.Clamp(width, 1, 100);
            boardHeight = Mathf.Clamp(height, 1, 100);
            difficulty = newDifficulty;
            arrows = new List<ArrowData>();
        }

        if (GUILayout.Button("Reset Grid"))
        {
            arrows = new List<ArrowData>();
        }
    }

    private void DrawPaintToolbar()
    {
        EditorGUILayout.LabelField("Paint Tools", EditorStyles.whiteBoldLabel);

        EditorGUILayout.BeginHorizontal();
        DrawPaintModeButton("Draw Arrow", "Draw an arrow by dragging left click in the board.", PaintMode.PaintArrow);
        DrawPaintModeButton("Erase Arrow", "Erase whole arrow by clicking any cell of arrow.", PaintMode.EraseArrow);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPaintModeButton(string label, string desc, PaintMode mode)
    {
        GUIContent buttonContent = new GUIContent(label, desc);

        GUI.backgroundColor = paintMode == mode ? Color.blueViolet : Color.white;
        if (GUILayout.Button(buttonContent))
        {
            paintMode = mode;
        }
    }

    private void DrawGrid()
    {
        float totalWidth = boardWidth * CELL;
        float totalHeight = boardHeight * CELL;
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        Rect gridRect = GUILayoutUtility.GetRect(totalWidth, totalHeight);
        
        EditorGUI.DrawRect(gridRect, Color.gray1);

        for (int y = 0; y < boardHeight; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                Rect cellRect = new Rect(
                    gridRect.x + (x * CELL), 
                    gridRect.y + ((boardHeight - 1 - y) * CELL), 
                    CELL, 
                    CELL
                );

                EditorGUI.DrawRect(cellRect, Color.black);
                
                Rect innerRect = new Rect(cellRect.x + 1, cellRect.y + 1, CELL - 2, CELL - 2);
                
                DrawCellContent(new Vector2Int(x, y), innerRect);
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        ProcessMouseEvents(gridRect);
    }

    private void DrawCellContent(Vector2Int position, Rect rect)
    {
        int drawIndex = currentDrawingPath.IndexOf(position);

        if (drawIndex != -1)
        {
            RenderArrowPiece(currentDrawingPath, drawIndex, rect, Color.yellow);
            return;
        }

        foreach (ArrowData arrow in arrows)
        {
            int savedIndex = arrow.cells.IndexOf(position);
            if (savedIndex != -1)
            {
                RenderArrowPiece(arrow.cells, savedIndex, rect, Color.darkGray);
                return;
            }
        }

        EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));
    }

    private void RenderArrowPiece(List<Vector2Int> path, int index, Rect rect, Color tint)
    {
        EditorGUI.DrawRect(rect, tint);

        Texture2D textureToDraw = null;
        float angle = 0f;

        // Tail
        if (index == 0) 
        {
            textureToDraw = bodyTexture;
            if (path.Count > 1) angle = GetAngle(path[0], path[1]);
        }
        // Head
        else if (index == path.Count - 1) 
        {
            textureToDraw = headTexture;
            angle = GetAngle(path[index - 1], path[index]);
        }
        // Body or Elbow
        else 
        {
            Vector2Int dirIn = path[index] - path[index - 1];
            Vector2Int dirOut = path[index + 1] - path[index];

            if (dirIn == dirOut) 
            {
                textureToDraw = bodyTexture;
                angle = GetAngle(path[index - 1], path[index]);
            }
            else 
            {
                textureToDraw = elbowTexture;
                angle = GetElbowRotation(dirIn, dirOut);
            }
        }

        if (textureToDraw != null)
        {
            Matrix4x4 oldMatrix = GUI.matrix;    
            GUIUtility.RotateAroundPivot(angle, rect.center);
            
            GUI.DrawTexture(rect, textureToDraw, ScaleMode.StretchToFill, true);
            
            GUI.matrix = oldMatrix;
        }
    }

    private float GetAngle(Vector2Int from, Vector2Int to)
    {
        Vector2Int dir = to - from;
        if (dir == Vector2Int.up) return 0f;
        if (dir == Vector2Int.right) return 90f;
        if (dir == Vector2Int.down) return 180f;
        if (dir == Vector2Int.left) return 270f;
        return 0f;
    }

    private float GetElbowRotation(Vector2Int directionIn, Vector2Int directionOut)
    {
        if (directionIn == Vector2Int.up && directionOut == Vector2Int.left) return 90f;
        if (directionIn == Vector2Int.up && directionOut == Vector2Int.right) return 0f;
        if (directionIn == Vector2Int.down && directionOut == Vector2Int.left) return 180f;
        if (directionIn == Vector2Int.down && directionOut == Vector2Int.right) return -90f;
        if (directionIn == Vector2Int.right && directionOut == Vector2Int.up) return 180f;
        if (directionIn == Vector2Int.right && directionOut == Vector2Int.down) return 90f;
        if (directionIn == Vector2Int.left && directionOut == Vector2Int.up) return -90f;
        if (directionIn == Vector2Int.left && directionOut == Vector2Int.down) return 0f;
        return 0f;
    }

    private void ProcessMouseEvents(Rect gridRect)
    {
        Event e = Event.current;

        if (gridRect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                int x = Mathf.FloorToInt((e.mousePosition.x - gridRect.x) / CELL);
                int y = boardHeight - 1 - Mathf.FloorToInt((e.mousePosition.y - gridRect.y) / CELL);
                Vector2Int currentCell = new Vector2Int(x, y);

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    if (paintMode == PaintMode.EraseArrow)
                    {
                        EraseArrowContentAt(currentCell);
                        e.Use();
                        return;
                    }

                    if (IsCellOccupied(currentCell))
                    {
                        e.Use();
                        return;
                    }
                    
                    isDrawing = true;
                    currentDrawingPath.Clear(); 
                    currentDrawingPath.Add(currentCell);
                    lastPaintedCell = currentCell;
                    e.Use();
                }

                if (isDrawing && e.type == EventType.MouseDrag)
                {
                    if (currentCell != lastPaintedCell)
                    {
                        int distance = Mathf.Abs(currentCell.x - lastPaintedCell.x) + Mathf.Abs(currentCell.y - lastPaintedCell.y);
                        
                        if (distance == 1) 
                        {
                            if (currentDrawingPath.Contains(currentCell) || IsCellOccupied(currentCell))
                            {
                                e.Use();
                                return;
                            }

                            currentDrawingPath.Add(currentCell);
                            lastPaintedCell = currentCell;
                            Repaint();
                        }
                    }
                    e.Use();
                }
            }
        }

        if (isDrawing && (e.type == EventType.MouseUp || e.rawType == EventType.MouseUp))
        {
            isDrawing = false;
            
            if (currentDrawingPath.Count > 1)
            {
                FinalizeDrawnArrow();
            }
            else
            {
                currentDrawingPath.Clear(); 
            }
            
            lastPaintedCell = new Vector2Int(-1, -1);
            Repaint();
        }
    }

    private bool IsCellOccupied(Vector2Int cell)
    {
        foreach (ArrowData arrow in arrows)
        {
            if (arrow.cells.Contains(cell))
                return true;
        }

        return false;
    }

    private void EraseArrowContentAt(Vector2Int cell)
    {
        for (int i = arrows.Count - 1; i >= 0; i--)
        {
            if (arrows[i].cells.Contains(cell))
            {
                arrows.RemoveAt(i);
                Repaint();
                return;
            }
        }
    }

    private void FinalizeDrawnArrow()
    {
        ArrowData newArrow = new ArrowData();
        newArrow.id = arrowId;
        newArrow.cells = new List<Vector2Int>(currentDrawingPath);

        int lastIndex = currentDrawingPath.Count - 1;
        Vector2Int finalMove = currentDrawingPath[lastIndex] - currentDrawingPath[lastIndex - 1];
        newArrow.direction = DirectionUtility.ToArrowDirection(finalMove);

        arrows.Add(newArrow);

        currentDrawingPath.Clear();
        arrowId++;
    }
}
