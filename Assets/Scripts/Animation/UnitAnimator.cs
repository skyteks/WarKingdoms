using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
public struct UnitAnimator
{
    public enum Clips
    {
        NONE = 0,
        Idle,
        Walk,
        IdleSelected,
        WalkSelected,
        IdleCarry1,
        WalkCarry1,
        IdleCarry2,
        WalkCarry2,
        Attack,
        AttackCarry1,
        AttackCarry2,
        Death,
    }

    const float transitionSpeed = 5f;

    PlayableGraph graph;

    AnimationMixerPlayable mixer;

    [SerializeField, ReadOnly]
    float transitionProgress;

#if UNITY_EDITOR
    [SerializeField, ReadOnly]
    double clipTime;
#endif

    [SerializeField, ReadOnly]
    private Clips previousClip;
    [SerializeField, ReadOnly]
    private Clips currentClip;

    public bool IsDone => GetPlayable(currentClip).IsDone();

#if UNITY_EDITOR
    public bool IsValid => graph.IsValid();
#endif

    private UnitAnimationConfig animationConfig;

    public void Configure(Animator animator, UnitAnimationConfig config)
    {
        animationConfig = config;

        previousClip = (Clips)0;
        currentClip = (Clips)0;

        graph = PlayableGraph.Create(config.name);
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(graph, System.Enum.GetValues(typeof(Clips)).Length);

        var clip = AnimationClipPlayable.Create(graph, config.clipIdle);
        // No duration, because looping
        clip.Pause();
        mixer.ConnectInput((int)Clips.Idle, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.clipWalk);
        // No duration, because looping
        clip.Pause();
        mixer.ConnectInput((int)Clips.Walk, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.clipAttack);
        clip.SetDuration(config.clipAttack.length);
        clip.Pause();
        mixer.ConnectInput((int)Clips.Attack, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.clipDeath);
        clip.SetDuration(config.clipDeath.length);
        clip.Pause();
        mixer.ConnectInput((int)Clips.Death, clip, 0);

        if (config.hasSelectedIdle)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipIdleSelected);
            // No duration, because looping
            clip.Pause();
            mixer.ConnectInput((int)Clips.IdleSelected, clip, 0);
        }

        if (config.hasSelectedWalk)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipWalkSelected);
            // No duration, because looping
            clip.Pause();
            mixer.ConnectInput((int)Clips.WalkSelected, clip, 0);
        }

        if (config.hasCarry1Idle)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipIdleCarry1);
            // No duration, because looping
            clip.Pause();
            mixer.ConnectInput((int)Clips.IdleCarry1, clip, 0);
        }

        if (config.hasCarry1Walk)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipWalkCarry1);
            // No duration, because looping
            clip.Pause();
            mixer.ConnectInput((int)Clips.WalkCarry1, clip, 0);
        }

        if (config.hasCarry1Attack)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipAttackCarry1);
            clip.SetDuration(config.clipAttackCarry1.length);
            clip.Pause();
            mixer.ConnectInput((int)Clips.AttackCarry1, clip, 0);
        }

        if (config.hasCarry2Idle)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipIdleCarry2);
            // No duration, because looping
            clip.Pause();
            mixer.ConnectInput((int)Clips.IdleCarry2, clip, 0);
        }

        if (config.hasCarry2Walk)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipWalkCarry2);
            // No duration, because looping
            clip.Pause();
            mixer.ConnectInput((int)Clips.WalkCarry2, clip, 0);
        }

        if (config.hasCarry2Attack)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipAttackCarry2);
            clip.SetDuration(config.clipAttackCarry2.length);
            clip.Pause();
            mixer.ConnectInput((int)Clips.AttackCarry2, clip, 0);
        }

        var output = AnimationPlayableOutput.Create(graph, "Enemy", animator);
        output.SetSourcePlayable(mixer);
    }

    public void GameUpdate()
    {
        if (transitionProgress >= 0f)
        {
            transitionProgress += Time.deltaTime * transitionSpeed;
            if (transitionProgress >= 1f)
            {
                transitionProgress = -1f;
                SetWeight(currentClip, 1f);
                SetWeight(previousClip, 0f);
                GetPlayable(previousClip).Pause();
            }
            else
            {
                SetWeight(currentClip, transitionProgress);
                SetWeight(previousClip, 1f - transitionProgress);
            }
        }
#if UNITY_EDITOR
        clipTime = GetPlayable(currentClip).GetTime();
#endif
        graph.Play();
    }

    public void PlayIdle(bool selected, int carry)
    {
        Clips clipType;
        if (animationConfig.hasSelectedIdle)
        {
            clipType = selected ? Clips.IdleSelected : Clips.Idle;
        }
        else if (animationConfig.hasCarry1Idle && animationConfig.hasCarry2Idle)
        {
            switch (carry)
            {
                default:
                case 0:
                    clipType = Clips.Idle;
                    break;
                case 1:
                    clipType = Clips.IdleCarry1;
                    break;
                case 2:
                    clipType = Clips.IdleCarry2;
                    break;
            }
        }
        else
        {
            clipType = Clips.Idle;
        }
        BeginTransition(clipType);
    }

    public void PlayWalk(float speed, bool selected, int carry)
    {
        Clips clipType;
        if (animationConfig.hasSelectedWalk)
        {
            clipType = selected ? Clips.WalkSelected : Clips.Walk;
        }
        else if (animationConfig.hasCarry1Walk && animationConfig.hasCarry2Walk)
        {
            switch (carry)
            {
                default:
                case 0:
                    clipType = Clips.Idle;
                    break;
                case 1:
                    clipType = Clips.IdleCarry1;
                    break;
                case 2:
                    clipType = Clips.IdleCarry2;
                    break;
            }
        }
        else
        {
            clipType = Clips.Idle;
        }
        GetPlayable(clipType).SetSpeed(speed);
        BeginTransition(clipType);
    }

    public void PlayAttack(int carry)
    {
        Clips clipType;
        if (animationConfig.hasCarry1Attack && animationConfig.hasCarry2Attack)
        {
            switch (carry)
            {
                default:
                case 0:
                    clipType = Clips.Attack;
                    break;
                case 1:
                    clipType = Clips.AttackCarry1;
                    break;
                case 2:
                    clipType = Clips.AttackCarry2;
                    break;
            }
        }
        else
        {
            clipType = Clips.Attack;
        }
        BeginTransition(clipType);
    }

    public void PlayDeath()
    {
        BeginTransition(Clips.Death);
    }

    private void BeginTransition(Clips nextClip)
    {
        if (nextClip == currentClip)
        {
            return;
        }
        previousClip = currentClip;
        currentClip = nextClip;
        transitionProgress = 0f;
        GetPlayable(nextClip).Play();
    }

    private Playable GetPlayable(Clips clip)
    {
        return mixer.GetInput((int)clip);
    }

    private void SetWeight(Clips clip, float weight)
    {
        mixer.SetInputWeight((int)clip, weight);
    }

    public void Stop()
    {
        graph.Stop();
    }

    public void Destroy()
    {
        graph.Destroy();
    }
}