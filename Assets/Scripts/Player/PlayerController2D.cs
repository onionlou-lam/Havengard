using UnityEngine;
using UnityEngine.AI;
using Havengard.Abilities;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool useNavMeshMovement = false;
    [Tooltip("When NavMesh is disabled, movement will stop if velocity drops below this threshold")]
    [SerializeField] private float movementStopThreshold = 0.5f;

    private NavMeshAgent agent;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector3 clickMoveTarget;
    private bool hasClickMoveTarget = false;

    [Header("Animation")]
    private Animator animator;

    [Header("Abilities")]
    private AbilityUser abilityUser;

    [Header("Input Settings")]
    [SerializeField]
    private KeyCode[] abilityKeys = new KeyCode[]
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4
    };

    [SerializeField]
    [Tooltip("Key for interacting with objects")]
    private KeyCode interactKey = KeyCode.E;

    private Camera mainCamera;

    // Animation parameter hashes for performance
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody2D>();
        abilityUser = GetComponent<AbilityUser>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;

            // NavMesh behavior depends on useNavMeshMovement setting
            if (useNavMeshMovement)
            {
                agent.updatePosition = true;
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
            }
            else
            {
                agent.enabled = false;
            }
        }

        // When not using NavMesh, use dynamic Rigidbody2D
        if (!useNavMeshMovement && rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        HandleMovementInput();
        HandleAbilityInput();
        HandleInteractionInput();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (!useNavMeshMovement)
        {
            ApplyDirectMovement();
        }
    }

    private void HandleMovementInput()
    {
        // Don't move while channeling
        if (abilityUser != null && abilityUser.IsChanneling)
        {
            if (useNavMeshMovement && agent != null)
            {
                agent.isStopped = true;
            }
            moveInput = Vector2.zero;
            hasClickMoveTarget = false;
            return;
        }

        if (useNavMeshMovement && agent != null)
        {
            agent.isStopped = false;
        }

        // WASD input using GetKey to avoid Unity's default Input Manager bindings
        // This prevents Q/E interference with movement
        moveInput = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) moveInput.y += 1;
        if (Input.GetKey(KeyCode.S)) moveInput.y -= 1;
        if (Input.GetKey(KeyCode.A)) moveInput.x -= 1;
        if (Input.GetKey(KeyCode.D)) moveInput.x += 1;

        moveInput.Normalize(); // Prevent diagonal speed boost

        // Right click to move (MB2)
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            if (useNavMeshMovement && agent != null)
            {
                // NavMesh pathfinding behavior
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(mousePos, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(mousePos);
                }
                else if (path.status == NavMeshPathStatus.PathPartial && path.corners.Length > 0)
                {
                    agent.SetDestination(path.corners[path.corners.Length - 1]);
                }
            }
            else
            {
                // Direct movement towards click position
                clickMoveTarget = mousePos;
                hasClickMoveTarget = true;
            }
        }

        // Cancel click-to-move if WASD input is detected
        if (moveInput.magnitude > 0.1f)
        {
            hasClickMoveTarget = false;
            if (useNavMeshMovement && agent != null)
            {
                agent.ResetPath();
            }
        }
    }

    private void ApplyDirectMovement()
    {
        if (rb == null) return;

        Vector2 targetVelocity = Vector2.zero;

        // WASD has priority over click-to-move
        if (moveInput.magnitude > 0.1f)
        {
            targetVelocity = moveInput * moveSpeed;
        }
        // Click-to-move behavior (non-NavMesh)
        else if (hasClickMoveTarget)
        {
            Vector2 directionToTarget = (clickMoveTarget - transform.position).normalized;
            float distanceToTarget = Vector2.Distance(transform.position, clickMoveTarget);

            // Stop if we're close enough to the target
            if (distanceToTarget < 0.2f)
            {
                hasClickMoveTarget = false;
                targetVelocity = Vector2.zero;
            }
            else
            {
                targetVelocity = directionToTarget * moveSpeed;

                // Check if movement is too slow - stop trying to reach destination
                if (rb.linearVelocity.magnitude < movementStopThreshold)
                {
                    // Movement is blocked or too slow, cancel click-to-move
                    hasClickMoveTarget = false;
                    targetVelocity = Vector2.zero;
                }
            }
        }

        rb.linearVelocity = targetVelocity;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        Vector2 velocity = Vector2.zero;

        // Get velocity based on movement mode
        if (useNavMeshMovement && agent != null)
        {
            velocity = new Vector2(agent.velocity.x, agent.velocity.y);
        }
        else if (rb != null)
        {
            velocity = rb.linearVelocity;
        }

        // Update animation parameters
        float speed = velocity.magnitude;
        bool isMoving = speed > 0.1f;

        // Normalize velocity for directional blend tree
        Vector2 direction = velocity.normalized;

        // Set animator parameters to match blend tree
        animator.SetFloat(Horizontal, direction.x);
        animator.SetFloat(Vertical, direction.y);
        animator.SetFloat(Speed, speed);
        animator.SetBool(IsMoving, isMoving);
    }

    private void HandleAbilityInput()
    {
        if (abilityUser == null) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Mouse Button 1 (MB1/Left Click) - ability targeting/casting at slot 0
        if (Input.GetMouseButtonDown(0))
        {
            if (IsSkillAssignedToSlot(0))
            {
                abilityUser.UseAbility(0, mouseWorldPos);
            }
        }

        if (Input.GetMouseButtonUp(0) && abilityUser.IsChanneling)
        {
            abilityUser.StopChanneling();
        }

        // Ability keys 1, 2, 3, 4 - cast abilities at mouse position
        for (int i = 0; i < abilityKeys.Length; i++)
        {
            // Key pressed - start ability (or start channeling)
            if (Input.GetKeyDown(abilityKeys[i]))
            {
                abilityUser.UseAbility(i, mouseWorldPos);
            }

            // Key released - stop channeling
            if (Input.GetKeyUp(abilityKeys[i]))
            {
                if (abilityUser.IsChanneling)
                {
                    abilityUser.StopChanneling();
                }
            }
        }

        // Cancel channeling with Escape
        if (Input.GetKeyDown(KeyCode.Escape) && abilityUser.IsChanneling)
        {
            abilityUser.CancelChanneling();
        }
    }

    private void HandleInteractionInput()
    {
        // E key for interaction
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }

        // MB2 on interactable (right-click interaction)
        // We'll implement this once the interaction system is set up
        // For now, MB2 is primarily used for click-to-move
    }

    private void TryInteract()
    {
        // TODO: Implement interaction system
        // This will be set up later once the rest is working
        // For now, this is a placeholder for future interaction logic
        Debug.Log("Interaction attempted (system to be implemented)");
    }

    /// <summary>
    /// Checks if a specific ability slot has a skill assigned
    /// </summary>
    private bool IsSkillAssignedToSlot(int slotIndex)
    {
        if (abilityUser == null) return false;

        var abilities = abilityUser.GetAbilities();
        if (abilities == null || slotIndex < 0 || slotIndex >= abilities.Count)
            return false;

        return abilities[slotIndex] != null && abilityUser.IsAbilityUnlocked(slotIndex);
    }

    /// <summary>
    /// Public method to get the ability key bindings (for UI display)
    /// </summary>
    public KeyCode[] GetAbilityKeys()
    {
        return abilityKeys;
    }

    #region Game Controller Support (Future Implementation)

    // The following methods will be implemented when adding game controller support

    private Vector2 GetGamepadMovementInput()
    {
        // TODO: Implement gamepad input using Input.GetAxis for left stick
        // Example: new Vector2(Input.GetAxis("Gamepad_LeftStickX"), Input.GetAxis("Gamepad_LeftStickY"))
        return Vector2.zero;
    }

    private Vector2 GetGamepadAimDirection()
    {
        // TODO: Implement gamepad aiming using right stick
        // Example: new Vector2(Input.GetAxis("Gamepad_RightStickX"), Input.GetAxis("Gamepad_RightStickY"))
        return Vector2.zero;
    }

    #endregion
}