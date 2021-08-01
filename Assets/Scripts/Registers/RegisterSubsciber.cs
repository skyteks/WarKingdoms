using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RegisterSubsciber : MonoBehaviour
{
    [System.Serializable]
    public struct RegistersAndObjectsToAdd
    {
        public RegisterObject listHolder;
        public MonoBehaviour objectToAdd;
    }

    [UnityEngine.Serialization.FormerlySerializedAs("lists")]
    public RegistersAndObjectsToAdd[] registers;

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
        foreach (var list in registers)
        {
            list.listHolder.AddObject(list.objectToAdd);
        }
    }

    private void Remove()
    {
        foreach (var list in registers)
        {
            list.listHolder.RemoveObject(list.objectToAdd);
        }
    }
}
