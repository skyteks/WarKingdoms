using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnitAnimation : MonoBehaviour
{
    public enum StateNames
    {
        DoAttack,
        DoDeath,
        DoCombatReady,
        Speed,
    }

    public readonly int hashDoAttack = Animator.StringToHash("DoAttack");
    public readonly int hashDoDeath = Animator.StringToHash("DoDeath");
    public readonly int hashDoCombatReady = Animator.StringToHash("DoCombatReady");
    public readonly int hashSpeed = Animator.StringToHash("Speed");

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            throw new System.NullReferenceException(string.Concat("Animator missing on ", gameObject.gameObject));
        }
    }

    private int GetHash(StateNames stateName)
    {
        switch (stateName)
        {
            case StateNames.DoAttack:
                return hashDoAttack;
            case StateNames.DoDeath:
                return hashDoDeath;
            case StateNames.DoCombatReady:
                return hashDoCombatReady;
            case StateNames.Speed:
                return hashSpeed;
        }
        throw new System.ArgumentException();
    }

    public void SetBool(StateNames stateName, bool state)
    {
        int hash = GetHash(stateName);
        animator?.SetBool(hash, state);
    }

    public void SetFloat(StateNames stateName, float value)
    {
        int hash = GetHash(stateName);
        animator?.SetFloat(hash, value);
    }

    public void SetTrigger(StateNames stateName)
    {
        int hash = GetHash(stateName);
        animator?.SetTrigger(hash);
    }

    public bool GetBool(StateNames stateName)
    {
        int hash = GetHash(stateName);
        return animator != null ? animator.GetBool(hash) : false;
    }

    public float GetFloat(StateNames stateName)
    {
        int hash = GetHash(stateName);
        return animator != null ? animator.GetFloat(hash) : 0f;
    }

    public bool CheckParameterExistance(StateNames stateName)
    {
        if (animator != null)
        {
            int hash = GetHash(stateName);
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == hash)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public float GetCurrentAnimationLenght()
    {
        return animator != null ? animator.GetCurrentAnimatorStateInfo(0).length : 0f;
    }
}
