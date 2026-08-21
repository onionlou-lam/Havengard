# Debug Menu Setup Guide

## Quick Setup Steps

### 1. Create the Debug Menu GameObject
1. In Unity, go to **Havengard > Create Debug Menu** (menu bar)
2. This will create a GameObject called `HavengardDebugMenu` in your scene
3. The canvas will be created with all the necessary UI elements

### 2. Configure the Debug Menu Component
1. Select the `HavengardDebugMenu` GameObject in the hierarchy
2. In the Inspector, find the `HavengardDebugMenu` component
3. Make sure these references are set:
   - **Debug Canvas**: Should auto-populate with the Canvas component
   - **Menu Panel**: Should auto-populate with the MenuPanel child object

### 3. Add Enemy/Boss Prefabs
1. In the Inspector under **Prefab References**:
   - **Enemy Prefabs**: Drag your enemy unit prefabs here
   - **Boss Prefabs**: Drag your boss unit prefabs here
2. These will populate the spawn dropdowns

### 4. Set Spawn Point (Optional)
1. Create an empty GameObject in your scene called "DebugSpawnPoint"
2. Position it where you want enemies to spawn
3. Drag it to the **Spawn Point** field in the debug menu component
4. If not set, enemies will spawn 5 units in front of the player

### 5. Make it Persistent (Optional)
If you want the debug menu to persist across scenes:
1. Right-click the `HavengardDebugMenu` GameObject
2. Select **Don't Destroy On Load** (you may need to add this script yourself)

## Testing

1. Press **Play** in Unity
2. Press **F1** to toggle the debug menu
3. You should see the menu appear/disappear
4. Check the Console for this message: `[HavengardDebugMenu] Initialized. Press F1 to toggle menu.`

## Troubleshooting

### Menu doesn't open when pressing F1
**Solution:** Make sure:
- The `HavengardDebugMenu` GameObject is active in the scene
- The script is enabled in the Inspector
- No errors in the Console
- Check if another script is capturing F1 input

### "References not found" warnings
**Solution:**
- **GoldSystem/CelestiumSystem**: Make sure `GameManager` exists in your scene with these components
- **Player Hero**: Make sure a GameObject with `HeroInstance` component exists
- **WaveManager**: Make sure your scene has a `WaveManager` (for combat testing)

### Buttons don't work
**Solution:**
- Make sure you wired up the button onClick events in the Inspector
- Each button should call the corresponding public method on `HavengardDebugMenu`
- Example: "Add Gold" button → `HavengardDebugMenu.AddGold()`

## Manual UI Wiring (If needed)

If you created the UI manually instead of using the menu command:

### Player Section Buttons
- **Add Gold** → `HavengardDebugMenu.AddGold()`
- **Add Celestium** → `HavengardDebugMenu.AddCelestium()`
- **Add XP** → `HavengardDebugMenu.AddXP()`
- **Add Skill Points** → `HavengardDebugMenu.AddSkillPoints()`
- **Level Up** → `HavengardDebugMenu.LevelUp()`
- **Reset Skills** → `HavengardDebugMenu.ResetSkills()`

### Combat Section Buttons
- **Spawn Enemy Unit** → `HavengardDebugMenu.SpawnEnemyUnit()`
- **Spawn Boss Unit** → `HavengardDebugMenu.SpawnBossUnit()`
- **Kill All** → `HavengardDebugMenu.KillAllEnemies()`
- **Start Wave** → `HavengardDebugMenu.StartWave()`
- **Skip Wave** → `HavengardDebugMenu.SkipWave()`

### Abilities Section Buttons
- **Reset Cooldowns** → `HavengardDebugMenu.ResetCooldowns()`
- **Test Ability** → `HavengardDebugMenu.TestAbility()`

### World Section Buttons
- **Change Area/Scene** → `HavengardDebugMenu.ChangeScene()`

### Reset Section Buttons
- **Reset Player** → `HavengardDebugMenu.ResetPlayer()`
- **Reset Skills** → `HavengardDebugMenu.ResetSkills()`
- **Reset Inventory** → `HavengardDebugMenu.ResetInventory()`
- **Reset Everything** → `HavengardDebugMenu.ResetEverything()`

## Input Field References
Wire these up in the Inspector under **Player Section**:
- **goldAmountInput**
- **celestiumAmountInput**
- **xpAmountInput**
- **skillPointsAmountInput**
- **levelUpAmountInput**

## Dropdown References
Wire these up in the Inspector:
- **enemyUnitDropdown** (Combat Section)
- **bossUnitDropdown** (Combat Section)
- **sceneDropdown** (World Section)
- **abilityTestDropdown** (Abilities Section)

## Slider References
- **gameSpeedSlider** (World Section)
- **gameSpeedText** (World Section)

## Toggle References
- **infiniteManaToggle** (Abilities Section)
- **damageNumbersToggle** (Abilities Section)
- **navMeshToggle** (Debug Visualization)
- **aiPathsToggle** (Debug Visualization)

## Notes

### Save System Conflict
- **F1 is now used for the Debug Menu**
- Saving is done via the Pause Menu (ESC → Save Game button)
- This matches the same functionality: `SaveManager.Instance.SaveGame()`

### Disabling in Builds
To disable the debug menu in release builds, add this to the top of `HavengardDebugMenu.cs`:
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD void Awake() { Destroy(gameObject); } #endif

Or use a build preprocessor to exclude the entire script from release builds.
