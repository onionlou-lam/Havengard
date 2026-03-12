using UnityEngine;
using System.Collections.Generic;
using Havengard.Core.HealthSystem;
using Havengard.Units;
using Havengard.Statuses;
using Havengard.Combat;

namespace Havengard.Abilities
{
    [RequireComponent(typeof(Collider2D))]
    public class WallEffect : MonoBehaviour
    {
        private GameObject caster;
        private float duration;
        private WallBehaviorType behaviorType;
        private bool isTargetable;
        private int maxHealth;
        private bool showHealthBar;
        private int damagePerTick;
        private float damageTickRate;
        private bool friendlyFire;
        private StatusEffectData statusEffect;
        private int maxStatusStacks;

        private float spawnTime;
        private Health wallHealth;
        private Collider2D wallCollider;
        private Dictionary<GameObject, float> lastDamageTimes = new Dictionary<GameObject, float>();
        private HashSet<GameObject> unitsInWall = new HashSet<GameObject>();

        public void Initialize(
            GameObject caster,
            float duration,
            WallBehaviorType behaviorType,
            bool isTargetable,
            int maxHealth,
            bool showHealthBar,
            int damagePerTick,
            float damageTickRate,
            bool friendlyFire,
            StatusEffectData statusEffect,
            int maxStatusStacks)
        {
            this.caster = caster;
            this.duration = duration;
            this.behaviorType = behaviorType;
            this.isTargetable = isTargetable;
            this.maxHealth = maxHealth;
            this.showHealthBar = showHealthBar;
            this.damagePerTick = damagePerTick;
            this.damageTickRate = damageTickRate;
            this.friendlyFire = friendlyFire;
            this.statusEffect = statusEffect;
            this.maxStatusStacks = maxStatusStacks;

            spawnTime = Time.time;
            wallCollider = GetComponent<Collider2D>();

            SetupWallBehavior();
            SetupWallHealth();
        }

        private void SetupWallBehavior()
        {
            if (wallCollider == null) return;

            switch (behaviorType)
            {
                case WallBehaviorType.Blocking:
                    wallCollider.isTrigger = false; // Solid wall
                    break;

                case WallBehaviorType.PassThrough:
                case WallBehaviorType.OneWay:
                    wallCollider.isTrigger = true; // Pass-through wall
                    break;
            }
        }

        private void SetupWallHealth()
        {
            if (!isTargetable) return;

            // Add Health component if targetable
            wallHealth = gameObject.AddComponent<Health>();
            
            var casterHealth = caster.GetComponent<IHealth>();
            Faction wallFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            wallHealth.SetStartingMaxHealth(maxHealth);
            wallHealth.SetFaction(wallFaction);

            // Subscribe to death event
            wallHealth.GetHealthSystem().OnDeath += OnWallDestroyed;

            // Show health bar if configured
            if (showHealthBar)
            {
                // Health bar will be spawned automatically by HealthBarSpawner if present
                Debug.Log($"[WallEffect] Wall created with {maxHealth} HP");
            }
        }

        private void Update()
        {
            // Check duration
            if (Time.time >= spawnTime + duration)
            {
                DestroyWall();
                return;
            }

            // Apply damage to units in pass-through walls
            if (behaviorType == WallBehaviorType.PassThrough)
            {
                ApplyPassThroughDamage();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (behaviorType == WallBehaviorType.PassThrough)
            {
                unitsInWall.Add(other.gameObject);
            }
            else if (behaviorType == WallBehaviorType.OneWay)
            {
                HandleOneWayCollision(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (behaviorType == WallBehaviorType.PassThrough)
            {
                unitsInWall.Remove(other.gameObject);
                lastDamageTimes.Remove(other.gameObject);
            }
        }

        private void ApplyPassThroughDamage()
        {
            var casterHealth = caster.GetComponent<IHealth>();
            Faction casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            // Create a copy to avoid modification during iteration
            var unitsToCheck = new List<GameObject>(unitsInWall);

            foreach (var unit in unitsToCheck)
            {
                if (unit == null)
                {
                    unitsInWall.Remove(unit);
                    continue;
                }

                // Check if unit can be damaged
                var health = unit.GetComponent<IHealth>();
                if (health == null) continue;

                if (!FactionUtility.CanDamage(casterFaction, health, friendlyFire))
                    continue;

                // Check damage tick timing
                if (!lastDamageTimes.ContainsKey(unit))
                    lastDamageTimes[unit] = 0f;

                if (Time.time - lastDamageTimes[unit] >= damageTickRate)
                {
                    // Apply damage
                    health.GetHealthSystem().Damage(damagePerTick);
                    lastDamageTimes[unit] = Time.time;

                    // Apply status effect
                    if (statusEffect != null)
                    {
                        StatusEffectApplier.ApplyEffect(unit, statusEffect, maxStatusStacks);
                    }

                    Debug.Log($"[WallEffect] Damaged {unit.name} for {damagePerTick}");
                }
            }
        }

        private void HandleOneWayCollision(Collider2D other)
        {
            // Implement one-way logic based on faction
            var casterHealth = caster.GetComponent<IHealth>();
            Faction casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            var otherHealth = other.GetComponent<IHealth>();
            if (otherHealth == null) return;

            Faction otherFaction = otherHealth.GetFaction();

            // Example: Allies can pass, enemies cannot
            // You can customize this logic as needed
            if (otherFaction == casterFaction)
            {
                // Same faction - allow pass through
                Physics2D.IgnoreCollision(other, wallCollider, true);
            }
            else
            {
                // Different faction - block
                Physics2D.IgnoreCollision(other, wallCollider, false);
            }
        }

        private void OnWallDestroyed()
        {
            Debug.Log($"[WallEffect] Wall destroyed by damage");
            DestroyWall();
        }

        private void DestroyWall()
        {
            // Cleanup
            if (wallHealth != null)
            {
                wallHealth.GetHealthSystem().OnDeath -= OnWallDestroyed;
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            // Cleanup on destroy
            lastDamageTimes.Clear();
            unitsInWall.Clear();
        }
    }
}