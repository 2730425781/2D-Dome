using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGenerator : EditorWindow
{
    private TextAsset mapLayoutFile;
    private string mapLayoutText = "";
    private Vector2 scrollPos;

    // Map character to tile asset name
    private static readonly Dictionary<char, string> TileMap = new Dictionary<char, string>
    {
        { '.', "blank_0" },

        // Floor tiles
        { 'F', "floor_tile_1_0" }, { 'G', "floor_tile_2_0" },
        { 'H', "floor_tile_3_0" }, { 'I', "floor_tile_4_0" },
        { 'J', "floor_wood_1_0" }, { 'K', "floor_wood_2_0" },
        { 'L', "floor_wood_3_0" }, { 'M', "floor_wood_4_0" },
        { ';', "floor_tile_carpet_1_0" }, { ':', "floor_tile_carpet_2_0" },
        { '\"', "floor_tile_carpet_3_0" }, { ',', "floor_tile_carpet_4_0" },

        // Walls
        { 'W', "wall_1_0" }, { 'X', "wall_2_0" }, { 'Y', "wall_3_0" }, { 'Z', "wall_4_0" },
        { '5', "wall_5_0" }, { '6', "wall_6_0" }, { '7', "wall_7_0" }, { '8', "wall_8_0" },
        { '9', "wall_9_0" }, { '0', "wall_10_0" },
        { 'a', "wall_11_0" }, { 'b', "wall_12_0" }, { 'c', "wall_13_0" },
        { '#', "wall_15_0" }, { '@', "wall_16_0" },

        // Ceilings
        { 'C', "ceiling_1_0" }, { 'D', "ceiling_2_0" },
        { 'E', "ceiling_3_0" }, { 'A', "ceiling_4_0" },
        { 'B', "ceiling_corner_left_0" }, { 'N', "ceiling_corner_right_0" },

        // Windows
        { '[', "window_big_1_0" }, { ']', "window_big_2_0" },
        { '<', "window_small_1_0" }, { '>', "window_small_2_0" },

        // Side tiles
        { 's', "tile_side_left_0" }, { 'd', "tile_side_right_0" },

        // Columns
        { 'O', "column_1_0" }, { 'P', "column_2_0" },
        { 'Q', "column_3_0" }, { 'R', "column_4_0" },

        // Platforms
        { '1', "platform_1_0" }, { '2', "platform_2_0" },
        { '3', "platform_3_0" }, { '4', "platform_4_0" },
        { '~', "spikes_0" },
    };

    private const string TilePalettePath = "Assets/Tile Palette/Tile";

    [MenuItem("Tools/Tilemap Generator")]
    public static void ShowWindow()
    {
        GetWindow<TilemapGenerator>("Tilemap Generator");
    }

    [System.Obsolete]
    private void OnGUI()
    {
        GUILayout.Label("Tilemap Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Layout file selector
        EditorGUILayout.BeginHorizontal();
        mapLayoutFile = (TextAsset)EditorGUILayout.ObjectField("Layout File", mapLayoutFile, typeof(TextAsset), false);
        if (mapLayoutFile != null && GUILayout.Button("Load", GUILayout.Width(50)))
            mapLayoutText = mapLayoutFile.text;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Map layout text area
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        mapLayoutText = EditorGUILayout.TextArea(mapLayoutText, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // Action buttons
        if (GUILayout.Button("Generate Tilemap", GUILayout.Height(35)))
            GenerateTilemap();

        if (GUILayout.Button("Load Example Town"))
            mapLayoutText = GetExampleTownLayout();
        if (GUILayout.Button("Load Example Dungeon"))
            mapLayoutText = GetExampleDungeonLayout();
        if (GUILayout.Button("Show Legend"))
            ShowLegend();
    }

    private void ShowLegend()
    {
        string legend = @"Tile Legend:
.  = blank
FGHI = floor_tile (1-4)
JKLM = floor_wood (1-4)
;:  = floor_tile_carpet (1-2)
WXYZ = wall (1-4)
567890abc#@ = wall (5-16)
CC  = ceiling
[]  = window_big
<>  = window_small
sd  = tile_side_left/right
OPQR = column (1-4)
1234 = platform (1-4)
~   = spikes";
        EditorUtility.DisplayDialog("Tile Legend", legend, "OK");
    }

    [System.Obsolete]
    private void GenerateTilemap()
    {
        Grid grid = FindFirstObjectByType<Grid>();
        if (grid == null)
        {
            var go = new GameObject("Grid");
            grid = go.AddComponent<Grid>();
        }

        Tilemap tilemap = grid.GetComponentInChildren<Tilemap>();
        if (tilemap == null)
        {
            var go = new GameObject("Tilemap", typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(grid.transform);
            tilemap = go.GetComponent<Tilemap>();
        }

        // Parse layout
        string[] rows = mapLayoutText.Split('\n')
            .Select(r => r.TrimEnd('\r'))
            .Where(r => r.Length > 0)
            .ToArray();

        if (rows.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No rows found in layout!", "OK");
            return;
        }

        int width = rows.Max(r => r.Length);
        int height = rows.Length;

        // Center map around origin
        int originX = -width / 2;
        int originY = height / 2;

        Undo.RecordObject(tilemap, "Generate Tilemap");
        tilemap.ClearAllTiles();

        int placed = 0;
        for (int y = 0; y < height; y++)
        {
            string row = rows[y];
            for (int x = 0; x < row.Length; x++)
            {
                if (row[x] == ' ') continue;

                TileBase tile = ResolveTile(row[x]);
                if (tile != null)
                {
                    tilemap.SetTile(new Vector3Int(originX + x, originY - y, 0), tile);
                    placed++;
                }
            }
        }

        tilemap.RefreshAllTiles();

        // Ensure collider exists
        if (tilemap.GetComponent<TilemapCollider2D>() == null)
            tilemap.gameObject.AddComponent<TilemapCollider2D>();

        Selection.activeGameObject = grid.gameObject;
        EditorUtility.SetDirty(tilemap);

        Debug.Log($"Generated {placed} tiles ({height}x{width})");
        EditorUtility.DisplayDialog("Success", $"Generated {placed} tiles!", "OK");
    }

    private TileBase ResolveTile(char key)
    {
        if (!TileMap.TryGetValue(key, out string name)) return null;
        return AssetDatabase.LoadAssetAtPath<TileBase>($"{TilePalettePath}/{name}.asset");
    }

    private string GetExampleTownLayout()
    {
        return @". . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
...WWWWWWWWW...WWWWWWWWWW...WWWW...WWWWWWWWWWWW...WWW...
...WJJJJJJJW...WKKKKKKKKW...WJJW...WMMMMMMMMMMW...WJKW...
...WJJJJJJJW...WKKKKKKKKW...WJJW...WMMMMMMMMMMW...WJKW...
...WJJJJJJJW...WKKKKKKKKW...WJJW...WMMMMMMMMMMW...WJKW...
...WJJJJJJJW...WKKKKKKKKW...WFW...WMMMMMMMMMMW...WJKW...
...WWWWWWWWW...W.......KW...WJW...W..........W...WJKW...
...GGGGGGGGG...K.......WW...WJW...W..........W...WJKW...
...HHHHHHHHH...K.............WJW...W..........W...WJKW...
...GGGGGGGGG...K.............WJW...W..........W...WJKW...
...FFFFFFFFF...K.............WJW...W..........W...WJKW...
...GGGGGGGGG...W.......WW...WJW...W..........W...WJKW...
...HHHHHHHHH...WWWWWWWWWW...WJW...W..........W...WJKW...
...GGGGGGGGG................WJW...W..........W...WJKW...
...FFFFFFFFF................WJW...WWWWWWWWWWWW...WJKW...
...GGGGGGGGG................WJW........WW.......WJKW...
...HHHHHHHHH..........................JJ.........WJKW...
...GGGGGGGGG.................W......JJ...........WJKW...
...FFFFFFFFF..................WWWWWW.............WJKW...
...GGGGGGGGG.................................................
...HHHHHHHHH.................................................
...GGGGGGGGG.................................................
...FFFFFFFFF.................................................";
    }

    private string GetExampleDungeonLayout()
    {
        return @". . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . .
W########################W
#.......................#
#.......................#
#.......................#
#..WWWWWWWWWW..WWWWWWW..#
#..W.........W..O.....W..#
#..W.........W..P.....W..#
#..W.........W..O.....W..#
#..W.........W..P.....W..#
#..WWWWWWWWWW..WWWWWWW..#
#.......................#
#.......................#
#..QQQQQQQQQQ...........#
#..Q..........#...........#
#..Q..........#...........#
#..Q..........#...........#
#..QQQQQQQQQQ...........#
#.......................#
#.......................#
#.......................#
W########################W
. . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . .
. . . . . . . . . . . . . . . . . . . . . .";
    }
}