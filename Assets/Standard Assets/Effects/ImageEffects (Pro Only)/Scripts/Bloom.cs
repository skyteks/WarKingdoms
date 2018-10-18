using UnityEngine;

namespace UnitySampleAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof (Camera))]
    [AddComponentMenu("Image Effects/Bloom (4.0, HDR, Lens Flares)")]
    public class Bloom : PostEffectsBase
    {
        public enum LensFlareStyle
        {
            Ghosting = 0,
            Anamorphic = 1,
            Combined = 2,
        }

        public enum TweakMode
        {
            Basic = 0,
            Complex = 1,
        }

        public enum HDRBloomMode
        {
            Auto = 0,
            On = 1,
            Off = 2,
        }

        public enum BloomScreenBlendMode
        {
            Screen = 0,
            Add = 1,
        }

        public enum BloomQuality
        {
            Cheap = 0,
            High = 1,
        }

        public TweakMode tweakMode = 0;
        public BloomScreenBlendMode screenBlendMode = BloomScreenBlendMode.Add;

        public HDRBloomMode hdr = HDRBloomMode.Auto;
        private bool doHdr = false;
        public float sepBlurSpread = 2.5f;

        public BloomQuality quality = BloomQuality.High;

        public float bloomIntensity = 0.5f;
        public float bloomThreshhold = 0.5f;
        public Color bloomThreshholdColor = Color.white;
        public int bloomBlurIterations = 2;

        public int hollywoodFlareBlurIterations = 2;
        public float flareRotation = 0.0f;
        public LensFlareStyle lensflareMode = LensFlareStyle.Anamorphic;
        public float hollyStretchWidth = 2.5f;
        public float lensflareIntensity = 0.0f;
        public float lensflareThreshhold = 0.3f;
        public float lensFlareSaturation = 0.75f;
        public Color flareColorA = new Color(0.4f, 0.4f, 0.8f, 0.75f);
        public Color flareColorB = new Color(0.4f, 0.8f, 0.8f, 0.75f);
        public Color flareColorC = new Color(0.8f, 0.4f, 0.8f, 0.75f);
        public Color flareColorD = new Color(0.8f, 0.4f, 0.0f, 0.75f);
        public float blurWidth = 1.0f;
        public Texture2D lensFlareVignetteMask;

        public Shader lensFlareShader;
        private Material lensFlareMaterial;

        public Shader screenBlendShader;
        private Material screenBlend;

        public Shader blurAndFlaresShader;
        private Material blurAndFlaresMaterial;

        public Shader brightPassFilterShader;
        private Material brightPassFilterMaterial;

        protected override bool CheckResources()
        {
            CheckSupport(false);

            screenBlend = CheckShaderAndCreateMaterial(screenBlendShader, screenBlend);
            lensFlareMaterial = CheckShaderAndCreateMaterial(lensFlareShader, lensFlareMaterial);
            blurAndFlaresMaterial = CheckShaderAndCreateMaterial(blurAndFlaresShader, blurAndFlaresMaterial);
            brightPassFilterMaterial = CheckShaderAndCreateMaterial(brightPassFilterShader, brightPassFilterMaterial);

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

            // screen blend is not supported when HDR is enabled (will cap values)

            doHdr = false;
            if (hdr == HDRBloomMode.Auto)
                doHdr = source.format == RenderTextureFormat.ARGBHalf && GetComponent<Camera>().hdr;
            else
            {
                doHdr = hdr == HDRBloomMode.On;
            }

            doHdr = doHdr && supportHDRTextures;

            BloomScreenBlendMode realBlendMode = screenBlendMode;
            if (doHdr)
                realBlendMode = BloomScreenBlendMode.Add;

            var rtFormat = (doHdr) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.Default;
            RenderTexture halfRezColor = RenderTexture.GetTemporary(source.width/2, source.height/2, 0, rtFormat);
            RenderTexture quarterRezColor = RenderTexture.GetTemporary(source.width/4, source.height/4, 0, rtFormat);
            RenderTexture secondQuarterRezColor = RenderTexture.GetTemporary(source.width/4, source.height/4, 0,
                                                                             rtFormat);
            RenderTexture thirdQuarterRezColor = RenderTexture.GetTemporary(source.width/4, source.height/4, 0, rtFormat);

            float widthOverHeight = (1.0f*source.width)/(1.0f*source.height);
            float oneOverBaseSize = 1.0f/512.0f;

            // downsample

            if (quality > BloomQuality.Cheap)
            {
                Graphics.Blit(source, halfRezColor, screenBlend, 2);
                Graphics.Blit(halfRezColor, secondQuarterRezColor, screenBlend, 2);
                Graphics.Blit(secondQuarterRezColor, quarterRezColor, screenBlend, 6);
            }
            else
            {
                Graphics.Blit(source, halfRezColor);
                Graphics.Blit(halfRezColor, quarterRezColor, screenBlend, 6);
            }

            // cut colors (threshholding)			

            BrightFilter(bloomThreshhold*bloomThreshholdColor, quarterRezColor, secondQuarterRezColor);

            // blurring

            if (bloomBlurIterations < 1) bloomBlurIterations = 1;
            else if (bloomBlurIterations > 10) bloomBlurIterations = 10;

            for (int iter = 0; iter < bloomBlurIterations; iter++)
            {
                float spreadForPass = (1.0f + (iter*0.25f))*sepBlurSpread;

                blurAndFlaresMaterial.SetVector("_Offsets", new Vector4(0.0f, spreadForPass*oneOverBaseSize, 0.0f, 0.0f));
                Graphics.Blit(secondQuarterRezColor, thirdQuarterRezColor, blurAndFlaresMaterial, 4);

                if (quality > BloomQuality.Cheap)
                {
                    blurAndFlaresMaterial.SetVector("_Offsets",
                                                    new Vector4((spreadForPass/widthOverHeight)*oneOverBaseSize, 0.0f,
                                                                0.0f, 0.0f));
                    Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, blurAndFlaresMaterial, 4);

                    if (iter == 0)
                        Graphics.Blit(secondQuarterRezColor, quarterRezColor);
                    else
                        Graphics.Blit(secondQuarterRezColor, quarterRezColor, screenBlend, 10);
                }
                else
                {
                    blurAndFlaresMaterial.SetVector("_Offsets",
                                                    new Vector4((spreadForPass/widthOverHeight)*oneOverBaseSize, 0.0f,
                                                                0.0f, 0.0f));
                    Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, blurAndFlaresMaterial, 4);
                }
            }

            if (quality > BloomQuality.Cheap)
                Graphics.Blit(quarterRezColor, secondQuarterRezColor, screenBlend, 6);

            // lens flares: ghosting, anamorphic or both (ghosted anamorphic flares) 

            if (lensflareIntensity > Mathf.Epsilon)
            {

                if (lensflareMode == 0)
                {

                    BrightFilter(lensflareThreshhold, secondQuarterRezColor, thirdQuarterRezColor);

                    if (quality > BloomQuality.Cheap)
                    {
                        // smooth a little
                        blurAndFlaresMaterial.SetVector("_Offsets",
                                                        new Vector4(0.0f, (1.5f)/(1.0f*quarterRezColor.height), 0.0f,
                                                                    0.0f));
                        Graphics.Blit(thirdQuarterRezColor, quarterRezColor, blurAndFlaresMaterial, 4);
                        blurAndFlaresMaterial.SetVector("_Offsets",
                                                        new Vector4((1.5f)/(1.0f*quarterRezColor.width), 0.0f, 0.0f,
                                                                    0.0f));
                        Graphics.Blit(quarterRezColor, thirdQuarterRezColor, blurAndFlaresMaterial, 4);
                    }

                    // no ugly edges!
                    Vignette(0.975f, thirdQuarterRezColor, thirdQuarterRezColor);
                    BlendFlares(thirdQuarterRezColor, secondQuarterRezColor);
                }
                else
                {

                    //Vignette (0.975f, thirdQuarterRezColor, thirdQuarterRezColor);	
                    //DrawBorder(thirdQuarterRezColor, screenBlend, 8);

                    float flareXRot = 1.0f*Mathf.Cos(flareRotation);
                    float flareyRot = 1.0f*Mathf.Sin(flareRotation);

                    float stretchWidth = (hollyStretchWidth*1.0f/widthOverHeight)*oneOverBaseSize;

                    blurAndFlaresMaterial.SetVector("_Offsets", new Vector4(flareXRot, flareyRot, 0.0f, 0.0f));
                    blurAndFlaresMaterial.SetVector("_Threshhold", new Vector4(lensflareThreshhold, 1.0f, 0.0f, 0.0f));
                    blurAndFlaresMaterial.SetVector("_TintColor",
                                                    new Vector4(flareColorA.r, flareColorA.g, flareColorA.b,
                                                                flareColorA.a)*flareColorA.a*lensflareIntensity);
                    blurAndFlaresMaterial.SetFloat("_Saturation", lensFlareSaturation);

                    Graphics.Blit(thirdQuarterRezColor, quarterRezColor, blurAndFlaresMaterial, 2);
                    Graphics.Blit(quarterRezColor, thirdQuarterRezColor, blurAndFlaresMaterial, 3);

                    blurAndFlaresMaterial.SetVector("_Offsets",
                                                    new Vector4(flareXRot*stretchWidth, flareyRot*stretchWidth, 0.0f,
                                                                0.0f));
                    blurAndFlaresMaterial.SetFloat("_StretchWidth", hollyStretchWidth);

                    Graphics.Blit(thirdQuarterRezColor, quarterRezColor, blurAndFlaresMaterial, 1);
                    blurAndFlaresMaterial.SetFloat("_StretchWidth", hollyStretchWidth*2.0f);
                    Graphics.Blit(quarterRezColor, thirdQuarterRezColor, blurAndFlaresMaterial, 1);
                    blurAndFlaresMaterial.SetFloat("_StretchWidth", hollyStretchWidth*4.0f);
                    Graphics.Blit(thirdQuarterRezColor, quarterRezColor, blurAndFlaresMaterial, 1);

                    for (int iter = 0; iter < hollywoodFlareBlurIterations; iter++)
                    {
                        stretchWidth = (hollyStretchWidth*2.0f/widthOverHeight)*oneOverBaseSize;
                        blurAndFlaresMaterial.SetVector("_Offsets",
                                                        new Vector4(stretchWidth*flareXRot, stretchWidth*flareyRot, 0.0f,
                                                                    0.0f));
                        Graphics.Blit(quarterRezColor, thirdQuarterRezColor, blurAndFlaresMaterial, 4);
                        blurAndFlaresMaterial.SetVector("_Offsets",
                                                        new Vector4(stretchWidth*flareXRot, stretchWidth*flareyRot, 0.0f,
                                                                    0.0f));
                        Graphics.Blit(thirdQuarterRezColor, quarterRezColor, blurAndFlaresMaterial, 4);
                    }

                    if (lensflareMode == (LensFlareStyle) 1)
                        AddTo(1.0f, quarterRezColor, secondQuarterRezColor);
                    else
                    {

                        // "combined" lens flares													

                        Vignette(1.0f, quarterRezColor, thirdQuarterRezColor);
                        BlendFlares(thirdQuarterRezColor, quarterRezColor);
                        AddTo(1.0f, quarterRezColor, secondQuarterRezColor);
                    }
                }
            }

            int blendPass = (int) realBlendMode;
            //if(Mathf.Abs(chromaticBloom) < Mathf.Epsilon) 
            //	blendPass += 4;

            screenBlend.SetFloat("_Intensity", bloomIntensity);
            screenBlend.SetTexture("_ColorBuffer", source);

            if (quality > BloomQuality.Cheap)
            {
                Graphics.Blit(secondQuarterRezColor, halfRezColor);
                Graphics.Blit(halfRezColor, destination, screenBlend, blendPass);
            }
            else
                Graphics.Blit(secondQuarterRezColor, destination, screenBlend, blendPass);

            RenderTexture.ReleaseTemporary(halfRezColor);
            RenderTexture.ReleaseTemporary(quarterRezColor);
            RenderTexture.ReleaseTemporary(secondQuarterRezColor);
            RenderTexture.ReleaseTemporary(thirdQuarterRezColor);
        }

        private void AddTo(float intensity_, RenderTexture from, RenderTexture to)
        {
            screenBlend.SetFloat("_Intensity", intensity_);
            Graphics.Blit(from, to, screenBlend, 9);
        }

        private void BlendFlares(RenderTexture from, RenderTexture to)
        {
            lensFlareMaterial.SetVector("colorA",
                                        new Vector4(flareColorA.r, flareColorA.g, flareColorA.b, flareColorA.a)*
                                        lensflareIntensity);
            lensFlareMaterial.SetVector("colorB",
                                        new Vector4(flareColorB.r, flareColorB.g, flareColorB.b, flareColorB.a)*
                                        lensflareIntensity);
            lensFlareMaterial.SetVector("colorC",
                                        new Vector4(flareColorC.r, flareColorC.g, flareColorC.b, flareColorC.a)*
                                        lensflareIntensity);
            lensFlareMaterial.SetVector("colorD",
                                        new Vector4(flareColorD.r, flareColorD.g, flareColorD.b, flareColorD.a)*
                                        lensflareIntensity);
            Graphics.Blit(from, to, lensFlareMaterial);
        }

        private void BrightFilter(float thresh, RenderTexture from, RenderTexture to)
        {
            brightPassFilterMaterial.SetVector("_Threshhold", new Vector4(thresh, thresh, thresh, thresh));
            Graphics.Blit(from, to, brightPassFilterMaterial, 0);
        }

        private void BrightFilter(Color threshColor, RenderTexture from, RenderTexture to)
        {
            brightPassFilterMaterial.SetVector("_Threshhold", threshColor);
            Graphics.Blit(from, to, brightPassFilterMaterial, 1);
        }

        private void Vignette(float amount, RenderTexture from, RenderTexture to)
        {
            if (lensFlareVignetteMask)
            {
                screenBlend.SetTexture("_ColorBuffer", lensFlareVignetteMask);
                Graphics.Blit(from == to ? null : from, to, screenBlend, from == to ? 7 : 3);
            }
            else if (from != to)
                Graphics.Blit(from, to);
        }

    }
}