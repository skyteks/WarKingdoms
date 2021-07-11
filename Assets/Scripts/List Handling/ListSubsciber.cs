using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ListSubsciber : MonoBehaviour
{
    [System.Serializable]
    public struct ListsAndObjectsToAdd
    {
        public ListHolderObject listHolder;
        public MonoBehaviour objectToAdd;
    }

    public ListsAndObjectsToAdd[] lists;

    private void OnEnable()
    {
        Add();
    }

    private void OnDisable()
    {
        Remove();
    }

    private void Add()
    {
        foreach (var list in lists)
        {
            list.listHolder.AddObject(list.objectToAdd);
        }
    }

    private void Remove()
    {
        foreach (var list in lists)
        {
            list.listHolder.RemoveObject(list.objectToAdd);
        }
    }
}
