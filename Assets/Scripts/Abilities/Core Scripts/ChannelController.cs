using UnityEngine;
using Havengard.Abilities;

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
    [Tooltip("Offset from caster position where beam/charge VFX spawn (local space)")]
    public Vector3 spawnOffset = new Vector3(0.5f, 0f, 0f); // ADD THIS

    private float chargeTimer;
    private bool isChanneling;
    private GameObject chargingVFXInstance;
    private GameObject beamInstance;
    private MagicArsenal.MagicBeamScript beamScript;
    private Camera mainCamera;
    
    private Vector3 originalChargeVFXScale = Vector3.one; // ADD THIS

    void Start()
    {
        mainCamera = Camera.main;
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

            // Update VFX scale - NOW RESPECTS ORIGINAL SCALE
            if (chargingVFXInstance)
            {
                float scale = Mathf.Lerp(0.2f, 1f, percent);
                chargingVFXInstance.transform.localScale = originalChargeVFXScale * scale;
            }

            // Update beam visuals & direction if present
            if (beamScript && beamInstance)
            {
                beamScript.SetCharge(percent);

                // Ensure beam stays at correct Z depth
                Vector3 beamPos = beamInstance.transform.position;
                beamPos.z = beamZPosition;
                beamInstance.transform.position = beamPos;

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

        // Calculate spawn position with offset
        Vector3 spawnPos = transform.position + transform.TransformDirection(spawnOffset);
        spawnPos.z = beamZPosition;

        // Spawn charging VFX with offset
        if (ability.ChargingVFXPrefab != null)
        {
            chargingVFXInstance = Instantiate(ability.ChargingVFXPrefab, transform);
            chargingVFXInstance.transform.position = spawnPos; // Apply offset
            
            // Store original scale
            originalChargeVFXScale = chargingVFXInstance.transform.localScale;
        }

        // Spawn beam prefab if provided and try to find MagicBeamScript
        if (ability.BeamPrefab != null)
        {
            beamInstance = Instantiate(ability.BeamPrefab, spawnPos, transform.rotation, transform);

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
        CleanUpChannel();
        ability.OnChannelCancel(gameObject);
        isChanneling = false;
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
}