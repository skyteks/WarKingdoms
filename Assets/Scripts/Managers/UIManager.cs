using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles selection rectangle for Unit selection
/// </summary>
public class UIManager : Singleton<UIManager>
{
    public enum HealthbarColoringModes : int
    {
        UnitColor,
        HealthPercentage,
    }
    public enum FactionColoringModes : int
    {
        TeamColor,
        FriendFoe,
    }

    public Color healthColorGreen = Color.green;
    public Color healthColorRed = Color.red;
    public Color healthColorOrange = Color.Lerp(Color.red, Color.yellow, 0.5f);
    public Color manaColor = Color.blue;

    [Space]

    public Color factionPlayerColor = Color.blue;
    public Color factionAlliesColor = Color.cyan;
    public Color factionEnemiesColor = Color.red;

    [Space]

    public Color uiPlayerColor = Color.green;
    public Color uiAlliesColor = Color.yellow;
    public Color uiEnemiesColor = Color.red;

    [Space]

    public FactionColoringModes factionColoringMode;
    public HealthbarColoringModes healthbarColoringMode;
    public bool showHealthbars;

    [Space]

    public Image selectionRectangle;
    public GridLayoutGroup selectionLayoutGroup;
    public GameObject selectedUnitUIPrefab;
    public PortraitButton selectedPortraitUI;
    public Canvas healthbarsGroup;
    public GameObject healthbarUIPrefab;

    public Text resourceGoldText;
    public Text resourceWoodText;

    [Space]

    public GridLayoutGroup skillsLayoutGroup;
    public GameObject skillButtonUIPrefab;

    void Awake()
    {
        ToggleSelectionRectangle(false);
        ClearSelection();
        ClearHealthbars();
    }

    void Update()
    {
        GameManager gameManager = GameManager.Instance;

        if (resourceGoldText != null && resourceWoodText != null)
        {
            resourceGoldText.text = gameManager.playerFaction.data.resourceGold.ToString();
            resourceWoodText.text = gameManager.playerFaction.data.resourceWood.ToString();
        }
    }

    void LateUpdate()
    {
        UpdateSelection();
        UpdateHealthbars();
    }

    public Color GetFactionColorForColorMode(FactionTemplate faction, bool shiftUpOnLowGreyscale = false)
    {
        GameManager gameManager = GameManager.Instance;


        Color tmpColor = Color.clear;
        switch (factionColoringMode)
        {
            case UIManager.FactionColoringModes.FriendFoe:
                if (faction == gameManager.playerFaction)
                {
                    tmpColor = factionPlayerColor;
                }
                else if (FactionTemplate.IsAlliedWith(faction, gameManager.playerFaction))
                {
                    tmpColor = factionAlliesColor;
                }
                else
                {
                    tmpColor = factionEnemiesColor;
                }
                break;
            case UIManager.FactionColoringModes.TeamColor:
                tmpColor = faction.color;
                if (shiftUpOnLowGreyscale && tmpColor.grayscale < 0.2f)
                {
                    tmpColor = tmpColor.ToSumWithoutAlpha(Vector3.one * 0.28f);
                }
                break;
        }
        return tmpColor;
    }

    public Color GetUIColorForColorMode(FactionTemplate faction, bool selected)
    {
        GameManager gameManager = GameManager.Instance;


        Color tmpColor = Color.clear;
        if (faction == gameManager.playerFaction)
        {
            tmpColor = uiPlayerColor;
        }
        else if (FactionTemplate.IsAlliedWith(faction, gameManager.playerFaction))
        {
            tmpColor = uiAlliesColor;
        }
        else
        {
            tmpColor = uiEnemiesColor;
        }
        tmpColor.a = selected ? 1f : 0.3f;

        return tmpColor;
    }

    public void ToggleSelectionRectangle(bool active)
    {
        selectionRectangle.enabled = active;
    }

    public void SetSelectionRectangle(Rect rectSize)
    {
        selectionRectangle.rectTransform.position = rectSize.center;
        selectionRectangle.rectTransform.sizeDelta = rectSize.size;
        selectionRectangle.rectTransform.ForceUpdateRectTransforms();
    }

    public void AddToSelection(ClickableObject newSelectedUnit)
    {
        GameManager gameManager = GameManager.Instance;
        bool showPortrait = gameManager.GetSelectionLength() == 1;
        if (showPortrait)
        {
            selectedPortraitUI.SetupButton(newSelectedUnit, healthColorGreen, healthColorOrange, healthColorRed, manaColor);
            newSelectedUnit.OnDeath += RemoveFromSelection;

            SetupSkillButtons(newSelectedUnit);
        }
        else
        {
            selectedPortraitUI.ClearButton();

            ClearSkillButtons();
        }
        selectionLayoutGroup.gameObject.SetActive(!showPortrait);

        UnitButton button = Instantiate<GameObject>(selectedUnitUIPrefab, selectionLayoutGroup.transform).GetComponent<UnitButton>();
        button.SetupButton(newSelectedUnit, healthColorGreen, healthColorOrange, healthColorRed, manaColor);
    }

