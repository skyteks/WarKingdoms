using System.Collections;
using System.Collections.Generic;
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
    }

    void LateUpdate()
    {
        UpdateSelectionUI();
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

    public void UpdateSelectionUI()
    {
        Unit[] selectedUnits = GameManager.Instance.GetSelectionUnits();

        Transform[] oldChildren = selectionLayoutGroup.transform.GetAllChildren();
        foreach (var oldChild in oldChildren) Destroy(oldChild.gameObject);
        selectionLayoutGroup.transform.DetachChildren();

        foreach (Unit unit in selectedUnits)
        {
            Transform newChild = Instantiate<GameObject>(selectedUnitPrefab, selectionLayoutGroup.transform).transform;
            newChild.Find("Portrait").GetComponent<Image>().sprite = unit.template.icon;
            newChild.Find("Healthbar").GetComponent<Image>().fillAmount = (float)unit.template.health / (float)unit.template.original.health;
            newChild.Find("Healthbar").GetComponentInChildren<Text>().text = unit.template.health + " / " + unit.template.original.health;
        }
    }
}
