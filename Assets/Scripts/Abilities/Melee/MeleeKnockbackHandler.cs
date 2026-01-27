using System.Collections;
using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Helper component to apply knockback force over time.
    /// Automatically added to targets when knockback is applied.
    /// </summary>
    public class MeleeKnockbackHandler : MonoBehaviour
    {
        private Coroutine activeKnockback;

        public void ApplyKnockback(Rigidbody2D rb, Vector2 force, float duration)
        {
            if (activeKnockback != null)
                StopCoroutine(activeKnockback);

            activeKnockback = StartCoroutine(KnockbackRoutine(rb, force, duration));
        }

        private IEnumerator KnockbackRoutine(Rigidbody2D rb, Vector2 force, float duration)
        {
            float elapsed = 0f;
            Vector2 velocity = force / duration;

            while (elapsed < duration)
            {
                rb.linearVelocity = Vector2.Lerp(velocity, Vector2.zero, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
        }
    }
}