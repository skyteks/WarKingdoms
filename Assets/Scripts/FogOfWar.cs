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
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
}
