using UnityEngine;

public class EnemyVFX : MonoBehaviour
{
    [SerializeField] private GameObject hitEffectPrefab;

    public void PlayHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
