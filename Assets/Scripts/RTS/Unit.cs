using System.Collections;
using System.Collections.Generic;
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
    }

    public UnitStates state = UnitStates.Idleing;
    public Transform projectileFirePoint;
    public bool alignToGround;

    //references
    protected Animator animator;
    protected NavMeshAgent navMeshAgent;

    protected List<AICommand> commandList = new List<AICommand>();
    protected bool agentReady = false;
    protected bool commandRecieved, commandExecuted;
    protected UnitStates? switchState;

    protected ClickableObject targetOfAttack;
    protected Vector3? targetOfMovement;

    private readonly float combatReadySwitchTime = 7f;
    private readonly float decayIntoGroundDistance = 2f;

    private Coroutine lerpingCombatReady;

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

    public new static bool IsDeadOrNull(ClickableObject unit)
    {
        return unit == null || ((unit is Unit) ? (unit as Unit).state == UnitStates.Dead : ClickableObject.IsDeadOrNull(unit));
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
            commandExecuted = false;
            commandRecieved = false;
        }
        if (command.commandType != AICommand.CommandType.Stop)
        {
            commandList.Add(command);
        }
    }

    private bool CheckCommandViability(AICommand command)
    {
        //make units be able to denie command... oh what could possibly go wrong
        switch (command.commandType)
        {
            case AICommand.CommandType.MoveTo:
                //case AICommand.CommandType.AttackMoveTo:
                //case AICommand.CommandType.Guard:
                return !command.destination.IsNaN();
            case AICommand.CommandType.AttackTarget:
                return !IsDeadOrNull(command.target) && command.target != this;
            case AICommand.CommandType.Stop:
            case AICommand.CommandType.Die:
                return true;
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
        commandRecieved = true;
        commandExecuted = false;
        switch (command.commandType)
        {
            case AICommand.CommandType.MoveTo:
                targetOfMovement = command.destination;
                TransitIntoState(UnitStates.MovingToSpot);
                break;

            case AICommand.CommandType.Stop:
                TransitIntoState(UnitStates.Idleing);
                break;

            case AICommand.CommandType.AttackTarget:
                targetOfAttack = command.target;
                TransitIntoState(UnitStates.MovingToTarget);
                break;

            case AICommand.CommandType.Die:
                TransitIntoState(UnitStates.Dead);
                break;
        }
    }

    private void TransitIntoState(UnitStates newState)
    {
        state = newState;
        switchState = null;
        switch (newState)
        {
            case UnitStates.Idleing:
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
                break;
            case UnitStates.MovingToSpot:
                navMeshAgent.stoppingDistance = 0.1f;
                navMeshAgent.SetDestination(targetOfMovement.Value);
                navMeshAgent.isStopped = false;
                agentReady = false;
                break;
            case UnitStates.Dead:
                Die();
                break;
        }
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
                        switchState = UnitStates.Attacking;
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
                        commandExecuted = true;
                    }
                    break;
                }
            case UnitStates.Dead:
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
                animator?.SetBool("DoAttack", false);
                break;
            case UnitStates.MovingToTarget:
                break;
            case UnitStates.MovingToSpot:
                break;
            case UnitStates.Dead:
                modelHolder.position += Vector3.up * decayIntoGroundDistance;
                break;
        }
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

        animator?.SetTrigger("DoDeath");

        //Remove itself from the selection Platoon
        GameManager.Instance.RemoveFromSelection(this);
        SetSelected(false);

        faction.units.Remove(this);

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

    private IEnumerator DecayIntoGround()
    {
        yield return Yielders.Get(5f);
        float startY = transform.position.y;
        while (modelHolder.position.y > startY - decayIntoGroundDistance)
        {
            modelHolder.position += Vector3.down * Time.deltaTime * 0.1f;
            yield return null;
        }
        Destroy(gameObject);
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

    public void TriggerAttackAnimEvent(int Int)//Functionname equals Eventname
    {
        if (state == UnitStates.Dead || IsDeadOrNull(targetOfAttack))
        {
            //already dead
            animator.SetBool("DoAttack", false);
            return;
        }

        int damage = Random.Range(template.damage.x, template.damage.y + 1);
        if (template.projectile != null)
        {
            ShootProjectileAtTarget(damage);
        }
        else
        {
            targetOfAttack.SufferAttack(damage);
        }
    }

    private void ShootProjectileAtTarget(int damage)
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
        projectileInstance.LaunchAt(targetOfAttack.fieldOfView.transform, damage, this);
    }

    //called by an attacker
    public override void SufferAttack(int damage)
    {
        if (state == UnitStates.Dead)
        {
            return;
        }

        base.SufferAttack(damage);
    }

#if UNITY_EDITOR
    public AICommand[] GetCommandList()
    {
        return commandList.ToArray();
    }
#endif
}
