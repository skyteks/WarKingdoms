using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class FindGameObjects : EditorWindow
{
    private enum SelectionType
    {
        Selection,
        Scenes,
        Everywhere,
    }

    private static int go_count = 0, components_count = 0, missing_count = 0;

    //public static void ShowWindow()
    //{
    //    EditorWindow.GetWindow(typeof(FindMissingScripts));
    //}

    //public void OnGUI()
    //{
    //    if (GUILayout.Button("Find Missing Scripts in selected GameObjects"))
    //    {
    //        FindInSelected();
    //    }
    //}

    [MenuItem("Tools/CleanUp/Find hidden GameObjects/In Selection")]
    public static void FindHiddenInSelected()
    {
        FindHidden(GetSelection(SelectionType.Selection));
    }

    [MenuItem("Tools/CleanUp/Find hidden GameObjects/In Scenes")]
    public static void FindHiddenInScenes()
    {
        FindHidden(GetSelection(SelectionType.Scenes));
    }

    [MenuItem("Tools/CleanUp/Find hidden GameObjects/In Scenes including Assets")]
    public static void FindHiddenInAssets()
    {
        FindHidden(GetSelection(SelectionType.Everywhere));
    }

    [MenuItem("Tools/CleanUp/Find missing Scripts/In Selection")]
    public static void FindMissingInSelected()
    {
        FindMissingScripts(GetSelection(SelectionType.Selection));
    }

    [MenuItem("Tools/CleanUp/Find missing Scripts/In Scenes")]
    public static void FindMissingInScenes()
    {
        FindMissingScripts(GetSelection(SelectionType.Scenes));
    }

    [MenuItem("Tools/CleanUp/Find missing Scripts/In Scenes including Assets")]
    public static void FindMissingInAssets()
    {
        FindMissingScripts(GetSelection(SelectionType.Everywhere));
    }

    private static GameObject[] GetSelection(SelectionType selectionType)
    {
        switch (selectionType)
        {
            default:
            case SelectionType.Selection:
                return Selection.gameObjects;
            case SelectionType.Scenes:
                return GameObject.FindObjectsOfType<GameObject>();
            case SelectionType.Everywhere:
                //return (GameObject.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]);
                return Resources.FindObjectsOfTypeAll<GameObject>();
        }
    }

    private static void FindMissingScripts(GameObject[] gos)
    {
        go_count = 0;
        components_count = 0;
        missing_count = 0;
        foreach (GameObject gameObject in gos)
        {
            FindMissingScriptsInGameObject(gameObject);

            // Now recurse through each child GO (if there are any):
            foreach (Transform childTransform in gameObject.transform)
            {
                //Debug.Log("Searching " + childT.name  + " " );
                FindMissingScriptsInGameObject(childTransform.gameObject);
            }
        }
        Debug.Log("<color=magenta><b>[Find]</b></color> " + string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
    }

    private static void FindMissingScriptsInGameObject(GameObject g)
    {
        go_count++;
        Component[] components = g.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            components_count++;
            if (components[i] == null)
            {
                missing_count++;
                string s = g.name;
                Transform t = g.transform;
                while (t.parent != null)
                {
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }
                Debug.Log("<color=magenta><b>[Find]</b></color> " + s + " has an empty script attached in position: " + i, g);
            }
        }
    }

    private static IEnumerable FindHidden(GameObject[] gos)
    {
        List<GameObject> hidden = new List<GameObject>();
        go_count = 0;
        missing_count = 0;
        foreach (GameObject gameObject in gos)
        {
            if(CheckIfHiddenGameObject(gameObject)) hidden.Add(gameObject);

            // Now recurse through each child GO (if there are any):
            foreach (Transform childTransform in gameObject.transform)
            {
                //Debug.Log("Searching " + childT.name  + " " );
                if(CheckIfHiddenGameObject(childTransform.gameObject)) hidden.Add(gameObject);
            }
        }
        Debug.Log("<color=magenta><b>[Find]</b></color> " + string.Format("Searched {0} GameObjects, found {1} hidden", go_count, missing_count));
        return hidden;
    }

    [MenuItem("Tools/CleanUp/Delete all hidden objects")]
    public static void DeleteHiddenObjects()
    {
        var hidden = FindHidden(GetSelection(SelectionType.Everywhere));
        foreach(GameObject go in hidden)
        {
            if(go != null) DestroyImmediate(go);
        }
    }

    private static bool CheckIfHiddenGameObject(GameObject g)
    {
        go_count++;
        if (g.hideFlags != HideFlags.None)
        {
            missing_count++;
            string path = g.name;
            Transform transform = g.transform;
            while (transform.parent != null)
            {
                path = transform.parent.gameObject.name + "/" + path;
                transform = transform.parent;
            }
            path = "<b>(SCENE: " + transform.gameObject.scene.name + ")</b>/" + path;
            Debug.Log("<color=magenta><b>[Find]</b></color> " + g.name + " is in State: " + g.hideFlags.ToString() + "\n" + path, g);
            return true;
        }
        return false;
    }

    [MenuItem("Tools/CleanUp/Get Selected GameObjects Info")]
    public static void GetSelectedGameObjectsInfo()
    {
        GameObject[] gos = Selection.gameObjects;
        foreach (GameObject gameObject in gos)
        {
            string info = "";

            string path = gameObject.name;
            Transform transform = gameObject.transform;
            while (transform.parent != null)
            {
                path = transform.parent.gameObject.name + "/" + path;
                transform = transform.parent;
            }
            path = "<b>(SCENE: " + transform.gameObject.scene.name + ")</b>/" + path;
            info += "Path: " + path + "\n";

            info += "HideFlags: " + gameObject.hideFlags.ToString() + "\n";

            //info += "Prefab: " + (gameObject.IsPrefab).ToString() + "\n";

            info += "Children: " + gameObject.transform.childCount + "\n";

            Debug.Log("<color=magenta><b>[Find]</b></color> " + gameObject.name + " Info:\n" + info, gameObject);
        }
    }
}