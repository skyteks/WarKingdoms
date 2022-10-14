using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnitAnimation : MonoBehaviour
{
    [SerializeField]
    private UnitAnimationConfig unitAnimations;

    private Animator animator;

    private static readonly AnimationCurve easeInEaseOutCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f));

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
