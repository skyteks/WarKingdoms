using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public Image selectionRectangle;
    public GridLayoutGroup selectionLayoutGroup;
    public GameObject selectedUnitPrefab;

    void Start()
    {
        ToggleSelectionRectangle(false);
        ClearSelection();
    }

    void LateUpdate()
    {
        UpdateSelection();
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

    public void UpdateSelection()
    {
        for (int i = 0; i < selectionLayoutGroup.transform.childCount; i++)
        {
            Transform child = selectionLayoutGroup.transform.GetChild(i);
            Unit unit = child.GetComponent<UnitReference>().reference;

            child.Find("Portrait").GetComponent<Image>().sprite = unit.template.icon;
            child.Find("Healthbar").GetComponent<Image>().fillAmount = (float)unit.template.health / (float)unit.template.original.health;
            child.Find("Healthbar").GetComponentInChildren<Text>().text = unit.template.health + " / " + unit.template.original.health;
        }
    }
    public void AddToSelection(IList<Unit> newSelectedUnits)
    {
        foreach (Unit unit in newSelectedUnits) AddToSelection(unit);
    }

    public void AddToSelection(Unit newSelectedUnit)
    {
        UnitReference holder = Instantiate<GameObject>(selectedUnitPrefab, selectionLayoutGroup.transform).AddComponent<UnitReference>();
        holder.reference = newSelectedUnit;
        holder.GetComponent<Button>().onClick.AddListener(holder.SendEvent);
        holder.OnSend += GameManager.Instance.SetSelection;
    }

    public void RemoveFromSelection(Unit unitToRemove)
    {
        Transform child = selectionLayoutGroup.transform.GetAllChildren().Where(holder => holder.GetComponent<UnitReference>().reference == unitToRemove).FirstOrDefault();
        if (child == null) return;
        child.SetParent(null);
        Destroy(child.gameObject);
        child.gameObject.SetActive(false);
    }

    public void ClearSelection()
    {
        Transform[] children = selectionLayoutGroup.transform.GetAllChildren();
        foreach (var child in children) Destroy(child.gameObject);
        selectionLayoutGroup.transform.DetachChildren();
    }
}
