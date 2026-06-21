using Havengard.Combat;
using Havengard.Core.HealthSystem;
using Havengard.Statuses;
using Havengard.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Abilities
{
    public class OrbitalExplosionEffect : MonoBehaviour
    {
        private GameObject caster;
        private Faction casterFaction;

        private float vortexSpeed;
        private float vortexDuration;
        private float explosionRadius;
        private float explosionDamage;

        private bool enableKnockback;
        private float knockbackForce;
        private float knockbackDuration;
        private GameObject explosionVFXPrefab;

        // Vortex damage over time
        private int vortexDamagePerTick;
        private float vortexTickInterval;
        private StatusEffectData vortexStatusEffect;
        private int maxVortexStatusStacks;

        // Scaling animation
        private Vector3 startScale;
        private Vector3 targetScale;
        private float scaleDuration;
        private AnimationCurve scaleCurve;

        private readonly HashSet<Rigidbody2D> affectedRigidbodies = new();

        [Header("VFX")]
        [SerializeField] private ParticleSystem vortexVFX;
        [SerializeField] private ParticleSystem explosionVFX;

        [Header("SFX")]
        [SerializeField] private AudioClip vortexSFX;
        [SerializeField] private AudioClip explosionSFX;

        private AudioSource audioSource;
        private bool isDestroying = false;

        public void Initialize(
            GameObject caster,
            Faction casterFaction,
            float vortexSpeed,
            float vortexDuration,
            float explosionRadius,
            float explosionDamage,
            bool enableKnockback,
            float knockbackForce,
            float knockbackDuration,
            GameObject explosionVFXPrefab = null,
            int vortexDamagePerTick = 0,
            float vortexTickInterval = 0.5f,
            StatusEffectData vortexStatusEffect = null,
            int maxVortexStatusStacks = 1,
            Vector3? startScale = null,
            Vector3? targetScale = null,
            float scaleDuration = 0f,
            AnimationCurve scaleCurve = null)
        {
            Debug.Log($"[OrbitalExplosion] Initialize called");
            this.caster = caster;
            this.casterFaction = casterFaction;
            this.vortexSpeed = vortexSpeed;
            this.vortexDuration = vortexDuration;
            this.explosionRadius = explosionRadius;
            this.explosionDamage = explosionDamage;
            this.enableKnockback = enableKnockback;
            this.knockbackForce = knockbackForce;
            this.knockbackDuration = knockbackDuration;
            this.explosionVFXPrefab = explosionVFXPrefab;

            // Vortex damage over time
            this.vortexDamagePerTick = vortexDamagePerTick;
            this.vortexTickInterval = vortexTickInterval;
            this.vortexStatusEffect = vortexStatusEffect;
            this.maxVortexStatusStacks = maxVortexStatusStacks;

            // Scaling animation
            this.startScale = startScale ?? Vector3.one;
            this.targetScale = targetScale ?? Vector3.one;
            this.scaleDuration = scaleDuration;
            this.scaleCurve = scaleCurve ?? AnimationCurve.Linear(0, 0, 1, 1);

            Debug.Log($"Vortex VFX assigned: {vortexVFX != null}");
            Debug.Log($"Explosion VFX assigned: {explosionVFX != null}");
            Debug.Log($"Explosion VFX Prefab assigned: {explosionVFXPrefab != null}");
            Debug.Log($"Vortex SFX assigned: {vortexSFX != null}");
            Debug.Log($"Explosion SFX assigned: {explosionSFX != null}");
            Debug.Log($"Vortex Damage Per Tick: {vortexDamagePerTick}");

            if (vortexVFX != null)
                vortexVFX.Play();

            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D audio

            if (vortexSFX != null)
            {
                audioSource.clip = vortexSFX;
                audioSource.loop = true;
                audioSource.Play();

                Debug.Log("[OrbitalExplosion] Playing vortex SFX");
            }

            StartCoroutine(OrbitalExplosionRoutine());

            // Start scaling animation if configured
            if (scaleDuration > 0f && this.startScale != this.targetScale)
            {
                StartCoroutine(ScaleAnimation());
            }
        }

        private IEnumerator OrbitalExplosionRoutine()
        {
            float elapsed = 0f;
            float nextTickTime = 0f;

            while (elapsed < vortexDuration)
            {
                PullEnemiesToCenter();

                // Apply vortex damage over time
                if (vortexDamagePerTick > 0 && elapsed >= nextTickTime)
                {
                    nextTickTime = elapsed + vortexTickInterval;
                    ApplyVortexDamage();
                }

                elapsed += Time.deltaTime;

                yield return null;
            }

            // Stop and cleanup audio properly before explosion
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.loop = false;
                Debug.Log("[OrbitalExplosion] Stopped vortex SFX");
            }

            // Small delay before explosion
            yield return new WaitForSeconds(0.1f);

            Explode();

            // Calculate destroy delay based on VFX duration
            float destroyDelay = 2f;

            if (explosionVFX != null)
            {
                destroyDelay = explosionVFX.main.duration + explosionVFX.main.startLifetime.constantMax + 0.5f;
            }
            else if (explosionVFXPrefab != null)
            {
                // If using prefab, check its particle system
                ParticleSystem prefabPS = explosionVFXPrefab.GetComponent<ParticleSystem>();
                if (prefabPS != null)
                {
                    destroyDelay = prefabPS.main.duration + prefabPS.main.startLifetime.constantMax + 0.5f;
                }
            }

            Debug.Log($"[OrbitalExplosion] Destroying in {destroyDelay} seconds");

            isDestroying = true;
            yield return new WaitForSeconds(destroyDelay);

            // Final cleanup
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }

            Destroy(gameObject);
        }

        private IEnumerator ScaleAnimation()
        {
            float elapsed = 0f;
            transform.localScale = startScale;

            Debug.Log($"[OrbitalExplosion] Starting scale animation from {startScale} to {targetScale} over {scaleDuration}s");

            while (elapsed < scaleDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / scaleDuration);
                float curveValue = scaleCurve.Evaluate(t);

                transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);

                yield return null;
            }

            transform.localScale = targetScale;
            Debug.Log($"[OrbitalExplosion] Scale animation complete");
        }

        private void PullEnemiesToCenter()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == caster)
                    continue;

                IHealth health = hit.GetComponent<IHealth>();

                if (health == null)
                    continue;

                if (!FactionUtility.CanDamage(casterFaction, health, false))
                    continue;

                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();

                if (rb == null)
                    continue;

                affectedRigidbodies.Add(rb);

                Vector2 direction = ((Vector2)transform.position - rb.position).normalized;

                rb.linearVelocity += direction * vortexSpeed * Time.deltaTime;
            }
        }

        private void ApplyVortexDamage()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == caster)
                    continue;

                IHealth health = hit.GetComponent<IHealth>();

                if (health == null)
                    continue;

                if (!FactionUtility.CanDamage(casterFaction, health, false))
                    continue;

                // Apply damage
                health.GetHealthSystem()?.Damage(vortexDamagePerTick);

                // Apply status effect if configured
                if (vortexStatusEffect != null)
                {
                    StatusEffectApplier applier = hit.GetComponent<StatusEffectApplier>();
                    if (applier != null)
                    {
                        for (int i = 0; i < maxVortexStatusStacks; i++)
                        {
                            applier.ApplyStatusEffect(vortexStatusEffect, caster);
                        }
                    }
                }
            }
        }

        private void Explode()
        {
            Debug.Log($"[OrbitalExplosion] EXPLODE fired at {transform.position}");

            // Stop vortex VFX
            if (vortexVFX != null)
                vortexVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // Handle explosion VFX - prefab takes priority
            if (explosionVFXPrefab != null)
            {
                GameObject vfxInstance = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
                Debug.Log($"[OrbitalExplosion] Instantiated explosion VFX prefab: {vfxInstance.name}");

                // Optionally parent it to this object so it gets cleaned up together
                vfxInstance.transform.SetParent(transform);
            }
            else if (explosionVFX != null)
            {
                explosionVFX.gameObject.SetActive(true);
                explosionVFX.Clear();
                explosionVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                explosionVFX.Play(true);

                Debug.Log("[OrbitalExplosion] Playing explosion VFX component");
            }

            // Play explosion SFX
            if (audioSource != null && explosionSFX != null)
            {
                audioSource.loop = false;
                audioSource.PlayOneShot(explosionSFX);

                Debug.Log("[OrbitalExplosion] Playing explosion SFX");
            }

            // Apply damage and knockback
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == caster)
                    continue;

                IHealth health = hit.GetComponent<IHealth>();

                if (health == null)
                    continue;

                if (!FactionUtility.CanDamage(casterFaction, health, false))
                    continue;

                health.GetHealthSystem()?.Damage(Mathf.RoundToInt(explosionDamage));

                if (!enableKnockback)
                    continue;

                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();

                if (rb == null)
                    continue;

                Vector2 direction = (rb.position - (Vector2)transform.position).normalized;

                ApplyKnockback(rb, direction * knockbackForce, knockbackDuration);
            }
        }

        private void ApplyKnockback(Rigidbody2D rb, Vector2 force, float duration)
        {
            MeleeKnockbackHandler handler = rb.GetComponent<MeleeKnockbackHandler>();

            if (handler == null)
            {
                handler = rb.gameObject.AddComponent<MeleeKnockbackHandler>();
            }

            handler.ApplyKnockback(rb, force, duration);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

        private void OnDestroy()
        {
            // Emergency cleanup if destroyed prematurely
            if (!isDestroying && audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.clip = null;
                Debug.Log("[OrbitalExplosion] Emergency audio cleanup in OnDestroy");
            }
        }
    }
}