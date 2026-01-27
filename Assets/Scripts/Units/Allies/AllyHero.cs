using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Abilities;
using Havengard.Combat;
using Havengard.Progression;
using Havengard.Items;

namespace Havengard.Units
{
    /// <summary>
    /// Hero ally unit with progression tracking (EXP and Items).
    /// Similar to player but with AI control.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(AbilityUser))]
    [RequireComponent(typeof(EXPSystem))]
    [RequireComponent(typeof(ItemInventory))]
    public class AllyHero : UnitBase
    {
        [Header("Ally Hero Settings")]
        [SerializeField] private AllyBehaviorMode behaviorMode = AllyBehaviorMode.Default;
        [SerializeField] private Transform playerTransform; // Set for Follow behavior
        [SerializeField] private float attackCooldown = 1f;

        [Header("Progression")]
        [SerializeField] private int[] expTable; // EXP required for each level

        private AllyBehavior currentBehavior;
        private AbilityUser abilityUser;
        private EXPSystem expSystem;
        private ItemInventory inventory;
        private float lastAttackTime;

        protected override void Awake()
        {
            base.Awake();

            // Cache components
            abilityUser = GetComponent<AbilityUser>();
            expSystem = GetComponent<EXPSystem>();
            inventory = GetComponent<ItemInventory>();

            // Initialize progression
            if (expSystem != null && expTable != null && expTable.Length > 0)
            {
                expSystem.InitEXPTable(expTable);
            }

            InitializeBehavior();
        }

        private void Start()
        {
            // Subscribe to level up events
            if (expSystem != null)
            {
                expSystem.OnLevelUp += OnLevelUp;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (expSystem != null)
            {
                expSystem.OnLevelUp -= OnLevelUp;
            }
        }

        private void InitializeBehavior()
        {
            // Remove any existing behavior
            if (currentBehavior != null)
            {
                Destroy(currentBehavior);
            }

            // Create appropriate behavior based on mode
            switch (behaviorMode)
            {
                case AllyBehaviorMode.Default:
                    currentBehavior = gameObject.AddComponent<DefaultBehavior>();
                    break;

                case AllyBehaviorMode.Stationary:
                    currentBehavior = gameObject.AddComponent<StationaryBehavior>();
                    break;

                case AllyBehaviorMode.Follow:
                    currentBehavior = gameObject.AddComponent<FollowBehavior>();
                    if (playerTransform == null)
                    {
                        // Try to find player automatically
                        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
                    }
                    currentBehavior.SetFollowTarget(playerTransform);
                    break;
            }

            currentBehavior.Initialize(this);
        }

        /// <summary>
        /// Change behavior at runtime.
        /// </summary>
        public void SetBehavior(AllyBehaviorMode newMode, Transform followTarget = null)
        {
            behaviorMode = newMode;
            if (followTarget != null)
            {
                playerTransform = followTarget;
            }
            InitializeBehavior();
        }

        protected override GameObject FindTarget()
        {
            // Check if behavior has custom targeting
            GameObject behaviorTarget = currentBehavior?.FindBehaviorTarget();
            if (behaviorTarget != null)
            {
                return behaviorTarget;
            }

            // Use default UnitBase targeting
            return base.FindTarget();
        }

        protected override void HandleMovementAndAttack()
        {
            if (agent == null) return;

            if (currentTarget == null)
            {
                // Let behavior handle what to do when idle
                currentBehavior?.OnNoTarget();
                return;
            }

            // Standard combat movement (same as UnitBase)
            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);

            if (dist > attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(currentTarget.transform.position);
            }
            else
            {
                agent.isStopped = true;
                PerformAttack(currentTarget);
            }
        }

        protected override void PerformAttack(GameObject target)
        {
            if (Time.time < lastAttackTime + attackCooldown || target == null) return;

            var h = target.GetComponent<IHealth>();
            if (h != null && FactionUtility.CanDamage(GetMyFaction(), h, false))
            {
                // Use equipped ability (similar to player)
                if (abilityUser != null)
                {
                    TriggerAttackAnim();
                    abilityUser.UseAbility(0, target);
                    lastAttackTime = Time.time;
                }
            }
        }

        // ------------- Progression Methods -------------

        /// <summary>
        /// Grant experience to this hero.
        /// </summary>
        public void GrantEXP(int amount)
        {
            if (expSystem != null)
            {
                expSystem.AddEXP(amount);
            }
        }

        /// <summary>
        /// Add item to hero's inventory.
        /// </summary>
        public bool AddItem(ItemInstance itemInstance)
        {
            if (inventory != null)
            {
                return inventory.TryAddItem(itemInstance);
            }
            return false;
        }

        /// <summary>
        /// Get current level.
        /// </summary>
        public int GetLevel()
        {
            return expSystem != null ? expSystem.CurrentLevel : 1;
        }

        /// <summary>
        /// Get current EXP.
        /// </summary>
        public int GetEXP()
        {
            return expSystem != null ? expSystem.CurrentExp : 0;
        }

        /// <summary>
        /// Called when hero levels up.
        /// </summary>
        private void OnLevelUp(int newLevel)
        {
            Debug.Log($"[AllyHero] {name} leveled up to {newLevel}!");
            // Add level up effects here (stat increases, heal, etc.)
        }

        // ------------- Public Accessors -------------

        public EXPSystem ExpSystem => expSystem;
        public ItemInventory Inventory => inventory;
        public AbilityUser AbilityUser => abilityUser;
    }
}
