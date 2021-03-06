using System;
using UnityEngine;

/// <summary>
/// Order for Units to move or attack somewhere
/// </summary>
[Serializable]
public struct AICommand
{
    public enum CommandTypes
    {
        MoveTo,
        AttackTarget,
        Stop,
        Guard,
        Die,
        AttackMoveTo, //TODO: implement follow behavior
        CustomActionAtPos,
        CustomActionAtObj,
    }

    public enum CustomActions
    {
        dropoffResources,
    }

    public CommandTypes commandType;

    public Vector3 destination;
    public InteractableObject target;
    public Vector3 origin;
    public CustomActions? customAction;

    public AICommand(CommandTypes ty, Vector3 v, CustomActions? a = null)
    {
        commandType = ty;
        destination = v;
        target = null;
        origin = Vector3.one * float.NaN;
        customAction = a;
    }

    public AICommand(CommandTypes ty, InteractableObject ta, CustomActions? a = null)
    {
        commandType = ty;
        destination = Vector3.one * float.NaN;
        target = ta;
        origin = Vector3.one * float.NaN;
        customAction = a;
    }

    public AICommand(CommandTypes ty)
    {
        commandType = ty;
        destination = Vector3.one * float.NaN;
        target = null;
        origin = Vector3.one * float.NaN;
        customAction = null;
    }
}