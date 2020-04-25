
using UnityEngine;
using UnitySampleAssets.ImageEffects;

namespace UnitySampleAssets
{
    public enum EdgeDetectMode
    {
        TriangleDepthNormals = 0,
        RobertsCrossDepthNormals = 1,
        SobelDepth = 2,
        SobelDepthThin = 3,
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof (Camera))]
    [AddComponentMenu("Image Effects/Edge Detection (Geometry)")]
    public class EdgeDetectEffectNormals : PostEffectsBase
    {

        public EdgeDetectMode mode = EdgeDetectMode.SobelDepthThin;
        public float sensitivityDepth = 1.0f;
        public float sensitivityNormals = 1.0f;
        public float edgeExp = 1.0f;
        public float sampleDist = 1.0f;
        public float edgesOnly = 0.0f;
        public Color edgesOnlyBgColor = Color.white;

        public Shader edgeDetectShader;
        private Material edgeDetectMaterial = null;
        private EdgeDetectMode oldMode = EdgeDetectMode.SobelDepthThin;

        protected override bool CheckResources()
        {
            CheckSupport(true);

            edgeDetectMaterial = CheckShaderAndCreateMaterial(edgeDetectShader, edgeDetectMaterial);
            if (mode != oldMode)
                SetCameraFlag();

            oldMode = mode;

            if (!isSupported)
                ReportAutoDisable();
            return isSupported;
        }

        private new void Start()
        {
            oldMode = mode;
        }

        private void SetCameraFlag()
        {
            if (mode > (EdgeDetectMode) 1)
                GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
            else
                GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
        }

        private void OnEnable()
        {
            SetCameraFlag();
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }

            Vector2 sensitivity = new Vector2(sensitivityDepth, sensitivityNormals);
            edgeDetectMaterial.SetVector("_Sensitivity", new Vector4(sensitivity.x, sensitivity.y, 1.0f, sensitivity.y));
            edgeDetectMaterial.SetFloat("_BgFade", edgesOnly);
            edgeDetectMaterial.SetFloat("_SampleDistance", sampleDist);
            edgeDetectMaterial.SetVector("_BgColor", edgesOnlyBgColor);
            edgeDetectMaterial.SetFloat("_Exponent", edgeExp);

            Graphics.Blit(source, destination, edgeDetectMaterial, (int) mode);
        }
    }

}