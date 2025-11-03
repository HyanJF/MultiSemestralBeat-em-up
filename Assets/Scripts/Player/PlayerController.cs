using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class PlayerController : NetworkBehaviour
{
    #region Parameters

    [Header("Motion")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float gravity = -9.81f;
    private float currentSpeed;
    public PlayerSystem inputActions;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public float verticalClamp = 30f;
    public Transform cameraPlayer;

    [Header("Interaction")]
    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 moveDirection;
    private float verticalRotation = 0f;
    #endregion

    private void Awake()
    {
        inputActions = new PlayerSystem();
        inputActions.Player.Enable();
        TryGetComponent(out controller);
        currentSpeed = walkSpeed;

    }

    public override void FixedUpdateNetwork()
    {
        //Move
        InputSystem.Update();
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        controller.Move(move * Runner.DeltaTime * currentSpeed);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        //Cámara
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalClamp, verticalClamp);
        cameraPlayer.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

}
