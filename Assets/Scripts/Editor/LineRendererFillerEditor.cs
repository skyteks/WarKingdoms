using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(LineRendererFiller))]
public class LineRendererFillerEditor : Editor
{

    private LineRendererFiller script;
    private SerializedProperty property;

    private ReorderableList reorderableList;


    void OnEnable()
    {
        property = serializedObject.FindProperty("Transforms");

        reorderableList = new ReorderableList(serializedObject, property, true, true, true, true);
        reorderableList.drawElementCallback += DrawElementCallback;
        reorderableList.drawHeaderCallback += DrawHeaderCallback;
        reorderableList.onAddCallback += OnAddCallback;
        reorderableList.onRemoveCallback += OnRemoveCallback;
        reorderableList.onChangedCallback += OnChangedCallback;
    }

    private void OnChangedCallback(ReorderableList list)
    {
        serializedObject.ApplyModifiedProperties();
    }

    private void OnRemoveCallback(ReorderableList list)
    {
        property.DeleteArrayElementAtIndex(list.index);
        serializedObject.ApplyModifiedProperties();
    }

    private void OnAddCallback(ReorderableList list)
    {
        property.InsertArrayElementAtIndex(list.count);
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        reorderableList.DoLayoutList();
    }

    private void DrawHeaderCallback(Rect rect)
    {
        EditorGUI.LabelField(rect, "Transforms (" + property.arraySize + ")");
    }

    private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        rect.height -= 1f;

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
