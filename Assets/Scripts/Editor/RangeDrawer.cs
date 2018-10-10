using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Range))]
public class RangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var amountRect = new Rect(position.x, position.y, position.width / 2f, position.height);
        var unitRect = new Rect(position.x + position.width / 2f, position.y, position.width / 2f, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUIUtility.labelWidth = 25f;
        EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("Min"), new GUIContent("Min"));
        EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("Max"), new GUIContent("Max"));
        EditorGUIUtility.labelWidth = 0f;

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}