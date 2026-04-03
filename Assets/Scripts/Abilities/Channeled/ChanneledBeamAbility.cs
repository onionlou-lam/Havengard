using UnityEngine;
using Havengard.Combat;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Channeled/Beam Ability")]
    public class ChanneledBeamAbility : ChanneledAbilityBase
    {
        [Header("Beam Configuration")]
        [SerializeField] private float beamWidth = 1f;
        [SerializeField] private float beamMaxDistance = 15f;
        [SerializeField] private LayerMask hitLayers;

        [Header("Damage Type Specific Effects")]
        [SerializeField] private float frostSlowPercent = 0.3f;
        [SerializeField] private float fireBurnDamagePerTick = 5f;
        [SerializeField] private int lightningChainCount = 2;
        [SerializeField] private float holyHealingBonus = 1.2f;

        // Public property for ChannelController
        public float BeamMaxRange => beamMaxDistance;

        // ChannelController calls this with (GameObject, float)
        public override void OnChannelTick(GameObject caster, float chargePercent)
        {
            // This is for ChannelController
            // Damage calculations happen here
        }

        // AbilityUser calls this with (AbilityUser, Vector3, GameObject)
        protected override void OnChannelTick(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            if (user == null) return;

            Vector3 direction = (targetPosition - user.transform.position).normalized;
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                user.transform.position,
                direction,
                beamMaxDistance,
                hitLayers
            );

            foreach (var hit in hits)
            {
                if (hit.collider == null || hit.collider.gameObject == user.gameObject)
                    continue;

                GameObject target = hit.collider.gameObject;
                ApplyDamage(target, user.gameObject);
            }
        }

        private void ApplyDamage(GameObject target, GameObject caster)
        {
            var health = target.GetComponent<Havengard.Core.HealthSystem.Health>();
            if (health != null)
            {
                float damage = CalculateDamage(caster);
                health.TakeDamage((int)damage, caster);
            }
        }
    }
}