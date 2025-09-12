using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerClickToMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float stoppingDistance = 0.1f;

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private bool isMoving = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        targetPosition = rb.position; // start at current position
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left click
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition = new Vector2(mouseWorld.x, mouseWorld.y);
            isMoving = true;
        }
    }

    void FixedUpdate()
    {
        if (!isMoving) return;

        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        // Stop when close enough
        if (Vector2.Distance(rb.position, targetPosition) <= stoppingDistance)
        {
            isMoving = false;
            rb.linearVelocity = Vector2.zero;
        }
    }
}
