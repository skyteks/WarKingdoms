using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera;
    public Camera miniMapCamera;
    public Rect panLimitBorders = new Rect(-100f, -100f, 200f, 200f);
    public Range scrollLimitBorders = new Range(20f, 50f);

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
        if (mainCamera == null) mainCamera = GetComponent<Camera>();
        if (mainCamera == null) mainCamera = Camera.main;//GameObject.FindObjectOfType<Camera>();
    }

    public void MoveGameplayCamera(Vector3 amount)
    {
        float scrollAmount = amount.y;
        amount.y = 0f;
        Vector3 position = mainCamera.transform.position;
        position += amount;
        Vector3 scrollPosition = position + -mainCamera.transform.forward * scrollAmount;
        if (scrollPosition.y >= scrollLimitBorders.Min && scrollPosition.y <= scrollLimitBorders.Max) position = scrollPosition;
        position.x = Mathf.Clamp(position.x, panLimitBorders.xMin, panLimitBorders.xMax);
        position.y = Mathf.Clamp(position.y, scrollLimitBorders.Min, scrollLimitBorders.Max);
        position.z = Mathf.Clamp(position.z, panLimitBorders.yMin, panLimitBorders.yMax);
        mainCamera.transform.position = position;
    }

    public void MoveGameplayCameraTo(Vector3 point)
    {
        Vector3 middlePoint;
        GetCameraViewPointOnGroundPlane(mainCamera, new Vector3(0.5f, 0.5f, 0f), out middlePoint);
        point -= middlePoint - mainCamera.transform.position.ToWithY(middlePoint.y);
        mainCamera.transform.position = point.ToWithY(mainCamera.transform.position.y);
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