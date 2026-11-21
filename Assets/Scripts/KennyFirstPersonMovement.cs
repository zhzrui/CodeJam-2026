using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; 

public class KennyFirstPersonMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float sprintSpeed = 4.5f;
    [SerializeField] private float acceleration = 14f;

    [Header("Jumping & Gravity")]
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 10f;
    [SerializeField] private float staminaDrainRate = 1f;
    [SerializeField] private float staminaRegenRate = 0.8f;
    [SerializeField] private float exhaustionThreshold = 1.5f;

    [Header("Cinemachine Head Bob")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float mouseSens = 0.20f;
    [SerializeField] private float bobFrequency = 8f;
    [SerializeField] private float bobAmplitude = 0.10f;
    [SerializeField] private float strafeTiltAmount = 1.5f;

    [Header("FOV")]
    [SerializeField] private float walkFov = 65f;
    [SerializeField] private float sprintFov = 73f;
    [SerializeField] private float fovTransitionSpeed = 8f;

    private CharacterController controller;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private bool jumpPressed;
    private bool sprintHeld;

    // Math shit
    private Vector3 currentHorizontalVelocity;
    private float verticalVelocity;
    private float cameraVerticalRotation;
    private float currentStamina;
    private bool isExhausted;
    private bool isSprinting;
    private float defaultCameraY;
    private float bobTimer;
    private float currentTilt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentStamina = maxStamina;
        defaultCameraY = cameraTarget.localPosition.y;

        if (cinemachineCamera != null)
        {
            var lens = cinemachineCamera.Lens;
            lens.FieldOfView = walkFov;
            cinemachineCamera.Lens = lens;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleStamina();
        Move();
        LookAndBob();
        UpdateFov();
    }

    // getting inputs
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        sprintHeld = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpPressed = true;
        }
    }

    private void HandleStamina()
    {
        bool wantsToSprint = sprintHeld && moveInput.y > 0f && controller.isGrounded;

        if (wantsToSprint && !isExhausted)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true;
                isSprinting = false;
            }
        }
        else
        {
            isSprinting = false;
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina >= maxStamina)
            {
                currentStamina = maxStamina;
            }

            if (isExhausted && currentStamina >= exhaustionThreshold)
            {
                isExhausted = false;
            }
        }
    }

    private void Move()
    {
        // get wasd input
        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

        float targetSpeed = walkSpeed;
        if (sprintHeld && !isExhausted && moveInput.y > 0f)
        {
            targetSpeed = sprintSpeed;
        }

        Vector3 targetVelocity = move * targetSpeed;
        currentHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, acceleration * Time.deltaTime);

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // jump
        if (jumpPressed && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        jumpPressed = false;

        // add gravity for fall -> fail case
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalVelocity = currentHorizontalVelocity;
        finalVelocity.y = verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void LookAndBob()
    {
        // camera on mouse movement
        float mouseX = lookInput.x * mouseSens;
        float mouseY = lookInput.y * mouseSens;

        // l-r, u-d camera rotations
        transform.Rotate(0f, mouseX, 0f);
        cameraVerticalRotation -= mouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -88f, 88f);

        // l-r tilted towers
        float targetTilt = -moveInput.x * strafeTiltAmount;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, 8f * Time.deltaTime);
        cameraTarget.localRotation = Quaternion.Euler(cameraVerticalRotation, 0f, currentTilt);

        // head bob
        float heightRatio = controller.height / 2f;
        float targetCamY = defaultCameraY * heightRatio;

        float speed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
        if (controller.isGrounded && speed > 0.2f)
        {
            float speedMultiplier = speed / walkSpeed;
            bobTimer += Time.deltaTime * bobFrequency * speedMultiplier;
            targetCamY += Mathf.Sin(bobTimer) * bobAmplitude * speedMultiplier;
        }
        else
        {
            bobTimer = 0f;
        }

        Vector3 camPos = cameraTarget.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetCamY, 12f * Time.deltaTime);
        cameraTarget.localPosition = camPos;
    }

    private void UpdateFov()
    {
        if (cinemachineCamera == null) 
        {
            return;
        }
        float targetFov = isSprinting ? sprintFov : walkFov;
        var lens = cinemachineCamera.Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFov, fovTransitionSpeed * Time.deltaTime);
        cinemachineCamera.Lens = lens;
    }
}