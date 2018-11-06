﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.Events;

/// <summary>
/// Unit semi-AI handles movement and stats
/// </summary>
public class Unit : MonoBehaviour
{
    public enum UnitStates
    {
        Idleing,
        Guarding,
        Attacking,
        MovingToTarget,
        MovingToSpot,
        AttackMovingToSpot,
        Dead,
    }

    public enum Factions
    {
        Neutral,
        Faction1,
        Faction2,
    }

    public static Dictionary<Factions, List<Unit>> globalUnits;
    private static int layerDefaultVisible;
    private static int layerDefaultHidden;
    private static int layerMiniMapVisible;
    private static int layerMiniMapHidden;

    static Unit()
    {
        globalUnits = new Dictionary<Factions, List<Unit>>();
        var factions = System.Enum.GetValues(typeof(Factions)).Cast<Factions>();
        foreach (var faction in factions)
        {
            globalUnits.Add(faction, new List<Unit>());
        }
    }

    //[ReadOnly]
    public UnitStates state = UnitStates.Idleing;
    public Factions faction;
    public float visionFadeTime = 1f;
    [Preview]
    public UnitTemplate template;

    //references
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private MeshRenderer selectionCircle, miniMapCircle, visionCircle;
    private FieldOfView fieldOfView;

    //private bool isSelected; //is the Unit currently selected by the Player
    [HideInInspector]
    public List<AICommand> commandList = new List<AICommand>();
    private bool commandRecieved, commandExecuted;
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
        fieldOfView = transform.Find("FieldOfView").GetComponent<FieldOfView>();

