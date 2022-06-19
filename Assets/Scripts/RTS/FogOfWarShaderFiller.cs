using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarShaderFiller : MonoBehaviour
{
    public RegisterObject units;
    public RegisterObject buildings;
    public FactionTemplate.PlayerID activePlayers;

    [Space]

    public Vector2Int gridSize = new Vector2Int(128, 128);
    public Material material;
    private TerrainHeightMap terrainGrid;
    [ReadOnly, SerializeField]
    private Texture2D terrainHeightMap;
    private List<Vector4> unitPositionsAndRange = new List<Vector4>();

    void Start()
    {
        SetTerrain();
    }

    void Update()
    {
        //render.GetPropertyBlock(propertyBlock);
        SetUnitPositionsAndRangeInShader();
    }

    void LateUpdate()
    {
        for (int i = 0; i < units.Count; i++)
        {
            var unit = units.GetByIndex(i) as Unit;
            float range = unit.template.guardDistance;

            //UnityEditor.Handles.color = Color.red;
            //UnityEditor.Handles.DrawWireDisc(unit.transform.position, Vector3.up, range);

            Vector3 lastOff = unit.transform.position + Vector3.forward * range;
            for (int a = 1; a < 361; a++)
            {
                Vector3 off = unit.transform.position + Quaternion.Euler(0f, a, 0f) * Vector3.forward * range;
                Debug.DrawLine(lastOff, off, Color.red);
                lastOff = off;
            }
        }
    }

    private void SetUnitPositionsAndRangeInShader()
    {
        var enumValue = (FactionTemplate.PlayerID)System.Enum.Parse(typeof(FactionTemplate.PlayerID), activePlayers.ToString());

        unitPositionsAndRange.Clear();
        for (int i = 0; i < units.Count; i++)
        {
            var unit = units.GetByIndex(i) as Unit;
            var enumFlag = (FactionTemplate.PlayerID)System.Enum.Parse(typeof(FactionTemplate.PlayerID), unit.faction.data.playerID.ToString());
            if (enumValue.HasFlag(enumFlag))
            {
                unitPositionsAndRange.Add(unit.transform.position.ToVector4(unit.template.guardDistance));
            }
        }

        //propertyBlock.SetVectorArray("_UnitPositionsAndRange", unitPositionsAndRange);
        material.SetInteger("_UnitPositionsCount", unitPositionsAndRange.Count);
        material.SetVectorArray("_UnitPositionsAndRange", unitPositionsAndRange);
    }

    [ContextMenu("SetTerrain")]
    private void SetTerrain()
    {
        terrainGrid = new TerrainHeightMap(gridSize, transform.position);
        CreateTexture();
        SetTextureData();
    }

    private void CreateTexture()
    {
        terrainHeightMap = new Texture2D(gridSize.x, gridSize.y, TextureFormat.Alpha8, false);
        terrainHeightMap.filterMode = FilterMode.Point;
        terrainHeightMap.wrapMode = TextureWrapMode.Clamp;
        terrainHeightMap.name = "Terrain Heightmap (Generated)";
        terrainHeightMap.anisoLevel = 0;
        material.SetTexture("_MainTex", terrainHeightMap);
    }

    private void SetTextureData()
    {
        Unity.Collections.NativeArray<byte> data = terrainHeightMap.GetRawTextureData<byte>();
        int lenght = gridSize.x * gridSize.y;
        for (int i = 0; i < lenght; i++)
        {
            data[i] = (byte)(terrainGrid.GetHeight(i) * 1);
        }
        terrainHeightMap.Apply();
    }
}