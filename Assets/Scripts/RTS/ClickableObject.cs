using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public abstract class ClickableObject : MonoBehaviour
{
    protected static int layerDefaultVisible;
    protected static int layerDefaultHidden;
    protected static int layerMiniMapVisible;
    protected static int layerMiniMapHidden;

    public static List<ClickableObject> globalObjectsList;

    [Preview]
    public FactionTemplate faction;
    public bool visible;
    [Preview]
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
    }

    protected virtual void Start()
    {
        globalObjectsList.Add(this);

        template = template.Clone(); //we copy the template otherwise it's going to overwrite the original asset!

        visionCircle.material.color = visionCircle.material.color.ToWithA(0f);
        if (FactionTemplate.IsAlliedWith(faction, GameManager.Instance.playerFaction))
        {
            StartCoroutine(VisionFade(visionFadeTime, false));
            SetVisibility(true);
        }
        else
        {
            fieldOfView.enabled = false;
            visible = true;
            SetVisibility(false);
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
            selectionCircle = transform.Find("SelectionCircle").GetComponent<MeshRenderer>();
        }
        if (fieldOfView == null)
        {
            fieldOfView = transform.Find("FieldOfView").GetComponent<FieldOfView>();
        }

        if (!IsDeadOrNull(this))
        {
            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.DrawWireDisc(fieldOfView.transform.position, Vector3.up, template.engageDistance);
            UnityEditor.Handles.color = Color.gray;
            UnityEditor.Handles.DrawWireDisc(fieldOfView.transform.position, Vector3.up, template.guardDistance);

            UnityEditor.Handles.color = Color.blue;
        }
        else
        {
            UnityEditor.Handles.color = Color.red;
        }
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, sizeRadius);
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
        layerDefaultVisible = LayerMask.NameToLayer("Default");
        layerDefaultHidden = LayerMask.NameToLayer("Default Hidden");
        layerMiniMapVisible = LayerMask.NameToLayer("MiniMap Only");
        layerMiniMapHidden = LayerMask.NameToLayer("MiniMap Hidden");
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

    public virtual void SetVisibility(bool visibility)
    {
        visible = visibility;

        IEnumerable<GameObject> parts = GetComponentsInChildren<Transform>().Where(form =>
            form.gameObject.layer == layerDefaultVisible ||
            form.gameObject.layer == layerDefaultHidden ||
            form.gameObject.layer == layerMiniMapVisible ||
            form.gameObject.layer == layerMiniMapHidden
        ).Select(form => form.gameObject);

        foreach (GameObject part in parts)
        {
            if (part.layer == layerDefaultVisible || part.layer == layerDefaultHidden)
            {
                if (visibility)
                {
                    part.layer = layerDefaultVisible;
                }
                else
                {
                    part.layer = layerDefaultHidden;
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
    [ContextMenu("Die")]
    protected virtual void Die()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            return;
        }
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
            Destroy(visionCircle);
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
