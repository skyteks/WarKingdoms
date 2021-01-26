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
#if UNITY_EDITOR
    public List<Renderer> renderers { get; private set; }
#endif

    public int resourceGold = 0;
    public int resourceWood = 0;

    void OnEnable()
    {
        units = new List<Unit>();
        buildings = new List<Building>();
        renderers = new List<Renderer>();
    }

    void OnDisable()
    {
        units = new List<Unit>();
        buildings = new List<Building>();
        renderers = new List<Renderer>();
    }

    public static bool IsAlliedWith(FactionTemplate faction1, FactionTemplate faction2)
    {
        if (faction1 != faction2 && (faction1 == null || faction2 == null || faction1.allianceId == 0 || faction2.allianceId == 0))
        {
            return false;
        }
        return faction1.allianceId == faction2.allianceId;
    }

#if UNITY_EDITOR
    [ContextMenu("Start Teamcolor Updating Coroutine")]
    private void StartUpdatingCoroutine()
    {
        if (Application.isPlaying)
        {
            GameManager.Instance.StartCoroutine(UpdatingCoroutine());
        }
    }

    private IEnumerator UpdatingCoroutine()
    {
        for (; ; )
        {
            yield return null;
            UpdateTeamcolorMaterials();
        }
    }

    [ContextMenu("Update Teamcolor Materials")]
    private void UpdateTeamcolorMaterials()
    {
        foreach (Renderer render in renderers)
        {
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            render.GetPropertyBlock(materialPropertyBlock, render.materials.Length - 1);
            materialPropertyBlock.SetColor("_TeamColor", color);
            render.SetPropertyBlock(materialPropertyBlock);
        }
    }
#endif

}
