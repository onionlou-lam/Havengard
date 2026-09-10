using UnityEngine;

namespace Havengard.Building
{
    /// <summary>
    /// Manages audio feedback specific to Building Mode gameplay events
    /// (placement, upgrades, selling, economy feedback).
    /// Distinct from Havengard.Audio.UIAudioManager, which handles generic UI chrome
    /// shared across menus (clicks, hovers, panel open/close).
    /// </summary>
    public class BuildingAudioManager : MonoBehaviour
    {
        public static BuildingAudioManager Instance { get; private set; }

        [Header("Mode Transitions")]
        [SerializeField] private AudioClip enterBuildingModeSound;
        [SerializeField] private AudioClip exitBuildingModeSound;

        [Header("Placement")]
        [SerializeField] private AudioClip towerPlacedSound;
        [SerializeField] private AudioClip placementInvalidSound;

        [Header("Tower Actions")]
        [SerializeField] private AudioClip towerUpgradedSound;
        [SerializeField] private AudioClip towerSoldSound;
        [SerializeField] private AudioClip maxLevelReachedSound;

        [Header("Economy")]
        [SerializeField] private AudioClip insufficientGoldSound;

        [Header("History")]
        [SerializeField] private AudioClip undoSound;
        [SerializeField] private AudioClip resetSound;

        [Header("Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField][Range(0f, 1f)] private float volume = 0.8f;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create audio source if not assigned
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = volume;
            }
        }

        /// <summary>
        /// Play sound when entering building mode
        /// </summary>
        public void PlayEnterBuildingMode()
        {
            PlaySound(enterBuildingModeSound);
        }

        /// <summary>
        /// Play sound when exiting building mode
        /// </summary>
        public void PlayExitBuildingMode()
        {
            PlaySound(exitBuildingModeSound);
        }

        /// <summary>
        /// Play sound when a tower is successfully placed
        /// </summary>
        public void PlayTowerPlaced()
        {
            PlaySound(towerPlacedSound);
        }

        /// <summary>
        /// Play sound when a placement attempt is invalid (occupied, out of bounds, etc.)
        /// </summary>
        public void PlayPlacementInvalid()
        {
            PlaySound(placementInvalidSound);
        }

        /// <summary>
        /// Play sound when a tower is upgraded
        /// </summary>
        public void PlayTowerUpgraded()
        {
            PlaySound(towerUpgradedSound);
        }

        /// <summary>
        /// Play sound when a tower is sold
        /// </summary>
        public void PlayTowerSold()
        {
            PlaySound(towerSoldSound);
        }

        /// <summary>
        /// Play sound when a tower is already at max level
        /// </summary>
        public void PlayMaxLevelReached()
        {
            PlaySound(maxLevelReachedSound);
        }

        /// <summary>
        /// Play sound when an action fails due to insufficient gold
        /// </summary>
        public void PlayInsufficientGold()
        {
            PlaySound(insufficientGoldSound);
        }

        /// <summary>
        /// Play sound when undoing an action
        /// </summary>
        public void PlayUndo()
        {
            PlaySound(undoSound);
        }

        /// <summary>
        /// Play sound when resetting the current phase
        /// </summary>
        public void PlayReset()
        {
            PlaySound(resetSound);
        }

        /// <summary>
        /// Play a specific sound
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }

        /// <summary>
        /// Set building audio volume
        /// </summary>
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
                audioSource.volume = volume;
        }
    }
}