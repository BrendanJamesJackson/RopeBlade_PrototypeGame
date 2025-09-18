using UnityEngine;

public class RigidbodyDebugger : MonoBehaviour
{
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log("Gravity: " + Physics.gravity);
        Debug.Log("Simulation mode: " + Physics.simulationMode);
    }

    void FixedUpdate()
    {
        //Debug.Log("Velocity: " + rb.linearVelocity + " Position: " + transform.position);
    }
}
