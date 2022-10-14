using System.Collections;
using UnityEngine;

/// <summary>
/// Moves and animates the movement command cursor
/// </summary>
public class MovementCursor : MonoBehaviour
{
    private Animation anim;
    private SkinnedMeshRenderer render;
    public float animationSpeed = 4;

    void Start()
    {
        UpdateAnimSpeed();
        render.enabled = false;
    }

    void OnWillRenderObject()
    {
        if (!anim.isPlaying)
        {
            render.enabled = false;
        }
    }

    public void AnimateOnPos(Vector3 pos, Color color)
    {
        transform.parent.position = pos;

        render.material.color = color;
        render.enabled = true;
        anim.Rewind("Play");
        anim.Play("Play", PlayMode.StopAll);
        float lenght = anim.GetClip("Play").length * (1f / animationSpeed);
    }

    private void UpdateAnimSpeed()
    {
        render = GetComponent<SkinnedMeshRenderer>();
        anim = GetComponentInParent<Animation>();
        anim["Play"].speed = animationSpeed;
    }
}
