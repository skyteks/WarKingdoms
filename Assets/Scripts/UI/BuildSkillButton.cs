using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildSkillButton : UIButton
{
    private Image portrait;

    public UnitTemplate building { get; protected set; }

    private void Awake()
    {
        portrait = GetComponent<Image>();
    }

    protected override void ClickThis()
    {
        BuildingPlacement placement = UIManager.Instance.GetComponent<BuildingPlacement>();
        placement.SetBuildingToPlace(building.prefab);
    }

    protected override void ClickSomethingElse()
    {
    }

    public void SetupButton(UnitTemplate buildingForButton)
    {
        building = buildingForButton;
        portrait.sprite = buildingForButton.icon;
    }
}
