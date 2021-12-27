using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    [System.Serializable]
    struct UnitVision
    {
        // A bit mask representing a group of players inside the vision system.
        public FactionTemplate.PlayerId playerId;

        // The range of the vision (in world coordinates)
        public int visionRange;

        // the position (in world coordinates)
        public Vector2Int position;

        // used for blocking vision
        public int terrainHeight;

        public UnitVision(FactionTemplate.PlayerId player, int range, Vector2Int pos, int height)
        {
            playerId = player;
            visionRange = range;
            position = pos;
            terrainHeight = height;
        }
    }

    [System.Serializable]
    public class VisionGrid
    {
        // the width and height of the grid (needed to access the arrays)
        public Vector2Int size = new Vector2Int(128, 128);

        // array of size width * height, each entry has an int with the 
        // bits representing which players have this entry in vision.
        private int[] values = null;

        // similar to the values but it just stores if a player visited
        // that entry at some point in time.
        private int[] visited = null;

        public void Setup()
        {
            values = new int[size.x * size.y];
            visited = new int[size.x * size.y];
        }

        public void SetVisible(Vector2Int pos, FactionTemplate.PlayerId players)
        {
            if (values == null)
            {
                Setup();
            }
            values[pos.x + pos.y * size.y] |= (int)players;
            visited[pos.x + pos.y * size.y] |= (int)players;
        }

        public void Clear()
        {
            if (values == null)
            {
                Setup();
            }
            else
            {
                System.Array.Clear(values, 0, size.x * size.y);
            }
        }

        public bool IsVisible(Vector2Int pos, FactionTemplate.PlayerId players)
        {
            if (values == null)
            {
                Setup();
            }
            return (values[pos.x + pos.y * size.y] & (int)players) > 0;
        }

        public bool WasVisible(Vector2Int pos, FactionTemplate.PlayerId players)
        {
            if (values == null)
            {
                Setup();
            }
            return (visited[pos.x + pos.y * size.y] & (int)players) > 0;
        }
    }

    [System.Serializable]
    public class Terrain
    {
        // the width and height of the grid (needed to access the arrays) 
        public Vector2Int size { get; private set; }

        // array of size width * height, has the terrain level of the 
        // grid entry. 
        private int[] height;

        public Terrain(Vector2Int gridSize, int[] data)
        {
            size = gridSize;
            height = data;
        }

        public int GetHeight(int i)
        {
            return height[i];
        }

        public int GetHeight(Vector2Int pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x > size.x || pos.y > size.y)
            {
                throw new System.ArgumentOutOfRangeException(string.Concat(pos, " is not inside the grid of size: ", size));
            }
            return height[pos.x + pos.y * size.y];
        }
    }


    public Vector2Int gridOffset
    {
        get
        {
            return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        }
    }
    public VisionGrid visionGrid;
    private Terrain terrain;
    public bool drawGrid;
    public bool drawHeightValues;

    public RegisterObject units;
    public RegisterObject buildings;

    [Space]

    private FactionTemplate playerFaction;
    private List<UnitVision> unitVisions = new List<UnitVision>();

    void Start()
    {
        playerFaction = GameManager.Instance.playerFaction;

        SetTerrain();
    }

    void OnDrawGizmos()
    {
        if (drawGrid)
        {
            float heightOffset = transform.position.y;

            UnityEditor.Handles.color = Color.grey;
            for (int x = 0; x <= visionGrid.size.x; x++)
            {
                UnityEditor.Handles.DrawAAPolyLine(new Vector3(x + gridOffset.x, heightOffset, 0 + gridOffset.y), new Vector3(x + gridOffset.x, heightOffset, visionGrid.size.y + gridOffset.y));
            }
            for (int y = 0; y <= visionGrid.size.y; y++)
            {
                UnityEditor.Handles.DrawAAPolyLine(new Vector3(0f + gridOffset.x, heightOffset, y + gridOffset.y), new Vector3(visionGrid.size.x + gridOffset.x, heightOffset, y + gridOffset.y));
            }
        }
        if (drawHeightValues && terrain != null)
        {
            float heightOffset = transform.position.y;

            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            int length = terrain.size.x * terrain.size.y;
            Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
            for (int i = 0; i < length; i++)
            {
                int height = terrain.GetHeight(i);
                int x = i % visionGrid.size.x;
                int y = i / visionGrid.size.x;
                Vector3 point = new Vector3(x + 0.5f + gridOffset.x, heightOffset, y + 0.5f + gridOffset.y);
                Vector3 tmp = cam.WorldToViewportPoint(point);
                if (tmp.x.Is01() && tmp.y.Is01() && tmp.z > 0f && Vector3.Distance(cam.transform.position, point) < 30f)
                {
                    style.normal.textColor = Color.Lerp(Color.cyan, Color.blue, ((float)height).LinearRemap(0, 10));
                    UnityEditor.Handles.Label(point, height.ToString(), style);
                }
            }
        }

        int gridLenght = visionGrid.size.x * visionGrid.size.y;
        FactionTemplate.PlayerId playerId = GameManager.Instance.playerFaction.data.playerId;
        for (int i = 0; i < gridLenght; i++)
        {
            int x = i % visionGrid.size.x;
            int y = i / visionGrid.size.x;
            if (visionGrid.IsVisible(new Vector2Int(x, y), playerId))
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(GridPosToUnityPos(new Vector2Int(x, y)), (Vector3.one * 0.9f).ToWithY(0.001f));
            }
        }
    }

    void LateUpdate()
    {
        CalculateVision();
    }

    [ContextMenu("SetTerrain")]
    private void SetTerrain()
    {
        LayerMask layerMask = new LayerMask().Add("Terrain");
        float heightOffset = transform.position.y;
        int length = visionGrid.size.x * visionGrid.size.y;
        var heightData = new int[length];
        for (int i = 0; i < length; i++)
        {
            int x = i % visionGrid.size.x;
            int y = i / visionGrid.size.x;
            Ray ray = new Ray(new Vector3(x + 0.5f + gridOffset.x, 24f, y + 0.5f + gridOffset.y), Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 32f, layerMask, QueryTriggerInteraction.Ignore))
            {
                heightData[i] = Mathf.RoundToInt(hit.point.y);
            }
        }
        terrain = new Terrain(visionGrid.size, heightData);
    }

    private void CalculateVision()
    {
        visionGrid.Clear();
        unitVisions.Clear();

        for (int i = 0; i < units.Count; i++)
        {
            Unit unit = units.GetByIndex(i) as Unit;
            Vector2Int unitPos = UnityPosToGridPos(unit.transform.position);
            int height = terrain.GetHeight(unitPos);
            UnitVision vision = new UnitVision(unit.faction.data.playerId, Mathf.RoundToInt(unit.template.guardDistance), unitPos, height);
            unitVisions.Add(vision);
        }
        for (int i = 0; i < buildings.Count; i++)
        {
            Building unit = buildings.GetByIndex(i) as Building;
            Vector2Int unitPos = UnityPosToGridPos(unit.transform.position);
            int height = terrain.GetHeight(unitPos);
            UnitVision vision = new UnitVision(unit.faction.data.playerId, Mathf.RoundToInt(unit.template.guardDistance), unitPos, height);
            unitVisions.Add(vision);
        }

        foreach (var unit in unitVisions)
        {
            List<Vector2Int> visionRange = GetCirclePositions(unit.position, unit.visionRange, visionGrid.size);
            foreach (var outlinePos in visionRange)
            {
                List<Vector2Int> line = GetLinePositions(unit.position, outlinePos, visionGrid.size);
                foreach (var linePos in line)
                {
                    if (terrain.GetHeight(linePos) > unit.terrainHeight)
                    {
                        break;
                    }
                    visionGrid.SetVisible(linePos, unit.playerId);
                }
            }
        }
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
                if (p.InBounds(upperBounds) && !list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x - x, center.y + y);
                if (p.InBounds(upperBounds) && !list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x + x, center.y - y);
                if (p.InBounds(upperBounds) && !list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x - x, center.y - y);
                if (p.InBounds(upperBounds) && !list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x + y, center.y + x);
                if (p.InBounds(upperBounds) && !list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x - y, center.y + x);
                if (p.InBounds(upperBounds) && !list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x + y, center.y - x);
                if (p.InBounds(upperBounds) && !list.Contains(p))
                {
                    list.Add(p);
                }
                p = new Vector2Int(center.x - y, center.y - x);
                if (p.InBounds(upperBounds) && !list.Contains(p))
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
            list.Add(new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y)));
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
