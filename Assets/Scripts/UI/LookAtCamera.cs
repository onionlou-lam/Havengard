using UnityEngine;

namespace Havengard.Core.HealthSystem
{
    /// <summary>
    /// Makes a Transform face the main camera.
    /// Commonly used for world-space UI elements (like health bars).
    /// </summary>
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private bool invert = false;

        private Transform mainCameraTransform;

        private void Awake()
        {
            if (Camera.main != null)
                mainCameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (mainCameraTransform != null)
                LookAt();
        }

        private void OnEnable()
        {
            if (mainCameraTransform != null)
                LookAt();
        }

        private void LookAt()
        {
            if (invert)
            {
                Vector3 dir = (transform.position - mainCameraTransform.position).normalized;
                transform.LookAt(transform.position + dir);
            }
            else
            {
                transform.LookAt(mainCameraTransform.position);
            }
        }
    }
}
