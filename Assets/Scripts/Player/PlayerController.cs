using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    #region Parameters
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpForce = 8f;
    public float gravity = -15f;
    private float verticalVelocity;
    private float currentSpeed;

    [Header("Combat")]
    public float attackCooldown = 0.4f;
    private bool isAttacking;
    private int comboStep = 0;
    private float comboTimer;
    public float comboResetTime = 1.2f;

    [Header("Camera")]
    public Transform cameraPlayer;
    public float mouseSensitivity = 2f;
    public float verticalClamp = 30f;
    private float verticalRotation;

    [Header("References")]
    private CharacterController controller;
    private Animator animator;
    private PlayerSystem inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;
    private bool runPressed;
    private bool punchPressed;
    private bool kickPressed;
    #endregion

    private void Awake()
    {
        inputActions = new PlayerSystem();
        inputActions.Player.Enable();

        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        currentSpeed = walkSpeed;

        // Bind inputs
        inputActions.Player.Jump.performed += ctx => jumpPressed = true;
        inputActions.Player.Run.performed += ctx => runPressed = ctx.ReadValueAsButton();
        inputActions.Player.Punch.performed += ctx => punchPressed = true;
        inputActions.Player.Kick.performed += ctx => kickPressed = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return; // Solo controla su propio personaje

        HandleMovement();
        HandleCamera();
        HandleCombat();
    }

    private void HandleMovement()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        // Cambiar velocidad (caminar / correr)
        currentSpeed = runPressed ? runSpeed : walkSpeed;

        // Aplicar gravedad y salto
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (jumpPressed)
            {
                verticalVelocity = jumpForce;
                jumpPressed = false;
                animator?.SetTrigger("Jump");
            }
        }
        else
        {
            verticalVelocity += gravity * Runner.DeltaTime;
        }

        Vector3 moveDirection = transform.TransformDirection(move) * currentSpeed;
        moveDirection.y = verticalVelocity;

        controller.Move(moveDirection * Runner.DeltaTime);

        if (move != Vector3.zero)
        {
            animator?.SetBool("IsMoving", true);
            transform.forward = new Vector3(move.x, 0, move.z);
        }
        else
        {
            animator?.SetBool("IsMoving", false);
        }
    }

    private void HandleCamera()
    {
        if (cameraPlayer == null) return;

        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalClamp, verticalClamp);

        cameraPlayer.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleCombat()
    {
        if (isAttacking)
            return;

        // Combo básico (punch → punch → kick)
        if (punchPressed)
        {
            punchPressed = false;
            StartAttack("Punch");
        }
        else if (kickPressed)
        {
            kickPressed = false;
            StartAttack("Kick");
        }

        // Reset combo si se tarda demasiado
        if (comboStep > 0)
        {
            comboTimer += Runner.DeltaTime;
            if (comboTimer > comboResetTime)
            {
                comboStep = 0;
                comboTimer = 0;
            }
        }
    }

    private void StartAttack(string type)
    {
        isAttacking = true;
        comboStep++;
        comboTimer = 0;

        // Disparar animaciones como "Punch1", "Punch2", "Kick1"
        animator?.SetTrigger(type + comboStep);

        Invoke(nameof(EndAttack), attackCooldown);
    }

    private void EndAttack()
    {
        isAttacking = false;
    }
}
