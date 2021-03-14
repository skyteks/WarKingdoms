using System.Collections;
using System.Linq;
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
    protected ParticleSystem[] burnEffects;

    protected override void Awake()
    {
        base.Awake();
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        fieldOfView = GetComponentInChildren<FieldOfView>();
        burnEffects = transform.Find("BurnPoints")?.GetChildren().Select(child => child.GetComponent<ParticleSystem>()).ToArray();
    }

    protected override void Start()
    {
        faction.data.buildings.Add(this);

        UpdateMaterialTeamColor();

        //Set some defaults, including the default state
        SetSelected(false);

        base.Start();

        navMeshObstacle.shape = NavMeshObstacleShape.Capsule;
        navMeshObstacle.radius = sizeRadius * 1.15f;
        navMeshObstacle.size = (Vector3.one * navMeshObstacle.radius).ToWithY(navMeshObstacle.size.y);
    }

    public static new bool IsDeadOrNull(InteractableObject unit)
    {
        return unit == null || ((unit is Building) ? (unit as Building).state == BuildingStates.Dead : ClickableObject.IsDeadOrNull(unit));
    }

    public override void SetVisibility(bool visibility, bool force = false)
    {
        if (!force && visibility == visible)
        {
            return;
        }

        base.SetVisibility(visibility, force);

        if (visible)
        {
            UIManager.Instance.AddHealthbar(this);
        }
        else
        {
            if (OnDisapearInFOW != null)
            {
                OnDisapearInFOW.Invoke(this);
            }
        }
    }

    public override bool SufferAttack(int damage, ResourceCollector resourceCollector = null)
    {
        if (state == BuildingStates.Dead)
        {
            return false;
        }

        TriggerBurnEffects();
        return base.SufferAttack(damage);
    }

    protected void TriggerBurnEffects()
    {
        if (burnEffects == null || burnEffects.Length == 0)
        {
            return;
        }
        float healthPerc = template.health / (float)template.original.health;

        if (healthPerc <= 0f || state == BuildingStates.Dead)
        {
            foreach (var effect in burnEffects)
            {
                effect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
        else
        {
            bool[] shouldBurn = new bool[burnEffects.Length];
            switch (burnEffects.Length)
            {
                case 4:
                case 3:
                default:
                    shouldBurn[0] = healthPerc < 0.75f;
                    shouldBurn[1] = healthPerc < 0.5f;
                    for (int i = 2; i < burnEffects.Length; i++)
                    {
                        shouldBurn[i] = healthPerc < 0.25f;
                    }
                    break;
                case 2:
                    shouldBurn[0] = healthPerc < 0.66f;
                    shouldBurn[1] = healthPerc < 0.33f;
                    break;
                case 1:
                    shouldBurn[0] = healthPerc < 0.5f;
                    break;
                case 0:
                    return;
            }

            for (int i = 0; i < burnEffects.Length; i++)
            {
                if (shouldBurn[i] != burnEffects[i].isPlaying)
                {
                    if (shouldBurn[i])
                    {
                        burnEffects[i].Play(true);
                    }
                    else
                    {
                        burnEffects[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    }
                }
            }
        }
    }

    protected override void Die()
    {
        base.Die();

        state = BuildingStates.Dead; //still makes sense to set it, because somebody might be interacting with this script before it is destroyed

        SetSelected(false);

        TriggerBurnEffects();

        faction.data.buildings.Remove(this);

        navMeshObstacle.enabled = false;

        //Remove unneeded Components
        StartCoroutine(HideSeenThings(visionFadeTime));
        StartCoroutine(VisionFade(visionFadeTime, true));
        StartCoroutine(DecayIntoGround());
    }
}
