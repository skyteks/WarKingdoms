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
            return amount;
        }
    }
}
