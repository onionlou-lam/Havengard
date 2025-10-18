using UnityEngine;
using System.Collections.Generic;
using Havengard.Units;
using Havengard.HealthSystem;
using Havengard.Combat;
using Havengard.Character;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Multishot")]
    public class Multishot : AbilityBase
    {
        [Header("Multishot Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private int projectileCount = 5;
        [SerializeField] private float range = 10f;
        [SerializeField] private float fieldOfView = 60f;
        [SerializeField] private int baseDamage = 15;
        [SerializeField] private float projectileSpeed = 14f;
        [SerializeField] private bool friendlyFire = false;

        [Header("Piercing Settings")]
        [Tooltip("Allow multishot projectiles to pierce through enemies.")]
        [SerializeField] private bool enablePiercing = true;

        [Tooltip("Max number of pierces per arrow.")]
        [SerializeField] private int maxPierces = 2;

        [Tooltip("Damage multiplier per pierce (1 = no falloff, 0.9 = 10% reduction per target).")]
        [Range(0.1f, 1f)]
        [SerializeField] private float damageFalloff = 0.9f;

        [Header("Spread Firing")]
        [SerializeField] private bool fireAsFanPattern = true;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return projectilePrefab != null && caster != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (!CanCast(caster, target)) return;

            var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;
            var stats = caster.GetComponent<StatsComponent>()?.CurrentStats;
            int attackPower = stats != null ? stats.Attack : baseDamage;

            if (fireAsFanPattern)
                FireFanPattern(caster, casterFaction, attackPower);
            else
                FireTowardEnemies(caster, casterFaction, attackPower);
        }

        private void FireFanPattern(GameObject caster, Faction casterFaction, int attackPower)
        {
            Vector3 forward = caster.transform.up;
            float halfSpread = fieldOfView * 0.5f;

            for (int i = 0; i < projectileCount; i++)
            {
                float t = (projectileCount == 1) ? 0.5f : (float)i / (projectileCount - 1);
                float angle = Mathf.Lerp(-halfSpread, halfSpread, t);
                Quaternion rotation = Quaternion.Euler(0, 0, angle);
                Vector3 dir = rotation * forward;

                GameObject proj = Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);
                var projectile = proj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Init(dir, casterFaction, friendlyFire, attackPower, projectileSpeed);
                    projectile.ConfigurePiercing(enablePiercing, maxPierces, damageFalloff);
                }
            }

            Debug.Log($"{caster.name} fired Multishot (fan pattern, {projectileCount} arrows, piercing={enablePiercing}).");
        }

        private void FireTowardEnemies(GameObject caster, Faction casterFaction, int attackPower)
        {
            List<GameObject> enemies = FindEnemiesInCone(caster);
            if (enemies.Count == 0)
            {
                FireFanPattern(caster, casterFaction, attackPower);
                return;
            }

            int targetsHit = 0;
            foreach (var enemy in enemies)
            {
                if (targetsHit >= projectileCount) break;

                Vector3 dir = (enemy.transform.position - caster.transform.position).normalized;
                GameObject proj = Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);

                var projectile = proj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Init(dir, casterFaction, friendlyFire, attackPower, projectileSpeed);
                    projectile.ConfigurePiercing(enablePiercing, maxPierces, damageFalloff);
                }

                targetsHit++;
            }

            Debug.Log($"{caster.name} fired Multishot (targeted, {targetsHit} arrows, piercing={enablePiercing}).");
        }

        private List<GameObject> FindEnemiesInCone(GameObject caster)
        {
            List<GameObject> enemies = new List<GameObject>();
            var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;
            Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, range);
            Vector3 forward = caster.transform.up;

            foreach (var hit in hits)
            {
                var health = hit.GetComponent<IHealth>();
                if (health == null || !FactionUtility.CanDamage(casterFaction, health, friendlyFire)) continue;

                Vector3 toTarget = (hit.transform.position - caster.transform.position).normalized;
                float angle = Vector3.Angle(forward, toTarget);
                if (angle <= fieldOfView * 0.5f)
                    enemies.Add(hit.gameObject);
            }

            enemies.Sort((a, b) =>
                Vector2.Distance(caster.transform.position, a.transform.position)
                .CompareTo(Vector2.Distance(caster.transform.position, b.transform.position))
            );

            return enemies;
        }

        public void ScaleWithLevel(int level)
        {
            projectileCount = Mathf.Clamp(3 + (level / 2), 3, 7);
            baseDamage = 15 + (level * 2);
            range = 10f + (level * 0.5f);
            maxPierces = Mathf.Clamp(2 + (level / 4), 2, 5); // +1 pierce every 4 levels
        }
    }
}
