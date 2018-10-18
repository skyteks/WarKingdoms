using UnityEditor;
using UnityEngine;

namespace UnitySampleAssets.ImageEffects.Inspector
{
    [CustomEditor(typeof(ColorCorrectionLut))]
    public class ColorCorrectionLutEditor : Editor
    {
        private SerializedObject serObj;

        private void OnEnable()
        {
            serObj = new SerializedObject(target);
        }

        private Texture2D tempClutTex2D;

        public override void OnInspectorGUI()
        {
            serObj.Update();

            EditorGUILayout.LabelField("Converts textures into color lookup volumes (for grading)",
                                       EditorStyles.miniLabel);

            Rect r;
            Texture2D t;

            //EditorGUILayout.Space();
            tempClutTex2D =
                EditorGUILayout.ObjectField(" Based on", tempClutTex2D, typeof (Texture2D), false) as Texture2D;
            if (tempClutTex2D == null)
            {
                t = AssetDatabase.LoadMainAssetAtPath((target as ColorCorrectionLut).basedOnTempTex) as Texture2D;
                if (t) tempClutTex2D = t;
            }

            Texture2D tex = tempClutTex2D;

            if (tex && (target as ColorCorrectionLut).basedOnTempTex != AssetDatabase.GetAssetPath(tex))
            {
                EditorGUILayout.Separator();
                if (!(target as ColorCorrectionLut).ValidDimensions(tex))
                {
                    EditorGUILayout.HelpBox(
                        "Invalid texture dimensions!\nPick another texture or adjust dimension to e.g. 256x16.",
                        MessageType.Warning);
                }
                else if (GUILayout.Button("Convert and Apply"))
                {
                    string path = AssetDatabase.GetAssetPath(tex);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    bool doImport = textureImporter.isReadable == false || textureImporter.mipmapEnabled == true ||
                                    textureImporter.textureFormat != TextureImporterFormat.AutomaticTruecolor;

                    if (doImport)
                    {
                        textureImporter.isReadable = true;
                        textureImporter.mipmapEnabled = false;
                        textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    }

                    (target as ColorCorrectionLut).Convert(tex, path);
                }
            }

            if ((target as ColorCorrectionLut).basedOnTempTex != "")
            {
                EditorGUILayout.HelpBox("Using " + (target as ColorCorrectionLut).basedOnTempTex, MessageType.Info);
                t = AssetDatabase.LoadMainAssetAtPath((target as ColorCorrectionLut).basedOnTempTex) as Texture2D;
                if (t)
                {
                    r = GUILayoutUtility.GetLastRect();
                    r = GUILayoutUtility.GetRect(r.width, 20);
                    r.x += r.width*0.05f/2.0f;
                    r.width *= 0.95f;
                    GUI.DrawTexture(r, t);
                    GUILayoutUtility.GetRect(r.width, 4);
                }
            }

            //EditorGUILayout.EndHorizontal ();    

            serObj.ApplyModifiedProperties();
        }
    }
}