csharp Assets\Scripts\Abilities\Core Scripts\ChannelController.cs
using UnityEngine;

// Attach to a caster GameObject. This controller demonstrates a channeled usage of a ChanneledAbilityBase.
// For demo/input hooking it uses left mouse; replace with your input system as needed.
public class ChannelController : MonoBehaviour
{
    [Tooltip("Channeled ability to use")]
    public ChanneledAbilityBase ability;

    // Optional override: permit control via methods (StartChannel/StopChannel) instead of automatic mouse
    public bool useManualControl = false;

    private float chargeTimer;
    private bool isChanneling;
    private GameObject chargingVFXInstance;
    private GameObject beamInstance;
    private MagicArsenal.MagicBeamScript beamScript;
    private Camera mainCamera;

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

            // Update VFX scale
            if (chargingVFXInstance)
            {
                float scale = Mathf.Lerp(0.2f, 1f, percent);
                chargingVFXInstance.transform.localScale = Vector3.one * scale;
            }

            // Update beam visuals & direction if present
            if (beamScript)
            {
                beamScript.SetCharge(percent);

                // align beam to mouse cursor position (raycast)
                if (mainCamera != null)
                {
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        beamScript.UpdateDirectionToPoint(hit.point);
                    }
                    else
                    {
                        // No hit: point some distance along the ray
                        beamScript.UpdateDirectionToPoint(ray.origin + ray.direction * 100f);
                    }
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

        // Spawn charging VFX
        if (ability.ChargingVFXPrefab != null)
        {
            chargingVFXInstance = Instantiate(ability.ChargingVFXPrefab, transform);
            chargingVFXInstance.transform.localPosition = Vector3.zero;
        }

        // Spawn beam prefab if provided and try to find MagicBeamScript
        if (ability.BeamPrefab != null)
        {
            beamInstance = Instantiate(ability.BeamPrefab, transform.position, transform.rotation, transform);
            beamScript = beamInstance.GetComponent<MagicArsenal.MagicBeamScript>();
            if (beamScript != null)
            {
                // let MagicBeamScript be driven externally
                beamScript.externalControl = true;
                beamScript.Activate();
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

        // Pick a target point from last beam direction or a raycast under mouse
        Vector3 targetPoint = transform.position;
        if (beamScript != null)
        {
            // BeamScript tracks endpoints internally; provide an approximate point ahead
            // Use a ray from camera if possible
            Ray ray = Camera.main != null ? Camera.main.ScreenPointToRay(Input.mousePosition) : new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
                targetPoint = hit.point;
            else
                targetPoint = ray.origin + ray.direction * 50f;
        }

        // Generate resource on cast (before effect) - keep consistent with AbilityBase
        ability.GenerateResourceOnCast(gameObject);

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
}