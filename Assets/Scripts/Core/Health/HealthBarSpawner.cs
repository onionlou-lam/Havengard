using Havengard.Core.HealthSystem;
using Havengard.Units;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthBarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private bool hideForNeutral = true;

    [Header("Offset (world units)")]
    [SerializeField] private float yOffset = 1.0f;   // absolute Y offset above the unit

    private HealthBar spawnedBar;

    private void Start()
    {
        var health = GetComponent<Health>();
        if (!health)
        {
            Debug.LogWarning($"HealthBarSpawner on {name} has no Health component.");
            return;
        }

        if (hideForNeutral && health.GetFaction() == Faction.Neutral)
            return;

        if (!healthBarPrefab)
        {
            Debug.LogWarning($"HealthBarSpawner on {name} has no healthBarPrefab assigned.");
            return;
        }

        var barGO = Instantiate(healthBarPrefab);
        spawnedBar = barGO.GetComponent<HealthBar>();

        if (!spawnedBar)
        {
            Debug.LogError($"HealthBar prefab {healthBarPrefab.name} is missing HealthBar component.");
            Destroy(barGO);
            return;
        }

        // Use absolute offset instead of sprite height + extra
        Vector3 offset = new Vector3(0, yOffset, 0);
        spawnedBar.Init(health, offset);
    }

    private void OnDestroy()
    {
        if (spawnedBar != null)
            Destroy(spawnedBar.gameObject);
    }
}
