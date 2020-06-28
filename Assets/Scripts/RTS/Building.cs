using System.Collections;
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

    public new static bool IsDeadOrNull(ClickableObject unit)
    {
        return unit == null || ((unit is Building) ? (unit as Building).state == BuildingStates.Dead : ClickableObject.IsDeadOrNull(unit));
    }

    protected IEnumerator DecayIntoGround()
    {
        float startY = transform.position.y;
        float depth = 5f;
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

    //called by an attacker
    public override void SufferAttack(int damage)
    {
        if (state == BuildingStates.Dead)
        {
            return;
        }

        base.SufferAttack(damage);
    }

    protected override void Die()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            return;
        }
        base.Die();

        state = BuildingStates.Dead; //still makes sense to set it, because somebody might be interacting with this script before it is destroyed

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
        navMeshObstacle.enabled = false;
        StartCoroutine(DecayIntoGround());
    }
}
