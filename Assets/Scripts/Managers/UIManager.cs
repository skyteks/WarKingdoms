using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles selection rectangle for Unit selection
/// </summary>
public class UIManager : Singleton<UIManager>
{
    public enum HealthbarColoringMode
    {
        FriendFoe,
        HealthPercentage,
        Teamcolor,
    }

    public Color healthColorGreen = Color.green;
    public Color healthColorRed = Color.red;
    public Color healthColorOrange = Color.Lerp(Color.red, Color.yellow, 0.5f);
    public HealthbarColoringMode healthbarColoringMode;

    public Image selectionRectangle;
    public GridLayoutGroup selectionLayoutGroup;
    public GameObject selectedUnitUIPrefab;
    public PortraitButton selectedPortraitUI;
    public Canvas healthbarsGroup;
    public GameObject healthbarUIPrefab;

    void Start()
    {
        ToggleSelectionRectangle(false);
        ClearSelection();
        ClearHealthbars();
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
        selectionRectangle.rectTransform.ForceUpdateRectTransforms();
        selectionRectangle.rectTransform.sizeDelta = new Vector2(rectSize.width, rectSize.height);
    }

    public void AddToSelection(Unit newSelectedUnit)
    {
        bool showPortrait = GameManager.Instance.GetSelectionLength() == 1;
        if (showPortrait)
        {
            selectedPortraitUI.SetupButton(newSelectedUnit, healthColorGreen, healthColorOrange, healthColorRed);
        }
        else
        {
            selectedPortraitUI.ClearButton();
        }
        selectionLayoutGroup.gameObject.SetActive(!showPortrait);

        GameObject holder = Instantiate<GameObject>(selectedUnitUIPrefab, selectionLayoutGroup.transform);
        holder.GetComponent<UnitButton>().SetupButton(newSelectedUnit, healthColorGreen, healthColorOrange, healthColorRed);
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

    public void RemoveFromSelection(Unit unitToRemove)
    {
        Transform child = selectionLayoutGroup.transform.GetChildren().Where(holder => holder.GetComponent<UnitButton>().Unit == unitToRemove).FirstOrDefault();
        if (child == null)
        {
            return;
        }

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

    public void AddHealthbar(Unit unit)
    {
        GameObject holder = Instantiate<GameObject>(healthbarUIPrefab, healthbarsGroup.transform);
        holder.GetComponent<UIAnchor>().canvas = healthbarsGroup;
        holder.GetComponent<UIAnchor>().objectToFollow = unit.transform;
    }

    public void UpdateHealthbars()
    {
        Transform[] children = healthbarsGroup.transform.GetChildren();
        foreach (var child in children)
        {
            Unit unit = child.GetComponent<UIAnchor>().objectToFollow.GetComponent<Unit>();
            Image healthbarSlice = child.FindDeepChild("HealthbarSlice").GetComponent<Image>();
            healthbarSlice.fillAmount = (float)unit.template.health / (float)unit.template.original.health;
            switch (healthbarColoringMode)
            {
                case HealthbarColoringMode.FriendFoe:
                    //TODO: allied color
                    GameManager gameManager = GameManager.Instance;
                    if (unit.faction == gameManager.playerFaction)
                    {
                        healthbarSlice.color = Color.green;
                    }
                    else if (FactionTemplate.IsAlliedWith(unit.faction, gameManager.playerFaction))
                    {
                        healthbarSlice.color = Color.yellow;
                    }
                    else
                    {
                        healthbarSlice.color = Color.red;
                    }
                    break;
                case HealthbarColoringMode.HealthPercentage:
                    if (healthbarSlice.fillAmount > 0.5f)
                    {
                        healthbarSlice.color = Color.Lerp(healthColorOrange, healthColorGreen, healthbarSlice.fillAmount.LinearRemap(0.5f, 1f));
                    }
                    else
                    {
                        healthbarSlice.color = Color.Lerp(healthColorRed, healthColorOrange, healthbarSlice.fillAmount.LinearRemap(0f, 0.5f));
                    }
                    break;
                case HealthbarColoringMode.Teamcolor:
                    healthbarSlice.color = unit.faction.color;
                    break;
            }
        }
    }

    public void RemoveHealthbar(Unit unitToRemoveFrom)
    {
        Transform child = healthbarsGroup.transform.GetChildren().Where(holder => holder.GetComponent<UIAnchor>().objectToFollow.GetComponent<Unit>() == unitToRemoveFrom).FirstOrDefault();
        if (child == null)
        {
            return;
        }

        child.SetParent(null);
        Destroy(child.gameObject);
        child.gameObject.SetActive(false);
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
