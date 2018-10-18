using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class TextureWriteAlphaToColor : EditorWindow
{
    private Texture2D texture;

    private bool running;
    private int index;
    private Coroutine coroutine;


    [MenuItem("Tools/Texture/WriteAlphaToColor")]
    private static void ShowWindow()
    {
        GetWindow<TextureWriteAlphaToColor>().Show();
    }

    void OnGUI()
    {
        texture = (EditorGUILayout.ObjectField("Texture to change", texture, typeof(Texture2D), false) as Texture2D);
        if (texture != null)
        {
            try
            {
                texture.GetPixel(0, 0);
            }
            catch (UnityException e)
            {
                Debug.LogError(e.Message);
                texture = null;
            }
        }

        if (texture != null)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Name", texture.name);
            EditorGUILayout.Vector2IntField("Resolution", new Vector2Int(texture.width, texture.height));
            EditorGUILayout.IntField("Pixel Count", texture.width * texture.height);
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(running);
        bool pressed = GUILayout.Button("Change");
        EditorGUI.EndDisabledGroup();
        if (pressed && coroutine == null)
        {
            //ChangeTexture();
            coroutine = GameManager.Instance.StartCoroutine(ChangeTexture());
        }

        EditorGUILayout.Space();

        bool pressed2 = false;
        if (running)
        {
            pressed2 = GUILayout.Button("Cancel");
        }
        if (pressed2 || texture == null)
        {
            if (coroutine != null) GameManager.Instance.StopCoroutine(coroutine);
            coroutine = null;
            index = 0;
            running = false;
        }

        if (running && texture != null)
        {
            Rect rect = new Rect(15f, 200f, EditorGUIUtility.currentViewWidth - 30f, EditorGUIUtility.singleLineHeight * 2f);
            float progress = (index / (float)(texture.width * texture.height));
            EditorGUI.ProgressBar(rect, progress, (progress * 100f).ToString("F3") + " %");
        }
    }

    private IEnumerator ChangeTexture()
    {
        running = true;
        index = 0;
        Debug.Log(texture.format.ToString());
        texture = Instantiate(texture);
        int width = texture.width;
        int height = texture.height;
        Color[] pixels = texture.GetPixels();
        yield return null;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //index = x + y * width;
                Color pixel = pixels[index];
                //pixel = pixel.ToScaleWithoutAlpha(pixel.a);
                pixel = new Color(pixel.r, pixel.r, pixel.r, pixel.r);
                pixels[index] = pixel;
                index++;
            }
            yield return null;
        }
        Debug.Log(texture.format.ToString());
        texture.SetPixels(pixels);
        texture.Apply();
        yield return null;

        byte[] bytes = texture.EncodeToPNG();
        string path = AssetDatabase.GetAssetPath(texture.GetInstanceID());
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(texture, path);
        AssetDatabase.SaveAssets();

        Debug.Log(texture + " successfull changed.", texture);
        texture = null;
        coroutine = null;
        running = false;
    }
}