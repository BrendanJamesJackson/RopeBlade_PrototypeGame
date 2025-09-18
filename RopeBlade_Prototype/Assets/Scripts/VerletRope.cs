using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VerletRope : MonoBehaviour
{
    [Header("Rope Settings")]
    public Transform startPoint;     // Anchor (fixed)
    public Transform endPoint;       // Movable end
    public Transform midPoint;       // Optional anchor
    public bool useMidPoint = false; // Toggle midpoint on/off
    [Range(0f, 1f)] public float midAnchorT = 0.5f; // 0 = near start, 1 = near end

    public int segmentCount = 20;    // How many rope segments
    public float segmentLength = 0.1f;
    public int constraintIterations = 20; // Higher = tighter rope

    [Header("Physics Settings")]
    public Vector3 gravity = new Vector3(0, -9.81f, 0);

    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount;

        // Initialize rope points
        Vector3 ropeStart = startPoint.position;
        for (int i = 0; i < segmentCount; i++)
        {
            ropeSegments.Add(new RopeSegment(ropeStart));
            ropeStart.y -= segmentLength; // stack vertically
        }
    }

    private void Update()
    {
        SimulateRope();
        DrawRope();
        DetectCollisions();
        //Debug.Log(GetEndDirection());
    }

    private void SimulateRope()
    {
        // Verlet integration
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            RopeSegment seg = ropeSegments[i];
            Vector3 posBeforeUpdate = seg.posNow;
            seg.posNow += seg.posNow - seg.posOld; // velocity
            seg.posNow += gravity * Time.deltaTime * Time.deltaTime; // gravity
            seg.posOld = posBeforeUpdate;
            ropeSegments[i] = seg;
        }

        // Apply constraints multiple times
        for (int i = 0; i < constraintIterations; i++)
        {
            ApplyConstraints();
        }
    }

    private void ApplyConstraints()
    {
        // --- Lock start
        RopeSegment startSeg = ropeSegments[0];
        startSeg.posNow = startPoint.position;
        ropeSegments[0] = startSeg;

        // --- Lock end
        RopeSegment endSeg = ropeSegments[ropeSegments.Count - 1];
        endSeg.posNow = endPoint.position;
        ropeSegments[ropeSegments.Count - 1] = endSeg;

        // --- Optional adjustable midpoint lock
        if (useMidPoint && midPoint != null)
        {
            int midIndex = Mathf.Clamp(Mathf.RoundToInt(midAnchorT * (ropeSegments.Count - 1)), 1, ropeSegments.Count - 2);
            RopeSegment midSeg = ropeSegments[midIndex];
            midSeg.posNow = midPoint.position;
            ropeSegments[midIndex] = midSeg;
        }

        // --- Enforce segment length
        for (int i = 0; i < ropeSegments.Count - 1; i++)
        {
            RopeSegment segA = ropeSegments[i];
            RopeSegment segB = ropeSegments[i + 1];

            float dist = (segA.posNow - segB.posNow).magnitude;
            float error = Mathf.Abs(dist - segmentLength);
            Vector3 changeDir = (segA.posNow - segB.posNow).normalized;

            Vector3 change = changeDir * error;

            if (i != 0) // don’t move anchor
                segA.posNow -= change * 0.5f;
            if (i + 1 != ropeSegments.Count - 1) // don’t move end
                segB.posNow += change * 0.5f;

            ropeSegments[i] = segA;
            ropeSegments[i + 1] = segB;
        }
    }

    private void DrawRope()
    {
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            lineRenderer.SetPosition(i, ropeSegments[i].posNow);
        }
    }

    private struct RopeSegment
    {
        public Vector3 posNow;
        public Vector3 posOld;

        public RopeSegment(Vector3 pos)
        {
            posNow = pos;
            posOld = pos;
        }
    }

    void DetectCollisions()
    {
        for (int i = 0; i < ropeSegments.Count - 1; i++)
        {
            Vector3 start = ropeSegments[i].posNow;
            Vector3 end = ropeSegments[i + 1].posNow;

            if (Physics.Linecast(start, end, out RaycastHit hit))
            {
                //Debug.Log($"Rope segment {i} hit {hit.collider.name}");
                Debug.DrawLine(start, end, Color.red); // visualize collision
            }
            else
            {
                Debug.DrawLine(start, end, Color.green); // visualize no hit
            }
        }
    }

    public Vector3 GetEndDirection(int smoothSegments = 1)
    {
        if (ropeSegments.Count < 2)
            return Vector3.forward;

        // Clamp smoothing to available segments
        smoothSegments = Mathf.Clamp(smoothSegments, 1, ropeSegments.Count - 1);

        Vector3 accum = Vector3.zero;
        int lastIndex = ropeSegments.Count - 1;

        for (int i = 0; i < smoothSegments; i++)
        {
            Vector3 a = ropeSegments[lastIndex - i - 1].posNow;
            Vector3 b = ropeSegments[lastIndex - i].posNow;

            Vector3 segment = b - a;
            if (segment.sqrMagnitude > Mathf.Epsilon)
                accum += segment.normalized;
        }

        return accum.sqrMagnitude > Mathf.Epsilon ? accum.normalized : Vector3.forward;
    }

}
