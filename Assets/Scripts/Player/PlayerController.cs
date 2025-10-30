using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Parameters

    [Header("Motion")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float gravity = -9.81f;
    private float currentSpeed;

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

    private void Start()
    {
        TryGetComponent(out controller);
        currentSpeed = walkSpeed;

    }

    private void Update()
    {
        //Move
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        moveDirection = move * currentSpeed;
        controller.Move(moveDirection * Time.deltaTime);

        //Cámara
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalClamp, verticalClamp);
        cameraPlayer.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    #region Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }


    #endregion

}
