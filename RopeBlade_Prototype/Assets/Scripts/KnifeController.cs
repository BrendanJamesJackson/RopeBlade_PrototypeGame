using UnityEngine;

public class KnifeController : MonoBehaviour
{
    public Transform target;          // Point to orbit around
    public Vector3 localAnchor;       // Local offset (anchor) on this object
    public float orbitSpeed = 50f;    // Degrees per second
    public Vector3 axis = Vector3.up; // Orbit axis

    private Rigidbody rb;

    public VerletRope rope;

    public bool isSpinning = false;
    public Transform spinStartLocation;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // optional
    }

    private void Update()
    {
        Vector3 euler = transform.eulerAngles;
        euler.z = 0f;
        transform.eulerAngles = euler;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        if (isSpinning)

        {
            // World position of the anchor on this object
            Vector3 worldAnchor = transform.TransformPoint(localAnchor);

            // Vector from target to anchor
            Vector3 dir = worldAnchor - target.position;

            // Rotate that vector
            Quaternion rotation = Quaternion.AngleAxis(orbitSpeed * Time.fixedDeltaTime, axis);
            Vector3 newDir = rotation * dir;

            // New world position of anchor
            Vector3 newAnchorPos = target.position + newDir;

            // How much the anchor needs to move
            Vector3 delta = newAnchorPos - worldAnchor;

            // Move Rigidbody by the same delta so anchor stays in orbit
            rb.MovePosition(rb.position + delta);

            // (Optional) also rotate the object itself around orbit axis
            //rb.MoveRotation(rotation * rb.rotation);

            Vector3 ropeDir = rope.GetEndDirection();
            if (ropeDir.sqrMagnitude > 0.0001f)
            {
                // Fixed up axis in world space (prevents roll)
                Vector3 up = Vector3.up;

                // Compute right axis from forward and up
                Vector3 right = Vector3.Cross(up, ropeDir).normalized;

                // Recompute exact up axis so axes are orthogonal
                Vector3 correctedUp = Vector3.Cross(ropeDir, right);

                // Build rotation from orthogonal axes
                Quaternion targetRot = Quaternion.LookRotation(ropeDir, correctedUp);

                rb.MoveRotation(targetRot);


            }
        }
            
    }
}
