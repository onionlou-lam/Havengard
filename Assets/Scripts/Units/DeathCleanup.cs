using UnityEngine;
using Havengard.HealthSystem;

[DisallowMultipleComponent]
public class DeathCleanup : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 0.35f; // tweak later to match death anim length
    [SerializeField] private GameObject deathVFX;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;
    [SerializeField] private bool disableCollidersImmediately = true;
    [SerializeField] private bool disableAllBehavioursOnDeath = true;

    private bool cleaned;

    private void Awake()
    {
        var h = GetComponent<Health>();
        if (h != null) h.OnDeath += HandleDeath;
    }
    private void OnDestroy()
    {
        var h = GetComponent<Health>();
        if (h != null) h.OnDeath -= HandleDeath;
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
                if (b != this) b.enabled = false; // keep this running to schedule Destroy
        }

        if (deathVFX) Destroy(Instantiate(deathVFX, transform.position, Quaternion.identity), 2f);
        if (deathSFX) AudioSource.PlayClipAtPoint(deathSFX, transform.position, sfxVolume);

        Destroy(gameObject, destroyDelay); // swap to Animator trigger + longer delay later
    }
}
