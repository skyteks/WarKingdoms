using System;
using UnityEditor;
using UnityEngine;

namespace UnitySampleAssets.ImageEffects.Inspector
{
    [CustomEditor(typeof(ColorCorrectionCurves))]
    public class ColorCorrectionCurvesEditor : Editor
    {
        private SerializedObject serObj;
        private SerializedProperty mode;
        private SerializedProperty redChannel;
        private SerializedProperty greenChannel;
        private SerializedProperty blueChannel;
        private SerializedProperty useDepthCorrection;
        private SerializedProperty depthRedChannel;
        private SerializedProperty depthGreenChannel;
        private SerializedProperty depthBlueChannel;
        private SerializedProperty zCurveChannel;
        private SerializedProperty saturation;
        private SerializedProperty selectiveCc;
        private SerializedProperty selectiveFromColor;
        private SerializedProperty selectiveToColor;

        private bool applyCurveChanges = false;

        private void OnEnable()
        {
            serObj = new SerializedObject(target);

            mode = serObj.FindProperty("mode");

            saturation = serObj.FindProperty("saturation");

            redChannel = serObj.FindProperty("redChannel");
            greenChannel = serObj.FindProperty("greenChannel");
            blueChannel = serObj.FindProperty("blueChannel");

            useDepthCorrection = serObj.FindProperty("useDepthCorrection");

            zCurveChannel = serObj.FindProperty("zCurve");

            depthRedChannel = serObj.FindProperty("depthRedChannel");
            depthGreenChannel = serObj.FindProperty("depthGreenChannel");
            depthBlueChannel = serObj.FindProperty("depthBlueChannel");

            if (redChannel.animationCurveValue.length == 0)
                redChannel.animationCurveValue = new AnimationCurve(new Keyframe(0, 0.0f, 1.0f, 1.0f),
                                                                    new Keyframe(1, 1.0f, 1.0f, 1.0f));
            if (greenChannel.animationCurveValue.length == 0)
                greenChannel.animationCurveValue = new AnimationCurve(new Keyframe(0, 0.0f, 1.0f, 1.0f),
                                                                      new Keyframe(1, 1.0f, 1.0f, 1.0f));
            if (blueChannel.animationCurveValue.length == 0)
                blueChannel.animationCurveValue = new AnimationCurve(new Keyframe(0, 0.0f, 1.0f, 1.0f),
                                                                     new Keyframe(1, 1.0f, 1.0f, 1.0f));

            if (depthRedChannel.animationCurveValue.length == 0)
                depthRedChannel.animationCurveValue = new AnimationCurve(new Keyframe(0, 0.0f, 1.0f, 1.0f),
                                                                         new Keyframe(1, 1.0f, 1.0f, 1.0f));
            if (depthGreenChannel.animationCurveValue.length == 0)
                depthGreenChannel.animationCurveValue = new AnimationCurve(new Keyframe(0, 0.0f, 1.0f, 1.0f),
                                                                           new Keyframe(1, 1.0f, 1.0f, 1.0f));
            if (depthBlueChannel.animationCurveValue.length == 0)
                depthBlueChannel.animationCurveValue = new AnimationCurve(new Keyframe(0, 0.0f, 1.0f, 1.0f),
                                                                          new Keyframe(1, 1.0f, 1.0f, 1.0f));

            if (zCurveChannel.animationCurveValue.length == 0)
                zCurveChannel.animationCurveValue = new AnimationCurve(new Keyframe(0, 0.0f, 1.0f, 1.0f),
                                                                       new Keyframe(1, 1.0f, 1.0f, 1.0f));

            serObj.ApplyModifiedProperties();

            selectiveCc = serObj.FindProperty("selectiveCc");
            selectiveFromColor = serObj.FindProperty("selectiveFromColor");
            selectiveToColor = serObj.FindProperty("selectiveToColor");
        }

        private void CurveGui(String name, SerializedProperty animationCurve, Color color)
        {
            // @NOTE: EditorGUILayout.CurveField is buggy and flickers, using PropertyField for now
            //animationCurve.animationCurveValue = EditorGUILayout.CurveField (GUIContent (name), animationCurve.animationCurveValue, color, Rect (0.0,0.0,1.0,1.0));
            EditorGUILayout.PropertyField(animationCurve, new GUIContent(name));
            if (GUI.changed)
                applyCurveChanges = true;
        }

        private void BeginCurves()
        {
            applyCurveChanges = false;
        }

        private void ApplyCurves()
        {
            if (applyCurveChanges)
            {
                serObj.ApplyModifiedProperties();
                (serObj.targetObject as ColorCorrectionCurves).gameObject.SendMessage("UpdateTextures");
            }
        }

        public override void OnInspectorGUI()
        {
            serObj.Update();

            GUILayout.Label("Use curves to tweak RGB channel colors", EditorStyles.miniBoldLabel);

            saturation.floatValue = EditorGUILayout.Slider("Saturation", saturation.floatValue, 0.0f, 5.0f);

            EditorGUILayout.PropertyField(mode, new GUIContent("Mode"));
            EditorGUILayout.Separator();

            BeginCurves();

            CurveGui(" Red", redChannel, Color.red);
            CurveGui(" Green", greenChannel, Color.green);
            CurveGui(" Blue", blueChannel, Color.blue);

            EditorGUILayout.Separator();

            if (mode.intValue > 0)
                useDepthCorrection.boolValue = true;
            else
                useDepthCorrection.boolValue = false;

            if (useDepthCorrection.boolValue)
            {
                CurveGui(" Red (depth)", depthRedChannel, Color.red);
                CurveGui(" Green (depth)", depthGreenChannel, Color.green);
                CurveGui(" Blue (depth)", depthBlueChannel, Color.blue);
                EditorGUILayout.Separator();
                CurveGui(" Blend Curve", zCurveChannel, Color.grey);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(selectiveCc, new GUIContent("Selective"));
            if (selectiveCc.boolValue)
            {
                EditorGUILayout.PropertyField(selectiveFromColor, new GUIContent(" Key"));
                EditorGUILayout.PropertyField(selectiveToColor, new GUIContent(" Target"));
            }


            ApplyCurves();

            if (!applyCurveChanges)
                serObj.ApplyModifiedProperties();
        }
    }
}