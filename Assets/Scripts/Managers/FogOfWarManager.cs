using System.Collections;
using System.Collections.Generic;
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

    [System.Serializable]
    public class GridTreeCell
    {
        public Vector2Int localPos;// { get; private set; }
        public int level;// { get; private set; }
        public GridTreeCell parent { get; private set; }
        [SerializeField]
        private List<GridTreeCell> branchedOffCells;

        public GridTreeCell(Vector2Int pos, int treeLevel, GridTreeCell parentCell = null)
        {
            localPos = pos;
            level = treeLevel;
            branchedOffCells = new List<GridTreeCell>();
            parent = parentCell;
            parentCell?.AddBranchCell(this);
        }

        public void AddBranchCell(GridTreeCell branchCell)
        {
            branchedOffCells.Add(branchCell);
        }

        public bool ContainsChildAt(Vector2Int pos)
        {
            return pos == localPos || branchedOffCells.Exists(cell => cell.localPos == pos) || parent != null && parent.ContainsChildAt(pos);
        }

        public List<GridTreeCell> GetChildren()
        {
            return branchedOffCells;
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
    public Mesh drawQuadMesh;

    private static System.Comparison<Vector2Int> signedAngleComparison = new System.Comparison<Vector2Int>(Vector_Extension.SignedAngle);

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

    public GridTreeCell rootCell;

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

        if (rootCell != null)
        {
            DrawTreeLevel(new GridTreeCell[1] { rootCell }, Random.ColorHSV());
        }
    }

    private void DrawTreeLevel(IList<GridTreeCell> cells, Color color)
    {
        Color nextColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 0.8f);
        if (cells != null && drawQuadMesh != null)
        {
            foreach (GridTreeCell cell in cells)
            {
                Gizmos.color = color;
                Gizmos.DrawMesh(drawQuadMesh, 0, GridPosToUnityPos(cell.localPos) - Random.insideUnitCircle.ToVector3XZ() * 0.5f, Quaternion.identity, (Vector3.one * 0.2f).ToWithY(0.001f));
                DrawTreeLevel(cell.GetChildren(), nextColor);
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

    [ContextMenu("CreateGridSelectionTree")]
    public void CreateGridSelectionTree()//int radius)
    {
        int radius = 5;
        List<Vector2Int> circlePoints = GetCirclePositions(Vector2Int.zero, radius);
        int startLenght = circlePoints.Count;
        for (int i = 0; i < startLenght; i++)
        {
            int j = (i + 1) % startLenght;
            List<Vector2Int> tmp = GetLinePositions(circlePoints[i], circlePoints[j]);

            foreach (Vector2Int lineOnCirclePos in tmp)
            {
                if (!circlePoints.Contains(lineOnCirclePos))
                {
                    circlePoints.Add(lineOnCirclePos);
                }
            }
        }
        List<List<Vector2Int>> linePointLists = new List<List<Vector2Int>>();
        int maxListLenght = 0;
        foreach (var outlinePos in circlePoints)
        {
            List<Vector2Int> linePoints = GetLinePositions(Vector2Int.zero, outlinePos);
            linePoints.RemoveAt(0);
            if (linePoints.Count > 0)
            {
                linePointLists.Add(linePoints);
            }
            if (maxListLenght < linePoints.Count)
            {
                maxListLenght = linePoints.Count;
            }
        }

        int treeLevel = 0;
        rootCell = new GridTreeCell(Vector2Int.zero, treeLevel);

        AddGridSelectionTreeLevel(treeLevel + 1, linePointLists, rootCell);
    }

    private void AddGridSelectionTreeLevel(int treeLevel, List<List<Vector2Int>> linePointLists, GridTreeCell cell)
    {
        List<GridTreeCell> levelCells = new List<GridTreeCell>();

        List<List<Vector2Int>> linesWithPointInIt = new List<List<Vector2Int>>();
        for (int i = 0; i < linePointLists.Count; i++)
        {
            List<Vector2Int> list = linePointLists[i];
            Vector2Int linePos = list[0];
            //list.RemoveAt(0);
            if (!cell.ContainsChildAt(linePos))
            {
                GridTreeCell branchCell = new GridTreeCell(linePos, treeLevel, cell);
                levelCells.Add(branchCell);
            }
            if (list.Count <= 1)
            {
                linePointLists.RemoveAt(i);
                i--;
            }
        }
        foreach (GridTreeCell branchCell in levelCells)
        {
            for (int i = 0; i < linePointLists.Count; i++)
            {
                List<Vector2Int> list = linePointLists[i];
                if (list[0] == branchCell.localPos)
                {
                    List<Vector2Int> pointsList = new List<Vector2Int>(list);
                    pointsList.RemoveAt(0);
                    linesWithPointInIt.Add(pointsList);
                }
            }
            AddGridSelectionTreeLevel(treeLevel + 1, linesWithPointInIt, branchCell);
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
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(GridPosToUnityPos(circle[i]), GridPosToUnityPos(circle[j]));
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
