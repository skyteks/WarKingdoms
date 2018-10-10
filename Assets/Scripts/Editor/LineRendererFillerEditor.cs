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

        this.reorderableList = new ReorderableList(serializedObject, property, true, true, true, true);
        this.reorderableList.drawElementCallback += this.DrawElementCallback;
        this.reorderableList.drawHeaderCallback += this.DrawHeaderCallback;
        this.reorderableList.onAddCallback += this.OnAddCallback;
        this.reorderableList.onRemoveCallback += this.OnRemoveCallback;
        this.reorderableList.onChangedCallback += this.OnChangedCallback;
    }

    private void OnChangedCallback(ReorderableList list)
    {
        this.serializedObject.ApplyModifiedProperties();
    }

    private void OnRemoveCallback(ReorderableList list)
    {
        property.DeleteArrayElementAtIndex(list.index);
        this.serializedObject.ApplyModifiedProperties();
    }

    private void OnAddCallback(ReorderableList list)
    {
        property.InsertArrayElementAtIndex(list.count);
        this.serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        this.reorderableList.DoLayoutList();
    }

    private void DrawHeaderCallback(Rect rect)
    {
        EditorGUI.LabelField(rect, "Transforms (" + this.property.arraySize + ")");
    }

    private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        rect.height -= 1f;

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index));
        if (EditorGUI.EndChangeCheck())
        {
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
