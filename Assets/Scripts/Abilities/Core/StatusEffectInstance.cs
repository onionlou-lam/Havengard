using UnityEngine;

namespace Havengard.Abilities
{
    public class StatusEffectInstance
    {
        public StatusEffectData Data { get; private set; }
        public GameObject Target { get; private set; }
        public GameObject Source { get; private set; }

        private float elapsedTime;
        private float nextTickTime;

        public StatusEffectInstance(StatusEffectData data, GameObject target, GameObject source)
        {
            Data = data;
            Target = target;
            Source = source;
            elapsedTime = 0f;
            nextTickTime = data.tickInterval;
        }

        public void Update(float deltaTime)
        {
            elapsedTime += deltaTime;

            // Apply damage ticks
            if (Data.damagePerTick > 0 && Time.time >= nextTickTime)
            {
                var health = Target.GetComponent<Havengard.Core.HealthSystem.Health>();
                if (health != null)
                {
                    health.TakeDamage((int)Data.damagePerTick, Source);
                }
                nextTickTime = Time.time + Data.tickInterval;
            }
        }

        public bool IsExpired()
        {
            if (Data.isPermanent) return false;
            return elapsedTime >= Data.duration;
        }

        public float RemainingTime => Mathf.Max(0f, Data.duration - elapsedTime);
        public float Progress => Data.duration > 0 ? elapsedTime / Data.duration : 1f;
    }
}