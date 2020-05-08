using UnityEngine;

/// <summary>
/// Handles Camera movement
/// </summary>
public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera;
    public Camera miniMapCamera;
    public Rect panLimitBorder = new Rect(-100f, -100f, 200f, 200f);
    public Range scrollLimitBorder = new Range(5f, 100f);

    public static Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
    private static Vector3 screenMiddlePos = new Vector3(0.5f, 0.5f, 0f);

    public bool isFramingPlatoon { get; private set; } //from the outside it's read-only

    private Vector3 newPosition;
    private Quaternion newRotation;
    private float newZoom;

    public Camera gameplayCamera
    {
        get
        {
            return mainCamera;
        }
    }

    void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = GetComponent<Camera>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;//GameObject.FindObjectOfType<Camera>();
        }
    }

    void Start()
    {
        newPosition = mainCamera.transform.parent.position;
        newRotation = mainCamera.transform.parent.rotation;
        newZoom = scrollLimitBorder.Clamp(mainCamera.transform.localPosition.magnitude);
    }

    void Update()
    {
        float movementTime = InputManager.Instance.cameraMovementTime;

        Vector3 position = Vector3.Lerp(mainCamera.transform.parent.position, newPosition, Time.deltaTime * movementTime);
        position.x = Mathf.Clamp(position.x, panLimitBorder.xMin, panLimitBorder.xMax);
        position.z = Mathf.Clamp(position.z, panLimitBorder.yMin, panLimitBorder.yMax);
        mainCamera.transform.parent.position = position;

        mainCamera.transform.parent.rotation = Quaternion.Lerp(mainCamera.transform.parent.rotation, newRotation, Time.deltaTime * movementTime);

        mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, mainCamera.transform.localPosition.normalized * newZoom, Time.deltaTime * movementTime);
    }

    void OnDrawGizmos()
    {
        /*
        Vector3 middlePoint;
        if (GetCameraViewPointOnGroundPlane(mainCamera, screenMiddlePos, out middlePoint, InputManager.Instance.groundLayerMask))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(middlePoint, 0.5f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(mainCamera.transform.parent.position, 0.5f);
        */
    }

    public void MoveGameplayCamera(Vector3 positionChange)
    {
        newPosition += mainCamera.transform.parent.rotation * positionChange;
    }

    public void RotateGameplayCamera(Quaternion rotationChange)
    {
        newRotation *= rotationChange;
    }

    public void ZoomGameplayCamera(float zoomAmount)
    {
        newZoom = scrollLimitBorder.Clamp(newZoom + zoomAmount);
    }

    public void MoveGameplayCameraTo(Vector3 point)
    {
        Vector3 middlePoint;
        if (GetCameraViewPointOnGroundPlane(mainCamera, screenMiddlePos, out middlePoint, InputManager.Instance.groundLayerMask))
        {
            point -= middlePoint - mainCamera.transform.parent.position;
            newPosition = point.ToWithY(mainCamera.transform.parent.position.y);
            mainCamera.transform.parent.position = newPosition;
        }
    }

    public static bool GetCameraScreenPointOnGroundPlane(Camera camera, Vector3 screenPoint, out Vector3 hitPoint)
    {
        hitPoint = Vector3.one * float.NaN;
        Ray ray = camera.ScreenPointToRay(screenPoint);
        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            hitPoint = ray.GetPoint(rayDistance);
            return true;
        }
        return false;
    }

    public static bool GetCameraScreenPointOnGround(Camera camera, Vector3 screenPoint, out Vector3 hitPoint, LayerMask groundMask)
    {
        hitPoint = Vector3.one * float.NaN;
        Ray ray = camera.ScreenPointToRay(screenPoint);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 100f, groundMask))
        {
            hitPoint = hitInfo.point;
            return true;
        }
        return false;
    }

    public static bool GetCameraViewPointOnGroundPlane(Camera camera, Vector3 viewportPoint, out Vector3 hitPoint, LayerMask groundMask)
    {
        hitPoint = Vector3.one * float.NaN;
        Ray ray = camera.ViewportPointToRay(viewportPoint);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 100f, groundMask))
        {
            hitPoint = hitInfo.point;
            return true;
        }
        return false;
    }
}