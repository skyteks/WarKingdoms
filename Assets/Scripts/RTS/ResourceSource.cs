﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class ResourceSource : MonoBehaviour
{
    public enum ResourceType
    {
        Wood,
        Ore,
    }

    public ResourceType resourceType;

    [SerializeField]
    private int stored = 20;
    public int maxStorage { get; private set; }

    private InteractableObject interactableObject;

    void Awake()
    {
        interactableObject = GetComponent<InteractableObject>();
    }

    void Start()
    {
        maxStorage = stored;
    }

    public int GetAmount(int amount)
    {
        if (stored > 0)
        {
            if (stored > amount)
            {
                stored -= amount;
                return amount;
            }
            else
            {
                amount = stored;
                stored = 0;
                Destroy(this);
                return amount;
            }
        }
        throw new System.ArgumentException("Source is empty");
    }

    public bool IsEmpty()
    {
        return stored == 0;
    }
}
