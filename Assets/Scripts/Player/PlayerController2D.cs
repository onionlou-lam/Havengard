using UnityEngine;
using UnityEngine.AI;
using Havengard.Abilities;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private NavMeshAgent agent;
    private Rigidbody2D rb;

    [Header("Animation")]
    private Animator animator;
    
    [Header("Abilities")]
    private AbilityUser abilityUser;

    [Header("Input")]
    [SerializeField]
    private KeyCode[] abilityKeys = new KeyCode[]
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5
    };

    private Camera mainCamera;

    // Animation parameter hashes for performance - FIXED: Match blend tree parameters
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
            
            // CRITICAL FIX: Let NavMeshAgent control position while Rigidbody2D stays kinematic
            agent.updatePosition = true;
        }

        // CRITICAL FIX: Ensure Rigidbody2D is Kinematic when using NavMeshAgent
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Update()
    {
        HandleMovementInput();
        HandleAbilityInput();
        UpdateAnimations();
    }

    private void HandleMovementInput()
    {
        // Don't move while channeling
        if (abilityUser != null && abilityUser.IsChanneling)
        {
            if (agent != null) agent.isStopped = true;
            return;
        }

        if (agent != null) agent.isStopped = false;

        // Right click to move
        if (Input.GetMouseButton(1))
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            if (agent != null)
            {
                // CRITICAL FIX: Only set destination if path is valid
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(mousePos, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(mousePos);
                }
                // If path is partial or invalid, try to get as close as possible
                else if (path.status == NavMeshPathStatus.PathPartial && path.corners.Length > 0)
                {
                    agent.SetDestination(path.corners[path.corners.Length - 1]);
                }
            }
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        Vector2 velocity = Vector2.zero;

        // Get velocity from NavMeshAgent if available
        if (agent != null)
        {
            velocity = new Vector2(agent.velocity.x, agent.velocity.y);
        }
        // Otherwise get from Rigidbody2D
        else if (rb != null)
        {
            velocity = rb.linearVelocity;
        }

        // Update animation parameters
        float speed = velocity.magnitude;
        bool isMoving = speed > 0.1f;

        // FIXED: Normalize velocity for directional blend tree
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

        // Check each ability key
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
}