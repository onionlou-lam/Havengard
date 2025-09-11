using UnityEngine;

namespace Havengard.Abilities
{
    public class AbilityController : MonoBehaviour
    {
        [SerializeField] private AbilityBase primaryAttack;
        [SerializeField] private AbilityBase secondaryAttack;
        [SerializeField] private AbilityBase[] hotbarAbilities = new AbilityBase[4]; // QWER

        public void UsePrimaryAttack(GameObject target) => primaryAttack?.Cast(gameObject, target);
        public void UseSecondaryAttack(GameObject target) => secondaryAttack?.Cast(gameObject, target);

        public void CastAbility(int index, GameObject target = null)
        {
            if (index < 0 || index >= hotbarAbilities.Length) return;
            hotbarAbilities[index]?.Cast(gameObject, target);
        }
    }
}
