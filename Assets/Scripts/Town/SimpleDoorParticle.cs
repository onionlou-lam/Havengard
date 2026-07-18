using UnityEngine;

namespace Havengard.Town
{
    /// <summary>
    /// Simple particle effect for door hover feedback.
    /// Attach to a GameObject with a ParticleSystem component.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class SimpleDoorParticle : MonoBehaviour
    {
        private ParticleSystem ps;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            if (ps != null)
            {
                ps.Play();
            }
        }

        public void StopEffect()
        {
            if (ps != null)
            {
                ps.Stop();
            }
        }
    }
}