using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Button to select/deselect a unit in the UI
/// </summary>
public class PortraitButton : UnitButton
{
    public Text healthText;
    public Text manaText;
    public Text damageText;
    public Text armorText;
    public Text attackSpeedText;
    //public Text movementSpeedText;

    public override void UpdateButton()
    {
        base.UpdateButton();
        float fill = (float)Unit.template.health / (float)Unit.template.original.health;
        healthText.text = string.Concat(Unit.template.health, " / ", Unit.template.original.health);
        if (fill > 0.5f)
        {
            healthText.color = Color.Lerp(healthColorOrange, healthColorGreen, fill.LinearRemap(0.5f, 1f));
        }
        else
        {
            healthText.color = Color.Lerp(healthColorRed, healthColorOrange, fill.LinearRemap(0f, 0.5f));
        }
        manaText.text = (Unit.template.original.mana > 0) ? string.Concat(Unit.template.mana, " / ", Unit.template.original.mana) : "";
        manaText.color = manaColor;
        damageText.text = string.Concat(Unit.template.damage.x, " - ", Unit.template.damage.y);
        attackSpeedText.text = Unit.template.attackSpeed.ToString();
        //movementSpeedText.text = Unit.template.movementSpeed.ToString();
    }

    public override void SetupButton(Unit unitForButton, Color healthGreen, Color healthOrange, Color healthRed, Color manaBlue)
    {
        base.SetupButton(unitForButton, healthGreen, healthOrange, healthRed, manaBlue);
        gameObject.SetActive(true);
    }

    public void ClearButton()
    {
        portrait.sprite = null;
        healthText.text = "";
        manaText.text = "";
        damageText.text = "";
        armorText.text = "";
        attackSpeedText.text = "";
        gameObject.SetActive(false);
    }
}
