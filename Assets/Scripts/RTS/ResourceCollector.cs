using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCollector : MonoBehaviour
{
    public int woodPerHitEarnings = 1;

    public int maxWood = 10;
    public int maxOre = 20;
    public int storage { get; private set; }
    public ResourceSource.ResourceType storedType { get; private set; }

    public void AddResource(int amount, ResourceSource.ResourceType type)
    {
        int max = 0;
        switch (type)
        {
            case ResourceSource.ResourceType.Wood:
                max = maxWood;
                break;
            case ResourceSource.ResourceType.Ore:
                max = maxOre;
                break;
        }
        storedType = type;
        storage = Mathf.Clamp(storage + amount, 0, maxWood);
    }

    public KeyValuePair<ResourceSource.ResourceType, int> EmptyStorage()
    {
        var tmp = new KeyValuePair<ResourceSource.ResourceType, int>(storedType, storage);
        storage = 0;
        return tmp;
    }
}
