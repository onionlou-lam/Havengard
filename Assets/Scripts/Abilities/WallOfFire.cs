using UnityEngine;
using Havengard.Units;
using Havengard.HealthSystem;
using Havengard.Combat;
using Havengard.Character;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Wall of Fire")]
    public class WallOfFire : AbilityBase
    {
        [Header("Wall Settings")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private float wallDuration = 6f;
        [SerializeField] private float width = 3f;
        [SerializeField] private bool friendlyFire = false;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return wallPrefab != null && caster != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (!CanCast(caster, target)) return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            Vector3 dir = (mousePos - caster.transform.position).normalized;

            // Decide orientation
            bool isHorizontal = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);
            Quaternion rotation = isHorizontal ? Quaternion.identity : Quaternion.Euler(0, 0, 90);

            // Slightly offset the wall so it spawns between player & cursor
            Vector3 spawnPos = caster.transform.position + dir * (width * 0.5f);

            GameObject wall = Instantiate(wallPrefab, spawnPos, rotation);

            var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;
            var casterStats = caster.GetComponent<StatsComponent>()?.CurrentStats;
            int baseDamage = casterStats != null ? casterStats.Attack : 20;

            var wallComp = wall.GetComponent<WallOfFireZone>();
            if (wallComp != null)
                wallComp.Init(casterFaction, friendlyFire, baseDamage);

            Destroy(wall, wallDuration);
        }
    }
}
