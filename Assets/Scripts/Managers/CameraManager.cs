using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera;
    public Camera miniMapCamera;
    public MeshFilter viewPort;

    [Range(0.1f, 100f)]
    public float viewPortBorderSize = 1.5f;

    private bool isFramingPlatoon = false;
    public bool IsFramingPlatoon { get { return isFramingPlatoon; } } //from the outside it's read-only

    void Awake()
    {
        if (mainCamera == null) mainCamera = GetComponent<Camera>();
        if (mainCamera == null) mainCamera = Camera.main;//GameObject.FindObjectOfType<Camera>();
    }

    void LateUpdate()
    {
        if (GameManager.Instance.gameMode == GameManager.GameMode.Gameplay)
        {
            CreateViewPortPlaneMesh();
        }
    }

    public void MoveGameplayCamera(Vector2 amount)
    {
        mainCamera.transform.Translate(amount.x, 0f, amount.y, Space.World);
    }

    public void MoveGameplayCameraTo(Vector3 point)
    {
        Vector3 middlePoint;
        GetCameraViewPointOnGroundPlane(mainCamera, new Vector3(0.5f, 0.5f, 0f), out middlePoint);
        point -= middlePoint - mainCamera.transform.position.ToWithY(middlePoint.y);
        mainCamera.transform.position = point.ToWithY(mainCamera.transform.position.y);
    }

    private void CreateViewPortPlaneMesh()
    {
        Vector2 resolution = new Vector2(Screen.width, Screen.height);
        Vector3 hitPoint;
        GetCameraScreenPointOnGroundPlane(mainCamera, resolution.ToScale(mainCamera.rect.position), out hitPoint);
        Vector3 bottomLeft = hitPoint;
        GetCameraScreenPointOnGroundPlane(mainCamera, resolution.ToScale(mainCamera.rect.position + mainCamera.rect.width * Vector2.right), out hitPoint);
        Vector3 bottomRight = hitPoint;
        GetCameraScreenPointOnGroundPlane(mainCamera, resolution.ToScale(mainCamera.rect.position + mainCamera.rect.height * Vector2.up), out hitPoint);
        Vector3 topLeft = hitPoint;
        GetCameraScreenPointOnGroundPlane(mainCamera, resolution.ToScale(mainCamera.rect.position + mainCamera.rect.size), out hitPoint);
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

        Vector3 bottomLeftOffset = (bottomLine + leftLine) * viewPortBorderSize;
        Vector3 bottomRightOffset = (-bottomLine + rightLine) * viewPortBorderSize;
        Vector3 topLeftOffset = (topLine + -leftLine) * viewPortBorderSize;
        Vector3 topRightOffset = (-topLine + -rightLine) * viewPortBorderSize;

        bottomLeftOffset += bottomLeft;
        bottomRightOffset += bottomRight;
        topLeftOffset += topLeft;
        topRightOffset += topRight;

        List<Vector3> vertexArray = new Vector3[]{
            bottomLeft,
            bottomRight,
            topLeft,
            topRight,
            bottomLeftOffset,
            bottomRightOffset,
            topLeftOffset,
            topRightOffset
        }.ToList();
        List<Vector3> normalArray = Vector3.up.ToListOfMultiple(vertexArray.Count);
        int[] indexArray = new int[] { 0, 4, 6, 2, 0, 1, 5, 4, 5, 1, 3, 7, 6, 7, 3, 2 }.Reverse().ToArray();
        for (int i = 0; i < vertexArray.Count; i++)
        {
            vertexArray[i] = mainCamera.transform.InverseTransformPoint(vertexArray[i]);
        }

        if (viewPort.sharedMesh == null) viewPort.sharedMesh = new Mesh();
        Mesh mesh = viewPort.sharedMesh;
        mesh.Clear();
        mesh.SetVertices(vertexArray);
        mesh.SetNormals(normalArray);
        mesh.SetIndices(indexArray, MeshTopology.Quads, 0);
    }

    public static bool GetCameraScreenPointOnGroundPlane(Camera camera, Vector3 screenPoint, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        Ray ray = camera.ScreenPointToRay(screenPoint);
        float rayDistance;
        if (GameManager.groundPlane.Raycast(ray, out rayDistance))
        {
            hitPoint = ray.GetPoint(rayDistance);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool GetCameraViewPointOnGroundPlane(Camera camera, Vector3 viewportPoint, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        Ray ray = camera.ViewportPointToRay(viewportPoint);
        float rayDistance;
        if (GameManager.groundPlane.Raycast(ray, out rayDistance))
        {
            hitPoint = ray.GetPoint(rayDistance);
            return true;
        }
        else
        {
            return false;
        }
    }
}