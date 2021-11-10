using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingHealthSim : MonoBehaviour
{
    public List<Building> buildings;

    [Range(0f, 1f)]
    public float healthPercentagePerSecond;

    [ContextMenu("Start")]
    public void StartDamaging()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        StartCoroutine(Coroutine());
    }

    private IEnumerator Coroutine()
    {
        Building lastBuilding = buildings[buildings.Count - 1];
        while (!lastBuilding.GetComponent<Healthpoints>().isDead)
        {
            int percentage = Mathf.RoundToInt(lastBuilding.template.original.health * healthPercentagePerSecond);
            foreach (Building building in buildings)
            {
                building.GetComponent<Healthpoints>().SufferAttack(1, gameObject);
            }
            yield return Yielders.Get(healthPercentagePerSecond);
        }
    }
}
