using UnityEngine;

namespace Havengard.Utility
{
    /// <summary>
    /// Automatically destroys a particle system object once it finishes playing.
    /// Attach this to any particle prefab (e.g., fireball explosion, ice impact, etc.)
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class AutoDestroyParticle : MonoBehaviour
    {
        private ParticleSystem ps;

        void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        void Update()
        {
            if (ps != null && !ps.IsAlive(true))
            {
                Destroy(gameObject);
            }
        }
    }
}
