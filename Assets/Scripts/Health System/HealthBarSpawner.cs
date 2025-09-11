using Havengard.HealthSystem;
using UnityEngine;

public class HealthBarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;

    void Start()
    {
        if (TryGetComponent<Health>(out var health))
        {
            var hb = Instantiate(healthBarPrefab);
            hb.GetComponent<HealthBar>().Init(health);
        }
    }
}
