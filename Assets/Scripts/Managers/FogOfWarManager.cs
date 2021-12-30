using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    [System.Serializable]
    public struct UnitVision
    {
        // A bit mask representing a group of players inside the vision system.
        public FactionTemplate.PlayerId playerId;

        // The range of the vision (in world coordinates)
        public int visionRange;

        // the position (in world coordinates)
        public Vector2Int position;

        // used for blocking vision
        //public int terrainHeight;

        public UnitVision(FactionTemplate.PlayerId player, int range, Vector2Int pos, int height)
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

        public void SetVisible(Vector2Int pos, FactionTemplate.PlayerId players)
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

        public bool IsVisible(int index, FactionTemplate.PlayerId players)
        {
            return (values[index] & (int)players) > 0;
        }

        public bool IsVisible(Vector2Int pos, FactionTemplate.PlayerId players)
        {
            return (values[pos.x + pos.y * size.y] & (int)players) > 0;
        }

        public bool WasVisible(int index, FactionTemplate.PlayerId players)
        {
            return (visited[index] & (int)players) > 0;
        }

        public bool WasVisible(Vector2Int pos, FactionTemplate.PlayerId players)
        {
            return (visited[pos.x + pos.y * size.y] & (int)players) > 0;
        }
    }

    public class Terrain
    {
        // the width and height of the grid (needed to access the arrays) 
        public Vector2Int size { get; private set; }

        // array of size width * height, has the terrain level of the 
        // grid entry. 
        private int[] height;

        public Terrain(Vector2Int gridSize, Vector3 unityPos)
        {
            size = gridSize;
            CreateMap(unityPos);
        }

        private void CreateMap(Vector3 unityPos)
        {
            LayerMask layerMask = new LayerMask().Add("Terrain");
            float heightOffset = unityPos.y;
            int length = size.x * size.y;
            height = new int[length];
            Vector2Int gridOffset = new Vector2Int(Mathf.RoundToInt(unityPos.x), Mathf.RoundToInt(unityPos.z));
            for (int i = 0; i < length; i++)
            {
                int x = i % size.x;
                int y = i / size.x;
                Ray ray = new Ray(new Vector3(x + 0.5f + gridOffset.x, 24f, y + 0.5f + gridOffset.y), Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 32f, layerMask, QueryTriggerInteraction.Ignore))
                {
                    height[i] = Mathf.RoundToInt(hit.point.y);
                }
            }
        }

        public int GetHeight(int i)
        {
            return height[i];
        }

        public int GetHeight(Vector2Int pos)
        {
            if (!pos.InBounds(size))
            {
                throw new System.ArgumentOutOfRangeException(size.ToString());
            }
            return height[pos.x + pos.y * size.y];
        }
    }

    public Vector2Int gridSize = new Vector2Int(128, 128);
    private VisionGrid visionGrid;
    private Terrain terrainGrid;

    private Texture2D texture;
    private Color[] colors;
    public Material material;
    public Projector projector;
    private Color fowUnexplored = Color.black;
    private Color fowNotViewed = Color.black.ToWithA(0.6f);

    [Space]

    public bool drawGrid;
    public bool drawHeightValues;

    [Space]

    public RegisterObject units;
    public RegisterObject buildings;

    public FactionTemplate.PlayerId activePlayers;
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
        activePlayers = GameManager.Instance.playerFaction.data.playerId;


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
            FactionTemplate.PlayerId playerId = GameManager.Instance.playerFaction.data.playerId;
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
        terrainGrid = new Terrain(gridSize, transform.position);
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
            UnitVision vision = new UnitVision(unit.faction.data.playerId, Mathf.RoundToInt(unit.template.guardDistance), unitPos, height);
            unitVisions.Add(vision);
        }
        for (int i = 0; i < buildings.Count; i++)
        {
            Building unit = buildings.GetByIndex(i) as Building;
            Vector2Int unitPos = UnityPosToGridPos(unit.transform.position);
            int height = terrainGrid.GetHeight(unitPos);
            UnitVision vision = new UnitVision(unit.faction.data.playerId, Mathf.RoundToInt(unit.template.guardDistance), unitPos, height);
            unitVisions.Add(vision);
        }

        foreach (var unit in unitVisions)
        {
            int terrainHeight = terrainGrid.GetHeight(unit.position);
            List<Vector2Int> visionRange = GetCirclePositions(unit.position, unit.visionRange, visionGrid.size);
            foreach (var outlinePos in visionRange)
            {
                List<Vector2Int> line = GetLinePositions(unit.position, outlinePos, visionGrid.size);
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

    public static List<Vector2Int> GetCirclePositions(Vector2Int center, int radius, Vector2Int upperBounds)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        for (int i = 0; i <= 1; i++, radius--)
        {
            int x = 0, y = radius;
            int d = 3 - 2 * radius;
            do
            {
                Vector2Int p;
                p = new Vector2Int(center.x + x, center.y + y);
                if (!list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x - x, center.y + y);
                if (!list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x + x, center.y - y);
                if (!list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x - x, center.y - y);
                if (!list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x + y, center.y + x);
                if (!list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x - y, center.y + x);
                if (!list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x + y, center.y - x);
                if (!list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x - y, center.y - x);
                if (!list.Contains(p))
                {
                    list.Add(p);
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
        }

        return list;
    }

    private static List<Vector2Int> GetLinePositions(Vector2Int p0, Vector2Int p1, Vector2Int upperBounds)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        int dx = p1.x - p0.x;
        int dy = p1.y - p0.y;
        int N = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        float divN = (N == 0) ? 0f : 1f / N;
        float xstep = dx * divN;
        float ystep = dy * divN;
        float x = p0.x, y = p0.y;
        for (int step = 0; step <= N; step++, x += xstep, y += ystep)
        {
            Vector2Int p = new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
            if (p.InBounds(upperBounds))
            {
                list.Add(p);
            }
        }
        return list;
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
