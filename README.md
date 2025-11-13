# CVR Advanced Avatar Preset Generator - Documentation

## Overview

**CVR Advanced Avatar Preset Generator** is a Unity editor tool designed for ChilloutVR avatars that allows creators to build and manage preset systems for their avatar parameters. It provides an intuitive way to create parameter configurations that can be quickly switched between, reducing the complexity of managing multiple avatar states.

## Table of Contents

- [What is CVR Advanced Avatar Preset Generator?](#what-is-cvr-advanced-avatar-preset-generator)
- [Why Use CVR Advanced Avatar Preset Generator?](#why-use-cvr-advanced-avatar-preset-generator)
- [Use Cases](#use-cases)
- [Installation](#installation)
- [How to Use](#how-to-use)
- [Technical Details](#technical-details)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

***

## What is CVR Advanced Avatar Preset Generator?

CVR Advanced Avatar Preset Generator is a component that automatically generates a preset system for ChilloutVR avatars by:

1. **Scanning** all available Advanced Avatar Settings (AAS) parameters
2. **Creating** preset configurations with specific parameter values
3. **Generating** the necessary animator drivers and animation clips
4. **Building** animator controller logic to switch between presets seamlessly

It eliminates the need to manually create complex animator setups for managing multiple avatar configurations.

***

## Why Use CVR Advanced Avatar Preset Generator?

### Problem It Solves

Managing multiple avatar states in ChilloutVR typically requires:
- Creating dozens of animation clips manually
- Setting up complex animator controller logic
- Managing multiple CVRAnimatorDriver components
- Keeping parameter values synchronized across different states

This process is **time-consuming**, **error-prone**, and **difficult to maintain**.

### Solution

CVR Advanced Avatar Preset Generator automates this entire workflow:
- ✅ **One-click generation** of preset systems
- ✅ **Automatic animator driver creation** with proper parameter values
- ✅ **Optimized animator controller** with minimal transitions
- ✅ **Easy preset management** through a simple UI
- ✅ **No manual animation clip creation** required

***

## Use Cases

### 1. **Outfit Combinations**

**Scenario:** Your avatar has 5 tops, 4 bottoms, and 3 pairs of shoes (60 possible combinations).

**Without CVR Advanced Avatar Preset Generator:**
- Manually toggle each piece individually
- Remember which combinations look good
- Difficult to quickly switch between favorite outfits

**With CVR Advanced Avatar Preset Generator:**
- Create presets like "Casual," "Formal," "Sporty," "Beach"
- Each preset stores the exact combination of clothing
- Switch between complete outfits with a single dropdown

### 2. **Character Variations**

**Scenario:** Your avatar can be a human, demon, angel, or robot form.

**Without CVR Advanced Avatar Preset Generator:**
- Toggle horns, wings, halos, and other features separately
- Adjust colors, materials, and effects individually
- Easy to forget a parameter and have inconsistent results

**With CVR Advanced Avatar Preset Generator:**
- Create presets for each form
- Store all relevant parameters (toggles, colors, material properties)
- Instantly transform between character types

### 3. **Seasonal Themes**

**Scenario:** You want different avatar appearances for different occasions.

**Examples:**
- "Christmas" preset: Red/green colors, winter accessories, festive effects
- "Halloween" preset: Dark colors, spooky particles, themed outfits
- "Summer" preset: Bright colors, sunglasses, beach accessories
- "Default" preset: Your everyday look

## Installation

### Prerequisites

- Unity 2019.4.31f1 or later
- ChilloutVR CCK (Content Creation Kit)
- A CVRAvatar component on your avatar

### Steps

1. **Add the Scripts to Your Project:**
   - Copy `CVRPresetManager.cs` to your `Assets/Scripts/` folder
   - Copy `CCK_CVRPresetManagerEditor.cs` to your `Assets/Editor/` folder

2. **Add the Component:**
   - Select your avatar GameObject (the one with CVRAvatar component)
   - Click "Add Component" in the Inspector
   - Search for "CVR Preset Manager"
   - Add the component

3. **You're Ready!** The component is now available on your avatar.

***

## How to Use

### Step 1: Setup Advanced Avatar Settings

1. On your CVRAvatar component, enable **Advanced Settings**
2. Set your **Base Controller** (typically the default AvatarAnimator)
3. Add all the parameters you want to control (toggles, sliders, colors, etc.)
4. Click **"Create Animator"** to generate your base advanced settings

### Step 2: Refresh Available Parameters

1. On the CVR Advanced Avatar Preset Generator component, click **"Refresh Available Parameters"**
2. This scans all your Advanced Avatar Settings and lists available parameters
3. You should see all your parameters appear in the "Available Parameters" list

### Step 3: Create Presets

1. In the **"Presets"** list, click the `+` button to add a new preset
2. Name your preset (e.g., "Casual Outfit", "Demon Form", "Winter Theme")
3. Select the preset to edit it
4. For each parameter:
   - ☑️ **Check the box** if you want this preset to control that parameter
   - **Set the value** you want for this parameter in this preset
   - Leave unchecked if this parameter should remain unchanged

5. Repeat for additional presets

**Example:**
```
Preset: "Demon Form"
✅ Horns = True
✅ Wings = True
✅ Tail = True
✅ HairColor-r = 0.8
✅ HairColor-g = 0.0
✅ HairColor-b = 0.0
✅ EyeGlow = True
❌ Shoes (not controlled by this preset)
```

### Step 4: Generate the Preset System

1. Click **"Generate Preset System"**
2. The tool will:
   - Create a "Preset Selector" dropdown in your Advanced Avatar Settings
   - Generate CVRAnimatorDriver objects for each preset
   - Create animation clips for each preset
   - Add animator controller logic to switch between presets
3. Wait for the success message

### Step 5: Test Your Presets

1. **In Unity Play Mode:**
   - Use the Gesture Manager or similar tool
   - Find the "PresetSelector" parameter
   - Change the value to switch between presets
   - All parameters should update automatically

2. **In ChilloutVR:**
   - Upload your avatar
   - Open the Advanced Avatar Settings menu
   - Use the "Preset Selector" dropdown
   - Select different presets to switch configurations

***

## Technical Details

### How It Works

1. **Parameter Discovery:**
   - Scans CVRAvatar Advanced Settings
   - Extracts all parameter names and types
   - Handles multi-component parameters (e.g., Color = R, G, B)

2. **Driver Generation:**
   - Creates CVRAnimatorDriver GameObjects for each preset
   - Each driver can handle up to 16 parameters
   - Multiple drivers are created if a preset has more than 16 parameters
   - Drivers are named: `PresetDriver_00`, `PresetDriver_01`, etc.

3. **Animation Clip Creation:**
   - One animation clip per preset
   - Animates GameObject activation (enables/disables drivers)
   - Animates all 16 parameter fields on each driver
   - Sets proper values based on preset configuration

4. **Animator Controller Logic:**
   - Adds "PresetSelector" integer parameter to base controller
   - Creates "PresetSystem" layer
   - One state per preset
   - One transition per preset (from AnyState)
   - Transitions trigger when PresetSelector matches preset index

### File Structure

After generation, you'll find:
```
Assets/
├── PresetClip_00.anim (Default Preset animation)
├── PresetClip_01.anim (First custom preset)
├── PresetClip_02.anim (Second custom preset)
└── ...

Avatar/
└── CVR Advanced Avatar Preset Generator/
    ├── PresetDriver_00 (Default preset drivers)
    ├── PresetDriver_01 (First preset drivers)
    ├── PresetDriver_01_00 (First preset, chunk 2 if >16 params)
    └── ...
```

### Performance Considerations

- **Minimal Runtime Overhead:** Drivers are disabled when not active
- **Optimized Transitions:** No blend times, instant switching
- **No Nested Blend Trees:** Direct animation clip usage
- **Efficient Parameter Sync:** Only marked parameters are synced

***

## Troubleshooting

### "CVRPresetManager must be on the same GameObject as CVRAvatar"

**Solution:** Move the CVR Advanced Avatar Preset Generator component to the same GameObject that has the CVRAvatar component (typically your avatar root).

### "Base controller not set in avatar settings!"

**Solution:** 
1. Select your avatar
2. Enable Advanced Settings on CVRAvatar
3. Set the Base Controller field
4. Try generating again

### Preset not switching in-game

**Possible Causes:**
1. Avatar not uploaded with latest changes
2. Base controller not regenerated after making changes
3. "PresetSelector" parameter not synced

**Solution:**
- Re-generate the preset system
- Re-upload your avatar
- Verify "PresetSelector" appears in Advanced Settings menu

### Too many parameters for one preset

**Automatic Handling:** The system automatically splits presets into multiple drivers if you have more than 16 parameters. This is handled transparently.

### Animation clips not being applied

**Solution:**
- Ensure drivers are children of CVR Advanced Avatar Preset Generator GameObject
- Verify animation clips exist in Assets folder
- Check that "PresetSystem" layer exists in animator controller

***

## Best Practices

### 1. Start with Default Preset
Always create a "Default Preset" first with all parameters in their normal state. This provides a baseline to return to.

### 2. Group Related Parameters
Create presets that control related parameters together (e.g., all outfit pieces in one preset).

### 3. Use Descriptive Names
Name presets clearly: "Summer Beach Outfit" instead of "Preset1"

### 4. Test Before Uploading
Always test presets in Play Mode before uploading to ChilloutVR

### 5. Don't Override Everything
You don't need every preset to control every parameter. Only check parameters that should change for that specific preset.

### 6. Regenerate After Changes
If you add new parameters or modify existing ones, regenerate the preset system to ensure everything is up-to-date.

---

## Contributing

Contributions are welcome! If you find bugs or have feature requests:

1. Create an issue describing the problem or feature
2. Submit a pull request with your changes
3. Ensure code follows the existing style
4. Test thoroughly before submitting
