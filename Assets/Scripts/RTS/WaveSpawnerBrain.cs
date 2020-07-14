using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Building))]
[RequireComponent(typeof(Rekruiting))]
public class WaveSpawnerBrain : MonoBehaviour
{
    private Rekruiting rekruiting;
    public Transform waypoint;

    public float timeToStart = 5f;
    public float timeBetweenUnits = 1f;
    public float timeBetweenWaves = 30f;

    private int counter;
    private Coroutine coroutine;

    void Awake()
    {
        rekruiting = GetComponent<Rekruiting>();
    }

    void OnEnable()
    {
        coroutine = StartCoroutine(Waves());
    }

    void OnDisable()
    {
        StopCoroutine(coroutine);
    }

    private IEnumerator Waves()
    {
        yield return null;
        yield return null;
        rekruiting.SetWaypoint(waypoint.position);

        bool second = false;
        yield return Yielders.Get(timeToStart);

        for (; ; )
        {
            SpawnAndSendUnit(0);
            yield return Yielders.Get(timeBetweenUnits);
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
        AICommand moveToWaypoint = new AICommand(AICommand.CommandType.AttackMoveTo, rekruiting.waypointPos);
        unitInstance.AddCommand(moveToWaypoint, true);

        unitInstance.gameObject.name += " " + unitInstance.faction.name + " " + counter;
        counter++;
    }
}
