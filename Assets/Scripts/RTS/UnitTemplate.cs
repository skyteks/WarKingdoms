using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpecificUnit", menuName = "RTS Test/Unit Template", order = 1)]
public class UnitTemplate : ScriptableObject
{
    public Faction faction;

    [Preview]
    public Sprite icon;

    [Tooltip("Determines the damage the Unit can take before it dies")]
    public int health = 10;

    [Tooltip("Damage dealt each attack")]
    public int attackPower = 2;

    [Tooltip("The attack rate. The higher, the faster the Unit is in attacking. 1 second/attackSpeed = time it takes for a single attack")]
    public float attackSpeed = 1f;

    [Tooltip("When it has reached this distance from its target, the Unit stops and attacks it")]
    public float engageDistance = 1f;

    [Tooltip("When guarding, if any enemy enters this range it will be attacked")]
    public float guardDistance = 5f;

    public enum Faction
    {
        Faction1,
        Faction2,
        Faction3,
    }
}
