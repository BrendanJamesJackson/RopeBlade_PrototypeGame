using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;             // horizontal speed (m/s)
    public float jumpHeight = 1.2f;          // desired jump height in meters
    public Transform groundCheck;            // small empty at feet
    public float groundCheckRadius = 0.35f;
    public LayerMask groundMask;

    [Header("Mouse Look")]
    public Transform playerCamera;           // the Camera (child of player)
    public float mouseSensitivity = 200f;

    [Header("Options")]
    public bool lockCursor = true;

    Rigidbody rb;
    Vector2 moveInput;
    bool jumpRequested;
    float xRotation = 0f; // camera pitch
    float yaw = 0f;       // player yaw
    bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Keep player upright
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        // Smooth visuals while physics runs in FixedUpdate
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // ----- MOUSE LOOK (store yaw/pitch) -----
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);


        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // ----- INPUT -----
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        if (Input.GetButtonDown("Jump"))
            jumpRequested = true;

        // Optional: allow unlocking the cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.visible ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !Cursor.visible;
        }
    }

    void FixedUpdate()
    {
        // ----- GROUND CHECK -----
        if (groundCheck == null) Debug.LogWarning("RigidbodyFPSController: groundCheck not assigned.");
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        // ----- APPLY YAW ROTATION (physics-safe) -----
        Quaternion targetRotation = Quaternion.Euler(0f, yaw, 0f);
        rb.MoveRotation(targetRotation);

        // Use that rotation to build movement vectors (keeps movement in sync with rotation)
        Vector3 forward = targetRotation * Vector3.forward*-1;
        Vector3 right = targetRotation * Vector3.right*-1;
        Vector3 desiredVelocity = (right * moveInput.x + forward * moveInput.y) * moveSpeed;

        // Preserve current vertical velocity (gravity/jumping)
        Vector3 newVelocity = new Vector3(desiredVelocity.x, rb.linearVelocity.y, desiredVelocity.z);
        rb.linearVelocity = newVelocity;

        // ----- JUMP -----
        if (jumpRequested && isGrounded)
        {
            // Compute the initial velocity required to reach jumpHeight: v = sqrt(2 * g * h)
            float jumpVel = Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(Physics.gravity.y));
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVel, rb.linearVelocity.z);
        }

        jumpRequested = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
