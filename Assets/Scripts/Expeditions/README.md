# Expedition System - Setup Guide

## Overview

The Expedition System allows players to send followers/heroes on expeditions to dungeon locations shown on a world map. Expeditions are simulated rather than fully playable, and progress is tied to completed game waves.

---

## Quick Setup Checklist

1. ✅ Create ExpeditionManager GameObject
2. ✅ Create Expedition Map Canvas
3. ✅ Create ExpeditionData ScriptableObjects
4. ✅ Add ExpeditionTableInteractable to scene
5. ✅ Configure UI prefabs
6. ✅ Test expedition flow

---

## 1. ExpeditionManager Setup

### Create GameObject
1. Create empty GameObject: `ExpeditionManager`
2. Add component: `ExpeditionManager`
3. Configure settings:
   - **Max Active Expedition Followers**: `10` (adjustable)
   - **Wave Manager**: Drag in your scene's WaveManager

### Important
- Only one ExpeditionManager should exist (it's a singleton)
- It will auto-find WaveManager if not assigned
- It subscribes to wave completion events automatically

---

## 2. Expedition Map Canvas Setup

### Create Canvas
1. Create Canvas: `ExpeditionMapCanvas`
2. Set **Render Mode**: Screen Space - Overlay
3. Set **Canvas Scaler**: Scale With Screen Size

### Create Map Structure
ExpeditionMapCanvas ├── MapPanel (holds all map UI) │   ├── WorldMapBackground (Image) │   ├── DungeonIconsContainer (holds dungeon icons) │   │   ├── DungeonIcon_ForestRuins │   │   ├── DungeonIcon_DarkCave │   │   └── DungeonIcon_AbandonedFort │   ├── FollowerSidePanel │   │   ├── FollowerScrollView │   │   │   └── FollowerListContainer (Vertical Layout Group) │   │   └── Header (TextMeshPro) │   └── CloseButton ├── ExpeditionSetupPanel (initially inactive) │   ├── DungeonInfoPanel │   │   ├── DungeonName (TextMeshPro) │   │   ├── Description (TextMeshPro) │   │   ├── RecommendedLevel (TextMeshPro) │   │   ├── RequiredLevel (TextMeshPro) │   │   ├── PartySize (TextMeshPro) │   │   ├── Duration (TextMeshPro) │   │   └── MissionType (TextMeshPro) │   ├── PartyPanel │   │   ├── SelectedFollowersText (TextMeshPro) │   │   └── PortraitContainer (Horizontal Layout Group) │   └── ButtonPanel │       ├── StartExpeditionButton │       └── CloseButton


### Add Components

**On MapPanel:**
- Add: `ExpeditionMapController`
- Assign references:
  - Expedition Map Canvas: The canvas itself
  - Map Panel: This panel
  - Setup Panel: The ExpeditionSetupPanel
  - Follower List UI: FollowerListUI component (see below)

**On FollowerScrollView/FollowerListContainer:**
- Add: `FollowerListUI`
- Assign references:
  - Follower Entry Container: The container with Vertical Layout Group
  - Follower Entry Prefab: Your follower entry prefab (see below)

**On ExpeditionSetupPanel:**
- Add: `ExpeditionSetupPanel`
- Assign all text and button references

**On each DungeonIcon:**
- Add: `DungeonIconUI`
- Assign:
  - Expedition Data: The ScriptableObject for this dungeon
  - Icon Image: The Image component
  - Active Indicator: A GameObject shown when expedition is active
  - Progress Fill: An Image with Fill type for progress bar
  - Progress Text: TextMeshPro for "Day X/Y"

---

## 3. Create Follower Entry Prefab

1. Create UI GameObject: `FollowerEntryPrefab`
2. Structure:
1. FollowerEntryPrefab ├── Background (Image) ├── Portrait (Image) ├── InfoPanel │   ├── NameText (TextMeshPro) │   ├── LevelText (TextMeshPro) │   ├── ClassText (TextMeshPro) │   └── StatusText (TextMeshPro) └── SelectButton (Button)
1. 
3. Add component: `FollowerEntryUI`
4. Assign all references
5. Save as prefab

---

## 4. Create ExpeditionData Assets

### Create ScriptableObjects
1. Right-click in Project: `Create > Havengard > Expeditions > Expedition Data`
2. Configure each expedition:

**Example: Forest Ruins**
- **ID**: `forest_ruins`
- **Display Name**: `Forest Ruins`
- **Description**: `Ancient ruins overrun by hostile creatures. Scout the area.`
- **Map Icon**: Assign sprite
- **Recommended Level**: `3`
- **Has Required Level**: `false`
- **Recommended Party Size**: `3`
- **Maximum Party Size**: `5`
- **Duration In Days**: `2`
- **Mission Type**: `Scouting`
- **Is Main Story Mission**: `false`
- **Is Available**: `true`
- **Is Unlocked**: `true`

Create multiple ExpeditionData assets for different dungeons.

---

## 5. Create Expedition Table Interactable

### Scene Setup
1. Find/create your **Expedition Table** GameObject in the scene
2. Ensure it has a `Collider2D` (e.g., BoxCollider2D, CircleCollider2D)
3. Set the **Layer** to your Interactable layer (check InteractionManager settings)
4. Add component: `ExpeditionTableInteractable`
5. Configure:
   - **Prompt Text**: "Open Expedition Map"
   - **Key Hint**: "E"
   - **Map Controller**: Will auto-find, or drag ExpeditionMapController

### Test Interaction
- Play the game
- Walk near the table
- Press E to open the map

---

## 6. Testing the System

### Basic Flow Test
1. **Open Map**: Press `M` or interact with Expedition Table
2. **Select Dungeon**: Click a dungeon icon
3. **Assign Followers**: Click available followers in side panel
4. **Start Expedition**: Click "Start Expedition" button
5. **Close Map**: Press `M` or `Escape`
6. **Complete Waves**: Play and complete waves
7. **Check Progress**: Open map again to see expedition progress

### Expected Behavior
- Followers change to "On Expedition" status
- Cannot assign more than maximum party size
- Cannot assign wounded followers
- Cannot exceed global follower limit (10)
- Expedition advances 1 day per completed wave
- Followers return to "Available" when expedition completes

### Debug Logs
Check console for:
[ExpeditionManager] Started expedition: Forest Ruins [ExpeditionManager] Party size: 3/5 [ExpeditionManager] Wave 1 completed. Advancing expeditions... [ExpeditionManager] Forest Ruins: Day 1/2 [ExpeditionManager] Expedition completed: Forest Ruins

---

## 7. Inspector Configuration

### ExpeditionManager
- **Max Active Expedition Followers**: Start with `10`, can change to `20` later
- **Wave Manager**: Auto-assigned, but verify it's correct

### ExpeditionMapController
- **Toggle Map Key**: `M` (KeyCode.M)
- **Close Key**: `Escape` (KeyCode.Escape)

### ExpeditionData
- Create one asset per dungeon
- Configure all fields appropriately
- Test with different durations and party sizes

---

## 8. Integration with Existing Systems

### WaveManager Integration
The system automatically subscribes to `WaveManager.waveEvents.OnWaveCleared`.

**If this doesn't work:**
1. Check that WaveManager has the updated code with public `waveEvents` property
2. Verify ExpeditionManager's Start() method runs before waves begin
3. Check console for subscription confirmation

### HeroInstance Integration
The system reuses existing quest tracking methods:
- `StartQuest(int days)` - Marks hero as busy
- `ProgressQuestDay()` - Not used (managed by ExpeditionManager)
- `CompleteQuest()` - Returns hero to available
- `IsOnQuest` property - Checked for availability

### QuestSystem Integration
**Future:** Subscribe to `ExpeditionManager.OnMainStoryExpeditionCompleted` in QuestSystem to trigger quest progression.

---

## 9. Future Expansion Points

### Follower Effects
Add effects to heroes by creating a new component:
sharp public class FollowerExpeditionEffects : MonoBehaviour { public float goldMultiplier = 1f; public float durationReduction = 0f; // etc. }

Then modify `ExpeditionManager.EvaluateFollowerEffects()`.

### Dungeon Effects
Add effects to ExpeditionData by uncommenting:
public ExpeditionEffect[] dungeonEffects;

Then implement `ExpeditionManager.EvaluateDungeonEffects()`.

### Rewards
Modify `ExpeditionManager.ProcessRewards()` to:
- Grant gold/celestium from ExpeditionData
- Generate items based on dungeon type
- Award experience to participating heroes

### Success/Failure
Implement success chance calculations in `CompleteExpedition()`:
- Calculate success based on party level, composition
- Apply dungeon/follower modifiers
- Create different outcomes based on success

---

## 10. Common Issues & Solutions

### Map doesn't open
- Check that Canvas is enabled
- Verify ExpeditionMapController is on the scene
- Check console for errors

### Followers not showing
- Call `ExpeditionManager.Instance.RefreshFollowerList()` in Start
- Ensure heroes have `HeroInstance` component
- Verify heroes are marked as Faction.Ally

### Expeditions not progressing
- Check WaveManager integration
- Verify wave completion events fire
- Check console for "[ExpeditionManager] Wave X completed"

### Can't start expedition
- Check validation messages in Start button text
- Verify followers are Available
- Check global follower limit

### Followers stuck "On Expedition"
- Check that wave completion triggers progression
- Manually call `hero.CompleteQuest()` to reset
- Verify expedition completes when days reach duration

---

## 11. Customization

### Change Global Limit

// In ExpeditionManager Inspector Max Active Expedition Followers: 20

### Add New Mission Type

### Customize Level Validation
Override in ExpeditionManager:

protected override bool ValidatePartyLevel(List<HeroInstance> followers, int requiredLevel) { // Example: Require average level float avgLevel = followers.Average(f => f.ExpSystem.Level); return avgLevel >= requiredLevel; }

---

## Summary

The Expedition System is now fully implemented and modular. All core functionality works, with clean extension points for future features like:
- Follower-specific effects
- Dungeon-specific effects
- Success/failure calculations
- Reward generation
- Quest integration

The system integrates cleanly with existing WaveManager and HeroInstance systems without breaking existing functionality.