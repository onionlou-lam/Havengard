using UnityEngine;

namespace Havengard.Abilities
{
    public abstract class AbilityBase : ScriptableObject
    {
        [Header("General Settings")]
        public string abilityName;
        public float cooldown = 1f;
        public float resourceCost = 0f;

        private float lastCastTime;

        public bool CanCast(GameObject caster, GameObject target)
        {
            if (Time.time < lastCastTime + cooldown)
            {
                Debug.LogWarning($"AbilityBase: {abilityName} is on cooldown.");
                return false;
            }

            if (target == null)
            {
                Debug.LogWarning($"AbilityBase: {abilityName} has no valid target.");
                return false;
            }

            return true;
        }


        public void Cast(GameObject caster, GameObject target)
        {
            Debug.Log($"AbilityBase: Attempting to cast {abilityName} from {caster.name} to {target?.name}");

            if (!CanCast(caster, target))
            {
                Debug.LogWarning($"AbilityBase: Cannot cast {abilityName} due to cooldown or invalid target.");
                return;
            }

            Execute(caster, target);
            lastCastTime = Time.time;
        }


        protected abstract void Execute(GameObject caster, GameObject target);
    }
}
