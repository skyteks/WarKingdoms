using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button to select/deselect a unit in the UI
/// </summary>
public class PortraitButton : UnitButton
{
    public Text damageTitle;
    public Text armorTitle;

    public Text healthText;
    public Text manaText;
    public Text damageText;
    public Text armorText;
    public Text attackSpeedText;
    //public Text movementSpeedText;

    public override void UpdateButton()
    {
        base.UpdateButton();
        if (unit.template.original.health > 1)
        {
            healthText.enabled = true;
            float fill = (float)unit.template.health / (float)unit.template.original.health;
            healthText.text = string.Concat(unit.template.health, " / ", unit.template.original.health);
            if (fill > 0.5f)
            {
                healthText.color = Color.Lerp(healthColorOrange, healthColorGreen, fill.LinearRemap(0.5f, 1f));
            }
            else
            {
                healthText.color = Color.Lerp(healthColorRed, healthColorOrange, fill.LinearRemap(0f, 0.5f));
            }
        }
        else
        {
            healthText.enabled = false;
        }

        if (unit.template.original.mana > 0)
        {
            manaText.enabled = true;
            manaText.text = string.Concat(unit.template.mana, " / ", unit.template.original.mana);
            manaText.color = manaColor;
        }
        else
        {
            manaText.enabled = false;
        }

        if (unit.template.damage.y > 0)
        {
            damageTitle.enabled = true;
            damageText.enabled = true;
            damageText.text = string.Concat(unit.template.damage.x, " - ", unit.template.damage.y);
        }
        else
        {
            damageTitle.enabled = false;
            damageText.enabled = false;
        }

        armorTitle.enabled = true;
        armorText.enabled = true;
        armorText.text = unit.template.armor.ToString();

        //movementSpeedText.text = Unit.template.movementSpeed.ToString();
    }

    public override void SetupButton(ClickableObject unitForButton, Color healthGreen, Color healthOrange, Color healthRed, Color manaBlue)
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
