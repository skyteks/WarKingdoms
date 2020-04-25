using UnityEditor;
using UnityEngine;

namespace UnitySampleAssets.ImageEffects.Inspector
{
    [CustomEditor(typeof (Vignetting))]
    public class VignettingEditor : Editor
    {
        private SerializedObject serObj;

        private SerializedProperty mode;
        private SerializedProperty intensity; // intensity == 0 disables pre pass (optimization)
        private SerializedProperty chromaticAberration;
        private SerializedProperty axialAberration;
        private SerializedProperty blur; // blur == 0 disables blur pass (optimization)
        private SerializedProperty blurSpread;
        private SerializedProperty luminanceDependency;

        private void OnEnable()
        {
            serObj = new SerializedObject(target);

            mode = serObj.FindProperty("mode");
            intensity = serObj.FindProperty("intensity");
            chromaticAberration = serObj.FindProperty("chromaticAberration");
            axialAberration = serObj.FindProperty("axialAberration");
            blur = serObj.FindProperty("blur");
            blurSpread = serObj.FindProperty("blurSpread");
            luminanceDependency = serObj.FindProperty("luminanceDependency");
        }

        public override void OnInspectorGUI()
        {
            serObj.Update();

            EditorGUILayout.LabelField("Simulates camera (lens) artifacts known as 'Vignette' and 'Aberration'",
                                       EditorStyles.miniLabel);

            EditorGUILayout.PropertyField(intensity, new GUIContent("Vignetting"));
            EditorGUILayout.PropertyField(blur, new GUIContent(" Blurred Corners"));
            if (blur.floatValue > 0.0f)
                EditorGUILayout.PropertyField(blurSpread, new GUIContent(" Blur Distance"));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(mode, new GUIContent("Aberration Mode"));
            if (mode.intValue > 0)
            {
                EditorGUILayout.PropertyField(chromaticAberration, new GUIContent("  Tangential Aberration"));
                EditorGUILayout.PropertyField(axialAberration, new GUIContent("  Axial Aberration"));
                luminanceDependency.floatValue = EditorGUILayout.Slider("  Contrast Dependency",
                                                                        luminanceDependency.floatValue, 0.001f, 1.0f);
            }
            else
                EditorGUILayout.PropertyField(chromaticAberration, new GUIContent(" Chromatic Aberration"));

            serObj.ApplyModifiedProperties();
        }
    }
}