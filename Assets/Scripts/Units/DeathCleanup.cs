using UnityEngine;
using Havengard.HealthSystem;

[DisallowMultipleComponent]
public class DeathCleanup : MonoBehaviour
{
    [Header("FX")]
    [SerializeField] private GameObject deathVFX;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;

    [Header("Cleanup")]
    [SerializeField] private bool disableCollidersImmediately = true;
    [SerializeField] private bool disableAllBehavioursOnDeath = true;
    [SerializeField] private float fallbackDestroyDelay = 2f; // only used if no animation event

    private bool cleaned;
    private bool destroyScheduled;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null) health.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        if (health != null) health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (cleaned) return;
        cleaned = true;

        if (disableCollidersImmediately)
        {
            foreach (var c in GetComponentsInChildren<Collider2D>(true)) c.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = false;
        }

        if (disableAllBehavioursOnDeath)
        {
            foreach (var b in GetComponentsInChildren<Behaviour>(true))
            {
                if (b == this) continue;
                if (b is Animator) continue; // keep death anim playing
                b.enabled = false;
            }
        }

        if (deathVFX) Destroy(Instantiate(deathVFX, transform.position, Quaternion.identity), 2f);
        if (deathSFX) AudioSource.PlayClipAtPoint(deathSFX, transform.position, sfxVolume);

        // Safety fallback in case the animation event is missing
        if (!destroyScheduled)
        {
            destroyScheduled = true;
            Destroy(gameObject, fallbackDestroyDelay);
        }

    }

    // Call this from an Animation Event at the end of the Death animation clip.
    public void OnDeathAnimationFinished()
    {
        // If you use pooling later, replace Destroy with ReturnToPool().
        if (gameObject != null)
            Destroy(gameObject);
    }
}
