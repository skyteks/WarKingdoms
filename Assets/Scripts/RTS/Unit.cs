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
    protected new UnitAnimation animation;
    protected MovementNavigation navigation;
    protected ResourceCollector resourceCollector;

    protected List<AICommand> commandList = new List<AICommand>();

    protected bool commandRecieved, commandExecuted;
    protected UnitStates? switchState;
    protected AICommand.CustomActions? customAction;

    protected InteractableObject targetOfAttack;
    protected Vector3? targetOfMovement;
    protected Vector3? returnPoint;

    private static readonly float combatReadySwitchTime = 3f;
    private static readonly AnimationCurve combatReadyAnimCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f));

    private Coroutine lerpingCombatReady;
    private Coroutine lerpingAttackEvent;

    protected override void Awake()
    {
        base.Awake();
        navigation = GetComponent<MovementNavigation>();
        animation = GetComponent<UnitAnimation>();
        resourceCollector = GetComponent<ResourceCollector>();
    }

    protected override void Start()
    {
        if (faction == null)
        {
            throw new System.NullReferenceException(string.Concat("No faction assigned to: ", gameObject.name));
        }
        faction.data.units.Add(this);

        UpdateMaterialTeamColor();

        //Set some defaults, including the default state
        SetSelected(false);

        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        UpdateStates();
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        if (navigation != null && navigation.isOnMesh && navigation.hasPath)
        {
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawLine(transform.position, navigation.destination);

            if (targetOfMovement.HasValue)
            {
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawLine(transform.position, targetOfMovement.Value);
            }
        }

        base.OnDrawGizmos();
    }
