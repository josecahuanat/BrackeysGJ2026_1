using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 3.5f;
    [SerializeField] float sprintSpeed = 6f;
    
    [Header("Mouse Look Settings")]
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float maxLookAngle = 80f;
    
    [Header("Head Bob Settings")]
    [SerializeField] bool enableHeadBob = true;
    [SerializeField] float bobFrequency = 2f;
    [SerializeField] float bobHorizontalAmplitude = 0.05f;
    [SerializeField] float bobVerticalAmplitude = 0.05f;
    
    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform cameraTransform;

    Vector3 velocity;
    float rotationX = 0f;
    Vector3 cameraStartPosition;
    float bobTimer = 0f;
    
    void Start()
    {        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        cameraStartPosition = cameraTransform.localPosition;
    }
    
    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void HandleMovement()
    {
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