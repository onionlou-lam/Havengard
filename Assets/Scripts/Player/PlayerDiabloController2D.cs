using UnityEngine;
using Havengard.Abilities;

namespace Havengard.Player
{
    /// <summary>
    /// Diablo-style PC controls:
    /// - Left Click: move to ground
    /// - Right Click: cast assigned ability (indexRightClick). If not holding Shift, will also move toward target clicked.
    /// - QWER: cast abilities [0..3] at mouse target
    /// - Hold Shift: attack/cast without moving (hold position)
    /// - Space: roll/dodge toward movement direction or mouse direction (configurable), with cooldown
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class PlayerDiabloController2D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float stoppingDistance = 0.1f;

        [Header("Right-Click Ability")]
        [Tooltip("Ability index to cast on right click (e.g., 0 for primary).")]
        [SerializeField] private int indexRightClick = 0;

        [Header("Roll / Dodge")]
        [SerializeField] private float rollDistance = 3f;
        [SerializeField] private float rollDuration = 0.15f;
        [SerializeField] private float rollCooldown = 0.75f;
        [SerializeField] private bool rollTowardMouseIfIdle = true;

        private Rigidbody2D rb;
        private AbilityUser abilityUser;

        private Vector2 clickMoveTarget;
        private bool isClickMoving;

        private float lastRollTime = -999f;
        private bool isRolling;
        private Vector2 rollVelocity; // cached during roll

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            abilityUser = GetComponent<AbilityUser>();
            clickMoveTarget = rb.position;
        }

        private void Update()
        {
            if (isRolling) return;

            HandleMouseInput();
            HandleKeyboardAbilities();
            HandleRollInput();
        }

        private void FixedUpdate()
        {
            if (isRolling)
            {
                rb.linearVelocity = rollVelocity;
                return;
            }

            if (isClickMoving)
            {
                Vector2 newPos = Vector2.MoveTowards(rb.position, clickMoveTarget, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);

                if (Vector2.Distance(rb.position, clickMoveTarget) <= stoppingDistance)
                {
                    isClickMoving = false;
                    rb.linearVelocity = Vector2.zero;
                }
            }
            else
            {
                // Idle (no WASD in this controller; you can add it if desired later)
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void HandleMouseInput()
        {
            bool holdPosition = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (Input.GetMouseButtonDown(0)) // Left click: move to ground point
            {
                var world = MouseWorldOnPlane();
                clickMoveTarget = new Vector2(world.x, world.y);
                isClickMoving = true;
            }

            if (Input.GetMouseButtonDown(1)) // Right click: cast ability at target (or ground), optionally move
            {
                GameObject target = MouseTarget();
                abilityUser?.UseAbility(indexRightClick, target);

                if (!holdPosition)
                {
                    // Move toward clicked thing (enemy or ground)
                    var world = MouseWorldOnPlane();
                    clickMoveTarget = new Vector2(world.x, world.y);
                    isClickMoving = true;
                }
            }
        }

        private void HandleKeyboardAbilities()
        {
            // QWER map to 0..3
            if (Input.GetKeyDown(KeyCode.Q)) abilityUser?.UseAbility(0, MouseTarget());
            if (Input.GetKeyDown(KeyCode.W)) abilityUser?.UseAbility(1, MouseTarget());
            if (Input.GetKeyDown(KeyCode.E)) abilityUser?.UseAbility(2, MouseTarget());
            if (Input.GetKeyDown(KeyCode.R)) abilityUser?.UseAbility(3, MouseTarget());
        }

        private void HandleRollInput()
        {
            if (!Input.GetKeyDown(KeyCode.Space)) return;
            if (Time.time < lastRollTime + rollCooldown) return;

            Vector2 dir = GetRollDirection();
            if (dir.sqrMagnitude < 0.0001f) return;

            StartCoroutine(RollRoutine(dir));
        }

        private System.Collections.IEnumerator RollRoutine(Vector2 direction)
        {
            isRolling = true;
            lastRollTime = Time.time;

            // Compute constant velocity for the roll
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

        private Vector3 MouseWorldOnPlane()
        {
            Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            w.z = 0f; // 2D plane
            return w;
        }

        private GameObject MouseTarget()
        {
            Vector3 mw = MouseWorldOnPlane();
            var hit = Physics2D.Raycast(mw, Vector2.zero);
            return hit.collider != null ? hit.collider.gameObject : null;
        }

        private Vector2 GetRollDirection()
        {
            // If we're currently moving via click, roll along that vector
            if (isClickMoving)
                return (clickMoveTarget - rb.position).normalized;

            // Otherwise, roll toward mouse if configured
            if (rollTowardMouseIfIdle)
            {
                Vector3 mw = MouseWorldOnPlane();
                Vector2 dir = (new Vector2(mw.x, mw.y) - rb.position);
                if (dir.sqrMagnitude > 0.001f) return dir.normalized;
            }

            // Default: no movement/roll
            return Vector2.zero;
        }
    }
}
