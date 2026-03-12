using Havengard.Core.HealthSystem;
using Havengard.Statuses;
using Havengard.Units;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Wall Ability")]
    public class WallAbility : AbilityBase
    {
        [Header("Wall Prefab")]
        [SerializeField] private GameObject wallPrefab; // Prefab with WallEffect component
        [SerializeField] private float maximumRange = 15f;

        [Header("Wall Properties")]
        [SerializeField] private float wallDuration = 10f;
        [SerializeField] private WallBehaviorType behaviorType = WallBehaviorType.Blocking;

        [Header("Wall Health (if targetable)")]
        [SerializeField] private bool isTargetable = true;
        [SerializeField] private int wallMaxHealth = 100;
        [SerializeField] private bool showHealthBar = true;

        [Header("Damage (for pass-through walls)")]
        [SerializeField] private int damagePerTick = 5;
        [SerializeField] private float damageTickRate = 0.5f;
        [SerializeField] private bool friendlyFire = false;

        [Header("Status Effects")]
        [SerializeField] private StatusEffectData statusEffect;
        [SerializeField] private int maxStatusStacks = 1;

        // REMOVED 'override' - not in base class
        public bool CanCast(GameObject caster, GameObject target)
        {
            if (wallPrefab == null) return false;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            return Vector3.Distance(caster.transform.position, mouseWorld) <= maximumRange;
        }

        // REMOVED 'override' - not in base class
        public void Cast(GameObject caster, GameObject target)
        {
            if (wallPrefab == null || !CanCast(caster, target)) return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            GameObject wallInstance = Instantiate(wallPrefab, mouseWorld, Quaternion.identity);

            // Initialize the wall effect component
            var wallEffect = wallInstance.GetComponent<WallEffect>();
            if (wallEffect != null)
            {
                wallEffect.Initialize(
                    caster,
                    wallDuration,
                    behaviorType,
                    isTargetable,
                    wallMaxHealth,
                    showHealthBar,
                    damagePerTick,
                    damageTickRate,
                    friendlyFire,
                    statusEffect,
                    maxStatusStacks
                );
            }
            else
            {
                Debug.LogError($"[WallAbility] Wall prefab '{wallPrefab.name}' is missing WallEffect component!");
                Destroy(wallInstance);
            }
        }

        // ADD: Implement abstract methods from AbilityBase
        public override void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            Cast(user.gameObject, targetEnemy);
        }

        public override void Deactivate(AbilityUser user)
        {
            // Walls handle their own cleanup
        }
    }

    public enum WallBehaviorType
    {
        Blocking,       // Units cannot pass through
        PassThrough,    // Units can pass through and take damage/effects
        OneWay          // Allies can pass, enemies cannot (or vice versa)
    }
}