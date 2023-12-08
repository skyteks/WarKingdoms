using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class UnitAnimation : MonoBehaviour
{
    [SerializeField]
    private UnitAnimationConfig unitAnimations;

    private Animator animator;
    [SerializeField, ReadOnly]
    private UnitAnimator unitAnimator;

    private static readonly AnimationCurve easeInEaseOutCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f));

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            throw new System.NullReferenceException(string.Concat("Animator missing on ", gameObject));
        }
        unitAnimator = new UnitAnimator();
    }

    void Start()
    {
        unitAnimator.Configure(animator, unitAnimations);
    }

    void OnDestroy()
    {
        unitAnimator.Destroy();
    }

    void Update()
    {
        unitAnimator.GameUpdate();
    }

    public float GetCurrentAnimationLenght()
    {
        return animator != null ? animator.GetCurrentAnimatorStateInfo(0).length : 0f;
    }

    public void Idle(bool selected = false, int carry = 0)
    {
        unitAnimator.PlayIdle(selected, carry);
    }

    public void Walk(float speed, bool selected = false, int carry = 0)
    {
        unitAnimator.PlayWalk(speed, selected, carry);
    }

    public void Attack(int carry = 0)
    {
        unitAnimator.PlayAttack(carry);
    }

    public void Death()
    {
        unitAnimator.PlayDeath();
    }
}
