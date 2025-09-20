using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/SummonWolf")]
    public class SummonWolf : AbilityBase
    {
        [SerializeField] private GameObject wolfPrefab;
        [SerializeField] private Vector2 offset = new(2f, 0f);

        public override bool CanCast(GameObject caster, GameObject target) => wolfPrefab != null;

        public override void Execute(GameObject caster, GameObject target)
        {
            if (wolfPrefab == null) return;

            var wolf = Instantiate(wolfPrefab, caster.transform.position + (Vector3)offset, Quaternion.identity);

            // If you later add a SetFaction(Faction) API to Health, copy allegiance here:
            // if (wolf.TryGetComponent<Health>(out var wH) && caster.TryGetComponent<Health>(out var cH))
            //     wH.SetFaction(cH.GetFaction());
        }
    }
}
