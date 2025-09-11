using UnityEngine;

namespace Havengard.Abilities
{
    public class AbilityUser : MonoBehaviour
    {
        [SerializeField] private AbilityBase[] abilities;

        public void UseAbility(int index, GameObject target)
        {
            if (index < 0 || index >= abilities.Length) return;
            abilities[index].Cast(gameObject, target);
        }
    }
}
