using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.5f;
    
    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    
    [Header("Head Bob Settings")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobHorizontalAmplitude = 0.05f;
    [SerializeField] private float bobVerticalAmplitude = 0.05f;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Light flashlight; // Optional: assign a flashlight
    
    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private float currentStamina;
    private float rotationX = 0f;
    private Vector3 cameraStartPosition;
    private float bobTimer = 0f;
    private bool isFlashlightOn = true;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        cameraStartPosition = cameraTransform.localPosition;
    }
    
    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        
        // Press ESC to unlock cursor (for testing)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void HandleMovement()
    {
        // Check if player is grounded
        bool isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // Small downward force to keep grounded
        
        // Get input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Calculate movement direction
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        float currentSpeed = Input.GetKey(KeyCode.LeftShift)? sprintSpeed : walkSpeed;
        
        controller.Move(move * currentSpeed * Time.deltaTime);
        
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // Head bob effect
        if (enableHeadBob && cameraTransform != null)
        {
            if (isGrounded && move.magnitude > 0.1f)
            {
                bobTimer += Time.deltaTime * bobFrequency;
                float horizontalBob = Mathf.Sin(bobTimer) * bobHorizontalAmplitude;
                float verticalBob = Mathf.Sin(bobTimer * 2) * bobVerticalAmplitude;
                
                cameraTransform.localPosition = new Vector3(
                    cameraStartPosition.x + horizontalBob,
                    cameraStartPosition.y + verticalBob,
                    cameraStartPosition.z
                );
            }
            else
            {
                // Reset camera position when not moving
                bobTimer = 0f;
                cameraTransform.localPosition = Vector3.Lerp(
                    cameraTransform.localPosition,
                    cameraStartPosition,
                    Time.deltaTime * 5f
                );
            }
        }
    }
    
    void HandleMouseLook()
    {
        if (cameraTransform == null) return;
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Rotate player body left/right
        transform.Rotate(Vector3.up * mouseX);
        
        // Rotate camera up/down (with clamping)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}