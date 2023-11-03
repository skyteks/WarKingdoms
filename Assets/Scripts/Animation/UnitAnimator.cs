using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
public struct UnitAnimator
{
    public enum Clips
    {
        Idle,
        Walk,
        IdleSelected,
        WalkSelected,
        Attack,
        Death,
    }

    const float transitionSpeed = 5f;

    PlayableGraph graph;

    AnimationMixerPlayable mixer;

    Clips previousClip;

    float transitionProgress;

#if UNITY_EDITOR
    double clipTime;
#endif

    public Clips CurrentClip { get; private set; }

    public bool IsDone => GetPlayable(CurrentClip).IsDone();

#if UNITY_EDITOR
    public bool IsValid => graph.IsValid();
#endif

    public void Configure(Animator animator, UnitAnimationConfig config)
    {
        graph = PlayableGraph.Create(config.name);
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(graph, System.Enum.GetValues(typeof(Clips)).Length);

        var clip = AnimationClipPlayable.Create(graph, config.clipIdle);
        // No duration, because looping
        //clip.SetDuration(config.clipIdle.length);
        clip.Pause();
        mixer.ConnectInput((int)Clips.Idle, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.walkIdle);
        // No duration, because looping
        //clip.SetDuration(config.walkIdle.length);
        clip.Pause();
        mixer.ConnectInput((int)Clips.Walk, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.attackIdle);
        clip.SetDuration(config.attackIdle.length);
        clip.Pause();
        mixer.ConnectInput((int)Clips.Attack, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.deathIdle);
        clip.SetDuration(config.deathIdle.length);
        clip.Pause();
        mixer.ConnectInput((int)Clips.Death, clip, 0);

        if (config.hasSelectedIdle)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipIdleSelected);
            // No duration, because looping
            //clip.SetDuration(config.clipIdleSelected.length);
            clip.Pause();
            mixer.ConnectInput((int)Clips.IdleSelected, clip, 0);
        }

        if (config.hasSelectedWalk)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipWalkSelected);
            // No duration, because looping
            //clip.SetDuration(config.clipWalkSelected.length);
            clip.Pause();
            mixer.ConnectInput((int)Clips.WalkSelected, clip, 0);
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
                SetWeight(CurrentClip, 1f);
                SetWeight(previousClip, 0f);
                GetPlayable(previousClip).Pause();
            }
            else
            {
                SetWeight(CurrentClip, transitionProgress);
                SetWeight(previousClip, 1f - transitionProgress);
            }
        }
#if UNITY_EDITOR
        clipTime = GetPlayable(CurrentClip).GetTime();
#endif
        graph.Play();
    }

    public void PlayIdle(bool selected)
    {
        Clips clipType = selected ? Clips.IdleSelected : Clips.Idle;
        SetWeight(clipType, 1f);
        // no transition
        GetPlayable(clipType).Play();
        CurrentClip = clipType;
        transitionProgress = -1f;
    }

    public void PlayWalk(float speed, bool selected)
    {
        Clips clipType = selected ? Clips.WalkSelected : Clips.Walk;
        GetPlayable(clipType).SetSpeed(speed);
        BeginTransition(clipType);
    }

    public void PlayAttack()
    {
        BeginTransition(Clips.Attack);
    }

    public void PlayDeath()
    {
        BeginTransition(Clips.Death);
    }

    private void BeginTransition(Clips nextClip)
    {
        previousClip = CurrentClip;
        CurrentClip = nextClip;
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