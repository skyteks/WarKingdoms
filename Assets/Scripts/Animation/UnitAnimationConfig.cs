using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Unit Animation Config", menuName = "RTS/Unit Anim Config")]
public class UnitAnimationConfig : ScriptableObject
{

    [SerializeField]
    private AnimationClip idle;
    public AnimationClip clipIdle => idle;

    [SerializeField]
    private AnimationClip walk;
    public AnimationClip clipWalk => walk;

    [Space]

    [SerializeField]
    private AnimationClip idleSelected;
    public AnimationClip clipIdleSelected => idleSelected;
    public bool hasSelectedIdle => idleSelected != null;

    [SerializeField]
    private AnimationClip walkSelected;
    public AnimationClip clipWalkSelected => walkSelected;
    public bool hasSelectedWalk => walkSelected != null;

    [Space]

    [SerializeField]
    private AnimationClip idleCarry1;
    public AnimationClip clipIdleCarry1 => idleCarry1;
    public bool hasCarry1Idle => idleCarry1 != null;

    [SerializeField]
    private AnimationClip walkCarry1;
    public AnimationClip clipWalkCarry1 => walkCarry1;
    public bool hasCarry1Walk => walkCarry1 != null;

    [SerializeField]
    private AnimationClip idleCarry2;
    public AnimationClip clipIdleCarry2 => idleCarry2;
    public bool hasCarry2Idle => idleCarry2 != null;

    [SerializeField]
    private AnimationClip walkCarry2;
    public AnimationClip clipWalkCarry2 => walkCarry2;
    public bool hasCarry2Walk => walkCarry2 != null;

    [Space]

    [SerializeField]
    private AnimationClip attack;
    public AnimationClip clipAttack => attack;

    [Space]

    [SerializeField]
    private AnimationClip attackCarry1;
    public AnimationClip clipAttackCarry1 => attackCarry1;
    public bool hasCarry1Attack => attackCarry1 != null;

    [SerializeField]
    private AnimationClip attackCarry2;
    public AnimationClip clipAttackCarry2 => attackCarry2;
    public bool hasCarry2Attack => attackCarry2 != null;

    [Space]

    [SerializeField]
    private AnimationClip death;
    public AnimationClip clipDeath => death;
}
