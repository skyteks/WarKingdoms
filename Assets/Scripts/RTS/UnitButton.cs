using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Button to select/deselect a unit in the UI
/// </summary>
public class UnitButton : UIButton
{
    public Image portrait;
    public Image healthbarSlice;
    public Image manabarSlice;
    protected Color healthColorGreen;
    protected Color healthColorOrange;
    protected Color healthColorRed;
    protected Color manaColor;

    public ClickableObject unit { get; protected set; }

    protected override void ClickThis()
    {
        GameManager.Instance.SetSelection(unit);
    }

    protected override void ClickSomethingElse()
    {
        GameManager.Instance.RemoveFromSelection(unit);
    }

    public virtual void SetupButton(ClickableObject unitForButton, Color healthGreen, Color healthOrange, Color healthRed, Color manaBlue)
    {
        unit = unitForButton;
        healthColorGreen = healthGreen;
        healthColorOrange = healthOrange;
        healthColorRed = healthRed;
        manaColor = manaBlue;
    }

    public virtual void UpdateButton()
    {
        portrait.sprite = unit.template.icon;
        float fill = (float)unit.template.health / (float)unit.template.original.health;
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
        if (manabarSlice != null)
        {
            if (unit.template.original.mana == 0)
            {
                manabarSlice.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                manabarSlice.fillAmount = (float)unit.template.mana / (float)unit.template.original.mana;
                manabarSlice.color = manaColor;
            }
        }
    }


}
