using UnityEngine;
public class TestEnemy : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Hit by: {other.name}");
    }
}
