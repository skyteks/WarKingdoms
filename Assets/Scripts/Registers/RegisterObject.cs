using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "new ListHolderObject", menuName = "List Holder Object")]
public class RegisterObject : ScriptableObject
{
    //public class MonoBehaviourEvent : UnityEvent<MonoBehaviour, System.Type> { }
    public System.Type typeOfListObjects { get; private set; } = null;
    protected List<MonoBehaviour> list = new List<MonoBehaviour>();
#if UNITY_EDITOR
    public List<MonoBehaviour> listForEditor => list;
#endif

    public delegate void MonobehaviorEvent(MonoBehaviour monoBehaviour, System.Type type);
    public event MonobehaviorEvent onAdded;
    public event MonobehaviorEvent onRemoved;

    public int Count => list.Count;

    void OnEnable()
    {
        list.Clear();
    }

    public bool AddObject(MonoBehaviour obj)
    {
        if (obj == null)
        {
            throw new System.NullReferenceException();
        }
        if (list.Contains(obj))
        {
            return false;
        }

        if (typeOfListObjects == null)
        {
            typeOfListObjects = obj.GetType();
        }
        else
        {
            System.Type objType = obj.GetType();
            if (typeOfListObjects.BaseType == objType.BaseType && objType.BaseType != typeof(MonoBehaviour) && objType != typeof(Object))
            {
                typeOfListObjects = typeOfListObjects.BaseType;
            }
            if (!(typeOfListObjects.IsAssignableFrom(objType) || objType.IsAssignableFrom(typeOfListObjects)))
            {
                throw new System.ArrayTypeMismatchException(string.Concat("List Type: ", typeOfListObjects.ToString(), "  Object Type: ", objType.ToString()));
            }
        }

        list.Add(obj);
        onAdded?.Invoke(obj, typeOfListObjects);
        return true;
    }

    public bool RemoveObject(MonoBehaviour obj)
    {
        if (obj == null)
        {
            throw new System.NullReferenceException();
        }
        bool removed = list.Remove(obj);
        if (removed)
        {
            onRemoved?.Invoke(obj, typeOfListObjects);
        }
        return removed;
    }

    public bool ContainsObject(MonoBehaviour obj)
    {
        if (obj == null)
        {
            throw new System.NullReferenceException();
        }
        return list.Contains(obj);
    }

    public MonoBehaviour GetByIndex(int index)
    {
        return list[index];
    }

    public IEnumerable<MonoBehaviour> GetEnumerable()
    {
        return list.AsEnumerable();
    }
}
