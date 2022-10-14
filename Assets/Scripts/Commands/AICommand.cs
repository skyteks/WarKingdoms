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
        CustomActionAtPos,
        CustomActionAtObj,
        AttackMoveTo, //TODO: implement follow behavior
    }

    public enum CustomActions
    {
        collectResources,
        dropoffResources,
    }

    public CommandTypes commandType;

    public Vector3 destination;
    public InteractableObject target;
    public Vector3 origin;
    public CustomActions? customAction;

    public AICommand(CommandTypes type, Vector3 position, CustomActions? action = null)
    {
        commandType = type;
        destination = position;
        target = null;
        origin = Vector3.one * float.NaN;
        customAction = action;
    }

    public AICommand(CommandTypes type, InteractableObject targetObject, CustomActions? action = null)
    {
        commandType = type;
        destination = Vector3.one * float.NaN;
        target = targetObject;
        origin = Vector3.one * float.NaN;
        customAction = action;
    }

    public AICommand(CommandTypes type)
    {
        commandType = type;
        destination = Vector3.one * float.NaN;
        target = null;
        origin = Vector3.one * float.NaN;
        customAction = null;
    }
}