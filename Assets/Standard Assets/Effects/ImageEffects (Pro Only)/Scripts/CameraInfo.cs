using UnityEngine;

namespace UnitySampleAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof (Camera))]
    [AddComponentMenu("Image Effects/Camera Info")]
    public class CameraInfo : MonoBehaviour
    {

        // display current depth texture mode
        public DepthTextureMode currentDepthMode;
        // render path
        public RenderingPath currentRenderPath;
        // number of official image fx used
        public int recognizedPostFxCount = 0;

#if UNITY_EDITOR
        private void Start()
        {
            UpdateInfo();
        }

        private void Update()
        {
            if (currentDepthMode != GetComponent<Camera>().depthTextureMode)
                GetComponent<Camera>().depthTextureMode = currentDepthMode;
            if (currentRenderPath != GetComponent<Camera>().actualRenderingPath)
                GetComponent<Camera>().renderingPath = currentRenderPath;

            UpdateInfo();
        }

        private void UpdateInfo()
        {
            currentDepthMode = GetComponent<Camera>().depthTextureMode;
            currentRenderPath = GetComponent<Camera>().actualRenderingPath;
            PostEffectsBase[] fx = gameObject.GetComponents<PostEffectsBase>();
            int fxCount = 0;
            foreach (var post in fx)
                if (post.enabled)
                    fxCount++;
            recognizedPostFxCount = fxCount;
        }
#endif
    }
}