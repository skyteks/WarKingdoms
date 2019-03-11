using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles selection rectangle for Unit selection
/// </summary>
public class UIManager : Singleton<UIManager>
{
    public Image selectionRectangle;
    public GridLayoutGroup selectionLayoutGroup;
    public GameObject selectedUnitUIPrefab;
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
        GameObject holder = Instantiate<GameObject>(selectedUnitUIPrefab, selectionLayoutGroup.transform);
        holder.GetComponent<UnitButton>().unit = newSelectedUnit;
    }

    public void UpdateSelection()
    {
        Transform[] children = selectionLayoutGroup.transform.GetChildren();
        foreach (var child in children)
        {
            Unit unit = child.GetComponent<UnitButton>().unit;
            child.FindDeepChild("Portrait").GetComponent<Image>().sprite = unit.template.icon;
            child.FindDeepChild("HealthbarSlice").GetComponent<Image>().fillAmount = (float)unit.template.health / (float)unit.template.original.health;
            //child.FindDeepChild("HealthbarSlice").GetComponentInChildren<Text>().text = unit.template.health + " / " + unit.template.original.health;
        }
    }

    public void RemoveFromSelection(Unit unitToRemove)
    {
        Transform child = selectionLayoutGroup.transform.GetChildren().Where(holder => holder.GetComponent<UnitButton>().unit == unitToRemove).FirstOrDefault();
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
    }

    public void AddHealthbar(Unit newUnit)
    {
        GameObject holder = Instantiate<GameObject>(healthbarUIPrefab, healthbarsGroup.transform);
        holder.GetComponent<UIAnchor>().canvas = healthbarsGroup;
        holder.GetComponent<UIAnchor>().objectToFollow = newUnit.transform;
    }

    public void UpdateHealthbars()
    {
        Transform[] children = healthbarsGroup.transform.GetChildren();
        foreach (var child in children)
        {
            Unit unit = child.GetComponent<UIAnchor>().objectToFollow.GetComponent<Unit>();
            child.FindDeepChild("HealthbarSlice").GetComponent<Image>().fillAmount = (float)unit.template.health / (float)unit.template.original.health;
            //child.FindDeepChild("HealthbarSlice").GetComponentInChildren<Text>().text = unit.template.health + " / " + unit.template.original.health;
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
