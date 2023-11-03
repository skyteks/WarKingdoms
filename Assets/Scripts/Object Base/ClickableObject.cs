﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ClickableObject : InteractableObject
{
#if UNITY_EDITOR
    [SerializeField]
    private bool drawViewDistance = false;
    [SerializeField]
    private bool drawEngageDistance = false;
    [Space]
#endif

    protected bool currentlySelected;

    public FactionTemplate faction;
    public UnitTemplate template;

    protected MeshRenderer miniMapCircle, visionCircle;
    protected Renderer[] modelRenderers;
    public FieldOfView fieldOfView;

    public UnityAction<ClickableObject> OnDeath;
    public UnityAction<ClickableObject> OnDisapearInFOW;

    protected override void Awake()
    {
        base.Awake();
        miniMapCircle = transform.Find("MiniMapCircle").GetComponent<MeshRenderer>();
        visionCircle = transform.Find("FieldOfView").GetComponent<MeshRenderer>();
        modelRenderers = modelHolder.GetComponentsInChildren<Renderer>();
        fieldOfView = transform.Find("FieldOfView").GetComponent<FieldOfView>();

        template = template.Clone(); //we copy the template otherwise it's going to overwrite the original asset!
    }

    protected virtual void Start()
    {
        if (visionCircle != null)
        {
            //visionCircle.material.color = visionCircle.material.color.ToWithA(0f);
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            visionCircle.GetPropertyBlock(materialPropertyBlock, 0);
            materialPropertyBlock.SetColor("_Color", materialPropertyBlock.GetColor("_Color").ToWithA(0f));
            visionCircle.SetPropertyBlock(materialPropertyBlock);
        }

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
        UpdateMaterialTeamColor();
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        if (fieldOfView == null)
        {
            fieldOfView = transform.Find("FieldOfView")?.GetComponent<FieldOfView>();
        }
        if (selectionCircle == null)
        {
            selectionCircle = transform.Find("SelectionCircle")?.GetComponent<MeshRenderer>();
        }

        if (template != null && attackable != null && !attackable.isDead && fieldOfView != null)
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

    protected void UpdateMaterialTeamColor()
    {
        Shader teamcolorShader = GameManager.Instance.tintShader;
        if (teamcolorShader == null)
        {
            throw new System.NullReferenceException();
        }
        UIManager uiManager = UIManager.Instance;

        foreach (Renderer render in modelRenderers)
        {
            for (int i = 0; i < render.sharedMaterials.Length; i++)
            {
                if (render.sharedMaterials[i].shader == teamcolorShader)
                {
                    Color tmpColor = uiManager.GetFactionColorForColorMode(faction, UIManager.ColorType.Shader);
                    FactionTemplate.ChangeTeamcolorOnRenderer(render, tmpColor, teamcolorShader);
                    break;
                }
            }
        }
    }

    protected void UpdateMinimapUI()
    {
        UIManager uiManager = UIManager.Instance;
        Color newColor = uiManager.GetFactionColorForColorMode(faction, UIManager.ColorType.UI);

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        miniMapCircle.GetPropertyBlock(materialPropertyBlock, 0);
        materialPropertyBlock.SetColor("_Color", newColor);
        miniMapCircle.SetPropertyBlock(materialPropertyBlock);
    }

    public float GetSelectionCircleSize()
    {
        return selectionCircle.transform.localScale.x;
    }

    public void SetSelected(bool selected)
    {
        UIManager uiManager = UIManager.Instance;

        currentlySelected = selected;
        Color tmpColor = uiManager.GetUIColorForColorMode(faction, currentlySelected);

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        selectionCircle.GetPropertyBlock(materialPropertyBlock, 0);
        materialPropertyBlock.SetColor("_Color", tmpColor);
        selectionCircle.SetPropertyBlock(materialPropertyBlock);
    }

    public override void Die()
    {
        template.health = 0;

        //Fire an event so any Platoon containing this Unit will be notified
        if (OnDeath != null)
        {
            OnDeath.Invoke(this);
        }

        //Remove unneeded Components
        selectionCircle.enabled = false;
        miniMapCircle.enabled = false;
        GetComponent<Collider>().enabled = false; //will make it unselectable on click
    }

    protected IEnumerator VisionFade(float fadeTime, bool fadeOut)
    {
        if (visionCircle == null)
        {
            yield break;
        }
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

    protected override IEnumerator DecayIntoGround()
    {
        yield return base.DecayIntoGround();
        Destroy(gameObject);
    }
}
