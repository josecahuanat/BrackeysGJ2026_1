using UnityEngine;

public class FirstPersonController_alt : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 3.5f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float crouchSpeed = 2f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float groundCheckDistance = 0.4f;
    [SerializeField] LayerMask groundMask = -1;
    
    [Header("Mouse Look Settings")]
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float maxLookAngle = 80f;
    [SerializeField] bool invertY = false;
    
    [Header("Head Bobbing")]
    [SerializeField] bool enableHeadBob = true;
    [SerializeField] float walkBobSpeed = 10f;
    [SerializeField] float walkBobAmount = 0.05f;
    [SerializeField] float sprintBobSpeed = 15f;
    [SerializeField] float sprintBobAmount = 0.1f;
    [SerializeField] float crouchBobSpeed = 7f;
    [SerializeField] float crouchBobAmount = 0.03f;
    
    [Header("Idle Breathing")]
    [SerializeField] bool enableIdleBreathing = true;
    [SerializeField] float idleBreathingSpeed = 2f;
    [SerializeField] float idleBreathingAmount = 0.02f;
    [SerializeField] float idleSwaySpeed = 1.5f;
    [SerializeField] float idleSwayAmount = 0.01f;
    [SerializeField] float idleTransitionSpeed = 3f;
    
    [Header("Footsteps")]
    [SerializeField] AudioSource footstepAudioSource;
    [SerializeField] AudioClip[] footstepSounds;
    [SerializeField] float walkStepInterval = 0.5f;
    [SerializeField] float sprintStepInterval = 0.3f;
    [SerializeField] float crouchStepInterval = 0.7f;
    [SerializeField] float minPitch = 0.9f;
    [SerializeField] float maxPitch = 1.1f;
    
    [Header("Idle Sounds")]
    [SerializeField] AudioSource idleAudioSource;
    [SerializeField] AudioClip[] idleBreathSounds;
    [SerializeField] float minIdleBreathInterval = 3f;
    [SerializeField] float maxIdleBreathInterval = 8f;
    [SerializeField] float idleBreathVolume = 0.5f;
    
    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform cameraTransform;
    
    // Movement variables
    Vector3 velocity;
    bool isGrounded;
    bool isCrouching;
    bool isMoving;
    bool wasMoving;
    float currentSpeed;
    float defaultControllerHeight;
    Vector3 defaultCameraPosition;
    
    // Mouse look variables
    float rotationX = 0f;
    
    // Head bob variables
    float bobTimer = 0f;
    float defaultYPos;
    
    // Idle variables
    float idleBreathTimer = 0f;
    float idleSwayTimer = 0f;
    float idleSoundTimer = 0f;
    Vector3 idleTargetPosition;
    Vector3 idleCurrentVelocity;
    
    // Footstep variables
    float stepTimer = 0f;
    
    // States
    enum MovementState { Idle, Walking, Sprinting, Crouching, CrouchWalking, Airborne }
    MovementState currentState = MovementState.Idle;
    MovementState previousState = MovementState.Idle;
    
    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Store default values
        defaultControllerHeight = controller.height;
        defaultCameraPosition = cameraTransform.localPosition;
        defaultYPos = cameraTransform.localPosition.y;
        
        // Setup audio sources
        if (footstepAudioSource == null)
            footstepAudioSource = GetComponent<AudioSource>();
            
        if (idleAudioSource == null && GetComponents<AudioSource>().Length > 1)
            idleAudioSource = GetComponents<AudioSource>()[1];
            
        // Initialize idle timers
        idleSoundTimer = Random.Range(minIdleBreathInterval, maxIdleBreathInterval);
    }
    
    void Update()
    {
        HandleGroundCheck();
        HandleMouseLook();
        HandleMovement();
        HandleJumpAndGravity();
        HandleCrouch();
        UpdateMovementState();
        
        // Handle camera effects based on state
        HandleCameraEffects();
        
        // Handle cursor lock with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Lock cursor again when clicking
        if (Cursor.visible && Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void UpdateMovementState()
    {
        previousState = currentState;
        
        // Determine if moving
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        isMoving = (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f);
        
        // Determine state
        if (!isGrounded)
        {
            currentState = MovementState.Airborne;
        }
        else if (isCrouching)
        {
            currentState = isMoving ? MovementState.CrouchWalking : MovementState.Crouching;
        }
        else if (isMoving)
        {
            currentState = Input.GetKey(KeyCode.LeftShift) ? MovementState.Sprinting : MovementState.Walking;
        }
        else
        {
            currentState = MovementState.Idle;
        }
        
        // Update wasMoving for transition effects
        wasMoving = isMoving;
    }
    
    void HandleCameraEffects()
    {
        switch (currentState)
        {
            case MovementState.Idle:
                HandleIdleEffects();
                HandleIdleSounds();
                break;
                
            case MovementState.Walking:
            case MovementState.Sprinting:
            case MovementState.CrouchWalking:
                if (enableHeadBob)
                    HandleHeadBob();
                HandleFootsteps();
                break;
                
            case MovementState.Crouching:
                // Slight movement when crouching idle
                if (enableIdleBreathing)
                    HandleSubtleIdleEffects(true);
                break;
                
            case MovementState.Airborne:
                // Smooth return to default when airborne
                cameraTransform.localPosition = Vector3.Lerp(
                    cameraTransform.localPosition,
                    defaultCameraPosition,
                    Time.deltaTime * idleTransitionSpeed
                );
                break;
        }
        
        // Smooth transition between states
        if (previousState != currentState)
        {
            bobTimer = 0f;
        }
    }
    
    void HandleIdleEffects()
    {
        if (!enableIdleBreathing) return;
        
        // Breathing effect (vertical)
        idleBreathTimer += Time.deltaTime * idleBreathingSpeed;
        float breathOffset = Mathf.Sin(idleBreathTimer) * idleBreathingAmount;
        
        // Subtle sway (horizontal)
        idleSwayTimer += Time.deltaTime * idleSwaySpeed;
        float swayOffset = Mathf.Sin(idleSwayTimer) * idleSwayAmount;
        
        // Combine effects
        Vector3 targetPosition = new Vector3(
            defaultCameraPosition.x + swayOffset,
            defaultYPos + breathOffset,
            defaultCameraPosition.z
        );
        
        // Smooth movement to target
        cameraTransform.localPosition = Vector3.SmoothDamp(
            cameraTransform.localPosition,
            targetPosition,
            ref idleCurrentVelocity,
            0.1f
        );
    }
    
    void HandleSubtleIdleEffects(bool isCrouching)
    {
        float intensity = isCrouching ? 0.5f : 1f;
        
        idleBreathTimer += Time.deltaTime * idleBreathingSpeed * 0.7f;
        float breathOffset = Mathf.Sin(idleBreathTimer) * idleBreathingAmount * intensity * 0.5f;
        
        Vector3 targetPosition = new Vector3(
            defaultCameraPosition.x,
            defaultYPos + breathOffset,
            defaultCameraPosition.z
        );
        
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetPosition,
            Time.deltaTime * 5f
        );
    }
    
    void HandleIdleSounds()
    {
        if (idleBreathSounds.Length == 0 || idleAudioSource == null) return;
        
        idleSoundTimer -= Time.deltaTime;
        
        if (idleSoundTimer <= 0f)
        {
            // Play random breath sound
            AudioClip clip = idleBreathSounds[Random.Range(0, idleBreathSounds.Length)];
            idleAudioSource.volume = idleBreathVolume;
            idleAudioSource.pitch = Random.Range(0.95f, 1.05f);
            idleAudioSource.PlayOneShot(clip);
            
            // Reset timer
            idleSoundTimer = Random.Range(minIdleBreathInterval, maxIdleBreathInterval);
        }
    }
    
    void HandleGroundCheck()
    {
        // Check if player is grounded
        isGrounded = Physics.CheckSphere(
            transform.position - Vector3.up * (controller.height * 0.5f - controller.radius),
            controller.radius * 0.9f,
            groundMask
        );
        
        // Reset velocity if grounded and falling
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }
    
    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Get movement direction relative to player rotation
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        // Normalize diagonal movement
        if (move.magnitude > 1f)
            move.Normalize();
        
        // Determine current speed based on input and state
        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = walkSpeed;
        
        // Apply movement
        controller.Move(move * currentSpeed * Time.deltaTime);
    }
    
    void HandleJumpAndGravity()
    {
        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            
            if (isCrouching)
            {
                controller.height = defaultControllerHeight * 0.5f;
                controller.center = new Vector3(0, controller.height * 0.5f, 0);
            }
            else
            {
                controller.height = defaultControllerHeight;
                controller.center = new Vector3(0, controller.height * 0.5f, 0);
            }
        }
    }
    
    void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? -1 : 1);
        
        // Rotate player body left/right
        transform.Rotate(Vector3.up * mouseX);
        
        // Rotate camera up/down (with clamping)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
    
    void HandleHeadBob()
    {
        // Determine bob speed and amount based on current state
        float bobSpeed = walkBobSpeed;
        float bobAmount = walkBobAmount;
        
        if (currentState == MovementState.CrouchWalking)
        {
            bobSpeed = crouchBobSpeed;
            bobAmount = crouchBobAmount;
        }
        else if (currentState == MovementState.Sprinting)
        {
            bobSpeed = sprintBobSpeed;
            bobAmount = sprintBobAmount;
        }
        
        // Calculate bob offset
        bobTimer += Time.deltaTime * bobSpeed;
        float newY = defaultYPos + Mathf.Sin(bobTimer) * bobAmount;
        float newX = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f;
        
        // Apply bob
        cameraTransform.localPosition = new Vector3(
            defaultCameraPosition.x + newX,
            newY,
            defaultCameraPosition.z
        );
    }
    
    void HandleFootsteps()
    {
        // Determine step interval based on current state
        float stepInterval = walkStepInterval;
        
        if (currentState == MovementState.CrouchWalking)
            stepInterval = crouchStepInterval;
        else if (currentState == MovementState.Sprinting)
            stepInterval = sprintStepInterval;
        
        // Update step timer
        stepTimer -= Time.deltaTime;
        
        // Play footstep when timer reaches zero
        if (stepTimer <= 0f && footstepSounds.Length > 0 && footstepAudioSource != null)
        {
            // Reset timer
            stepTimer = stepInterval;
            
            // Select random footstep sound
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            
            // Set random pitch for variation
            footstepAudioSource.pitch = Random.Range(minPitch, maxPitch);
            
            // Play sound
            footstepAudioSource.PlayOneShot(clip);
        }
    }
    
    // Public method to get current state
    public string GetCurrentState()
    {
        return currentState.ToString();
    }
    
    // Public method to toggle cursor lock state
    public void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    // Draw ground check gizmo for debugging
    void OnDrawGizmosSelected()
    {
        if (controller != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                transform.position - Vector3.up * (controller.height * 0.5f - controller.radius),
                controller.radius * 0.9f
            );
        }
    }
}