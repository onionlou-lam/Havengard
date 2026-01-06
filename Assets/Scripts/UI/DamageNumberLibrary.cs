using UnityEngine;
using DamageNumbersPro;

namespace Havengard.UI
{
    [CreateAssetMenu(menuName = "Havengard/UI/Damage Numbers Library", fileName = "DamageNumbersLibrary")]
    public class DamageNumberLibrary : ScriptableObject
    {
        [Header("Prefab Assets (must be PREFAB ASSETS, not scene objects)")]
        [Tooltip("Your BaseDamage prefab that has the DamageNumberMesh component.")]
        [SerializeField] private DamageNumberMesh damageNumberPrefab;

        [Tooltip("Optional heal number prefab (DamageNumberMesh).")]
        [SerializeField] private DamageNumberMesh healNumberPrefab;

        public DamageNumberMesh DamagePrefab => damageNumberPrefab;
        public DamageNumberMesh HealPrefab => healNumberPrefab;
    }
}
