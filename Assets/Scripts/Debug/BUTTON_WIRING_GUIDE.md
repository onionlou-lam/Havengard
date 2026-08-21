# Button Wiring Guide - Step by Step

## Understanding the Problem

Your buttons aren't working because the function dropdown is showing "No Function". You need to select the specific function for each button.

## How to Wire Up Buttons (Step-by-Step)

### Example: "Add Gold" Button

1. **Select the Button** in the Hierarchy
   - Find: `HavengardDebugMenu > MenuPanel > ScrollView > Content > Btn_AddGold`

2. **Look at the Inspector** - Find the `Button` component

3. **Under "On Click ()"**:
   - You should see your `HavengardDebugMenu` already assigned in the object field ✓
   - Next to it is a dropdown that says "No Function"

4. **Click the "No Function" dropdown**:
   - A menu will appear
   - Select: `HavengardDebugMenu` (the class name)
   - Then select: `AddGold ()` (the method name)

5. **It should now show**: `HavengardDebugMenu.AddGold`

### Complete Button Mapping

Wire each button using the pattern above:

#### PLAYER SECTION
- `Btn_AddGold` → `HavengardDebugMenu.AddGold()`
- `Btn_AddCelestium` → `HavengardDebugMenu.AddCelestium()`
- `Btn_AddXP` → `HavengardDebugMenu.AddXP()`
- `Btn_AddSkillPoints` → `HavengardDebugMenu.AddSkillPoints()`
- `Btn_LevelUp` → `HavengardDebugMenu.LevelUp()`
- `Btn_ResetSkills` → `HavengardDebugMenu.ResetSkills()`

#### COMBAT SECTION
- `Btn_SpawnEnemyUnit` → `HavengardDebugMenu.SpawnEnemyUnit()`
- `Btn_SpawnBossUnit` → `HavengardDebugMenu.SpawnBossUnit()`
- `Btn_KillAll` → `HavengardDebugMenu.KillAllEnemies()`
- `Btn_StartWave` → `HavengardDebugMenu.StartWave()`
- `Btn_SkipWave` → `HavengardDebugMenu.SkipWave()`

#### ABILITIES SECTION
- `Btn_ResetCooldowns` → `HavengardDebugMenu.ResetCooldowns()`
- `Btn_TestAbility` → `HavengardDebugMenu.TestAbility()`

#### WORLD SECTION
- `Btn_ChangeArea/Scene` → `HavengardDebugMenu.ChangeScene()`

#### RESET SECTION
- `Btn_ResetPlayer` → `HavengardDebugMenu.ResetPlayer()`
- `Btn_ResetSkills` → `HavengardDebugMenu.ResetSkills()` (duplicate, shares with Player section)
- `Btn_ResetInventory` → `HavengardDebugMenu.ResetInventory()`
- `Btn_ResetEverything` → `HavengardDebugMenu.ResetEverything()`

## Input Fields - Where Should They Be?

Input fields should be **ALWAYS VISIBLE** next to their corresponding buttons. They are NOT popups.

### Layout Structure (Example for Player Section):
Header_PLAYER ├── InputField_Gold (shows "100") ├── Btn_AddGold ├── InputField_Celestium (shows "100") ├── Btn_AddCelestium ├── InputField_XP (shows "100") ├── Btn_AddXP ├── InputField_SkillPoints (shows "1") ├── Btn_AddSkillPoints ├── InputField_LevelUp (shows "1") └── Btn_LevelUp


### Creating Input Fields

If you don't have input fields yet:

1. **Right-click in Content** → UI → Input Field
2. **Rename it**: `InputField_Gold`
3. **Position it** ABOVE the `Btn_AddGold` button
4. **Set default value**: In the InputField component, set "Text" to "100"
5. **Drag the InputField** into the `HavengardDebugMenu` component:
   - Inspector → HavengardDebugMenu → Player Section → Gold Amount Input

### Input Field References to Wire Up

In the `HavengardDebugMenu` Inspector, drag these input fields:

**Player Section:**
- `goldAmountInput` ← InputField_Gold
- `celestiumAmountInput` ← InputField_Celestium
- `xpAmountInput` ← InputField_XP
- `skillPointsAmountInput` ← InputField_SkillPoints
- `levelUpAmountInput` ← InputField_LevelUp

## Testing Checklist

After wiring everything up:

1. ✅ **Canvas GameObject is ACTIVE** in hierarchy (checkbox checked)
2. ✅ **MenuPanel is INACTIVE** by default (checkbox unchecked)
3. ✅ **All buttons** have `HavengardDebugMenu.MethodName()` in On Click()
4. ✅ **All input fields** are dragged into the Inspector references
5. ✅ **Press Play** → Press F1 → Menu should appear
6. ✅ **Click a button** → Should see debug log: `[DebugMenu] MethodName() called!`

## Quick Test

1. **Press Play**
2. **Press F1** - Menu should open
3. **Click "Add Gold"** - Console should show: [DebugMenu] AddGold() called! [DebugMenu] Added 100 gold (Total: XXX)

If you see the "called!" message but nothing happens, check:
- GameManager exists in scene
- GoldSystem is assigned to GameManager

## Visual Reference

Your button inspector should look like this:
┌─ Button ────────────────────────────┐ │ Interactable: ✓                     │ │ Transition: Color Tint              │ │ ...                                  │ │                                      │ │ 
On Click ()                          │ │ ┌────────────────────────────────┐  │ │ │ Runtime Only ▼                 │  │ │ │ HavengardDebugMenu  (circle)   │  │ ← Object 
assigned │ │ HavengardDebugMenu.AddGold     │  │ ← Function selected │ └────────────────────────────────┘  │ 
└──────────────────────────────────────┘

If it shows "No Function", click the dropdown and navigate:
`HavengardDebugMenu` → `AddGold ()`