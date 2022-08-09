using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnitAnimation : MonoBehaviour
{
    [SerializeField]
    private UnitAnimationConfig unitAnimations;

    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            throw new System.NullReferenceException(string.Concat("Animator missing on ", gameObject));
        }
    }

    public float GetCurrentAnimationLenght()
    {
        return animator != null ? animator.GetCurrentAnimatorStateInfo(0).length : 0f;
    }
}
