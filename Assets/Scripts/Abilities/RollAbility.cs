using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Roll")]
    public class RollAbility : AbilityBase
    {
        [SerializeField] private float rollDistance = 3f;
        [SerializeField] private float rollDuration = 0.15f;

        // REMOVED 'override' - not in base class
        public bool CanCast(GameObject caster, GameObject target)
        {
            return caster != null;
        }

        // REMOVED 'override' - not in base class
        public void Cast(GameObject caster, GameObject target)
        {
            var rb = caster.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            Vector3 targetPos = target != null ? target.transform.position : caster.transform.position + caster.transform.right;
            Vector2 dir = (targetPos - caster.transform.position).normalized;

            caster.GetComponent<MonoBehaviour>().StartCoroutine(RollRoutine(rb, dir));
        }

        private System.Collections.IEnumerator RollRoutine(Rigidbody2D rb, Vector2 dir)
        {
            float speed = rollDistance / Mathf.Max(0.01f, rollDuration);
            Vector2 velocity = dir.normalized * speed;

            float t = 0f;
            while (t < rollDuration)
            {
                rb.linearVelocity = velocity;
                t += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
        }

        // ADD: Implement abstract methods from AbilityBase
        public override void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            Cast(user.gameObject, targetEnemy);
        }

        public override void Deactivate(AbilityUser user)
        {
            // Roll is instant, no cleanup needed
        }
    }
}