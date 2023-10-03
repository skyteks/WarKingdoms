using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FogOfWar : MonoBehaviour
{
    public RenderTexture renderTexture;
    public RegisterObject[] registers;
    [SerializeField]
    private Texture2D texture;


    void Update()
    {
        Shader.SetGlobalTexture("_FowTexture", renderTexture);
        Shader.SetGlobalMatrix("_WorldToFow", transform.worldToLocalMatrix);
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.gray.ToWithA(0.2f);
        Bounds box = new Bounds(Vector3.zero, Vector3.one);
        Gizmos.DrawWireCube(box.max, Vector3.one);
    }
}
