using UnityEngine;
using System.Collections;
using Havengard.HealthSystem;
using Havengard.Character;

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

        // Stacking
        private int stacks = 1;
        private StatusVFXStack vfxStack;

#pragma warning disable CS0414
        [SerializeField] private bool controlsDisabled = false;
#pragma warning restore CS0414

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
            vfxStack = targetMono.GetComponent<StatusVFXStack>();
            if (vfxStack == null) vfxStack = targetMono.gameObject.AddComponent<StatusVFXStack>();

            remainingTime = Mathf.Max(0.01f, data.duration);
            stacks = 1;

            // VFX (one stack on first apply)
            if (data.attachVFX != null)
                vfxStack.AddStack(data.attachVFX, data.duration);

            if (data.applySFX != null)
                AudioSource.PlayClipAtPoint(data.applySFX, targetMono.transform.position);

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

                // Stackable DoT = tickDamage * stacks
                int tick = Data.tickDamage * Mathf.Max(1, stacks);
                targetHealth.GetHealthSystem().Damage(tick);

                yield return new WaitForSeconds(Mathf.Max(0.01f, Data.tickInterval));
            }
        }

        private void ApplyModifiers()
        {
            if (stats != null)
            {
                // NOTE: this assumes CurrentStats is a class or you have setter methods.
                // If CurrentStats is a struct, you MUST modify a local copy and assign back.
                var s = stats.CurrentStats;
                s.MoveSpeed *= Data.moveSpeedMultiplier;
                s.AttackSpeed *= Data.attackSpeedMultiplier;
                s.Attack = Mathf.RoundToInt(s.Attack * Data.damageMultiplier);
                s.Defense = Mathf.RoundToInt(s.Defense * Data.defenseMultiplier);
                stats.SetCurrentStats(s);
            }

            if (Data.causesStun || Data.causesRoot || Data.causesSilence)
                controlsDisabled = true;
        }

        private void RemoveModifiers()
        {
            if (stats != null)
            {
                var s = stats.CurrentStats;
                s.MoveSpeed /= Data.moveSpeedMultiplier;
                s.AttackSpeed /= Data.attackSpeedMultiplier;
                s.Attack = Mathf.RoundToInt(s.Attack / Data.damageMultiplier);
                s.Defense = Mathf.RoundToInt(s.Defense / Data.defenseMultiplier);
                stats.SetCurrentStats(s);
            }

            controlsDisabled = false;
        }

        /// <summary>
        /// Called when the same effect is applied again.
        /// Stack if allowed, otherwise refresh if configured.
        /// </summary>
        public void RefreshOrStack(StatusEffectData newData)
        {
            if (newData == null || Data == null) return;

            if (Data.stackable)
            {
                stacks++;

                // Optionally refresh duration when stacking
                if (Data.refreshDurationOnReapply)
                    remainingTime = Mathf.Max(remainingTime, newData.duration);

                // Add another VFX stack for feedback
                if (newData.attachVFX != null && vfxStack != null)
                    vfxStack.AddStack(newData.attachVFX, newData.duration);
            }
            else if (Data.refreshDurationOnReapply)
            {
                remainingTime = newData.duration;
            }
        }

        // Backwards-compatible alias if any scripts still call "Reapply"
        public void Reapply(StatusEffectData newData) => RefreshOrStack(newData);

        public bool IsStunned() => Data != null && Data.causesStun;
        public bool IsRooted() => Data != null && Data.causesRoot;
        public bool IsSilenced() => Data != null && Data.causesSilence;
    }
}
