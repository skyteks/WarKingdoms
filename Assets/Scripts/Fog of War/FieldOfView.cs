using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Creates mesh from field of view raycasts
/// </summary>
public class FieldOfView : MonoBehaviour
{
    private struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool didHit, Vector3 hitPoint, float rayDistance, float fireAngle)
        {
            hit = didHit;
            point = hitPoint;
            distance = rayDistance;
            angle = fireAngle;
        }
    }

    [Range(0f, 360f)]
    public float viewAngle = 360f;
    public float viewRadius
    {
        get
        {
            return unit != null && unit.template != null ? unit.template.guardDistance : 0f;
        }
    }

    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public List<Transform> lastVisibleTargets = new List<Transform>(0);
    private ClickableObject unit;

    void Awake()
    {
        unit = GetComponentInParent<ClickableObject>();
    }

    void OnEnable()
    {
        StartCoroutine(FindTargetsWithDelay(0.1f));
    }

    private IEnumerator FindTargetsWithDelay(float delay)
    {
        for (; ; )
        {
            yield return Yielders.Get(delay);

            if (unit.attackable.isDead)
            {
                yield break;
            }

            MarkTargetsVisibility();
        }
    }

    public void MarkTargetsVisibility()
    {
        List<Transform> visibleTargets = FindTargets(targetMask, false);

        var goneTargets = lastVisibleTargets.Except(visibleTargets);
        foreach (var unseen in goneTargets)
        {
            if (unseen == null)
            {
                continue;
            }

            if (!FactionTemplate.IsAlliedWith(unit.faction, GameManager.Instance.playerFaction))// if unit not allied to player, ignore
            {
                continue;
            }

            ClickableObject target = unseen.GetComponent<ClickableObject>();
            if (target == null)
            {
                continue;
            }

            if (FactionTemplate.IsAlliedWith(target.faction, GameManager.Instance.playerFaction))// if target allied to player, ignore
            {
                continue;
            }

            if (target.attackable.isDead)
            {
                continue;//Don't hide dead enemies, we wanna see the death anim
            }

            target.SetVisibility(false);
        }
        foreach (var seen in visibleTargets)
        {
            if (!FactionTemplate.IsAlliedWith(unit.faction, GameManager.Instance.playerFaction))// if not allied to player, ignore
            {
                continue;
            }

            ClickableObject target = seen.GetComponent<ClickableObject>();
            if (target == null)
            {
                continue;
            }

            target.SetVisibility(true);
        }

        lastVisibleTargets = visibleTargets;
    }

    private List<Transform> FindTargets(LayerMask mask, bool justInRange)
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, mask);
        List<Transform> visibleTargetsInViewRadius;
        if (justInRange)
        {
            visibleTargetsInViewRadius = targetsInViewRadius.Select(target => target.transform).ToList();
            visibleTargetsInViewRadius.Remove(unit.transform);
            return visibleTargetsInViewRadius;
        }
        else
        {
            visibleTargetsInViewRadius = new List<Transform>(targetsInViewRadius.Length);
        }

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            if (target == unit.transform)
            {
                continue;
            }

            FieldOfView other = target.GetComponentInChildren<FieldOfView>();
            if (other != null)
            {
                target = other.transform;
            }
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (viewAngle == 360f || Vector3.Angle(transform.forward, dirToTarget) < (viewAngle / 2f))
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    visibleTargetsInViewRadius.Add(other != null ? target.GetComponentInParent<ClickableObject>().transform : target);
                    //Debug.DrawRay(transform.position, dirToTarget * distToTarget, Color.cyan, 0.1f);
                }
                else
                {
                    //Debug.DrawRay(transform.position, dirToTarget * distToTarget, Color.red, 0.1f);
                }
            }
        }
        return visibleTargetsInViewRadius;
    }

    public Vector3 DirFromAngle(float angle, bool isGlobal)
    {
        if (!isGlobal)
        {
            angle += transform.eulerAngles.y;
        }
        return new Vector3(angle.Sin(), 0f, angle.Cos()).normalized;
    }
}
