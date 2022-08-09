using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Unit Animation Config", menuName = "RTS/Unit Anim Config")]
public class UnitAnimationConfig : ScriptableObject
{
    [Header("Parameters")]

    private float movementSpeed;
    private bool combatReady;

    public bool hasSelectedIdle => idleSelected != null;
    public bool hasSelectedWalk => walkSelected != null;

    [Header("Animation Clips")]

    [SerializeField]
    private AnimationClip idle;
    public AnimationClip clipIdle => idle;

    [SerializeField]
    private AnimationClip walk;
    public AnimationClip walkIdle => walk;

    [Space]

    [SerializeField]
    private AnimationClip idleSelected;
    public AnimationClip clipIdleSelected => idleSelected;

    [SerializeField]
    private AnimationClip walkSelected;
    public AnimationClip clipWalkSelected => walkSelected;

    [Space]

    [SerializeField]
    private AnimationClip attack;
    public AnimationClip attackIdle => attack;

    [Space]

    [SerializeField]
    private AnimationClip death;
    public AnimationClip deathIdle => death;
}
