using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ClickableObject))]
public class ResourceDropoff : MonoBehaviour
{
    public List<ResourceSource.ResourceType> dropoffTypes = new List<ResourceSource.ResourceType>(2);

    private ClickableObject clickableObject;

    void Awake()
    {
        clickableObject = GetComponent<ClickableObject>();
    }

    public void DropResource(int amount, ResourceSource.ResourceType type)
    {
        if (amount <= 0)
        {
            throw new System.ArgumentOutOfRangeException("Cannot add a negative amount");
        }
        if (dropoffTypes.Contains(type))
        {
            switch (type)
            {
                case ResourceSource.ResourceType.Wood:
                    clickableObject.faction.data.resourceWood += amount;
                    break;
                case ResourceSource.ResourceType.Ore:
                    clickableObject.faction.data.resourceGold += amount;
                    break;
            }
        }
    }
}
