using UnityEngine;

public class Health : MonoBehaviour, IHealth
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private FactionProvider factionProvider;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public event System.Action OnDamaged;
    public event System.Action OnHealed;
    public event System.Action OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
        factionProvider = GetComponent<FactionProvider>();
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        currentHealth -= amount;
        OnDamaged?.Invoke();

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        OnHealed?.Invoke();
    }

    public Faction GetFaction()
    {
        return factionProvider != null ? factionProvider.faction : Faction.Neutral;
    }
}
