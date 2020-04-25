using UnityEngine;

namespace UnitySampleAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Color Correction (Ramp)")]
    public class ColorCorrectionEffect : ImageEffectBase
    {
        public Texture textureRamp;

        // Called by camera to apply image effect
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            material.SetTexture("_RampTex", textureRamp);
            Graphics.Blit(source, destination, material);
        }
    }
}