#endif

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
                //case AICommand.CommandTypes.AttackMoveTo:
                //case AICommand.CommandTypes.Guard:
                return !command.destination.IsNaN();
            case AICommand.CommandTypes.AttackTarget:
                return command.target != null && !command.target.attackable.isDead && command.target != this;
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
                return template.original.customActions.Contains(command.customAction.Value) && command.target != null && !command.target.attackable.isDead;
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
                if (!commandRecieved)
                {
                    Debug.LogWarning(string.Concat("Command Type '", nextCommand.commandType.ToString(), "' could not be recieved on ", name), gameObject);
                    return;
                }
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
                switch (customAction.Value)
                {
                    case AICommand.CustomActions.collectResources:
                        if (SeekNewResourceSource(resourceCollector.storedType, false))
                        {
                            TransitIntoState(UnitStates.CustomActionAtObj);
                        }
                        else
                        {
                            commandRecieved = false;
                        }
                        break;
                    default:
                        TransitIntoState(UnitStates.CustomActionAtPos);
                        break;
                }
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
                AttackAnim(false);

                navigation.isStopped = true;
                break;
            case UnitStates.Attacking:
                navigation.stoppingDistance = template.engageDistance;
                targetOfMovement = targetOfAttack.transform.position;
                navigation.SetDestination(targetOfMovement.Value);
                navigation.agentReady = false;
                break;
            case UnitStates.MovingToTarget:
                navigation.stoppingDistance = template.engageDistance;
                targetOfMovement = targetOfAttack.transform.position;
                navigation.SetDestination(targetOfMovement.Value);
                navigation.isStopped = false;
                navigation.agentReady = false;
                AttackAnim(false);
                break;
            case UnitStates.MovingToSpot:
                navigation.stoppingDistance = 0.1f;
                navigation.SetDestination(targetOfMovement.Value);
                navigation.isStopped = false;
                navigation.agentReady = false;
                AttackAnim(false);
                break;
            case UnitStates.Dead:
                Die();
                break;
            case UnitStates.CustomActionAtPos:
                navigation.stoppingDistance = 0.1f;
                navigation.SetDestination(targetOfMovement.Value);
                navigation.isStopped = false;
                navigation.agentReady = false;
                break;
            case UnitStates.CustomActionAtObj:
                navigation.stoppingDistance = template.engageDistance;
                targetOfMovement = targetOfAttack.transform.position;
                navigation.SetDestination(targetOfMovement.Value);
                navigation.agentReady = false;
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
                navigation.isStopped = true;
                break;
            case UnitStates.Attacking:
                {
                    navigation.isStopped = true;

                    if (targetOfAttack.attackable.isDead)
                    {
                        commandExecuted = true;
                    }
                    float remainingDistance = Vector3.Distance(transform.position, targetOfMovement.Value);
                    //recalculate path
                    if (Vector3.Distance(targetOfAttack.transform.position, targetOfMovement.Value) > 0.05f)
                    {
                        targetOfMovement = targetOfAttack.transform.position;
                        navigation.SetDestination(targetOfMovement.Value);
                    }
                    //check if in attack range
                    if ((template.engageDistance + targetOfAttack.sizeRadius) < remainingDistance)
                    {
                        switchState = UnitStates.MovingToTarget;
                    }
                    else
                    {
                        FaceTarget();
                        AttackAnim(true);
                    }
                }
                break;
            case UnitStates.MovingToTarget:
                {
                    if (!navigation.agentReady || navigation.pathPending)
                    {
                        break;
                    }
                    if (targetOfAttack.attackable.isDead)
                    {
                        commandExecuted = true;
                    }
                    float remainingDistance = Vector3.Distance(transform.position, targetOfMovement.Value);
                    //recalculate path
                    if (Vector3.Distance(targetOfAttack.transform.position, targetOfMovement.Value) > 0.05f)
                    {
                        targetOfMovement = targetOfAttack.transform.position;
                        navigation.SetDestination(targetOfMovement.Value);
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
                }
                break;
            case UnitStates.MovingToSpot:
                {
                    if (!navigation.agentReady || navigation.pathPending)
                    {
                        break;
                    }
                    float remainingDistance = Vector3.Distance(transform.position, targetOfMovement.Value);
                    if (remainingDistance < 0.1f)
                    {
                        commandExecuted = true;
                    }
                }
                break;
            case UnitStates.Dead:
                break;
            case UnitStates.CustomActionAtPos:
                {
                    switch (customAction.Value)
                    {
                        case AICommand.CustomActions.collectResources:
                            switchState = UnitStates.CustomActionAtObj;
                            break;
                    }
                }
                break;
            case UnitStates.CustomActionAtObj:
                {
                    navigation.isStopped = true;

                    if (targetOfAttack.attackable.isDead)
                    {
                        switch (customAction.Value)
                        {
                            case AICommand.CustomActions.collectResources:
                                ResourceSource resourceSource = targetOfAttack.GetComponent<ResourceSource>();
                                SeekNewResourceSource(resourceSource.resourceType, true);
                                break;
                            case AICommand.CustomActions.dropoffResources:
                                commandExecuted = true;
                                break;
                        }
                    }
                    float remainingDistance = Vector3.Distance(transform.position, targetOfMovement.Value);
                    //recalculate path
                    if (Vector3.Distance(targetOfAttack.transform.position, targetOfMovement.Value) > 0.05f)
                    {
                        targetOfMovement = targetOfAttack.transform.position;
                        navigation.SetDestination(targetOfMovement.Value);
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
                                FaceTarget();
                                AttackAnim(true);
                                break;
                            case AICommand.CustomActions.dropoffResources:
                                if (resourceCollector.isNotEmpty)
                                {
                                    KeyValuePair<ResourceSource.ResourceType, int> resourceBundle = resourceCollector.EmptyStorage();
                                    ResourceDropoff resourceDropoff = targetOfAttack.GetComponent<ResourceDropoff>();
                                    resourceDropoff.DropResource(resourceBundle.Value, resourceBundle.Key);
                                }
                                AICommand getBackCollectingCommand = new AICommand(AICommand.CommandTypes.CustomActionAtPos, transform.position, AICommand.CustomActions.collectResources);
                                AddCommand(getBackCollectingCommand);

                                commandExecuted = true;
                                break;
                        }
                    }
                }
                break;
        }
    }

    private void TransitOutOfState(UnitStates oldState)
    {
        switch (oldState)
        {
            case UnitStates.Idleing:
                break;
            case UnitStates.Attacking:
                AttackAnim(false);
                break;
            case UnitStates.MovingToTarget:
                break;
            case UnitStates.MovingToSpot:
                break;
            case UnitStates.Dead:
                modelHolder.position += Vector3.up * decayIntoGroundDistance;
                break;
            case UnitStates.CustomActionAtPos:
                break;
            case UnitStates.CustomActionAtObj:
                switch (customAction.Value)
                {
                    case AICommand.CustomActions.collectResources:
                        AttackAnim(false);

                        if (resourceCollector != null && resourceCollector.isFull)
                        {
                            Building dropoffBuilding = faction.GetClosestBuildingWithResourceDropoff(transform.position, targetOfAttack.GetComponent<ResourceSource>().resourceType);
                            AICommand dropResourcesCommand = new AICommand(AICommand.CommandTypes.CustomActionAtObj, dropoffBuilding, AICommand.CustomActions.dropoffResources);
                            AddCommand(dropResourcesCommand);
                        }
                        break;
                    default:
                        break;
                }
                break;
        }
    }

    public void TriggerAttackAnimEvent(int Int)///Functionname equals Eventname
    {
        if (state == UnitStates.Dead || targetOfAttack == null || targetOfAttack.attackable.isDead)
        {
            AttackAnim(false);
            return;
        }

        int damage = Random.Range(template.damage.x, template.damage.y + 1);
        if (template.projectile != null)
        {
            ShootProjectileAtTarget(damage);
        }
        else
        {
            bool success = targetOfAttack.GetComponent<Attackable>().SufferAttack(damage, gameObject);
            if (!success)
            {
                AttackAnim(false);

                if (state == UnitStates.CustomActionAtObj && customAction.Value == AICommand.CustomActions.collectResources)
                {
                    AICommand getBackCollectingCommand = new AICommand(AICommand.CommandTypes.CustomActionAtPos, targetOfMovement.Value, AICommand.CustomActions.collectResources);
                    AddCommand(getBackCollectingCommand);
                }
                return;
            }
        }
    }

    private bool SeekNewEnemies()
    {
        IEnumerable<ClickableObject> enemies;

        Collider[] colliders = Physics.OverlapSphere(transform.position, template.guardDistance, InputManager.Instance.unitsLayerMask, QueryTriggerInteraction.Collide);
        enemies = colliders.Select(collider => collider.GetComponent<ClickableObject>()).Where(clickable => clickable != null && !FactionTemplate.IsAlliedWith(faction, clickable.faction));

        ClickableObject closest = null;
        float distanceToClosestSqr = float.PositiveInfinity;
        foreach (var enemy in enemies)
        {
            float distanceSqr = (enemy.transform.position - targetOfMovement.Value).sqrMagnitude;
            if (distanceSqr < distanceToClosestSqr)
            {
                distanceToClosestSqr = distanceSqr;
                closest = enemy;
            }
        }

        if (closest == null)
        {
            return false;
        }

        returnPoint = transform.position;
        targetOfAttack = closest;
        targetOfMovement = targetOfAttack.transform.position;
        return true;
    }

    private bool SeekNewResourceSource(ResourceSource.ResourceType resourceType, bool closeby)
    {
        IEnumerable<ResourceSource> resources;
        if (closeby)
        {
            /// search surroundings for colliders of resource sources
            Collider[] colliders = Physics.OverlapSphere(transform.position, template.guardDistance, InputManager.Instance.unitsLayerMask, QueryTriggerInteraction.Collide);
            resources = colliders.Select(collider => collider.GetComponent<ResourceSource>()).Where(source => source != null && source.resourceType == resourceType);
        }
        else
        {
            resources = resourceCollector.resourceSourcesRegister.GetEnumerable().Select(behavior => behavior as ResourceSource).Where(source => source.resourceType == resourceType);
        }

        ResourceSource closest = null;
        float distanceToClosestSqr = float.PositiveInfinity;
        foreach (var resource in resources)
        {
            float distanceSqr = (resource.transform.position - targetOfMovement.Value).sqrMagnitude;
            if (distanceSqr < distanceToClosestSqr)
            {
                distanceToClosestSqr = distanceSqr;
                closest = resource;
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
            return false;
        }

        targetOfAttack = closest.GetComponent<InteractableObject>();
        targetOfMovement = targetOfAttack.transform.position;
        return true;
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

    public override void Die()
    {
        if (state != UnitStates.Dead)
        {
            TransitOutOfState(state);
            state = UnitStates.Dead;
        }

        base.Die();

        commandExecuted = true;

        commandList.Clear();

        navigation.isStopped = true;
        navigation.enabled = false;

        AttackAnim(false);
        animation.SetTrigger(UnitAnimation.StateNames.DoDeath);

        ///Remove itself from the selection Platoon
        GameManager.Instance.RemoveFromSelection(this);
        SetSelected(false);

        faction.data.units.Remove(this);

        ///Remove unneeded Components
        StartCoroutine(HideSeenThings(visionFadeTime));
        StartCoroutine(VisionFade(visionFadeTime, true));
        ///navMeshAgent.enabled = false;
        StartCoroutine(DecayIntoGround());
    }

    private void SetWalkingSpeed()
    {
        float navMeshAgentSpeed = navigation.velocity.magnitude;
        animation?.SetFloat(UnitAnimation.StateNames.Speed, navMeshAgentSpeed * 0.05f);
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
            if (animation.CheckParameterExistance(UnitAnimation.StateNames.DoCombatReady))
            {
                float value = animation.GetFloat(UnitAnimation.StateNames.DoCombatReady);
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
        return false;
    }

    private IEnumerator LerpCombatReadyAnim(float state)
    {
        float start = animation.GetFloat(UnitAnimation.StateNames.DoCombatReady);
        float key = start;
        float value;
        for (; ; )
        {
            key = Mathf.MoveTowards(key, state, Time.deltaTime * combatReadySwitchTime);
            value = combatReadyAnimCurve.Evaluate(key);
            animation.SetFloat(UnitAnimation.StateNames.DoCombatReady, value);
            if (key != state)
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

    private void AttackAnim(bool state)
    {
        if (state)
        {
            if (lerpingAttackEvent != null)
            {
                return;
            }
            else
            {
                lerpingAttackEvent = StartCoroutine(LerpAttackEvent());
            }
        }
        else if (lerpingAttackEvent != null)
        {
            animation?.SetBool(UnitAnimation.StateNames.DoAttack, false);
            StopCoroutine(lerpingAttackEvent);
            lerpingAttackEvent = null;
        }
    }

    private IEnumerator LerpAttackEvent()
    {
        animation?.SetBool(UnitAnimation.StateNames.DoAttack, false);
        yield return null;
        yield return null;
        animation?.SetBool(UnitAnimation.StateNames.DoAttack, true);

        float lenght = float.NaN;
        if (animation != null)
        {
            lenght = animation.GetCurrentAnimationLenght();
            yield return Yielders.Get(lenght * template.attackEventTime);
        }

        TriggerAttackAnimEvent(0);

        if (animation != null)
        {
            yield return Yielders.Get(lenght * (1f - template.attackEventTime));
        }

        lerpingAttackEvent = null;
    }

#if UNITY_EDITOR
    public List<AICommand> listForEditor { get { return commandList; } }
#endif
}
