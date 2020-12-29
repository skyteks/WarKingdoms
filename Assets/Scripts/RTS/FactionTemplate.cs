using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores faction stats
/// </summary>
[CreateAssetMenu(fileName = "new Faction Template", menuName = "RTS/Faction Template", order = 2)]
public class FactionTemplate : ScriptableObject
{
    public enum FactionColor
    {
        black,
        blue,
        brown,
        green,
        purple,
        red,
        orange,
        white,
    }

    public enum Race
    {
        Neutral,
        Human,
        Orc,
        WoodElf,
    }

    public FactionColor factionColorName = FactionColor.black;
    public Color color = Color.black;

    public byte allianceId;

    public List<Unit> units { get; private set; }
    public List<Building> buildings { get; private set; }

    public int resourceGold = 0;
    public int resourceWood = 0;

    void OnEnable()
    {
        units = new List<Unit>();
        buildings = new List<Building>();
    }

    void OnDisable()
    {
        units = new List<Unit>();
        buildings = new List<Building>();
    }

    public static bool IsAlliedWith(FactionTemplate faction1, FactionTemplate faction2)
    {
        if (faction1 != faction2 && (faction1 == null || faction2 == null || faction1.allianceId == 0 || faction2.allianceId == 0))
        {
            return false;
        }
        return faction1.allianceId == faction2.allianceId;
    }
}
