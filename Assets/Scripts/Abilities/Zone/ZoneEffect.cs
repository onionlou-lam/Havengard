using Havengard.Combat;
using Havengard.Core.HealthSystem;
using Havengard.Statuses;
using Havengard.Units;
using System.Collections;
using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Attach this to a zone prefab.  Handles damage, VFX, and SFX.
    /// The prefab should have VFX as child GameObjects.
    /// </summary>
    public class ZoneEffect : MonoBehaviour
    {
        [Header("Zone Settings")]
        [SerializeField] private float areaOfEffectRadius = 5f;
        [SerializeField] private float duration = 5f;
        [SerializeField] private float delayBeforeEffect = 1f;

        [Header("Damage Settings")]
        [SerializeField] private int damagePerTick = 5;
        [SerializeField] private float tickInterval = 0.5f;
        [SerializeField] private bool friendlyFire = false;

        [Header("VFX (Optional - Can be child objects)")]
        [SerializeField] private ParticleSystem spawnVFX;
        [SerializeField] private ParticleSystem activeVFX;

        [Header("SFX")]
        [SerializeField] private AudioClip spawnSFX;
        [SerializeField] private AudioClip loopSFX;

        private GameObject caster;
        private Faction casterFaction;
        private bool followsCaster;
        private StatusEffectData statusEffect;
        private int maxStatusStacks;
        private AudioSource audioSource;

        public void Initialize(GameObject caster, bool followsCaster, StatusEffectData statusEffect = null, int maxStatusStacks = 1)
        {
            this.caster = caster;
            this.followsCaster = followsCaster;
            this.statusEffect = statusEffect;
            this.maxStatusStacks = maxStatusStacks;

            var casterHealth = caster.GetComponent<IHealth>();
            casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            // Play spawn VFX
            if (spawnVFX != null)
                spawnVFX.Play();

            // Play spawn SFX
            if (spawnSFX != null)
                AudioSource.PlayClipAtPoint(spawnSFX, transform.position);

            // Setup looping audio
            if (loopSFX != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = loopSFX;
                audioSource.loop = true;
                audioSource.spatialBlend = 0.5f; // 2D/3D mix
                audioSource.Play();
            }

            StartCoroutine(EffectRoutine());
        }

        private IEnumerator EffectRoutine()
        {
            // Wait before activating
            yield return new WaitForSeconds(delayBeforeEffect);

            // Play active VFX
            if (activeVFX != null)
                activeVFX.Play();

            float elapsed = 0f;
            float nextTickTime = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Follow caster if needed
                if (followsCaster && caster != null)
                    transform.position = caster.transform.position;

                // Apply damage at intervals
                if (elapsed >= nextTickTime)
                {
                    nextTickTime = elapsed + tickInterval;
                    ApplyEffects();
                }

                yield return null;
            }

            // Cleanup
            if (audioSource != null)
                audioSource.Stop();

            Destroy(gameObject);
        }

        private void ApplyEffects()
        {
            foreach (var hit in Physics2D.OverlapCircleAll(transform.position, areaOfEffectRadius))
            {
                var health = hit.GetComponent<IHealth>();
                if (health != null && FactionUtility.CanDamage(casterFaction, health, friendlyFire))
                {
                    health.GetHealthSystem().Damage(damagePerTick);

                    if (statusEffect != null)
                        StatusEffectApplier.ApplyEffect(hit.gameObject, statusEffect, maxStatusStacks);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, areaOfEffectRadius);
        }
    }
}