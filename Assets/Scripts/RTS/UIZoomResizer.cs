using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIAnchor))]
public class UIZoomResizer : MonoBehaviour
{
    private UIAnchor anchor;
    private RectTransform rectTransform;

    private float size;

    void Start()
    {
        anchor = GetComponent<UIAnchor>();
        rectTransform = GetComponent<RectTransform>();

        Transform circle = anchor.objectToFollow.Find("SelectionCircle");
        float radius = circle != null ? circle.localScale.x : 1f;
        size = rectTransform.sizeDelta.x * (1f + (Mathf.Log(radius) / Mathf.Log(3f)));
    }

    void LateUpdate()
    {
        Vector3 worldPoint = anchor.objectToFollow.TransformPoint(anchor.localOffset);
        Vector3 sizeVector = Camera.main.transform.right.ToWithY(0f);

#if UNITY_EDITOR
        if (anchor.drawDebugAxis)
        {
            Debug.DrawLine(worldPoint, worldPoint + sizeVector, Color.red);
        }
#endif

        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(worldPoint);
        Vector3 viewportSizePoint = Camera.main.WorldToViewportPoint(worldPoint + sizeVector);

        float diff = (viewportSizePoint - viewportPoint).magnitude * 50f * size;

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, diff);
    }
}
