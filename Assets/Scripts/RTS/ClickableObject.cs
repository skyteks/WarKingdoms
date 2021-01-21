using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public abstract class ClickableObject : MonoBehaviour
{
    protected static int layerObjectsVisible = -1;
    protected static int layerObjectsHidden = -1;
    protected static int layerMiniMapVisible = -1;
    protected static int layerMiniMapHidden = -1;

    public static List<ClickableObject> globalObjectsList;

#if UNITY_EDITOR
    [SerializeField]
    private bool drawViewDistance = false;
    [SerializeField]
    private bool drawEngageDistance = false;
    [Space]
#endif

    public FactionTemplate faction;
    public bool visible;
    public UnitTemplate template;
    public float visionFadeTime = 1f;

    protected Transform modelHolder;
    protected MeshRenderer selectionCircle, miniMapCircle, visionCircle;
    protected Renderer[] modelRenderers;
    public FieldOfView fieldOfView;

    public UnityAction<ClickableObject> OnDeath;
    public UnityAction<ClickableObject> OnDisapearInFOW;

    public float sizeRadius
    {
        get
        {
            return selectionCircle.transform.localScale.x * 0.5f;
        }
    }

    public float damageReductionMuliplier
    {
        get
        {
            if (template.armor >= 0)
            {
                return 100f / (100f + template.armor);
            }
            else
            {
                return 2f - (100f / (100f - template.armor));
            }
        }
    }

    static ClickableObject()
    {
        globalObjectsList = new List<ClickableObject>();
    }

    protected virtual void Awake()
    {
        selectionCircle = transform.Find("SelectionCircle").GetComponent<MeshRenderer>();
        miniMapCircle = transform.Find("MiniMapCircle").GetComponent<MeshRenderer>();
        visionCircle = transform.Find("FieldOfView").GetComponent<MeshRenderer>();
        modelHolder = transform.Find("Model");
        modelRenderers = modelHolder.GetComponentsInChildren<Renderer>(true);
        fieldOfView = transform.Find("FieldOfView").GetComponent<FieldOfView>();

        SetLayers();

        template = template.Clone(); //we copy the template otherwise it's going to overwrite the original asset!

        globalObjectsList.Add(this);
    }

    protected virtual void Start()
    {
        visionCircle.material.color = visionCircle.material.color.ToWithA(0f);
        if (FactionTemplate.IsAlliedWith(faction, GameManager.Instance.playerFaction))
        {
            StartCoroutine(VisionFade(visionFadeTime, false));
            SetVisibility(true, true);
        }
        else
        {
            SetVisibility(false, true);
        }
    }

    protected virtual void Update()
    {
        UpdateMinimapUI();
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (selectionCircle == null)
        {
            selectionCircle = transform.Find("SelectionCircle")?.GetComponent<MeshRenderer>();
        }
        if (fieldOfView == null)
        {
            fieldOfView = transform.Find("FieldOfView")?.GetComponent<FieldOfView>();
        }

        if (!IsDeadOrNull(this) && template != null && fieldOfView != null)
        {
            if (drawViewDistance)
            {
                UnityEditor.Handles.color = Color.gray;
                UnityEditor.Handles.DrawWireDisc(fieldOfView.transform.position, Vector3.up, template.guardDistance);
            }
            if (drawEngageDistance)
            {
                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.DrawWireDisc(fieldOfView.transform.position, Vector3.up, template.engageDistance);
            }

            UnityEditor.Handles.color = Color.blue;
        }
        else
        {
            UnityEditor.Handles.color = Color.red;
        }
        if (selectionCircle != null)
        {
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, sizeRadius);
        }
    }
