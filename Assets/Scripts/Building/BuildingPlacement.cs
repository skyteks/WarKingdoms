using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    public Material holoMaterial;
    public LayerMask placementLayerMask;
    public float buildingStartLifePercentage = 0.2f;
    public Vector3 buildingPlacementRotation = Vector3.up * 180f;

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
        if (InputManager.Instance.buildingPlacementInitiated && buildingPrefab != null)
        {
            if (buildingCursorHolo == null)
            {
                CreateHolo();
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 100f, placementLayerMask))
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
        InputManager.Instance.buildingPlacementInitiated = true;
    }

    private void PlaceBuilding()
    {
        GameObject buildingInstance = GameObject.Instantiate(buildingPrefab, buildingCursorHolo.position, buildingCursorHolo.rotation);
        Building building = buildingInstance.GetComponent<Building>();
        building.faction = GameManager.Instance.playerFaction;
        building.template.health = Mathf.RoundToInt(building.template.original.health * buildingStartLifePercentage);

        Destroy(buildingCursorHolo.gameObject);
        buildingPrefab = null;
        buildingCursorHolo = null;
        buildingCollider = null;
        placeable = false;

        InputManager.Instance.buildingPlacementInitiated = false;
    }

    private void CreateHolo()
    {
        buildingCursorHolo = new GameObject(string.Concat(buildingPrefab.name, " (Holo)")).transform;

        buildingCursorHolo.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(buildingPlacementRotation));
        buildingCollider = buildingPrefab.GetComponent<BoxCollider>();

        Transform buildingModelHolder = buildingPrefab.transform.Find("Model");
        Transform buildingHoloHolder = GameObject.Instantiate(buildingModelHolder.gameObject, buildingModelHolder.localPosition, Quaternion.Euler(buildingPlacementRotation) * buildingModelHolder.localRotation, buildingCursorHolo.transform).transform;

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
