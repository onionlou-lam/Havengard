using UnityEngine;
public class TestProjectile : MonoBehaviour
{
    void Start() => GetComponent<Rigidbody2D>().linearVelocity = Vector2.right * 5;
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Projectile collided with {other.name}");
    }
}
