using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class adds some extension methods for Lists
/// </summary>
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

    public static bool ScrambledEqualsFast<T>(this IList<T> list1, IList<T> list2, IEqualityComparer<T> comparer = null)
    {
        if (list1.Count != list2.Count) return false;

        Dictionary<T, int> cnt;
        if (comparer != null) cnt = new Dictionary<T, int>(comparer);
        else cnt = new Dictionary<T, int>();

        foreach (T s in list1)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]++;
            }
            else
            {
                cnt.Add(s, 1);
            }
        }
        foreach (T s in list2)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]--;
            }
            else
            {
                return false;
            }
        }
        return cnt.Values.All(c => c == 0);
    }

    public static bool ScrambledEqualsHashSet<T>(this IList<T> list1, IList<T> list2)
    {
        if (list1.Count != list2.Count) return false;
        var set1 = new HashSet<T>(list1);
        var set2 = new HashSet<T>(list2);
        return set1.SetEquals(set2);
    }

    public static bool ScrambledEquals<T>(this IList<T> list1, IList<T> list2)
    {
        if (list1.Count != list2.Count) return false;

        var lookup1 = list1.ToLookup(t => t);
        var lookup2 = list2.ToLookup(t => t);

        return lookup1.Count == lookup2.Count &&
        lookup1.All(group => lookup2.Contains(group.Key) &&
        lookup2[group.Key].Count() == group.Count());
    }

    public static string ToStringList<T>(this IList<T> list)
    {
        string tmp = "";
        foreach (var obj in list)
        {
            tmp = string.Concat(tmp, obj.ToString(), "\n");
        }
        return tmp;
    }
}
