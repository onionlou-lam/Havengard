using UnityEngine;
using System.Collections;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;

namespace Havengard.Abilities
{
    public class WallOfFireZone : MonoBehaviour
    {
        public Faction sourceFaction;
        public bool friendlyFire;
        public int baseDamage;

        [Header("DoT Settings")]
        [SerializeField] private float tickInterval = 1f;
        [SerializeField] private int dotDamage = 5;
        [SerializeField] private float dotDuration = 3f;

        [Header("Visuals")]
        [SerializeField] private float fadeDuration = 1f;  // duration of shrink/fade
        [SerializeField]
        private AnimationCurve fadeScaleCurve =
            AnimationCurve.EaseInOut(0, 1, 1, 0);         // nice smooth shrink

        private float lifetime;
        private float elapsed;
        private bool fading;
        private Vector3 initialScale;
        private ParticleSystem[] particles;

        public void Init(Faction faction, bool allowFriendlyFire, int dmg, float duration)
        {
            sourceFaction = faction;
            friendlyFire = allowFriendlyFire;
            baseDamage = dmg;
            lifetime = duration;

            initialScale = transform.localScale;
            particles = GetComponentsInChildren<ParticleSystem>();
        }

        private void Update()
        {
            if (fading) return;
            if (lifetime <= 0) return;

            elapsed += Time.deltaTime;
            if (elapsed >= lifetime)
            {
                StartCoroutine(FadeAndDestroy());
                fading = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponent<IHealth>();
            if (health == null) return;
            if (!FactionUtility.CanDamage(sourceFaction, health, friendlyFire)) return;

            int hitDamage = CombatCalculator.CalculateDamage(gameObject, other.gameObject);
            health.GetHealthSystem().Damage(hitDamage);

            DamageOverTimeEffect.ApplyTo(other.gameObject, dotDamage, tickInterval, dotDuration);
        }

        private IEnumerator FadeAndDestroy()
        {
            // Stop particle emission for a softer exit
            if (particles != null)
            {
                foreach (var ps in particles)
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                }
            }

            float t = 0f;
            while (t < fadeDuration)
            {
                float normalized = t / fadeDuration;
                float scaleFactor = fadeScaleCurve.Evaluate(normalized);
                foreach (var ps in particles)
                {
                    var main = ps.main;
                    Color c = main.startColor.color;
                    c.a = 1f - normalized;   // fade alpha
                    main.startColor = c;
                }

                transform.localScale = initialScale * scaleFactor;

                t += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
