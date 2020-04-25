using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class adds some extension methods for LayerMask
/// </summary>
public static class LayerMask_Extension
{
    public static LayerMask ToEverything(this LayerMask layerMask)
    {
        layerMask.value = -1;
        return layerMask;
    }

    public static LayerMask ToNothing(this LayerMask layerMask)
    {
        layerMask.value = 0;
        return layerMask;
    }

    public static bool Includes(this LayerMask layerMask, string layerName)
    {
        ValidLayerName(layerName);
        return layerMask.Includes(LayerMask.NameToLayer(layerName));
    }

    public static bool Includes(this LayerMask layerMask, int layer)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    public static string[] GetLayers(this LayerMask layerMask)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            if ((layerMask & (1 << i)) != 0) result.Add(LayerMask.LayerToName(i));
        }
        return result.ToArray();
    }

    public static int Count(this LayerMask layerMask)
    {
        int result = 0;
        int value = layerMask.value;
        while (value != 0)
        {
            if (value % 2 == 1) result++;
            value = value / 2;
        }
        return result;
    }

    public static LayerMask Add(this LayerMask layerMask, string layerName)
    {
        ValidLayerName(layerName);
        return layerMask.Add(LayerMask.NameToLayer(layerName));
    }

    public static LayerMask Add(this LayerMask layerMask, int layer)
    {
        return layerMask | 1 << layer;
    }

    public static LayerMask Remove(this LayerMask layerMask, string layerName)
    {
        ValidLayerName(layerName);
        return layerMask.Remove(LayerMask.NameToLayer(layerName));
    }

    public static LayerMask Remove(this LayerMask layerMask, int layer)
    {
        return layerMask & ~(1 << layer);
    }

    private static void ValidLayerName(string layerName)
    {
        if (string.IsNullOrEmpty(layerName)) throw new Exception("Layer name should not be empty");

        if (LayerMask.NameToLayer(layerName) == -1) throw new Exception("Invalid layer name: " + layerName);
    }

    public static LayerMask Inverse(this LayerMask layerMask)
    {
        return ~layerMask;
    }
}
