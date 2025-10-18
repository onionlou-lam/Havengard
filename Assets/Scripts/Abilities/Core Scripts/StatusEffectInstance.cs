using UnityEngine;
using System.Collections;
using Havengard.HealthSystem;
using Havengard.Combat;
using Havengard.Character;

namespace Havengard.Statuses
{
    /// <summary>
    /// Handles runtime application of a status effect (damage over time, stuns, slows, buffs, etc.)
    /// </summary>
    public class StatusEffectInstance : MonoBehaviour
    {
        public StatusEffectData Data { get; private set; }
        private IHealth targetHealth;
        private StatsComponent stats;
        private float remainingTime;
        private Coroutine tickRoutine;
        private bool controlsDisabled;

        public void Apply(StatusEffectData data, IHealth target)
        {
            Data = data;
            targetHealth = target;
            stats = (target as MonoBehaviour)?.GetComponent<StatsComponent>();
            remainingTime = data.duration;

            var targetMono = target as MonoBehaviour;
            if (targetMono != null)
            {
                // Spawn attached VFX
                if (data.attachVFX != null)
                {
                    var fx = Instantiate(data.attachVFX, targetMono.transform.position, Quaternion.identity, targetMono.transform);
                    Destroy(fx, data.duration);
                }

                // Play sound
                if (data.applySFX != null)
                    AudioSource.PlayClipAtPoint(data.applySFX, targetMono.transform.position);
            }

            ApplyModifiers();

            if (Data.causesDamage)
                tickRoutine = StartCoroutine(TickDamage());

            StartCoroutine(Lifetime());
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
                targetHealth.GetHealthSystem().Damage(Data.tickDamage);
                yield return new WaitForSeconds(Data.tickInterval);
            }
        }

        private void ApplyModifiers()
        {
            if (stats != null)
            {
                stats.CurrentStats.MoveSpeed *= Data.moveSpeedMultiplier;
                stats.CurrentStats.AttackSpeed *= Data.attackSpeedMultiplier;
                stats.CurrentStats.Attack = Mathf.RoundToInt(stats.CurrentStats.Attack * Data.damageMultiplier);
                stats.CurrentStats.Defense = Mathf.RoundToInt(stats.CurrentStats.Defense * Data.defenseMultiplier);
            }

            if (Data.causesStun || Data.causesRoot || Data.causesSilence)
                controlsDisabled = true;
        }

        private void RemoveModifiers()
        {
            if (stats != null)
            {
                stats.CurrentStats.MoveSpeed /= Data.moveSpeedMultiplier;
                stats.CurrentStats.AttackSpeed /= Data.attackSpeedMultiplier;
                stats.CurrentStats.Attack = Mathf.RoundToInt(stats.CurrentStats.Attack / Data.damageMultiplier);
                stats.CurrentStats.Defense = Mathf.RoundToInt(stats.CurrentStats.Defense / Data.defenseMultiplier);
            }

            controlsDisabled = false;
        }

        public void RefreshOrStack(StatusEffectData newData)
        {
            if (Data.stackable)
            {
                var newInstance = (targetHealth as MonoBehaviour).gameObject.AddComponent<StatusEffectInstance>();
                newInstance.Apply(newData, targetHealth);
            }
            else if (Data.refreshDurationOnReapply)
            {
                remainingTime = newData.duration;
            }
        }

        public bool IsStunned() => Data != null && Data.causesStun;
        public bool IsRooted() => Data != null && Data.causesRoot;
        public bool IsSilenced() => Data != null && Data.causesSilence;
    }
}
