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
    private Vector2 lastMoveDirection = Vector2.down; // Default facing down
    private Vector2 lastAttackDirection = Vector2.down; // Last direction when attacking

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
    private static readonly int IdleFrame = Animator.StringToHash("IdleFrame");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int AttackHorizontal = Animator.StringToHash("AttackHorizontal");
    private static readonly int AttackVertical = Animator.StringToHash("AttackVertical");
    private static readonly int AttackDirection = Animator.StringToHash("AttackDirection");

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

        // Subscribe to ability events for attack animations
        if (abilityUser != null)
        {
            abilityUser.OnAbilityUsed += HandleAbilityUsed;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from ability events
        if (abilityUser != null)
        {
            abilityUser.OnAbilityUsed -= HandleAbilityUsed;
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
        moveInput = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) moveInput.y += 1;
        if (Input.GetKey(KeyCode.S)) moveInput.y -= 1;
        if (Input.GetKey(KeyCode.A)) moveInput.x -= 1;
        if (Input.GetKey(KeyCode.D)) moveInput.x += 1;

        // Normalize to ensure consistent speed in all directions
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

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

        // Update last move direction when moving
        if (isMoving)
        {
            lastMoveDirection = velocity.normalized;
        }

        // For 4-directional movement, snap to cardinal directions
        Vector2 animationDirection = isMoving ? SnapToFourDirections(velocity.normalized) : SnapToFourDirections(lastMoveDirection);

        // Set animator parameters for blend trees
        animator.SetFloat(Horizontal, animationDirection.x);
        animator.SetFloat(Vertical, animationDirection.y);
        animator.SetFloat(Speed, speed);
        animator.SetBool(IsMoving, isMoving);

        // Set idle frame based on last facing direction (0-3 for 4 directions)
        if (!isMoving)
        {
            int idleFrame = GetIdleFrameFromDirection(lastMoveDirection);
            animator.SetFloat(IdleFrame, idleFrame);
        }
    }

    /// <summary>
    /// Snaps a direction to one of 4 cardinal directions (Up, Down, Left, Right)
    /// Horizontal takes priority for diagonals
    /// </summary>
    private Vector2 SnapToFourDirections(Vector2 direction)
    {
        if (direction.magnitude < 0.1f)
            return Vector2.down; // Default direction

        // Check if horizontal or vertical is dominant
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal movement (Left or Right)
            return direction.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            // Vertical movement (Up or Down)
            return direction.y > 0 ? Vector2.up : Vector2.down;
        }
    }

    /// <summary>
    /// Converts a direction vector to an idle frame index (0-3 for 4 directions)
    /// </summary>
    private int GetIdleFrameFromDirection(Vector2 direction)
    {
        Vector2 snappedDirection = SnapToFourDirections(direction);

        // Map to 4 directions:
        // 0: Down (270°)
        // 1: Left (180°)
        // 2: Up (90°)
        // 3: Right (0°)

        if (snappedDirection == Vector2.down) return 0;
        if (snappedDirection == Vector2.left) return 1;
        if (snappedDirection == Vector2.up) return 2;
        if (snappedDirection == Vector2.right) return 3;

        return 0; // Default to down
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
                abilityUser.UseAbility(0, mouseWorldPos, null, false);
            }
        }

        // Hold MB1 for hold-to-cast
        if (Input.GetMouseButton(0))
        {
            // Hold-to-cast is handled in AbilityUser.Update()
            // Just keep the hold state active
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (abilityUser.IsChanneling)
            {
                abilityUser.StopChanneling();
            }
            else
            {
                abilityUser.ReleaseAbility(0);
            }
        }

        // Ability keys 1, 2, 3, 4 - cast abilities at mouse position
        for (int i = 0; i < abilityKeys.Length; i++)
        {
            // Key pressed - start ability (or start channeling)
            if (Input.GetKeyDown(abilityKeys[i]))
            {
                abilityUser.UseAbility(i, mouseWorldPos, null, false);
            }

            // Key held - hold-to-cast
            if (Input.GetKey(abilityKeys[i]))
            {
                // Hold state tracked, AbilityUser handles repeat casting
            }

            // Key released - stop channeling or release hold
            if (Input.GetKeyUp(abilityKeys[i]))
            {
                if (abilityUser.IsChanneling)
                {
                    abilityUser.StopChanneling();
                }
                else
                {
                    abilityUser.ReleaseAbility(i);
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
    }

    private void TryInteract()
    {
        // TODO: Implement interaction system
        Debug.Log("Interaction attempted (system to be implemented)");
    }

    /// <summary>
    /// Called when an ability is used - triggers attack animation
    /// </summary>
    private void HandleAbilityUsed(int abilityIndex, AbilityBase ability)
    {
        if (animator != null && ability != null)
        {
            // Calculate attack direction (towards mouse)
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            Vector2 attackDirection = (mouseWorldPos - transform.position).normalized;

            // Store attack direction for animation
            lastAttackDirection = SnapToFourDirections(attackDirection);

            // Trigger ability-specific animation or default attack
            TriggerAttackAnimation(ability);
        }
    }

    /// <summary>
    /// Triggers the attack animation with ability-specific support
    /// </summary>
    public void TriggerAttackAnimation(AbilityBase ability = null)
    {
        if (animator == null) return;

        // Set attack direction parameters
        animator.SetFloat(AttackHorizontal, lastAttackDirection.x);
        animator.SetFloat(AttackVertical, lastAttackDirection.y);
        animator.SetFloat(AttackDirection, GetIdleFrameFromDirection(lastAttackDirection));

        // Check if ability has a custom animation override
        string animationTrigger = GetAbilityAnimationTrigger(ability);

        if (!string.IsNullOrEmpty(animationTrigger) && HasAnimatorParameter(animationTrigger))
        {
            // Trigger ability-specific animation
            animator.SetTrigger(animationTrigger);
            Debug.Log($"Triggered ability-specific animation: {animationTrigger}");
        }
        else
        {
            // Trigger default attack animation
            animator.SetTrigger(Attack);
        }
    }

    /// <summary>
    /// Gets the animation trigger name for a specific ability
    /// </summary>
    private string GetAbilityAnimationTrigger(AbilityBase ability)
    {
        if (ability == null) return null;

        // Get custom animation trigger from ability
        return ability.GetAnimationTrigger();
    }

    /// <summary>
    /// Checks if animator has a parameter with the given name
    /// </summary>
    private bool HasAnimatorParameter(string parameterName)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == parameterName)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Public method to trigger attack animation from external scripts
    /// </summary>
    public void TriggerAttackAnimation()
    {
        TriggerAttackAnimation(null);
    }

    private bool IsSkillAssignedToSlot(int slotIndex)
    {
        if (abilityUser == null) return false;

        var abilities = abilityUser.GetAbilities();
        if (abilities == null || slotIndex < 0 || slotIndex >= abilities.Count)
            return false;

        return abilities[slotIndex] != null && abilityUser.IsAbilityUnlocked(slotIndex);
    }

    public KeyCode[] GetAbilityKeys()
    {
        return abilityKeys;
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            lastMoveDirection = direction.normalized;
        }
    }

    public Vector2 GetLastMoveDirection()
    {
        return lastMoveDirection;
    }

    public Vector2 GetLastAttackDirection()
    {
        return lastAttackDirection;
    }

    #region Game Controller Support (Future Implementation)

    private Vector2 GetGamepadMovementInput()
    {
        // TODO: Implement gamepad support
        return Vector2.zero;
    }

    #endregion
}