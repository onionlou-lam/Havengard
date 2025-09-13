public interface IHealth
{
    float CurrentHealth { get; }
    float MaxHealth { get; }

    Faction GetFaction();

    void TakeDamage(float amount);
    void Heal(float amount);

    event System.Action OnDamaged;
    event System.Action OnHealed;
    event System.Action OnDeath;
}
