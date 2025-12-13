using Havengard.Combat;
using Havengard.HealthSystem;
using Havengard.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Abilities
{
    [RequireComponent(typeof(Collider2D))]
    public class WallOfFireRuntime : MonoBehaviour
    {
        private float duration;
        private float tickInterval;
        private int damagePerTick;
        private Faction sourceFaction;
        private bool friendlyFire;

        private readonly Dictionary<IHealth, float> tickTimers = new();

        public void Init(
            float lifetime,
            float tickRate,
            int tickDamage,
            Faction faction,
            bool allowFriendlyFire
        )
        {
            duration = lifetime;
            tickInterval = Mathf.Max(0.1f, tickRate);
            damagePerTick = tickDamage;
            sourceFaction = faction;
            friendlyFire = allowFriendlyFire;

            Destroy(gameObject, duration);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponent<IHealth>();
            if (health == null) return;
            if (!FactionUtility.CanDamage(sourceFaction, health, friendlyFire)) return;

            // Start ticking immediately for this target
            if (!tickTimers.ContainsKey(health))
                tickTimers.Add(health, 0f);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var health = other.GetComponent<IHealth>();
            if (health == null) return;

            tickTimers.Remove(health);
        }

        private void Update()
        {
            if (tickTimers.Count == 0) return;

            var keys = new List<IHealth>(tickTimers.Keys);

            foreach (var health in keys)
            {
                if (health == null)
                {
                    tickTimers.Remove(health);
                    continue;
                }

                tickTimers[health] += Time.deltaTime;

                if (tickTimers[health] >= tickInterval)
                {
                    tickTimers[health] = 0f;
                    health.GetHealthSystem().Damage(damagePerTick);
                }
            }
        }
    }
}
