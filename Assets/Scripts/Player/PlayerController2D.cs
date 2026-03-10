using Havengard.Abilities;
using Havengard.HealthSystem;
using Havengard.Units;
using UnityEngine;
using UnityEngine.AI;

namespace Havengard.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NavMeshAgent))]
    [DisallowMultipleComponent]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float stoppingDistance = 0.1f;

        [Header("Animator")]
        [SerializeField] private Animator animator;

        [Tooltip("If true, idle will keep the last facing direction instead of snapping to (0,0).")]
        [SerializeField] private bool keepLastFacingOnIdle = true;

        [Header("Right-Click Ability")]
        [SerializeField] private int indexRightClick = 0;

        [Header("Roll / Dodge")]
        [SerializeField] private float rollDistance = 3f;
        [SerializeField] private float rollDuration = 0.15f;
        [SerializeField] private float rollCooldown = 0.75f;
        [SerializeField] private bool rollTowardMouseIfIdle = true;

        private Rigidbody2D rb;
        private NavMeshAgent agent;
        private AbilityUser abilityUser;
        private ChannelController channelController;

        private Vector2 clickMoveTarget;
        private bool isClickMoving;
        private bool isChannelingWithMovementLock; // Tracks if movement is locked due to channeling

        private float lastRollTime = -999f;
        private bool isRolling;
        private Vector2 rollVelocity;

        private Vector2 lastFacingDir = Vector2.down;

        // Animator parameter hashes
        private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
        private static readonly int AnimHorizontal = Animator.StringToHash("Horizontal");
        private static readonly int AnimVertical = Animator.StringToHash("Vertical");
        private static readonly int AnimIdleFrame = Animator.StringToHash("IdleFrame");

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            abilityUser = GetComponent<AbilityUser>();
            channelController = GetComponent<ChannelController>();
            agent = GetComponent<NavMeshAgent>();
            if (animator == null) animator = GetComponent<Animator>();

            // Rigidbody2D used for collisions; NavMeshAgent moves transform
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;

            // Configure NavMeshAgent for top-down 2D
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;

            clickMoveTarget = rb.position;
        }

        private void Update()
        {
            if (isRolling)
            {
                UpdateAnimatorFromVelocity(rollVelocity);
                return;
            }

            // Don't process movement input if channeling with movement lock
            if (!isChannelingWithMovementLock)
            {
                HandleMouseInput();
                HandleRollInput();
            }

            HandleKeyboardAbilities();

            // If we stopped click-moving, avoid constantly ResetPath every frame
            if (!isClickMoving && agent.hasPath)
                agent.ResetPath();

            UpdateAnimatorFromNavMesh();
        }

        private void FixedUpdate()
        {
            if (isRolling)
            {
                rb.linearVelocity = rollVelocity;
                return;
            }

            // NavMesh handles normal movement (only if not channeling with movement lock)
            if (isClickMoving && !isChannelingWithMovementLock)
            {
                if (!agent.pathPending && agent.remainingDistance <= stoppingDistance)
                {
                    isClickMoving = false;
                    if (agent.hasPath) agent.ResetPath();
                }
            }
        }

        // --- INPUT HANDLING ---

        private void HandleMouseInput()
        {
            bool holdPosition = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (Input.GetMouseButtonDown(0)) // Left click: move unless holding Shift to hold position
            {
                if (!holdPosition)
                {
                    var world = MouseWorldOnPlane();
                    clickMoveTarget = new Vector2(world.x, world.y);
                    isClickMoving = true;
                    agent.SetDestination(clickMoveTarget);
                }
                else
                {
                    // Ensure we stay put if we were already moving
                    isClickMoving = false;
                    if (agent.hasPath) agent.ResetPath();
                }
            }

            if (Input.GetMouseButtonDown(1)) // Right click: cast
            {
                AbilityBase rightClickAbility = abilityUser?.GetAbility(indexRightClick);
                if (rightClickAbility != null)
                {
                    GameObject target = MouseTarget();
                    if (target == null)
                    {
                        CastAbilityAtMouse(indexRightClick);
                    }
                    else
                    {
                        abilityUser.UseAbility(indexRightClick, target);
                    }
                }
            }
        }

        private void HandleKeyboardAbilities()
        {
            // Q - Regular ability
            if (Input.GetKeyDown(KeyCode.Q)) 
                CastAbilityAtMouse(0);
            
            // W - Regular ability
            if (Input.GetKeyDown(KeyCode.W)) 
                CastAbilityAtMouse(1);
            
            // E - Channeled ability (FrostBeam)
            if (Input.GetKey(KeyCode.E))
            {
                // Hold to channel
                if (!IsChanneling())
                {
                    StartChanneledAbility();
                }
            }
            else
            {
                // Release to stop
                if (IsChanneling())
                {
                    StopChanneledAbility();
                }
            }
            
            // R - Regular ability
            if (Input.GetKeyDown(KeyCode.R)) 
                CastAbilityAtMouse(3);
        }

        private void HandleRollInput()
        {
            if (!Input.GetKeyDown(KeyCode.Space)) return;
            if (Time.time < lastRollTime + rollCooldown) return;

            Vector2 dir = GetRollDirection();
            if (dir.sqrMagnitude < 0.0001f) return;

            StartCoroutine(RollRoutine(dir));
        }

        // --- CHANNELED ABILITY HELPERS ---

        private bool IsChanneling()
        {
            // Use public property if available, otherwise use reflection
            if (channelController != null)
            {
                return channelController.IsChanneling;
            }
            return false;
        }

        private void StartChanneledAbility()
        {
            if (channelController != null && channelController.ability != null)
            {
                channelController.StartChannel();
                //Debug.Log("Started channeling FrostBeam");
            }
        }

        private void StopChanneledAbility()
        {
            if (channelController != null)
            {
                channelController.StopChannel();
                //Debug.Log("Stopped channeling FrostBeam");
            }
        }

        // --- CHANNEL CALLBACKS (called via SendMessage from ChannelController) ---

        private void OnChannelStarted()
        {
            isChannelingWithMovementLock = true;
            isClickMoving = false;
            //Debug.Log("Movement locked - channeling started");
        }

        private void OnChannelEnded()
        {
            isChannelingWithMovementLock = false;
            //Debug.Log("Movement unlocked - channeling ended");
        }

        // --- HELPERS ---

        private Vector3 MouseWorldOnPlane()
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            return world;
        }

        private GameObject MouseTarget()
        {
            Vector3 mouseWorldPos = MouseWorldOnPlane();
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
            return hit.collider != null ? hit.collider.gameObject : null;
        }

        private void CastAbilityAtMouse(int index)
        {
            if (abilityUser == null) return;

            Vector3 mouseWorldPos = MouseWorldOnPlane();
            GameObject fakeTarget = new GameObject("CursorTarget");
            fakeTarget.transform.position = mouseWorldPos;

            abilityUser.UseAbility(index, fakeTarget);
            Destroy(fakeTarget, 0.05f);
        }

        private Vector2 GetRollDirection()
        {
            if (isClickMoving)
                return (clickMoveTarget - (Vector2)transform.position).normalized;

            if (rollTowardMouseIfIdle)
            {
                Vector3 mw = MouseWorldOnPlane();
                Vector2 dir = (new Vector2(mw.x, mw.y) - (Vector2)transform.position);
                if (dir.sqrMagnitude > 0.001f) return dir.normalized;
            }

            // fallback to last facing
            return lastFacingDir.sqrMagnitude > 0.001f ? lastFacingDir : Vector2.down;
        }

        private System.Collections.IEnumerator RollRoutine(Vector2 direction)
        {
            isRolling = true;
            lastRollTime = Time.time;

            // Stop navmesh movement while rolling
            isClickMoving = false;
            if (agent.hasPath) agent.ResetPath();

            float speed = rollDistance / Mathf.Max(0.01f, rollDuration);
            rollVelocity = direction.normalized * speed;

            float t = 0f;
            while (t < rollDuration)
            {
                t += Time.deltaTime;
                yield return null;
            }

            isRolling = false;
            rb.linearVelocity = Vector2.zero;
        }

        private void UpdateAnimatorFromNavMesh()
        {
            Vector2 velocity = agent != null ? (Vector2)agent.desiredVelocity : Vector2.zero;
            UpdateAnimatorFromVelocity(velocity);
        }

        private void UpdateAnimatorFromVelocity(Vector2 velocity)
        {
            bool moving = velocity.sqrMagnitude > 0.001f;
            animator.SetBool(AnimIsMoving, moving);

            if (moving)
            {
                Vector2 dir = velocity.normalized;
                lastFacingDir = dir;

                animator.SetFloat(AnimHorizontal, dir.x);
                animator.SetFloat(AnimVertical, dir.y);
            }
            else
            {
                // keep last facing direction
                animator.SetFloat(AnimHorizontal, lastFacingDir.x);
                animator.SetFloat(AnimVertical, lastFacingDir.y);

                // force "first frame"
                animator.SetFloat(AnimIdleFrame, 0f);
            }
        }
    }
}