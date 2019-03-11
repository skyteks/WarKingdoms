using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Creates a mesh on ground plane with camera frustum collision points
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class ViewPort : MonoBehaviour
{

    [Range(0.1f, 100f)]
    public float viewPortBorderSize = 1.5f;
    private MeshFilter viewportMeshFilter;

    void Awake()
    {
        viewportMeshFilter = GetComponent<MeshFilter>();
    }

    void OnEnable()
    {
        MeshRenderer render = GetComponent<MeshRenderer>();
        if (render != null)
        {
            render.enabled = true;
        }
    }

    void OnDisable()
    {
        MeshRenderer render = GetComponent<MeshRenderer>();
        if (render != null)
        {
            render.enabled = false;
        }
    }

    void LateUpdate()
    {
        if (GameManager.Instance.gameMode == GameManager.GameMode.Gameplay)
        {
            CreateViewPortPlaneMesh();
        }
    }

    void CreateViewPortPlaneMesh()
    {
        Camera mainCamera = CameraManager.Instance.gameplayCamera;
        if (mainCamera == null)
        {
            return;
        }

        Vector2 resolution = new Vector2(Screen.width, Screen.height);
        Vector3 hitPoint;
        CameraManager.GetCameraScreenPointOnGroundPlane(mainCamera, resolution.ToScale(mainCamera.rect.position), out hitPoint);
        Vector3 bottomLeft = hitPoint;
        CameraManager.GetCameraScreenPointOnGroundPlane(mainCamera, resolution.ToScale(mainCamera.rect.position + mainCamera.rect.width * Vector2.right), out hitPoint);
        Vector3 bottomRight = hitPoint;
        CameraManager.GetCameraScreenPointOnGroundPlane(mainCamera, resolution.ToScale(mainCamera.rect.position + mainCamera.rect.height * Vector2.up), out hitPoint);
        Vector3 topLeft = hitPoint;
        CameraManager.GetCameraScreenPointOnGroundPlane(mainCamera, resolution.ToScale(mainCamera.rect.position + mainCamera.rect.size), out hitPoint);
        Vector3 topRight = hitPoint;

        //Color debugRayColor = Color.blue;
        //Debug.DrawLine(mainCamera.transform.position, bottomLeft, debugRayColor);
        //Debug.DrawLine(mainCamera.transform.position, bottomRight, debugRayColor);
        //Debug.DrawLine(mainCamera.transform.position, topLeft, debugRayColor);
        //Debug.DrawLine(mainCamera.transform.position, topRight, debugRayColor);

        Vector3 leftLine = (topLeft - bottomLeft).normalized / 2f;
        Vector3 rightLine = (topRight - bottomRight).normalized / 2f;
        Vector3 bottomLine = (bottomRight - bottomLeft).normalized / 2f;
        Vector3 topLine = (topRight - topLeft).normalized / 2f;

        Vector3 bottomLeftOffset = (bottomLine + leftLine) * -1f * viewPortBorderSize;
        Vector3 bottomRightOffset = (-bottomLine + rightLine) * -1f * viewPortBorderSize;
        Vector3 topLeftOffset = (topLine + -leftLine) * -1f * viewPortBorderSize;
        Vector3 topRightOffset = (-topLine + -rightLine) * -1f * viewPortBorderSize;

        bottomLeftOffset += bottomLeft;
        bottomRightOffset += bottomRight;
        topLeftOffset += topLeft;
        topRightOffset += topRight;

        List<Vector3> vertexArray = new Vector3[]{
            bottomLeftOffset,
            bottomRightOffset,
            topLeftOffset,
            topRightOffset,
            bottomLeft,
            bottomRight,
            topLeft,
            topRight
        }.ToList();
        List<Vector3> normalArray = new List<Vector3>(vertexArray.Count);
        for (int i = 0; i < vertexArray.Count; i++)
        {
            normalArray.Add(Vector3.up);
        }

        int[] indexArray = new int[] { 0, 4, 6, 2, 0, 1, 5, 4, 5, 1, 3, 7, 6, 7, 3, 2 }.Reverse().ToArray();
        for (int i = 0; i < vertexArray.Count; i++)
        {
            vertexArray[i] = mainCamera.transform.InverseTransformPoint(vertexArray[i]);
        }

        if (viewportMeshFilter.sharedMesh == null)
        {
            viewportMeshFilter.sharedMesh = new Mesh();
        }

        Mesh mesh = viewportMeshFilter.sharedMesh;
        mesh.name = "View Port Mesh (Generated)";
        mesh.Clear();
        mesh.SetVertices(vertexArray);
        mesh.SetNormals(normalArray);
        mesh.SetIndices(indexArray, MeshTopology.Quads, 0);
    }
}
