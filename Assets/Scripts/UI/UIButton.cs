using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIButton : UIClickable
{
    public override void OnPointerDown(PointerEventData e)
    {
        base.OnPointerDown(e);

        if (e.pointerId == -1)
        {
            ClickThis();
        }
        else if (e.pointerId == -2)
        {
            ClickSomethingElse();
        }
    }

    protected abstract void ClickThis();

    protected abstract void ClickSomethingElse();
}
