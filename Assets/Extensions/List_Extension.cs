using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class List_Extension
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count - 1;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, list.Count);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
