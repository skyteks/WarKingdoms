using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class adds some extension methods for the GameObject
/// </summary>
public static class GameObject_Extension
{
    public static bool IsPrefab(this GameObject gameObject)
    {
        return /*gameObject.scene == null ? true : */gameObject.scene.rootCount == 0;
    }

    /// <summary>
    /// Sets the layer for itself and all children
    /// </summary>
    /// <param name="gameObject">the object at which you want to start</param>
    /// <param name="layer">the layer you want to set</param>
    public static void SetLayerRecursivly(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        Transform[] children = gameObject.transform.GetChildren();
        foreach (var child in children)
        {
            SetLayerRecursivly(child.gameObject, layer);
        }
    }
}
