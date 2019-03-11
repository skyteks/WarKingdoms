﻿using UnityEngine.EventSystems;

/// <summary>
/// Button to select/deselect a unit in the UI
/// </summary>
public class UnitButton : UIClickable
{
    public Unit unit;

    public override void OnPointerDown(PointerEventData e)
    {
        base.OnPointerDown(e);

        if (e.pointerId == -1)
        {
            GameManager.Instance.SetSelection(unit);
        }
        else if (e.pointerId == -2)
        {
            GameManager.Instance.RemoveFromSelection(unit);
        }
    }
}
