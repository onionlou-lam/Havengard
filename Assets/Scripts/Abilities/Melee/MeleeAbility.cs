using UnityEngine;
using Havengard.Core.HealthSystem;
using Havengard.Units;
using Havengard.Combat;
using System.Collections.Generic;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Melee Ability")]
    public class MeleeAbility : AbilityBase
    {
        [Header("Hit Detection")]
        [SerializeField] private MeleeHitShape hitShape = MeleeHitShape.Circle;
        [SerializeField] private float hitRadius = 1.5f;
        [SerializeField] private Vector2 hitBoxSize = new Vector2(2f, 1.5f);
        [SerializeField] private float hitArcAngle = 120f;
        [SerializeField] private float hitRange = 2f;
        [SerializeField] private Vector2 hitOffset = Vector2.zero;

        [Header("Timing")]
        [SerializeField] private float hitDelay = 0.2f;
        [SerializeField] private float activeHitDuration = 0f;

        [Header("Damage")]
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private bool friendlyFire = false;
        [SerializeField] private bool preventMultiHit = true;

        [Header("Knockback")]
        [SerializeField] private bool enableKnockback = false;
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private float knockbackDuration = 0.2f;

        [Header("Movement")]
        [SerializeField] private bool enableLunge = false;
        [SerializeField] private float lungeDistance = 2f;
        [SerializeField] private float lungeDuration = 0.15f;

        [Header("VFX")]
        [SerializeField] private GameObject hitVFXPrefab;
        [SerializeField] private GameObject casterVFXPrefab;
        [SerializeField] private bool spawnVFXOnEachHit = true;
        [SerializeField] private bool rotateVFXToDirection = true;
        [SerializeField] private float vfxRotationOffset = 0f;
        [SerializeField] private bool spawnCasterVFXAtHitPosition = false;

        [Header("SFX")]
        [SerializeField] private AudioClip swingSFX;
        [SerializeField] private AudioClip hitSFX;
        [SerializeField] private bool randomizeSwingPitch = false;
        [SerializeField] private bool randomizeHitPitch = false;
        [SerializeField] private float minPitch = 0.85f;
        [SerializeField] private float maxPitch = 1.15f;

        private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

        public override void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            if (user == null) return;

            Vector2 attackDirection = targetEnemy != null
                ? (Vector2)(targetEnemy.transform.position - user.transform.position).normalized
                : (Vector2)(targetPosition - user.transform.position).normalized;

            // Play swing SFX
            if (swingSFX != null)
            {
                PlayAudioWithPitch(swingSFX, user.transform.position, randomizeSwingPitch);
            }

            // Spawn caster VFX
            if (casterVFXPrefab != null)
            {
                SpawnCasterVFX(user.transform.position, attackDirection);
            }

            // Start delayed hit detection
            user.StartCoroutine(DelayedHitDetection(user.gameObject, attackDirection));
        }

        private System.Collections.IEnumerator DelayedHitDetection(GameObject caster, Vector2 direction)
        {
            yield return new UnityEngine.WaitForSeconds(hitDelay);

            hitTargets.Clear();
            PerformHitDetection(caster, direction);
        }

        private void PerformHitDetection(GameObject caster, Vector2 direction)
        {
            Vector2 origin = (Vector2)caster.transform.position + hitOffset;
            Collider2D[] hits = null;

            switch (hitShape)
            {
                case MeleeHitShape.Circle:
                    hits = Physics2D.OverlapCircleAll(origin, hitRadius, targetLayers);
                    break;
                case MeleeHitShape.Box:
                    hits = Physics2D.OverlapBoxAll(origin, hitBoxSize, 0f, targetLayers);
                    break;
                    // Add other shapes as needed
            }

            if (hits != null)
            {
                foreach (var hit in hits)
                {
                    if (hit.gameObject == caster) continue;
                    if (preventMultiHit && hitTargets.Contains(hit.gameObject)) continue;

                    ApplyHitEffects(caster, hit.gameObject, direction);
                    hitTargets.Add(hit.gameObject);
                }
            }
        }

        private void ApplyHitEffects(GameObject caster, GameObject target, Vector2 direction)
        {
            // Apply damage
            var health = target.GetComponent<Havengard.Core.HealthSystem.Health>();
            if (health != null)
            {
                float damage = CalculateDamage(caster) * damageMultiplier;
                health.TakeDamage((int)damage, caster);
            }

            // Spawn hit VFX
            if (hitVFXPrefab != null && spawnVFXOnEachHit)
            {
                Instantiate(hitVFXPrefab, target.transform.position, Quaternion.identity);
            }

            // Play hit SFX
            if (hitSFX != null)
            {
                PlayAudioWithPitch(hitSFX, target.transform.position, randomizeHitPitch);
            }

            // Apply knockback
            if (enableKnockback)
            {
                var rb = target.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }

        private void SpawnCasterVFX(Vector3 position, Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = rotateVFXToDirection
                ? Quaternion.Euler(0, 0, angle + vfxRotationOffset)
                : Quaternion.identity;

            GameObject vfx = Instantiate(casterVFXPrefab, position, rotation);
            Destroy(vfx, 2f);
        }

        private void PlayAudioWithPitch(AudioClip clip, Vector3 position, bool randomize)
        {
            if (randomize)
            {
                float pitch = Random.Range(minPitch, maxPitch);
                // You'll need an audio manager or AudioSource.PlayClipAtPoint with pitch
                AudioSource.PlayClipAtPoint(clip, position);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, position);
            }
        }

        public override void Deactivate(AbilityUser user)
        {
            hitTargets.Clear();
        }
    }

    public enum MeleeHitShape
    {
        Circle,
        Box,
        Arc,
        Cone
    }
}