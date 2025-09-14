using UnityEngine;
using Havengard.Health;
using Havengard.UI;

namespace Havengard.UI
{
    public class HealthBarSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject healthBarPrefab; // prefab with HealthBarUI and Slider
        [SerializeField] private Transform parentCanvas;     // world-space canvas or UI root

        private void Start()
        {
            if (healthBarPrefab == null || parentCanvas == null) return;

            if (GetComponent<IHealth>() is IHealth health)
            {
                var go = Instantiate(healthBarPrefab, parentCanvas);
                var ui = go.GetComponent<HealthBarUI>();
                ui.Setup(health);
            }
        }
    }
}
