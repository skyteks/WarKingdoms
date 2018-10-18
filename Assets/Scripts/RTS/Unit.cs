using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.Events;

public class Unit : MonoBehaviour
{
    public enum UnitState
    {
        Idle,
        Guarding,
        Attacking,
        MovingToTarget,
        MovingToSpotIdle,
        MovingToSpotGuard,
        Dead,
    }

    public static Dictionary<UnitTemplate.Faction, List<Unit>> globalUnits;

    static Unit()
    {
        globalUnits = new Dictionary<UnitTemplate.Faction, List<Unit>>();
        var factions = System.Enum.GetValues(typeof(UnitTemplate.Faction)).Cast<UnitTemplate.Faction>();
        foreach (var faction in factions)
        {
            globalUnits.Add(faction, new List<Unit>());
        }
    }

    public UnitState state = UnitState.Idle;
    [Preview]
    public UnitTemplate template;
    public float visionFadeTime = 1f;

    //references
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private MeshRenderer selectionCircle, miniMapCircle, visionCircle;

    //private bool isSelected; //is the Unit currently selected by the Player
    private Unit targetOfAttack;
    private Unit[] hostiles;
    private float lastGuardCheckTime, guardCheckInterval = 1f;
    private bool agentReady = false;

    public UnityAction<Unit> OnDie;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        selectionCircle = transform.Find("SelectionCircle").GetComponent<MeshRenderer>();
        miniMapCircle = transform.Find("MiniMapCircle").GetComponent<MeshRenderer>();
        visionCircle = transform.Find("FieldOfView").GetComponent<MeshRenderer>();

        //Randomization of NavMeshAgent speed. More fun!
        //float rndmFactor = navMeshAgent.speed * .15f;
        //navMeshAgent.speed += Random.Range(-rndmFactor, rndmFactor);
    }

    private void Start()
    {
        globalUnits[template.faction].Add(this);

        template = template.Clone(); //we copy the template otherwise it's going to overwrite the original asset!

        //Set some defaults, including the default state
        SetSelected(false);
        Idle();

        //visionCircle.material.color = visionCircle.material.color.ToWithA(0f);
        //if (template.faction == GameManager.Instance.faction)
        //{
        //    StartCoroutine(VisionFade(visionFadeTime, false));
        //}
        //else
        //{
        //    visionCircle.enabled = false;
        //}
    }

    void Update()
    {
        //Little hack to give time to the NavMesh agent to set its destination.
        //without this, the Unit would switch its state before the NavMeshAgent can kick off, leading to unpredictable results
        if (!agentReady)
        {
            agentReady = true;
            return;
        }

        switch (state)
        {
            case UnitState.MovingToSpotIdle:
                if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance + .1f)
                {
                    Idle();
                }
                break;

            case UnitState.MovingToSpotGuard:
                if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance + .1f)
                {
                    Guard();
                }
                break;

            case UnitState.MovingToTarget:
                //check if target has been killed by somebody else
                if (IsDeadOrNull(targetOfAttack))
                {
                    Guard();
                }
                else
                {
                    //Check for distance from target
                    if (navMeshAgent.remainingDistance < template.engageDistance)
                    {
                        navMeshAgent.velocity = Vector3.zero;
                        StartAttacking();
                    }
                    else
                    {
                        navMeshAgent.SetDestination(targetOfAttack.transform.position); //update target position in case it's moving
                    }
                }
                break;

            case UnitState.Guarding:
                if (Time.time > lastGuardCheckTime + guardCheckInterval)
                {
                    lastGuardCheckTime = Time.time;
                    Unit t = GetNearestHostileUnit();
                    if (t != null)
                    {
                        MoveToAttack(t);
                    }
                }
                break;

            case UnitState.Attacking:
                //check if target has been killed by somebody else
                if (IsDeadOrNull(targetOfAttack))
                {
                    Guard();
                }
                else
                {
                    //look towards the target
                    Vector3 desiredForward = (targetOfAttack.transform.position - transform.position).normalized;
                    transform.forward = Vector3.Lerp(transform.forward, desiredForward, Time.deltaTime * 10f);
                }
                break;
            case UnitState.Dead:
                if (template.health != 0)
                {
                    Die();
                }
                return;
        }

        float navMeshAgentSpeed = navMeshAgent.velocity.magnitude;
        if (animator != null) animator.SetFloat("Speed", navMeshAgentSpeed * .05f);

        //float scalingCorrection = template.guardDistance * 2f * 1.05f;
        //if (visionCircle.transform.localScale.x != template.guardDistance * scalingCorrection)
        //{
        //    visionCircle.transform.localScale = Vector3.one * scalingCorrection;
        //}
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (navMeshAgent != null
            && navMeshAgent.isOnNavMesh
            && navMeshAgent.hasPath)
        {
            //UnityEditor.Handles.color = Random.onUnitSphere.ToVector4(1f).ToColor();
            UnityEditor.Handles.DrawLine(transform.position, navMeshAgent.destination);
        }

        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, template.engageDistance);
        UnityEditor.Handles.color = Color.gray;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, template.guardDistance);
    }
