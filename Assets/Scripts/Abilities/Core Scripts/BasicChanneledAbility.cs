using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Character;

namespace Havengard.Abilities
{
    /// <summary>
    /// Simple example channeled ability - useful for testing or as a template
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Abilities/Basic Channeled (Example)")]
    public class BasicChanneledAbility : ChanneledAbilityBase
    {
        [Header("Release Effect")]
        [SerializeField] private GameObject releasePrefab;
        [SerializeField] private int baseDamage = 50;
        [SerializeField] private float damageScaling = 2f;
        [SerializeField] private float releaseRadius = 3f;
        
        public override bool CanCast(GameObject caster, GameObject target)
        {
            return caster != null;
        }

        public override void OnChannelTick(GameObject caster, float chargePercent)
        {
            base.OnChannelTick(caster, chargePercent);
            // Optional: add per-tick effects here
        }

        public override void OnRelease(GameObject caster, GameObject target, float chargePercent)
        {
            // Calculate damage based on charge
            int damage = Mathf.RoundToInt(baseDamage + (baseDamage * damageScaling * chargePercent));
            
            Debug.Log($"Released basic channeled ability at {chargePercent * 100}% charge for {damage} damage");
            
            // Spawn release VFX if configured
            if (releasePrefab != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0;
                
                GameObject releaseVFX = Instantiate(releasePrefab, mouseWorld, Quaternion.identity);
                
                // Auto-destroy after particle lifetime
                var ps = releaseVFX.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(releaseVFX, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(releaseVFX, 3f);
                }
            }

            // Apply damage in radius (example AoE release)
            Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, releaseRadius);
            foreach (var hit in hits)
            {
                if (hit.gameObject == caster) continue;
                
                var health = hit.GetComponent<HealthSystem.IHealth>();
                if (health != null)
                {
                    health.GetHealthSystem().Damage(damage);
                    ApplyBuffDebuff(hit.gameObject);
                }
            }
        }

        public override void OnChannelCancel(GameObject caster)
        {
            base.OnChannelCancel(caster);
            Debug.Log("Basic channeled ability cancelled");
        }
    }
}