using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class FogOfWarRenderFeature : ScriptableRendererFeature
{
    private FogOfWarRenderPass renderPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }

    public override void Create()
    {
        renderPass = new FogOfWarRenderPass();
        renderPass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }
}
