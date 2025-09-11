using UnityEngine;
using UnityEngine.AI; // optional if using navmesh for smooth movement
using Havengard.Abilities;

namespace Havengard.CharacterController
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        private Rigidbody2D rb;
        private Vector3 targetPosition;
        private bool isMovingToTarget;

        private AbilityController abilityController;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            abilityController = GetComponent<AbilityController>();
        }

        private void Update()
        {
            HandleMouseInput();
            HandleKeyboardInput();
        }

        private void FixedUpdate()
        {
            if (isMovingToTarget)
            {
                Vector2 direction = (targetPosition - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;

                if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
                {
                    rb.linearVelocity = Vector2.zero;
                    isMovingToTarget = false;
                }
            }
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        abilityController.UsePrimaryAttack(hit.collider.gameObject);
                        if (!Input.GetKey(KeyCode.LeftShift))
                            MoveToPoint(hit.collider.transform.position);
                    }
                    else
                    {
                        MoveToPoint(hit.point);
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1)) // Right click
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

                if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                {
                    abilityController.UseSecondaryAttack(hit.collider.gameObject);
                    if (!Input.GetKey(KeyCode.LeftShift))
                        MoveToPoint(hit.collider.transform.position);
                }
            }
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Q)) abilityController.CastAbility(0);
            if (Input.GetKeyDown(KeyCode.W)) abilityController.CastAbility(1);
            if (Input.GetKeyDown(KeyCode.E)) abilityController.CastAbility(2);
            if (Input.GetKeyDown(KeyCode.R)) abilityController.CastAbility(3);
        }

        private void MoveToPoint(Vector3 point)
        {
            targetPosition = point;
            isMovingToTarget = true;
        }
    }
}
