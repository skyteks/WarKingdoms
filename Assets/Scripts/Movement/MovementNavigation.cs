using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementNavigation : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    public virtual Vector3 velocity
    {
        get
        {
            return navMeshAgent.velocity;
        }
    }

    public virtual Vector3 destination
    {
        get
        {
            return navMeshAgent.destination;
        }
    }

    public virtual bool isOnMesh
    {
        get
        {
            return navMeshAgent.isOnNavMesh;
        }
    }

    public virtual bool hasPath
    {
        get
        {
            return navMeshAgent.hasPath;
        }
    }

    public virtual bool pathPending
    {
        get
        {
            return /*!agentReady ||*/ navMeshAgent.pathPending;
        }
    }

    public virtual bool isStopped
    {
        get
        {
            return navMeshAgent.isStopped;
        }
        set
        {
            navMeshAgent.isStopped = value;
        }
    }

    public virtual float stoppingDistance
    {
        get
        {
            return navMeshAgent.stoppingDistance;
        }
        set
        {
            navMeshAgent.stoppingDistance = value;
        }
    }

    protected virtual void Awake()
    {
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        navMeshAgent?.SetEnabled(false);
    }

    protected virtual void OnEnable()
    {
        navMeshAgent.enabled = true;
    }

    protected virtual void OnDisable()
    {
        navMeshAgent?.SetEnabled(false);
    }

    public virtual void SetDestination(Vector3 position)
    {
        navMeshAgent.SetDestination(position);
    }
}
