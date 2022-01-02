using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementNavigation : MonoBehaviour
{
    public enum NavSystems
    {
        UnityNavMesh,
        AStarProAIPath,
        AStarProRichAI,
    }

    public NavSystems usedNavSystem;

    // Unity NavMesh
    private UnityEngine.AI.NavMeshAgent navMeshAgent;

    // A* Pro
    private static AstarPath aStarPath;
    private static bool searchedForMesh;
    private Pathfinding.Seeker seeker;
    private Pathfinding.AIPath pathAI;
    private Pathfinding.RichAI richAI;

    public Vector3 velocity
    {
        get
        {
            switch (usedNavSystem)
            {
                case NavSystems.UnityNavMesh:
                    return navMeshAgent.velocity;
                case NavSystems.AStarProAIPath:
                    return pathAI.velocity;
                case NavSystems.AStarProRichAI:
                    return richAI.velocity;
            }
            throw new System.NotSupportedException();
        }
    }

    public Vector3 destination
    {
        get
        {
            switch (usedNavSystem)
            {
                case NavSystems.UnityNavMesh:
                    return navMeshAgent.destination;
                case NavSystems.AStarProAIPath:
                    return pathAI.destination;
                case NavSystems.AStarProRichAI:
                    return richAI.destination;
            }
            throw new System.NotSupportedException();
        }
    }

    public bool isOnMesh
    {
        get
        {
            switch (usedNavSystem)
            {
                case NavSystems.UnityNavMesh:
                    return navMeshAgent.isOnNavMesh;
                case NavSystems.AStarProAIPath:
                    return true;
                case NavSystems.AStarProRichAI:
                    return true;
            }
            throw new System.NotSupportedException();
        }
    }

    public bool hasPath
    {
        get
        {
            switch (usedNavSystem)
            {
                case NavSystems.UnityNavMesh:
                    return navMeshAgent.hasPath;
                case NavSystems.AStarProAIPath:
                    return pathAI.hasPath;
                case NavSystems.AStarProRichAI:
                    return richAI.hasPath;
            }
            throw new System.NotSupportedException();
        }
    }

    public bool pathPending
    {
        get
        {
            switch (usedNavSystem)
            {
                case NavSystems.UnityNavMesh:
                    return /*!agentReady ||*/ navMeshAgent.pathPending;
                case NavSystems.AStarProAIPath:
                    return pathAI.pathPending;
                case NavSystems.AStarProRichAI:
                    return richAI.pathPending;
            }
            throw new System.NotSupportedException();
        }
    }

    public bool isStopped
    {
        get
        {
            switch (usedNavSystem)
            {
                case NavSystems.UnityNavMesh:
                    return navMeshAgent.isStopped;
                case NavSystems.AStarProAIPath:
                    return pathAI.isStopped;
                case NavSystems.AStarProRichAI:
                    return richAI.isStopped;
            }
            throw new System.NotSupportedException();
        }
        set
        {
            switch (usedNavSystem)
            {
                case NavSystems.UnityNavMesh:
                    navMeshAgent.isStopped = value;
                    return;
                case NavSystems.AStarProAIPath:
                    pathAI.isStopped = value;
                    return;
                case NavSystems.AStarProRichAI:
                    richAI.isStopped = value;
                    return;
            }
        }
    }

    public float stoppingDistance
    {
        get
        {
            switch (usedNavSystem)
            {
                case NavSystems.UnityNavMesh:
                    return navMeshAgent.stoppingDistance;
                case NavSystems.AStarProAIPath:
                    return pathAI.endReachedDistance;
                case NavSystems.AStarProRichAI:
                    return richAI.endReachedDistance;
            }
            throw new System.NotSupportedException();
        }
        set
        {
            switch (usedNavSystem)
            {
                case NavSystems.UnityNavMesh:
                    navMeshAgent.stoppingDistance = value;
                    return;
                case NavSystems.AStarProAIPath:
                    pathAI.endReachedDistance = value;
                    return;
                case NavSystems.AStarProRichAI:
                    richAI.endReachedDistance = value;
                    return;
            }
        }
    }

    void Awake()
    {
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        navMeshAgent?.SetEnabled(false);

        seeker = GetComponent<Pathfinding.Seeker>();
        seeker?.SetEnabled(false);
        pathAI = GetComponent<Pathfinding.AIPath>();
        pathAI?.SetEnabled(false);
        richAI = GetComponent<Pathfinding.RichAI>();
        richAI?.SetEnabled(false);

        if (!searchedForMesh && aStarPath == null)
        {
            aStarPath = FindObjectOfType<AstarPath>();
            searchedForMesh = true;
            if (aStarPath != null)
            {
                aStarPath.showGraphs = (usedNavSystem == NavSystems.AStarProAIPath || usedNavSystem == NavSystems.AStarProRichAI);
            }
        }
    }

    void OnEnable()
    {
        switch (usedNavSystem)
        {
            case NavSystems.UnityNavMesh:
                navMeshAgent.enabled = true;
                break;
            case NavSystems.AStarProAIPath:
                seeker.enabled = true;
                pathAI.enabled = true;
                break;
            case NavSystems.AStarProRichAI:
                seeker.enabled = true;
                richAI.enabled = true;
                break;
        }
    }

    void OnDisable()
    {
        navMeshAgent?.SetEnabled(false);

        seeker?.SetEnabled(false);
        pathAI?.SetEnabled(false);
    }

    public void SetDestination(Vector3 position)
    {
        switch (usedNavSystem)
        {
            case NavSystems.UnityNavMesh:
                navMeshAgent.SetDestination(position);
                break;
            case NavSystems.AStarProAIPath:
                pathAI.destination = position;
                break;
            case NavSystems.AStarProRichAI:
                richAI.destination = position;
                break;
        }
    }
}
