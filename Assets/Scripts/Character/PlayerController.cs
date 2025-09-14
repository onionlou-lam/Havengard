using UnityEngine;
using Havengard.Health; // our HealthSystem namespace

[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 targetPosition;
    private Camera mainCamera;

    [Header("References")]
    private Health healthSystem;

    private void Awake()
    {
        mainCamera = Camera.main;
        healthSystem = GetComponent<Health>();

        // Subscribe to health events
        healthSystem.OnDamaged += HandleDamaged;
        healthSystem.OnHealed += HandleHealed;
        healthSystem.OnDeath += HandleDeath;
    }

    private void Update()
    {
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        if (Input.GetMouseButton(0)) // Left click = move
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                targetPosition = hit.point;
            }
        }

        // Move towards target position
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    #region Health Event Handlers
    private void HandleDamaged()
    {
        Debug.Log("Player took damage! Current HP: " + healthSystem.CurrentHealth);
        // TODO: trigger hit animation / particle effect / floating damage text
    }

    private void HandleHealed()
    {
        Debug.Log("Player healed! Current HP: " + healthSystem.CurrentHealth);
        // TODO: trigger heal effect / floating text
    }

    private void HandleDeath()
    {
        Debug.Log("Player died!");
        // TODO: trigger death animation, respawn, or game over logic
    }
    #endregion
}
