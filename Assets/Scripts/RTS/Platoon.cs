using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Movement and selection handling for a list of Units
/// </summary>
public class Platoon : MonoBehaviour
{
    public enum FormationModes : int
    {
        Rectangle,
        HexGrid,
        Circle,
    }

#if UNITY_EDITOR
    private struct Location
    {
        public Vector3 position;
        public Quaternion rotation;

        public Location(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
    }
#endif

    public FormationModes formationMode;
    [Range(1f, 4f)]
    public float formationOffset = 3f;
    [HideInInspector]
    public List<Unit> units = new List<Unit>();

#if UNITY_EDITOR
    public Mesh debugMesh;
    private Location[] debugCommandLocations = new Location[0];
#endif

    private void Start()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].OnDeath += UnitDeadHandler;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        for (int i = 0; i < debugCommandLocations.Length && debugMesh != null; i++)
        {
            Gizmos.color = Color.green.ToWithA(0.3f);
            Gizmos.DrawMesh(debugMesh, debugCommandLocations[i].position, debugCommandLocations[i].rotation, new Vector3(1f, .1f, 1f));
        }
    }
#endif

    //Executes a command on all Units
    public void ExecuteCommand(AICommand command, bool followUpCommand)
    {
#if UNITY_EDITOR
        debugCommandLocations = new Location[0];
#endif
        if (command.destination.IsNaN())
        {
            for (int i = 0; i < units.Count; i++)
            {
                units[i].AddCommand(command, !followUpCommand);
            }
            return;
            //yield break;
        }
        //change the position for the command for each unit
        //so they move to a formation position rather than in the exact same place
        Vector3 destination = command.destination;
        Vector3 origin;
        if (units.Count == 1)
        {
            origin = units[0].transform.position;
        }
        else
        {
            Vector3[] positions = new Vector3[units.Count];
            for (int i = 0; i < units.Count; i++)
            {
                positions[i] = units[i].transform.position;
            }
            origin = positions.FindCentroid();
        }
        Quaternion rotation = Quaternion.LookRotation((destination - origin).normalized);
        Vector3[] offsets = GetFormationOffsets();
#if UNITY_EDITOR
        debugCommandLocations = new Location[offsets.Length];
        for (int i = 0; i < offsets.Length; i++)
        {
            debugCommandLocations[i] = new Location(destination + rotation * offsets[i], rotation);
        }
#endif

        List<Unit> sortedUnits = units.OrderBy(unit => Vector3.Distance(unit.transform.position, origin)).ToList();

        List<Vector3> remainingOffsets = offsets.ToList();

        for (int i = 0; i < sortedUnits.Count; i++)
        {
            Vector3 nextOffset = remainingOffsets.OrderBy(offset => Vector3.Distance(sortedUnits[i].transform.position, origin + rotation * offset)).First();
            remainingOffsets.Remove(nextOffset);
            command.destination = destination + rotation * nextOffset;
            sortedUnits[i].AddCommand(command, !followUpCommand);
            //yield return null;
        }
    }

    public bool IncludesUnit(Unit unitToCheck)
    {
        return units.Contains(unitToCheck);
    }

    public void AddUnit(Unit unitToAdd)
    {
#if UNITY_EDITOR
        debugCommandLocations = new Location[0];
#endif
        unitToAdd.OnDeath += UnitDeadHandler;
        units.Add(unitToAdd);

        unitToAdd.SetCombatReady(true);
    }

    //Removes an Unit from the Platoon and returns if the operation was successful
    public bool RemoveUnit(Unit unitToRemove)
    {
#if UNITY_EDITOR
        debugCommandLocations = new Location[0];
#endif
        bool isThere = units.Contains(unitToRemove);

        if (isThere)
        {
            units.Remove(unitToRemove);
            unitToRemove.OnDeath -= UnitDeadHandler;

            unitToRemove.SetCombatReady(false);
        }

        return isThere;
    }

    public void Clear()
    {
#if UNITY_EDITOR
        debugCommandLocations = new Location[0];
#endif
        foreach (Unit unitToRemove in units)
        {
            unitToRemove.OnDeath -= UnitDeadHandler;
            unitToRemove.SetCombatReady(false);
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
                    int index = 1;
                    int remaining = count - 1;
                    float currentOffset = formationOffset;
                    for (int j = 0; remaining > 0; j++)
                    {
                        currentOffset = formationOffset * (j + 1);
                        float circumfence = 2f * Mathf.PI * currentOffset;
                        int spaces = Mathf.FloorToInt(circumfence / formationOffset);
                        float angle = 360f / (spaces > remaining ? remaining : spaces);

                        for (int i = 0; i < spaces && index < count; i++)
                        {
                            float rotationOffset = angle * (j % 2f) * 0.5f;
                            offsets[index] = new Vector3(
                                (angle * i + rotationOffset).Sin() * currentOffset,
                                0f,
                                (angle * i + rotationOffset).Cos() * currentOffset
                            );
                            remaining--;
                            index++;
                        }
                    }
                }
                break;
            case FormationModes.Rectangle:
                {
                    float sqrt = Mathf.Sqrt(count);
                    int i = 0;
                    //if (sqrt % 1f == 0f)
                    //{
                    //    float halfSquareDiameter = (sqrt - 1f) * 0.5f * formationOffset;
                    //    //for (int y = 0; y < sqrt && i < count; y++)
                    //    for (int y = Mathf.FloorToInt(sqrt) - 1; y >= 0 && i < count; y--)
                    //    {
                    //        for (int x = 0; x < sqrt && i < count; x++, i++)
                    //        {
                    //            offsets[i] = new Vector3(
                    //                x * formationOffset - halfSquareDiameter,
                    //                0f,
                    //                y * formationOffset - halfSquareDiameter
                    //            );
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    float currentOffset = formationOffset;

                    int h = Mathf.FloorToInt(sqrt);
                    float tmp = count / (float)h;
                    int w = Mathf.CeilToInt(tmp);

                    for (int y = h - 1; y >= 0 && i < count; y--)
                    {
                        int remaining = (count - i);
                        float x = 0;
                        if (remaining < w && formationMode == FormationModes.Rectangle)
                        {
                            //int missing = w - remaining;
                            //x += missing / 2f;
                            currentOffset = formationOffset * (w - 1f) / (remaining - 1);
                        }
                        for (; x < w && i < count; x++, i++)
                        {
                            offsets[i] = new Vector3(
                                x * currentOffset - (w - 1f) * 0.5f * formationOffset,
                                0f,
                                y * currentOffset - (h - 1f) * 0.5f * formationOffset
                            );
                        }
                    }
                    //}
                }
                if (formationMode == FormationModes.HexGrid)
                {
                    caseCounter++;
                    goto case FormationModes.HexGrid;
                }
                break;
            case FormationModes.HexGrid:
                {
                    if (caseCounter == 0)
                    {
                        goto case FormationModes.Rectangle;
                    }

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
                && units[i].state != Unit.UnitStates.Dead)
            {
                allDead = false;
                break;
            }
        }

        return allDead;
    }

    //Fired when a unit belonging to this Platoon dies
    private void UnitDeadHandler(ClickableObject whoDied)
    {
        if (whoDied is Unit)
        {
            RemoveUnit(whoDied as Unit); //will also remove the handler
        }
    }
}