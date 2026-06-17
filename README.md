# ☢️  Nuclear Option Tactical Tools (plus) ☢️

[NOTT (plus) GitHub repository](https://github.com/yessidor/NO_Tactitools). [NOTT (plus) Codeberg repository](https://codeberg.org/yessidor/NO_Tactitools).  
[Original NOTT repository](https://github.com/clumzy/NO_Tactitools).

**Original NOTT README.md content follows, see below for [Additional features](#additional-features) .**

---

## About

Nuclear Option Tactical Tools is an immersion and QoL focused gameplay mod.

The mod aims to enhance cockpit immersion and reduce repetitive UI actions without automating core combat mechanics, with a heavy focus on ensuring non-mod users are not at a disadvantage and a *vanilla* feel for the new functionalities.
Every component of the mod is togglable, and you can use your keyboard and HOTAS to interact with some of the new functionalities.

I sincerely hope you enjoy the mod as much as I enjoy developing it, feel free to hit me up on [Discord](https://discord.com/channels/909034158205059082/1387441277414539316) !

Fly safe,

George

## Main features

### **Combat & Targeting Features**

### 🎯 Interception vector on the target screen for single targets

- Only works for single targets
- Takes 3 seconds to spool up
- ETA and bearing are displayed at the bottom of the target screen
- The interception solution is not updated if the target is not tracked
- The interception solution is reset and deactivated when you are being jammed
- The solution is based on the target's last 3 seconds of movement to ensure this feature provides no advantage in combat
<details>
<summary>Screenshot :</summary>
<IMG src="readme_content/intercept.png"  alt="1.png"/>
</details>

### 💾 Target list handling (Uses [new bindings](#how-to-setup-the-mod-to-use-your-peripherals))

- Control and navigate through your target list
- Currently focused target is displayed on the targeting screen, as well as its information
- The inputs used for this feature will function when the Autopilot screen is not in use
- **Target Navigation** (Uses **MFD Nav Left/Right**):
  - **Short press** -> Cycle through selected targets (Previous/Next)
  - **Long press** -> Sort targets (Name/Distance)
- **Target Pop/Keep Only** (Uses **MFD Nav Enter**):
  - **Short press** -> Deselect current target
  - **Long press** -> Keep ONLY current target
- **Save/Recall Group** (Uses **MFD Nav Up**):
  - **Long press** -> Save target group
  - **Short press** -> Recall target group
- **Smart Filter** (Uses **MFD Nav Down**):
  - **Short press** -> Keep only data-linked targets
  - **Long press** -> Keep closest targets based on available ammo count
<details>
<summary>Screenshot :</summary>
<IMG src="readme_content/targetlist.png"  alt="1.png"/>
</details>

### 💣 Target Screen delivery indicators and per-shot indicators to indicate launch/detonation "delivery" status

- Show indicators on each side of the Target Screen for each launched missile/bomb; indicators persist ~2s after impact
- Color delivery: green = armor hit, red = miss for instant outcome feedback
- Missiles are on the left side of the screen and bombs are on the right side of the screen
<details>
<summary>Screenshots :</summary>
<IMG src="readme_content/newdeliveryindicator.png"  alt="1.png"/>
</details>

### 🟢 Per-target Ammo Conservation indicator on the Target Screen
- Shows a green dot below the target box on the Target Screen if the target is already being tracked by a deliverable
- Works with multiple targets, each target will have its own indicator
<details>
<summary>Screenshot :</summary>
<IMG src="readme_content/ammocon.png"  alt="1.png"/>
</details>

### 🔘 Separate, dedicated buttons for slot selection (Uses [new bindings](#how-to-setup-the-mod-to-use-your-peripherals))

- Can be assigned to any peripheral button
- Direct-select weapon slots via dedicated buttons
- Slot order is based on the order weapons are first shown on the loadout screen
- **NOTT (plus)**: Number of slots is configured by `Advanced Slot Selection - Number` variable (restart the game to apply changes). If `Advanced Slot Selection - Skip Empty Stations` is enabled, empty weapons will be skipped when changing active weapon with `Next Weapon` and `Previous Weapon` keys bound in game controls (it's still possible to select empty weapon by pressing the key bound to its slot in mod settings).

### 💥 Separate, dedicated buttons for Flares and Jammer selection (Uses [new bindings](#how-to-setup-the-mod-to-use-your-peripherals))

- Can be assigned to any peripheral button

### **Quality-of-Life Features**

### 📊 Weapon & Countermeasure Display MFD (Uses [new bindings](#how-to-setup-the-mod-to-use-your-peripherals))

- Shows flares/jammer status, current weapon name, and ammo in the cockpit
- Per-airframe layouts
- **Toggling between new and original content** (Uses **MFD Nav Toggle Screens**)
  - **Long press** -> Toggle between new and original content
<details>
<summary>Screenshots :</summary>
<IMG src="readme_content/weapon1.png"  alt="1.png"/>
<IMG src="readme_content/weapon2.png"  alt="2.png"/>
</details>

### 📋 Loadout Preview on main MFD

- Displays weapon loadout on the MFD on active slot switch
- Preview duration is configurable (default: 1 second)
- Automatically hides after the specified duration for uncluttered gameplay
- Can be setup to only be shown once when the airframe starts
- Can be setup to display on the HMD (main UI)
  - By default if the vanilla weapon panel is present, the loadout preview will follow its position
  - By default if the vanilla weapon panel is not present, the loadout preview will stay in the top right corner
  - The loadout preview's position is overridable with settings in Config Manager if you don't like the default behaviour
<details>
<summary>Screenshots :</summary>
<IMG src="readme_content/loadout1.png"  alt="1.png"/>
<IMG src="readme_content/loadout2.png"  alt="2.png"/>
</details>

### 📡 Unit marker distance indicator

- Changes HMD marker orientation for enemy air units when within a configurable distance threshold
  - The enemy unit's icon points downwards when the enemy unit is under the threshold
  - The speed at which the icon rotates when crossing the threshold indicates the enemy unit's speed
- Optional “near” sound cue
<details>
<summary>Screenshot :</summary>
<IMG src="readme_content/distance.png"  alt="1.png"/>
</details>

### 🧭 Artificial Horizon on the HMD

- Horizon line always shown
- Cardinal directions are indicated and hidden when in front of the main HUD
- The transparency is configurable
- You can select for which airframe you want the Artificial Horizon to display by editing an included config file
  - The default airframes for this feature are:
    - SAH-46 Chicane
    - VL-49 Tarantula
    - UH-80 Ibis
<details>
<summary>Screenshot :</summary>
<IMG src="readme_content/horizon.png"  alt="1.png"/>
</details>

### 🛬 ILS Widget on the HUD
- Shows an ILS widget on the HUD when you are cleared for landing at a friendly runway
- The widget ranges from -1° to +1°, this setting is configurable
- The widget's position is adjustable in Config Manager
<details>
<summary>Screenshot :</summary>
<IMG src="readme_content/ils.png"  alt="1.png"/>
</details>

### ⚖️ Bank Indicator on the HUD
- Shows a bank angle indicator on the HUD at all times
- The indicator ranges from -45° to +45°, the max angle is configurable
  - The preferred setting for the max angle is 45° since the needle will always point to the ground
- The indicator's position is adjustable in Config Manager
- The number of notches adapts to the max angle setting
- The transparency is configurable
- You can select for which airframe you want the Bank Indicator to display by editing an included config file
  - The default airframes for this feature are:
    - SAH-46 Chicane
    - VL-49 Tarantula
    - UH-80 Ibis
    - CI-22 Cricket
    - EW-1 Medusa
    - SFB-81
    - A-19 Brawler
<details>
<summary>Screenshot :</summary>
<IMG src="readme_content/slipskid.png"  alt="1.png"/>
</details>

### ↗️ Slip/Skid Indicator on the HUD
- Shows a slip/skid indicator on the HUD at all times
- The indicator calculates the ratio between the lateral acceleration and the upwards acceleration to determine if you are slipping or skidding, and in which direction
- The sensitivity of the indicator (ratio at max offset) is adjustable in Config Manager
- The damping of the indicator is adjustable in Config Manager
- The indicator's position is adjustable in Config Manager
- The transparency is configurable
- You can select for which airframe you want the Slip/Skid Indicator to display by editing an included config file
  - The default airframes for this feature are:
    - SAH-46 Chicane
    - VL-49 Tarantula
    - UH-80 Ibis
    - CI-22 Cricket
    - EW-1 Medusa
    - SFB-81
    - A-19 Brawler
<details>
<summary>Screenshot :</summary>
<IMG src="readme_content/slipskid.png"  alt="1.png"/>
</details>

### **Cosmetic & Enhancement Features**

### 🎨 Cockpit MFD color customization

- Set main and texts MFD colors
- Optional alternative attitude (horizon/ground) colors
- Works with vanilla and modded cockpit UI elements
- The MFD main color is updated in real time ingame
<details>
<summary>Screenshots :</summary>
<IMG src="readme_content/mfd1.png"  alt="1.png"/>
<IMG src="readme_content/mfd2.png"  alt="2.png"/>
</details>

### ⚡ Boot Screen animation

- A short booting animation is displayed on airframe start
- The animation lasts for 2 seconds

### **Camera features**

### 📷 New cockpit camera QoL inputs (Uses [new bindings](#how-to-setup-the-mod-to-use-your-peripherals))

- Adds a button that smoothly resets the cockpit's camera FOV to it's set default value when pressed
  - The reset speed is configurable in Config Manager
- Adds a button that focuses the cockpit's camera on the closest airbase when held

### **Mod Compatibility Features**

### 🛩️ NOAutopilot Control Menu (Uses [new bindings](#how-to-setup-the-mod-to-use-your-peripherals))

- Full HOTAS-friendly menu navigation for the [NOAutopilot mod](https://github.com/qwerty1423/no-autopilot-mod) with intuitive short/long press inputs
- Toggleables are visually indicated on the new MFD menu
- **Opening/Closing the menu** (Uses **MFD Nav Toggle Screens**):
  - **Short press** -> Open/Close the menu
- **Menu Navigation** (Uses **MFD Nav Up/Down/Left/Right**):
  - **Short press** -> Single-step navigation
  - **Long press** -> Continuous navigation
- **Staged Value Adjustment** (Uses **MFD Nav Enter** on +/- buttons):
  - **Short press** -> Increment/decrement by 1 step
  - **Long press** -> Rapid adjustment
- **Set Staged Value to Current** (Uses **MFD Nav Enter** on staged value fields):
  - Loads current flight values into editable fields, rounded to appropriate increments
- **Clear Staged Value** (Uses **MFD Nav Enter** on C buttons):
  - **Short press** -> Resets individual parameters to OFF state
  - **Long press** -> Resets all parameters to OFF state and disengages entire autopilot
- **Apply Staged Values** (Uses **MFD Nav Enter** on SET button):
  - Commits all staged values to the autopilot system
- **Speed Mode Toggle** (Uses **MFD Nav Enter** on Target Speed value field):
  - **Long press** -> Switches between **Mach** and **True Air Speed (TAS)** modes
- **Navigation Mode Toggle** (Uses **MFD Nav Enter** on Target Bearing value field):
  - **Long press** -> Enables/disables autopilot bearing hold mode
- **Extreme Throttle Toggle** (Uses **MFD Nav Enter** on Target Climb Rate value field):
  - **Long press** -> Allows autopilot to command full throttle range when enabled
- **System Toggles** (Uses **MFD Nav Enter** on specific buttons):
  - **Autopilot** -> Engage/disengage entire autopilot
  - **Auto-Jammer** -> Toggle automatic countermeasure deployment
  - **GCAS** -> Ground Collision Avoidance System on/off with status indication

<details>
<summary>Screenshots :</summary>
<IMG src="readme_content/autopilot1.png"  alt="1.png"/>
<IMG src="readme_content/autopilot2.png"  alt="2.png"/>
</details>

### **Deprecated Features**

### 🛡️ AA unit icon recolor on the main map (**DEPRECATED**)

- **I recommend you switch to the excellent Vanilla Icons Plus mod for the same
functionnality and more**
- **Download it [here](https://discord.com/channels/909034158205059082/1465420909295697942)**
- **Current mod users should deactivate the feature using Config Manager**
- Enemy AA units are recolored on the main map
- The color is configurable
- You can select which units are recolored by editing an included config file

## Installing

### :one: Installing BepInEx

- Download the BepInEx version corresponding to your OS [here](https://github.com/BepInEx/BepInEx/releases)
- Extract the content of the ZIP file to the root of your Nuclear Option folder (usually *[your steamapps folder]/common/Nuclear Option*)
- Your Nuclear Option folder should normally have a new folder called *BepInEx* inside

### :two: Installing Configuration Manager (to configure the mod)

- Download Configuration Manager [here](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases)
  - **🚨 Make sure you download the BepInEx5 version 🚨**
- Extract the content of the ZIP file to the root of your Nuclear Option folder
- The BepInEx folder in your Nuclear Option folder should now have a new folder called *plugins* inside
- Press F1 in-game to display the configuration menu
- If the configuration menu doesn't show up, follow these steps :
  - Go to *Nuclear Option/BepInEx/config* and open *BepInEx.cfg*
  - Set **HideManagerGameObject** to **true**
  - You can change the shortcut by editing the setting **Show config manager** in *com.bepis.bepinex.configurationmanager.cfg*

### :three: Installing the mod

- Download Nuclear Option Tactical Tools [here](https://github.com/clumzy/NO_Tactitools/releases)
- Extract the content of the ZIP file in *Nuclear Option/BepInEx/plugins* (where Configuration Manager is already located)
- The plugins folder should now have a new folder called *NOTT* inside

## Configuring the mod

### How to activate/deactivate and configure features

- Open Configuration Manager once the main menu of Nuclear Option is loaded
- Click on the *NOTT* tab
- Hovering your mouse over each setting will give you more details
- Disable/Enable the components you want, and edit their settings if appliable
- **RESTART THE WHOLE GAME** (activated mod components are patched on game start)

### Advanced configuration (Unit Icon Recolor, Artificial Horizon, Slip Indicator, Bank Indicator)

You can configure these modules using text files located in the mod's folder.
As time goes on I will allow more features to be precisely configured using text files.

- Open the *config* folder located in *Nuclear Option/BepInEx/plugins/NOTT*
- Open the two text files and follow the instructions in the comments (comments start with *//*)

### How to setup the mod to use your peripherals

- Open Configuration Manager once the main menu of Nuclear Option is loaded
- Click on the *NOTT* tab
- Bind the controls as you would in-game
  - Press ESC to cancel the assignement
  - Press SUPPR to clear the assignement
- That's it !

## Compatibility

### Compatible mods

- **QoL** (qol_1.1.6.1b3)
- **FQ-106** Kestrel (fq106_2.0.2)
- **Vanilla Icons PLUS** (VanillaIconsPLUS_1.5.1)
- **NOAutopilot** (NOAutopilot v4.17.1)
- **ThirdPersonHud** (ThirdPersonHud v1.2.2)

## Common issues

### I've activated/deactivated a feature but I don't see any change ingame

Restart the game, **I BEG YOU**.

## Contributing

### Reporting bugs

- You can either send me a DM on Discord (look for *cleunaygeorges*), or report it in [the mod's thread](https://discord.com/channels/909034158205059082/1387441277414539316)
- You can also submit an issue on GitHub
- When submitting bugs, I request that you provide two files :
  - *LogOutput.log*, found in *[your steam folder]/steamapps/common/Nuclear Option/BepInEx/*
  - *Player.log*, found in *[your user folder]/AppData/LocalLow/Shockfront/NuclearOption/*
- Please be as descriptive as possible so that I can reproduce the bug
- **NO LOGS, NO HELP**

### Contributing to the mod

- Feel free to suggest additions
- You can also submit a pull request if you want to help me develop the mod !

## FAQ

*Coming soon*

---

## Additional features

### Active target selection

Cycling through target list (with `MFD NAV Left/Right` keys) makes the currently focused target active, just like if it was added to target list last. This allows, for example, to easily select specific target for gun attack without first clearing the target list.  

Feature state is controlled by `Target List Controller - Switch Current Target - Enabled` setting in plugin settings.  

### Additional target lists

Extra target lists were added in addition to the default target list saved and restored by `MFD NAV Up` key.  

Number of extra target lists is defined by `MFD Nav - Extra Key - Number` setting (restart the game to apply changes).  

Long press the corresponding `MFD Nav - Extra Key #` to save target list, short press to restore it.

### Target filter presets

This feature allows to save and load presets for target filter configuration (a window opened by by **"TGT"** button on the right side of the maximized map). Loading filter preset when some targets are already selected will deselect not matching targets.  

Feature state is controlled by `Target Filter Preset - Enabled` setting in plugin settings (restart the game to apply changes).  

Number of target filter presets is defined by `Target Filter Preset - Number` setting (restart the game to apply changes). Keys are bound in `"Target Filter Preset - Slot #"` settings.  

Presets are persistent: they are saved to config file `TargetFilterPreset.cfg` when modified and loaded on mission load.

### HUD options presets

Adds key-bound presets for HUD options, just like [Target Filter Presets](#target-filter-presets).

`HUD Options Preset - Enabled` controls the state (restart the game to apply changes), `HUD Options Preset - Number` sets number of presets (restart the game to apply these settings). `HUD Options Preset - Slot #` binds key to given preset, long press saves HUD settings to preset, short press loads. `HUD Options Preset - Enable Builtin Settings` enables built-in switching HUD settings on selecting weapon (when using HUD options presets, this most likely needs to be false).

Presets are persistent: they are saved to config file `HUDOptionsPreset.cfg` when modified and loaded on mission load.

### Maximizing markers of targetable units

If this option is enabled, markers of units eligible for targeting by target filter configuration will be always maximized regardless of HUD settings (and when gears are deployed).

Feature state is controlled by `Target Filter Preset - Maximize Targetable Markers - Enabled` setting in plugin settings.  

### Alternative target selection on HMD

Targeting in NO does not always produce expected results, so a more simpler algorithm was added.  
In single target selection mode (with target selection key clicked) it selects the closest target that passes target filters and is within target selection cone centered around direction designated by target selection marker.  
In "paint" mode (with target selection key held) it will not account for distances and will select all targets that pass filters and fall into the target selection cone.

`Alternative Target Selection - Enabled` setting controls the state of the feature, and `Alternative Target Selection - Camera FOV Fraction` sets the fraction that is multiplied by camera vertical FOV to get the apex angle (aperture) of selection cone. `Alternative Target Selection - Max Distance` sets the max distance to select target at, measured in meters (set to 0 do select targets at any distance). If `Alternative Target Selection - Pick Active` is enabled, when target selection key is clicked and no new target can be selected, the best matching target from already selected ones is made active.

### Filtering targets tracked or not tracked by deliverables

This is an add-on to **Ammo Conservation indicator** and allows to remove targets that are either tracked or untracked from the selected targets list.  

Short press on the key bound to `MFD Nav - Backspace` removes tracked targets, long press removes untracked targets.

### Filtering targets based on the unit name of the current target

Short press on the key bound to `MFD Nav - Select Targets By Unit Name` deselects targets which have the same unit name as the currently active target (including the active target itself). Long press on the same key removes targets which unit names *differ* from active target unit name.

### Filtering lased or unlased targets

Short press on the key bound to `MFD Nav - Select Targets By Lased status' deselects lased targets, long press deselects unlased.

### Incoming missiles targeting

This feature enables fast targeting of incoming missiles, both in manual and automated mode.

Short press on key bound to `MFD Nav - Missile Targeting System` saves currently selected targets and targets all incoming missiles sorted by increasing distance, so the closest missile becomes the active target. Another short press on the same key restores previous targets. Previous targets are also automatically restored when the last missile is defeated.  
Long press on the controlling key toggles the automated incoming missile targeting: it is engaged when a missile is registered as a threat. Like in the manual mode, previous targets (if any) are automatically restored when the last missile is defeated.

Edge case: targets selected while incoming missile targeting is active, will be deselected when the incoming missiles list is updated.

### Extended Ammo Conservation Indicator

This feature allows to recolor HMD markers of selected targets that are being tracked by deliverables. MFD box markers of selected targets can also be painted with a different color if those targets are being tracked. In addition to that, it's possible to turn of the dot markers under MFD boxes of tracked targets.

Feature state is controlled by `Ammo Conservation Indicator - HMD Markers Color - Enabled` setting. Colors are controlled by color settings under `Ammo Conservation Indicator` section.

### MFD Target camera mode toggle

MFD Target camera can be toggled between looking at all selected targets (the default behavior) and looking at active target only.

Feature state is controlled by `Target Cam Mode - Enabled` setting, mode toggle key is bound to `Target Cam Mode - Toggle Mode Key`.

### Hide objectives and airbase markers with text on HMD

If this option is enabled, turning off **"OBJ"** button in HUD settings (window opened by **"HUD"** button on the left side of maximized map) turns off objectives and airbase markers and text on HMD, in addition to hiding objective marker and text on the map.

Feature state is controlled by `Hide Objectives - Enabled` setting.

### HMD unit marker recoloring

A small convenience feature that allows to recolor HMD unit markers.

`HMD Unit Markers Recolor - Enabled` controls the state, `HMD Unit Markers Recolor - Friendly|Enemy|Neutral Unit Color` control respective colors.

### Minimap zoom

Allows to change zoom level of the minimap.

`MiniMap Zoom - Enabled` controls the state (restart the game to apply changes). `MiniMap Zoom - Zoom levels` is a semicolon-separated list of zoom levels with dot (.) acting as fraction separator. Default in-game minimap zoom level is 2.0. `MiniMap Zoom - Offset` is an offset from center of the minimap to player aircraft in meters for default zoom level. Short press on key bound to `MiniMap Zoom - Cycle Zoom Key` cycles zoom levels towards next zoom level, long press restores default zoom level. Short press on key bound to `MiniMap Zoom - Cycle Zoom Down Key` cycles zoom levels towards previous zoom level, long press restores default zoom level. If `MiniMap Zoom - Report` is enabled, minimap zoom level changes are reported on HMD.

### HMD Declutter

Declutters HMD by introducing marker draw distance and options to minimize markers that are supposed to be maximized and to hide markers that are supposed to be minimized. Selected and flashing icons will be drawn at any distance and regardless of marker minimizing and hiding options (see below). Also, if `Target Filter Preset - Maximize Targetable Markers - Enabled` is enabled, targetable markers will be always maximized regardless of the following settings. 

`HMD Declutter - Enabled` controls the state of the feature.

#### HMD markers draw distance

`HMD Declutter - Marker Draw Distances` is a string of marker draw distances, measured in units specified by `HMD Declutter - Unit`. Distance values separator is ";", fraction separator is ".", "0.0" is unlimited distance. Example of the distances string: "0;1000.0;5000;50000". `HMD Declutter - Cycle Marker Draw Distance Up` and `HMD Declutter - Cycle Marker Draw Distance Down` are key bindings for cycling distance up or down the distances list. `HMD Declutter - Report` determines whether changing the marker draw distance will be reported on HMD.

#### Minimizing and hiding HMD markers

If `HMD Declutter - Not Always Maximized` is enabled, no markers will be always maximized by default (currently only aircraft markers are always maximized by game). If `HMD Declutter - Minimize Maximized` is enabled, markers that should be maximized according to HUD settings, will be minimized to dots. If `HMD Declutter - Hide Minimized` is enabled, markers that should be minimized, will be hidden instead. `HMD Declutter - Enemy Minimized Marker Scale` and `HMD Declutter - Friendly Minimized Marker Scale` set scales of enemy and friendly minimized markers respectively.

### Map and minimap target arrows

Adds arrows that point to selected targets if they are out of map bounds. Active target is distinguished by different arrow and marker color and, optionally, by "T" marker.

`Map Target Arrows - Enabled` controls the state (restart the game to apply changes), `Map Target Arrows - Arrow Scale` sets the arrows scale (relative to target arrow on HMD), `Map Target Arrows - Selected Color` and `Map Target Arrows - Active Color` set colors for selected targets and the active target respectively, `Map Target Arrows - Show T` determines whether to show "T" near the active target arrow.

### Multiple HMD target arrows

This feature fixes position of active target arrow marker and adds an option to display arrow markers for other targets. Primary target arrow is designated by "TARGET" label.

`Target Arrows - Enabled` controls the state (restart the game to apply changes). `Target Arrows - Number of arrows` set number of target arrows (0 is unlimited, 1 is default primary target arrow). `Target Arrows - Arrow Color` and `Target Arrows - Arrow Scale` set the color and scale of target arrows. But if `Target Arrows - Match Marker Color` is enabled, target arrow color (and the color of "TARGET" text for main target arrow) will match the color of corresponding target marker (useful if `Ammo Conservation Indicator - HMD Markers Color - Enabled` is enabled).

### HUD center direction arrow

Adds an arrow pointing to HUD center.

`HUD Center Direction - Enabled` controls the state (restart the game to apply changes), `HUD Center Direction - Arrow Color` and `HUD Center Direction - Arrow Scale` set arrow color and scale.

### Target velocity indicator

When enabled, the marker on HMD shows velocity vector of the current (active) target relative to current cockpit view. This vector is represented by a marker placed at the offset from marker of the current target. "x" marker means target moves toward the player aircraft, "o" - away from it. The size of offset depends only on the lateral movement of target and is not scaled by distance.

`Target Velocity Indicator - Enabled` controls the state (restart the game to apply changes). `Target Velocity Indicator - Max Speed` is the maximum speed (velocity magnitude; in km/h) for maximum offset of marker. `Target Velocity Indicator - Max Length` is the maximum offset of velocity marker (in pixels) for maximum speed. `Target Velocity Indicator - Dot Step` is distance between the dots connecting target and velocity markers.

### Text size adjustments for HUD and map

Adds settings to adjust sizes of various text fields: target marker info and tooltip, objective marker text, grid labels on map; time of flight and missile ranged on HUD. `UI Adjustments - Enabled` controls the state, numerical options in `UI Adjustments` config section control text sizes. Note: wing and nozzle gauge font settings will be applied after respawning the aircraft. 

### Alternative algorithm for target selection on map

Implements alternative way to select units on the maximized map, because selection radius of the built-in algorithm is too large.

`Alternative Map Target Selection - Enabled` controls the state, `Alternative Map Target Selection - Selection Radius` sets selection radius (in pixels) around mouse cursor. To select the unit, click on the unit marker with the key bound to "Select" in controls settings; hold the key to select all units in the selection radius. If `Alternative Map Target Selection - Pick Active` is active, clicking on already selected unit designates is as active target.

### Togglable free look

Allows to toggle the free look mode and adds some view-related actions.

This feature uses keys bound to `Free Look` and `Center View` in game control bindings menu. Virtual Joystick is assumed to be enabled. `Target Padlock` option in Gameplay settings tab is also supposed to be enabled.

Clicking `Free Look` key toggles free look mode on (when mouse controls camera) and off (when mouse controls player aircraft). Holding `Free Look` key temporarily sets view to forward and disables free look on press and restores previous free look state and view on release.  
Clicking `Center view` key switches between target padlock mode and previous view (if `Target Padlock` option in Gameplay settings tab is enabled, otherwise does nothing). Holding `Center view` key sets view to forward.

In the mod settings, `Free Look Toggle - Enabled` controls the state of the feature (restart the game to apply changes). If `Free Look Toggle - Disable Free Look In Padlock` is enabled, free look mode is automatically disabled in padlock mode. If `Free Look Toggle - FOV-dependent Sensitivity - Enabled` is enabled, mouse sensitivity in free look mode depends on the current FOV: the lesser the FOV, the lesser the sensitivity. If `Free Look Toggle - Report` is enabled, free look and padlock state changes are reported on HMD.

### Third person HUD

Enables HUD in third person mode (when camera is in "orbit" or "chase" modes).

`Third Person HUD - Enabled` enables the feature (restart the game to apply changes). If `Third Person HUD - HUD Roll - Enabled` is enabled, HUD will pivot with aircraft roll. If `Third Person HUD - HUD Bound To Screen - Enabled` is enabled, HUD will stay at `Third Person HUD - HUD Screen Offset` position relative to screen center. If `Third Person HUD - Set Target Designator Position - Enabled` is enabled, target designator will be placed at `Third Person HUD - Target Designator Screen Offset` from screen center.

### Cokpit camera control with keys

Implements alternative way to control cockpit camera with keys.

`Key View Control - Enabled` controls the state (restart the game to apply changes. `Key View Control - Pan Left`, `Key View Control - Pan Right`, `Key View Control - Tilt Up`, `Key View Control - Tilt Down` are key bindings for pan (horizontal) and tilt (vertical) axes respectivelly. Short press will change angles by values set by `Key View Control - Pan Step` and `Key View Control - Tilt Step` config variables, long press will gradually change angles with speed set by `Key View Control - Pan Speed` and `Key View Control - Tilt Speed`. If `Key View Control - FOVDependent` is enabled, step and speed will be adjusted by FOV-dependent factor (lesser the FOV, lesser the speed and step). If `Key View Control - Stop At 0` is enabled, changing pan and tilt anges in steps will stop at zero angle values regardless of step size. 

### Dynamic landing camera

Enables to pivot landing camera towards velocity vector.

`Dynamic Landing Cam - Enabled` enables the feature (restart the game to apply changes). `Dynamic Landing Cam - Keep On After Touchdown - Enabled` keeps landing camera on after touchdown and after spawning on ground. `Dynamic Landing Cam - Rotate - Enabled` makes landing camera rotate towards velocity vector at `Dynamic Landing Cam - Rotation Speed` and within `Dynamic Landing Cam - Tilt Limits` and `Dynamic Landing Cam - Pan Limits`. `Dynamic Landing Cam - Initial Angles` set initial landing camera tilt and pan angles, `Dynamic Landing Cam - Landing Cam FOV` sets landing camera FOV. `Dynamic Landing Cam - Deadzone` sets the deadzone angle (in degrees): landing camera won't rotate if angle between velocity vector and camera direction is less that this angle. `Dynamic Landing Cam - Fix A-19 Brawler Landing Cam` fixes A-19 Brawler landing cam position: as of NO 0.33.4, it is set inside the aircraft.

### Customizable modifier keys

This feature allows to assign any button on any device as a modifier key.

`Modifiers - Number` sets total number of modifier keys (restart the game to apply changes). `Modifiers - Modifier #` setting assigns bound key as a modifier. If some key was bound as non-modifier, functional key, it won't be allowed to be bound as modifier, and vice versa (error message about this conflict is displayed within input field). When binding modifier and functional keys combination to selected action in the mod settings, just hold modifier key(s) while pressing functional key.

### Axes binding

Input subsystem supports axis binding (though currently no mod components use this functionality).

### Better Virtual Joystick

#### 3 modes of operation

This feature enables **Virtual Joystick** to operate in 3 modes:

 * game default `roll` mode, when mouse `x` axis input controls `roll` axis,
 * `yaw` mode, when mouse `x` axis input controls `yaw` axis,
 * `roll&yaw` mode, when mouse `x` axis controls both `roll` and `yaw` axes.

mouse `y` controls `pitch` axis in all modes.

Feature state is controlled by `Virtual Joystick Extender - Enabled` setting.
If enabled, state is toggled by `Virtual Joystick Extender - Toggle Key`.
Switching to `yaw` mode is done by `Virtual Joystick Extender - Yaw Mode Key`; switching to `roll&yaw` mode - by `Virtual Joystick Extender - Roll&Yaw Mode Key`.  
If `Virtual Joystick Extender - Toggle Mode - Enabled` setting is disabled, `yaw` and `roll&yaw` modes are temporarily enabled by respective key press and `roll` mode is restored when key is released; if this setting is enabled, modes are toggled between a corresponding mode and `roll` mode by key press.
Default mode is set by `Virtual Joystick Extender - Default Mode` option.

In `yaw` mode the output to `yaw` axis is multiplied by value set by `Virtual Joystick Extender - Yaw Mode Multiplier` setting, in `roll&yaw` mode - by `Virtual Joystick Extender - Roll&Yaw Mode Multiplier` setting.

#### Axis response curves

This feature also adds response curves for `yaw`, `pitch` and `roll` axes.

Shape of these response curves are defined by so-called depressed cubic equation of the form y = Curvature\*x<sup>3</sup> + (1 - Curvature)\*x .

`Curvature` parameters for respective axes are controlled by `Virtual Joystick Extender - Yaw Curvature`, `Virtual Joystick Extender - Pitch Curvature` and `Virtual Joystick Extender - Roll Curvature` settings.

#### Setting primary axes to 0 when virtual joystick is temporarily disabled

Set `Virtual Joystick Extender - Decay Mode` to `Instant` to set pitch, roll, and yaw axes to 0 when virtual joystick control is disabled by opening map, leaderboard, or radial menu.

#### Controlling in third person mode

If `Virtual Joystick Extender - Control In Third Person Mode - Enabled` virtual joystick will control aircraft in third person mode. See [Third person HUD](#third-person-hud).

### Better axis control with keys

All axes -- `pitch`, `roll`, `yaw`, `brakes`, `throttle`, `custom axis 1` -- can be controlled by key-controlled response curves.

State of this feature is controlled by `Key Axes - Enabled` setting. Key bindings and curves settings are under `Key Axes` section.

Each axis is assigned a pair of keys, pressing one key will decrease axis value, pressing the other will increase it, both at the base `Build-Up speed` modified by response curves (see below). If both keys are pressed, the currently attained axis value is maintained. When both keys are released, if `Decay Speed` parameter (see below) is greater than 0, the axis value will decay to `Default Value`, otherwise the attained axis value is maintained until further input. 

Each axis is assigned so-called `Dynamic` and `Static` response curves. Shapes of these response curves are defined by so-called depressed cubic equation of the form y = Curvature\*x<sup>3</sup> + (1 - Curvature)\*x + DefaultValue .  
`Dynamic` curve basically determines the acceleration of axis value change: the higher is the `Dynamic Curvature` the slower the axis value changes initially.  
`Static` curve determines how fast the axis value changes around `Default Value`: the higher is the `Static Curvature`, the slower.

 *Note:* for `throttle` axis, the same controlling keys must be bound in the game controls menu in order to control `throttle` axis in hover mode.

## Compatibility

Updated mod (0.7.8.5) was tested under Nuclear Option 0.33.3. Compatible with **QoL** (1.1.7.1), **NOAutopilot** (5.2.0), **FQ-106** Kestrel (2.1.0; mind that it is bugged by itself), **MC-260** Chimera (1.0.9), **RAH-72** Knockout (1.0.0). Compatibility with **Vanilla Icons PLUS** and **ThirdPersonHud** was not tested.

## On possible "Could not load file or assembly MonoMod.Backports" error

If the mod does not work and the error message *"Could not load file or assembly 'MonoMod.Backports...'"* is reported in `LogOutput.log`, place `MonoMod.Backports.dll` and `MonoMod.ILHelpers.dll` files from the folder `ON_ERROR_PLACE_IN_GAME_FOLDER` within the archive into the Nuclear Option folder. Don't place them into the NOTT folder, as BepInEx will delete them.
