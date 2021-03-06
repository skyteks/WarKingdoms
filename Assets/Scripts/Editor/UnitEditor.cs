using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Unit)), CanEditMultipleObjects]
public class UnitEditor : Editor
{
    private Unit unit;

    private ReorderableList reorderableList;

    Editor templateEditor;
    bool templateFoldout;

    void OnEnable()
    {
        if (targets.Length == 1)
        {
            unit = (target as Unit);
            reorderableList = new ReorderableList(unit.GetCommandList(), typeof(AICommand), false, true, false, false);
            reorderableList.drawHeaderCallback += DrawHeaderCallBack;
            reorderableList.drawElementCallback += DrawElementCallback;
        }
    }

    private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        AICommand command = unit.GetCommandList()[index];
        Rect[] rects = rect.SplitOnXAxis(0.5f);
        for (int i = 0; i < rects.Length; i++)
        {
            rects[i].height = EditorGUIUtility.singleLineHeight;
        }

        rects[0].xMax -= 15f / 2f;
        rects[1].xMin += 15f / 2f;
        //EditorGUI.BeginDisabledGroup(true);
        EditorGUI.EnumFlagsField(rects[0], command.commandType);
        switch (command.commandType)
        {
            case AICommand.CommandTypes.AttackTarget:
                EditorGUI.ObjectField(rects[1], command.target, command.target.GetType(), true);
                break;
            case AICommand.CommandTypes.MoveTo:
            case AICommand.CommandTypes.AttackMoveTo:
            case AICommand.CommandTypes.Guard:
                EditorGUI.Vector3Field(rects[1], "", command.destination);
                break;
            default:
                break;
        }
        //EditorGUI.EndDisabledGroup();
    }

    private void DrawHeaderCallBack(Rect rect)
    {
        EditorGUI.LabelField(rect, "Commands Queue");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        if (targets.Length == 1)
        {
            reorderableList.DoLayoutList();
            DrawTemplateEditor(unit.template, ref templateFoldout, ref templateEditor);
            EditorPrefs.SetBool(nameof(templateFoldout), templateFoldout);
        }
        else
        {
            EditorGUILayout.HelpBox("multi-select Commands Queue not supported", MessageType.Info, true);
        }
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
