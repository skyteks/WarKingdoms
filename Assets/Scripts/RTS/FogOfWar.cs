using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Fog of war texture drawer [obsolete]
/// </summary>
public class FogOfWar : MonoBehaviour
{
    public Texture2D readWriteTexture;
    [Range(0f, 1f)]
    public float fogScatterSpeed = 0.2f;
    [Range(0f, 1f)]
    public float hiddenAlpha = 0.3f;

    private Color[] pixels;

    private int layerDefaultVisible;
    private int layerDefaultHidden;
    private int layerMiniMapVisible;
    private int layerMiniMapHidden;

    private int textureWidth;
    private int textureHeight;

    void Start()
    {
        textureHeight = readWriteTexture.height;
        textureWidth = readWriteTexture.width;

        ResetTexture();

        layerDefaultVisible = LayerMask.NameToLayer("Default");
        layerDefaultHidden = LayerMask.NameToLayer("Default Hidden");
        layerMiniMapVisible = LayerMask.NameToLayer("MiniMap Only");
        layerMiniMapHidden = LayerMask.NameToLayer("MiniMap Hidden");
    }

    void Update()
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            Color tmp = pixels[i];
            tmp.a += fogScatterSpeed * Time.deltaTime;
            pixels[i] = tmp;
        }

        UpdateUnitsSight();
        readWriteTexture.SetPixels(pixels);
        readWriteTexture.Apply();

        UpdateUnitsVisibility();
    }

    [ContextMenu("Reset Texture")]
    private void ResetTexture()
    {
        pixels = readWriteTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }

        readWriteTexture.SetPixels(pixels);
        readWriteTexture.Apply();
    }

    private void UpdateUnitsSight()
    {
        var units = GameManager.Instance.GetAllSelectableUnits();
        foreach (Unit unit in units)
        {
            Vector2Int uv = WorldToTextureUV(unit.transform.position);
            int radius = Mathf.RoundToInt((unit.template.guardDistance / (transform.localScale.x + transform.localScale.z) * 2f) * readWriteTexture.height);
            ClearRadius(uv, radius);
        }
    }

    private void UpdateUnitsVisibility()
    {
        var units = GameManager.Instance.GetAllNonSelectableUnits();
        foreach (Unit unit in units)
        {
            Vector2Int uv = WorldToTextureUV(unit.transform.position);
            //int index = GetPixelIndexOnUV(uv);
            Color pixel = readWriteTexture.GetPixel(uv.x, uv.y);
            Debug.Log(pixel.ToString());
            float visibility = pixel.a;
            IEnumerable<GameObject> parts = unit.GetComponentsInChildren<Transform>().Where(form =>
                form.gameObject.layer == layerDefaultVisible ||
                form.gameObject.layer == layerDefaultHidden ||
                form.gameObject.layer == layerMiniMapVisible ||
                form.gameObject.layer == layerMiniMapHidden
            ).Select(form => form.gameObject);
            foreach (GameObject part in parts)
            {
                if (part.layer == layerDefaultVisible || part.layer == layerDefaultHidden)
                {
                    if (visibility < hiddenAlpha) part.layer = layerDefaultVisible;
                    else part.layer = layerDefaultHidden;
                }
                else
                {
                    if (visibility < hiddenAlpha) part.layer = layerMiniMapVisible;
                    else part.layer = layerMiniMapHidden;
                }
            }
        }
    }

    private void ClearRadius(Vector2Int uv, int radius)
    {
        for (int y = -radius; y < radius; y++)
        {
            for (int x = -radius; x < radius; x++)
            {
                Vector2Int tmp = uv;
                if (Mathf.Sqrt(x * x + y * y) <= radius)
                {
                    tmp.x += x;
                    tmp.y += y;
                    int index = GetPixelIndexOnUV(tmp);
                    pixels[index] = Color.clear;
                }
            }
        }
    }

    private Vector2Int WorldToTextureUV(Vector3 position)
    {
        return new Vector2Int(
                    Mathf.RoundToInt((position.x / transform.localScale.x) * textureWidth) + (textureWidth / 2),
                    Mathf.RoundToInt((position.z / transform.localScale.z) * textureHeight) + (textureHeight / 2)
                );
    }

    private int GetPixelIndexOnUV(Vector2Int uv)
    {
        int xPixel = Mathf.Clamp(uv.x, 0, textureWidth - 1);
        int yPixel = Mathf.Clamp(uv.y, 0, textureHeight - 1);
        int index = xPixel + yPixel * textureWidth;
        return index;
    }
}
