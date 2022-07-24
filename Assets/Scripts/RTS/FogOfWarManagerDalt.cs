using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class FogOfWarManagerDalt : MonoBehaviour
{
    [System.Serializable]
    public class VisionGrid
    {
        // the width and height of the grid (needed to access the arrays)
        public Vector2Int size { get; private set; }

        // array of size width * height, each entry has an int with the 
        // bits representing which players have this entry in vision.
        private byte[] target = null;

        // similar to the values but it just stores if a player visited
        // that entry at some point in time.
        private byte[] fadeout = null;

        private bool[] activeCells = null;
        public List<int> activeCellList = null;

        public VisionGrid(Vector2Int gridSize)
        {
            size = gridSize;
            int length = size.x * size.y;
            target = new byte[length];
            fadeout = new byte[length];

            for (int i = 0; i < length; i++)
            {
                target[i] = 0;
                fadeout[i] = 255;
            }

            activeCells = new bool[length];
            activeCellList = new List<int>(length);
        }

        public void SetVisible(Vector2Int pos)
        {
            int cell = pos.x + pos.y * size.y;
            if (activeCells[cell] == false)
            {
                activeCells[cell] = true;
                activeCellList.Add(cell);
            }

            fadeout[cell] = 100;
        }

        public void Update(byte decline)
        {
            for (var i = activeCellList.Count - 1; i >= 0; i--)
            {
                var targetIdx = activeCellList[i];
                var fade = fadeout[targetIdx];
                if (fade > decline)
                {
                    fade -= decline;
                    fadeout[targetIdx] = fade;
                    continue;
                }

                var targetValue = target[targetIdx];
                targetValue += decline;
                // we are not fading out to no-visibility at all
                if (targetValue >= 215)
                {
                    target[targetIdx] = 215;
                    activeCells[targetIdx] = false;

                    activeCellList[i] = activeCellList[activeCellList.Count - 1];
                    activeCellList.RemoveAt(activeCellList.Count - 1);
                }
                else
                {
                    target[targetIdx] = targetValue;
                }
            }
        }

        public byte this[int key]
        {
            get => fadeout[key];
        }
    }

    [System.Serializable]
    public class GridTreeCell
    {
        public Vector2Int localPos;// { get; private set; }
        public GridTreeCell parent { get; private set; }
        [SerializeField]
        private List<GridTreeCell> branchedOffCells;

        public GridTreeCell(Vector2Int pos, GridTreeCell parentCell = null)
        {
            localPos = pos;
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
            return pos == localPos || branchedOffCells.Exists(cell => cell.localPos == pos);// || parent != null && parent.ContainsChildAt(pos);
        }

        public List<GridTreeCell> GetChildren()
        {
            return branchedOffCells;
        }
    }

    public Vector2Int gridSize = new Vector2Int(128, 128);
    public int cellSize = 1;
    private VisionGrid visionGrid;
    private TerrainHeightMap terrainGrid;
    private RectInt gridBounds;

    private Texture2D texture;
    private Color[] colors;

    public Material material;
    public FilterMode filter = FilterMode.Bilinear;
    public Projector projector;
    public byte decline = 6;

    [Space]

    public int drawFunction;
    public bool drawGrid;
    public bool drawHeightValues;
    public Mesh drawQuadMesh;

    private static System.Comparison<Vector2Int> signedAngleComparison = new System.Comparison<Vector2Int>(Vector_Extension.SignedAngle);

    [Space]

    public RegisterObject units;
    public RegisterObject buildings;
    private List<ClickableObject> queueCurrent = new List<ClickableObject>();
    private int queueIndex = 0;
    public int perFrame = 10;

    public FactionTemplate.PlayerID activePlayersFlag;

    private float lastCheck;

    public float interpolateColorSpeed = 6f;

    public Vector2Int gridOffset
    {
        get
        {
            return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        }
    }

    public Dictionary<int, GridTreeCell> trees = new Dictionary<int, GridTreeCell>();

    void Start()
    {
        activePlayersFlag = GameManager.Instance.playerFaction.data.playerID;

        gridBounds = new RectInt(Vector2Int.zero, gridSize);
        visionGrid = new VisionGrid(gridSize);
        CreateTerrainHeightmap();
        int length = gridSize.x * gridSize.y;

        colors = new Color[length];
        for (int i = 0; i < length; i++)
        {
            colors[i] = Color.clear;
        }

        CreateTexture();
        SetupProjector();
    }

    void OnEnable()
    {
        units.onAdded += OnUnitAdd;
        buildings.onAdded += OnUnitAdd;
    }

    void OnDisable()
    {
        units.onRemoved += OnUnitRemove;
        buildings.onRemoved += OnUnitRemove;
    }

    void LateUpdate()
    {
        if (Time.realtimeSinceStartup - lastCheck < 0.2f)
        {
            //return;
        }

        bool fullIteration = CalculateVision();

        DrawVisionOnTextureWithRawData();

        if (fullIteration)
        {
            lastCheck = Time.realtimeSinceStartup;
        }
    }

    private void SetupProjector()
    {
        projector.transform.position = new Vector3(gridSize.x * cellSize / 2f, 32 - 8, gridSize.y * cellSize / 2f);
        projector.orthographic = true;
        projector.orthographicSize = Mathf.Max(gridSize.x, gridSize.y) / 2f;
        projector.material = material;
    }

    [ContextMenu("Create TerrainHeightmap")]
    private void CreateTerrainHeightmap()
    {
        terrainGrid = new TerrainHeightMap(gridSize, cellSize, transform.position);
    }

    private void CreateTexture()
    {
        texture = new Texture2D(gridSize.x, gridSize.y, TextureFormat.Alpha8, false);
        texture.filterMode = filter;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.name = "FOW grid (Generated)";
        material.SetTexture("_MainTex", texture);
    }

    private bool CalculateVision()
    {
        int length = queueCurrent.Count;
        for (int i = queueIndex; i < queueIndex + perFrame; i++)
        {
            if (i == length)
            {
                queueIndex = 0;
                return true;
            }
            ClickableObject unit = queueCurrent[i];
            FactionTemplate.PlayerID playersFlag = unit.faction.data.playerID;

            if (!playersFlag.HasFlag(activePlayersFlag))
            {
                continue;
            }
            Vector2Int unitPos = UnityPosToGridPos(unit.transform.position);
            int unitLocationHeight = terrainGrid.GetHeight(unitPos);
            int unitVisionRange = Mathf.RoundToInt(unit.template.guardDistance / cellSize);

            if (!trees.TryGetValue(unitVisionRange, out GridTreeCell rootCell))
            {
                rootCell = CreateGridSelectionTree(unitVisionRange);
                trees.Add(unitVisionRange, rootCell);
            }
            CheckVisionWithTree(rootCell, unitPos, unitLocationHeight, playersFlag);
        }
        queueIndex += perFrame;
        return false;
    }

    public void CheckVisionWithTree(GridTreeCell cell, Vector2Int unitPos, int unitLocationHeight, FactionTemplate.PlayerID playersFlag)
    {
        Vector2Int pos = cell.localPos + unitPos;
        if (!gridBounds.Contains(pos))
        {
            return;
        }
        int index = pos.ToIndex(gridSize.y);
        if (terrainGrid.GetHeight(pos) > unitLocationHeight)
        {
            return;
        }

        visionGrid.SetVisible(pos);

        List<GridTreeCell> children = cell.GetChildren();
        foreach (GridTreeCell childCell in children)
        {
            CheckVisionWithTree(childCell, unitPos, unitLocationHeight, playersFlag);
        }
    }

    public GridTreeCell CreateGridSelectionTree(int radius)
    {
        List<Vector2Int> circle = GetCirclePositions(Vector2Int.zero, radius);
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
        List<List<Vector2Int>> lines = new List<List<Vector2Int>>();
        foreach (var outlinePos in circle)
        {
            List<Vector2Int> line = GetLinePositions(Vector2Int.zero, outlinePos);
            line.RemoveAt(0);
            if (line.Count > 0)
            {
                lines.Add(line);
            }
        }

        GridTreeCell rootCell = new GridTreeCell(Vector2Int.zero);
        AddGridSelectionTreeLevel(lines, rootCell, 0);
        return rootCell;
    }

    private void AddGridSelectionTreeLevel(List<List<Vector2Int>> linePointLists, GridTreeCell cell, int level)
    {
        List<KeyValuePair<Vector2Int, List<List<Vector2Int>>>> firstPointDict = new List<KeyValuePair<Vector2Int, List<List<Vector2Int>>>>();
        for (int i = linePointLists.Count - 1; i >= 0; i--)
        {
            List<Vector2Int> line = linePointLists[i];
            Vector2Int linePos = line[0];

            List<List<Vector2Int>> lines = firstPointDict.Find(pair => pair.Key == linePos).Value;
            if (lines == null || lines.Count == 0)
            {
                lines = new List<List<Vector2Int>>();
                firstPointDict.Add(new KeyValuePair<Vector2Int, List<List<Vector2Int>>>(linePos, lines));
            }

            line.RemoveAt(0);
            linePointLists.RemoveAt(i);
            if (line.Count > 0)
            {
                lines.Add(line);
            }
        }

        for (int i = 0; i < firstPointDict.Count; i++)
        {
            Vector2Int linePos = firstPointDict[i].Key;
            List<List<Vector2Int>> lines = firstPointDict[i].Value;
            GridTreeCell brachCell = new GridTreeCell(linePos, cell);

            AddGridSelectionTreeLevel(lines, brachCell, level + 1);
        }
    }

    private void DrawVisionOnTextureWithRawData()
    {
        float lenght = gridSize.x * gridSize.y;

        Unity.Collections.NativeArray<byte> data = texture.GetRawTextureData<byte>();

        visionGrid.Update(decline);

        for (int i = 0; i < lenght; i++)
        {
            data[i] = visionGrid[i];
        }

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
        List<Vector2Int> line = new List<Vector2Int> { p };

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
                if ((2 * ix + 0) * ny < (2 * iy + 1) * nx)
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
        Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(position.x) / cellSize, Mathf.FloorToInt(position.z) / cellSize) - gridOffset;
        return gridPos;
    }

    private Vector3 GridPosToUnityPos(Vector2Int gridPos)
    {
        gridPos += gridOffset;
        Vector3 position = new Vector3(gridPos.x * cellSize + 0.5f, transform.position.y, gridPos.y * cellSize + 0.5f);
        return position;
    }

    public void OnUnitAdd(MonoBehaviour monoBehaviour, System.Type type)
    {
        if (monoBehaviour is ClickableObject)
        {
            ClickableObject clickableObject = monoBehaviour as ClickableObject;
            queueCurrent.Add(clickableObject);
        }
        else
        {
            throw new System.ArrayTypeMismatchException();
        }
    }

    public void OnUnitRemove(MonoBehaviour monoBehaviour, System.Type type)
    {
        if (monoBehaviour is ClickableObject)
        {
            ClickableObject clickableObject = monoBehaviour as ClickableObject;
            queueCurrent.Remove(clickableObject);
            queueIndex--;
        }
        else
        {
            throw new System.ArrayTypeMismatchException();
        }
    }
}
