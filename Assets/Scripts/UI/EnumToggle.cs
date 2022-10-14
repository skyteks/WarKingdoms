using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A toggle button for UI to switch between enum modes
/// </summary>
public class EnumToggle : MonoBehaviour
{
    public Text factionColorText;
    public Text healthbarColorButtonText;
    public Text platoonFormationText;

    void Start()
    {
        SetFactionColorText();
        SetHealthbarColorText();
        SetPlatoonFormationText();
    }

    public void ToggleFactionColor()
    {
        UIManager uiManager = UIManager.Instance;

        int newValue = (((int)uiManager.factionColoringMode) + 1) % Enum.GetValues(uiManager.factionColoringMode.GetType()).GetLength(0);
        uiManager.factionColoringMode = (UIManager.FactionColoringModes)Enum.ToObject(typeof(UIManager.FactionColoringModes), newValue);
        SetFactionColorText();
    }

    public void ToggleHealthbarColor()
    {
        UIManager uiManager = UIManager.Instance;

        int newValue = (((int)uiManager.healthbarColoringMode) + 1) % Enum.GetValues(uiManager.healthbarColoringMode.GetType()).GetLength(0);
        uiManager.healthbarColoringMode = (UIManager.HealthbarColoringModes)Enum.ToObject(typeof(UIManager.HealthbarColoringModes), newValue);
        SetHealthbarColorText();
    }

    public void TogglePlatoonFormationMode()
    {
        Platoon selectedPlatoon = GameManager.Instance.selectedPlatoon;

        int newValue = (((int)selectedPlatoon.formationMode) + 1) % Enum.GetValues(selectedPlatoon.formationMode.GetType()).GetLength(0);
        selectedPlatoon.formationMode = (Platoon.FormationModes)Enum.ToObject(typeof(Platoon.FormationModes), newValue);
        SetPlatoonFormationText();
    }

    private void SetFactionColorText()
    {
        UIManager uiManager = UIManager.Instance;

        if (factionColorText != null)
        {
            factionColorText.text = string.Concat("Faction Color:\n", uiManager.factionColoringMode.ToString());
        }
    }

    private void SetHealthbarColorText()
    {
        UIManager uiManager = UIManager.Instance;

        if (healthbarColorButtonText != null)
        {
            healthbarColorButtonText.text = string.Concat("Healthbar Color:\n", uiManager.healthbarColoringMode.ToString());
        }
    }

    private void SetPlatoonFormationText()
    {
        Platoon selectedPlatoon = GameManager.Instance.selectedPlatoon;

        if (platoonFormationText != null)
        {
            platoonFormationText.text = string.Concat("Formation:\n", selectedPlatoon.formationMode.ToString());
        }
    }
}
