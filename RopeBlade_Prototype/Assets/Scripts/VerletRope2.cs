using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VerletRope2 : MonoBehaviour
{
    [Header("Rope Setup")]
    public Rigidbody rbA;                     // First rigidbody (anchor A)
    public Rigidbody rbB;                     // Second rigidbody (anchor B)
    public int segmentCount = 20;             // Rope resolution
    public float ropeLength = 5f;             // Initial max rope length
    public int constraintIterations = 10;     // Higher = stiffer rope
    public float damping = 0.99f;             // Velocity damping (0-1)

    private LineRenderer line;
    private Vector3[] positions;
    private Vector3[] prevPositions;
    private float segmentLength;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        InitializeRope();
    }

    void FixedUpdate()
    {
        Simulate(Time.fixedDeltaTime);
        ApplyConstraints();
        EnforceMaxLength();
        DetectCollisions();   // <-- Added from VerletRope
        UpdateLineRenderer();
    }

    // ---------------- Core Verlet Simulation ----------------
    void InitializeRope()
    {
        positions = new Vector3[segmentCount];
        prevPositions = new Vector3[segmentCount];

        // Compute segment length based on ropeLength
        segmentLength = ropeLength / (segmentCount - 1);

        Vector3 start = rbA.position;
        Vector3 end = rbB.position;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            positions[i] = Vector3.Lerp(start, end, t);
            prevPositions[i] = positions[i];
        }

        line.positionCount = segmentCount;
    }

    void Simulate(float deltaTime)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 current = positions[i];
            Vector3 velocity = (positions[i] - prevPositions[i]) * damping;

            prevPositions[i] = current;
            positions[i] = current + velocity;
            positions[i] += Physics.gravity * deltaTime * deltaTime; // gravity effect
        }
    }

    void ApplyConstraints()
    {
        for (int iter = 0; iter < constraintIterations; iter++)
        {
            for (int i = 0; i < positions.Length - 1; i++)
            {
                Vector3 delta = positions[i + 1] - positions[i];
                float dist = delta.magnitude;
                float error = dist - segmentLength;
                Vector3 correction = delta.normalized * (error * 0.5f);

                if (i != 0) positions[i] += correction;
                else positions[i] = rbA.position; // anchor to rbA

                if (i + 1 != positions.Length - 1) positions[i + 1] -= correction;
                else positions[i + 1] = rbB.position; // anchor to rbB
            }
        }
    }

    void EnforceMaxLength()
    {
        float maxDist = ropeLength;
        Vector3 dir = rbB.position - rbA.position;
        if (dir.magnitude > maxDist)
        {
            Vector3 clampedPos = rbA.position + dir.normalized * maxDist;
            rbB.MovePosition(clampedPos);
            positions[positions.Length - 1] = rbB.position;
        }
    }

    void UpdateLineRenderer()
    {
        line.SetPositions(positions);
    }

    // ---------------- Collision Detection (from VerletRope) ----------------
    void DetectCollisions()
    {
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 start = positions[i];
            Vector3 end = positions[i + 1];

            if (Physics.Linecast(start, end, out RaycastHit hit))
            {
                // Example: you could apply forces here or react to collision
                Debug.DrawLine(start, end, Color.red);
            }
            else
            {
                Debug.DrawLine(start, end, Color.green);
            }
        }
    }

    // ---------------- End Direction (from VerletRope) ----------------
    public Vector3 GetEndDirection(int smoothSegments = 1)
    {
        if (positions.Length < 2)
            return Vector3.forward;

        // Clamp smoothing to available segments
        smoothSegments = Mathf.Clamp(smoothSegments, 1, positions.Length - 1);

        Vector3 accum = Vector3.zero;
        int lastIndex = positions.Length - 1;

        for (int i = 0; i < smoothSegments; i++)
        {
            Vector3 a = positions[lastIndex - i - 1];
            Vector3 b = positions[lastIndex - i];

            Vector3 segment = b - a;
            if (segment.sqrMagnitude > Mathf.Epsilon)
                accum += segment.normalized;
        }

        return accum.sqrMagnitude > Mathf.Epsilon ? accum.normalized : Vector3.forward;
    }

    // ---------------- Public API ----------------
    /// <summary>
    /// Dynamically sets the max rope length at runtime.
    /// </summary>
    public void SetRopeLength(float newLength)
    {
        ropeLength = Mathf.Max(0.1f, newLength); // prevent zero-length rope
        segmentLength = ropeLength / (segmentCount - 1);
    }
}
