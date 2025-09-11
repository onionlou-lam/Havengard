using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Player
{
    public class PlayerController : MonoBehaviour
    {
        private IHealth health;

        private void Awake()
        {
            health = GetComponent<IHealth>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                health.GetHealthSystem().Damage(10f);
                Debug.Log($"{gameObject.name} took 10 damage. Current HP: {health.GetHealthSystem().GetHealth()}");
            }
        }
    }
}
