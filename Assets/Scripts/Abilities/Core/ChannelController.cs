using UnityEngine;
using Havengard.Abilities;
using UnityEngine.AI;

public class ChannelController : MonoBehaviour
{
    [Tooltip("Channeled ability to use")]
    public ChanneledAbilityBase ability;

    public bool useManualControl = false;

    [Tooltip("Use 2D physics for raycasting (set true for 2D games)")]
    public bool use2DPhysics = true;

    [Tooltip("Z position for beam in 2D (should match your sprite layer, typically 0)")]
    public float beamZPosition = 0f;

    [Header("Spawn Offset")]
    [Tooltip("Distance in front of caster to spawn beam/charge VFX")]
    public float spawnDistance = 0.5f;

    [Header("Caster Rotation")]
    [Tooltip("Enable to rotate the caster sprite/gameobject to face the beam direction")]
    public bool rotateCasterToBeam = true;
    [Tooltip("If specified, only this transform will rotate. Leave empty to rotate the root GameObject")]
    public Transform rotationTarget;

    [Header("Audio")]
    [Tooltip("Play cast/impact SFX from ability (disable if beam prefab has its own audio)")]
    public bool useAbilityAudio = true;
    [Tooltip("Volume for cast sound (charge-up sound when starting channel)")]
    [Range(0f, 3f)]
    public float castSoundVolume = 1.5f;
    [Tooltip("Volume for impact sound (release/explosion sound when beam fires)")]
    [Range(0f, 3f)]
    public float impactSoundVolume = 2.0f;

    private float chargeTimer;
    private bool isChanneling;
    private GameObject chargingVFXInstance;
    private GameObject beamInstance;
    private MagicArsenal.MagicBeamScript beamScript;
    private Camera mainCamera;

    private Vector3 originalChargeVFXScale = Vector3.one;
    private Animator animator;
    private NavMeshAgent navAgent;

    // Rotation tracking
    private Quaternion originalRotation;
    private bool wasRotatingBeforeChannel;

    // Audio tracking
    private bool hasPlayedCastSound = false;
    private AudioSource audioSource;

    void Start()
    {
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();

        // Get or create AudioSource for ability sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && useAbilityAudio)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f; // 2D/3D blend
        }

        // Store original rotation
        if (rotateCasterToBeam)
        {
            if (rotationTarget != null)
                originalRotation = rotationTarget.rotation;
            else
                originalRotation = transform.rotation;
        }
    }

    void Update()
    {
        if (ability == null) return;

        if (!useManualControl)
        {
            if (Input.GetMouseButtonDown(0))
                StartChannel();
            if (Input.GetMouseButtonUp(0))
                StopChannel();
        }

        if (isChanneling)
        {
            chargeTimer += Time.deltaTime;
            float percent = Mathf.Clamp01(chargeTimer / ability.MaxChargeTime);

            // Get current facing direction
            Vector2 facingDir = GetFacingDirection();

            // Rotate caster to face beam direction
            if (rotateCasterToBeam)
            {
                RotateCasterToDirection(facingDir);
            }

            // Update beam and charge VFX positions to follow caster
            Vector3 spawnPos = transform.position + (Vector3)facingDir * spawnDistance;
            spawnPos.z = beamZPosition;

            // Update charging VFX position and scale
            if (chargingVFXInstance)
            {
                float scale = Mathf.Lerp(0.2f, 1f, percent);
                chargingVFXInstance.transform.localScale = originalChargeVFXScale * scale;
                chargingVFXInstance.transform.position = spawnPos;

                // Rotate VFX to face direction
                float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;
                chargingVFXInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            // Update beam visuals & direction if present
            if (beamScript && beamInstance)
            {
                beamScript.SetCharge(percent);

                // Update beam position
                beamInstance.transform.position = spawnPos;

                // Update beam rotation based on facing direction
                float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;
                beamInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

                // align beam to mouse cursor position
                if (mainCamera != null)
                {
                    Vector3 targetPoint = GetMouseWorldPoint();
                    beamScript.UpdateDirectionToPoint(targetPoint);
                }
            }

            // Allow ability logic per-frame
            ability.OnChannelTick(gameObject, percent);
        }
    }

    // Can be invoked from input system
    public void StartChannel()
    {
        if (isChanneling || ability == null) return;

        // Basic cast gating
        if (!ability.CanCast(gameObject, null)) return;

        isChanneling = true;
        chargeTimer = 0f;
        wasRotatingBeforeChannel = true;
        hasPlayedCastSound = false;

        // Prevent movement if configured
        if (ability.PreventMovement)
        {
            PreventMovement();
        }

        // Play cast sound
        if (useAbilityAudio && !hasPlayedCastSound)
        {
            var abilityBase = ability as AbilityBase;
            if (abilityBase != null && abilityBase.castSFX != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(abilityBase.castSFX, castSoundVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(abilityBase.castSFX, transform.position, castSoundVolume);
                }
                hasPlayedCastSound = true;
                Debug.Log($"[ChannelController] Played cast SFX: {abilityBase.castSFX.name} at volume {castSoundVolume}");
            }
        }

        // Get facing direction and calculate spawn position
        Vector2 facingDir = GetFacingDirection();
        Vector3 spawnPos = transform.position + (Vector3)facingDir * spawnDistance;
        spawnPos.z = beamZPosition;

        // Calculate rotation based on facing direction
        float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;
        Quaternion spawnRotation = Quaternion.Euler(0, 0, angle);

        // Spawn charging VFX with offset and rotation
        if (ability.ChargingVFXPrefab != null)
        {
            chargingVFXInstance = Instantiate(ability.ChargingVFXPrefab, spawnPos, spawnRotation, transform);

            // Store original scale
            originalChargeVFXScale = chargingVFXInstance.transform.localScale;
        }

        // Spawn beam prefab if provided and try to find MagicBeamScript
        if (ability.BeamPrefab != null)
        {
            beamInstance = Instantiate(ability.BeamPrefab, spawnPos, spawnRotation, transform);

            // Set beam and all children to VFX layer
            int vfxLayer = LayerMask.NameToLayer("VFX");
            if (vfxLayer >= 0)
            {
                SetLayerRecursively(beamInstance, vfxLayer);
            }

            // Disable any AudioSource on the beam prefab if using ability audio
            if (useAbilityAudio)
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
                // Sync beam range with ability range if it's a ChanneledBeamAbility
                var beamAbility = ability as Havengard.Abilities.ChanneledBeamAbility;
                if (beamAbility != null)
                {
                    beamScript.maxBeamDistance = beamAbility.BeamMaxRange;
                    Debug.Log($"Synced beam visual range to ability range: {beamAbility.BeamMaxRange}");
                }

                // let MagicBeamScript be driven externally
                beamScript.externalControl = true;
                beamScript.Activate();

                Debug.Log($"Beam activated at Z={beamZPosition}, Position={beamInstance.transform.position}");
            }
            else
            {
                Debug.LogError("MagicBeamScript component not found on beam prefab!");
            }
        }
    }

    // Can be invoked from input system
    public void StopChannel()
    {
        if (!isChanneling || ability == null) return;

        float percent = Mathf.Clamp01(chargeTimer / ability.MaxChargeTime);

        // Play impact sound on release (if beam hit something)
        if (useAbilityAudio)
        {
            var abilityBase = ability as AbilityBase;
            if (abilityBase != null && abilityBase.impactSFX != null)
            {
                // Play at mouse position or forward from caster
                Vector3 impactPos = GetMouseWorldPoint();
                AudioSource.PlayClipAtPoint(abilityBase.impactSFX, impactPos, impactSoundVolume);
                Debug.Log($"[ChannelController] Played impact SFX: {abilityBase.impactSFX.name} at volume {impactSoundVolume}");
            }
        }

        // Restore movement if it was prevented
        if (ability.PreventMovement)
        {
            RestoreMovement();
        }

        // Restore rotation if we were rotating the caster
        if (rotateCasterToBeam)
        {
            RestoreCasterRotation();
        }

        // Determine whether release is allowed
        if (!ability.AllowPartialRelease && percent < 1f)
        {
            // Cancel
            CleanUpChannel();
            ability.OnChannelCancel(gameObject);
            isChanneling = false;
            return;
        }

        if (percent < ability.MinReleasePercent)
        {
            // Too small to release -> cancel
            CleanUpChannel();
            ability.OnChannelCancel(gameObject);
            isChanneling = false;
            return;
        }

        // Generate resource on cast (before effect)
        (ability as AbilityBase)?.GetType()
            .GetMethod("GenerateResourceOnCast", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.Invoke(ability, new object[] { gameObject });

        // Call the ability release implementation
        var method = ability.GetType().GetMethod("OnRelease", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        method?.Invoke(ability, new object[] { gameObject, null, percent });

        // Cleanup visuals
        CleanUpChannel();
        isChanneling = false;
    }

    private void CleanUpChannel()
    {
        if (chargingVFXInstance)
        {
            Destroy(chargingVFXInstance);
            chargingVFXInstance = null;
        }

        if (beamInstance)
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
        if (rotationTarget != null)
        {
            rotationTarget.rotation = originalRotation;
        }
        else
        {
            transform.rotation = originalRotation;
        }
    }

    private void PreventMovement()
    {
        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }
    }

    private void RestoreMovement()
    {
        if (navAgent != null)
        {
            navAgent.isStopped = false;
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

    /// <summary>
    /// Public property to check if currently channeling
    /// </summary>
    public bool IsChanneling => isChanneling;
}