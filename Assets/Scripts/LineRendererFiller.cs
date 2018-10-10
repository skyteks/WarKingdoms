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
        this.line = GetComponent<LineRenderer>();
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
        if (this.Transforms == null || this.Transforms.Count == 0 || this.Transforms.Any(trans => trans == null)) return;
        if (this.line.positionCount != this.Transforms.Count)
        {
            this.line.positionCount = this.Transforms.Count;
        }
        this.line.useWorldSpace = true;
        for (int i = 0; i < this.Transforms.Count; i++)
        {
            this.line.SetPosition(i, this.Transforms[i].position);
        }
    }
}
