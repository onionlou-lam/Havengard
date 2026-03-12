using UnityEngine;
using DamageNumbersPro;
using Havengard.Core.HealthSystem;

namespace Havengard.UI
{
    [DisallowMultipleComponent]
    public class DamageNumberOnHealthChanged : MonoBehaviour
    {
        [Header("Optional overrides (leave blank to use DamageNumberLibraryRuntime)")]
        [SerializeField] private DamageNumberMesh damageNumberPrefabOverride;
        [SerializeField] private DamageNumberMesh healNumberPrefabOverride;

        [Header("Spawn Settings")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.9f, 0f);
        [SerializeField] private bool followTarget = true;

        [Header("Filtering")]
        [SerializeField] private bool showZeroChanges = false;

        private Health health;
        private int lastHealth;
        private bool initialized;
        private bool suppressNextChange = false;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            TryInitAndSubscribe();
        }

        private void OnDisable()
        {
            if (health != null)
                health.OnDamaged -= OnDamagedHandler;

            if (health != null)
                health.OnHealed -= OnHealedHandler;
        }

        private void TryInitAndSubscribe()
        {
            if (initialized || health == null) return;

            var hs = health.GetHealthSystem();
            if (hs == null) return;

            lastHealth = hs.Current;
            suppressNextChange = true;
            initialized = true;

            // Subscribe
            health.OnDamaged += OnDamagedHandler;
            health.OnHealed += OnHealedHandler;
        }

        private void OnDamagedHandler(int amount)
        {
            HandleHealthChanged();
        }

        private void OnHealedHandler(int amount)
        {
            HandleHealthChanged();
        }

        private void HandleHealthChanged()
        {
            if (!initialized || health == null) return;

            var hs = health.GetHealthSystem();
            if (hs == null) return;

            int current = hs.Current;
            int delta = current - lastHealth;

            if (suppressNextChange)
            { suppressNextChange = false;
                lastHealth = current;
                return;
            }
            if (!showZeroChanges && delta == 0)
            {
                lastHealth = current;
                return;
            }

            // Pick prefabs (override -> runtime library)
            DamageNumberMesh damagePrefab = damageNumberPrefabOverride;
            DamageNumberMesh healPrefab = healNumberPrefabOverride;

            var lib = DamageNumberLibraryRuntime.Instance != null ? DamageNumberLibraryRuntime.Instance.Library : null;
            if (damagePrefab == null && lib != null) damagePrefab = lib.DamagePrefab;
            if (healPrefab == null && lib != null) healPrefab = lib.HealPrefab;

            // Spawn position + follow
            Vector3 pos = transform.position + worldOffset;
            Transform follow = followTarget ? transform : null;

            if (delta < 0)
            {
                if (damagePrefab != null)
                    damagePrefab.Spawn(pos, -delta, follow);
            }
            else if (delta > 0)
            {
                if (healPrefab != null)
                    healPrefab.Spawn(pos, delta, follow);
            }

            lastHealth = current;
        }
    }
}
