using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Unit : ClickableObject
{
    public enum UnitStates
    {
        Idleing,
        Attacking,
        MovingToTarget,
        MovingToSpot,
        Dead,
        CustomActionAtPos,
        CustomActionAtObj,
    }

    public UnitStates state = UnitStates.Idleing;
    public Transform projectileFirePoint;
    public bool alignToGround;

    //references
    protected Animator animator;
    protected NavMeshAgent navMeshAgent;
    protected ResourceCollector resourceCollector;

    protected List<AICommand> commandList = new List<AICommand>();
    protected bool agentReady = false;
    protected bool commandRecieved, commandExecuted;
    protected UnitStates? switchState;
    protected AICommand.CustomActions? customAction;

    protected InteractableObject targetOfAttack;
    protected Vector3? targetOfMovement;

    private readonly float combatReadySwitchTime = 7f;

    private Coroutine lerpingCombatReady;

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        resourceCollector = GetComponent<ResourceCollector>();
    }

    protected override void Start()
    {
        faction.data.units.Add(this);

        UpdateMaterialTeamColor();

        //Set some defaults, including the default state
        SetSelected(false);

        base.Start();
    }

    protected override void Update()
    {
        //Little hack to give time to the NavMesh agent to set its destination.
        //without this, the Unit would switch its state before the NavMeshAgent can kick off, leading to unpredictable results
        if (!agentReady)
        {
            agentReady = true;
            return;
        }

        base.Update();

        UpdateStates();
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh && navMeshAgent.hasPath)
        {
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawLine(transform.position, navMeshAgent.destination);
        }

        base.OnDrawGizmos();
    }
