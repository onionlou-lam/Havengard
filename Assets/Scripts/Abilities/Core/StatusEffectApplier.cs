using Havengard.Statuses;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Component that manages status effects on a game object
    /// </summary>
    public class StatusEffectApplier : MonoBehaviour
    {
        private List<StatusEffectInstance> activeEffects = new List<StatusEffectInstance>();

        private void Update()
        {
            // Update all active effects
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].IsExpired())
                {
                    activeEffects.RemoveAt(i);
                }
                else
                {
                    activeEffects[i].Update(Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Apply a status effect to this game object
        /// </summary>
        public void ApplyStatusEffect(StatusEffectData data, GameObject source)
        {
            if (data == null) return;

            var instance = new StatusEffectInstance(data, gameObject, source);
            activeEffects.Add(instance);

            Debug.Log($"[StatusEffectApplier] Applied {data.effectName} to {gameObject.name}");
        }

        /// <summary>
        /// Remove all effects of a specific type
        /// </summary>
        public void RemoveEffect(string effectName)
        {
            activeEffects.RemoveAll(e => e.Data.effectName == effectName);
        }

        /// <summary>
        /// Clear all active effects
        /// </summary>
        public void ClearAllEffects()
        {
            activeEffects.Clear();
        }

        public IReadOnlyList<StatusEffectInstance> ActiveEffects => activeEffects;
    }
}