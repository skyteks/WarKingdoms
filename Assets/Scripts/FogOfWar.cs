using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    public Texture2D fogTexture;

    [Range(0f, 1f)]
    public float fogScatterSpeed = 0.2f;
    [Range(0f, 1f)]
    public float hiddenAlpha = 0.3f;

    private Color[] pixels;

    private int layerVisible;
    private int layerHidden;

    void Start()
    {
        pixels = fogTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }

        fogTexture.SetPixels(pixels);
        fogTexture.Apply();

        layerVisible = LayerMask.NameToLayer("Default");
        layerHidden = LayerMask.NameToLayer("Default Hidden");
    }

    void Update()
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            Color tmp = pixels[i];
            tmp.a += fogScatterSpeed * Time.deltaTime;
            pixels[i] = tmp;
        }

        UpdateUnitPositions();
        fogTexture.SetPixels(pixels);
        fogTexture.Apply();

        UpdateUnitVisibility();
    }

    private void UpdateUnitPositions()
    {
        Unit[] units = GameManager.Instance.GetAllSelectableUnits();
        foreach (Unit unit in units)
        {
            Vector2Int uv = WorldToTextureUV(unit.transform.position);
            int radius = Mathf.RoundToInt((unit.template.guardDistance / (transform.localScale.x + transform.localScale.z) * 2f) * fogTexture.height);
            ClearRadius(uv, radius);
        }
    }

    private void UpdateUnitVisibility()
    {
        Unit[] units = GameManager.Instance.GetAllNonSelectableUnits();
        foreach (Unit unit in units)
        {
            Vector2Int uv = WorldToTextureUV(unit.transform.position);
            //int index = GetPixelIndexOnUV(uv);
            Color pixel = fogTexture.GetPixel(uv.x, uv.y);
            IEnumerable<GameObject> parts = unit.GetComponentsInChildren<Transform>().Where(form =>
                form.gameObject.layer == layerVisible ||
                form.gameObject.layer == layerHidden
            ).Select(form => form.gameObject);
            foreach (GameObject part in parts)
            {
                if (pixel.a < hiddenAlpha) part.layer = layerVisible;
                else part.layer = layerHidden;
            }
        }
    }

    private void ClearRadius(Vector2Int uv, int radius)
    {
        for (int y = -radius; y < radius; y++)
        {
            for (int x = -radius; x < radius; x++)
            {
                if (Mathf.Sqrt(x * x + y * y) <= radius)
                {
                    int index = GetPixelIndexOnUV(uv + new Vector2Int(x, y));
                    pixels[index] = Color.clear;
                }
            }
        }
    }

    private Vector2Int WorldToTextureUV(Vector3 position)
    {
        return new Vector2Int(
                    Mathf.RoundToInt((position.x / transform.localScale.x) * fogTexture.width) + (fogTexture.width / 2),
                    Mathf.RoundToInt((position.z / transform.localScale.z) * fogTexture.height) + (fogTexture.height / 2)
                );
    }

    private int GetPixelIndexOnUV(Vector2Int uv)
    {
        int xPixel = Mathf.Clamp(Mathf.RoundToInt(uv.x), 0, fogTexture.width - 1);
        int yPixel = Mathf.Clamp(Mathf.RoundToInt(uv.y), 0, fogTexture.height - 1);
        int index = xPixel + yPixel * fogTexture.width;
        return index;
    }
}
