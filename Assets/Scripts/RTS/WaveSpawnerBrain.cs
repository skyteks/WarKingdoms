using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Building))]
[RequireComponent(typeof(Rekruiting))]
public class WaveSpawnerBrain : MonoBehaviour
{
    private Rekruiting rekruiting;
    private Building building;
    public Transform[] waypoints;
    private Vector3[] lanepoints;
    public bool justOneUnit;
    public float timeToStart = 5f;
    public float timeBetweenUnits = 1f;
    public float timeBetweenWaves = 30f;

    private int counter;
    private Coroutine coroutine;

    void Awake()
    {
        rekruiting = GetComponent<Rekruiting>();
        building = GetComponent<Building>();
    }

    void OnEnable()
    {
        coroutine = StartCoroutine(Waves());
    }

    void OnDisable()
    {
        StopCoroutine(coroutine);
    }

    void Update()
    {
        if (Building.IsDeadOrNull(building))
        {
            enabled = false;
        }
    }

    private void SetLanePoints()
    {
        lanepoints = new Vector3[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(waypoints[i].position, out hit, 2f, NavMesh.AllAreas))
            {
                lanepoints[i] = hit.position;
            }
            else if (NavMesh.SamplePosition(waypoints[i].position, out hit, 8f, NavMesh.AllAreas))
            {
                lanepoints[i] = hit.position;
            }
        }

    }

    private IEnumerator Waves()
    {
        yield return null;
        yield return null;
        SetLanePoints();

        bool second = false;
        yield return Yielders.Get(timeToStart);

        for (; ; )
        {
            SpawnAndSendUnit(0);
            yield return Yielders.Get(timeBetweenUnits);
            if (justOneUnit)
            {
                yield break;
            }
            SpawnAndSendUnit(0);
            yield return Yielders.Get(timeBetweenUnits);
            SpawnAndSendUnit(0);
            //yield break;
            yield return Yielders.Get(timeBetweenUnits);
            if (second)
            {
                SpawnAndSendUnit(2);
                yield return Yielders.Get(timeBetweenUnits);
            }
            SpawnAndSendUnit(1);
            yield return Yielders.Get(timeBetweenUnits);
            SpawnAndSendUnit(1);
            yield return Yielders.Get(timeBetweenUnits);
            SpawnAndSendUnit(1);

            yield return Yielders.Get(timeBetweenWaves);
            second = !second;
        }
    }

    private void SpawnAndSendUnit(int index)
    {
        Unit unitInstance = rekruiting.SpawnUnit(index);
        for (int i = 0; i < lanepoints.Length; i++)
        {
            AICommand moveToWaypoint = new AICommand(AICommand.CommandTypes.AttackMoveTo, lanepoints[i]);
            unitInstance.AddCommand(moveToWaypoint, i == 0);
        }

        unitInstance.gameObject.name += " " + unitInstance.faction.name + " " + counter;
        counter++;
    }
}