#endif

    public static bool IsDeadOrNull(ClickableObject unit)
    {
        if (unit is Unit)
        {
            return Unit.IsDeadOrNull(unit);
        }
        else if (unit is Building)
        {
            return Building.IsDeadOrNull(unit);
        }
        else
        {
            return unit == null;
        }
    }

    protected static void SetLayers()
    {
        if (layerObjectsVisible == -1)
        {
            layerObjectsVisible = LayerMask.NameToLayer("Units");
            layerObjectsHidden = LayerMask.NameToLayer("Units Hidden");
            layerMiniMapVisible = LayerMask.NameToLayer("MiniMap Only");
            layerMiniMapHidden = LayerMask.NameToLayer("MiniMap Hidden");
        }
    }

    protected void SetColorMaterial()
    {
        foreach (Renderer render in modelRenderers)
        {
            //render.materials[render.materials.Length - 1].SetColor("_TeamColor", faction.color);

            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            render.GetPropertyBlock(materialPropertyBlock, render.materials.Length - 1);
            materialPropertyBlock.SetColor("_TeamColor", faction.color);
            render.SetPropertyBlock(materialPropertyBlock);
        }
    }

    protected void UpdateMinimapUI()
    {
        GameManager gameManager = GameManager.Instance;
        UIManager uiManager = UIManager.Instance;

        Color newColor = Color.clear;
        switch (uiManager.minimapColoringMode)
        {
            case UIManager.MinimapColoringModes.FriendFoe:
                if (faction == gameManager.playerFaction)
                {
                    newColor = Color.green;
                }
                else if (FactionTemplate.IsAlliedWith(faction, gameManager.playerFaction))
                {
                    newColor = Color.yellow;
                }
                else
                {
                    newColor = Color.red;
                }
                break;
            case UIManager.MinimapColoringModes.Teamcolor:
                newColor = faction.color;
                break;
        }

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        miniMapCircle.GetPropertyBlock(materialPropertyBlock, 0);
        materialPropertyBlock.SetColor("_Color", newColor);
        miniMapCircle.SetPropertyBlock(materialPropertyBlock);
    }

    public virtual void SetVisibility(bool visibility, bool force = false)
    {
        if (!force && visibility == visible)
        {
            return;
        }

        visible = visibility;

        GameObject[] parts = GetComponentsInChildren<Transform>().Where(form =>
            form.gameObject.layer == layerObjectsVisible ||
            form.gameObject.layer == layerObjectsHidden ||
            form.gameObject.layer == layerMiniMapVisible ||
            form.gameObject.layer == layerMiniMapHidden
        ).Select(form => form.gameObject).ToArray();

        foreach (GameObject part in parts)
        {
            if (part.layer == layerObjectsVisible || part.layer == layerObjectsHidden)
            {
                if (visibility)
                {
                    part.layer = layerObjectsVisible;
                }
                else
                {
                    part.layer = layerObjectsHidden;
                }
            }
            else
            {
                if (visibility)
                {
                    part.layer = layerMiniMapVisible;
                }
                else
                {
                    part.layer = layerMiniMapHidden;
                }
            }
        }
    }

    public float GetSelectionCircleSize()
    {
        return selectionCircle.transform.localScale.x;
    }

    public void SetSelected(bool selected)
    {
        //Set transparency dependent on selection
        GameManager gameManager = GameManager.Instance;
        Color newColor = Color.clear;
        if (faction == gameManager.playerFaction)
        {
            newColor = Color.green;
        }
        else if (FactionTemplate.IsAlliedWith(faction, gameManager.playerFaction))
        {
            newColor = Color.yellow;
        }
        else
        {
            newColor = Color.red;
        }
        newColor.a = selected ? 1f : 0.3f;

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        selectionCircle.GetPropertyBlock(materialPropertyBlock, 0);
        materialPropertyBlock.SetColor("_Color", newColor);
        selectionCircle.SetPropertyBlock(materialPropertyBlock);
    }

    //called in SufferAttack, but can also be from a Timeline clip
    protected virtual void Die()
    {
        template.health = 0;

        //Fire an event so any Platoon containing this Unit will be notified
        if (OnDeath != null)
        {
            OnDeath.Invoke(this);
        }

        //To avoid the object participating in any Raycast or tag search
        //gameObject.tag = "Untagged";
        gameObject.layer = 0;

        globalObjectsList.Remove(this);

        //Remove unneeded Components
        selectionCircle.enabled = false;
        miniMapCircle.enabled = false;
        GetComponent<Collider>().enabled = false; //will make it unselectable on click
    }

    public virtual void SufferAttack(int damage)
    {
        if (template.health <= 0)
        {
            return;
        }

        damage = Mathf.RoundToInt(damage * damageReductionMuliplier);
        template.health -= damage;

        if (template.health <= 0)
        {
            Die();
        }
    }

    protected IEnumerator VisionFade(float fadeTime, bool fadeOut)
    {
        Color newColor = visionCircle.material.color;
        float deadline = Time.time + fadeTime;
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        while (Time.time < deadline)
        {
            //newColor = sightCircle.material.color;
            newColor.a = newColor.a + Time.deltaTime * fadeTime * -fadeOut.ToSignFloat();

            visionCircle.GetPropertyBlock(materialPropertyBlock, 0);
            materialPropertyBlock.SetColor("_Color", newColor);
            visionCircle.SetPropertyBlock(materialPropertyBlock);
            yield return null;
        }
        if (fadeOut)
        {
            //Destroy(visionCircle);
            visionCircle.enabled = false;
        }
    }

    protected IEnumerator HideSeenThings(float fadeTime)
    {
        if (fadeTime != 0f)
        {
            yield return Yielders.Get(fadeTime);
        }

        float radius = template.guardDistance;
        template.guardDistance = 0f;
        fieldOfView.MarkTargetsVisibility();
        template.guardDistance = radius;
    }
}
