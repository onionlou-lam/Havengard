using Havengard.Combat;
using Havengard.Core.HealthSystem;
using Havengard.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Orbital Explosion effect that pulls enemies towards center like a vortex,
    /// then explodes dealing AOE damage with optional knockback.
    /// Attach this component to the orbital explosion zone prefab.
    /// </summary>
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

        private HashSet<Rigidbody2D> affectedRigidbodies = new HashSet<Rigidbody2D>();
        private bool isVortexActive = false;

        [Header("VFX (Optional - Can be child objects)")]
        [SerializeField] private ParticleSystem vortexVFX;
        [SerializeField] private ParticleSystem explosionVFX;

        [Header("SFX")]
        [SerializeField] private AudioClip vortexSFX;
        [SerializeField] private AudioClip explosionSFX;

        private AudioSource audioSource;

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
            GameObject explosionVFXPrefab)
        {
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

            // Play vortex VFX
            if (vortexVFX != null)
                vortexVFX.Play();

            // Play vortex SFX
            if (vortexSFX != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = vortexSFX;
                audioSource.loop = true;
                audioSource.spatialBlend = 0.5f;
                audioSource.Play();
            }

            StartCoroutine(OrbitalExplosionRoutine());
        }

        private IEnumerator OrbitalExplosionRoutine()
        {
            isVortexActive = true;
            float elapsed = 0f;

            // Vortex phase - pull enemies towards center
            while (elapsed < vortexDuration)
            {
                PullEnemiesToCenter();
                elapsed += Time.deltaTime;
                yield return null;
            }

            isVortexActive = false;

            // Stop vortex audio
            if (audioSource != null)
                audioSource.Stop();

            // Explosion phase
            yield return new WaitForSeconds(0.1f); // Small delay before explosion
            Explode();

            // Cleanup
            Destroy(gameObject, 2f); // Keep alive for VFX to finish
        }

        private void PullEnemiesToCenter()
        {
            // Find all enemies in explosion radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (var hit in hits)
            {
                if (hit.gameObject == caster) continue;

                var health = hit.GetComponent<IHealth>();
                if (health != null && FactionUtility.CanDamage(casterFaction, health, false))
                {
                    var rb = hit.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        affectedRigidbodies.Add(rb);

                        // Calculate direction towards center
                        Vector2 direction = (Vector2)transform.position - rb.position;
                        float distance = direction.magnitude;

                        if (distance > 0.1f)
                        {
                            direction.Normalize();
                            // Apply force towards center
                            Vector2 pullForce = direction * vortexSpeed * Time.deltaTime;
                            rb.AddForce(pullForce, ForceMode2D.Impulse);
                        }
                    }
                }
            }
        }

        private void Explode()
        {
            // Stop pulling enemies
            foreach (var rb in affectedRigidbodies)
            {
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
            }

            // Play explosion VFX from prefab if provided
            if (explosionVFXPrefab != null)
            {
                Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
            }
            // Fallback to attached VFX
            else if (explosionVFX != null)
            {
                explosionVFX.Play();
            }

            // Play explosion SFX
            if (explosionSFX != null)
            {
                AudioSource.PlayClipAtPoint(explosionSFX, transform.position);
            }

            // Apply damage and knockback to all enemies in radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (var hit in hits)
            {
                if (hit.gameObject == caster) continue;

                var health = hit.GetComponent<IHealth>();
                if (health != null && FactionUtility.CanDamage(casterFaction, health, false))
                {
                    // Apply damage
                    var healthSystem = health.GetHealthSystem();
                    if (healthSystem != null)
                    {
                        healthSystem.Damage((int)explosionDamage);
                    }

                    // Apply knockback
                    if (enableKnockback)
                    {
                        var rb = hit.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            Vector2 knockbackDirection = (rb.position - (Vector2)transform.position).normalized;
                            ApplyKnockback(rb, knockbackDirection * knockbackForce, knockbackDuration);
                        }
                    }
                }
            }

            // Hide vortex VFX
            if (vortexVFX != null)
                vortexVFX.Stop();
        }

        private void ApplyKnockback(Rigidbody2D rb, Vector2 force, float duration)
        {
            // Use the same knockback system as MeleeAbility
            var knockbackHandler = rb.gameObject.GetComponent<MeleeKnockbackHandler>();
            if (knockbackHandler == null)
            {
                knockbackHandler = rb.gameObject.AddComponent<MeleeKnockbackHandler>();
            }

            knockbackHandler.ApplyKnockback(rb, force, duration);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw vortex/explosion radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

        private void OnDestroy()
        {
            // Reset velocities when destroyed
            if (isVortexActive)
            {
                foreach (var rb in affectedRigidbodies)
                {
                    if (rb != null)
                        rb.linearVelocity = Vector2.zero;
                }
            }
        }
    }
}