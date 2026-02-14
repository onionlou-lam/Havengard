using Havengard.Abilities;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Channeled Ability", fileName = "NewChanneledAbility")]
public abstract class ChanneledAbilityBase : AbilityBase
{
    [Header("Channel Settings")]
    [Tooltip("Time in seconds to reach full charge")]
    [SerializeField] private float maxChargeTime = 2f;
    [Tooltip("Minimum charge percent (0..1) required to allow release. If release is lower and allowPartialRelease is false, the channel will cancel.")]
    [Range(0f, 1f)]
    [SerializeField] private float minReleasePercent = 0.05f;
    [Tooltip("Allow releasing early and deal partial effect based on charge percent")]
    [SerializeField] private bool allowPartialRelease = true;

    [Header("VFX/Beam (optional)")]
    [Tooltip("Charging VFX prefab instantiated on the caster while charging. Should be scalable by localScale.")]
    [SerializeField] private GameObject chargingVFXPrefab = null;
    [Tooltip("Optional beam prefab (contains MagicBeamScript). If set, ChannelController will instantiate it.")]
    [SerializeField] private GameObject beamPrefab = null;

    public float MaxChargeTime => Mathf.Max(0.0001f, maxChargeTime);
    public float MinReleasePercent => Mathf.Clamp01(minReleasePercent);
    public bool AllowPartialRelease => allowPartialRelease;
    public GameObject ChargingVFXPrefab => chargingVFXPrefab;
    public GameObject BeamPrefab => beamPrefab;

    // Called by ChannelController each frame while channeling (chargePercent in 0..1)
    public virtual void OnChannelTick(GameObject caster, float chargePercent) { }

    // Called when the channel is released (chargePercent in 0..1). Implement the final effect here.
    public abstract void OnRelease(GameObject caster, GameObject target, float chargePercent);

    // Called when channeling is cancelled (didn't meet release conditions or interrupted)
    public virtual void OnChannelCancel(GameObject caster) { }

    // For compatibility: fall back Cast to immediate full release
    public override void Cast(GameObject caster, GameObject target)
    {
        // Generate resource if appropriate then do a full-power release
        GenerateResourceOnCast(caster);
        OnRelease(caster, target, 1f);
    }
}