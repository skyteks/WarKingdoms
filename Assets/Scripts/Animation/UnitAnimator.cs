using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
public struct UnitAnimator
{

    public enum Clip
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

    Clip previousClip;

    float transitionProgress;

#if UNITY_EDITOR
    double clipTime;
#endif

    public Clip CurrentClip { get; private set; }

    public bool IsDone => GetPlayable(CurrentClip).IsDone();

#if UNITY_EDITOR
    public bool IsValid => graph.IsValid();
#endif

    public void Configure(Animator animator, UnitAnimationConfig config)
    {

        graph = PlayableGraph.Create();
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(graph, System.Enum.GetValues(typeof(Clip)).Length);



        var clip = AnimationClipPlayable.Create(graph, config.clipIdle);
        //clip.SetDuration(config.clipIdle.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Idle, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.walkIdle);
        //clip.SetDuration(config.walkIdle.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Walk, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.attackIdle);
        clip.SetDuration(config.attackIdle.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Attack, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.deathIdle);
        clip.SetDuration(config.deathIdle.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Death, clip, 0);

        if (config.hasSelectedIdle)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipIdleSelected);
            //clip.SetDuration(config.clipIdleSelected.length);
            clip.Pause();
            mixer.ConnectInput((int)Clip.IdleSelected, clip, 0);
        }

        if (config.hasSelectedWalk)
        {
            clip = AnimationClipPlayable.Create(graph, config.clipWalkSelected);
            //clip.SetDuration(config.clipWalkSelected.length);
            clip.Pause();
            mixer.ConnectInput((int)Clip.WalkSelected, clip, 0);
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
    }

    public void PlayIdle()
    {
        SetWeight(Clip.Idle, 1f);
        GetPlayable(Clip.Idle).Play();
        CurrentClip = Clip.Idle;
        graph.Play();
        transitionProgress = -1f;
    }

    public void PlayWalk(float speed)
    {
        GetPlayable(Clip.Walk).SetSpeed(speed);
        BeginTransition(Clip.Walk);
    }

    public void PlayAttack()
    {
        BeginTransition(Clip.Attack);
    }

    public void PlayDeath()
    {
        BeginTransition(Clip.Death);
    }

    public void Stop()
    {
        graph.Stop();
    }

    public void Destroy()
    {
        graph.Destroy();
    }

    void BeginTransition(Clip nextClip)
    {
        previousClip = CurrentClip;
        CurrentClip = nextClip;
        transitionProgress = 0f;
        GetPlayable(nextClip).Play();
    }

    Playable GetPlayable(Clip clip)
    {
        return mixer.GetInput((int)clip);
    }

    void SetWeight(Clip clip, float weight)
    {
        mixer.SetInputWeight((int)clip, weight);
    }
}