using Havengard.Character;
using Havengard.HealthSystem;
using Havengard.Units;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Wall Of Fire")]
    public class WallOfFire : AbilityBase
    {
        [Header("Wall Settings")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private float wallLength = 5f;
        [SerializeField] private int baseDamage = 10;
        [SerializeField] private float duration = 4f;

        [Header("Faction / Friendly Fire")]
        [SerializeField] private bool friendlyFire = false;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return wallPrefab != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (wallPrefab == null) return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            Vector3 delta = mouseWorld - caster.transform.position;

            // If mouse is above/below player => horizontal wall
            bool horizontal = Mathf.Abs(delta.y) > Mathf.Abs(delta.x);
            Quaternion rotation = horizontal ? Quaternion.identity : Quaternion.Euler(0, 0, 90f);

            // Place at mouse position (feels best for “wall placement”)
            Vector3 spawnPos = mouseWorld;

            var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;

            // Use attack stat if available; otherwise fall back to baseDamage
            int damage = baseDamage;
            var statsComp = caster.GetComponent<StatsComponent>();
            if (statsComp != null && statsComp.CurrentStats != null)
                damage = statsComp.CurrentStats.Attack;

            // Spawn wall
            GameObject wallGO = Instantiate(wallPrefab, spawnPos, rotation);

            // Scale wall length
            wallGO.transform.localScale = new Vector3(
                horizontal ? wallLength : wallGO.transform.localScale.x,
                horizontal ? wallGO.transform.localScale.y : wallLength,
                wallGO.transform.localScale.z
            );

            // Ensure runtime behaviour exists
            var zone = wallGO.GetComponent<WallOfFireZone>();
            if (zone == null)
                zone = wallGO.AddComponent<WallOfFireZone>();

            zone.Init(casterFaction, friendlyFire, damage, duration);
        }
    }
}
