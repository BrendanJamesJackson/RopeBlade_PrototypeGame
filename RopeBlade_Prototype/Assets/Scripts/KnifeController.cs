using UnityEngine;

public class KnifeController : MonoBehaviour
{
    public Transform target;          // Point to orbit around
    public Transform spinStart;
    public Vector3 localAnchor;       // Local offset (anchor) on this object
    public float orbitSpeedMax = 50f;   // Degrees per second
    public float orbitSpeedMin;
    public Vector3 axis = Vector3.up; // Orbit axis

    private Rigidbody rb;

    //public VerletRope rope;

    public bool isSpinning = false;
    public Transform spinStartLocation;

    public bool isWhipping = false;
    public Transform whipEndPosition;
    public float whipForce;

    public Animator anim;

    public VerletRope2 rope;
    public float spinLength;
    public float maxLength;
    public float minLength;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // optional
    }

    private void Update()
    {
        

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

        if (Input.GetMouseButtonDown(0))
        {
            StartSpinning();
        }

        if (Input.GetMouseButton(1))
        {
            WhipForward();
        }

    }

    void FixedUpdate()
    {
        if (target == null) return;

        if (isSpinning)
        {
            Spin(); 
        }

        if (isWhipping)
        {
            Vector3 target = whipEndPosition.position;
            float step = whipForce * Time.fixedDeltaTime;
            Vector3 newPos = Vector3.MoveTowards(rb.position, target, step);
            rb.MovePosition(newPos);
        }
            
    }

    public void Spin()
    {
        // World position of the anchor on this object
        Vector3 worldAnchor = transform.TransformPoint(localAnchor);

        // Vector from target to anchor
        Vector3 dir = worldAnchor - target.position;

        // Rotate that vector
        Quaternion rotation = Quaternion.AngleAxis(orbitSpeedMax * Time.fixedDeltaTime, axis);
        Vector3 newDir = rotation * dir;

        // New world position of anchor
        Vector3 newAnchorPos = target.position + newDir;

        // How much the anchor needs to move
        Vector3 delta = newAnchorPos - worldAnchor;

        // Move Rigidbody by the same delta so anchor stays in orbit
        rb.MovePosition(rb.position + delta);
    }
    public void StartSpinning()
    {
        isWhipping = false;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        this.transform.position = spinStart.position;
        this.transform.rotation = spinStart.rotation;
        isSpinning = true;
        anim.SetTrigger("Spin");
    }

    public void StopSpinning()
    {
        isSpinning = false;
    }

    public void WhipForward()
    {
        StopSpinning();
        isWhipping = true;
        anim.SetTrigger("Release");
        /*transform.position = spinStart.position;

        Vector3 direction = (whipEndPosition.position - transform.position).normalized;

        rb.isKinematic = false;
        rb.AddForce(direction * whipForce, ForceMode.Impulse);
        */

        
    }

    public void StopOvershoot()
    {

        Debug.Log(Vector3.Distance(transform.position, whipEndPosition.position));

        /*Vector3 toWhipPos = whipEndPosition.position - transform.position;

        if (toWhipPos.magnitude > 0.1f)
        {
            Vector3 direction = toWhipPos.normalized;
            rb.AddForce(direction * whipForce, ForceMode.Impulse);
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }*/


        if (Vector3.Distance(transform.position, whipEndPosition.position) < 1f)
        {
            Debug.Log(Vector3.Distance(transform.position, whipEndPosition.position));
            rb.linearVelocity = Vector3.zero;
            rb.MovePosition(whipEndPosition.position);
        }

    }

}
