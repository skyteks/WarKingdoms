using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Button to select/deselect a unit in the UI
/// </summary>
public class UnitButton : UIClickable
{
    public Image portrait;
    public Image healthbarSlice;
    protected Color healthColorGreen;
    protected Color healthColorOrange;
    protected Color healthColorRed;

    public Unit Unit { get; protected set; }

    public override void OnPointerDown(PointerEventData e)
    {
        base.OnPointerDown(e);

        if (e.pointerId == -1)
        {
            GameManager.Instance.SetSelection(Unit);
        }
        else if (e.pointerId == -2)
        {
            GameManager.Instance.RemoveFromSelection(Unit);
        }
    }

    public virtual void SetupButton(Unit unitForButton, Color healthGreen, Color healthOrange, Color healthRed)
    {
        Unit = unitForButton;
        healthColorGreen = healthGreen;
        healthColorOrange = healthOrange;
        healthColorRed = healthRed;
    }

    public virtual void UpdateButton()
    {
        portrait.sprite = Unit.template.icon;
        float fill = (float)Unit.template.health / (float)Unit.template.original.health;
        if (healthbarSlice != null)
        {
            healthbarSlice.fillAmount = fill;
            if (fill > 0.5f)
            {
                healthbarSlice.color = Color.Lerp(healthColorOrange, healthColorGreen, fill.LinearRemap(0.5f, 1f));
            }
            else
            {
                healthbarSlice.color = Color.Lerp(healthColorRed, healthColorOrange, fill.LinearRemap(0f, 0.5f));
            }
        }
    }
}
