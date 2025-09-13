using UnityEngine;

public class HealthBarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Transform uiCanvas;

    private void Start()
    {
        var health = GetComponent<IHealth>();
        if (health == null) return;

        var bar = Instantiate(healthBarPrefab, uiCanvas);
        var healthBar = bar.GetComponent<HealthBar>();
        healthBar.Init(health);
    }
}