        SetLayers();
    }

    void Start()
    {
        //Randomization of NavMeshAgent speed. More fun!
        //float rndmFactor = navMeshAgent.speed * .15f;
        //navMeshAgent.speed += Random.Range(-rndmFactor, rndmFactor);

        globalUnits[faction].Add(this);

        template = template.Clone(); //we copy the template otherwise it's going to overwrite the original asset!


        //Set some defaults, including the default state
        SetSelected(false);

        StartCoroutine(DequeueCommands());

        visionCircle.material.color = visionCircle.material.color.ToWithA(0f);
        if (faction == GameManager.Instance.faction)
        {
            StartCoroutine(VisionFade(visionFadeTime, false));
            SetVisibility(true);
        }
        else
        {
            fieldOfView.enabled = false;
            SetVisibility(false);
        }
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
            case UnitStates.MovingToSpot:
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    commandExecuted = true;
                    AddCommand(new AICommand(AICommand.CommandType.Stop));
                }
                break;

            case UnitStates.AttackMovingToSpot:
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    commandExecuted = true;
                    AddCommand(new AICommand(AICommand.CommandType.Guard));
                }
                else
                {
                    if (fieldOfView.lastVisibleTargets.Count > 0)
                    {
                        var enemies = fieldOfView.lastVisibleTargets.Where(target => target.GetComponent<Unit>().faction != faction && target.GetComponent<Unit>().state != UnitStates.Dead);
                        if (enemies.Count() > 0)
                        {
                            var closestEnemy = enemies.FindClosestToPoint(transform.position).GetComponent<Unit>();
                            //MoveToAttack(closestEnemy);
                            commandExecuted = true;
                            commandRecieved = false;
                            InsertCommand(new AICommand(AICommand.CommandType.AttackTarget, closestEnemy));
                        }
                    }
                }
                break;

            case UnitStates.MovingToTarget:
                //check if target has been killed by somebody else
                if (IsDeadOrNull(targetOfAttack) || targetOfAttack.faction == faction)
                {
                    commandExecuted = true;
                    //Idle();
                }
                else
                {
                    if (commandList.Count >= 2 && commandList[1].commandType == AICommand.CommandType.Guard)
                    {
                        if (Vector3.Distance(commandList[1].destination.Value, transform.position) > template.guardDistance * 2f)
                        {
                            commandExecuted = true;
                            InsertCommand(new AICommand(AICommand.CommandType.MoveTo, commandList[1].destination.Value), 1);
                        }
                    }
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

            case UnitStates.Guarding:
                if (Time.time > lastGuardCheckTime + guardCheckInterval)
                {
                    lastGuardCheckTime = Time.time;
                    Unit closestEnemy = GetNearestHostileUnit();
                    if (closestEnemy != null)
                    {
                        commandExecuted = true;
                        commandRecieved = false;
                        InsertCommand(new AICommand(AICommand.CommandType.AttackTarget, closestEnemy));
                    }
                }
                break;

            case UnitStates.Attacking:
                //check if target has been killed by somebody else
                if (IsDeadOrNull(targetOfAttack) || targetOfAttack.faction == faction)
                {
                    commandExecuted = true;
                    //Guard();
                }
                else
                {
                    //look towards the target
                    Vector3 desiredForward = (targetOfAttack.transform.position - transform.position).normalized;
                    transform.forward = Vector3.Lerp(transform.forward, desiredForward, Time.deltaTime * 10f);
                }
                break;
            case UnitStates.Dead:
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

    private static void SetLayers()
    {
        layerDefaultVisible = LayerMask.NameToLayer("Default");
        layerDefaultHidden = LayerMask.NameToLayer("Default Hidden");
        layerMiniMapVisible = LayerMask.NameToLayer("MiniMap Only");
        layerMiniMapHidden = LayerMask.NameToLayer("MiniMap Hidden");
    }

    public void AddCommand(AICommand command, bool clear = false)
    {
        if (clear)
        {
            commandList.Clear();
            commandExecuted = true;
            commandRecieved = false;
        }
        commandList.Add(command);
    }

    public void InsertCommand(AICommand command, int position = 0)
    {
        commandList.Insert(position, command);
    }

    private IEnumerator DequeueCommands()
    {
        commandRecieved = false;
        commandExecuted = true;
        AICommand stopCommand = new AICommand(AICommand.CommandType.Stop);
        switch (state)
        {
            case UnitStates.Idleing:
                AddCommand(stopCommand);
                break;
            case UnitStates.Guarding:
                AddCommand(new AICommand(AICommand.CommandType.Guard, transform.position));
                break;
            default:
                Debug.LogError("Cannot start with a state different to Idle or Guard. State has been set to Idle.", gameObject);
                state = UnitStates.Idleing;
                goto case UnitStates.Idleing;
        }
        for (; ; )
        {
            if (state == UnitStates.Dead)
            {
                //already dead
                yield break;
            }
            if (commandList.Count == 0)
            {
                if (commandExecuted && state != UnitStates.Idleing && state != UnitStates.Guarding) AddCommand(stopCommand);
                else yield return null;
                continue;
            }
            else
            {
                if (commandExecuted)
                {
                    if(commandList.Count == 1 && (commandList[0].commandType == AICommand.CommandType.Stop || commandList[0].commandType == AICommand.CommandType.Guard))
                    {
                        yield return null;
                        continue;
                    }

                    if (commandRecieved)
                    {
                        commandList.RemoveAt(0);
                        commandRecieved = false;
                    }
                    commandExecuted = false;

                    if (commandList.Count == 0)
                    {
                        continue;
                    }

                    AICommand nextCommand = commandList[0];
                    ExecuteCommand(nextCommand);
                }
                yield return null;
            }
        }
    }

    private void ExecuteCommand(AICommand command)
    {
        if (state == UnitStates.Dead)
        {
            //already dead
            Debug.LogWarning("Unit is dead. Cannot execute command.", gameObject);
            return;
        }

        print(name + " Execute cmd: " + command.commandType);

        commandExecuted = false;
        commandRecieved = true;
        switch (command.commandType)
        {
            case AICommand.CommandType.MoveTo:
                MoveToSpot(command.destination.Value);
                break;

            case AICommand.CommandType.AttackMoveTo:
                AttackMoveToSpot(command.destination.Value);
                break;

            case AICommand.CommandType.Stop:
                Idle();
                break;

            case AICommand.CommandType.Guard:
                Guard();
                break;

            case AICommand.CommandType.AttackTarget:
                MoveToTarget(command.target);
                break;

            case AICommand.CommandType.Die:
                Die();
                break;
        }
    }

    //move to a position and be idle
    private void MoveToSpot(Vector3 location)
    {
        state = UnitStates.MovingToSpot;

        targetOfAttack = null;
        agentReady = false;

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(location);
    }

    //move to a position and be guarding
    private void AttackMoveToSpot(Vector3 location)
    {
        state = UnitStates.AttackMovingToSpot;

        targetOfAttack = null;
        agentReady = false;

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(location);
    }

    //stop and stay Idle
    private void Idle()
    {
        state = UnitStates.Idleing;
        commandExecuted = true;

        targetOfAttack = null;
        agentReady = false;

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
    }

    //stop but watch for enemies nearby
    public void Guard()
    {
        state = UnitStates.Guarding;
        commandExecuted = true;

        targetOfAttack = null;
        agentReady = false;

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
    }

    //move towards a target to attack it
    private void MoveToTarget(Unit target)
    {
        if (!IsDeadOrNull(target))
        {
            state = UnitStates.MovingToTarget;
            targetOfAttack = target;
            agentReady = false;

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(target.transform.position);
        }
        else
        {
            //if the command is dealt by a Timeline, the target might be already dead
            //Guard();
            commandExecuted = true;
            AddCommand(new AICommand(AICommand.CommandType.Stop));
        }
    }

    //reached the target (within engageDistance), time to attack
    private void StartAttacking()
    {
        //somebody might have killed the target while this Unit was approaching it
        if (!IsDeadOrNull(targetOfAttack))
        {
            state = UnitStates.Attacking;
            agentReady = false;
            navMeshAgent.isStopped = true;
            StartCoroutine(DealAttack());
        }
        else
        {
            //Guard();
            commandExecuted = true;
            AddCommand(new AICommand(AICommand.CommandType.Stop));
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

                MoveToTarget(targetOfAttack);

                yield break;
            }

            yield return new WaitForSeconds(1f / template.attackSpeed);

            //check is performed after the wait, because somebody might have killed the target in the meantime
            if (IsDeadOrNull(targetOfAttack))
            {
                break;
            }

            if (targetOfAttack.faction == faction)
            {
                break;
            }

            if (state == UnitStates.Dead)
            {
                yield break;
            }

            //Too far away check moved to before waittime

            targetOfAttack.SufferAttack(template.attackPower);
        }
        if (animator != null) animator.SetBool("DoAttack", false);

        //only move into Guard if the attack was interrupted (dead target, etc.)
        if (state == UnitStates.Attacking)
        {
            commandExecuted = true;
            //AddCommand(new AICommand(AICommand.CommandType.Stop));
        }
    }

    //called by an attacker
    private void SufferAttack(int damage)
    {
        if (state == UnitStates.Dead)
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

        commandList.Clear();
        commandExecuted = true;

        state = UnitStates.Dead; //still makes sense to set it, because somebody might be interacting with this script before it is destroyed
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

        globalUnits[faction].Remove(this);

        //Remove unneeded Components
        //Destroy(sightCircle);
        StartCoroutine(HideSeenThings(visionFadeTime / 2f));
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

    private IEnumerator HideSeenThings(float fadeTime)
    {
        if (fadeTime != 0f) yield return Yielders.Get(fadeTime);
        float radius = template.guardDistance;
        template.guardDistance = 0f;
        fieldOfView.MarkTargetsVisibility();
        template.guardDistance = radius;
    }

    private static bool IsDeadOrNull(Unit unit)
    {
        return (unit == null || unit.state == UnitStates.Dead);
    }

    private Unit GetNearestHostileUnit()
    {
        hostiles = FindObjectsOfType<Unit>().Where(unit => unit.faction != faction).ToArray();//GameObject.FindGameObjectsWithTag(template.GetOtherFaction().ToString()).Select(x => x.GetComponent<Unit>()).ToArray();

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
        Color newColor = (faction == GameManager.Instance.faction) ? Color.green : Color.red;
        miniMapCircle.material.color = newColor;
        newColor.a = (selected) ? 1f : .3f;
        selectionCircle.material.color = newColor;
    }

    public void SetVisibility(bool visibility)
    {
        if (visibility)
        {
            if (gameObject.layer == layerDefaultVisible) return;
        }
        else
        {
            if (gameObject.layer == layerDefaultHidden) return;
        }

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
                if (visibility) part.layer = layerDefaultVisible;
                else part.layer = layerDefaultHidden;
            }
            else
            {
                if (visibility) part.layer = layerMiniMapVisible;
                else part.layer = layerMiniMapHidden;
            }
        }
    }
}