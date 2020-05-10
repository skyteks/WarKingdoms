using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PreviewAttribute))]
public class PreviewDrawer : PropertyDrawer
{
    private bool foldout;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUI.GetPropertyHeight(property, label, true);
        if (property.objectReferenceValue != null && foldout)
        {
            System.Type type = property.objectReferenceValue.GetType();
            if (type == typeof(Sprite))
            {
                height += (property.objectReferenceValue as Sprite).texture.height + EditorGUIUtility.standardVerticalSpacing;
            }
            else if (type == typeof(Texture))
            {
                height += (property.objectReferenceValue as Texture).height + EditorGUIUtility.standardVerticalSpacing;
            }
            else if (type == typeof(Material))
            {
                height += (property.objectReferenceValue as Material).mainTexture.height + EditorGUIUtility.standardVerticalSpacing;
            }
            else if (type.IsSubclassOf(typeof(ScriptableObject)))
            {
                var fields = type.GetFields();
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(Sprite))
                    {
                        //height += (property.objectReferenceValue as Sprite).texture.height;
                        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else if (field.FieldType == typeof(Texture))
                    {
                        //height += (property.objectReferenceValue as Texture).height;
                        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                    //else if (field.FieldType == typeof(Material))
                    //{
                    //    //height += (property.objectReferenceValue as Texture).height;
                    //    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    //    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    //}
                    else
                    {
                        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }
            else
            {
                height += EditorGUIUtility.singleLineHeight * 3f;
            }
        }
        return height + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label, true);

        if (property.objectReferenceValue == null)
        {
            return;
        }

        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        position.x += 15f;
        position.width -= 15f;

        foldout = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), foldout, "");

        EditorGUI.LabelField(position, "Preview");
        if (!foldout)
        {
            return;
        }
        position.y += EditorGUIUtility.singleLineHeight;

        System.Type type = property.objectReferenceValue.GetType();

        if (type == typeof(Sprite))
        {
            Texture texture = (property.objectReferenceValue as Sprite).texture;
            EditorGUI.LabelField(position, new GUIContent(texture));
            position.y += EditorGUIUtility.standardVerticalSpacing;
        }
        else if (type == typeof(Texture))
        {
            Texture texture = (property.objectReferenceValue as Texture);
            EditorGUI.LabelField(position, new GUIContent(texture));
            position.y += EditorGUIUtility.standardVerticalSpacing;
        }
        else if (type == typeof(Material))
        {
            Texture texture = (property.objectReferenceValue as Material).mainTexture;
            EditorGUI.LabelField(position, new GUIContent(texture));
            position.y += EditorGUIUtility.standardVerticalSpacing;
        }
        else if (type.IsSubclassOf(typeof(ScriptableObject)))
        {
            position.y += EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.BeginDisabledGroup(true);
            position.height = EditorGUIUtility.singleLineHeight;

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                Object obj = property.objectReferenceValue;
                object value = field.GetValue(obj);

                System.Type baseType = (field.FieldType.BaseType != typeof(System.ValueType)) ? field.FieldType.BaseType : field.FieldType;

                switch (baseType.Name)
                {
                    case "SByte":
                        EditorGUI.IntField(position, field.Name, (sbyte)value);
                        break;
                    case "Char":
                        //EditorGUI.IntField(position, field.Name, (char)value);
                        EditorGUI.TextField(position, field.Name, ((char)value).ToString());
                        break;
                    case "Int16":
                        EditorGUI.IntField(position, field.Name, (short)value);
                        break;
                    case "Int32":
                        EditorGUI.IntField(position, field.Name, (int)value);
                        break;
                    case "Int64":
                        EditorGUI.LongField(position, field.Name, (long)value);
                        break;
                    case "Byte":
                        EditorGUI.IntField(position, field.Name, (byte)value);
                        break;
                    case "UInt16":
                        EditorGUI.IntField(position, field.Name, (ushort)value);
                        break;
                    case "UInt32":
                        EditorGUI.LongField(position, field.Name, (uint)value);
                        break;
                    case "float":
                        EditorGUI.FloatField(position, field.Name, (float)value);
                        break;
                    case "Single":
                        EditorGUI.FloatField(position, field.Name, (float)((System.Single)value));
                        break;
                    case "Boolean":
                        EditorGUI.Toggle(position, field.Name, (bool)value);
                        break;
                    case "Enum":
                        EditorGUI.EnumFlagsField(position, field.Name, (System.Enum)value);
                        break;
                    case "Vector2":
                        EditorGUI.Vector2Field(position, field.Name, (Vector2)value);
                        break;
                    case "Vector2Int":
                        EditorGUI.Vector2IntField(position, field.Name, (Vector2Int)value);
                        break;
                    case "Vector3":
                        EditorGUI.Vector3Field(position, field.Name, (Vector3)value);
                        break;
                    case "Vector3Int":
                        EditorGUI.Vector3IntField(position, field.Name, (Vector3Int)value);
                        break;
                    case "Vector4":
                        EditorGUI.Vector4Field(position, field.Name, (Vector4)value);
                        break;
                    case "Rect":
                        EditorGUI.RectField(position, field.Name, (Rect)value);
                        break;
                    case "RectInt":
                        EditorGUI.RectIntField(position, field.Name, (RectInt)value);
                        break;
                    case "Bounds":
                        EditorGUI.BoundsField(position, field.Name, (Bounds)value);
                        break;
                    case "BoundsInt":
                        EditorGUI.BoundsIntField(position, field.Name, (BoundsInt)value);
                        break;
                    case "Color":
                        EditorGUI.ColorField(position, field.Name, (Color)value);
                        break;
                    case "Gradient":
                        EditorGUI.GradientField(position, field.Name, (Gradient)value);
                        break;
                    case "AnimationCurve":
                        EditorGUI.CurveField(position, field.Name, (AnimationCurve)value);
                        break;
                    case "Object":
                        EditorGUI.ObjectField(position, field.Name, (Object)value, field.FieldType, true);
                        if (field.FieldType == typeof(Sprite))
                        {
                            position.y += EditorGUIUtility.singleLineHeight;
                            Texture texture = (value as Sprite).texture;
                            EditorGUI.LabelField(position, new GUIContent(texture));
                            position.y += EditorGUIUtility.standardVerticalSpacing;
                        }
                        else if (field.FieldType == typeof(Texture))
                        {
                            position.y += EditorGUIUtility.singleLineHeight;
                            Texture texture = (value as Texture);
                            EditorGUI.LabelField(position, new GUIContent(texture));
                            position.y += EditorGUIUtility.standardVerticalSpacing;
                        }
                        //else if (field.FieldType == typeof(Material))
                        //{
                        //    position.y += EditorGUIUtility.singleLineHeight;
                        //    Texture texture = (value as Material).mainTexture;
                        //    EditorGUI.LabelField(position, new GUIContent(texture));
                        //    position.y += EditorGUIUtility.standardVerticalSpacing;
                        //}
                        break;
                    case "Range":
                        Range range = (Range)value;
                        RangeDrawer.Draw(position, range, new GUIContent(field.Name));
                        break;
                    default:
                        EditorGUI.LabelField(position, field.Name);
                        position.x += EditorGUIUtility.labelWidth;
                        EditorGUI.HelpBox(position, "Cannot draw property field type: " + field.FieldType, MessageType.Error);
                        position.x -= EditorGUIUtility.labelWidth;
                        break;
                }
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.yMax -= EditorGUIUtility.singleLineHeight * 1.5f;
            EditorGUI.HelpBox(position, "Use Preview only with type Sprite, Texture or ScriptableObject.", MessageType.Error);
        }
    }
}
