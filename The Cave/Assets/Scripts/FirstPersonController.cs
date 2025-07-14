using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform lampTransform;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Sprint Settings")]
    [SerializeField] private float maxStamina = 3f;
    [SerializeField] private float staminaRecoveryRate = 1f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference lookAction;

    private CharacterController controller;
    private Vector3 currentVelocity;
    private Vector3 velocitySmooth;
    private float currentStamina;
    private float yaw;
    private float pitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        currentStamina = maxStamina;

        // Calculate and store lamp's local transform relative to camera
        Vector3 lampLocalPos = cameraTransform.InverseTransformPoint(lampTransform.position);
        Quaternion lampLocalRot = Quaternion.Inverse(cameraTransform.rotation) * lampTransform.rotation;

        // Parent lamp to camera without preserving world transform
        lampTransform.SetParent(cameraTransform, false);
        lampTransform.localPosition = lampLocalPos;
        lampTransform.localRotation = lampLocalRot;

        // Hide and lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        sprintAction.action.Enable();
        lookAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        sprintAction.action.Disable();
        lookAction.action.Disable();
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
        yaw += lookInput.x;
        pitch -= lookInput.y;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        // Rotate player around Y
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        // Rotate camera (pitch only)
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 desiredDirection = (transform.forward * input.y + transform.right * input.x).normalized;

        bool sprintPressed = sprintAction.action.ReadValue<float>() > 0.5f && currentStamina > 0f;
        float targetSpeed = sprintPressed ? sprintSpeed : walkSpeed;

        if (sprintPressed)
            currentStamina = Mathf.Max(0f, currentStamina - Time.deltaTime);
        else
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRecoveryRate * Time.deltaTime);

        Vector3 targetVelocity = desiredDirection * targetSpeed;
        currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref velocitySmooth, smoothTime);
        controller.Move(currentVelocity * Time.deltaTime);
    }
}
