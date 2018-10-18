using UnityEngine;

namespace UnitySampleAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof (Camera))]
    [AddComponentMenu("Image Effects/Contrast Enhance (Unsharp Mask)")]
    internal class ContrastEnhance : PostEffectsBase
    {
        public float intensity = 0.5f;
        public float threshhold = 0.0f;

        private Material separableBlurMaterial;
        private Material contrastCompositeMaterial;

        public float blurSpread = 1.0f;

        public Shader separableBlurShader = null;
        public Shader contrastCompositeShader = null;

        protected override bool CheckResources()
        {
            CheckSupport(false);

            contrastCompositeMaterial = CheckShaderAndCreateMaterial(contrastCompositeShader, contrastCompositeMaterial);
            separableBlurMaterial = CheckShaderAndCreateMaterial(separableBlurShader, separableBlurMaterial);

            if (!isSupported)
                ReportAutoDisable();
            return isSupported;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }

            RenderTexture halfRezColor = RenderTexture.GetTemporary((int) (source.width/2.0f),
                                                                    (int) (source.height/2.0f), 0);
            RenderTexture quarterRezColor = RenderTexture.GetTemporary((int) (source.width/4.0f),
                                                                       (int) (source.height/4.0f), 0);
            RenderTexture secondQuarterRezColor = RenderTexture.GetTemporary((int) (source.width/4.0f),
                                                                             (int) (source.height/4.0f), 0);

            // ddownsample

            Graphics.Blit(source, halfRezColor);
            Graphics.Blit(halfRezColor, quarterRezColor);

            // blur

            separableBlurMaterial.SetVector("offsets",
                                            new Vector4(0.0f, (float) ((blurSpread*1.0)/quarterRezColor.height), 0.0f,
                                                        0.0f));
            Graphics.Blit(quarterRezColor, secondQuarterRezColor, separableBlurMaterial);
            separableBlurMaterial.SetVector("offsets",
                                            new Vector4((float) ((blurSpread*1.0)/quarterRezColor.width), 0.0f, 0.0f,
                                                        0.0f));
            Graphics.Blit(secondQuarterRezColor, quarterRezColor, separableBlurMaterial);

            // composite

            contrastCompositeMaterial.SetTexture("_MainTexBlurred", quarterRezColor);
            contrastCompositeMaterial.SetFloat("intensity", intensity);
            contrastCompositeMaterial.SetFloat("threshhold", threshhold);
            Graphics.Blit(source, destination, contrastCompositeMaterial);

            RenderTexture.ReleaseTemporary(halfRezColor);
            RenderTexture.ReleaseTemporary(quarterRezColor);
            RenderTexture.ReleaseTemporary(secondQuarterRezColor);
        }
    }
}