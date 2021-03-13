using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class ResourceCollector : MonoBehaviour
{
    public int woodPerHitEarnings = 1;

    public int maxWood = 10;
    public int maxOre = 20;
    [SerializeField]
    [ReadOnly]
    private ResourceSource.ResourceType storedType;
    [SerializeField]
    [ReadOnly]
    private int storage;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void AddResource(int amount, ResourceSource.ResourceType type)
    {
        if (amount == 0)
        {
            return;
        }
        if (storedType != type)
        {
            storage = 0;
            storedType = type;
        }
        animator?.SetFloat("DoCarry", (float)type);
        int max = 0;
        switch (storedType)
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
        animator?.SetFloat("DoCarry", 0f);
        return tmp;
    }

    public bool IsFull()
    {
        int max = 0;
        switch (storedType)
        {
            case ResourceSource.ResourceType.Wood:
                max = maxWood;
                break;
            case ResourceSource.ResourceType.Ore:
                max = maxOre;
                break;
        }
        return storage >= max;
    }

    public bool IsNotEmpty()
    {
        return storage > 0;
    }
}
