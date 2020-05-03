using System;
using UnityEngine;
using UnityEngine.UI;

public class EnumToggle : MonoBehaviour
{
    public Text minimapButtonText;
    public Text healthbarButtonText;
    public Text platoonFormationButtonText;

    void Start()
    {
        UIManager uiManager = UIManager.Instance;
        if (minimapButtonText != null)
        {
            minimapButtonText.text = uiManager.minimapColoringMode.ToString();
        }
        if (healthbarButtonText != null)
        {
            healthbarButtonText.text = uiManager.healthbarColoringMode.ToString();
        }

        Platoon selectedPlatoon = GameManager.Instance.selectedPlatoon;
        if (platoonFormationButtonText != null)
        {
            platoonFormationButtonText.text = selectedPlatoon.formationMode.ToString();
        }
    }

    public void ToggleMinimapColor()
    {
        UIManager uiManager = UIManager.Instance;

        int newValue = (((int)uiManager.minimapColoringMode) + 1) % Enum.GetValues(uiManager.minimapColoringMode.GetType()).GetLength(0);
        uiManager.minimapColoringMode = (UIManager.MinimapColoringModes)Enum.ToObject(typeof(UIManager.MinimapColoringModes), newValue);
        if (minimapButtonText != null)
        {
            minimapButtonText.text = uiManager.minimapColoringMode.ToString();
        }
    }

    public void ToggleHealthbarColor()
    {
        UIManager uiManager = UIManager.Instance;

        int newValue = (((int)uiManager.healthbarColoringMode) + 1) % Enum.GetValues(uiManager.healthbarColoringMode.GetType()).GetLength(0);
        uiManager.healthbarColoringMode = (UIManager.HealthbarColoringModes)Enum.ToObject(typeof(UIManager.HealthbarColoringModes), newValue);
        if (healthbarButtonText != null)
        {
            healthbarButtonText.text = uiManager.healthbarColoringMode.ToString();
        }
    }

    public void TogglePlatoonFormationMode()
    {
        Platoon selectedPlatoon = GameManager.Instance.selectedPlatoon;

        int newValue = (((int)selectedPlatoon.formationMode) + 1) % Enum.GetValues(selectedPlatoon.formationMode.GetType()).GetLength(0);
        selectedPlatoon.formationMode = (Platoon.FormationModes)Enum.ToObject(typeof(Platoon.FormationModes), newValue);
        if (platoonFormationButtonText != null)
        {
            platoonFormationButtonText.text = selectedPlatoon.formationMode.ToString();
        }
    }
}
