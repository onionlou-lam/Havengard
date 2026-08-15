using Havengard.Abilities;
using Havengard.Combat;
using Havengard.Core.Stats;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityBase : ScriptableObject
{
    [Header("Basic Info")]
    public string abilityName = "New Ability";
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public int maxLevel = 5;

    [Header("Damage Configuration")]
    public DamageType damageType = DamageType.Physical;
    [Tooltip("If true, this ability can heal allies when damageType is Holy")]
    public bool canHeal = false;
    [Range(0f, 1f)]
    [Tooltip("For Holy damage: percentage that heals instead of damages (0 = pure damage, 1 = pure heal)")]
    public float healingRatio = 0f;

    [Header("Cooldown & Resource")]
    public float baseCooldown = 1f;
    public int resourceCost = 10;

    [Header("Casting Behavior")]
    [Tooltip("If true, ability can be held down to cast repeatedly on cooldown")]
    public bool canHoldToCast = false;
    [Tooltip("If true, uses attack speed stat to modify cooldown")]
    public bool scalesWithAttackSpeed = false;

    [Header("Damage & Effects")]
    public float baseDamage = 10f;
    public float damagePerLevel = 5f;

    [Header("Lifesteal")]
    [Range(0f, 1f)]
    public float lifestealPercent = 0f;

    [Header("Resource Generation")]
    [Tooltip("Enable resource generation on hit/damage")]
    public bool enableResourceGeneration = false;
    [Range(0f, 1f)]
    [Tooltip("Percentage of damage dealt that is converted to resource (0.0 - 1.0)")]
    public float resourceGenerationPercent = 0f;
    [Tooltip("Flat amount of resource generated (added to percentage-based generation)")]
    public int flatResourceGeneration = 0;

    [Header("Targeting")]
    public float range = 10f;
    public LayerMask targetLayers;
    public bool requiresTarget = true;

    [Header("Visual & Audio")]
    public GameObject castVFX;
    public GameObject impactVFX;
    public AudioClip castSFX;
    public AudioClip impactSFX;

    [Header("Animation")]
    [Tooltip("Optional: Custom animation trigger name. Leave empty to use default 'Attack' animation")]
    public string customAnimationTrigger = "";

    [Header("Sub-Skills / Modifiers (Future)")]
    [Tooltip("Active sub-skills that modify this ability's behavior")]
    public List<AbilitySubSkill> activeSubSkills = new List<AbilitySubSkill>();

    [Header("Investment System")]
    [Tooltip("Configure how this ability scales with skill point investments")]
    public AbilityInvestment investment = new AbilityInvestment();

    [Header("Crit Stats")]
    [Range(0f, 1f)]
    public float baseCritChance = 0.05f;
    public float baseCritDamage = 1.5f; // 150% = 1.5x damage

    protected int currentLevel = 1;

    public int CurrentLevel
    {
        get => currentLevel;
        set => currentLevel = Mathf.Clamp(value, 1, maxLevel);
    }

    /// <summary>
    /// Calculate final damage with level scaling and damage type bonuses
    /// </summary>
    public virtual float CalculateDamage(GameObject caster = null, int level = -1, bool isCrit = false)
    {
        if (level < 0) level = currentLevel;

        float damage = baseDamage + (damagePerLevel * (level - 1));

        // ✅ Apply investment bonuses
        float investmentBonus = investment.GetTotalModifier(StatModifier.StatType.Damage);
        damage += investmentBonus;

        // Apply damage type bonus from PlayerStatAllocator
        if (caster != null)
        {
            var statAllocator = caster.GetComponent<Havengard.Stats.PlayerStatAllocator>();
            if (statAllocator != null)
            {
                float bonus = statAllocator.GetDamageTypeBonus(damageType);
                damage *= (1f + bonus);
            }

            // Apply damage multiplier from stats
            var statsComponent = caster.GetComponent<Havengard.Core.Character.StatsComponent>();
            if (statsComponent != null && statsComponent.CurrentStats != null)
            {
                damage *= (1f + statsComponent.CurrentStats.DamageMultiplier);
            }
        }

        // Apply sub-skill modifiers
        foreach (var subSkill in activeSubSkills)
        {
            if (subSkill != null && subSkill.modifiesDamage)
            {
                damage *= subSkill.damageMultiplier;
            }
        }

        // ✅ Apply crit
        if (isCrit)
        {
            float critDamage = GetEffectiveCritDamage(caster);
            damage *= critDamage;
        }

        return damage;
    }

    /// <summary>
    /// Calculate healing amount (for Holy abilities)
    /// </summary>
    public float CalculateHealing(GameObject caster, int level = -1)
    {
        if (damageType != DamageType.Holy || !canHeal)
            return 0f;

        float baseDamageValue = CalculateDamage(caster, level);
        return baseDamageValue * healingRatio;
    }

    /// <summary>
    /// Calculate resource generation amount based on damage dealt
    /// </summary>
    public int CalculateResourceGeneration(int damageDealt)
    {
        if (!enableResourceGeneration)
            return 0;

        int percentBased = Mathf.RoundToInt(damageDealt * resourceGenerationPercent);
        return percentBased + flatResourceGeneration;
    }

    /// <summary>
    /// Get effective cooldown with modifiers applied
    /// </summary>
    public virtual float GetEffectiveCooldown(GameObject caster = null)
    {
        float cooldown = baseCooldown;

        // ✅ Apply investment bonus
        float investmentReduction = investment.GetTotalModifier(StatModifier.StatType.Cooldown);
        cooldown *= (1f - investmentReduction);

        // Apply attack speed scaling if enabled
        if (scalesWithAttackSpeed && caster != null)
        {
            var statsComponent = caster.GetComponent<Havengard.Core.Character.StatsComponent>();
            if (statsComponent != null && statsComponent.CurrentStats != null)
            {
                float attackSpeed = statsComponent.CurrentStats.AttackSpeed;
                if (attackSpeed > 0)
                {
                    cooldown /= attackSpeed; // Higher attack speed = lower cooldown
                }
            }
        }

        // Apply cooldown reduction from stats
        if (caster != null)
        {
            var statsComponent = caster.GetComponent<Havengard.Core.Character.StatsComponent>();
            if (statsComponent != null && statsComponent.CurrentStats != null)
            {
                cooldown *= (1f - statsComponent.CurrentStats.CooldownReduction);
            }
        }

        // Apply sub-skill cooldown reduction
        foreach (var subSkill in activeSubSkills)
        {
            if (subSkill != null && subSkill.modifiesCooldown)
            {
                cooldown *= (1f - subSkill.cooldownReduction);
            }
        }

        return Mathf.Max(0.1f, cooldown); // Minimum 0.1s cooldown
    }

    /// <summary>
    /// Get effective resource cost with modifiers applied
    /// </summary>
    public virtual int GetEffectiveResourceCost()
    {
        float cost = resourceCost;

        // Apply sub-skill cost reduction
        foreach (var subSkill in activeSubSkills)
        {
            if (subSkill != null && subSkill.modifiesCost)
            {
                cost *= (1f - subSkill.costReduction);
            }
        }

        return Mathf.Max(0, Mathf.RoundToInt(cost));
    }

    /// <summary>
    /// Get the animation trigger name for this ability
    /// </summary>
    public string GetAnimationTrigger()
    {
        return string.IsNullOrEmpty(customAnimationTrigger) ? null : customAnimationTrigger;
    }

    private Color GetDamageTypeColor()
    {
        return damageType switch
        {
            DamageType.Fire => new Color(1f, 0.3f, 0f),
            DamageType.Frost => new Color(0.3f, 0.8f, 1f),
            DamageType.Lightning => new Color(1f, 1f, 0.3f),
            DamageType.Holy => new Color(1f, 0.9f, 0.3f),
            DamageType.Physical => new Color(0.8f, 0.8f, 0.8f),
            DamageType.Arcane => new Color(0.7f, 0.3f, 1f),
            _ => Color.white
        };
    }

    /// <summary>
    /// Setup method to assign a sub-skill modifier to this ability (for sub-skill abilities)
    /// </summary>
    public void AssignSubSkillModifier(AbilitySubSkill modifier)
    {
        if (modifier != null && !activeSubSkills.Contains(modifier))
        {
            activeSubSkills.Add(modifier);
        }
    }

    /// <summary>
    /// Calculate crit chance with investments
    /// </summary>
    public virtual float GetEffectiveCritChance(GameObject caster = null)
    {
        float critChance = baseCritChance;

        // ✅ Add investment bonus
        critChance += investment.GetTotalModifier(StatModifier.StatType.CritChance);

        // Add crit from caster stats
        if (caster != null)
        {
            var statsComponent = caster.GetComponent<Havengard.Core.Character.StatsComponent>();
            if (statsComponent != null && statsComponent.CurrentStats != null)
            {
                critChance += statsComponent.CurrentStats.CritChance;
            }
        }

        return Mathf.Clamp01(critChance);
    }

    /// <summary>
    /// Calculate crit damage multiplier with investments
    /// </summary>
    public virtual float GetEffectiveCritDamage(GameObject caster = null)
    {
        float critDamage = baseCritDamage;

        // ✅ Add investment bonus
        critDamage += investment.GetTotalModifier(StatModifier.StatType.CritDamage);

        // Add crit damage from caster stats
        if (caster != null)
        {
            var statsComponent = caster.GetComponent<Havengard.Core.Character.StatsComponent>();
            if (statsComponent != null && statsComponent.CurrentStats != null)
            {
                // ✅ FIX: Use CritMultiplier instead of CritDamageMultiplier
                critDamage += statsComponent.CurrentStats.CritMultiplier - 1f; // Subtract 1 since base is already 1.5x
            }
            else
            {
                // Try HeroStats if StatsComponent doesn't exist
                var heroInstance = caster.GetComponent<Havengard.Core.Heroes.HeroInstance>();
                var heroStats = heroInstance?.GetStats();
                if (heroStats != null)
                {
                    critDamage += heroStats.CritMultiplier - 1f;
                }
            }
        }

        return critDamage;
    }

    /// <summary>
    /// Roll for crit
    /// </summary>
    public bool RollCrit(GameObject caster = null)
    {
        float critChance = GetEffectiveCritChance(caster);
        return UnityEngine.Random.value < critChance;
    }

    public abstract void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy);
    public abstract void Deactivate(AbilityUser user);
}