using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public PlayerInput inputActions;
    private InputAction spinAction;

    private void Awake()
    {
        spinAction = inputActions.actions["Spin"];
    }

    private void Update()
    {
        Spin();
    }

    public void Spin()
    {
        //Debug.Log(spinAction.ReadValue<float>());
    }
}
