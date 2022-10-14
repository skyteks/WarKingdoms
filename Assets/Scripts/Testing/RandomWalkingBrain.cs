using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes a Unit walk randomly in a direction
/// </summary>
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
            waitTime = Random.Range(intervalRanges.min, intervalRanges.max);
        }
    }

    private void SendCommand()
    {
        Vector3 move = Random.insideUnitCircle.ToVector3XZ() * unit.template.guardDistance * 4f;
        AICommand command = new AICommand(AICommand.CommandTypes.MoveTo, unit.transform.position + move);
        unit.AddCommand(command, true);
    }
}
