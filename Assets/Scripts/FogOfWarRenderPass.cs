using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FogOfWarRenderPass : ScriptableRenderPass
{
    private CommandBuffer commandBuffer;
    private FogOfWar component;
    private Matrix4x4[] transforms = new Matrix4x4[1024];
    private Mesh mesh;
    private Material material;
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        mesh = !mesh ? GetMesh() : mesh;
        material = !material ? GetMaterial() : material;

        commandBuffer = CommandBufferPool.Get("FogOfWar Command Buffer");
        commandBuffer.Clear();


        if (component == null)
        {
            component = GameObject.FindObjectOfType<FogOfWar>();
        }

        if (component)
        {
            commandBuffer.SetViewProjectionMatrices(component.transform.worldToLocalMatrix, Matrix4x4.identity);

            int instanceCount = 0;
            for (int i = 0; i < component.registers.Length; i++)
            {
                int count = component.registers[i].Count;
                for (int j = 0; j < count; j++)
                {
                    ClickableObject unit = component.registers[i].GetByIndex(j) as ClickableObject;
                    Vector3 position = unit.transform.position;
                    float viewDistance = unit.template.guardDistance;
                    transforms[instanceCount] = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * viewDistance);
                    instanceCount++;
                }
            }

            commandBuffer.SetRenderTarget(component.renderTexture);

            commandBuffer.ClearRenderTarget(true, true, new Color(1, 0, 0, 0));

            commandBuffer.DrawMeshInstanced(mesh, 0, material, 0, transforms, instanceCount);

            Graphics.ExecuteCommandBuffer(commandBuffer);
        }
        CommandBufferPool.Release(commandBuffer);
    }

    private Mesh GetMesh()
    {
        mesh = new Mesh();

        int[] triangles =
        {
            0, 1, 2,
            0, 2, 3
        };

        Vector3[] positions =
        {
            new Vector3(-1, 0, -1),
            new Vector3(1, 0, -1),
            new Vector3(1, 0,1),
            new Vector3(-1, 0,1)
        };

        mesh.vertices = positions;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }

    private Material GetMaterial()
    {
        Shader shader = Shader.Find("WarKingdoms/FieldOfView");
        material = new Material(shader);
        material.enableInstancing = true;
        return material;
    }
}
