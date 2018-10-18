using UnityEngine;

namespace UnitySampleAssets.ImageEffects
{
   [ExecuteInEditMode]
    [RequireComponent(typeof (Camera))]
    [AddComponentMenu("Image Effects/Tilt shift")]
   public class TiltShift : PostEffectsBase
    {
        public Shader tiltShiftShader;
        private Material tiltShiftMaterial = null;

        public int renderTextureDivider = 2;
        public int blurIterations = 2;
        public bool enableForegroundBlur = true;
        public int foregroundBlurIterations = 2;
        public float maxBlurSpread = 1.5f;

        public float focalPoint = 30.0f;
        public float smoothness = 1.65f;

        public bool visualizeCoc = false;

        // these values will be automatically determined

        private float start01 = 0.0f;
        private float distance01 = 0.2f;
        private float end01 = 1.0f;
        private float curve = 1.0f;

        protected override bool CheckResources()
        {
            CheckSupport(true);

            tiltShiftMaterial = CheckShaderAndCreateMaterial(tiltShiftShader, tiltShiftMaterial);

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

            float widthOverHeight = (1.0f*source.width)/(1.0f*source.height);
            float oneOverBaseSize = 1.0f/512.0f;

            // clamp some values

            renderTextureDivider = renderTextureDivider < 1 ? 1 : renderTextureDivider;
            renderTextureDivider = renderTextureDivider > 4 ? 4 : renderTextureDivider;
            blurIterations = blurIterations < 1 ? 0 : blurIterations;
            blurIterations = blurIterations > 4 ? 4 : blurIterations;

            // automagically calculate parameters based on focalPoint

            float focalPoint01 =
                GetComponent<Camera>().WorldToViewportPoint(focalPoint*GetComponent<Camera>().transform.forward + GetComponent<Camera>().transform.position).z/
                (GetComponent<Camera>().farClipPlane);

            distance01 = focalPoint01;
            start01 = 0.0f;
            end01 = 1.0f;
            start01 = Mathf.Min(focalPoint01 - Mathf.Epsilon, start01);
            end01 = Mathf.Max(focalPoint01 + Mathf.Epsilon, end01);
            curve = smoothness*distance01;

            // resources

            RenderTexture cocTex = RenderTexture.GetTemporary(source.width, source.height, 0);
            RenderTexture cocTex2 = RenderTexture.GetTemporary(source.width, source.height, 0);
            RenderTexture lrTex1 = RenderTexture.GetTemporary(source.width/renderTextureDivider,
                                                              source.height/renderTextureDivider, 0);
            RenderTexture lrTex2 = RenderTexture.GetTemporary(source.width/renderTextureDivider,
                                                              source.height/renderTextureDivider, 0);

            // coc		

            tiltShiftMaterial.SetVector("_SimpleDofParams", new Vector4(start01, distance01, end01, curve));
            tiltShiftMaterial.SetTexture("_Coc", cocTex);

            if (enableForegroundBlur)
            {
                Graphics.Blit(source, cocTex, tiltShiftMaterial, 0);
                Graphics.Blit(cocTex, lrTex1); // downwards (only really needed if lrTex resolution is different)

                for (int fgBlurIter = 0; fgBlurIter < foregroundBlurIterations; fgBlurIter++)
                {
                    tiltShiftMaterial.SetVector("offsets",
                                                new Vector4(0.0f, (maxBlurSpread*0.75f)*oneOverBaseSize, 0.0f, 0.0f));
                    Graphics.Blit(lrTex1, lrTex2, tiltShiftMaterial, 3);
                    tiltShiftMaterial.SetVector("offsets",
                                                new Vector4((maxBlurSpread*0.75f/widthOverHeight)*oneOverBaseSize, 0.0f,
                                                            0.0f, 0.0f));
                    Graphics.Blit(lrTex2, lrTex1, tiltShiftMaterial, 3);
                }

                Graphics.Blit(lrTex1, cocTex2, tiltShiftMaterial, 7);
                    // upwards (only really needed if lrTex resolution is different)
                tiltShiftMaterial.SetTexture("_Coc", cocTex2);
            }
            else
            {
                RenderTexture.active = cocTex;
                GL.Clear(false, true, Color.black);
            }

            // combine coc's
            Graphics.Blit(source, cocTex, tiltShiftMaterial, 5);
            tiltShiftMaterial.SetTexture("_Coc", cocTex);

            // downsample & blur

            Graphics.Blit(source, lrTex2);

            for (int iter = 0; iter < blurIterations; iter++)
            {
                tiltShiftMaterial.SetVector("offsets",
                                            new Vector4(0.0f, (maxBlurSpread*1.0f)*oneOverBaseSize, 0.0f, 0.0f));
                Graphics.Blit(lrTex2, lrTex1, tiltShiftMaterial, 6);
                tiltShiftMaterial.SetVector("offsets",
                                            new Vector4((maxBlurSpread*1.0f/widthOverHeight)*oneOverBaseSize, 0.0f, 0.0f,
                                                        0.0f));
                Graphics.Blit(lrTex1, lrTex2, tiltShiftMaterial, 6);
            }

            tiltShiftMaterial.SetTexture("_Blurred", lrTex2);

            Graphics.Blit(source, destination, tiltShiftMaterial, visualizeCoc ? 4 : 1);

            RenderTexture.ReleaseTemporary(cocTex);
            RenderTexture.ReleaseTemporary(cocTex2);
            RenderTexture.ReleaseTemporary(lrTex1);
            RenderTexture.ReleaseTemporary(lrTex2);
        }
    }
}