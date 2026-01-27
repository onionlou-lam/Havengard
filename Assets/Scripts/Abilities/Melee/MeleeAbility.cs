using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;
using Havengard.Character;
using Havengard.Statuses;
using System.Collections;
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
        [SerializeField] private float hitArcAngle = 120f; // For arc shape
        [SerializeField] private float hitRange = 2f; // For line/thrust shape
        [Tooltip("Offset from caster position (useful for positioning hitbox)")]
        [SerializeField] private Vector2 hitOffset = Vector2.zero;
        
        [Header("Timing")]
        [Tooltip("Delay before hit detection (for animation sync)")]
        [SerializeField] private float hitDelay = 0.2f;
        [Tooltip("Duration of active hit detection (0 = instant)")]
        [SerializeField] private float activeHitDuration = 0f;

        [Header("Damage")]
        [Tooltip("Damage multiplier based on caster's Attack stat")]
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private int baseDamage = 10;
        [SerializeField] private bool friendlyFire = false;
        [Tooltip("If true, each target can only be hit once per cast")]
        [SerializeField] private bool preventMultiHit = true;

        [Header("Knockback")]
        [SerializeField] private bool enableKnockback = false;
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private float knockbackDuration = 0.2f;

        [Header("Movement")]
        [SerializeField] private bool enableLunge = false;
        [Tooltip("Distance to move toward attack direction")]
        [SerializeField] private float lungeDistance = 2f;
        [SerializeField] private float lungeDuration = 0.15f;

        [Header("VFX")]
        [SerializeField] private GameObject hitVFXPrefab;
        [SerializeField] private GameObject casterVFXPrefab;
        [SerializeField] private bool spawnVFXOnEachHit = true;
        [Tooltip("Rotate VFX to face attack direction")]
        [SerializeField] private bool rotateVFXToDirection = true;
        [Tooltip("Additional rotation offset (useful if your VFX faces wrong direction by default)")]
        [SerializeField] private float vfxRotationOffset = 0f;
        [Tooltip("Spawn VFX at hit position instead of caster position")]
        [SerializeField] private bool spawnCasterVFXAtHitPosition = false;

        [Header("SFX")]
        [SerializeField] private AudioClip swingSFX;
        [SerializeField] private AudioClip hitSFX;

        private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return caster != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (caster == null) return;

            // Get attack direction (toward mouse or target)
            Vector2 attackDirection = GetAttackDirection(caster, target);

            // Play swing SFX
            if (swingSFX != null)
                AudioSource.PlayClipAtPoint(swingSFX, caster.transform.position);

            // Spawn caster VFX
            if (casterVFXPrefab != null)
            {
                // Calculate spawn position
                Vector3 spawnPosition = caster.transform.position;
                if (spawnCasterVFXAtHitPosition)
                {
                    Vector2 rotatedOffset = Quaternion.Euler(0, 0, Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg) * hitOffset;
                    spawnPosition = (Vector2)caster.transform.position + rotatedOffset;
                }

                GameObject vfx = Instantiate(casterVFXPrefab, spawnPosition, Quaternion.identity);
                
                // Optionally parent to caster (useful if you want VFX to follow movement)
                // vfx.transform.SetParent(caster.transform);
                
                // Rotate VFX to face attack direction
                if (rotateVFXToDirection)
                {
                    float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
                    vfx.transform.rotation = Quaternion.Euler(0, 0, angle + vfxRotationOffset);
                }
            }

            // Start melee attack coroutine
            var meleeHandler = caster.GetComponent<MeleeAbilityHandler>();
            if (meleeHandler == null)
                meleeHandler = caster.AddComponent<MeleeAbilityHandler>();

            meleeHandler.StartCoroutine(ExecuteMeleeAttack(caster, attackDirection, meleeHandler));
        }

        private Vector2 GetAttackDirection(GameObject caster, GameObject target)
        {
            if (target != null)
            {
                return (target.transform.position - caster.transform.position).normalized;
            }
            else
            {
                // Use mouse position
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0;
                return (mouseWorld - caster.transform.position).normalized;
            }
        }

        private IEnumerator ExecuteMeleeAttack(GameObject caster, Vector2 attackDirection, MonoBehaviour coroutineHost)
        {
            hitTargets.Clear();

            // Perform lunge if enabled
            if (enableLunge)
            {
                yield return coroutineHost.StartCoroutine(PerformLunge(caster, attackDirection));
            }

            // Wait for hit delay
            yield return new WaitForSeconds(hitDelay);

            // Perform hit detection
            float elapsedHitTime = 0f;
            do
            {
                PerformHitDetection(caster, attackDirection);
                
                if (activeHitDuration > 0)
                {
                    yield return new WaitForSeconds(0.1f); // Check every 0.1s during active window
                    elapsedHitTime += 0.1f;
                }
            }
            while (elapsedHitTime < activeHitDuration);
        }

        private IEnumerator PerformLunge(GameObject caster, Vector2 direction)
        {
            var rb = caster.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 targetPosition = (Vector2)caster.transform.position + direction * lungeDistance;
                Vector2 startPosition = caster.transform.position;
                float elapsed = 0f;

                while (elapsed < lungeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / lungeDuration;
                    rb.MovePosition(Vector2.Lerp(startPosition, targetPosition, t));
                    yield return null;
                }
            }
            else
            {
                // Fallback: instant position change
                caster.transform.position += (Vector3)direction * lungeDistance;
            }
        }

        private void PerformHitDetection(GameObject caster, Vector2 attackDirection)
        {
            var casterHealth = caster.GetComponent<IHealth>();
            Faction casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            // Calculate hit position with offset
            Vector2 rotatedOffset = Quaternion.Euler(0, 0, Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg) * hitOffset;
            Vector2 hitPosition = (Vector2)caster.transform.position + rotatedOffset;

            // Get hits based on shape
            Collider2D[] hits = GetHitsForShape(hitPosition, attackDirection);

            bool anyHit = false;

            foreach (var hit in hits)
            {
                if (hit == null || hit.gameObject == caster) continue;

                // Check if already hit
                if (preventMultiHit && hitTargets.Contains(hit.gameObject))
                    continue;

                var health = hit.GetComponent<IHealth>();
                if (health != null && FactionUtility.CanDamage(casterFaction, health, friendlyFire))
                {
                    anyHit = true;

                    // Calculate damage
                    int finalDamage = CalculateDamage(caster);
                    health.GetHealthSystem().Damage(finalDamage);

                    // Apply status effect
                    ApplyBuffDebuff(hit.gameObject);

                    // Apply knockback
                    if (enableKnockback)
                    {
                        ApplyKnockback(hit.gameObject, caster.transform.position);
                    }

                    // Spawn hit VFX
                    if (hitVFXPrefab != null && spawnVFXOnEachHit)
                    {
                        GameObject hitVFX = Instantiate(hitVFXPrefab, hit.transform.position, Quaternion.identity);
                        
                        // Rotate hit VFX based on direction from caster to target
                        if (rotateVFXToDirection)
                        {
                            Vector2 hitDirection = (hit.transform.position - caster.transform.position).normalized;
                            float hitAngle = Mathf.Atan2(hitDirection.y, hitDirection.x) * Mathf.Rad2Deg;
                            hitVFX.transform.rotation = Quaternion.Euler(0, 0, hitAngle + vfxRotationOffset);
                        }
                    }

                    // Mark as hit
                    if (preventMultiHit)
                        hitTargets.Add(hit.gameObject);
                }
            }

            // Play hit SFX if anything was hit
            if (anyHit && hitSFX != null)
            {
                AudioSource.PlayClipAtPoint(hitSFX, hitPosition);
            }

            // Spawn single hit VFX at hit position if not spawning per target
            if (!spawnVFXOnEachHit && hitVFXPrefab != null && anyHit)
            {
                GameObject hitVFX = Instantiate(hitVFXPrefab, hitPosition, Quaternion.identity);
                
                if (rotateVFXToDirection)
                {
                    float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
                    hitVFX.transform.rotation = Quaternion.Euler(0, 0, angle + vfxRotationOffset);
                }
            }
        }

        private Collider2D[] GetHitsForShape(Vector2 hitPosition, Vector2 attackDirection)
        {
            switch (hitShape)
            {
                case MeleeHitShape.Circle:
                    return Physics2D.OverlapCircleAll(hitPosition, hitRadius);

                case MeleeHitShape.Box:
                    float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
                    return Physics2D.OverlapBoxAll(hitPosition, hitBoxSize, angle);

                case MeleeHitShape.Arc:
                    return GetArcHits(hitPosition, attackDirection);

                case MeleeHitShape.Line:
                    return Physics2D.OverlapCapsuleAll(
                        hitPosition, 
                        new Vector2(hitRange, hitRadius), 
                        CapsuleDirection2D.Horizontal,
                        Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg
                    );

                default:
                    return new Collider2D[0];
            }
        }

        private Collider2D[] GetArcHits(Vector2 hitPosition, Vector2 attackDirection)
        {
            List<Collider2D> arcHits = new List<Collider2D>();
            Collider2D[] allHits = Physics2D.OverlapCircleAll(hitPosition, hitRadius);

            float attackAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;

            foreach (var hit in allHits)
            {
                Vector2 directionToTarget = (hit.transform.position - (Vector3)hitPosition).normalized;
                float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
                float angleDiff = Mathf.DeltaAngle(attackAngle, targetAngle);

                if (Mathf.Abs(angleDiff) <= hitArcAngle / 2f)
                {
                    arcHits.Add(hit);
                }
            }

            return arcHits.ToArray();
        }

        private int CalculateDamage(GameObject caster)
        {
            var stats = caster.GetComponent<StatsComponent>();
            int attackValue = baseDamage;

            if (stats != null && stats.CurrentStats != null)
            {
                attackValue = Mathf.RoundToInt(stats.CurrentStats.Attack * damageMultiplier);
            }
            else
            {
                attackValue = Mathf.RoundToInt(baseDamage * damageMultiplier);
            }

            return Mathf.Max(1, attackValue);
        }

        private void ApplyKnockback(GameObject target, Vector3 sourcePosition)
        {
            var rb = target.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockbackDirection = (target.transform.position - sourcePosition).normalized;
                
                // Use a coroutine handler to apply knockback over time
                var knockbackHandler = target.GetComponent<MeleeKnockbackHandler>();
                if (knockbackHandler == null)
                    knockbackHandler = target.AddComponent<MeleeKnockbackHandler>();

                knockbackHandler.ApplyKnockback(rb, knockbackDirection * knockbackForce, knockbackDuration);
            }
        }

        // Optional: Draw gizmos in editor for visualization
        public void DrawGizmos(Vector3 position, Vector2 direction)
        {
            Vector2 rotatedOffset = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) * hitOffset;
            Vector2 hitPosition = (Vector2)position + rotatedOffset;

            Gizmos.color = Color.red;
            
            switch (hitShape)
            {
                case MeleeHitShape.Circle:
                    Gizmos.DrawWireSphere(hitPosition, hitRadius);
                    break;
                case MeleeHitShape.Box:
                    Gizmos.DrawWireCube(hitPosition, hitBoxSize);
                    break;
                case MeleeHitShape.Arc:
                    Gizmos.DrawWireSphere(hitPosition, hitRadius);
                    // Draw arc lines
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Vector2 left = Quaternion.Euler(0, 0, angle - hitArcAngle / 2f) * Vector2.right * hitRadius;
                    Vector2 right = Quaternion.Euler(0, 0, angle + hitArcAngle / 2f) * Vector2.right * hitRadius;
                    Gizmos.DrawLine(hitPosition, hitPosition + left);
                    Gizmos.DrawLine(hitPosition, hitPosition + right);
                    break;
                case MeleeHitShape.Line:
                    Gizmos.DrawLine(position, (Vector2)position + direction * hitRange);
                    break;
            }
        }
    }

    public enum MeleeHitShape
    {
        Circle,    // Radial attack around caster
        Box,       // Rectangular hitbox
        Arc,       // Cone/arc in front of caster
        Line       // Thrust/lunge attack
    }
}