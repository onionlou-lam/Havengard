using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Roll/Dodge")]
    public class RollAbility : AbilityBase
    {
        [SerializeField] private float rollDistance = 3f;
        [SerializeField] private float rollDuration = 0.15f;

        public override void Execute(GameObject caster, GameObject target)
        {
            if (caster.TryGetComponent<Rigidbody2D>(out var rb))
            {
                caster.GetComponent<MonoBehaviour>().StartCoroutine(RollRoutine(rb, caster));
            }
        }

        private System.Collections.IEnumerator RollRoutine(Rigidbody2D rb, GameObject caster)
        {
            // Decide roll direction
            Vector2 dir = Vector2.zero;

            // If caster is player, roll toward mouse
            if (caster.CompareTag("Player"))
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0f;
                dir = ((Vector2)mouseWorld - rb.position).normalized;
            }
            else
            {
                // Allies/Enemies roll away from nearest threat (optional)
                dir = caster.transform.right; // simple placeholder
            }

            float speed = rollDistance / Mathf.Max(0.01f, rollDuration);
            Vector2 rollVelocity = dir * speed;

            float t = 0f;
            while (t < rollDuration)
            {
                rb.linearVelocity = rollVelocity;
                t += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
        }
    }
}
