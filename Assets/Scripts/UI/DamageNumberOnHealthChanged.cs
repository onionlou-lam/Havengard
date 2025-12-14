using UnityEngine;
using Havengard.HealthSystem;

#if TMP_PRESENT
using TMPro;
#endif

// Damage Numbers Pro
using DamageNumbersPro;

namespace Havengard.UI
{
    /// <summary>
    /// Spawns Damage Numbers Pro popups whenever this unit's Health changes.
    /// Designed to work with world-space health bars (spawned as children).
    /// </summary>
    [DisallowMultipleComponent]
    public class DamageNumberOnHealthChanged : MonoBehaviour
    {
        [Header("Damage Numbers Pro Prefabs")]
        [Tooltip("Prefab for damage numbers (red, etc.).")]
        [SerializeField] private DamageNumber damageNumberPrefab;

        [Tooltip("Optional prefab for healing numbers (green, etc.). Leave null to disable heal popups.")]
        [SerializeField] private DamageNumber healNumberPrefab;

        [Header("Follow / Position")]
        [Tooltip("If true, tries to follow the spawned world-space HealthBar transform (child). If not found, follows this unit.")]
        [SerializeField] private bool followHealthBarIfPresent = true;

        [Tooltip("Offset applied when healthbar isn't found (world space).")]
        [SerializeField] private Vector3 fallbackWorldOffset = new Vector3(0f, 0.9f, 0f);

        [Tooltip("Extra offset added on top of the followed transform position when spawning.")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.15f, 0f);

        [Header("Filtering")]
        [Tooltip("Minimum absolute delta to show a popup.")]
        [SerializeField] private int minimumAmountToShow = 1;

        private Health health;
        private int lastHealth;
        private int maxHealth;

        private Transform followedTarget;   // healthbar transform if present, otherwise unit transform
        private bool initialized;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            TryInit();
        }

        private void Start()
        {
            // HealthBarSpawner usually creates bars in Start(), so we re-check one frame later.
            StartCoroutine(InitNextFrame());
        }

        private System.Collections.IEnumerator InitNextFrame()
        {
            yield return null;
            TryInit(forceRecheckFollowTarget: true);
        }

        private void OnDisable()
        {
            if (health == null) return;
            health.OnDamaged -= OnHealthChanged;
            health.OnHealed -= OnHealthChanged;
        }

        private void TryInit(bool forceRecheckFollowTarget = false)
        {
            if (health == null) return;

            var hs = health.GetHealthSystem();
            if (hs == null) return;

            int current = hs.GetHealth();
            int max = hs.GetMaxHealth();

            if (!initialized)
            {
                lastHealth = current;
                maxHealth = max;
                initialized = true;

                health.OnDamaged += OnHealthChanged;
                health.OnHealed += OnHealthChanged;
            }

            if (forceRecheckFollowTarget || followedTarget == null)
                followedTarget = ResolveFollowTarget();

            // keep max in sync in case stats change
            maxHealth = max;
        }

        private Transform ResolveFollowTarget()
        {
            if (!followHealthBarIfPresent)
                return transform;

            // Your HealthBar is a MonoBehaviour on the spawned prefab.
            // If it's present under this unit, follow it.
            var bar = GetComponentInChildren<Havengard.HealthSystem.HealthBar>(includeInactive: true);
            if (bar != null)
                return bar.transform;

            return transform;
        }

        private void OnHealthChanged()
        {
            TryInit();

            var hs = health.GetHealthSystem();
            if (hs == null) return;

            int current = hs.GetHealth();
            int delta = current - lastHealth; // negative = damage, positive = heal

            if (Mathf.Abs(delta) < minimumAmountToShow)
            {
                lastHealth = current;
                return;
            }

            // Spawn position / follow target
            Transform follow = followedTarget != null ? followedTarget : transform;
            Vector3 spawnPos = follow.position + spawnOffset;

            if (follow == transform)
                spawnPos += fallbackWorldOffset;

            if (delta < 0)
            {
                // Damage
                int dmg = -delta;
                if (damageNumberPrefab != null)
                {
                    // Spawn and follow target (keeps it above healthbar)
                    damageNumberPrefab.Spawn(spawnPos, dmg, follow);
                }
            }
            else
            {
                // Heal
                int heal = delta;
                if (healNumberPrefab != null)
                {
                    healNumberPrefab.Spawn(spawnPos, heal, follow);
                }
            }

            lastHealth = current;
        }
    }
}
