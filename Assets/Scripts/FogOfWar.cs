using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    public Texture2D fogTexture;

    private Color[] pixels;

    void Start()
    {
        pixels = fogTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }
        //Reset();
    }

    void Reset()
    {
        fogTexture.SetPixels(pixels);
        fogTexture.Apply();
    }

    void Update()
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            Color tmp = pixels[i];
            tmp.a += 0.2f * Time.deltaTime;
            pixels[i] = tmp;
        }

        UpdateUnitPositions();

        fogTexture.SetPixels(pixels);
        fogTexture.Apply();
    }

    void OnDestroy()
    {
        //Reset();
    }

    private void UpdateUnitPositions()
    {
        Unit[] units = GameManager.Instance.GetAllSelectableUnits();
        foreach (Unit unit in units)
        {
            Vector2Int position = new Vector2Int(
                    Mathf.RoundToInt((unit.transform.position.x / transform.localScale.x) * fogTexture.width) + (fogTexture.width / 2),
                    Mathf.RoundToInt((unit.transform.position.z / transform.localScale.z) * fogTexture.height) + (fogTexture.height / 2)
                );
            int radius = Mathf.RoundToInt((unit.template.guardDistance / transform.localScale.x) * fogTexture.height);
            ClearRadius(position, radius);
        }
    }

    private void ClearRadius(Vector2Int position, int radius)
    {
        for (int y = -radius; y < radius; y++)
        {
            for (int x = -radius; x < radius; x++)
            {
                if (Mathf.Sqrt(x * x + y * y) <= radius)
                {
                    int xPixel = Mathf.Clamp(Mathf.RoundToInt(position.x + x), 0, fogTexture.width - 1);
                    int yPixel = Mathf.Clamp(Mathf.RoundToInt(position.y + y), 0, fogTexture.height - 1);
                    int index = xPixel + yPixel * fogTexture.width;
                    pixels[index] = Color.clear;
                }
            }
        }
    }
}
