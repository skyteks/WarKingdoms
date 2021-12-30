using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Stores faction stats
/// </summary>
[CreateAssetMenu(fileName = "new Faction Template", menuName = "RTS/Faction Template", order = 2)]
public class FactionTemplate : ScriptableObject
{
    [System.Flags]
    public enum PlayerId : int
    {
        None,
        _01 = (1 << 0),
        _02 = (1 << 1),
        _03 = (1 << 2),
        _04 = (1 << 3),
        _05 = (1 << 4),
        _06 = (1 << 5),
        _07 = (1 << 6),
        _08 = (1 << 7),
    }

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
        cyan,
    }

    public enum Race
    {
        Neutral,
        Human,
        Orc,
        WoodElf,
        Undead,
    }

    [System.Serializable]
    public class FactionInfo
    {
        public PlayerId playerId;
        public byte allianceId = 0;

        public List<Unit> units { get; private set; }
        public List<Building> buildings { get; private set; }

        public List<Renderer> renderersTeamcolor { get; private set; }

        public int resourceGold { get; set; }
        public int resourceWood { get; set; }

        public FactionInfo()
        {
            ResetLists();
        }

        public void ResetLists()
        {
            units = new List<Unit>();
            buildings = new List<Building>();
            renderersTeamcolor = new List<Renderer>();
        }
    }

    public FactionColor factionColorName = FactionColor.black;

    public Color colorShader = Color.black;
    public Color colorUI = Color.black;

    public FactionInfo data;

    void OnEnable()
    {
        data.ResetLists();
    }

    public void SetTeamColorToRenderers()
    {
        Shader teamcolorShader = GameManager.Instance.tintShader;
        if (teamcolorShader == null)
        {
            throw new System.NullReferenceException();
        }
        UIManager uiManager = UIManager.Instance;
        Color tmpColor = uiManager.GetFactionColorForColorMode(this, UIManager.ColorType.Shader);

        foreach (var render in data.renderersTeamcolor)
        {
            ChangeTeamcolorOnRenderer(render, tmpColor, teamcolorShader);
        }
    }

    public void AddRendererForTeamColorChange(Renderer render)
    {
        data.renderersTeamcolor.Add(render);

        Shader teamcolorShader = GameManager.Instance.tintShader;
        if (teamcolorShader == null)
        {
            throw new System.NullReferenceException();
        }
        UIManager uiManager = UIManager.Instance;
        Color tmpColor = uiManager.GetFactionColorForColorMode(this, UIManager.ColorType.Shader);

        ChangeTeamcolorOnRenderer(render, tmpColor, teamcolorShader);
    }

    public static void ChangeTeamcolorOnRenderer(Renderer render, Color color, Shader tintShader)
    {
        for (int i = 0; i < render.sharedMaterials.Length; i++)
        {
            if (render.sharedMaterials[i].shader == tintShader)
            {
                MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(materialPropertyBlock, i);
                materialPropertyBlock.SetColor("_TintRColor", color);
                render.SetPropertyBlock(materialPropertyBlock);
            }
        }
    }

    public static bool IsAlliedWith(FactionTemplate faction1, FactionTemplate faction2)
    {
        return faction1 != faction2 && (faction1 == null || faction2 == null || faction1.data.allianceId == 0 || faction2.data.allianceId == 0) ? false : faction1.data.allianceId == faction2.data.allianceId;
    }

    public Building GetClosestBuildingWithResourceDropoff(Vector3 position, ResourceSource.ResourceType resourceType)
    {
        Building[] dropoffBuildings = data.buildings.Where(building => building.GetComponent<ResourceDropoff>() != null && building.GetComponent<ResourceDropoff>().dropoffTypes.Contains(resourceType)).ToArray();
        Building closest = null;
        float distanceToClosestSqr = float.PositiveInfinity;
        foreach (var building in dropoffBuildings)
        {
            float distanceSqr = (building.transform.position - position).sqrMagnitude;
            if (distanceSqr < distanceToClosestSqr)
            {
                distanceToClosestSqr = distanceSqr;
                closest = building;
            }
        }
        return closest;
    }
}
