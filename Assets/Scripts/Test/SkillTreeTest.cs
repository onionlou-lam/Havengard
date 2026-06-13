using UnityEngine;
using Havengard.Core.Progression;
using Havengard.Abilities;
using Havengard.Core.Heroes;

namespace Havengard.Test
{
    /// <summary>
    /// Test script to verify skill tree backend functionality
    /// Attach to a hero GameObject for testing
    /// </summary>
    public class SkillTreeTest : MonoBehaviour
    {
        [Header("Test Keys")]
        [SerializeField] private KeyCode grantExpKey = KeyCode.E;
        [SerializeField] private KeyCode unlockAbilityKey = KeyCode.U;
        [SerializeField] private KeyCode showStatusKey = KeyCode.I;

        [Header("Test Settings")]
        [SerializeField] private int expToGrant = 100;
        [SerializeField] private int abilityIndexToUnlock = 0;

        private EXPSystem expSystem;
        private AbilityUser abilityUser;
        private HeroInstance heroInstance;

        private void Start()
        {
            // Get components
            expSystem = GetComponent<EXPSystem>();
            abilityUser = GetComponent<AbilityUser>();
            heroInstance = GetComponent<HeroInstance>();

            if (expSystem == null)
            {
                Debug.LogError("[SkillTreeTest] No EXPSystem found!");
                return;
            }

            if (abilityUser == null)
            {
                Debug.LogError("[SkillTreeTest] No AbilityUser found!");
                return;
            }

            if (heroInstance == null)
            {
                Debug.LogError("[SkillTreeTest] No HeroInstance found!");
                return;
            }

            // Initialize unlock tracking
            var heroClass = heroInstance.Class;
            if (heroClass != null && heroClass.classAbilities != null)
            {
                abilityUser.InitializeUnlockTracking(heroClass.classAbilities.Length);
                Debug.Log($"[SkillTreeTest] Initialized unlock tracking for {heroClass.classAbilities.Length} abilities");
            }

            Debug.Log($"[SkillTreeTest] Ready! Press {grantExpKey} to gain EXP, {unlockAbilityKey} to unlock ability, {showStatusKey} for status");
        }

        private void Update()
        {
            if (expSystem == null || abilityUser == null) return;

            // Grant EXP (to level up and gain skill points)
            if (Input.GetKeyDown(grantExpKey))
            {
                Debug.Log($"=== GRANTING {expToGrant} EXP ===");
                expSystem.AddEXP(expToGrant);
                ShowStatus();
            }

            // Try to unlock an ability
            if (Input.GetKeyDown(unlockAbilityKey))
            {
                Debug.Log($"=== ATTEMPTING TO UNLOCK ABILITY {abilityIndexToUnlock} ===");
                TryUnlockAbility(abilityIndexToUnlock);
                ShowStatus();
            }

            // Show current status
            if (Input.GetKeyDown(showStatusKey))
            {
                ShowStatus();
            }
        }

        private void TryUnlockAbility(int index)
        {
            var heroClass = heroInstance.Class;
            if (heroClass == null || heroClass.classAbilities == null)
            {
                Debug.LogError("[SkillTreeTest] No class abilities configured!");
                return;
            }

            if (index < 0 || index >= heroClass.classAbilities.Length)
            {
                Debug.LogError($"[SkillTreeTest] Invalid ability index: {index}");
                return;
            }

            var classAbility = heroClass.classAbilities[index];

            // Check if already unlocked
            if (abilityUser.IsAbilityUnlocked(index))
            {
                Debug.LogWarning($"[SkillTreeTest] Ability {index} ({classAbility.ability.abilityName}) already unlocked!");
                return;
            }

            // Check level requirement
            if (expSystem.CurrentLevel < classAbility.requiredLevel)
            {
                Debug.LogWarning($"[SkillTreeTest] Level {expSystem.CurrentLevel} < {classAbility.requiredLevel} required!");
                return;
            }

            // Check prerequisites
            bool[] unlocked = abilityUser.GetUnlockedAbilities();
            if (!classAbility.ArePrerequisitesMet(unlocked))
            {
                Debug.LogWarning($"[SkillTreeTest] Prerequisites not met for ability {index}!");
                return;
            }

            // Check skill points
            if (expSystem.AvailableSkillPoints < classAbility.skillPointCost)
            {
                Debug.LogWarning($"[SkillTreeTest] Not enough skill points! Have {expSystem.AvailableSkillPoints}, need {classAbility.skillPointCost}");
                return;
            }

            // All checks passed - unlock!
            if (expSystem.TrySpendSkillPoints(classAbility.skillPointCost))
            {
                abilityUser.UnlockAbility(index, classAbility.ability);
                Debug.Log($"<color=green>[SkillTreeTest] ✓ UNLOCKED: {classAbility.ability.abilityName}!</color>");
            }
        }

        private void ShowStatus()
        {
            Debug.Log("==================== SKILL TREE STATUS ====================");
            Debug.Log($"Level: {expSystem.CurrentLevel} | EXP: {expSystem.CurrentExp}/{expSystem.ExpToNextLevel}");
            Debug.Log($"Skill Points: {expSystem.AvailableSkillPoints} (Total: {expSystem.SkillPoints}, Spent: {expSystem.SpentSkillPoints})");
            
            var heroClass = heroInstance.Class;
            if (heroClass != null && heroClass.classAbilities != null)
            {
                Debug.Log($"Abilities ({heroClass.classAbilities.Length} total):");
                
                bool[] unlocked = abilityUser.GetUnlockedAbilities();
                for (int i = 0; i < heroClass.classAbilities.Length; i++)
                {
                    var classAbility = heroClass.classAbilities[i];
                    if (classAbility == null || classAbility.ability == null)
                    {
                        Debug.LogWarning($"  [{i}] NULL ABILITY!");
                        continue;
                    }
                    
                    bool isUnlocked = unlocked != null && i < unlocked.Length && unlocked[i];
                    string status = isUnlocked ? "<color=green>UNLOCKED</color>" : "<color=red>LOCKED</color>";
                    
                    Debug.Log($"  [{i}] {classAbility.ability.abilityName} - {status} (Lv {classAbility.requiredLevel}, {classAbility.skillPointCost} pts)");
                }
            }
            
            var abilities = abilityUser.GetAbilities();
            if (abilities != null)
            {
                Debug.Log($"Usable Abilities: {abilities.Count}");
                foreach (var ability in abilities)
                {
                    if (ability != null)
                    {
                        Debug.Log($"  - {ability.abilityName}");
                    }
                    else
                    {
                        Debug.LogWarning("  - NULL ABILITY in list!");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Usable Abilities: NULL");
            }
            
            Debug.Log("==========================================================");
        }

        /*private void OnGUI()
        {
            if (expSystem == null) return;

            // Draw on-screen instructions
            GUI.Box(new Rect(10, 150, 400, 120), "SKILL TREE TEST");

            float yPos = 175;
            GUI.Label(new Rect(20, yPos, 380, 20), $"{grantExpKey} - Grant {expToGrant} EXP");
            yPos += 20;
            GUI.Label(new Rect(20, yPos, 380, 20), $"{unlockAbilityKey} - Unlock Ability {abilityIndexToUnlock}");
            yPos += 20;
            GUI.Label(new Rect(20, yPos, 380, 20), $"{showStatusKey} - Show Status");
            yPos += 20;

            GUI.Label(new Rect(20, yPos, 380, 20), $"Level: {expSystem.CurrentLevel} | Skill Points: {expSystem.AvailableSkillPoints}");
        }
        */
    }
}