#endif

    public static new bool IsDeadOrNull(InteractableObject unit)
    {
        return unit == null || ((unit is Unit) ? (unit as Unit).state == UnitStates.Dead : ClickableObject.IsDeadOrNull(unit));
    }

    public void AddCommand(AICommand command, bool clear = false)
    {
        if (!CheckCommandViability(command))
        {
            Debug.LogWarning(string.Concat("Command not accepted: ", command.commandType, " ", command.customAction), this);
            return;
        }
        if (clear || command.commandType == AICommand.CommandTypes.Stop)
        {
            commandList.Clear();
            commandExecuted = false;
            commandRecieved = false;
        }
        if (command.commandType != AICommand.CommandTypes.Stop)
        {
            commandList.Add(command);
        }
    }

    private bool CheckCommandViability(AICommand command)
    {
        //make units be able to denie command... oh what could possibly go wrong
        switch (command.commandType)
        {
            case AICommand.CommandTypes.MoveTo:
                //case AICommand.CommandType.AttackMoveTo:
                //case AICommand.CommandType.Guard:
                return !command.destination.IsNaN();
            case AICommand.CommandTypes.AttackTarget:
                return !IsDeadOrNull(command.target) && command.target != this;
            case AICommand.CommandTypes.Stop:
            case AICommand.CommandTypes.Die:
                return true;
            case AICommand.CommandTypes.CustomActionAtPos:
                if (!command.customAction.HasValue)
                {
                    throw new System.ArgumentNullException();
                }
                return template.original.customActions.Contains(command.customAction.Value) && !command.destination.IsNaN();
            case AICommand.CommandTypes.CustomActionAtObj:
                if (!command.customAction.HasValue)
                {
                    throw new System.ArgumentNullException();
                }
                return template.original.customActions.Contains(command.customAction.Value) && !IsDeadOrNull(command.target);
        }
        throw new System.NotImplementedException(string.Concat("Command Type '", command.commandType.ToString(), "' not valid"));
    }

    private void UpdateStates()
    {
        if (state == UnitStates.Dead)
        {
            return;
        }
        if (!commandRecieved || commandRecieved && commandExecuted)
        {
            if (commandExecuted)
            {
                commandList.RemoveAt(0);
                commandRecieved = false;
                commandExecuted = false;
            }
            if (commandList.Count > 0)
            {
                AICommand nextCommand = commandList[0];
                ExecuteCommand(nextCommand);
            }
            else if (state != UnitStates.Idleing)
            {
                switchState = UnitStates.Idleing;
                TransitOutOfState(state);
                TransitIntoState(switchState.Value);
            }
        }
        if (commandRecieved && !commandExecuted && switchState.HasValue)
        {
            TransitOutOfState(state);
            TransitIntoState(switchState.Value);
        }
        UpdateState(state);
    }

    private void ExecuteCommand(AICommand command)
    {
        command.origin = transform.position;

        targetOfMovement = null;
        targetOfAttack = null;
        switchState = null;
        customAction = null;
        commandRecieved = true;
        commandExecuted = false;
        switch (command.commandType)
        {
            case AICommand.CommandTypes.MoveTo:
                targetOfMovement = command.destination;
                TransitIntoState(UnitStates.MovingToSpot);
                break;
            case AICommand.CommandTypes.Stop:
                TransitIntoState(UnitStates.Idleing);
                break;
            case AICommand.CommandTypes.AttackTarget:
                targetOfAttack = command.target;
                TransitIntoState(UnitStates.MovingToTarget);
                break;
            case AICommand.CommandTypes.Die:
                TransitIntoState(UnitStates.Dead);
                break;
            case AICommand.CommandTypes.CustomActionAtPos:
                targetOfMovement = command.destination;
                customAction = command.customAction;
                TransitIntoState(UnitStates.CustomActionAtPos);
                break;
            case AICommand.CommandTypes.CustomActionAtObj:
                targetOfAttack = command.target;
                customAction = command.customAction;
                TransitIntoState(UnitStates.CustomActionAtObj);
                break;
        }
    }

    private void TransitIntoState(UnitStates newState)
    {
        switch (newState)
        {
            case UnitStates.Idleing:
                animator?.SetBool("DoAttack", false);

                navMeshAgent.isStopped = true;
                break;
            case UnitStates.Attacking:
                navMeshAgent.stoppingDistance = template.engageDistance;
                targetOfMovement = targetOfAttack.transform.position;
                navMeshAgent.SetDestination(targetOfMovement.Value);
                agentReady = false;
                break;
            case UnitStates.MovingToTarget:
                navMeshAgent.stoppingDistance = template.engageDistance;
                targetOfMovement = targetOfAttack.transform.position;
                navMeshAgent.SetDestination(targetOfMovement.Value);
                navMeshAgent.isStopped = false;
                agentReady = false;
                animator?.SetBool("DoAttack", false);
                break;
            case UnitStates.MovingToSpot:
                navMeshAgent.stoppingDistance = 0.1f;
                navMeshAgent.SetDestination(targetOfMovement.Value);
                navMeshAgent.isStopped = false;
                agentReady = false;
                animator?.SetBool("DoAttack", false);
                break;
            case UnitStates.Dead:
                Die();
                break;
            case UnitStates.CustomActionAtPos:
                navMeshAgent.stoppingDistance = 0.1f;
                navMeshAgent.SetDestination(targetOfMovement.Value);
                navMeshAgent.isStopped = false;
                agentReady = false;
                break;
            case UnitStates.CustomActionAtObj:
                navMeshAgent.stoppingDistance = template.engageDistance;
                targetOfMovement = targetOfAttack.transform.position;
                navMeshAgent.SetDestination(targetOfMovement.Value);
                agentReady = false;
                break;
        }
        state = newState;
        switchState = null;
    }

    private void UpdateState(UnitStates currentState)
    {
        //always run these
        SetWalkingSpeed();
        AdjustModelAngleToGround();

        switch (currentState)
        {
            case UnitStates.Idleing:
                {
                    navMeshAgent.isStopped = true;

                    break;
                }
            case UnitStates.Attacking:
                {
                    navMeshAgent.isStopped = true;

                    if (IsDeadOrNull(targetOfAttack))
                    {
                        commandExecuted = true;
                    }
                    float remainingDistance = Vector3.Distance(transform.position, targetOfMovement.Value);
                    //recalculate path
                    if (Vector3.Distance(targetOfAttack.transform.position, targetOfMovement.Value) > 0.05f)
                    {
                        targetOfMovement = targetOfAttack.transform.position;
                        navMeshAgent.SetDestination(targetOfMovement.Value);
                    }
                    //check if in attack range
                    if ((template.engageDistance + targetOfAttack.sizeRadius) < remainingDistance)
                    {
                        switchState = UnitStates.MovingToTarget;
                    }
                    else
                    {
                        FaceTarget();
                        animator?.SetBool("DoAttack", true);
                    }
                    break;
                }
            case UnitStates.MovingToTarget:
                {
                    if (!agentReady || navMeshAgent.pathPending)
                    {
                        break;
                    }
                    if (IsDeadOrNull(targetOfAttack))
                    {
                        commandExecuted = true;
                    }
                    float remainingDistance = Vector3.Distance(transform.position, targetOfMovement.Value);
                    //recalculate path
                    if (Vector3.Distance(targetOfAttack.transform.position, targetOfMovement.Value) > 0.05f)
                    {
                        targetOfMovement = targetOfAttack.transform.position;
                        navMeshAgent.SetDestination(targetOfMovement.Value);
                    }
                    //check if in attack range
                    if ((template.engageDistance + targetOfAttack.sizeRadius) >= remainingDistance)
                    {
                        if (customAction == null)
                        {
                            switchState = UnitStates.Attacking;
                        }
                        else
                        {
                            switchState = UnitStates.CustomActionAtObj;
                        }
                    }
                    break;
                }
            case UnitStates.MovingToSpot:
                {
                    if (!agentReady || navMeshAgent.pathPending)
                    {
                        break;
                    }
                    float remainingDistance = Vector3.Distance(transform.position, targetOfMovement.Value);
                    if (remainingDistance < 0.3f)
                    {
                        if (customAction == null)
                        {
                            commandExecuted = true;
                        }
                        else
                        {
                            switchState = UnitStates.CustomActionAtPos;
                        }
                    }
                    break;
                }
            case UnitStates.Dead:
                break;
            case UnitStates.CustomActionAtPos:
                {
                    navMeshAgent.isStopped = true;

                    float remainingDistance = Vector3.Distance(transform.position, targetOfMovement.Value);
                    //check if in attack range
                    if (template.engageDistance < remainingDistance)
                    {
                        switchState = UnitStates.MovingToSpot;
                    }
                    else
                    {
                        switch (customAction.Value)
                        {
                            case AICommand.CustomActions.collectResources:
                                SeekNewResourceSource();
                                break;
                        }
                    }
                    break;
                }
            case UnitStates.CustomActionAtObj:
                {
                    navMeshAgent.isStopped = true;

                    if (IsDeadOrNull(targetOfAttack))
                    {
                        commandExecuted = true;
                    }
                    float remainingDistance = Vector3.Distance(transform.position, targetOfMovement.Value);
                    //recalculate path
                    if (Vector3.Distance(targetOfAttack.transform.position, targetOfMovement.Value) > 0.05f)
                    {
                        targetOfMovement = targetOfAttack.transform.position;
                        navMeshAgent.SetDestination(targetOfMovement.Value);
                    }
                    //check if in attack range
                    if ((template.engageDistance + targetOfAttack.sizeRadius) < remainingDistance)
                    {
                        switchState = UnitStates.MovingToTarget;
                    }
                    else
                    {
                        ResourceSource resourceSource = targetOfAttack.GetComponent<ResourceSource>();
                        switch (customAction.Value)
                        {
                            case AICommand.CustomActions.collectResources:
                                if (resourceCollector.isFull)
                                {
                                    commandExecuted = true;
                                    break;
                                }
                                else if (resourceSource.isEmpty)
                                {
                                    SeekNewResourceSource();
                                    break;
                                }
                                FaceTarget();
                                animator?.SetBool("DoAttack", true);
                                break;
                            case AICommand.CustomActions.dropoffResources:
                                if (resourceCollector.isNotEmpty)
                                {
                                    KeyValuePair<ResourceSource.ResourceType, int> resourceBundle = resourceCollector.EmptyStorage();
                                    ResourceDropoff resourceDropoff = targetOfAttack.GetComponent<ResourceDropoff>();
                                    resourceDropoff.DropResource(resourceBundle.Value, resourceBundle.Key);
                                }
                                commandExecuted = true;
                                break;
                        }
                    }
                    break;
                }
        }

        if (switchState.HasValue)
        {
            //print(gameObject.name + ": switch from " + state + " to " + switchState.Value);
        }
    }

    private void TransitOutOfState(UnitStates oldState)
    {
        switch (oldState)
        {
            case UnitStates.Idleing:
                break;
            case UnitStates.Attacking:
                animator?.SetBool("DoAttack", false);
                break;
            case UnitStates.MovingToTarget:
                break;
            case UnitStates.MovingToSpot:
                break;
            case UnitStates.Dead:
                modelHolder.position += Vector3.up * decayIntoGroundDistance;
                break;
            case UnitStates.CustomActionAtPos:
                switch (customAction.Value)
                {
                    case AICommand.CustomActions.collectResources:
                        animator?.SetBool("DoAttack", false);
                        break;
                }
                break;
            case UnitStates.CustomActionAtObj:
                switch (customAction.Value)
                {
                    case AICommand.CustomActions.collectResources:
                        animator?.SetBool("DoAttack", false);

                        if (resourceCollector != null && resourceCollector.isFull)
                        {
                            Building dropoffBuilding = faction.GetClosestBuildingWithResourceDropoff(transform.position, targetOfAttack.GetComponent<ResourceSource>().resourceType);
                            AICommand dropResourcesCommand = new AICommand(AICommand.CommandTypes.CustomActionAtObj, dropoffBuilding, AICommand.CustomActions.dropoffResources);
                            AddCommand(dropResourcesCommand);

                            AICommand getBackCollectingCommand = new AICommand(AICommand.CommandTypes.CustomActionAtPos, transform.position, AICommand.CustomActions.collectResources);
                            AddCommand(getBackCollectingCommand);
                        }
                        break;
                    default:
                        break;
                }
                break;
        }
    }

    public void TriggerAttackAnimEvent(int Int)//Functionname equals Eventname
    {
        if (state == UnitStates.Dead || IsDeadOrNull(targetOfAttack))
        {
            animator?.SetBool("DoAttack", false);
            return;
        }

        int damage = Random.Range(template.damage.x, template.damage.y + 1);
        if (template.projectile != null)
        {
            ShootProjectileAtTarget(damage);
        }
        else
        {
            bool success = targetOfAttack.SufferAttack(damage, resourceCollector);
            if (!success)
            {
                animator?.SetBool("DoAttack", false);

                if (state == UnitStates.CustomActionAtObj && customAction.Value == AICommand.CustomActions.collectResources)
                {
                    AICommand getBackCollectingCommand = new AICommand(AICommand.CommandTypes.CustomActionAtPos, targetOfMovement.Value, AICommand.CustomActions.collectResources);
                    AddCommand(getBackCollectingCommand);
                }
                return;
            }
        }
    }

    private void SeekNewResourceSource()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, template.guardDistance, InputManager.Instance.unitsLayerMask, QueryTriggerInteraction.Collide);
        Collider[] resources = colliders.Where(collider => collider.GetComponent<ResourceSource>() != null).ToArray();

        InteractableObject closest = null;
        float distanceToClosestSqr = float.PositiveInfinity;
        foreach (var collider in resources)
        {
            float distanceSqr = (collider.transform.position - targetOfMovement.Value).sqrMagnitude;
            if (distanceSqr < distanceToClosestSqr)
            {
                distanceToClosestSqr = distanceSqr;
                closest = collider.GetComponent<InteractableObject>();
            }
        }

        if (closest == null)
        {
            if (resourceCollector.isNotEmpty)
            {
                Building dropoffBuilding = faction.GetClosestBuildingWithResourceDropoff(transform.position, targetOfAttack.GetComponent<ResourceSource>().resourceType);
                AICommand dropResourcesCommand = new AICommand(AICommand.CommandTypes.CustomActionAtObj, dropoffBuilding, AICommand.CustomActions.dropoffResources);
                AddCommand(dropResourcesCommand);
            }
            commandExecuted = true;
            return;
        }
        targetOfAttack = closest;
        targetOfMovement = targetOfAttack.transform.position;
        switchState = UnitStates.CustomActionAtObj;
    }

    private void ShootProjectileAtTarget(int damage)
    {
        if (template.projectile == null || template.projectile.GetComponent<Projectile>() == null)
        {
            throw new System.NullReferenceException("This unit has no Projectile set");
        }
        if (projectileFirePoint == null)
        {
            throw new System.NullReferenceException("This unit has no Projectile Fire Point set");
        }

        Projectile projectileInstance = Instantiate(template.projectile, projectileFirePoint.position, projectileFirePoint.rotation).GetComponent<Projectile>();
        projectileInstance.LaunchAt(targetOfAttack.transform, damage, this);
    }

    public override bool SufferAttack(int damage, ResourceCollector resourceCollector = null)
    {
        if (state == UnitStates.Dead)
        {
            return false;
        }

        return base.SufferAttack(damage);
    }

    protected override void Die()
    {
        if (state != UnitStates.Dead)
        {
            TransitOutOfState(state);
            state = UnitStates.Dead;
        }

        base.Die();

        commandExecuted = true;

        commandList.Clear();

        navMeshAgent.isStopped = true;

        animator?.SetBool("DoAttack", false);
        animator?.SetTrigger("DoDeath");

        //Remove itself from the selection Platoon
        GameManager.Instance.RemoveFromSelection(this);
        SetSelected(false);

        faction.data.units.Remove(this);

        //Remove unneeded Components
        StartCoroutine(HideSeenThings(visionFadeTime));
        StartCoroutine(VisionFade(visionFadeTime, true));
        //navMeshAgent.enabled = false;
        StartCoroutine(DecayIntoGround());
    }

    private void SetWalkingSpeed()
    {
        float navMeshAgentSpeed = navMeshAgent.velocity.magnitude;
        animator?.SetFloat("Speed", navMeshAgentSpeed * 0.05f);
    }

    private void FaceTarget()
    {
        Vector3 dir = (targetOfMovement.Value - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir.ToWithY(0f));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
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

    public void AdjustModelAngleToGround()
    {
        Ray ray = new Ray(selectionCircle.transform.position + Vector3.up * 0.1f, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1f, InputManager.Instance.groundLayerMask))
        {
            Quaternion newRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * selectionCircle.transform.parent.rotation;
            newRotation = Quaternion.Lerp(selectionCircle.transform.rotation, newRotation, Time.deltaTime * 8f);
            if (alignToGround)
            {
                modelHolder.rotation = newRotation;
            }
            selectionCircle.transform.rotation = newRotation;
        }
    }

    public bool SetCombatReady(bool state)
    {
        const string name = "DoCombatReady";
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == name)
            {
                float value = animator.GetFloat(name);
                float stat = state.ToFloat();
                if (value != stat)
                {
                    if (lerpingCombatReady != null)
                    {
                        StopCoroutine(lerpingCombatReady);
                    }
                    lerpingCombatReady = StartCoroutine(LerpCombatReadyAnim(state.ToFloat()));
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
                lerpingCombatReady = null;
                yield break;
            }
        }
    }

#if UNITY_EDITOR
    public List<AICommand> GetCommandList()
    {
        return commandList;
    }
#endif
}
