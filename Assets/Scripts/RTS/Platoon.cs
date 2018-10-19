using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class Platoon : MonoBehaviour
{
    public enum FormationModes
    {
        SquareGrid,
        HexGrid,
        Circle,
    }

    public FormationModes formationMode;
    [Range(1f, 4f)]
    public float formationOffset = 3f;
    [HideInInspector]
    public List<Unit> units = new List<Unit>();

    private void Start()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].OnDie += UnitDeadHandler;
        }
    }

    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
            {
                Gizmos.color = new Color(.8f, .8f, 1f, 1f);
                Gizmos.DrawCube(units[i].transform.position, new Vector3(1f, .1f, 1f));
            }
        }
    }

    //Executes a command on all Units
    public void ExecuteCommand(AICommand command)
    {
        if (units.Count == 1)
        {
            units[0].ExecuteCommand(command);
            return;
            //yield break;
        }
        //change the position for the command for each unit
        //so they move to a formation position rather than in the exact same place
        Vector3 destination = command.destination;
        Vector3 origin = units.Select(unit => unit.transform.position).FindCentroid();
        Quaternion rotation = Quaternion.LookRotation((destination - origin).normalized);
        Vector3[] offsets = GetFormationOffsets();
        //for (int i = 0; i < offsets.Length; i++) offsets[i] = destination + rotation * offsets[i];

        List<Unit> sortedUnits = units.OrderBy(unit => Vector3.Distance(unit.transform.position, origin)).ToList();

        List<Vector3> remainingOffsets = offsets.ToList();

        for (int i = 0; i < sortedUnits.Count; i++)
        {
            Vector3 nextOffset = remainingOffsets.OrderBy(offset => Vector3.Distance(sortedUnits[i].transform.position, origin + rotation * offset)).First();
            remainingOffsets.Remove(nextOffset);
            command.destination = destination + rotation * nextOffset;
            sortedUnits[i].ExecuteCommand(command);
            //yield return null;
        }
    }

    public void AddUnit(Unit unitToAdd)
    {
        unitToAdd.OnDie += UnitDeadHandler;
        units.Add(unitToAdd);
    }

    //Adds an array of Units to the Platoon, and returns the new length
    public int AddUnits(IList<Unit> unitsToAdd)
    {
        for (int i = 0; i < unitsToAdd.Count; i++)
        {
            AddUnit(unitsToAdd[i]);
        }

        return units.Count;
    }

    //Removes an Unit from the Platoon and returns if the operation was successful
    public bool RemoveUnit(Unit unitToRemove)
    {
        bool isThere = units.Contains(unitToRemove);

        if (isThere)
        {
            units.Remove(unitToRemove);
            unitToRemove.OnDie -= UnitDeadHandler;
        }

        return isThere;
    }

    public void Clear()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].OnDie -= UnitDeadHandler;
        }
        units.Clear();
    }

    //Returns the current position of the units
    public Vector3[] GetCurrentPositions()
    {
        Vector3[] positions = new Vector3[units.Count];

        for (int i = 0; i < units.Count; i++)
        {
            positions[i] = units[i].transform.position;
        }

        return positions;
    }

    //Returns an array of positions to be used to send units into a circular formation
    public Vector3[] GetFormationOffsets()
    {
        int count = units.Count;
        Vector3[] offsets = new Vector3[count];

        int caseCounter = 0;
        switch (formationMode)
        {
            default:
            case FormationModes.Circle:
                {
                    offsets[0] = Vector3.zero;
                    int remaining = count - 1;

                    //float circumfence = 2f * Mathf.PI * formationOffset;
                    //int spaces = Mathf.FloorToInt(circumfence / formationOffset);
                    //float cirumfenceOffset = circumfence / (float)spaces;

                    float increment = 360f / remaining;
                    for (int i = 0; i < remaining; i++)
                    {
                        float angle = increment * i;
                        offsets[i] = new Vector3(
                            formationOffset * angle.Cos(),
                            0f,
                            formationOffset * angle.Sin()
                        );
                    }
                }
                break;
            case FormationModes.SquareGrid:
                {
                    float sqrt = Mathf.Sqrt(count);
                    int i = 0;
                    if (sqrt % 1f == 0f)
                    {
                        float halfSquareDiameter = (sqrt - 1f) * 0.5f * formationOffset;
                        //for (int y = 0; y < sqrt && i < count; y++)
                        for (int y = Mathf.FloorToInt(sqrt) - 1; y >= 0 && i < count; y--)
                        {
                            for (int x = 0; x < sqrt && i < count; x++, i++)
                            {
                                offsets[i] = new Vector3(
                                    x * formationOffset - halfSquareDiameter,
                                    0f,
                                    y * formationOffset - halfSquareDiameter
                                );
                                Debug.Log(offsets[i]);
                            }
                        }
                    }
                    else
                    {
                        int w = Mathf.RoundToInt(sqrt);
                        float rest = count / (float)w;
                        int h = Mathf.FloorToInt(rest) + (rest % 1f != 0f).ToInt();
                        for (int y = h - 1; y >= 0 && i < count; y--)
                        {
                            float x = 0;
                            int remaining = (count - i);
                            if (remaining < w && formationMode == FormationModes.SquareGrid)
                            {
                                int missing = w - remaining;
                                x += missing / 2f;
                            }
                            for (; x < w && i < count; x++, i++)
                            {
                                offsets[i] = new Vector3(
                                    x * formationOffset - (w - 1f) * 0.5f * formationOffset,
                                    0f,
                                    y * formationOffset - (h - 1f) * 0.5f * formationOffset
                                );
                            }
                        }
                    }
                }
                if (formationMode == FormationModes.HexGrid)
                {
                    caseCounter++;
                    goto case FormationModes.HexGrid;
                }
                break;
            case FormationModes.HexGrid:
                {
                    if (caseCounter == 0) goto case FormationModes.SquareGrid;

                    float halfFormationOffset = formationOffset / 2f;
                    float triangleHeightOffset = Mathf.Sqrt(Mathf.Pow(formationOffset, 2f) - Mathf.Pow(halfFormationOffset, 2f));

                    float lastY = offsets[0].z;
                    bool toggle = true;
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 offset = offsets[i];
                        if (offset.z != lastY)
                        {
                            toggle = !toggle;
                        }
                        lastY = offset.z;
                        offset.x += halfFormationOffset * toggle.ToSignFloat() * 0.5f;
                        offset.z = (offset.z / formationOffset) * triangleHeightOffset;
                        offsets[i] = offset;
                    }
                    break;
                }
        }

        return offsets;
    }

    //Forces the position of the units. Useful in Edit mode only (Play mode would use the NavMeshAgent)
    public void SetPositions(Vector3[] newPositions)
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].transform.position = newPositions[i];
        }
    }

    //Returns true if all the Units are dead
    public bool CheckIfAllDead()
    {
        bool allDead = true;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null
                && units[i].state != Unit.UnitState.Dead)
            {
                allDead = false;
                break;
            }
        }

        return allDead;
    }

    //Fired when a unit belonging to this Platoon dies
    private void UnitDeadHandler(Unit whoDied)
    {
        RemoveUnit(whoDied); //will also remove the handler
    }
}