using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores unit stats as asset file
/// </summary>
[CreateAssetMenu(fileName = "new Unit Template", menuName = "RTS/Unit Template", order = 1)]
public class UnitTemplate : ScriptableObject
{
    public UnitTemplate original { get; private set; }

    [Preview]
    public Sprite icon;

    public FactionTemplate.Race race;

    [Tooltip("Determines the damage the Unit can take before it dies")]
    public int health = 100;

    public float healthRegen = 0.5f;

    [Tooltip("Damage dealt each attack")]
    public Vector2Int damage = new Vector2Int(9, 11);

    [Tooltip("The attack rate. The higher, the faster the Unit is in attacking. 1 second/attackSpeed = time it takes for a single attack")]
    public float attackSpeed = 1f;

    [Tooltip("When it has reached this distance from its target, the Unit stops and attacks it")]
    public float engageDistance = 1f;

    [Tooltip("When guarding, if any enemy enters this range it will be attacked")]
    public float guardDistance = 5f;

    [Space]

    public int mana = 0;

    public float manaRegen = 0;

    public UnitTemplate Clone()
    {
        UnitTemplate clone = Instantiate<UnitTemplate>(this);
        clone.original = this;
        return clone;
    }
}
