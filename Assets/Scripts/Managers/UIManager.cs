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
        FriendFoe,
        HealthPercentage,
        Teamcolor,
    }
    public enum MinimapColoringModes : int
    {
        FriendFoe,
        Teamcolor,
    }

    public Color healthColorGreen = Color.green;
    public Color healthColorRed = Color.red;
    public Color healthColorOrange = Color.Lerp(Color.red, Color.yellow, 0.5f);
    public Color manaColor = Color.blue;

    [Space]

    public HealthbarColoringModes healthbarColoringMode;
    public bool showHealthbars;
    public MinimapColoringModes minimapColoringMode;

    [Space]

    public Image selectionRectangle;
    public GridLayoutGroup selectionLayoutGroup;
    public GameObject selectedUnitUIPrefab;
    public PortraitButton selectedPortraitUI;
    public Canvas healthbarsGroup;
    public GameObject healthbarUIPrefab;

    public Text resourceGoldText;
    public Text resourceWoodText;

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
            resourceGoldText.text = gameManager.playerFaction.resourceGold.ToString();
            resourceWoodText.text = gameManager.playerFaction.resourceWood.ToString();
        }
    }

    void LateUpdate()
    {
        UpdateSelection();
        UpdateHealthbars();
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
        }
        else
        {
            selectedPortraitUI.ClearButton();
        }
        selectionLayoutGroup.gameObject.SetActive(!showPortrait);

        GameObject holder = Instantiate<GameObject>(selectedUnitUIPrefab, selectionLayoutGroup.transform);
        holder.GetComponent<UnitButton>().SetupButton(newSelectedUnit, healthColorGreen, healthColorOrange, healthColorRed, manaColor);
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
        Transform child = selectionLayoutGroup.transform.GetChildren().Where(holder => holder.GetComponent<UnitButton>().Unit == unitToRemove).FirstOrDefault();
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
            // scale bars according to camera zoom
            //RectTransform rectTransform = child.GetComponent<RectTransform>();
            //rectTransform.sizeDelta = new Vector2(100f * (1f / CameraManager.Instance.GetDifferenceToOptimalZoom() * 1.5f), rectTransform.sizeDelta.y);

            ClickableObject unit = child.GetComponent<UIAnchor>().objectToFollow.GetComponent<ClickableObject>();
            Image healthbarSlice = child.FindDeepChild("HealthbarSlice").GetComponent<Image>();
            healthbarSlice.fillAmount = (float)unit.template.health / (float)unit.template.original.health;
            switch (healthbarColoringMode)
            {
                case HealthbarColoringModes.FriendFoe:
                    GameManager gameManager = GameManager.Instance;
                    if (unit.faction == gameManager.playerFaction)
                    {
                        healthbarSlice.color = healthColorGreen;
                    }
                    else if (FactionTemplate.IsAlliedWith(unit.faction, gameManager.playerFaction))
                    {
                        healthbarSlice.color = Color.yellow;
                    }
                    else
                    {
                        healthbarSlice.color = healthColorRed;
                    }
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
                case HealthbarColoringModes.Teamcolor:
                    healthbarSlice.color = unit.faction.color;
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
