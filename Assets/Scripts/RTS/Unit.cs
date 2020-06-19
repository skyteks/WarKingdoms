using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// Unit semi-AI handles movement and stats
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Unit : ClickableObject
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

    public UnitStates state = UnitStates.Idleing;
    public float combatReadySwitchTime = 7f;
    public Transform projectileFirePoint;

    //references
    protected NavMeshAgent navMeshAgent;
    protected Animator animator;

    //private bool isSelected; //is the Unit currently selected by the Player
    [HideInInspector]
    public List<AICommand> commandList = new List<AICommand>();
    protected bool commandRecieved, commandExecuted;
    protected ClickableObject targetOfAttack;
    protected ClickableObject[] hostiles;
    protected float lastGuardCheckTime, guardCheckInterval = 1f;
    protected bool agentReady = false;
    protected Coroutine LerpingCombatReady;

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    protected override void Start()
    {
        faction.units.Add(this);

        SetColorMaterial();

        //Set some defaults, including the default state
        SetSelected(false);

        StartCoroutine(DequeueCommands());

        base.Start();
    }

    protected void Update()
    {
        //Little hack to give time to the NavMesh agent to set its destination.
        //without this, the Unit would switch its state before the NavMeshAgent can kick off, leading to unpredictable results
        if (!agentReady)
        {
            agentReady = true;
            return;
        }

        UpdateMinimapUI();

        switch (state)
        {
            case UnitStates.MovingToSpot:
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    Idle();
                }
                AdjustModelAngleToGround();
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
                        //var enemies = fieldOfView.lastVisibleTargets.Where(target => !IsDeadOrNull(target.GetComponent<Unit>()) && !FactionTemplate.IsAlliedWith(faction, target.GetComponent<Unit>().faction));
                        var enemies = fieldOfView.lastVisibleTargets.Where(target => !FactionTemplate.IsAlliedWith(target.GetComponent<Unit>().faction, faction) && target.GetComponent<Unit>().state != UnitStates.Dead);
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
                AdjustModelAngleToGround();
                break;

            case UnitStates.MovingToTarget:
                //check if target has been killed by somebody else
                if (targetOfAttack.IsDeadOrNull(targetOfAttack))
                {
                    commandExecuted = true;
                    //Idle();
                }
                else
                {
                    if (commandList.Count >= 2 && commandList[1].commandType == AICommand.CommandType.Guard)
                    {
                        if (Vector3.Distance(commandList[1].destination, transform.position) > template.guardDistance * 2f)
                        {
                            commandExecuted = true;
                            InsertCommand(new AICommand(AICommand.CommandType.MoveTo, commandList[1].destination), 1);
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
                AdjustModelAngleToGround();
                break;

            case UnitStates.Guarding:
                if (Time.time > lastGuardCheckTime + guardCheckInterval)
                {
                    lastGuardCheckTime = Time.time;
                    ClickableObject[] closestEnemies = GetNearestHostileUnits();
                    for (int i = 0; i < closestEnemies.Length; i++)
                    {
                        commandExecuted = true;
                        commandRecieved = false;
                        InsertCommand(new AICommand(AICommand.CommandType.AttackTarget, closestEnemies[i]));
                    }
                    AdjustModelAngleToGround();
                }
                break;

            case UnitStates.Attacking:
                //check if target has been killed by somebody else
                commandExecuted = true;
                if (targetOfAttack.IsDeadOrNull(targetOfAttack))
                {
                    if (animator != null)
                    {
                        animator.SetBool("DoAttack", false);
                    }
                    Idle();
                }
                else if (commandList.Count >= 2 && commandList[1].commandType == AICommand.CommandType.Guard)
                {
                    if (Vector3.Distance(commandList[1].destination, transform.position) > 0.1f)
                    {
                        InsertCommand(new AICommand(AICommand.CommandType.MoveTo, commandList[1].destination), 1);
                    }
                }
                else if (Vector3.Distance(targetOfAttack.transform.position, transform.position) > template.engageDistance)
                {
                    //Check if the target moved away for some reason
                    if (animator != null)
                    {
                        animator.SetBool("DoAttack", false);
                    }

                    MoveToTarget(targetOfAttack);
                }
                else if (Vector3.Angle(transform.forward, (targetOfAttack.transform.position - transform.position).normalized) > 10f)
                {
                    //look towards the target
                    Vector3 desiredForward = (targetOfAttack.transform.position - transform.position).normalized;
                    transform.forward = Vector3.Lerp(transform.forward, desiredForward, Time.deltaTime * 10f);
                }
                else
                {
                    if (animator != null)
                    {
                        animator.SetBool("DoAttack", true);
                    }
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
        if (animator != null)
        {
            animator.SetFloat("Speed", navMeshAgentSpeed * .05f);
        }

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
            UnityEditor.Handles.color = Color.yellow;//Random.onUnitSphere.ToVector4(1f).ToColor();
            UnityEditor.Handles.DrawLine(transform.position, navMeshAgent.destination);
        }

        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, template.engageDistance);
        UnityEditor.Handles.color = Color.gray;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, template.guardDistance);
    }
#endif

    public override bool IsDeadOrNull(ClickableObject unit)
    {
        return unit == null || (unit as Unit).state == UnitStates.Dead;
    }

    public void AddCommand(AICommand command, bool clear = false)
    {
        if (!CheckCommandViability(command))
        {
            return;
        }
        if (clear || command.commandType == AICommand.CommandType.Stop)
        {
            commandList.Clear();
        }
        commandExecuted = true;
        commandRecieved = false;
        if (command.commandType != AICommand.CommandType.Stop)
        {
            commandList.Add(command);
        }
    }

    public void InsertCommand(AICommand command, int position = 0)
    {
        if (!CheckCommandViability(command))
        {
            return;
        }
        commandList.Insert(position, command);
    }

    private bool CheckCommandViability(AICommand command)
    {
        //make units be able to denie command... oh what could possibly go wrong
        switch (command.commandType)
        {
            case AICommand.CommandType.MoveTo:
            case AICommand.CommandType.AttackMoveTo:
            case AICommand.CommandType.Guard:
                return !command.destination.IsNaN();
            case AICommand.CommandType.AttackTarget:
                return !command.target.IsDeadOrNull(command.target) && command.target != this;
            case AICommand.CommandType.Stop:
            case AICommand.CommandType.Die:
                return true;
        }
        throw new System.NotImplementedException(string.Concat("Command Type '", command.commandType.ToString(), "' not valid"));
    }

    private IEnumerator DequeueCommands()
    {
        commandRecieved = false;
        commandExecuted = true;
        switch (state)
        {
            case UnitStates.Idleing:
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
                yield return null;
                continue;
            }
            else
            {
                if (commandExecuted)
                {
                    if (commandList.Count == 1 && (commandList[0].commandType == AICommand.CommandType.Guard))
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


        //Debug.Log(string.Concat(name, " Execute cmd: ", command.commandType));

        commandExecuted = false;
        commandRecieved = true;
        switch (command.commandType)
        {
            case AICommand.CommandType.MoveTo:
                MoveToSpot(command.destination);
                break;

            case AICommand.CommandType.AttackMoveTo:
                AttackMoveToSpot(command.destination);
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
    private void MoveToTarget(ClickableObject target)
    {
        if (!target.IsDeadOrNull(target))
        {
            state = UnitStates.MovingToTarget;
            targetOfAttack = target;
            agentReady = false;

            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(target.transform.position);
            }
        }
        else
        {
            //if the command is dealt by a Timeline, the target might be already dead
            commandExecuted = true;
        }
    }

    //reached the target (within engageDistance), time to attack
    private void StartAttacking()
    {
        //somebody might have killed the target while this Unit was approaching it
        if (!targetOfAttack.IsDeadOrNull(targetOfAttack))
        {
            state = UnitStates.Attacking;
            agentReady = false;
            navMeshAgent.isStopped = true;
        }
        else
        {
            commandExecuted = true;
            //AddCommand(new AICommand(AICommand.CommandType.Stop));
        }
    }

    public void TriggerAttackAnimEvent(int Int)//Functionname equals Eventname
    {
        if (state == UnitStates.Dead || targetOfAttack.IsDeadOrNull(targetOfAttack))
        {
            //already dead
            animator.SetBool("DoAttack", false);
            return;
        }

        int damage = Random.Range(template.damage.x, template.damage.y + 1);
        if (template.projectile != null)
        {
            ShootProjectile(damage);
        }
        else
        {
            targetOfAttack.SufferAttack(damage);
        }
    }

    //called by an attacker
    public override void SufferAttack(int damage)
    {
        if (state == UnitStates.Dead)
        {
            return;
        }

        base.Die();
    }

    //called in SufferAttack, but can also be from a Timeline clip
    protected override void Die()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            return;
        }
        base.Die();

        AdjustModelAngleToGround();

        commandList.Clear();
        commandExecuted = true;

        state = UnitStates.Dead; //still makes sense to set it, because somebody might be interacting with this script before it is destroyed
        if (animator != null)
        {
            animator.SetTrigger("DoDeath");
        }

        //Remove itself from the selection Platoon
        GameManager.Instance.RemoveFromSelection(this);
        SetSelected(false);

        //Fire an event so any Platoon containing this Unit will be notified
        if (OnDeath != null)
        {
            OnDeath.Invoke(this);
        }

        faction.units.Remove(this);

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
        navMeshAgent.enabled = false;
        StartCoroutine(DecayIntoGround());
    }

    private IEnumerator DecayIntoGround()
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

    private ClickableObject[] GetNearestHostileUnits()
    {
        hostiles = FindObjectsOfType<Unit>().Where(unit => !FactionTemplate.IsAlliedWith(unit.faction, faction)).Where(unit => Vector3.Distance(unit.transform.position, transform.position) < template.guardDistance).ToArray();

        //TODO: sort array by distance
        return hostiles;
    }

    private ClickableObject GetNearestHostileUnit()
    {
        hostiles = FindObjectsOfType<Unit>().Where(unit => !FactionTemplate.IsAlliedWith(unit.faction, faction)).ToArray();

        ClickableObject nearestEnemy = null;
        float nearestEnemyDistance = float.PositiveInfinity;
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

    private void UpdateMinimapUI()
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

    public void AdjustModelAngleToGround()
    {
        Ray ray = new Ray(modelHolder.position + Vector3.up * 0.1f, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1f, InputManager.Instance.groundLayerMask))
        {
            Quaternion newRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * modelHolder.parent.rotation;
            modelHolder.rotation = Quaternion.Lerp(modelHolder.rotation, newRotation, Time.deltaTime * 8f);
            selectionCircle.transform.rotation = modelHolder.rotation;
        }
    }

    public bool SetCombatReady(bool state)
    {
        const string name = "DoCombatReady";
        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == name)
            {
                float value = animator.GetFloat(name);
                float stat = state.ToFloat();
                if (value != stat)
                {
                    if (LerpingCombatReady != null)
                    {
                        StopCoroutine(LerpingCombatReady);
                    }
                    LerpingCombatReady = StartCoroutine(LerpCombatReadyAnim(state.ToFloat()));
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator LerpCombatReadyAnim(float state)
    {
        const string name = "DoCombatReady";

        float value;
        for (; ; )
        {
            value = animator.GetFloat(name);
            value = Mathf.MoveTowards(value, state, Time.deltaTime * combatReadySwitchTime);
            animator.SetFloat(name, value);
            if (value != state)
            {
                yield return null;
            }
            else
            {
                LerpingCombatReady = null;
                yield break;
            }
        }
    }

    private void ShootProjectile(int damage)
    {
        if (template.projectile == null || template.projectile.GetComponent<Projectile>() == null)
        {
            Debug.LogError("This unit has no Projectile set", this);
            return;
        }
        if (projectileFirePoint == null)
        {
            Debug.LogError("This unit has no Projectile Fire Point set", this);
            return;
        }

        Projectile projectileInstance = Instantiate(template.projectile, projectileFirePoint.position, projectileFirePoint.rotation).GetComponent<Projectile>();
        projectileInstance.LaunchAt(targetOfAttack.transform, damage, this);
    }
}