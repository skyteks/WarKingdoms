using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
public struct UnitAnimator
{
    public enum Clips
    {
        NONE = 0,
        Idle = 1,
        Walk,
        IdleSelected,
        WalkSelected,
        Attack,
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

    public void Configure(Animator animator, UnitAnimationConfig config)
    {
        previousClip = (Clips)0;
        currentClip = (Clips)0;

        graph = PlayableGraph.Create(config.name);
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(graph, System.Enum.GetValues(typeof(Clips)).Length);

        var clip = AnimationClipPlayable.Create(graph, config.clipIdle);
        // No duration, because looping
        clip.Pause();
        mixer.ConnectInput((int)Clips.Idle, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.walkIdle);
        // No duration, because looping
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

    public void PlayIdle(bool selected)
    {
        Clips clipType = selected ? Clips.IdleSelected : Clips.Idle;
        BeginTransition(clipType);
        /*
        previousClip = currentClip;
        currentClip = clipType;
        if (previousClip != currentClip)
        {
            SetWeight(previousClip, 0f);
            GetPlayable(previousClip).Pause();
        }
        SetWeight(clipType, 1f);
        GetPlayable(clipType).Play();
        transitionProgress = -1f;
        */
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