using UnityEngine;
using UnityEngine.AI;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Roll")]
    public class RollAbility : AbilityBase
    {
        [SerializeField] private float rollDistance = 3f;
        [SerializeField] private float rollDuration = 0.15f;
        [SerializeField] private float rollSpeed = 20f; // Fast movement during roll

        public bool CanCast(GameObject caster, GameObject target)
        {
            return caster != null;
        }

        public void Cast(GameObject caster, GameObject target)
        {
            var agent = caster.GetComponent<NavMeshAgent>();
            if (agent == null) return;

            Vector3 targetPos = target != null ? target.transform.position : caster.transform.position + caster.transform.right;
            Vector2 dir = (targetPos - caster.transform.position).normalized;

            caster.GetComponent<MonoBehaviour>().StartCoroutine(RollRoutine(agent, caster.transform, dir));
        }

        private System.Collections.IEnumerator RollRoutine(NavMeshAgent agent, Transform casterTransform, Vector2 dir)
        {
            // Calculate roll destination
            Vector3 rollDestination = casterTransform.position + (Vector3)(dir * rollDistance);
            
            // Store original speed
            float originalSpeed = agent.speed;
            
            // Boost speed for roll
            agent.speed = rollSpeed;
            agent.SetDestination(rollDestination);

            float t = 0f;
            while (t < rollDuration && agent.remainingDistance > 0.1f)
            {
                t += Time.deltaTime;
                yield return null;
            }

            // Restore original speed
            agent.speed = originalSpeed;
            agent.ResetPath(); // Stop movement after roll
        }

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