using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Havengard.Abilities
{
    [DisallowMultipleComponent]
    public class AbilityUser : MonoBehaviour
    {
        [Header("Ability Slots")]
        [SerializeField] private List<AbilityBase> abilities = new List<AbilityBase>();

        [Header("Resource")]
        [Tooltip("Optional. If null, will try GetComponent<ResourceSystem>() on this GameObject.")]
        [SerializeField] private ResourceSystem resourceSystem;

        [Header("Cooldowns")]
        [SerializeField] private bool useGlobalCooldown = false;
        [SerializeField] private float globalCooldownDuration = 0.1f;

        private float[] nextReadyTimes;
        private float globalCooldownEndTime;

        public event Action<int, AbilityBase> OnAbilityUsed;
        public event Action<int, float> OnAbilityCooldownStarted;
        public event Action OnAbilitiesChanged; // ADD THIS NEW EVENT

        [Header("Channeling Visual Settings")]
        [SerializeField] private float beamSpawnDistance = 0.5f;
        [SerializeField] private float beamZPosition = 0f;
        [SerializeField] private bool rotateToBeam = true;
        [SerializeField] private Transform rotationTarget;

        [Header("Channeling Fizzle Effect")]
        [Tooltip("Enable beam shrink/fizzle effect when stopping channel early")]
        [SerializeField] private bool enableFizzleEffect = true;
        [Tooltip("How fast the beam shrinks when released (higher = faster)")]
        [SerializeField] private float fizzleSpeed = 5f;
        [Tooltip("Duration of fizzle effect in seconds")]
        [SerializeField] private float fizzleDuration = 0.3f;

        [Header("Channeling Audio")]
        [SerializeField] private bool useChannelingAudio = true;
        [Tooltip("Volume for cast sound (charge-up)")]
        [Range(0f, 3f)]
        [SerializeField] private float castSoundVolume = 1.5f;
        [Tooltip("Volume for impact sound (release)")]
        [Range(0f, 3f)]
        [SerializeField] private float impactSoundVolume = 2.5f;

        // Private channeling state
        private ChanneledAbilityBase activeChanneledAbility;
        private int activeChanneledAbilityIndex = -1;
        private GameObject chargingVFXInstance;
        private GameObject beamInstance;
        private MagicArsenal.MagicBeamScript beamScript;
        private float channelStartTime;
        private float lastChannelTick;
        private Vector3 originalChargeVFXScale = Vector3.one;
        private Quaternion originalRotation;
        private bool wasRotatingBeforeChannel;
        private bool isFizzling = false;

        private Camera mainCamera;

        [Header("Skill Tree Unlocks")]
        [Tooltip("Tracks which abilities from PlayerClass are unlocked")]
        [SerializeField] public bool[] unlockedAbilities;

        private void Awake()
        {
            if (resourceSystem == null)
                resourceSystem = GetComponent<ResourceSystem>();

            if (abilities == null)
                abilities = new List<AbilityBase>();

            mainCamera = Camera.main;

            // Store original rotation
            if (rotateToBeam)
            {
                if (rotationTarget != null)
                    originalRotation = rotationTarget.rotation;
                else
                    originalRotation = transform.rotation;
            }

            RebuildCooldownArray();
        }

        private void OnValidate()
        {
            if (abilities == null)
                abilities = new List<AbilityBase>();

            if (!Application.isPlaying)
                RebuildCooldownArray();
        }

        private void Update()
        {
            // Update active channeled ability
            if (activeChanneledAbility != null && !isFizzling)
            {
                UpdateChanneling();
            }
        }

        public void RebuildCooldownArray()
        {
            nextReadyTimes = (abilities != null && abilities.Count > 0)
                ? new float[abilities.Count]
                : new float[0];
        }

        public void AssignAbilities(List<AbilityBase> list)
        {
            abilities = list ?? new List<AbilityBase>();
            RebuildCooldownArray();
        }

        public void AssignAbilities(AbilityBase[] array)
        {
            abilities = array != null ? new List<AbilityBase>(array) : new List<AbilityBase>();
            RebuildCooldownArray();
        }

        public AbilityBase GetAbility(int index)
        {
            if (abilities == null) return null;
            if (index < 0 || index >= abilities.Count) return null;
            return abilities[index];
        }

        public void AddAbility(AbilityBase ability)
        {
            if (ability == null) return;

            if (abilities == null)
                abilities = new List<AbilityBase>();

            abilities.Add(ability);
            RebuildCooldownArray();
        }

        public float GetRemainingCooldown(int index)
        {
            if (nextReadyTimes == null || index < 0 || index >= nextReadyTimes.Length)
                return 0f;

            float remaining = nextReadyTimes[index] - Time.time;
            return Mathf.Max(0f, remaining);
        }

        public bool IsOnCooldown(int index)
        {
            return GetRemainingCooldown(index) > 0f;
        }

        public bool CanUseAbility(int index)
        {
            var ability = GetAbility(index);
            if (ability == null) return false;

            float now = Time.time;

            if (useGlobalCooldown && now < globalCooldownEndTime)
                return false;

            if (nextReadyTimes != null &&
                index >= 0 &&
                index < nextReadyTimes.Length &&
                now < nextReadyTimes[index])
            {
                return false;
            }

            // Resource check
            if (resourceSystem != null && ability.resourceCost > 0)
            {
                if (!resourceSystem.HasResource(ability.resourceCost))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Main ability activation method - handles both normal and channeled abilities
        /// </summary>
        public bool UseAbility(int index, Vector3 targetPosition, GameObject targetEnemy = null)
        {
            if (index < 0 || index >= abilities.Count) return false;

            AbilityBase ability = abilities[index];
            if (ability == null) return false;

            // Check if it's a channeled ability
            var channeledAbility = ability as ChanneledAbilityBase;
            if (channeledAbility != null)
            {
                if (activeChanneledAbility == null)
                {
                    return StartChanneling(index, channeledAbility, targetPosition, targetEnemy);
                }
                return false; // Already channeling
            }

            // Normal ability activation
            if (!CanUseAbility(index)) return false;

            ConsumeResource(ability);
            ability.Activate(this, targetPosition, targetEnemy);
            StartCooldown(index);

            OnAbilityUsed?.Invoke(index, ability);
            return true;
        }

        /// <summary>
        /// Overload for backwards compatibility
        /// </summary>
        public bool UseAbility(int index, GameObject target)
        {
            Vector3 targetPos = target != null ? target.transform.position : transform.position;
            return UseAbility(index, targetPos, target);
        }

        /// <summary>
        /// Stop active channeled ability with fizzle effect
        /// </summary>
        public void StopChanneling()
        {
            if (activeChanneledAbility == null) return;

            float channelTime = Time.time - channelStartTime;
            float percent = Mathf.Clamp01(channelTime / activeChanneledAbility.MaxChargeTime);

            // Check if channel can be released
            if (percent < activeChanneledAbility.MinReleasePercent)
            {
                // Too early - cancel with fizzle
                CancelChanneling();
                return;
            }

            // Play impact sound
            if (useChannelingAudio && activeChanneledAbility.impactSFX != null)
            {
                Vector3 impactPos = GetMouseWorldPoint();
                AudioSource.PlayClipAtPoint(activeChanneledAbility.impactSFX, impactPos, impactSoundVolume);
                //Debug.Log($"[AbilityUser] Played impact SFX at volume {impactSoundVolume}");
            }

            // Call release on ability
            activeChanneledAbility.StopChannel(this, GetMouseWorldPoint(), null);

            // Start cooldown for channeled ability
            if (activeChanneledAbilityIndex >= 0)
            {
                StartCooldown(activeChanneledAbilityIndex);
                OnAbilityUsed?.Invoke(activeChanneledAbilityIndex, activeChanneledAbility);
            }

            // Fizzle effect or immediate cleanup
            if (enableFizzleEffect && beamInstance != null)
            {
                StartCoroutine(FizzleBeam());
            }
            else
            {
                CleanupChannelingVFX();
                RestoreCasterRotation();
            }

            activeChanneledAbility = null;
            activeChanneledAbilityIndex = -1;
        }

        /// <summary>
        /// Cancel channeling without release effects (with fizzle)
        /// </summary>
        public void CancelChanneling()
        {
            if (activeChanneledAbility == null) return;

            activeChanneledAbility.OnChannelCancel(gameObject);

            // Fizzle effect when canceling
            if (enableFizzleEffect && beamInstance != null)
            {
                StartCoroutine(FizzleBeam());
            }
            else
            {
                CleanupChannelingVFX();
                RestoreCasterRotation();
            }

            activeChanneledAbility = null;
            activeChanneledAbilityIndex = -1;

            Debug.Log("[AbilityUser] Channeling canceled");
        }

        /// <summary>
        /// Shrink/fizzle beam effect when stopping
        /// </summary>
        private IEnumerator FizzleBeam()
        {
            isFizzling = true;
            float fizzleTimer = 0f;
            Vector3 originalBeamScale = beamInstance != null ? beamInstance.transform.localScale : Vector3.one;
            Vector3 originalChargeScale = chargingVFXInstance != null ? chargingVFXInstance.transform.localScale : Vector3.one;

            while (fizzleTimer < fizzleDuration)
            {
                fizzleTimer += Time.deltaTime;
                float t = fizzleTimer / fizzleDuration;
                float scale = Mathf.Lerp(1f, 0f, t * fizzleSpeed);

                // Shrink beam
                if (beamInstance != null)
                {
                    beamInstance.transform.localScale = originalBeamScale * scale;
                }

                // Shrink charging VFX
                if (chargingVFXInstance != null)
                {
                    chargingVFXInstance.transform.localScale = originalChargeScale * scale;
                }

                // Reduce beam charge visually
                if (beamScript != null)
                {
                    beamScript.SetCharge(scale);
                }

                yield return null;
            }

            // Cleanup after fizzle
            CleanupChannelingVFX();
            RestoreCasterRotation();
            isFizzling = false;
        }

        /// <summary>
        /// Check if currently channeling an ability
        /// </summary>
        public bool IsChanneling => activeChanneledAbility != null;

        /// <summary>
        /// Get current channel progress (0-1)
        /// </summary>
        public float GetChannelProgress()
        {
            if (activeChanneledAbility == null) return 0f;
            float elapsed = Time.time - channelStartTime;
            return Mathf.Clamp01(elapsed / activeChanneledAbility.MaxChargeTime);
        }

        private bool StartChanneling(int abilityIndex, ChanneledAbilityBase ability, Vector3 targetPos, GameObject targetEnemy)
        {
            if (!CanUseAbility(abilityIndex)) return false;
            if (!ability.CanCast(gameObject, targetEnemy)) return false;

            // Consume resource
            ConsumeResource(ability);

            // Activate ability
            ability.Activate(this, targetPos, targetEnemy);

            activeChanneledAbility = ability;
            activeChanneledAbilityIndex = abilityIndex;
            channelStartTime = Time.time;
            lastChannelTick = Time.time;
            wasRotatingBeforeChannel = true;
            isFizzling = false;

            // Play cast sound
            if (useChannelingAudio && ability.castSFX != null)
            {
                AudioSource.PlayClipAtPoint(ability.castSFX, transform.position, castSoundVolume);
                Debug.Log($"[AbilityUser] Played cast SFX at volume {castSoundVolume}");
            }

            // Spawn VFX
            SpawnChannelingVFX(ability, targetPos);

            Debug.Log($"[AbilityUser] Started channeling {ability.abilityName}");
            return true;
        }

        private void UpdateChanneling()
        {
            if (activeChanneledAbility == null) return;

            float elapsed = Time.time - channelStartTime;
            float percent = Mathf.Clamp01(elapsed / activeChanneledAbility.MaxChargeTime);

            // Auto-stop if max duration reached
            if (elapsed >= activeChanneledAbility.MaxChargeTime)
            {
                StopChanneling();
                return;
            }

            // Get facing direction (towards mouse)
            Vector2 facingDir = GetFacingDirection();

            // Rotate caster to face beam direction
            if (rotateToBeam)
            {
                RotateCasterToDirection(facingDir);
            }

            // Update visuals
            UpdateChannelingVFX(percent, facingDir);

            // Tick damage/effects
            if (Time.time >= lastChannelTick + activeChanneledAbility.TickRate)
            {
                activeChanneledAbility.OnChannelTick(gameObject, percent);
                lastChannelTick = Time.time;
            }
        }

        private void SpawnChannelingVFX(ChanneledAbilityBase ability, Vector3 targetPos)
        {
            Vector2 facingDir = GetFacingDirection();
            Vector3 spawnPos = transform.position + (Vector3)facingDir * beamSpawnDistance;
            spawnPos.z = beamZPosition;

            float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;
            Quaternion spawnRotation = Quaternion.Euler(0, 0, angle);

            // Spawn charging VFX
            if (ability.ChargingVFXPrefab != null)
            {
                chargingVFXInstance = Instantiate(ability.ChargingVFXPrefab, spawnPos, spawnRotation, transform);
                originalChargeVFXScale = chargingVFXInstance.transform.localScale;
            }

            // Spawn beam prefab
            if (ability.BeamPrefab != null)
            {
                beamInstance = Instantiate(ability.BeamPrefab, spawnPos, spawnRotation, transform);

                // Set to VFX layer
                int vfxLayer = LayerMask.NameToLayer("VFX");
                if (vfxLayer >= 0)
                {
                    SetLayerRecursively(beamInstance, vfxLayer);
                }

                // Disable beam audio if using ability audio
                if (useChannelingAudio)
                {
                    var beamAudioSources = beamInstance.GetComponentsInChildren<AudioSource>();
                    foreach (var beamAudio in beamAudioSources)
                    {
                        beamAudio.enabled = false;
                    }
                }

                beamScript = beamInstance.GetComponent<MagicArsenal.MagicBeamScript>();

                if (beamScript != null)
                {
                    // Sync beam range
                    var beamAbility = ability as ChanneledBeamAbility;
                    if (beamAbility != null)
                    {
                        beamScript.maxBeamDistance = beamAbility.BeamMaxRange;
                    }

                    beamScript.externalControl = true;
                    beamScript.Activate();
                }
            }
        }

        private void UpdateChannelingVFX(float percent, Vector2 facingDir)
        {
            Vector3 spawnPos = transform.position + (Vector3)facingDir * beamSpawnDistance;
            spawnPos.z = beamZPosition;

            float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;

            // Update charging VFX
            if (chargingVFXInstance != null)
            {
                float scale = Mathf.Lerp(0.2f, 1f, percent);
                chargingVFXInstance.transform.localScale = originalChargeVFXScale * scale;
                chargingVFXInstance.transform.position = spawnPos;
                chargingVFXInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            // Update beam
            if (beamScript != null && beamInstance != null)
            {
                beamScript.SetCharge(percent);
                beamInstance.transform.position = spawnPos;
                beamInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

                if (mainCamera != null)
                {
                    Vector3 targetPoint = GetMouseWorldPoint();
                    beamScript.UpdateDirectionToPoint(targetPoint);
                }
            }
        }

        private void CleanupChannelingVFX()
        {
            if (chargingVFXInstance != null)
            {
                Destroy(chargingVFXInstance);
                chargingVFXInstance = null;
            }

            if (beamInstance != null)
            {
                Destroy(beamInstance);
                beamInstance = null;
                beamScript = null;
            }
        }

        private Vector2 GetFacingDirection()
        {
            if (mainCamera != null)
            {
                Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 dir = (mousePos - transform.position).normalized;
                return dir;
            }

            return Vector2.right;
        }

        private Vector3 GetMouseWorldPoint()
        {
            if (mainCamera == null) return transform.position + Vector3.right * 10f;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            worldPos.z = beamZPosition;
            return worldPos;
        }

        private void RotateCasterToDirection(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            if (rotationTarget != null)
            {
                rotationTarget.rotation = targetRotation;
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }

        private void RestoreCasterRotation()
        {
            if (!rotateToBeam) return;

            if (rotationTarget != null)
            {
                rotationTarget.rotation = originalRotation;
            }
            else
            {
                transform.rotation = originalRotation;
            }
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private void ConsumeResource(AbilityBase ability)
        {
            if (resourceSystem != null && ability.resourceCost > 0)
            {
                resourceSystem.SpendResource(ability.resourceCost);
            }
        }

        private void StartCooldown(int index)
        {
            var ability = GetAbility(index);
            if (ability == null) return;

            float now = Time.time;

            if (nextReadyTimes != null && index >= 0 && index < nextReadyTimes.Length)
            {
                nextReadyTimes[index] = now + ability.baseCooldown;
                OnAbilityCooldownStarted?.Invoke(index, ability.baseCooldown);
            }

            if (useGlobalCooldown)
            {
                globalCooldownEndTime = now + globalCooldownDuration;
            }
        }

        public List<AbilityBase> GetAllAbilities()
        {
            return new List<AbilityBase>(abilities);
        }

        public int GetAbilityCount()
        {
            return abilities != null ? abilities.Count : 0;
        }

        /// <summary>
        /// Get array of unlocked abilities (indices match PlayerClass.classAbilities)
        /// </summary>
        public bool[] GetUnlockedAbilities()
        {
            return unlockedAbilities;
        }

        /// <summary>
        /// Initialize unlock tracking based on PlayerClass
        /// </summary>
        public void InitializeUnlockTracking(int abilityCount)
        {
            if (unlockedAbilities == null || unlockedAbilities.Length != abilityCount)
            {
                unlockedAbilities = new bool[abilityCount];
                Debug.Log($"[AbilityUser] Initialized unlock tracking for {abilityCount} abilities");
            }
        }

        /// <summary>
        /// Check if an ability is unlocked
        /// </summary>
        public bool IsAbilityUnlocked(int abilityIndex)
        {
            if (unlockedAbilities == null || abilityIndex < 0 || abilityIndex >= unlockedAbilities.Length)
                return false;

            return unlockedAbilities[abilityIndex];
        }

        /// <summary>
        /// Unlock an ability in the skill tree
        /// </summary>
        public void UnlockAbility(int abilityIndex, AbilityBase ability)
        {
            if (unlockedAbilities == null || abilityIndex < 0 || abilityIndex >= unlockedAbilities.Length)
            {
                Debug.LogWarning($"[AbilityUser] Cannot unlock ability at invalid index {abilityIndex}");
                return;
            }

            if (unlockedAbilities[abilityIndex])
            {
                Debug.LogWarning($"[AbilityUser] Ability at index {abilityIndex} already unlocked");
                return;
            }

            unlockedAbilities[abilityIndex] = true;

            if (ability != null && !abilities.Contains(ability))
            {
                abilities.Add(ability);
                RebuildCooldownArray();
                Debug.Log($"[AbilityUser] Unlocked ability: {ability.abilityName}");
                
                // FIRE EVENT
                OnAbilitiesChanged?.Invoke(); // ADD THIS LINE
            }
        }

        /// <summary>
        /// Set unlocked abilities directly (for loading saves)
        /// </summary>
        public void SetUnlockedAbilities(bool[] unlocked)
        {
            unlockedAbilities = unlocked;
            Debug.Log($"[AbilityUser] Set unlocked abilities: {unlocked?.Length ?? 0} total");
        }
        
        /// <summary>
        /// Get the list of currently assigned abilities
        /// </summary>
        public List<AbilityBase> GetAbilities()
        {
            return abilities;
        }
    }
}