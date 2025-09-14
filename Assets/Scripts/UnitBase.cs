using Havengard.Health;
using UnityEngine;
using UnityEngine.AI;

public enum UnitState { Idle, Moving, Attacking }

[RequireComponent(typeof(NavMeshAgent))]
public class UnitBase : MonoBehaviour, IHealth
{
    [Header("Faction Settings")]
    [SerializeField] private FactionProvider factionProvider;

    [Header("Combat Settings")]
    public float detectionRange = 8f;
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    public float maxHealth = 100f;

    protected float currentHealth;
    protected Transform target;
    protected NavMeshAgent agent;
    protected float lastAttackTime;
    private UnitState currentState = UnitState.Idle;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public event System.Action<float> OnDamaged;
    public event System.Action<float> OnHealed;
    public event System.Action OnDeath;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        switch (currentState)
        {
            case UnitState.Idle:
                LookForTargets();
                break;
            case UnitState.Moving:
                HandleMovement();
                break;
            case UnitState.Attacking:
                HandleAttack();
                break;
        }
    }

    private void LookForTargets()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (Collider hit in hits)
        {
            IHealth candidate = hit.GetComponent<IHealth>();
            if (candidate != null && candidate.GetFaction() != GetFaction())
            {
                target = hit.transform;
                currentState = UnitState.Moving;
                break;
            }
        }
    }

    private void HandleMovement()
    {
        if (target == null) { currentState = UnitState.Idle; return; }

        agent.SetDestination(target.position);
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            currentState = UnitState.Attacking;
            agent.ResetPath();
        }
    }

    private void HandleAttack()
    {
        if (target == null) { currentState = UnitState.Idle; return; }

        transform.LookAt(target);

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > attackRange) { currentState = UnitState.Moving; return; }

        if (Time.time - lastAttackTime > attackCooldown)
        {
            PerformAttack(target); // 🔑 single entry point for children
            lastAttackTime = Time.time;
        }
    }

    // 🔑 Override this in child classes to define attack behavior
    protected virtual void PerformAttack(Transform target)
    {
        Debug.Log($"{name} attacks {target.name} (default no-op).");
    }

    // ============ IHealth ============
    public Faction GetFaction() =>
        factionProvider != null ? factionProvider.Faction : Faction.Neutral;

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        OnDamaged?.Invoke(amount);

        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealed?.Invoke(amount);
    }

    public void ApplyDoT(float dps, float duration, float interval)
    {
        StartCoroutine(DoTDamage(dps, duration, interval));
    }

    private System.Collections.IEnumerator DoTDamage(float dps, float duration, float interval)
    {
        float elapsed = 0f;
        while (elapsed < duration && currentHealth > 0)
        {
            TakeDamage(dps);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
