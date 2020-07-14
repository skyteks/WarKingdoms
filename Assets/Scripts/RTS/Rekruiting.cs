using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Building))]
public class Rekruiting : MonoBehaviour
{
    private Building building;

    public GameObject[] rekruitablesPrefabs;

    public Vector3 waypointPos;
    private byte navmeshReady = 0;

    void Awake()
    {
        building = GetComponent<Building>();
    }

    void Update()
    {
        if (navmeshReady == 1)
        {
            SetWaypoint(transform.position);
        }
        if (navmeshReady != 2)
        {
            navmeshReady++;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(waypointPos, Vector3.up, 0.1f);
            UnityEditor.Handles.DrawLine(transform.position, waypointPos);
        }
    }
#endif

    public void SetWaypoint(Vector3 position)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, building.template.guardDistance, NavMesh.AllAreas))
        {
            waypointPos = hit.position;
        }
        else
        {
            waypointPos = Vector3.one * float.NaN;
        }
    }

    public Unit SpawnUnit(int index)
    {
        if (rekruitablesPrefabs.Length < index)
        {
            throw new IndexOutOfRangeException();
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, building.template.guardDistance, NavMesh.AllAreas))
        {
            GameObject unitGameObject = Instantiate<GameObject>(rekruitablesPrefabs[index], hit.position, Quaternion.identity);
            Unit unitInstance = unitGameObject.GetComponent<Unit>();
            unitInstance.faction = building.faction;

            AICommand moveToWaypoint = new AICommand(AICommand.CommandType.MoveTo, waypointPos);
            unitInstance.AddCommand(moveToWaypoint, true);
            return unitInstance;
        }
        else
        {
            throw new Exception("No space for unit found");
        }
    }
}
