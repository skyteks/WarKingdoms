using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    public Material holoMaterial;

    private GameObject buildingPrefab;
    private Transform buildingCursorHolo;
    private BoxCollider buildingCollider;
    private bool placeable;

    void Start()
    {
        holoMaterial = Material.Instantiate(holoMaterial);
    }

    void Update()
    {
        if (buildingPrefab != null)
        {
            if (buildingCursorHolo == null)
            {
                CreateHolo();
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 100f))
            {
                buildingCursorHolo.position = hitInfo.point;
            }

            placeable = !Physics.CheckBox(buildingCursorHolo.position + buildingCollider.center + Vector3.up * 0.0001f, buildingCollider.size * 0.5f, buildingCursorHolo.rotation);
            holoMaterial.SetColor("_EmissionColor", placeable ? Color.green : Color.red);

            if (Input.GetMouseButtonDown(0) && placeable)
            {
                PlaceBuilding();
            }
        }
    }

    public void SetBuildingToPlace(GameObject buildingToPlacePrefab)
    {
        buildingPrefab = buildingToPlacePrefab;
    }

    private void PlaceBuilding()
    {
        GameObject buildingInstance = GameObject.Instantiate(buildingPrefab, buildingCursorHolo.position, buildingCursorHolo.rotation);
        Building building = buildingInstance.GetComponent<Building>();
        building.faction = GameManager.Instance.playerFaction;

        Destroy(buildingCursorHolo.gameObject);
        buildingPrefab = null;
        buildingCursorHolo = null;
        buildingCollider = null;
        placeable = false;
    }

    private void CreateHolo()
    {
        buildingCursorHolo = new GameObject(string.Concat(buildingPrefab.name, " (Holo)")).transform;
        buildingCursorHolo.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        buildingCollider = buildingPrefab.GetComponent<BoxCollider>();

        Transform buildingModelHolder = buildingPrefab.transform.Find("Model");
        Transform buildingHoloHolder = GameObject.Instantiate(buildingModelHolder.gameObject, buildingModelHolder.localPosition, buildingModelHolder.localRotation, buildingCursorHolo.transform).transform;

        MeshRenderer[] renderers = buildingHoloHolder.GetComponentsInChildren<MeshRenderer>(false);
        foreach (var render in renderers)
        {
            render.sharedMaterial = holoMaterial;
            render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            render.receiveShadows = false;
        }

        buildingCursorHolo.gameObject.SetLayerRecursivly(LayerMask.NameToLayer("TransparentFX"));
    }
}
