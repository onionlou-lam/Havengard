using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Summon Wolf")]
    public class SummonWolf : AbilityBase
    {
        [SerializeField] private GameObject wolfPrefab;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return wolfPrefab != null && caster != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (!CanCast(caster, target)) return;

            Vector3 spawnPos = caster.transform.position + caster.transform.right;
            Instantiate(wolfPrefab, spawnPos, Quaternion.identity);
        }
    }
}
