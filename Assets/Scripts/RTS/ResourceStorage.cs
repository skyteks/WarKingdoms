using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceStorage : MonoBehaviour
{
    public enum ResourceType
    {
        None,
        Wood,
        Ore,
    }

    public int maxWood = 10;
    public int maxOre = 20;
    public int storage { get; private set; }
    public ResourceType storedType { get; private set; }

    public void AddResource(int amount, ResourceType type)
    {
        int max = 0;
        switch (type)
        {
            case ResourceType.None:
                throw new System.ArgumentException("Cannot add nothing");
            case ResourceType.Wood:
                max = maxWood;
                break;
            case ResourceType.Ore:
                max = maxOre;
                break;
        }
        storedType = type;
        storage = 0;
        storage = Mathf.Clamp(amount, 0, maxWood);
    }

    public KeyValuePair<ResourceType, int> EmptyStorage()
    {
        var tmp = new KeyValuePair<ResourceType, int>(storedType, storage);
        storage = 0;
        storedType = ResourceType.None;
        return tmp;
    }
}
