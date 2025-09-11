using UnityEngine;

namespace Havengard.Player
{
    public class PlayerAbilities : MonoBehaviour
    {
        private Havengard.Abilities.AbilityUser abilityUser;

        private void Awake()
        {
            abilityUser = GetComponent<Havengard.Abilities.AbilityUser>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                abilityUser.UseAbility(0, GetMouseTarget());

            if (Input.GetKeyDown(KeyCode.Alpha2))
                abilityUser.UseAbility(1, GetMouseTarget());
        }

        private GameObject GetMouseTarget()
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);
            return hit.collider != null ? hit.collider.gameObject : null;
        }
    }
}
