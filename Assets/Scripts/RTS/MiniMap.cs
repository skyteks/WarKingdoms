﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles MiniMap mouse input
/// </summary>
public class MiniMap : UIClickable
{
    public Camera miniMapCamera;

    public override void OnDrag(PointerEventData e)
    {
        base.OnDrag(e);
        Vector2 clickPosition = NormalisedClickPosition(e.position, true);

        if (e.pointerId == -1)
        {
            MoveGameplayCameraToMinimapClickPosition(clickPosition);
        }
    }

    public override void OnPointerEnter(PointerEventData e)
    {
        base.OnPointerEnter(e);
    }

    public override void OnPointerExit(PointerEventData e)
    {
        base.OnPointerExit(e);
    }

    public override void OnPointerUp(PointerEventData e)
    {
        base.OnPointerUp(e);
    }

    public override void OnPointerDown(PointerEventData e)
    {
        base.OnPointerDown(e);
        Vector2 clickPosition = NormalisedClickPosition(e.position, true);

        if (e.pointerId == -1)
        {
            MoveGameplayCameraToMinimapClickPosition(clickPosition);
        }
        else if (e.pointerId == -2)
        {
            MoveSelectionToMinimapClickPosition(clickPosition);
        }
    }

    private void MoveGameplayCameraToMinimapClickPosition(Vector2 clickPosition)
    {
        Vector3 hitPoint;
        if (CameraManager.GetCameraViewPointOnGroundPlane(miniMapCamera, clickPosition, out hitPoint, InputManager.Instance.groundLayerMask))
        {
            CameraManager.Instance.MoveGameplayCameraTo(hitPoint);
        }
    }

    private void MoveSelectionToMinimapClickPosition(Vector2 clickPosition)
    {
        if (GameManager.Instance.GetSelectionLength() == 0) return;

        Vector3 hitPoint;
        if (CameraManager.GetCameraViewPointOnGroundPlane(miniMapCamera, clickPosition, out hitPoint, InputManager.Instance.groundLayerMask))
        {
            if (!Input.GetButton("Attack"))
            {
                GameManager.Instance.MoveSelectedUnitsTo(hitPoint);
                Debug.DrawLine(miniMapCamera.transform.position, hitPoint, Color.Lerp(Color.black, Color.green, 0.6f), 1f);
            }
            else
            {
                GameManager.Instance.AttackMoveSelectedUnitsTo(hitPoint);
                Debug.DrawLine(miniMapCamera.transform.position, hitPoint, Color.Lerp(Color.black, Color.Lerp(Color.yellow, Color.red, 0.6f), 0.6f), 1f);
            }
        }
    }
}
