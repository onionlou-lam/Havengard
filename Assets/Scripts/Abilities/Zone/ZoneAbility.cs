using Havengard.Core;
using Havengard.HealthSystem;
using System.Collections;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Zone Ability")]
    public class ZoneAbility : AbilityBase
    {
        [Header("Zone Properties")]
        [SerializeField] private GameObject zonePrefab;
        [SerializeField] private float maximumRange = 15f; // Maximum range of ability cast
        [SerializeField] private float areaOfEffectRadius = 5f; // AoE of the zone
        [SerializeField] private float duration = 5f; // Duration of the zone
        [SerializeField] private float delayBeforeEffect = 1f; // Time before the zone becomes active
        [SerializeField] private bool followsCaster = false; // Does the zone follow the player?

        [Header("VFX/SFX")]
        [SerializeField] private GameObject spawnVFX;
        [SerializeField] private GameObject activeVFX;
        [SerializeField] private AudioClip spawnSFX;
        [SerializeField] private AudioClip loopSFX;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            if (zonePrefab == null) return false;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            return Vector3.Distance(caster.transform.position, mouseWorld) <= maximumRange;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (zonePrefab == null || !CanCast(caster, target)) return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            Vector3 targetPosition = followsCaster ? caster.transform.position : mouseWorld;

            GameObject zoneInstance = Instantiate(zonePrefab, targetPosition, Quaternion.identity);

            if (spawnVFX != null)
                Instantiate(spawnVFX, targetPosition, Quaternion.identity, zoneInstance.transform);

            if (spawnSFX != null)
                AudioSource.PlayClipAtPoint(spawnSFX, targetPosition);

            GameManager.Instance.StartCoroutine(ZoneEffectCoroutine(zoneInstance, caster));
        }

        private IEnumerator ZoneEffectCoroutine(GameObject zoneInstance, GameObject caster)
        {
            AudioSource audioSource = null;
            if (loopSFX != null)
            {
                audioSource = zoneInstance.AddComponent<AudioSource>();
                audioSource.clip = loopSFX;
                audioSource.loop = true;
                audioSource.Play();
            }

            yield return new WaitForSeconds(delayBeforeEffect);

            if (activeVFX != null)
                Instantiate(activeVFX, zoneInstance.transform.position, Quaternion.identity, zoneInstance.transform);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                if (followsCaster)
                    zoneInstance.transform.position = caster.transform.position;

                foreach (var hit in Physics2D.OverlapCircleAll(zoneInstance.transform.position, areaOfEffectRadius))
                {
                    var health = hit.GetComponent<IHealth>();
                    if (health != null)
                    {
                        health.GetHealthSystem().Damage(5);
                        ApplyBuffDebuff(hit.gameObject);
                    }
                }

                yield return null;
            }

            if (audioSource != null)
                audioSource.Stop();

            Destroy(zoneInstance);
        }
    }
}