﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class RandomWalkingBrain : MonoBehaviour
{
    [MinMaxSlider(0.1f, 10f)]
    public Range intervalRanges = new Range(2f, 7f);

    private Unit unit;

    void Awake()
    {
        unit = GetComponent<Unit>();
    }

    void Start()
    {
        StartCoroutine(BrainLoop());
    }

    private IEnumerator BrainLoop()
    {
        float waitTime = 1f;
        for (; ; )
        {
            yield return new WaitForSeconds(waitTime);
            SendCommand();
            waitTime = Random.Range(intervalRanges.Min, intervalRanges.Max);
        }
    }

    private void SendCommand()
    {
        Vector3 move = Random.insideUnitCircle.ToVector3XZ() * unit.template.guardDistance * 4f;
        AICommand command = new AICommand(AICommand.CommandType.MoveToAndIdle, unit.transform.position + move);
        unit.ExecuteCommand(command);
    }
}
