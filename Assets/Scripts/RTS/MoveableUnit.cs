using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveableUnit : Unit
{
    /*
    public enum UnitStates
    {
        Idleing,
        Attacking,
        MovingToTarget,
        MovingToSpot,
        Dead,
    }
    */

    //public UnitStates state = UnitStates.Idleing;

    //references
    //protected Animator animator;
    //protected NavMeshAgent navMeshAgent;

    //protected List<AICommand> commandList = new List<AICommand>();
    //protected bool agentReady = false;
    //protected bool commandRecieved, commandExecuted;
    protected UnitStates? switchState;

    //protected ClickableObject targetOfAttack;
    protected Vector3? targetOfMovement;

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
        if (navMeshAgent != null
            && navMeshAgent.isOnNavMesh
            && navMeshAgent.hasPath)
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

    public new void AddCommand(AICommand command, bool clear = false)
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
                return;
            }
            else
            {
                switchState = UnitStates.Idleing;
                TransitOutOfState(state);
                TransitIntoState(switchState.Value);
                return;
            }
        }
        if (commandRecieved && !commandExecuted && switchState.HasValue)
        {
            TransitOutOfState(state);
            TransitIntoState(switchState.Value);
            return;
        }
    }

    private void ExecuteCommand(AICommand command)
    {
        command.origin = transform.position;

        targetOfMovement = null;
        targetOfAttack = null;
        switchState = null;
        commandRecieved = true;
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
                break;

            case AICommand.CommandType.Die:
                TransitIntoState(UnitStates.Dead);
                break;
        }
    }

    private void TransitIntoState(UnitStates newState)
    {
        switchState = null;
        switch (newState)
        {
            case UnitStates.Idleing:
                break;
            case UnitStates.Attacking:
                animator?.SetBool("DoAttack", true);
                break;
            case UnitStates.MovingToTarget:
                navMeshAgent.stoppingDistance = targetOfAttack.sizeRadius;
                targetOfMovement = targetOfAttack.transform.position;
                navMeshAgent.SetDestination(targetOfMovement.Value);
                break;
            case UnitStates.MovingToSpot:
                navMeshAgent.stoppingDistance = 0f;
                navMeshAgent.SetDestination(targetOfMovement.Value);
                break;
            case UnitStates.Dead:
                Die();
                break;
        }
        state = newState;
    }

    private void UpdateState(UnitStates currentState)
    {
        switch (currentState)
        {
            case UnitStates.Idleing:
                break;
            case UnitStates.Attacking:
                {
                    if (IsDeadOrNull(targetOfAttack))
                    {
                        commandExecuted = true;
                    }
                    //check if in attack range
                    if ((template.engageDistance + targetOfAttack.sizeRadius) < navMeshAgent.remainingDistance)
                    {
                        switchState = UnitStates.MovingToTarget;
                    }
                    break;
                }
            case UnitStates.MovingToTarget:
                {
                    if (IsDeadOrNull(targetOfAttack))
                    {
                        commandExecuted = true;
                    }
                    //recalculate path
                    if (Vector3.Distance(targetOfAttack.transform.position, targetOfMovement.Value) > 0.05f)
                    {
                        targetOfMovement = targetOfAttack.transform.position;
                        navMeshAgent.SetDestination(targetOfMovement.Value);
                    }
                    //check if in attack range
                    if ((template.engageDistance + targetOfAttack.sizeRadius) >= navMeshAgent.remainingDistance)
                    {
                        switchState = UnitStates.MovingToTarget;
                    }
                    break;
                }
            case UnitStates.MovingToSpot:
                break;
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
                break;
        }
    }

    protected override void Die()
    {
        base.Die();
        return;

        commandExecuted = true;

        AdjustModelAngleToGround();

        commandList.Clear();

        if (animator != null)
        {
            animator.SetTrigger("DoDeath");
        }

        //Remove itself from the selection Platoon
        GameManager.Instance.RemoveFromSelection(this);
        SetSelected(false);

        faction.units.Remove(this);

        //Remove unneeded Components
        StartCoroutine(HideSeenThings(visionFadeTime));
        StartCoroutine(VisionFade(visionFadeTime, true));
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

    public new void AdjustModelAngleToGround()
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
}
