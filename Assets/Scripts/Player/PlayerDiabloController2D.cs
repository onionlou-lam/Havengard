using UnityEngine;
using Havengard.Abilities;
using Havengard.HealthSystem;
using Havengard.Units;

namespace Havengard.Player
{
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
        private Vector2 rollVelocity;

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
                rb.linearVelocity = Vector2.zero;
            }
        }

        // --- INPUT HANDLING ---

        private void HandleMouseInput()
        {
            bool holdPosition = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (Input.GetMouseButtonDown(0)) // Left click: move
            {
                var world = MouseWorldOnPlane();
                clickMoveTarget = new Vector2(world.x, world.y);
                isClickMoving = true;
            }

            if (Input.GetMouseButtonDown(1)) // Right click: cast
            {
                AbilityBase rightClickAbility = abilityUser?.GetAbility(indexRightClick);
                if (rightClickAbility == null) return;

                GameObject target = MouseTarget();
                if (target != null)
                {
                    var health = target.GetComponent<IHealth>();
                    if (health != null)
                    {
                        var faction = health.GetFaction();

                        if (rightClickAbility.abilityType == AbilityType.Offensive &&
                            faction == Faction.Enemy)
                        {
                            abilityUser.UseAbility(indexRightClick, target);
                        }
                        else if (rightClickAbility.abilityType == AbilityType.Supportive &&
                                 (faction == Faction.Ally || faction == Faction.Player))
                        {
                            abilityUser.UseAbility(indexRightClick, target);
                        }
                        else if (rightClickAbility.abilityType == AbilityType.Utility)
                        {
                            CastAbilityAtMouse(indexRightClick);
                        }
                        else
                        {
                            CastAbilityAtMouse(indexRightClick);
                        }
                    }
                    else
                    {
                        CastAbilityAtMouse(indexRightClick);
                    }
                }
                else
                {
                    CastAbilityAtMouse(indexRightClick);
                }

                if (!holdPosition)
                {
                    var world = MouseWorldOnPlane();
                    clickMoveTarget = new Vector2(world.x, world.y);
                    isClickMoving = true;
                }
            }
        }

        private void HandleKeyboardAbilities()
        {
            if (Input.GetKeyDown(KeyCode.Q)) CastAbilityAtMouse(0);
            if (Input.GetKeyDown(KeyCode.W)) CastAbilityAtMouse(1);
            if (Input.GetKeyDown(KeyCode.E)) CastAbilityAtMouse(2);
            if (Input.GetKeyDown(KeyCode.R)) CastAbilityAtMouse(3);
        }

        private void HandleRollInput()
        {
            if (!Input.GetKeyDown(KeyCode.Space)) return;
            if (Time.time < lastRollTime + rollCooldown) return;

            Vector2 dir = GetRollDirection();
            if (dir.sqrMagnitude < 0.0001f) return;

            StartCoroutine(RollRoutine(dir));
        }

        // --- HELPERS ---

        private System.Collections.IEnumerator RollRoutine(Vector2 direction)
        {
            isRolling = true;
            lastRollTime = Time.time;

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
            w.z = 0f;
            return w;
        }

        private GameObject MouseTarget()
        {
            Vector3 mw = MouseWorldOnPlane();
            RaycastHit2D hit = Physics2D.Raycast(mw, Vector2.zero);
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
                return (clickMoveTarget - rb.position).normalized;

            if (rollTowardMouseIfIdle)
            {
                Vector3 mw = MouseWorldOnPlane();
                Vector2 dir = (new Vector2(mw.x, mw.y) - rb.position);
                if (dir.sqrMagnitude > 0.001f) return dir.normalized;
            }

            return Vector2.zero;
        }
    }
}
