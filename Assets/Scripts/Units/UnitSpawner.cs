using UnityEngine;


namespace Havengard.Units
{
    public class UnitSpawner : MonoBehaviour
    {
        [Header("Generic Spawner")]
        [SerializeField] private Transform defaultSpawnPoint;

        // Spawn any NavMesh-based unit (enemy, ally, boss, etc.)
        public UnitBase Spawn(UnitBase prefab, Vector3? position = null, Quaternion? rotation = null)
        {
            Vector3 pos = position ?? (defaultSpawnPoint ? defaultSpawnPoint.position : transform.position);
            Quaternion rot = rotation ?? Quaternion.identity;
            return Instantiate(prefab, pos, rot);
        }

        // Overload for Melee
        public MeleeEnemy SpawnMelee(MeleeEnemy prefab, Vector3? position = null, Quaternion? rotation = null)
        {
            return (MeleeEnemy)Spawn((UnitBase)prefab, position, rotation);
        }

        // Overload for Ranged
        public RangedEnemy SpawnRanged(RangedEnemy prefab, Vector3? position = null, Quaternion? rotation = null)
        {
            return (RangedEnemy)Spawn((UnitBase)prefab, position, rotation);
        }

        // Overload for Allies
        public AllyUnit SpawnAlly(AllyUnit prefab, Vector3? position = null, Quaternion? rotation = null)
        {
            return (AllyUnit)Spawn((UnitBase)prefab, position, rotation);
        }
    }
}
