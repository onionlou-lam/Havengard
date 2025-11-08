/*using Havengard.Combat;
using Havengard.Enemies;
using Havengard.HealthSystem;
using Havengard.Statuses;
using UnityEngine;

namespace Havengard.Units
{
    public class MeleeEnemy : EnemyBase
    {
        [Header("Melee Attack")]
        [SerializeField] private bool friendlyFire = false;

        public override void PerformAttack(GameObject target)
        {
            if (!IsAttackReady() || target == null) return;
            if (GetComponent<StatusEffectInstance>()?.IsSilenced() == true) return;

            var targetHealth = target.GetComponent<IHealth>();
            if (targetHealth == null) return;

            if (FactionUtility.CanDamage(faction, targetHealth, friendlyFire))
            {
                attackEffects?.PlayAttackEffect();

                int finalDamage = CombatCalculator.CalculateDamage(gameObject, target);
                targetHealth.GetHealthSystem().Damage(finalDamage);

                attackEffects?.PlayImpactEffect(target.transform.position);

                Debug.Log($"{name} hit {target.name} for {finalDamage} damage!");
                ResetAttackCooldown();
            }
        }
    }
}
*/