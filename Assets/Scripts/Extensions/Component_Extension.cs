using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class adds some extension methods for Components
/// </summary>
public static class Component_Extension
{
    /// <summary>
    /// Gets the closest Transform from IEnumerable<Transform>
    /// </summary>
    /// <param name="components">The list/array of Transforms</param>
    /// <param name="currentPosition">The position from where you want the closest from</param>
    /// <returns></returns>
    public static T GetClosestInEnumeration<T>(this IEnumerable<T> components, Vector3 currentPosition) where T : Component
    {
        if (components.Count() == 0)
        {
            return null;
        }

        T closest = null;
        float closestsDistance = Mathf.Infinity;
        foreach (T component in components)
        {
            if (component == null)
            {
                continue;
            }

            float currentsDistance = Vector3.Distance(component.transform.position, currentPosition);
            if (currentsDistance < closestsDistance)
            {
                closestsDistance = currentsDistance;
                closest = component;
            }
        }
        return closest;
    }

    public static T CopyComponent<T>(this T original, GameObject destinationGameObject) where T : Component
    {
        System.Type type = original.GetType();
        T destination = destinationGameObject.AddComponent<T>();
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            if (field.IsStatic)
            {
                continue;
            }

            field.SetValue(destination, field.GetValue(original));
        }
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name")
            {
                continue;
            }

            prop.SetValue(destination, prop.GetValue(original, null), null);
        }
        return destination as T;
    }

    public static void SetEnabled<T>(this T behaviour, bool toggle) where T : Behaviour
    {
        if (behaviour != null)
        {
            behaviour.enabled = toggle;
        }
    }
}
