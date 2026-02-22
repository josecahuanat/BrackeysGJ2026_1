using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 3.5f;
    [SerializeField] float sprintSpeed = 6f;
    
    [Header("Mouse Look Settings")]
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float maxLookAngle = 80f;
    
    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform cameraTransform;

    float rotationX = 0f;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        if (!controller.isGrounded)
            move += Vector3.down;
        float currentSpeed = Input.GetKey(KeyCode.LeftShift)? sprintSpeed : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);
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