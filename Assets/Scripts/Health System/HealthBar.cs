using UnityEngine;
using UnityEngine.UI;
using Havengard.HealthSystem;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    private Health targetHealth;

    public void Init(Health health)
    {
        targetHealth = health;
        health.OnDamaged += UpdateBar;
        health.OnHealed += UpdateBar;
        health.OnDeath += HandleDeath;
        UpdateBar();
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    void LateUpdate()
    {
        if (targetHealth != null)
        {
            transform.position = targetHealth.transform.position + offset;
            transform.rotation = Quaternion.identity; // stay upright in 2D
        }
    }

    private void UpdateBar()
    {
        if (targetHealth == null) return;
        float normalized = targetHealth.GetHealthSystem().GetHealthNormalized();
        fillImage.fillAmount = normalized;
    }
    private void HandleDeath()
    {
        if (HealthBarPool.Instance != null)
            HealthBarPool.Instance.Return(this);
        else
            Destroy(gameObject);
    }
}
