using System;
using UnityEngine;

[Serializable]
public class AICommand
{
    public enum CommandType
    {
        MoveToAndIdle,
        AttackMoveToAndGuard,
        AttackTarget, //attacks a specific target, then becomes Guarding
        Stop,
        Die,
        //Flee,
    }

    public CommandType commandType;

    public Vector3 destination;
    public Unit target;

    public AICommand(CommandType ty, Vector3 v)
    {
        commandType = ty;
        destination = v;
        target = null;
    }

    public AICommand(CommandType ty, Unit ta)
    {
        commandType = ty;
        destination = Vector3.one * float.NaN;
        target = ta;
    }

    public AICommand(CommandType ty)
    {
        commandType = ty;
        destination = Vector3.one * float.NaN;
        target = null;
    }
}