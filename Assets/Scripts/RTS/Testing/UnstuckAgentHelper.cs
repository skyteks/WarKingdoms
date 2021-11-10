using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnstuckAgentHelper : MonoBehaviour
{
    private NavMeshAgent agent;

    private Vector3 lastPos;
    public float maxOffset = 0.1f;
    public float minIdleTime = 2f;
    private float lastMovement;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent.isActiveAndEnabled)
        {
            if (agent.isStopped || !agent.hasPath)
            {
                lastMovement = Time.time;
                return;
            }
            
            if (Time.time - lastMovement > minIdleTime)
            {
                if (Vector3.Distance(lastPos, transform.position) > maxOffset)
                {
                    lastPos = transform.position;
                    lastMovement = Time.time;
                    return;
                }
                lastMovement = Time.time;
                TryUnstuck();
            }
        }
    }

    private void TryUnstuck()
    {
        Debug.LogWarning("Try unstuck " + name, gameObject);

        Vector3 destination = agent.destination;
        agent.ResetPath();
        agent.SetDestination(destination);
    }
}
