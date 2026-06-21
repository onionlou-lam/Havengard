using Havengard.Core.HealthSystem;
using Havengard.Statuses;
using Havengard.Units;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Orbital Explosion Ability")]
    public class OrbitalExplosionAbility : ZoneAbility
    {
        [Header("Orbital Explosion")]
        [SerializeField] private float vortexSpeed = 5f;
        [SerializeField] private float vortexDuration = 2f;
        [SerializeField] private float explosionRadius = 8f;
        [SerializeField] private float explosionDamageMultiplier = 2f;

        [Header("Vortex Damage Over Time")]
        [Tooltip("Damage dealt per tick while in the vortex (0 = no vortex damage)")]
        [SerializeField] private int vortexDamagePerTick = 2;
        [Tooltip("Time between damage ticks during vortex phase")]
        [SerializeField] private float vortexTickInterval = 0.5f;
        [Tooltip("Optional status effect applied during vortex phase")]
        [SerializeField] private StatusEffectData vortexStatusEffect;
        [SerializeField] private int maxVortexStatusStacks = 1;

        [Header("Knockback")]
        [SerializeField] private bool enableKnockback = true;
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private float knockbackDuration = 0.3f;

        [Header("VFX")]
        [SerializeField] private GameObject explosionVFXPrefab;

        [Header("Scale Animation")]
        [Tooltip("Starting scale of the ability (e.g., 0.1 for small, 2 for large). Leave as (1,1,1) for no animation")]
        [SerializeField] private Vector3 startScale = Vector3.one;
        [Tooltip("Target scale (normal size). Usually (1,1,1)")]
        [SerializeField] private Vector3 targetScale = Vector3.one;
        [Tooltip("Duration of scale animation in seconds (0 = no animation)")]
        [SerializeField] private float scaleDuration = 0f;
        [Tooltip("Animation curve for scale transition (default is linear)")]
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public override void Cast(GameObject caster, GameObject target)
        {
            if (!CanCast(caster, target))
                return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            GameObject zoneInstance = Instantiate(ZonePrefab, mouseWorld, Quaternion.identity);

            OrbitalExplosionEffect effect = zoneInstance.GetComponent<OrbitalExplosionEffect>();

            if (effect == null)
            {
                Debug.LogError($"[{nameof(OrbitalExplosionAbility)}] Missing OrbitalExplosionEffect component on prefab!");
                Destroy(zoneInstance);
                return;
            }

            IHealth casterHealth = caster.GetComponent<IHealth>();
            Faction casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            effect.Initialize(
                caster,
                casterFaction,
                vortexSpeed,
                vortexDuration,
                explosionRadius,
                CalculateDamage(caster) * explosionDamageMultiplier,
                enableKnockback,
                knockbackForce,
                knockbackDuration,
                explosionVFXPrefab,
                vortexDamagePerTick,
                vortexTickInterval,
                vortexStatusEffect,
                maxVortexStatusStacks,
                startScale,
                targetScale,
                scaleDuration,
                scaleCurve);
        }
    }
}