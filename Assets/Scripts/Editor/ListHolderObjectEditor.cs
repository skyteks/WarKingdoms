using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(RegisterObject))]
public class RegisterObjectEditor : Editor
{
    private RegisterObject registerObject;

    private ReorderableList reorderableList;

    void OnEnable()
    {
        if (targets.Length == 1)
        {
            registerObject = (target as RegisterObject);
            reorderableList = new ReorderableList(registerObject.listForEditor, registerObject.typeOfListObjects, false, true, false, false);
            reorderableList.drawHeaderCallback += DrawHeaderCallBack;
            reorderableList.drawElementCallback += DrawElementCallback;
        }
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        reorderableList.DoLayoutList();
    }

    private void DrawHeaderCallBack(Rect rect)
    {
        EditorGUI.LabelField(rect, registerObject.typeOfListObjects == null ? string.Concat(typeof(MonoBehaviour).ToString(), " [0]") : string.Concat(registerObject.typeOfListObjects.ToString(), " [", registerObject.listForEditor.Count, "]"));
    }

    private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        MonoBehaviour obj = registerObject.listForEditor[index];
        /*
        Rect[] rects = rect.SplitOnXAxis(0.5f);
        for (int i = 0; i < rects.Length; i++)
        {
            rects[i].height = EditorGUIUtility.singleLineHeight;
        }

        rects[0].xMax -= 15f / 2f;
        rects[1].xMin += 15f / 2f;
        */
        //EditorGUI.BeginDisabledGroup(true);
        //EditorGUI.EnumFlagsField(rects[0], );
        EditorGUI.ObjectField(rect, obj, registerObject.typeOfListObjects, true);
        //EditorGUI.EndDisabledGroup();
    }


    void DrawTemplateEditor(Object obj, ref bool foldout, ref Editor editor)
    {
        if (obj != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, obj);
            if (foldout)
            {
                CreateCachedEditor(obj, null, ref editor);
                editor.OnInspectorGUI();
            }
        }
    }
}
