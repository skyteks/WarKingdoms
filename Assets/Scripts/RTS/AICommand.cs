using System;
using UnityEngine;

[Serializable]
public class AICommand
{
    public enum CommandType
    {
        MoveTo,
        AttackMoveTo,
        AttackTarget, //attacks a specific target, then becomes Guarding
        Stop,
        Guard,
        Die,
        //Flee,
    }

    public CommandType commandType;

    public Vector3? destination;
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
        destination = null;
        target = ta;
    }

    public AICommand(CommandType ty)
    {
        commandType = ty;
        destination = null;
        target = null;
    }
}