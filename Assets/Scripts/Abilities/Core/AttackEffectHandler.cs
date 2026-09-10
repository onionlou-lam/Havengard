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
                PlayManagedSFX(attackSFX, transform.position, sfxVolume);
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
                PlayManagedSFX(hitSFX, hitPosition, sfxVolume);
        }

        /// <summary>
        /// Routes SFX through GameAudioManager when available so concurrent-instance limiting
        /// and pause-awareness apply (important since many enemies may share this component's clips).
        /// Falls back to raw playback if no manager exists in the scene.
        /// </summary>
        private void PlayManagedSFX(AudioClip clip, Vector3 position, float volume)
        {
            if (Havengard.Audio.GameAudioManager.Instance != null)
            {
                Havengard.Audio.GameAudioManager.Instance.PlaySFX(clip, position, volume);
                return;
            }

            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}
