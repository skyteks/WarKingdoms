using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// This class filles a LineRenderer's positions array with the positions of its transform array
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LineRendererFiller : MonoBehaviour
{
    private LineRenderer line;
    [HideInInspector]
    public List<Transform> Transforms;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    void LateUpdate()
    {
        SetLinePositions();
    }

    void OnDrawGizmosSelected()
    {
        //SetLinePositions();
    }

    /// <summary>
    /// Adds the transforms positions to the LineRenderer
    /// </summary>
    [ContextMenu("Set Line Positions in LineRenderer")]
    public void SetLinePositions()
    {
        if (Transforms == null || Transforms.Count == 0 || Transforms.Any(trans => trans == null)) return;
        if (line.positionCount != Transforms.Count)
        {
            line.positionCount = Transforms.Count;
        }
        line.useWorldSpace = true;
        for (int i = 0; i < Transforms.Count; i++)
        {
            line.SetPosition(i, Transforms[i].position);
        }
    }
}
