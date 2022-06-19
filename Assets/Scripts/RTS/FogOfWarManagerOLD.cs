using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarManagerOLD : MonoBehaviour
{
    [System.Serializable]
    public struct UnitVision
    {
        // A bit mask representing a group of players inside the vision system.
        public FactionTemplate.PlayerID playerId;

        // The range of the vision (in world coordinates)
        public int visionRange;

        // the position (in world coordinates)
        public Vector2Int position;

        // used for blocking vision
        //public int terrainHeight;

        public UnitVision(FactionTemplate.PlayerID player, int range, Vector2Int pos, int height)
        {
            playerId = player;
            visionRange = range;
            position = pos;
            //terrainHeight = height;
        }
    }

    public class VisionGrid
    {
        // the width and height of the grid (needed to access the arrays)
        public Vector2Int size { get; private set; }

        // array of size width * height, each entry has an int with the 
        // bits representing which players have this entry in vision.
        private int[] values = null;

        // similar to the values but it just stores if a player visited
        // that entry at some point in time.
        private int[] visited = null;

        public VisionGrid(Vector2Int gridSize)
        {
            size = gridSize;
            values = new int[size.x * size.y];
            visited = new int[size.x * size.y];
        }

        public void SetVisible(Vector2Int pos, FactionTemplate.PlayerID players)
        {
            values[pos.x + pos.y * size.y] |= (int)players;
            visited[pos.x + pos.y * size.y] |= (int)players;
        }

        public void Clear()
        {
            if (values != null)
            {
                System.Array.Clear(values, 0, size.x * size.y);
            }
        }

        public bool IsVisible(int index, FactionTemplate.PlayerID players)
        {
            return (values[index] & (int)players) > 0;
        }

        public bool IsVisible(Vector2Int pos, FactionTemplate.PlayerID players)
        {
            return (values[pos.x + pos.y * size.y] & (int)players) > 0;
        }

        public bool WasVisible(int index, FactionTemplate.PlayerID players)
        {
            return (visited[index] & (int)players) > 0;
        }

        public bool WasVisible(Vector2Int pos, FactionTemplate.PlayerID players)
        {
            return (visited[pos.x + pos.y * size.y] & (int)players) > 0;
        }
    }

    public Vector2Int gridSize = new Vector2Int(128, 128);
    private VisionGrid visionGrid;
    private TerrainHeightMap terrainGrid;

    private Texture2D texture;
    private Color[] colors;
    public Material material;
    public Projector projector;
    private Color fowUnexplored = Color.black;
    private Color fowNotViewed = Color.black.ToWithA(0.6f);

    [Space]

    public bool drawGrid;
    public bool drawHeightValues;
    public Mesh drawQuadMesh;

    private static System.Comparison<Vector2Int> signedAngleComparison = new System.Comparison<Vector2Int>(Vector_Extension.SignedAngle);

    [Space]

    public RegisterObject units;
    public RegisterObject buildings;

    public FactionTemplate.PlayerID activePlayers;
    private List<UnitVision> unitVisions = new List<UnitVision>();

    private float lastCheck;

    public Vector2Int gridOffset
    {
        get
        {
            return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        }
    }

    void Start()
    {
        activePlayers = GameManager.Instance.playerFaction.data.playerID;


        visionGrid = new VisionGrid(gridSize);
        SetTerrain();

        colors = new Color[gridSize.x * gridSize.y];

        texture = new Texture2D(gridSize.x, gridSize.y);
        texture.name = "FOW grid";
        material.SetTexture("_MainTex", texture);
        projector.material = material;
    }

    void OnDrawGizmos()
    {
        if (drawGrid)
        {
            float heightOffset = transform.position.y;

            UnityEditor.Handles.color = Color.grey;
            for (int x = 0; x <= gridSize.x; x++)
            {
                UnityEditor.Handles.DrawAAPolyLine(new Vector3(x + gridOffset.x, heightOffset, 0 + gridOffset.y), new Vector3(x + gridOffset.x, heightOffset, gridSize.y + gridOffset.y));
            }
            for (int y = 0; y <= gridSize.y; y++)
            {
                UnityEditor.Handles.DrawAAPolyLine(new Vector3(0f + gridOffset.x, heightOffset, y + gridOffset.y), new Vector3(gridSize.x + gridOffset.x, heightOffset, y + gridOffset.y));
            }
        }
        if (drawHeightValues && terrainGrid != null)
        {
            float heightOffset = transform.position.y;

            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            int length = terrainGrid.size.x * terrainGrid.size.y;
            Camera cam = UnityEditor.SceneView.currentDrawingSceneView != null ? UnityEditor.SceneView.currentDrawingSceneView.camera : Camera.current;
            for (int i = 0; i < length; i++)
            {
                int height = terrainGrid.GetHeight(i);
                int x = i % gridSize.x;
                int y = i / gridSize.x;
                Vector3 point = new Vector3(x + 0.5f + gridOffset.x, heightOffset, y + 0.5f + gridOffset.y);
                Vector3 tmp = cam.WorldToViewportPoint(point);
                if (tmp.x.IsPercent() && tmp.y.IsPercent() && tmp.z > 0f && Vector3.Distance(cam.transform.position, point) < 30f)
                {
                    style.normal.textColor = Color.Lerp(Color.cyan, Color.blue, ((float)height).LinearRemap(0, 10));
                    UnityEditor.Handles.Label(point, height.ToString(), style);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (visionGrid != null)
        {
            int gridLenght = gridSize.x * gridSize.y;
            FactionTemplate.PlayerID playerId = GameManager.Instance.playerFaction.data.playerID;
            for (int i = 0; i < gridLenght; i++)
            {
                int x = i % gridSize.x;
                int y = i / gridSize.x;
                if (visionGrid.IsVisible(new Vector2Int(x, y), playerId))
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawCube(GridPosToUnityPos(new Vector2Int(x, y)), (Vector3.one * 0.9f).ToWithY(0.001f));
                }
            }
        }

        /*
        int radius = 10;
        List<Vector2Int> circle = GetCirclePositions(Vector2Int.zero, radius);
        Gizmos.color = Color.blue;
        foreach (Vector2Int pos in circle)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(GridPosToUnityPos(pos), (Vector3.one * 0.9f).ToWithY(0.001f));
        }
        Gizmos.color = Random.ColorHSV();
        Vector2Int rnd = circle[Random.Range(0, circle.Count)];
        List<Vector2Int> line = GetLinePositions(Vector2Int.zero, rnd);
        foreach (Vector2Int pos in line)
        {
            Gizmos.DrawCube(GridPosToUnityPos(pos), (Vector3.one * 0.9f).ToWithY(0.001f));
        }
        Gizmos.color = Color.green;
        Gizmos.DrawCube(GridPosToUnityPos(line[0]), (Vector3.one * 0.9f).ToWithY(0.001f));
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(GridPosToUnityPos(line[line.Count - 1]), (Vector3.one * 0.9f).ToWithY(0.001f));
        */

    }

    void LateUpdate()
    {
        if (Time.realtimeSinceStartup - lastCheck < 0.05f)
        {
            return;
        }
        CalculateVision();
        DrawVision();
        lastCheck = Time.realtimeSinceStartup;
    }

    [ContextMenu("SetTerrain")]
    private void SetTerrain()
    {
        terrainGrid = new TerrainHeightMap(gridSize, transform.position);
    }

    private void CalculateVision()
    {
        visionGrid.Clear();
        unitVisions.Clear();

        for (int i = 0; i < units.Count; i++)
        {
            Unit unit = units.GetByIndex(i) as Unit;
            Vector2Int unitPos = UnityPosToGridPos(unit.transform.position);
            int height = terrainGrid.GetHeight(unitPos);
            UnitVision vision = new UnitVision(unit.faction.data.playerID, Mathf.RoundToInt(unit.template.guardDistance), unitPos, height);
            unitVisions.Add(vision);
        }
        for (int i = 0; i < buildings.Count; i++)
        {
            Building unit = buildings.GetByIndex(i) as Building;
            Vector2Int unitPos = UnityPosToGridPos(unit.transform.position);
            int height = terrainGrid.GetHeight(unitPos);
            UnitVision vision = new UnitVision(unit.faction.data.playerID, Mathf.RoundToInt(unit.template.guardDistance), unitPos, height);
            unitVisions.Add(vision);
        }

        foreach (var unit in unitVisions)
        {
            int terrainHeight = terrainGrid.GetHeight(unit.position);
            List<Vector2Int> visionRange = GetCirclePositions(unit.position, unit.visionRange);
            foreach (var outlinePos in visionRange)
            {
                List<Vector2Int> line = GetLinePositions(unit.position, outlinePos);
                foreach (var linePos in line)
                {
                    if (terrainGrid.GetHeight(linePos) > terrainHeight)// unit.terrainHeight
                    {
                        break;
                    }
                    visionGrid.SetVisible(linePos, unit.playerId);
                }
            }
        }
    }

    private void DrawVision()
    {
        int length = gridSize.x * gridSize.y;
        for (int i = 0; i < length; i++)
        {
            //int x = i % gridSize.x;
            //int y = i / gridSize.x;
            if (colors[i] != fowUnexplored)
            {
                colors[i] = fowUnexplored;
            }

            if (visionGrid.IsVisible(i, activePlayers))
            {
                colors[i] = Color.clear;
            }
            else if (visionGrid.WasVisible(i, activePlayers))
            {
                colors[i] = fowNotViewed;
            }
        }
        texture.SetPixels(colors);
        texture.Apply();
    }

    public List<Vector2Int> GetCirclePositions(Vector2Int center, int radius)
    {
        List<Vector2Int> circle = new List<Vector2Int>();
        int x = 0;
        int y = radius;
        int d = 3 - 2 * radius;
        do
        {
            Vector2Int p;
            p = new Vector2Int(center.x + x, center.y + y);
            if (!circle.Contains(p))
            {
                circle.Add(p);
            }
            p = new Vector2Int(center.x - x, center.y + y);
            if (!circle.Contains(p))
            {
                circle.Add(p);
            }
            p = new Vector2Int(center.x + x, center.y - y);
            if (!circle.Contains(p))
            {
                circle.Add(p);
            }
            p = new Vector2Int(center.x - x, center.y - y);
            if (!circle.Contains(p))
            {
                circle.Add(p);
            }
            p = new Vector2Int(center.x + y, center.y + x);
            if (!circle.Contains(p))
            {
                circle.Add(p);
            }
            p = new Vector2Int(center.x - y, center.y + x);
            if (!circle.Contains(p))
            {
                circle.Add(p);
            }
            p = new Vector2Int(center.x + y, center.y - x);
            if (!circle.Contains(p))
            {
                circle.Add(p);
            }
            p = new Vector2Int(center.x - y, center.y - x);
            if (!circle.Contains(p))
            {
                circle.Add(p);
            }

            x++;
            if (d > 0)
            {
                y--;
                d = d + 4 * (x - y) + 10;
            }
            else
            {
                d = d + 4 * x + 6;
            }
        } while (y >= x);

        //circle = circle.OrderBy(pos => Vector2.SignedAngle(pos, Vector2.right) + 180f).ToList();
        circle.Sort(signedAngleComparison);
        int startLenght = circle.Count;
        for (int i = 0; i < startLenght; i++)
        {
            int j = (i + 1) % startLenght;
            List<Vector2Int> tmp = GetLinePositions(circle[i], circle[j]);
            foreach (Vector2Int lineOnCirclePos in tmp)
            {
                if (!circle.Contains(lineOnCirclePos))
                {
                    circle.Add(lineOnCirclePos);
                }
            }
        }

        return circle;
    }

    private List<Vector2Int> GetLinePositions(Vector2Int p0, Vector2Int p1)
    {
        int dx = p1.x - p0.x;
        int dy = p1.y - p0.y;
        int nx = Mathf.Abs(dx);
        int ny = Mathf.Abs(dy);
        int sign_x = dx > 0 ? 1 : -1;
        int sign_y = dy > 0 ? 1 : -1;

        Vector2Int p = p0;
        List<Vector2Int> line = new List<Vector2Int>();
        line.Add(p);

        for (int ix = 0, iy = 0; ix < nx || iy < ny;)
        {
            if (dx.Sign() != dy.Sign())
            {
                if ((2 * ix + 1) * ny < (2 * iy + 1) * nx)
                {
                    p.x += sign_x;
                    ix++;
                }
                else
                {
                    p.y += sign_y;
                    iy++;
                }
            }
            else
            {
                if ((2 * ix - 0) * ny < (2 * iy + 1) * nx)
                {
                    p.x += sign_x;
                    ix++;
                }
                else
                {
                    p.y += sign_y;
                    iy++;
                }
            }
            line.Add(p);
        }
        return line;
    }

    private Vector2Int UnityPosToGridPos(Vector3 position)
    {
        Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.z)) - gridOffset;
        return gridPos;
    }

    private Vector3 GridPosToUnityPos(Vector2Int gridPos)
    {
        gridPos += gridOffset;
        Vector3 position = new Vector3(gridPos.x + 0.5f, transform.position.y, gridPos.y + 0.5f);
        return position;
    }
}
