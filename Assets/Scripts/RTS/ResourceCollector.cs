using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
[DisallowMultipleComponent]
public class ResourceCollector : MonoBehaviour
{
    [UnityEngine.Serialization.FormerlySerializedAs("resourceSources")]
    public RegisterObject resourceSourcesRegister;

    public int woodPerHitEarnings = 1;

    public int maxWood = 10;
    public int maxOre = 20;
    public ResourceSource.ResourceType storedType { get; private set; }
    private int storage;

    private Animator animator;

    public bool isFull
    {
        get
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
                default:
                    return false;
            }
            return storage >= max;
        }
    }

    public bool isNotEmpty
    {
        get
        {
            return storage > 0;
        }
    }

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
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
        storage = Mathf.Clamp(storage + amount, 0, max);
    }

    public KeyValuePair<ResourceSource.ResourceType, int> EmptyStorage()
    {
        var tmp = new KeyValuePair<ResourceSource.ResourceType, int>(storedType, storage);
        storage = 0;
        animator?.SetFloat("DoCarry", 0f);
        return tmp;
    }
}
