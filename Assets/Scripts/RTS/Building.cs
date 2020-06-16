using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class Building : ClickableObject
{

    public enum BuildingStates
    {
        Idleing,
        Attacking,
        Dead,
    }


    public BuildingStates state = BuildingStates.Idleing;

    //references
    protected NavMeshObstacle navMeshObstacle;

    protected override void Awake()
    {
        base.Awake();
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        fieldOfView = GetComponentInChildren<FieldOfView>();
    }

    protected override void Start()
    {
        faction.buildings.Add(this);

        SetColorMaterial();

        //Set some defaults, including the default state
        SetSelected(false);

        base.Start();
    }

    public override bool IsDeadOrNull(ClickableObject unit)
    {
        return unit == null || (unit as Building).state == BuildingStates.Dead;
    }

    protected IEnumerator DecayIntoGround()
    {
        yield return Yielders.Get(5f);
        float startY = transform.position.y;
        float depth = 2f;
        while (transform.position.y > startY - depth)
        {
            transform.position += Vector3.down * Time.deltaTime * 0.1f;
            yield return null;
        }
        Destroy(gameObject);
    }

    public override void SetVisibility(bool visibility)
    {
        if (visibility)
        {
            if (visible)
            {
                return;
            }
        }
        else
        {
            if (!visible)
            {
                return;
            }
        }

        base.SetVisibility(visibility);
    }

    private void UpdateMinimapUI()
    {
        GameManager gameManager = GameManager.Instance;
        UIManager uiManager = UIManager.Instance;
        Material minimapCircleMaterial = miniMapCircle.material;
        switch (uiManager.minimapColoringMode)
        {
            case UIManager.MinimapColoringModes.FriendFoe:
                if (faction == gameManager.playerFaction)
                {
                    minimapCircleMaterial.color = Color.green;
                }
                else if (FactionTemplate.IsAlliedWith(faction, gameManager.playerFaction))
                {
                    minimapCircleMaterial.color = Color.yellow;
                }
                else
                {
                    minimapCircleMaterial.color = Color.red;
                }
                break;
            case UIManager.MinimapColoringModes.Teamcolor:
                minimapCircleMaterial.color = faction.color;
                break;
        }
    }

    //called by an attacker
    public override void SufferAttack(int damage)
    {
        if (state == BuildingStates.Dead)
        {
            return;
        }

        base.Die();
    }

    protected override void Die()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            return;
        }
        base.Die();

        SetSelected(false);

        //Fire an event so any Platoon containing this Unit will be notified
        if (OnDeath != null)
        {
            OnDeath.Invoke(this);
        }

        faction.buildings.Remove(this);

        //Remove unneeded Components
        StartCoroutine(HideSeenThings(visionFadeTime / 2f));
        StartCoroutine(VisionFade(visionFadeTime, true));
        //Destroy(selectionCircle);
        //Destroy(miniMapCircle);
        //Destroy(navMeshAgent);
        //Destroy(GetComponent<Collider>()); //will make it unselectable on click
        //if (animator != null)
        //{
        //    Destroy(animator, 10f); //give it some time to complete the animation
        //}
        navMeshObstacle.enabled = false;
        StartCoroutine(DecayIntoGround());
    }
}
