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
}
