using System;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Vector2 || property.type == typeof(Range).Name)
        {
            float min;
            float max;
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                min = property.vector2Value.x;
                max = property.vector2Value.y;
            }
            else
            {
                min = property.FindPropertyRelative("min").floatValue;
                max = property.FindPropertyRelative("max").floatValue;
            }
            MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
            label.tooltip = string.Format("{0}, {1}", min, max);
            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(position, label, ref min, ref max, attr.Min, attr.Max);

            if (attr.Step > 0f)
            {
                min -= (min % attr.Step);
                max -= (max % attr.Step);
            }

            position.xMin += EditorGUIUtility.labelWidth;
            position.yMin += EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;

            var fullWidth = position.width;
            var span = attr.Max - attr.Min;
            var minPos = fullWidth * (min - attr.Min) / span;
            var maxPos = fullWidth * (max - attr.Min) / span;

            position.xMin += minPos;
            position.width = maxPos - minPos;

            var style = new GUIStyle(EditorStyles.miniLabel);
            style.alignment = TextAnchor.UpperLeft;
            EditorGUI.LabelField(position, min.ToString("0.00"), style);
            style.alignment = TextAnchor.UpperRight;
            EditorGUI.LabelField(position, max.ToString("0.00"), style);

            if (EditorGUI.EndChangeCheck())
            {
                if (property.propertyType == SerializedPropertyType.Vector2)
                {
                    property.vector2Value = new Vector2(min, max);
                }
                else
                {
                    property.FindPropertyRelative("min").floatValue = Mathf.RoundToInt(min);
                    property.FindPropertyRelative("max").floatValue = Mathf.RoundToInt(max);
                }
            }
        }
        else
        {
            EditorGUI.HelpBox(position, "Use MinMaxSlider only with type Vector2 or Range.", MessageType.Warning);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
    }
}
