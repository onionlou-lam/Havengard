using UnityEngine;
using Havengard.Health;
using Havengard.Abilities;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AbilityUser))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.1f;

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private bool isMoving = false;

    [Header("References")]
    private Health health;
    private AbilityUser abilityUser;
    private Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        abilityUser = GetComponent<AbilityUser>();
        mainCamera = Camera.main;

        targetPosition = rb.position;

        health.OnDamaged += HandleDamaged;
        health.OnHealed += HandleHealed;
        health.OnDeath += HandleDeath;
    }

    private void Update()
    {
        HandleClickInput();
        HandleAbilityInput();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleClickInput()
    {
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            targetPosition = new Vector2(mouseWorld.x, mouseWorld.y);
            isMoving = true;

            Debug.Log($"PlayerController: Moving to {targetPosition}");
        }
    }

    private void HandleMovement()
    {
        if (!isMoving || Input.GetKey(KeyCode.LeftShift)) return;

        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        if (Vector2.Distance(rb.position, targetPosition) <= stoppingDistance)
        {
            isMoving = false;
            rb.linearVelocity = Vector2.zero;
            Debug.Log("PlayerController: Reached destination.");
        }
    }

    private void HandleAbilityInput()
    {
        GameObject target = GetMouseTarget();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("PlayerController: Casting ability slot 0.");
            abilityUser.UseAbility(0, target);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("PlayerController: Casting ability slot 1.");
            abilityUser.UseAbility(1, target);
        }
    }

    private GameObject GetMouseTarget()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log($"PlayerController: Target acquired - {hit.collider.name}");
            return hit.collider.gameObject;
        }

        Debug.Log("PlayerController: No target found.");
        return null;
    }

    private void HandleDamaged(float amount)
    {
        Debug.Log($"PlayerController: Took {amount} damage. Current HP: {health.CurrentHealth}");
    }

    private void HandleHealed(float amount)
    {
        Debug.Log($"PlayerController: Healed {amount}. Current HP: {health.CurrentHealth}");
    }

    private void HandleDeath()
    {
        Debug.Log("PlayerController: Player died.");
    }
}