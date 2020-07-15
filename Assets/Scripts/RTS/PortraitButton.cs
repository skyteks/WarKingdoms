using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button to select/deselect a unit in the UI
/// </summary>
public class PortraitButton : UnitButton
{
    public Text damageTitle;
    public Text armorTitle;
    public Text attackSpeedTitle;

    public Text healthText;
    public Text manaText;
    public Text damageText;
    public Text armorText;
    public Text attackSpeedText;
    //public Text movementSpeedText;

    public override void UpdateButton()
    {
        base.UpdateButton();
        if (Unit.template.original.health > 1)
        {
            healthText.enabled = true;
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
        }
        else
        {
            healthText.enabled = false;
        }

        if (Unit.template.original.mana > 0)
        {
            manaText.enabled = true;
            manaText.text = string.Concat(Unit.template.mana, " / ", Unit.template.original.mana);
            manaText.color = manaColor;
        }
        else
        {
            manaText.enabled = false;
        }

        if (Unit.template.damage.y > 0)
        {
            damageTitle.enabled = true;
            damageText.enabled = true;
            damageText.text = string.Concat(Unit.template.damage.x, " - ", Unit.template.damage.y);
        }
        else
        {
            damageTitle.enabled = false;
            damageText.enabled = false;
        }

        armorTitle.enabled = true;
        armorText.enabled = true;
        armorText.text = Unit.template.armor.ToString();

        if (Unit.template.attackSpeed > 0)
        {
            attackSpeedTitle.enabled = true;
            attackSpeedText.enabled = true;
            attackSpeedText.text = Unit.template.attackSpeed.ToString();
        }
        else
        {
            attackSpeedTitle.enabled = false;
            attackSpeedText.enabled = false;
        }
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
