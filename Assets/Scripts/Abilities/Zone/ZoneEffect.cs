using Havengard.Combat;
using Havengard.Core.HealthManagement;
using Havengard.Statuses;
using Havengard.Units;
using System.Collections;
using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Attach this to a zone prefab. Handles damage, VFX, and SFX.
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
        private Havengard.Audio.GameAudioManager.LoopHandle loopHandle;
        private bool isDestroying = false;

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

            // Play spawn SFX (one-shot, not looping) — routed through GameAudioManager for
            // concurrency limiting when many zones spawn at once.
            if (spawnSFX != null)
            {
                if (Havengard.Audio.GameAudioManager.Instance != null)
                    Havengard.Audio.GameAudioManager.Instance.PlaySFX(spawnSFX, transform.position);
                else
                    AudioSource.PlayClipAtPoint(spawnSFX, transform.position);
            }

            // Setup looping audio via GameAudioManager so it automatically pauses/resumes
            // with the game's pause state instead of ignoring Time.timeScale.
            if (loopSFX != null)
            {
                if (Havengard.Audio.GameAudioManager.Instance != null)
                {
                    loopHandle = Havengard.Audio.GameAudioManager.Instance.PlayLoop(loopSFX, transform, 1f, 0.5f);
                }
                else
                {
                    // Fallback: legacy behavior if no manager is present in the scene
                    var fallbackSource = GetComponent<AudioSource>();
                    if (fallbackSource == null)
                        fallbackSource = gameObject.AddComponent<AudioSource>();

                    fallbackSource.clip = loopSFX;
                    fallbackSource.loop = true;
                    fallbackSource.playOnAwake = false;
                    fallbackSource.spatialBlend = 0.5f;
                    fallbackSource.Play();
                }
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

            // Proper cleanup before destruction
            isDestroying = true;

            if (loopHandle.IsValid && Havengard.Audio.GameAudioManager.Instance != null)
            {
                Havengard.Audio.GameAudioManager.Instance.StopLoop(loopHandle);
            }
            else
            {
                // Fallback cleanup for legacy AudioSource path
                var fallbackSource = GetComponent<AudioSource>();
                if (fallbackSource != null && fallbackSource.isPlaying)
                {
                    fallbackSource.Stop();
                    fallbackSource.loop = false;
                    fallbackSource.clip = null;
                }
            }

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
                    {
                        var applier = hit.GetComponent<StatusEffectApplier>();
                        if (applier != null)
                        {
                            for (int i = 0; i < maxStatusStacks; i++)
                            {
                                applier.ApplyStatusEffect(statusEffect, caster);
                            }
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, areaOfEffectRadius);
        }

        private void OnDestroy()
        {
            // Emergency cleanup if destroyed prematurely
            if (!isDestroying)
            {
                if (loopHandle.IsValid && Havengard.Audio.GameAudioManager.Instance != null)
                {
                    Havengard.Audio.GameAudioManager.Instance.StopLoop(loopHandle);
                }
                else
                {
                    // Fallback cleanup for legacy AudioSource path
                    var fallbackSource = GetComponent<AudioSource>();
                    if (fallbackSource != null && fallbackSource.isPlaying)
                    {
                        fallbackSource.Stop();
                        fallbackSource.loop = false;
                        fallbackSource.clip = null;
                    }
                }
            }
        }
    }
}