using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/SummonWolf")]
    public class SummonWolf : AbilityBase
    {
        [SerializeField] private GameObject wolfPrefab;

        protected override void Execute(GameObject caster, GameObject target)
        {
            if (wolfPrefab == null) return;

            Vector3 spawnPos = caster.transform.position + (Vector3.right * 2);
            Instantiate(wolfPrefab, spawnPos, Quaternion.identity);
        }
    }
}
