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

    [System.Serializable]
    public class FactionInfo
    {
        public byte allianceId = 0;

        public List<Unit> units { get; private set; }
        public List<Building> buildings { get; private set; }

        public List<Renderer> renderersTeamcolor;

        public int resourceGold { get; set; }
        public int resourceWood { get; set; }

        public FactionInfo()
        {
            units = new List<Unit>();
            buildings = new List<Building>();

            renderersTeamcolor = new List<Renderer>();
        }
    }

    public FactionColor factionColorName = FactionColor.black;
    public Color color = Color.black;

    public FactionInfo data;

    void OnEnable()
    {
        data = new FactionInfo();
    }

    void OnDisable()
    {
        data = null;
    }

    public Color GetColorForColorMode()
    {
        GameManager gameManager = GameManager.Instance;
        UIManager uiManager = UIManager.Instance;


        Color newColor = Color.clear;
        switch (uiManager.minimapColoringMode)
        {
            case UIManager.MinimapColoringModes.FriendFoe:
                if (this == gameManager.playerFaction)
                {
                    newColor = Color.green;
                }
                else if (IsAlliedWith(this, gameManager.playerFaction))
                {
                    newColor = Color.yellow;
                }
                else
                {
                    newColor = Color.red;
                }
                break;
            case UIManager.MinimapColoringModes.Teamcolor:
                newColor = color;
                break;
        }
        return newColor;
    }

    public void SetTeamColorToRenderers()
    {
        Shader teamcolorShader = GameManager.Instance.teamcolorShader;
        Color tmpColor = GetColorForColorMode();

        foreach (var render in data.renderersTeamcolor)
        {
            ChangeTeamcolorOnRenderer(render, tmpColor, teamcolorShader);
        }
    }

    public void AddRendererForTeamColorChange(Renderer render)
    {
        data.renderersTeamcolor.Add(render);

        Shader teamcolorShader = GameManager.Instance.teamcolorShader;
        Color tmpColor = GetColorForColorMode();

        ChangeTeamcolorOnRenderer(render, tmpColor, teamcolorShader);
    }

    public static void ChangeTeamcolorOnRenderer(Renderer render, Color color, Shader teamcolorShader)
    {
        for (int i = 0; i < render.sharedMaterials.Length; i++)
        {
            if (render.sharedMaterials[i].shader == teamcolorShader)
            {
                MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(materialPropertyBlock, i);
                materialPropertyBlock.SetColor("_TeamColor", color);
                render.SetPropertyBlock(materialPropertyBlock);
            }
        }
    }

    public static bool IsAlliedWith(FactionTemplate faction1, FactionTemplate faction2)
    {
        return faction1 != faction2 && (faction1 == null || faction2 == null || faction1.data.allianceId == 0 || faction2.data.allianceId == 0) ? false : faction1.data.allianceId == faction2.data.allianceId;
    }
}
