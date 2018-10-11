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
                height += ((Sprite)property.objectReferenceValue).texture.height;
            }
            else if (type == typeof(Texture))
            {
                height += ((Texture)property.objectReferenceValue).height;
            }
            else if (type.IsSubclassOf(typeof(ScriptableObject)))
            {
                height += type.GetFields().Length * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
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
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        System.Type type = property.objectReferenceValue.GetType();

        if (type == typeof(Sprite))
        {
            Texture texture = ((Sprite)property.objectReferenceValue).texture;
            EditorGUI.LabelField(position, new GUIContent(texture));
        }
        else if (type == typeof(Texture))
        {
            Texture texture = ((Texture)property.objectReferenceValue);
            EditorGUI.LabelField(position, new GUIContent(texture));
        }
        else if (type.IsSubclassOf(typeof(ScriptableObject)))
        {
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
                        EditorGUI.ObjectField(position, field.Name, (Object)value, field.FieldType, true);
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
