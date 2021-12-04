using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementNavigation : MonoBehaviour
{
    public enum NavSystems
    {
        UnityNavMesh,
        AStarPro_AIPath,
        AStarPro_RichAI,
    }

    public NavSystems usedNavSystem;

    // Unity NavMesh
    private UnityEngine.AI.NavMeshAgent navMeshAgent;
    private bool agentReady;

    // A* Pro
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
                case NavSystems.AStarPro_AIPath:
                    return pathAI.velocity;
                case NavSystems.AStarPro_RichAI:
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
                case NavSystems.AStarPro_AIPath:
                    return pathAI.destination;
                case NavSystems.AStarPro_RichAI:
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
                case NavSystems.AStarPro_AIPath:
                    return true;
                case NavSystems.AStarPro_RichAI:
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
                case NavSystems.AStarPro_AIPath:
                    return pathAI.hasPath;
                case NavSystems.AStarPro_RichAI:
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
                    return navMeshAgent.pathPending;
                case NavSystems.AStarPro_AIPath:
                    return pathAI.pathPending;
                case NavSystems.AStarPro_RichAI:
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
                case NavSystems.AStarPro_AIPath:
                    return pathAI.isStopped;
                case NavSystems.AStarPro_RichAI:
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
                case NavSystems.AStarPro_AIPath:
                    pathAI.isStopped = value;
                    return;
                case NavSystems.AStarPro_RichAI:
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
                case NavSystems.AStarPro_AIPath:
                    return pathAI.endReachedDistance;
                case NavSystems.AStarPro_RichAI:
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
                case NavSystems.AStarPro_AIPath:
                    pathAI.endReachedDistance = value;
                    return;
                case NavSystems.AStarPro_RichAI:
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
    }

    void OnEnable()
    {
        switch (usedNavSystem)
        {
            case NavSystems.UnityNavMesh:
                navMeshAgent.enabled = true;
                StartCoroutine(ReadyUpNavMeshAgent());
                break;
            case NavSystems.AStarPro_AIPath:
                seeker.enabled = true;
                pathAI.enabled = true;
                break;
            case NavSystems.AStarPro_RichAI:
                seeker.enabled = true;
                richAI.enabled = true;
                break;
        }
    }

    void OnDisable()
    {
        navMeshAgent?.SetEnabled(false);
        agentReady = false;

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
            case NavSystems.AStarPro_AIPath:
                pathAI.destination = position;
                break;
            case NavSystems.AStarPro_RichAI:
                richAI.destination = position;
                break;
        }
    }

    private IEnumerator ReadyUpNavMeshAgent()
    {
        yield return null;
        agentReady = true;
    }
}
