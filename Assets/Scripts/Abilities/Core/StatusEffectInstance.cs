using System.Collections;
using UnityEngine;
using Havengard.Core.HealthSystem;
using Havengard.Core.Character;

namespace Havengard.Statuses
{
    [DisallowMultipleComponent]
    public class StatusEffectInstance : MonoBehaviour
    {
        public StatusEffectData Data { get; private set; }

        private IHealth targetHealth;
        private StatsComponent stats;

        private float remainingTime;
        private Coroutine tickRoutine;

        private int stackCount = 1;

        private Stats baseSnapshot;

        public void Apply(StatusEffectData data, IHealth target)
        {
            Data = data;
            targetHealth = target;

            var targetMono = target as MonoBehaviour;
            if (targetMono == null)
            {
                Destroy(this);
                return;
            }

            stats = targetMono.GetComponent<StatsComponent>();
            remainingTime = data.duration;
            stackCount = 1;

            if (stats != null)
            {
                baseSnapshot = stats.GetCurrentStatsClone();
            }

            // Add VFX stack
            if (data.attachVFX != null)
            {
                StatusVFXStack.AddStack(this, GetInstanceID(), data.attachVFX, data.duration, data.maxStacks, forceRefresh: false);
            }

            if (data.applySFX != null)
                AudioSource.PlayClipAtPoint(data.applySFX, targetMono.transform.position);

            RecomputeModifiers();

            if (Data.causesDamage)
                tickRoutine = StartCoroutine(TickDamage());

            StartCoroutine(Lifetime());
        }

        public int GetStackCount() => stackCount;

        /// <summary>
        /// Try to refresh duration (used when stacks are capped or non-stackable).
        /// </summary>
        public bool TryRefreshDuration(float newDuration)
        {
            if (Data == null) return false;
            if (!Data.refreshDurationOnReapply) return false;

            remainingTime = newDuration;
            return true;
        }

        /// <summary>
        /// If stackable, increments stack count up to maxStacks, else refreshes duration when allowed.
        /// </summary>
        public void RefreshOrStack(StatusEffectData newData, int maxStacks = int.MaxValue)
        {
            if (Data == null)
            {
                Apply(newData, targetHealth);
                return;
            }

            if (Data.stackable)
            {
                if (stackCount < Mathf.Max(1, maxStacks))
                {
                    stackCount++;
                    remainingTime = Mathf.Max(remainingTime, newData.duration);

                    // Add another VFX stack using the AddStack method (not forced)
                    if (newData.attachVFX != null)
                    {
                        StatusVFXStack.AddStack(this, GetInstanceID(), newData.attachVFX, newData.duration, newData.maxStacks, forceRefresh: false);
                    }

                    RecomputeModifiers();
                    return;
                }

                // At cap → refresh duration if allowed AND force spawn VFX for visual feedback
                bool refreshed = TryRefreshDuration(newData.duration);
                
                if (refreshed && newData.attachVFX != null)
                {
                    StatusVFXStack.AddStack(this, GetInstanceID(), newData.attachVFX, newData.duration, newData.maxStacks, forceRefresh: true);
                }
                return;
            }

            // Non-stackable → refresh duration if allowed AND force spawn VFX for visual feedback
            bool durationRefreshed = TryRefreshDuration(newData.duration);
            
            if (durationRefreshed && newData.attachVFX != null)
            {
                StatusVFXStack.AddStack(this, GetInstanceID(), newData.attachVFX, newData.duration, newData.maxStacks, forceRefresh: true);
            }
        }

        private IEnumerator Lifetime()
        {
            while (remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            RemoveModifiers();
            Destroy(this);
        }

        private IEnumerator TickDamage()
        {
            while (remainingTime > 0f)
            {
                if (targetHealth == null) yield break;

                // Scale DoT with stacks (tweak as desired)
                int dmg = Data.tickDamage * Mathf.Max(1, stackCount);
                targetHealth.GetHealthSystem().Damage(dmg);

                yield return new WaitForSeconds(Data.tickInterval);
            }
        }

        private void RecomputeModifiers()
        {
            if (stats == null || Data == null || baseSnapshot == null) return;

            // Rebuild runtime stats from baseline snapshot, then apply stack-scaled multipliers.
            Stats s = baseSnapshot.Clone();

            float ms = Mathf.Pow(Data.moveSpeedMultiplier, stackCount);
            float aspeed = Mathf.Pow(Data.attackSpeedMultiplier, stackCount);
            float dmgMul = Mathf.Pow(Data.damageMultiplier, stackCount);
            float defMul = Mathf.Pow(Data.defenseMultiplier, stackCount);

            s.MoveSpeed *= ms;
            s.AttackSpeed *= aspeed;
            s.Attack = Mathf.RoundToInt(s.Attack * dmgMul);
            s.Defense = Mathf.RoundToInt(s.Defense * defMul);

            stats.SetCurrentStats(s);
        }

        private void RemoveModifiers()
        {
            if (stats != null && baseSnapshot != null)
            {
                stats.SetCurrentStats(baseSnapshot);
            }
        }

        public bool IsStunned() => Data != null && Data.causesStun;
        public bool IsRooted() => Data != null && Data.causesRoot;
        public bool IsSilenced() => Data != null && Data.causesSilence;

        // Add this method to StatusEffectInstance class:
        public float GetLifestealPercent()
        {
            if (Data == null) return 0f;
            // Scale lifesteal with stacks (additive)
            return Data.lifestealPercent * Mathf.Max(1, stackCount);
        }
    }
}
