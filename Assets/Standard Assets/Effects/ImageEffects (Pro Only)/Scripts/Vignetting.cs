using UnityEngine;

namespace UnitySampleAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof (Camera))]
    [AddComponentMenu("Image Effects/Vignette and Chromatic Aberration")]
    public class Vignetting /* And Chromatic Aberration */ : PostEffectsBase
    {

        public enum AberrationMode
        {
            Simple = 0,
            Advanced = 1,
        }

        public AberrationMode mode = AberrationMode.Simple;

        public float intensity = 0.375f; // intensity == 0 disables pre pass (optimization)
        public float chromaticAberration = 0.2f;
        public float axialAberration = 0.5f;

        public float blur = 0.0f; // blur == 0 disables blur pass (optimization)
        public float blurSpread = 0.75f;

        public float luminanceDependency = 0.25f;

        public Shader vignetteShader;
        private Material vignetteMaterial;

        public Shader separableBlurShader;
        private Material separableBlurMaterial;

        public Shader chromAberrationShader;
        private Material chromAberrationMaterial;

        protected override bool CheckResources()
        {
            CheckSupport(false);

            vignetteMaterial = CheckShaderAndCreateMaterial(vignetteShader, vignetteMaterial);
            separableBlurMaterial = CheckShaderAndCreateMaterial(separableBlurShader, separableBlurMaterial);
            chromAberrationMaterial = CheckShaderAndCreateMaterial(chromAberrationShader, chromAberrationMaterial);

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

            bool doPrepass = (Mathf.Abs(blur) > 0.0f || Mathf.Abs(intensity) > 0.0f);

            float widthOverHeight = (1.0f*source.width)/(1.0f*source.height);
            float oneOverBaseSize = 1.0f/512.0f;

            RenderTexture color = null;
            RenderTexture halfRezColor = null;
            RenderTexture secondHalfRezColor = null;

            if (doPrepass)
            {
                color = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

                if (Mathf.Abs(blur) > 0.0f)
                {
                    halfRezColor = RenderTexture.GetTemporary((int) (source.width/2.0f), (int) (source.height/2.0f), 0,
                                                              source.format);
                    secondHalfRezColor = RenderTexture.GetTemporary((int) (source.width/2.0f),
                                                                    (int) (source.height/2.0f), 0, source.format);

                    Graphics.Blit(source, halfRezColor, chromAberrationMaterial, 0);

                    for (int i = 0; i < 2; i++)
                    {
                        // maybe make iteration count tweakable
                        separableBlurMaterial.SetVector("offsets",
                                                        new Vector4(0.0f, blurSpread*oneOverBaseSize, 0.0f, 0.0f));
                        Graphics.Blit(halfRezColor, secondHalfRezColor, separableBlurMaterial);
                        separableBlurMaterial.SetVector("offsets",
                                                        new Vector4(blurSpread*oneOverBaseSize/widthOverHeight, 0.0f,
                                                                    0.0f, 0.0f));
                        Graphics.Blit(secondHalfRezColor, halfRezColor, separableBlurMaterial);
                    }
                }

                vignetteMaterial.SetFloat("_Intensity", intensity); // intensity for vignette
                vignetteMaterial.SetFloat("_Blur", blur); // blur intensity
                vignetteMaterial.SetTexture("_VignetteTex", halfRezColor); // blurred texture

                Graphics.Blit(source, color, vignetteMaterial, 0); // prepass blit: darken & blur corners
            }

            chromAberrationMaterial.SetFloat("_ChromaticAberration", chromaticAberration);
            chromAberrationMaterial.SetFloat("_AxialAberration", axialAberration);
            chromAberrationMaterial.SetFloat("_Luminance", 1.0f/(Mathf.Epsilon + luminanceDependency));

            if (doPrepass) color.wrapMode = TextureWrapMode.Clamp;
            else source.wrapMode = TextureWrapMode.Clamp;
            Graphics.Blit(doPrepass ? color : source, destination, chromAberrationMaterial,
                          mode == AberrationMode.Advanced ? 2 : 1);

            if (color) RenderTexture.ReleaseTemporary(color);
            if (halfRezColor) RenderTexture.ReleaseTemporary(halfRezColor);
            if (secondHalfRezColor) RenderTexture.ReleaseTemporary(secondHalfRezColor);
        }

    }
}