# Buff Ability System

A comprehensive buff ability system that supports both temporary (duration-based) and persistent (toggle) buffs with stat modifications.

## Features

- **Two Buff Types:**
  - **Duration**: Temporary buffs that expire after a set time
  - **Toggle**: Persistent buffs that remain active until toggled off

- **Stat Modification:**
  - Modify any player stat (HP, Attack, Defense, Speed, etc.)
  - Additive modifiers (e.g., +50 HP)
  - Multiplicative modifiers (e.g., x1.5 Attack = +50% Attack)

- **Visual & Audio Feedback:**
  - Activation VFX (one-time effect on cast)
  - Persistent VFX (follows caster while buff is active)
  - Deactivation VFX (plays when buff ends)
  - Activation and deactivation sound effects

- **Buff Management:**
  - Refresh duration by recasting (duration buffs)
  - Toggle on/off (toggle buffs)
  - Automatic cleanup on expiration
  - Multiple buffs can be active simultaneously

## Creating a Buff Ability

### 1. Create the Ability Asset

1. Right-click in Project window
2. Select `Create > Havengard > Abilities > Buff Ability`
3. Name it (e.g., "BerserkerRage", "IronSkin", "SwiftFeet")

### 2. Configure Buff Settings

**Buff Type:**
- **Duration**: Set duration in seconds (e.g., 5 seconds)
- **Toggle**: Remains active until cast again

**Can Refresh Or Toggle:**
- ✓ Enabled: Can recast to refresh duration or toggle off
- ✗ Disabled: Cannot recast while active

### 3. Add Stat Modifiers

Click `+` to add modifiers:

**Stat Type Options:**
- MaxHP
- Attack
- Defense
- MaxResource (Mana)
- AttackSpeed
- MoveSpeed
- CritChance
- CritMultiplier

**Modifier Type:**
- **Additive**: Adds flat value
  - Example: +50 HP, +10 Attack
- **Multiplicative**: Multiplies by percentage
  - Example: 1.5 = +50%, 2.0 = +100%

**Value Examples:**
- Additive HP: `50` = +50 HP
- Multiplicative Attack: `1.5` = +50% Attack
- Multiplicative Speed: `2.0` = +100% Speed (2x faster)

### 4. Assign Visual Effects

**Activation VFX**: Plays once when buff activates
**Persistent VFX**: Follows player while buff is active
**Deactivation VFX**: Plays when buff ends

Drag your particle effect prefabs into these slots.

### 5. Assign Audio

**Activation SFX**: Sound played when buff starts
**Deactivation SFX**: Sound played when buff ends

## Example Buff Configurations

### Berserker Rage (Duration Buff)

- **Buff Type**: Duration
- **Duration**: 10 seconds
- **Stat Modifiers**:
  - Additive Attack: +20
  - Multiplicative Crit Chance: 1.5 (50% more chance)
- **VFX**: Fire particles
- **SFX**: Loud battle cry

### Iron Skin (Toggle Buff)

- **Buff Type**: Toggle
- **Can Refresh Or Toggle**: ✓ Enabled
- **Stat Modifiers**:
  - Additive Defense: +15
- **VFX**: Shield outline
- **SFX**: Metallic shield sound

### Swift Feet (Duration Buff)

- **Buff Type**: Duration
- **Duration**: 7 seconds
- **Stat Modifiers**:
  - Multiplicative Move Speed: 1.3 (30% faster)
- **VFX**: Wind gusts
- **SFX**: Whooshing wind

## Adding to Player

1. Open your player prefab
2. Find the `AbilityUser` component
3. Add your buff ability to the abilities list
4. Assign to a keybind slot (Q, W, E, R, etc.)

## UI Integration

### Buff Duration Display (Optional)

To show active buffs with timers:

1. Create a UI canvas
2. Add `BuffIndicatorUI` component
3. Create a buff icon prefab with:
   - Image component (for buff icon)
   - TextMeshPro (for duration timer)
4. Assign references in inspector

The UI will automatically show all active buffs with countdown timers.

## How It Works

### Runtime Behavior

1. **On Cast:**
   - Checks if buff already active
   - Creates `BuffInstance` component on caster
   - Takes snapshot of original stats
   - Applies stat modifiers
   - Spawns VFX and plays SFX

2. **While Active:**
   - Persistent VFX follows caster
   - Duration countdown (if duration type)
   - Stats remain modified

3. **On End:**
   - Restores original stats
   - Destroys persistent VFX
   - Plays deactivation effects
   - Removes `BuffInstance` component

### Toggle Buffs

- Cast once: Activates
- Cast again: Deactivates
- No duration limit
- Perfect for stance abilities

### Duration Buffs

- Fixed duration timer
- Can refresh by recasting
- Automatically expires
- Perfect for temporary power-ups

## Advanced Tips

### Stacking Multiple Buffs

Multiple different buffs can be active simultaneously:
- Each buff has its own `BuffInstance`
- Stat changes stack additively
- Example: +50% speed buff + +30% speed buff = +80% total

### Negative Modifiers

Create debuff-like effects:
- Defense x0.5 = Half defense
- MoveSpeed x0.7 = 30% slower
- Useful for high-risk/high-reward abilities

### Combining Modifiers

Mix additive and multiplicative: