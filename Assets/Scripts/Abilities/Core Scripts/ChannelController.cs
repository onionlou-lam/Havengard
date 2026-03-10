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

    private float chargeTimer;
    private bool isChanneling;
    private GameObject chargingVFXInstance;
    private GameObject beamInstance;
    private MagicArsenal.MagicBeamScript beamScript;
    private Camera mainCamera;

    private Vector3 originalChargeVFXScale = Vector3.one;
    private Animator animator; // For reading facing direction
    private NavMeshAgent navAgent; // For preventing movement

    // Rotation tracking
    private Quaternion originalRotation;
    private bool wasRotatingBeforeChannel;

    void Start()
    {
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();

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

        // Prevent movement if configured
        if (ability.PreventMovement)
        {
            PreventMovement();
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

            // Set beam and all children to VFX layer (layer 31 for example, or use LayerMask.NameToLayer("VFX"))
            int vfxLayer = LayerMask.NameToLayer("VFX");
            if (vfxLayer >= 0)
            {
                SetLayerRecursively(beamInstance, vfxLayer);
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

        // Generate resource on cast (before effect) - keep consistent with AbilityBase
        (ability as AbilityBase)?.GetType()
            .GetMethod("GenerateResourceOnCast", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.Invoke(ability, new object[] { gameObject });

        // Call the ability release implementation
        ability.OnRelease(gameObject, null, percent);

        // Cleanup visuals
        CleanUpChannel();
        isChanneling = false;
    }

    private void CleanUpChannel()
    {
        if (chargingVFXInstance != null)
            Destroy(chargingVFXInstance);
        chargingVFXInstance = null;

        if (beamScript != null)
        {
            beamScript.SetCharge(0f);
            beamScript.Deactivate();
            beamScript.externalControl = false;
            beamScript = null;
        }

        if (beamInstance != null)
            Destroy(beamInstance);
        beamInstance = null;

        chargeTimer = 0f;
    }

    // If an outside system needs to forcibly cancel the channel (stun, root, etc)
    public void CancelChannel()
    {
        if (!isChanneling) return;

        // Restore movement if it was prevented
        if (ability != null && ability.PreventMovement)
        {
            RestoreMovement();
        }

        // Restore rotation
        if (rotateCasterToBeam)
        {
            RestoreCasterRotation();
        }

        CleanUpChannel();
        ability.OnChannelCancel(gameObject);
        isChanneling = false;
    }

    /// <summary>
    /// Prevents movement by stopping NavMeshAgent and broadcasting to PlayerController
    /// </summary>
    private void PreventMovement()
    {
        // Stop NavMeshAgent movement
        if (navAgent != null && navAgent.enabled)
        {
            if (navAgent.hasPath)
            {
                navAgent.ResetPath();
            }
            navAgent.isStopped = true;
        }

        // Notify PlayerController to stop click movement
        SendMessage("OnChannelStarted", SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Restores movement by re-enabling NavMeshAgent
    /// </summary>
    private void RestoreMovement()
    {
        // Re-enable NavMeshAgent movement
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.isStopped = false;
        }

        // Notify PlayerController that channeling ended
        SendMessage("OnChannelEnded", SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Rotates the caster sprite/GameObject to face the beam direction
    /// </summary>
    private void RotateCasterToDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        if (rotationTarget != null)
        {
            rotationTarget.rotation = Quaternion.Slerp(rotationTarget.rotation, targetRotation, Time.deltaTime * 15f);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        }
    }

    /// <summary>
    /// Restores the caster's original rotation
    /// </summary>
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

    /// <summary>
    /// Gets the facing direction from Animator parameters or falls back to mouse direction
    /// </summary>
    private Vector2 GetFacingDirection()
    {
        if (animator != null)
        {
            // Try to get facing direction from animator parameters
            float horizontal = animator.GetFloat("Horizontal");
            float vertical = animator.GetFloat("Vertical");

            Vector2 facingDir = new Vector2(horizontal, vertical);

            // If we have a valid direction from animator, use it
            if (facingDir.sqrMagnitude > 0.01f)
            {
                return facingDir.normalized;
            }
        }

        // Fallback: direction toward mouse
        if (mainCamera != null)
        {
            Vector3 mouseWorld = GetMouseWorldPoint();
            Vector2 dirToMouse = (mouseWorld - transform.position).normalized;

            if (dirToMouse.sqrMagnitude > 0.01f)
            {
                return dirToMouse;
            }
        }

        // Last resort: face right
        return Vector2.right;
    }

    // Helper to get mouse world point in 2D or 3D
    private Vector3 GetMouseWorldPoint()
    {
        if (use2DPhysics)
        {
            // For 2D: Simply convert screen to world
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = beamZPosition; // Match beam Z depth
            return mouseWorld;
        }
        else
        {
            // For 3D: Use raycast to find world point
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.point;
            }
            else
            {
                // No hit: point some distance along the ray
                return ray.origin + ray.direction * 100f;
            }
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