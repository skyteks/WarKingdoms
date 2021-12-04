using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementNavigation : MonoBehaviour
{
    public enum NavSystems
    {
        UnityNavMesh,
        AStarPro,
    }

    public NavSystems usedNavSystem;

    private NavMeshAgent navMeshAgent;
    private bool agentReady;



    public float velocity
    {
        get
        {
            switch (usedNavSystem)
            {
                case NavSystems.UnityNavMesh:
                    return navMeshAgent.velocity.magnitude;
                case NavSystems.AStarPro:
                    throw new System.NotImplementedException();
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
                case NavSystems.AStarPro:
                    throw new System.NotImplementedException();
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
                case NavSystems.AStarPro:
                    throw new System.NotImplementedException();
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
                case NavSystems.AStarPro:
                    throw new System.NotImplementedException();
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
                case NavSystems.AStarPro:
                    throw new System.NotImplementedException();
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
                case NavSystems.AStarPro:
                    throw new System.NotImplementedException();
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
                case NavSystems.AStarPro:
                    throw new System.NotImplementedException();
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
                case NavSystems.AStarPro:
                    throw new System.NotImplementedException();
            }
        }
    }

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void OnEnable()
    {
        navMeshAgent.enabled = true;
        StartCoroutine(ReadyUpNavMeshAgent());
    }

    void OnDisable()
    {
        navMeshAgent.enabled = false;
        agentReady = false;
    }

    void Update()
    {
        
    }

    public void SetDestination(Vector3 position)
    {
        switch (usedNavSystem)
        {
            case NavSystems.UnityNavMesh:
                navMeshAgent.SetDestination(position);
                break;
            case NavSystems.AStarPro:
                break;
        }
    }

    private IEnumerator ReadyUpNavMeshAgent()
    {
        yield return null;
        agentReady = true;
    }
}
