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
                return false;

            // If enemies or players have resource systems, check here
            return true;
        }

        public void Cast(GameObject caster, GameObject target)
        {
            if (!CanCast(caster, target)) return;

            Execute(caster, target);
            lastCastTime = Time.time;
        }

        protected abstract void Execute(GameObject caster, GameObject target);
    }
}
