using UnityEngine;

namespace Havengard.Combat
{
    /// <summary>
    /// Handles visual and audio feedback when a unit performs an attack.
    /// Can be triggered manually from any combat or ability script.
    /// </summary>
    public class AttackEffectHandler : MonoBehaviour
    {
        [Header("Attack Effects")]
        [Tooltip("Optional particle effect prefab to spawn when this unit attacks.")]
        [SerializeField] private GameObject attackVFXPrefab;

        [Tooltip("Optional particle effect to spawn on successful hit.")]
        [SerializeField] private GameObject impactVFXPrefab;

        [Tooltip("Sound effect played when the attack is executed.")]
        [SerializeField] private AudioClip attackSFX;

        [Tooltip("Sound effect played when the attack connects with a target.")]
        [SerializeField] private AudioClip hitSFX;

        [Header("Effect Settings")]
        [SerializeField] private Transform effectSpawnPoint;
        [SerializeField] private float vfxLifetime = 1.5f;
        [SerializeField] private float sfxVolume = 0.8f;

        /// <summary>
        /// Plays the attack (launch) visuals and sound.
        /// </summary>
        public void PlayAttackEffect()
        {
            if (attackVFXPrefab != null)
            {
                var fx = Instantiate(attackVFXPrefab,
                    effectSpawnPoint ? effectSpawnPoint.position : transform.position,
                    Quaternion.identity,
                    transform);
                Destroy(fx, vfxLifetime);
            }

            if (attackSFX != null)
                AudioSource.PlayClipAtPoint(attackSFX, transform.position, sfxVolume);
        }

        /// <summary>
        /// Plays the on-hit visuals and sound.
        /// </summary>
        public void PlayImpactEffect(Vector3 hitPosition)
        {
            if (impactVFXPrefab != null)
            {
                var fx = Instantiate(impactVFXPrefab, hitPosition, Quaternion.identity);
                Destroy(fx, vfxLifetime);
            }

            if (hitSFX != null)
                AudioSource.PlayClipAtPoint(hitSFX, hitPosition, sfxVolume);
        }
    }
}
