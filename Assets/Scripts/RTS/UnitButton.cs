using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
