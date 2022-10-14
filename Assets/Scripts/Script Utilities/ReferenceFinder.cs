using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;


/// Reference finder - Finds prefabs that reference an object in unity
//		To use, stick this under a folder named "Editor", then right click an object/script in the project window and click "Find References"
//		Made by Dave Lloyd- www.powerhoof.com - but go nuts & use it however you want.
public class ReferenceFinder : EditorWindow
{
    Vector2 m_scrollPosition = Vector2.zero;
    List<GameObject> m_references = new List<GameObject>();
    List<string> m_paths = null;

    // Used to queue a call to FindObjectReferences() to avoid doing it mid-layout
    Object m_findReferencesAfterLayout = null;


    [MenuItem("Assets/Find References", false, 39)]
    static void FindObjectReferences()
    {
        //Show existing window instance. If one doesn't exist, make one.
        ReferenceFinder window = EditorWindow.GetWindow<ReferenceFinder>(true, "Find References", true);
        window.FindObjectReferences(Selection.activeObject);
    }

    void OnGUI()
    {
        GUILayout.Space(5);
        m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Found: " + m_references.Count);
        if (GUILayout.Button("Clear", EditorStyles.miniButton))
        {
            m_references.Clear();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        for (int i = m_references.Count - 1; i >= 0; --i)
        {
            LayoutItem(i, m_references[i]);
        }

        EditorGUILayout.EndScrollView();

        if (m_findReferencesAfterLayout != null)
        {
            FindObjectReferences(m_findReferencesAfterLayout);
            m_findReferencesAfterLayout = null;
        }
    }


    void LayoutItem(int i, UnityEngine.Object obj)
    {
        GUIStyle style = EditorStyles.miniButtonLeft;
        style.alignment = TextAnchor.MiddleLeft;

        if (obj != null)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(obj.name, style))
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }

            // Use "right arrow" unicode character 
            if (GUILayout.Button("\u25B6", EditorStyles.miniButtonRight, GUILayout.MaxWidth(20)))
            {
                m_findReferencesAfterLayout = obj;
            }

            GUILayout.EndHorizontal();
        }
    }

    /// Finds references to passed objects and puts them in m_references
    void FindObjectReferences(Object toFind)
    {
        EditorUtility.DisplayProgressBar("Searching", "Generating file paths", 0.0f);

        //
        // Get all prefabs in the project
        //
        if (m_paths == null)
        {
            m_paths = new List<string>();
            GetFilePaths("Assets", ".prefab", ref m_paths);
        }

        float progressBarPos = 0;
        int numPaths = m_paths.Count;
        int hundredthIteration = Mathf.Max(1, numPaths / 100); // So we only update progress bar 100 times, not for every item

        string toFindName = AssetDatabase.GetAssetPath(toFind);
        toFindName = System.IO.Path.GetFileNameWithoutExtension(toFindName);
        Object[] tmpArray = new Object[1];
        m_references.Clear();

        //
        // Loop through all files, and add any that have the selected object in it's list of dependencies
        //
        for (int i = 0; i < numPaths; ++i)
        {
            tmpArray[0] = AssetDatabase.LoadMainAssetAtPath(m_paths[i]);
            if (tmpArray != null && tmpArray.Length > 0 && tmpArray[0] != toFind) // Don't add self
            {
                Object[] dependencies = EditorUtility.CollectDependencies(tmpArray);
                if (System.Array.Exists(dependencies, item => item == toFind))
                {
                    // Don't add if another of the dependencies is already in there
                    m_references.Add(tmpArray[0] as GameObject);
                }

            }
            if (i % hundredthIteration == 0)
            {
                progressBarPos += 0.01f;
                EditorUtility.DisplayProgressBar("Searching", "Searching dependencies", progressBarPos);
            }
        }

        EditorUtility.DisplayProgressBar("Searching", "Removing redundant references", 1);

        //
        // Go through the references, get dependencies of each and remove any that have another dependency on the match list. We only want direct dependencies.
        //
        for (int i = m_references.Count - 1; i >= 0; i--)
        {
            tmpArray[0] = m_references[i];
            Object[] dependencies = EditorUtility.CollectDependencies(tmpArray);

            bool shouldRemove = false;

            for (int j = 0; j < dependencies.Length && shouldRemove == false; ++j)
            {
                Object dependency = dependencies[j];
                shouldRemove = (m_references.Find(item => item == dependency && item != tmpArray[0]) != null);
            }

            if (shouldRemove)
                m_references.RemoveAt(i);
        }

        EditorUtility.ClearProgressBar();
    }

    /// Recursively find all file paths with particular extention in a directory
    static void GetFilePaths(string startingDirectory, string extention, ref List<string> paths)
    {
        try
        {
            // Add any file paths with the correct extention
            string[] files = Directory.GetFiles(startingDirectory);
            for (int i = 0; i < files.Length; ++i)
            {
                string file = files[i];
                if (file.EndsWith(extention))
                {
                    paths.Add(file);
                }
            }

            // Recurse for all directories
            string[] directories = Directory.GetDirectories(startingDirectory);
            for (int i = 0; i < directories.Length; ++i)
            {
                GetFilePaths(directories[i], extention, ref paths);
            }
        }
        catch (System.Exception excpt)
        {
            Debug.LogError(excpt.Message);
        }
    }

}