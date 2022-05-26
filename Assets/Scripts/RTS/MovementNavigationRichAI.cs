using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class MovementNavigationRichAI : MovementNavigation
{
    private static AstarPath aStarPath;
    private static bool searchedForMesh;
    private Pathfinding.Seeker seeker;
    private Pathfinding.RichAI richAI;

    public override Vector3 velocity
    {
        get
        {
            return richAI.velocity;
        }
    }

    public override Vector3 destination
    {
        get
        {
            return richAI.destination;
        }
    }

    public override bool isOnMesh
    {
        get
        {
            return true;
        }
    }

    public override bool hasPath
    {
        get
        {
            return richAI.hasPath;
        }
    }

    public override bool pathPending
    {
        get
        {
            return richAI.pathPending;
        }
    }

    public override bool isStopped
    {
        get
        {
            return richAI.isStopped;
        }
        set
        {
            richAI.isStopped = value;
        }
    }

    public override float stoppingDistance
    {
        get
        {
            return richAI.endReachedDistance;
        }
        set
        {
            richAI.endReachedDistance = value;
        }
    }

    protected override void Awake()
    {
        seeker = GetComponent<Pathfinding.Seeker>();
        seeker?.SetEnabled(false);
        richAI = GetComponent<Pathfinding.RichAI>();
        richAI?.SetEnabled(false);

        if (!searchedForMesh && aStarPath == null)
        {
            aStarPath = FindObjectOfType<AstarPath>();
            searchedForMesh = true;
            if (aStarPath != null)
            {
                aStarPath.showGraphs = true;
            }
        }
    }

    protected override void OnEnable()
    {
        seeker.enabled = true;
        richAI.enabled = true;
    }

    protected override void OnDisable()
    {
        seeker?.SetEnabled(false);
        richAI.enabled = false;
    }

    public override void SetDestination(Vector3 position)
    {
        richAI.destination = position;
    }
}

*/