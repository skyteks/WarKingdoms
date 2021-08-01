using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class ResourceSource : MonoBehaviour
{
    public enum ResourceType : int //Used for animation
    {
        Ore = 1,
        Wood,
    }

    public ResourceType resourceType;

    [SerializeField]
    private int stored = 20;
    public int maxStorage { get; private set; }

    private InteractableObject interactableObject;
    private RegisterSubsciber registerSubsciber;

    public bool isEmpty
    {
        get
        {
            return stored == 0;
        }
    }

    void Awake()
    {
        interactableObject = GetComponent<InteractableObject>();
        registerSubsciber = GetComponent<RegisterSubsciber>();
    }

    void Start()
    {
        maxStorage = stored;
    }

    public int GetAmount(int amount)
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
            registerSubsciber.enabled = false;
            return amount;
        }
    }
}
