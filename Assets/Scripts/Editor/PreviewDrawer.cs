using UnityEngine;
using UnityEditor;

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

        if (property.objectReferenceValue == null) return;

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
                    case "Enum":
                        EditorGUI.EnumFlagsField(position, field.Name, (System.Enum)value);
                        break;
                    case "Single":
                        EditorGUI.FloatField(position, field.Name, (float)((System.Single)value));
                        break;
                    case "float":
                        EditorGUI.FloatField(position, field.Name, (float)value);
                        break;
                    case "Int32":
                        EditorGUI.IntField(position, field.Name, (int)value);
                        break;
                    case "Object":
                        //if (field.FieldType == typeof(Sprite) || field.FieldType == typeof(Texture))// || field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                        //OnGUI (position, property, label);
                        EditorGUI.ObjectField(position, field.Name, (Object)value, field.FieldType, true);
                        position.y += EditorGUIUtility.singleLineHeight;
                        if (field.FieldType == typeof(Sprite))
                        {
                            Texture texture = (value as Sprite).texture;
                            EditorGUI.LabelField(position, new GUIContent(texture));
                        }
                        else if (field.FieldType == typeof(Texture))
                        {
                            Texture texture = (value as Texture);
                            EditorGUI.LabelField(position, new GUIContent(texture));
                        }
                        position.y += EditorGUIUtility.standardVerticalSpacing;
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
            EditorGUI.HelpBox(position, "Use Preview only with type Sprite, Texture or ScriptableObject.", MessageType.Warning);
        }
    }
}