    private void SetupSkillButtons(ClickableObject newSelectedUnit)
    {
        BuildingBuilder builder = newSelectedUnit.GetComponent<BuildingBuilder>();
        if (builder != null)
        {
            foreach (var building in builder.buildingsToBuild)
            {
                BuildSkillButton button = Instantiate<GameObject>(skillButtonUIPrefab, skillsLayoutGroup.transform).GetComponent<BuildSkillButton>();
                button.SetupButton(building);
            }
        }
    }

    private void ClearSkillButtons()
    {
        Transform[] children = skillsLayoutGroup.transform.GetChildren();
        foreach (var child in children)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateSelection()
    {
        Transform[] children = selectionLayoutGroup.transform.GetChildren();
        foreach (var child in children)
        {
            child.GetComponent<UnitButton>().UpdateButton();
        }
        if (children.Length == 1)
        {
            selectedPortraitUI.UpdateButton();
        }
        else
        {
            selectedPortraitUI.ClearButton();
        }
    }

    public void RemoveFromSelection(ClickableObject unitToRemove)
    {
        Transform child = selectionLayoutGroup.transform.GetChildren().Where(holder => holder.GetComponent<UnitButton>().unit == unitToRemove).FirstOrDefault();
        if (child == null)
        {
            return;
        }

        unitToRemove.OnDeath -= RemoveFromSelection;

        child.SetParent(null);
        Destroy(child.gameObject);
        child.gameObject.SetActive(false);
    }

    public void ClearSelection()
    {
        Transform[] children = selectionLayoutGroup.transform.GetChildren();
        foreach (var child in children)
        {
            child.SetParent(null);
            Destroy(child.gameObject);
            child.gameObject.SetActive(false);
        }
        selectedPortraitUI.ClearButton();

        ClearSkillButtons();
    }

    public void AddHealthbar(ClickableObject unit)
    {
        if (!showHealthbars)
        {
            return;
        }

        Transform holder = Instantiate<GameObject>(healthbarUIPrefab, healthbarsGroup.transform).transform;
        holder.GetComponent<UIAnchor>().canvas = healthbarsGroup;
        holder.GetComponent<UIAnchor>().objectToFollow = unit.transform;

        unit.OnDeath += RemoveHealthbar;
        unit.OnDisapearInFOW += RemoveHealthbar;

        holder.GetComponent<UIAnchor>().screenOffset *= unit.GetSelectionCircleSize() * 1.1f;
        RectTransform rectTransform = holder.FindDeepChild("HealthbarSlice").parent.GetComponent<RectTransform>();
        rectTransform.sizeDelta = rectTransform.sizeDelta.ToScale(new Vector2(unit.GetSelectionCircleSize(), 1f));
        rectTransform = holder.FindDeepChild("ManabarSlice").parent.GetComponent<RectTransform>();
        rectTransform.sizeDelta = rectTransform.sizeDelta.ToScale(new Vector2(unit.GetSelectionCircleSize(), 1f));
    }

    public void UpdateHealthbars()
    {
        Transform[] children = healthbarsGroup.transform.GetChildren();
        foreach (var child in children)
        {
            ClickableObject unit = child.GetComponent<UIAnchor>().objectToFollow.GetComponent<ClickableObject>();
            Image healthbarSlice = child.FindDeepChild("HealthbarSlice").GetComponent<Image>();
            healthbarSlice.fillAmount = (float)unit.template.health / (float)unit.template.original.health;
            switch (healthbarColoringMode)
            {
                case HealthbarColoringModes.UnitColor:
                    healthbarSlice.color = GetFactionColorForColorMode(unit.faction, true);
                    break;
                case HealthbarColoringModes.HealthPercentage:
                    if (healthbarSlice.fillAmount > 0.5f)
                    {
                        healthbarSlice.color = Color.Lerp(healthColorOrange, healthColorGreen, healthbarSlice.fillAmount.LinearRemap(0.5f, 1f));
                    }
                    else
                    {
                        healthbarSlice.color = Color.Lerp(healthColorRed, healthColorOrange, healthbarSlice.fillAmount.LinearRemap(0f, 0.5f));
                    }
                    break;
            }

            Image manabarSlice = child.FindDeepChild("ManabarSlice").GetComponent<Image>();
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

    public void RemoveHealthbar(ClickableObject unitToRemoveFrom)
    {
        Transform holder = healthbarsGroup.transform.GetChildren().Where(holderr => holderr.GetComponent<UIAnchor>().objectToFollow.GetComponent<ClickableObject>() == unitToRemoveFrom).FirstOrDefault();
        if (holder == null)
        {
            return;
        }

        ClickableObject unit = holder.GetComponent<UIAnchor>().objectToFollow.GetComponent<ClickableObject>();
        unit.OnDeath -= RemoveHealthbar;
        unit.OnDisapearInFOW -= RemoveHealthbar;

        holder.SetParent(null);
        Destroy(holder.gameObject);
        holder.gameObject.SetActive(false);
    }

    public void ClearHealthbars()
    {
        Transform[] children = healthbarsGroup.transform.GetChildren();
        foreach (var child in children)
        {
            child.SetParent(null);
            Destroy(child.gameObject);
            child.gameObject.SetActive(false);
        }
    }
}
