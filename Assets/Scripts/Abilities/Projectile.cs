using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;

namespace Havengard.Abilities
{
    public class Projectile : MonoBehaviour
    {
        public Faction sourceFaction;
        public bool friendlyFire = false;

        public int damage = 10;
        public float speed = 10f;
        public float lifeTime = 5f;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.Translate(Vector3.right * (speed * Time.deltaTime));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var healthComp = other.GetComponent<IHealth>();
            if (!FactionUtility.CanDamage(sourceFaction, healthComp, friendlyFire)) return;

            healthComp.GetHealthSystem().Damage(damage);
            Destroy(gameObject);
        }
    }
}