#endif

    public void ExecuteCommand(AICommand c)
    {
        if (state == UnitState.Dead)
        {
            //already dead
            return;
        }

        switch (c.commandType)
        {
            case AICommand.CommandType.GoToAndIdle:
                GoToAndIdle(c.destination);
                break;

            case AICommand.CommandType.GoToAndGuard:
                GoToAndGuard(c.destination);
                break;

            case AICommand.CommandType.Stop:
                Idle();
                break;

            case AICommand.CommandType.AttackTarget:
                MoveToAttack(c.target);
                break;

            case AICommand.CommandType.Die:
                Die();
                break;
        }
    }

    //move to a position and be idle
    private void GoToAndIdle(Vector3 location)
    {
        state = UnitState.MovingToSpotIdle;
        targetOfAttack = null;
        agentReady = false;

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(location);
    }

    //move to a position and be guarding
    private void GoToAndGuard(Vector3 location)
    {
        state = UnitState.MovingToSpotGuard;
        targetOfAttack = null;
        agentReady = false;

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(location);
    }

    //stop and stay Idle
    private void Idle()
    {
        state = UnitState.Idle;
        targetOfAttack = null;
        agentReady = false;

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
    }

    //stop but watch for enemies nearby
    public void Guard()
    {
        state = UnitState.Guarding;
        targetOfAttack = null;
        agentReady = false;

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
    }

    //move towards a target to attack it
    private void MoveToAttack(Unit target)
    {
        if (!IsDeadOrNull(target))
        {
            state = UnitState.MovingToTarget;
            targetOfAttack = target;
            agentReady = false;

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(target.transform.position);
        }
        else
        {
            //if the command is dealt by a Timeline, the target might be already dead
            Guard();
        }
    }

    //reached the target (within engageDistance), time to attack
    private void StartAttacking()
    {
        //somebody might have killed the target while this Unit was approaching it
        if (!IsDeadOrNull(targetOfAttack))
        {
            state = UnitState.Attacking;
            agentReady = false;
            navMeshAgent.isStopped = true;
            StartCoroutine(DealAttack());
        }
        else
        {
            Guard();
        }
    }

    //the single blows
    private IEnumerator DealAttack()
    {
        if (animator != null) animator.SetBool("DoAttack", true);
        while (!IsDeadOrNull(targetOfAttack))
        {
            //Check if the target moved away for some reason
            if (Vector3.Distance(targetOfAttack.transform.position, transform.position) > template.engageDistance)
            {
                if (animator != null) animator.SetBool("DoAttack", false);

                MoveToAttack(targetOfAttack);

                yield break;
            }

            yield return new WaitForSeconds(1f / template.attackSpeed);

            //check is performed after the wait, because somebody might have killed the target in the meantime
            if (IsDeadOrNull(targetOfAttack))
            {
                break;
            }

            if (targetOfAttack.template.faction == template.faction)
            {
                break;
            }

            if (state == UnitState.Dead)
            {
                yield break;
            }

            //Too far away check moved to before waittime

            targetOfAttack.SufferAttack(template.attackPower);
        }
        if (animator != null) animator.SetBool("DoAttack", false);

        //only move into Guard if the attack was interrupted (dead target, etc.)
        if (state == UnitState.Attacking)
        {
            Guard();
        }
    }

    //called by an attacker
    private void SufferAttack(int damage)
    {
        if (state == UnitState.Dead)
        {
            //already dead
            return;
        }

        template.health -= damage;

        if (template.health <= 0)
        {
            Die();
        }
    }

    //called in SufferAttack, but can also be from a Timeline clip
    [ContextMenu("Die")]
    private void Die()
    {
        template.health = 0;

        state = UnitState.Dead; //still makes sense to set it, because somebody might be interacting with this script before it is destroyed
        if (animator != null) animator.SetTrigger("DoDeath");

        //Remove itself from the selection Platoon
        GameManager.Instance.RemoveFromSelection(this);
        SetSelected(false);

        //Fire an event so any Platoon containing this Unit will be notified
        if (OnDie != null)
        {
            OnDie(this);
        }

        //To avoid the object participating in any Raycast or tag search
        gameObject.tag = "Untagged";
        gameObject.layer = 0;

        globalUnits[template.faction].Remove(this);

        //Remove unneeded Components
        ParticleSystem ps = visionCircle.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Emit(1);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(ps, ps.main.startLifetimeMultiplier);
        }
        //Destroy(sightCircle);
        StartCoroutine(VisionFade(visionFadeTime, true));
        Destroy(selectionCircle);
        Destroy(miniMapCircle);
        Destroy(navMeshAgent);
        Destroy(GetComponent<Collider>()); //will make it unselectable on click
        if (animator != null) Destroy(animator, 10f); //give it some time to complete the animation
    }

    private IEnumerator VisionFade(float fadeTime, bool fadeOut)
    {
        Color newColor = visionCircle.material.color;
        float deadline = Time.time + fadeTime;
        while (Time.time < deadline)
        {
            //newColor = sightCircle.material.color;
            newColor.a = newColor.a + Time.deltaTime * fadeTime * -fadeOut.ToSignFloat();
            visionCircle.material.color = newColor;
            yield return null;
        }
        if (fadeOut)
        {
            Destroy(visionCircle);
        }
    }

    private bool IsDeadOrNull(Unit unit)
    {
        return (unit == null || unit.state == UnitState.Dead);
    }

    private Unit GetNearestHostileUnit()
    {
        hostiles = FindObjectsOfType<Unit>().Where(unit => unit.template.faction != template.faction).ToArray();//GameObject.FindGameObjectsWithTag(template.GetOtherFaction().ToString()).Select(x => x.GetComponent<Unit>()).ToArray();

        Unit nearestEnemy = null;
        float nearestEnemyDistance = 1000f;
        for (int i = 0; i < hostiles.Count(); i++)
        {
            if (IsDeadOrNull(hostiles[i]))
            {
                continue;
            }

            float distanceFromHostile = Vector3.Distance(hostiles[i].transform.position, transform.position);
            if (distanceFromHostile <= template.guardDistance)
            {
                if (distanceFromHostile < nearestEnemyDistance)
                {
                    nearestEnemy = hostiles[i];
                    nearestEnemyDistance = distanceFromHostile;
                }
            }
        }

        return nearestEnemy;
    }

    public void SetSelected(bool selected)
    {
        //Set transparency dependent on selection
        Color newColor = (template.faction == GameManager.Instance.faction) ? Color.green : Color.red;
        miniMapCircle.material.color = newColor;
        newColor.a = (selected) ? 1f : .3f;
        selectionCircle.material.color = newColor;
    }
}