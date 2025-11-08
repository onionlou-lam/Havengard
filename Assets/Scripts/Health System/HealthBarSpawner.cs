using Havengard.HealthSystem;
using Havengard.Units;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthBarSpawner : MonoBehaviour
{
    [SerializeField] private bool hideForNeutral = true;
    [SerializeField] private float scaleMultiplier = 1f;

    private HealthBar activeBar;

    void Start()
    {
        if (!TryGetComponent(out Health health)) return;
        if (hideForNeutral && health.GetFaction() == Faction.Neutral) return;

        // Use pooled health bar instead of Instantiate
        var hb = HealthBarPool.Instance != null
            ? HealthBarPool.Instance.Get()
            : Instantiate(Resources.Load<GameObject>("HealthBarPrefab")).GetComponent<HealthBar>();

        hb.Init(health);

        float spriteHeight = GetSpriteHeight();
        Vector3 offset = new Vector3(0, spriteHeight + 0.3f, 0);
        hb.SetOffset(offset);
        hb.transform.localScale *= scaleMultiplier;

        activeBar = hb;
    }

    private float GetSpriteHeight()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        return sr != null ? sr.bounds.size.y : 1f;
    }

    private void OnDestroy()
    {
        if (activeBar != null && HealthBarPool.Instance != null)
            HealthBarPool.Instance.Return(activeBar);
    }
}
