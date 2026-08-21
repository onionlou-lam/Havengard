# Havengard Debug Menu

## Overview
The Havengard Debug Menu is an in-game developer tool for testing and debugging game features during development. It provides quick access to common testing operations without needing to modify code or inspector values.

## Setup

### Quick Setup (Editor)
1. Go to **Havengard > Create Debug Menu** in the Unity menu
2. This will automatically create a canvas with all sections pre-configured
3. Manually wire up input fields and dropdowns in the inspector as needed
4. Add enemy/boss prefabs to the prefab lists
5. Press **Play** and use **F1** to toggle the menu

### Manual Setup
1. Add `HavengardDebugMenu` component to a canvas in your scene
2. Configure the canvas to be Screen Space - Overlay with high sorting order (9999)
3. Create UI elements and wire them up in the inspector
4. Add prefab references for enemies and bosses

## Usage

### Keyboard Shortcut
- **F1**: Toggle debug menu visibility (configurable in inspector)

### Player Section
- **Add Gold**: Adds gold (default: 100, configurable via input field)
- **Add Celestium**: Adds celestium (default: 100, configurable)
- **Add XP**: Adds experience points (default: 100, configurable)
- **Add Skill Points**: Directly adds skill points (default: 1, configurable)
- **Level Up**: Levels up the player (default: 1 level, configurable)
- **Reset Skills**: Refunds all spent skill points and clears learned abilities

### Combat Section
- **Spawn Enemy Unit**: Spawns selected enemy from dropdown
- **Spawn Boss Unit**: Spawns selected boss from dropdown
- **Kill All**: Instantly kills all enemies on screen
- **Start Wave**: Manually starts the next wave
- **Skip Wave**: Kills all enemies to complete the current wave

### Abilities Section
- **Reset Cooldowns**: Instantly resets all ability cooldowns
- **Infinite Mana**: Toggle infinite resource (mana/energy)
- **Test Ability**: Casts selected ability slot
- **Toggle Damage Numbers**: Show/hide damage number popups

### World Section
- **Change Area/Scene**: Load different scene from dropdown
- **Game Speed**: Slider to control Time.timeScale (0.1x to 4x)
- **Toggle NavMesh**: Enable/disable NavMesh debugging visualization
- **Show AI Paths**: Display AI pathfinding paths

### Reset Test State Section
- **Reset Player**: Restores player health, resource, and position
- **Reset Skills**: Clears all learned abilities and refunds skill points
- **Reset Inventory**: Removes all items from inventory
- **Reset Everything**: Full reset (player, skills, inventory, currencies, level)

## Architecture Integration

The debug menu integrates with Havengard's existing architecture:

### Systems Used
- **GameManager**: Accesses `GoldSystem` and `CelestiumSystem`
- **HeroInstance**: For player stats, XP, and abilities
- **EXPSystem**: For leveling and skill points
- **AbilityUser**: For ability testing and cooldown management
- **WaveManager**: For wave control
- **UnitSpawner**: For enemy spawning
- **ItemInventory**: For inventory management
- **SceneTransitionManager**: For scene loading

### Extension Points
You can easily extend the menu by:
1. Adding new public methods to `HavengardDebugMenu`
2. Creating new UI buttons/controls
3. Wiring them up in the inspector
4. Following the existing pattern in the code

## Recommendations

### Additional Features to Consider
- **Save/Load State**: Quick save/load buttons for testing specific scenarios
- **Teleport**: Predefined waypoints for quick navigation
- **Time Control**: Fast-forward time of day
- **Spawn Item**: Dropdown to spawn specific items
- **Weather Control**: If you add weather systems
- **Quest Testing**: Start/complete specific quests
- **Achievement Testing**: Unlock achievements for testing
- **Performance Metrics**: FPS counter, memory usage display

### Best Practices
1. **Keep the menu disabled in builds**: Use preprocessor directives or build configurations
2. **Don't commit custom settings**: Add the debug menu GameObject to `.gitignore` scene exclusions
3. **Use consistent naming**: Follow the existing pattern for new features
4. **Test thoroughly**: Ensure debug commands don't break game state
5. **Document new features**: Update this README when adding functionality

## Build Configuration

### Disable in Release Builds
Add this to your build script or use conditional compilation:
#if UNITY_EDITOR || DEVELOPMENT_BUILD // Debug menu enabled #else // Destroy debug menu in release builds Destroy(debugMenuObject); #endif
### Keyboard Override
Consider adding a secret key combination for QA testing in release builds:
// Ctrl + Shift + F1 to enable in release if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F1)) { EnableDebugMenu(); }

## Troubleshooting

### Menu doesn't appear
- Check canvas is enabled and has correct render mode
- Verify F1 key isn't captured by another system
- Ensure `debugCanvas` reference is set in inspector

### Functions don't work
- Verify all manager references are cached in `Start()`
- Check that systems exist in the scene (GameManager, etc.)
- Look for null reference errors in console

### Spawning doesn't work
- Add enemy/boss prefabs to the inspector lists
- Ensure UnitSpawner exists or can be created
- Check spawn position is valid

## Future Enhancements
- **Console Window**: Built-in console for viewing debug logs
- **Command System**: Text-based command input (e.g., "spawn goblin 5")
- **Profiles**: Save/load different debug configurations
- **Hotkeys**: Bind functions to F-keys for quick access
- **Visual Gizmos**: Show spawn points, AI targets, ranges, etc.
- **Network Testing**: If multiplayer is added, test network conditions
