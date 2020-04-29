using UnityEngine;

[RequireComponent(typeof(Animation))]
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

    public void AnimateOnPos(Vector3 pos, Color color)
    {
        //transform.SetPositionAndRotation(pos, Quaternion.LookRotation(Quaternion.FromToRotation(Vector3.up, Vector3.forward) * up, up));
        transform.position = pos;

        //Material mat = render.material;
        //mat.color = color;
        render.material.color = color;
        render.enabled = true;
        anim.Rewind("Play");
        anim.Play("Play", PlayMode.StopAll);
        float lenght = anim.GetClip("Play").length * (1f / animationSpeed);
    }

    private void UpdateAnimSpeed()
    {
        render = GetComponentInChildren<SkinnedMeshRenderer>(true);
        anim = GetComponent<Animation>();
        anim["Play"].speed = animationSpeed;
    }
}
