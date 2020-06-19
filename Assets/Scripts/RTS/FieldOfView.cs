using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Creates mesh from field of view raycasts
/// </summary>
[RequireComponent(typeof(MeshFilter))]
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

    private struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 firstPoint, Vector3 secondPoint)
        {
            pointA = firstPoint;
            pointB = secondPoint;
        }
    }

    public ClickableObject unit;

    [Range(0f, 360f)]
    public float viewAngle = 360f;
    public float viewRadius
    {
        get
        {
            return unit.template.guardDistance;
        }
    }

    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public float meshResolution;
    public int edgeResolveIterations;
    [Range(0f, 90f)]
    public float edgeDistanceTreshholdDegrees = 80f;
    [Range(0f, 1f)]
    public float maskCutawayDistance;
    public bool maskCutawayHorizontalyOnly;
    public List<Transform> lastVisibleTargets = new List<Transform>(0);
    private List<Transform> lastObstacles = new List<Transform>(0);
    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;
    private Vector3 lastPosition;
    private Vector3 lastForward;

    void Awake()
    {
        viewMeshFilter = GetComponent<MeshFilter>();
    }

    private void OnEnable()
    {
        MeshRenderer render = GetComponent<MeshRenderer>();
        if (render != null) render.enabled = true;

        StartCoroutine(FindTargetsWithDelay(0.1f));
    }

    private void OnDisable()
    {
        MeshRenderer render = GetComponent<MeshRenderer>();
        if (render != null) render.enabled = false;
    }

    void Start()
    {
        SetMesh();
    }

    void LateUpdate()
    {
        if (ShouldRegenerateMesh())
        {
            GenerateMesh();
        }
    }

    void OnDrawGizmosSelected()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngle = viewAngle / stepCount;

        for (int i = 0; i < stepCount + 1; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2f + stepAngle * i;
            Vector3 dir = DirFromAngle(angle, true);
            Debug.DrawRay(transform.position, dir * viewRadius, Color.blue);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Create Mesh")]
    private void CreateMesh()
    {
        if (Application.isPlaying) return;
        if (viewMeshFilter == null) viewMeshFilter = GetComponent<MeshFilter>();
        if (viewMeshFilter.sharedMesh == null || viewMeshFilter.mesh == null || viewMesh == null) SetMesh();
        GenerateMesh();
    }
#endif

    private bool ShouldRegenerateMesh()
    {
        if (obstacleMask == obstacleMask.ToEverything()) return true;
        if (obstacleMask == obstacleMask.ToNothing()) return false;

        bool draw = false;

        List<Transform> obstacles = null;
        if (!draw)
        {
            obstacles = FindTargets(obstacleMask, true);

            draw = lastObstacles.Count != obstacles.Count;
            if (!draw)
            {
                draw = !obstacles.ScrambledEqualsHashSet(lastObstacles);
            }
        }
        if (draw)
        {
            lastObstacles = obstacles;
        }

        if (!draw)
        {
            if (obstacles.Count > 1)
            {
                draw |= Vector3.Distance(lastPosition, transform.position) > 0.01f;
                draw |= Vector3.Angle(lastForward, transform.forward) > 0.1f;
            }
        }
        if (draw)
        {
            lastPosition = transform.position;
            lastForward = transform.forward;
        }

        return draw;
    }

    private void SetMesh()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh (Generated)";
        if (Application.isPlaying) viewMeshFilter.mesh = viewMesh;
        else viewMeshFilter.sharedMesh = viewMesh;
    }

    private IEnumerator FindTargetsWithDelay(float delay)
    {
        for (; ; )
        {
            yield return Yielders.Get(delay);
            if (!FactionTemplate.IsAlliedWith(unit.faction, GameManager.Instance.playerFaction)) continue;
            if (unit.IsDeadOrNull(unit)) yield break;

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
            ClickableObject unit = unseen.GetComponent<ClickableObject>();
            if (unit == null) continue;
            if (FactionTemplate.IsAlliedWith(unit.faction, GameManager.Instance.playerFaction)) continue;
            unit.SetVisibility(false);
        }
        foreach (var seen in visibleTargets)
        {
            ClickableObject unit = seen.GetComponent<ClickableObject>();
            if (unit == null) continue;
            if (FactionTemplate.IsAlliedWith(unit.faction, GameManager.Instance.playerFaction)) continue;
            unit.SetVisibility(true);
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
        else visibleTargetsInViewRadius = new List<Transform>(targetsInViewRadius.Length);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            if (target == unit.transform) continue;
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

    private void GenerateMesh()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngle = viewAngle / stepCount;
        float edgeDistanceTreshhold = (viewRadius * Mathf.PI / 360f) * stepAngle * edgeDistanceTreshholdDegrees.Tan();
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i < stepCount + 1; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2f + stepAngle * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDistanceTreshholdExceeded = Mathf.Abs(oldViewCast.distance - newViewCast.distance) > edgeDistanceTreshhold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistanceTreshholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] indices = new int[(vertexCount - 2) * 3];
        Vector2[] uvs = new Vector2[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];

        vertices[0] = Vector3.zero;
        uvs[0] = Vector2.zero;
        normals[0] = Vector3.up;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            Vector3 vertex = transform.InverseTransformPoint(viewPoints[i]);
            vertices[i + 1] = vertex;
            uvs[i + 1] = vertex.ToVector2XZ() / viewRadius / 2f;
            normals[i + 1] = Vector3.up;
            if (i < vertexCount - 2)
            {
                indices[i * 3] = 0;
                indices[i * 3 + 1] = i + 1; ;
                indices[i * 3 + 2] = i + 2; ;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = indices;
        viewMesh.uv = uvs;
        viewMesh.normals = normals;
        //viewMesh.RecalculateNormals();

        //Debug.Log("Regenerated mesh on " + transform.parent.gameObject, transform.parent.gameObject);
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngle = viewAngle / stepCount;
        float edgeDistanceTreshhold = (viewRadius * Mathf.PI / 360f) * stepAngle * edgeDistanceTreshholdDegrees.Tan();

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2f;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDistanceTreshholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDistanceTreshhold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceTreshholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            //Debug.DrawLine(transform.position, hit.point, Color.black);
            Vector3 cutaway = -hit.normal.ToScale(new Vector3(1f, 1f, (!maskCutawayHorizontalyOnly).ToFloat())) * maskCutawayDistance;
            return new ViewCastInfo(true, hit.point + cutaway, hit.distance, globalAngle);
        }
        else
        {
            //Debug.DrawRay(transform.position, dir * viewRadius, Color.black);
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
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
