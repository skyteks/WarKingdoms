using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class MovementNavigationAIPath : MovementNavigation
{
    private static AstarPath aStarPath;
    private static bool searchedForMesh;
    private Pathfinding.Seeker seeker;
    private Pathfinding.AIPath pathAI;

    public override Vector3 velocity
    {
        get
        {
            return pathAI.velocity;
        }
    }

    public override Vector3 destination
    {
        get
        {
            return pathAI.destination;
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
            return pathAI.hasPath;
        }
    }

    public override bool pathPending
    {
        get
        {
            return pathAI.pathPending;
        }
    }

    public override bool isStopped
    {
        get
        {
            return pathAI.isStopped;
        }
        set
        {
            pathAI.isStopped = value;
        }
    }

    public override float stoppingDistance
    {
        get
        {
            return pathAI.endReachedDistance;
        }
        set
        {
            pathAI.endReachedDistance = value;
        }
    }

    protected override void Awake()
    {
        seeker = GetComponent<Pathfinding.Seeker>();
        seeker?.SetEnabled(false);
        pathAI = GetComponent<Pathfinding.AIPath>();
        pathAI?.SetEnabled(false);

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
        pathAI.enabled = true;
    }

    protected override void OnDisable()
    {
        seeker?.SetEnabled(false);
        pathAI?.SetEnabled(false);
    }

    public override void SetDestination(Vector3 position)
    {
        pathAI.destination = position;
    }
}

*/