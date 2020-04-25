using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class adds some extension methods for the Transform component
/// </summary>
public static class Transform_Extension
{
    /// <summary>
    /// Returns the component of Type T in any of the transforms's children
    /// </summary>
    /// <typeparam name="T">The Type of component to retrieve</typeparam>
    /// <param name="transform">The transform you want to search from</param>
    /// <returns></returns>
    public static T GetComponentInChildrenOnly<T>(this Transform transform, bool includeInactive = false) where T : Component
    {
        T[] components = transform.GetComponentsInChildren<T>(includeInactive);
        foreach (var component in components)
        {
            if (component.gameObject != transform.gameObject)
            {
                return component;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns all components of Type T in any of the transforms's children
    /// </summary>
    /// <typeparam name="T">The Type of component to retrieve</typeparam>
    /// <param name="transform">The transform you want to search from</param>
    /// <returns></returns>
    public static T[] GetComponentsInChildrenOnly<T>(this Transform transform, bool includeInactive = false) where T : Component
    {
        List<T> components = transform.GetComponentsInChildren<T>(includeInactive).ToList();
        var own = transform.GetComponents<T>();
        components.RemoveAll(component => own.Contains(component));
        return components.ToArray();
    }

    /// <summary>
    /// Gets all the direct children of a transform
    /// </summary>
    /// <param name="transform">The transform you want to search from</param>
    /// <returns>Array of direct children</returns>
    public static Transform[] GetChildren(this Transform transform)
    {
        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }
        return children;
    }

    /// <summary>
    /// Finds a child in transforms hirarchie and returns it
    /// </summary>
    /// <param name="transform">The transform you want to search from</param>
    /// <param name="name">Name of the child to be found</param>
    /// <returns>Child if found, or null</returns>
    public static Transform FindDeepChild(this Transform transform, string name)
    {
        var result = transform.Find(name);
        if (result != null)
        {
            return result;
        }

        foreach (Transform child in transform)
        {
            result = child.FindDeepChild(name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    public static Ray CreateRay(this Transform transform)
    {
        return new Ray(transform.position, transform.forward);
    }

    /// <summary>
    /// Find closest transform to a point
    /// </summary>
    /// <param name="transforms"></param>
    /// <param name="otherPoint"></param>
    /// <returns></returns>
    public static Transform FindClosestToPoint(this IEnumerable<Transform> transforms, Vector3 otherPoint)
    {
        Transform closestTransform = null;
        float closestDistance = float.PositiveInfinity;
        foreach (Transform transform in transforms)
        {
            float distance = Vector3.Distance(transform.position, otherPoint);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTransform = transform;
            }
        }
        return closestTransform;
    }

    /// <summary>
    /// Find the center point in collection
    /// </summary>
    /// <param name="transforms"></param>
    /// <returns></returns>
    public static Vector3 FindCentroid(this IList<Transform> transforms)
    {
        if (transforms.Count == 0)
        {
            throw new System.ArgumentOutOfRangeException();
        }

        if (transforms.Count == 1)
        {
            return transforms[0].transform.position;
        }

        Vector3 minPoint = transforms[0].transform.position;
        Vector3 maxPoint = transforms[0].transform.position;

        foreach (Transform transform in transforms)
        {
            Vector3 point = transform.position;
            if (point.x < minPoint.x)
            {
                minPoint.x = point.x;
            }
            if (point.x > maxPoint.x)
            {
                maxPoint.x = point.x;
            }
            if (point.y < minPoint.y)
            {
                minPoint.y = point.y;
            }
            if (point.y > maxPoint.y)
            {
                maxPoint.y = point.y;
            }
            if (point.z < minPoint.z)
            {
                minPoint.z = point.z;
            }
            if (point.z > maxPoint.z)
            {
                maxPoint.z = point.z;
            }
        }
        Vector3 centroid = minPoint + (maxPoint - minPoint) * 0.5f;
        return centroid;
    }
}
