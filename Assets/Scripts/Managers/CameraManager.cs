using UnityEngine;

/// <summary>
/// Handles Camera movement
/// </summary>
public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera;
    public Camera miniMapCamera;
    public Rect panLimitBorders = new Rect(-100f, -100f, 200f, 200f);
    public Range scrollLimitBorders = new Range(20f, 50f);

    private static Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    public bool isFramingPlatoon { get; private set; } //from the outside it's read-only
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

    public void MoveGameplayCamera(Vector3 amount)
    {
        float scrollAmount = amount.y;
        amount.y = 0f;
        Vector3 position = mainCamera.transform.position;
        position += amount;
        Vector3 scrollPosition = position + -mainCamera.transform.forward * scrollAmount;
        if (scrollPosition.y >= scrollLimitBorders.Min && scrollPosition.y <= scrollLimitBorders.Max)
        {
            position = scrollPosition;
        }

        position.x = Mathf.Clamp(position.x, panLimitBorders.xMin, panLimitBorders.xMax);
        position.y = Mathf.Clamp(position.y, scrollLimitBorders.Min, scrollLimitBorders.Max);
        position.z = Mathf.Clamp(position.z, panLimitBorders.yMin, panLimitBorders.yMax);
        mainCamera.transform.position = position;
    }

    public void MoveGameplayCameraTo(Vector3 point)
    {
        Vector3 middlePoint;
        GetCameraViewPointOnGroundPlane(mainCamera, new Vector3(0.5f, 0.5f, 0f), out middlePoint, InputManager.Instance.groundLayerMask);
        point -= middlePoint - mainCamera.transform.position.ToWithY(middlePoint.y);
        mainCamera.transform.position = point.ToWithY(mainCamera.transform.position.y);
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