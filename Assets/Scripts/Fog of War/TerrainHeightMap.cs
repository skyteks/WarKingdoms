using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHeightMap
{
    // the width and height of the grid (needed to access the arrays) 
    public Vector2Int size { get; private set; }

    // array of size width * height, has the terrain level of the 
    // grid entry. 
    private byte[] height;

    public TerrainHeightMap(Vector2Int gridSize, int cellSize, Vector3 unityPos)
    {
        size = gridSize;
        CreateMap(unityPos, cellSize);
    }

    private void CreateMap(Vector3 unityPos, int cellSize)
    {
        LayerMask layerMask = new LayerMask().Add("Terrain");
        float heightOffset = unityPos.y;
        int length = size.x * size.y;
        height = new byte[length];
        Vector2Int gridOffset = new Vector2Int(Mathf.FloorToInt(unityPos.x) / cellSize, Mathf.FloorToInt(unityPos.z) / cellSize);
        for (int i = 0; i < length; i++)
        {
            int x = i % size.x;
            int y = i / size.x;
            Ray ray = new Ray(new Vector3(x * cellSize + 0.5f + gridOffset.x, 24f, y * cellSize + 0.5f + gridOffset.y), Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 32f, layerMask, QueryTriggerInteraction.Ignore))
            {
                height[i] = (byte)Mathf.RoundToInt(hit.point.y);
            }
        }
    }

    public byte GetHeight(int i)
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
