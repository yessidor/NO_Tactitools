# ☢️  Nuclear Option Tactical Tools (plus) ☢️

[NOTT (plus) Codeberg repository](https://codeberg.org/yessidor/NO_Tactitools). [NOTT (plus) GitHub repository](https://github.com/yessidor/NO_Tactitools).

[Original NOTT repository](https://github.com/clumzy/NO_Tactitools).

**Original NOTT README.md content follows, see below for [Additional components and features](#additional-components-and-features) . Also see [CHANGELOG.md](CHANGELOG.md) for history of changes.**

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

### NOTT+

Config settings named `Authorized For` were added for Artificial Horizon, Slip Indicator, Bank Indicator. These settings are sequences of aircraft name patterns, separated by ';'. Aircraft name pattern is a regular expression pattern, so it can be full or partial aircraft name (i.e. 'CI-22' or 'Cricket', or 'CI-22 Cricket'). A component will check this setting first; if no match for current platform name is found, it will look in the corresponding .txt config file. Note that entries of these .txt config files are still not patterns, but full platform names. Respawn to apply changes.

Likewise, `AA Units Icon Recolor` component now has `Unit Names` setting with the same functionality.

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

## Additional components and features

### Input Subsystem

#### Press delay

`Common - Press Delay` sets press delay for NOTT+ key bindings (restart the game to apply changes). Note that in-game key bindings still use "Press delay" setting in game "Controls" menu, even if these key bindings are used by NOTT+ components (like `Free Look Toggle`, `Alternative Target Selection`, and `Alternative Map Target Selection` components).

#### Customizable modifier keys

This feature allows to assign any button on any device as a modifier key.

`Modifiers - Number` sets total number of modifier keys (restart the game to apply changes). `Modifiers - Modifier #` setting assigns bound key as a modifier. If some key was bound as non-modifier, functional key, it won't be allowed to be bound as modifier, and vice versa (error message about this conflict is displayed within input field). When binding modifier and functional keys combination to selected action in the mod settings, just hold modifier key(s) while pressing functional key.

#### Axes binding

Added support for axis binding (used by `Head Axes` and `Key Axes` components).

#### Key and axis binding menu

Pressing on the key/axis binding field in configuration manager menu opens small menu for that binding, containing `Bind button` and `Match modifiers exactly` toggle.

Pressing on `Bind` button starts binding process. If the same key/axis was already bound to another function, this is treated as conflict. Pressing `Bind anyway` button allows to ignore the conflict and bind another action to the same key/axis.

If `Match modifiers exactly` toggle is enabled, extra modifiers pressed along with bound modifiers (if any) won't toggle the binding. I.e. if `Match modifiers exactly` is enabled, LShift-LCtrl-A combination won't toggle LCtrl-A binding, but if the toggle is disabled, it will.

#### Sensitivity fix

This component fixes FPS-bound mouse sensitivity for axes bound to `Pan View`, `Tilt View`, and `Zoom View` in various camera modes, while controlling maximized map, and while using built-in virtual joystick (restart the game to apply changes).

`Sensitivity Fix - Common Sensitivity` sets common sensitivity multiplier that is multiplied by `View Motion Sensitivity` value in game controls menu. Sensitivity multipliers for specific cases are further multiplied by this combined value. `x` component corresponds to `Pan View` axis, `y` - to `Tilt View` axis, `z` - to `Zoom View` or `FOV` axis (if applicable). 0 means "don't modify".

`Sensitivity Fix - # Camera Mouse Sensitivity` settings set sensitivity multipliers for corresponding camera modes. These settings stack with common sensitivity multiplier and game sensitivity setting.

`Sensitivity Fix - Map Controls Mouse Sensitivity` sets sensitivity multiplier that is applied when controlling maximized map with mouse.

`Sensitivity Fix - Built-in Virtual Joystick Mouse Sensitivity` sets sensitivity multiplier for built-in virtual joystick.

`Sensitivity Fix - Radial Menu Mouse Sensitivity` sets sensitivity multiplier for radial menu.

### Targeting

#### Target List Controller

##### Active target selection

This feature allows to select active target by cycling through target list (with `MFD NAV Left/Right` keys); just like if the target was added to target list last. This allows, for example, to easily select specific target for gun or turret without first clearing the target list.  

Feature state is controlled by `Target List Controller - Switch Current Target - Enabled` setting.  

##### Additional target lists

This feature adds extra target lists in addition to the default target list saved and restored by `MFD NAV Up` key.  

Number of extra target lists is defined by `MFD Nav - Extra Key - Number` setting (restart the game to apply changes).  

Long press the corresponding `MFD Nav - Extra Key #` saves target list, short press restores it.

##### Target list extensions

###### Extra functions of target entry checkbox

The built-in target list (accessible by "TGT" button on the right side of maximized map) allows to deselect unit by left-click on unit entry checkbox. Enabling `Target List Controller - Extra Entry Checkbox Functions` adds extra functions to the checkbox: right-click removes _other_ units from target list; if Shift key is held, left-click removes units with the same name, right-click removes units with names that differ. Restart the game to apply changes.

###### Highlighting active target entry

If `Target List Controller - Highlight Active Target List Entry` is enabled, target list entry belonging to active target will be highlighted (restart the game to apply changes).

##### Filtering targets that are tracked (or not tracked) by deliverables

This feature uses `Ammo Conservation Indicator` component to enable removing targets that are either tracked or untracked by deliverables. Needless to say, that `Ammo Conservation Indicator` component needs to be enabled for this feature to work.

Short press on the key bound to `MFD Nav - Backspace` removes tracked targets, long press removes untracked targets.

##### Filtering targets based on the unit name of the current target

This feature allows to filter targets based on the unit name of active target.

Short press on the key bound to `MFD Nav - Select Targets By Unit Name` deselects targets which have the same unit name as the currently active target (including the active target itself). Long press on the same key removes targets which unit names *differ* from active target unit name.

##### Filtering lased or unlased targets

This feature allows to filter lased or unlased targets.

Short press on the key bound to `MFD Nav - Select Targets By Lased status' deselects lased targets, long press deselects unlased.

##### Targeting nearest units

This feature allows to select closest target(s) that match targeting filters and (optionally) are within targeting-related distances set by other components (HMD Declutter and Alternative Target Selection).

Short press on `MFD Nav - Select Closest Targets` selects one closest target, long press selects as many targets as possible and sorts them by distance. If one or several closest targets are found, they are loaded in target list, replacing currently selected targets (if any). If no closest targets could be found, current target list is left intact.

If `Target List Controller - Respect Targeting Distances` is enabled, when calling 'Select Closest Targets' command, units will be selected within minimum of HMD Declutter marker drawing distance and Alternative Target Selection distance. If either of these distances is 0, it is treated as 'not set'.

Enabling `Target List Controller - Select New Closest Targets By Ammo` makes long press on `MFD Nav - Down` key select new closest targets (that match targeting filters and distance) based on current ammo. If the setting is disabled, long press on said key will filter closest targets based on ammo from already selected targets.

If `Target List Controller - Select Units To Reload` is enabled, when map options tooltip is set to 'AMMO', 'Select Closest Targets' command will select friendly unit(s) that need ammo. In this mode targeting filters won't be used, but if 'Respect Targeting Distances' is enabled, targeting distances *will* be used.  
Also, if map options tooltip is "AMMO", current ammo level of selected friendly unit will be displayed on Target Camera feed.

##### Targeting of incoming missiles

This feature enables fast targeting of incoming missiles, both in manual and automated mode.

Short press on key bound to `MFD Nav - Missile Targeting System` saves currently selected targets and targets all incoming missiles sorted by increasing distance, so the closest missile becomes the active target. Another short press on the same key restores previous targets. Previous targets are also automatically restored when the last missile is defeated.  
Long press on the controlling key toggles the automated incoming missile targeting: it is engaged when a missile is registered as a threat. Like in the manual mode, previous targets (if any) are automatically restored when the last missile is defeated.

If `Target List Controller - Select Only Interceptable Missiles` is enabled, missile targeting system will select only interceptable missiles. If current weapon is jammer, only SARH and ARH missiles will be selected. If current weapon is missile or laser, only missiles within `Target List Controller - MTS Angle` will be selected.

Edge case: targets selected while incoming missile targeting is active, will be deselected when the incoming missiles list is updated.

#### Target Filter Preset

This component allows to save and load presets for target filter configuration (a window opened by by **"TGT"** button on the right side of the maximized map). Loading filter preset when some targets are already selected will deselect not matching targets.  

Component state is controlled by `Target Filter Preset - Enabled` setting in plugin settings (restart the game to apply changes).  

Number of target filter presets is defined by `Target Filter Preset - Number` setting (restart the game to apply changes). Keys are bound in `"Target Filter Preset - Slot #"` settings.  

Presets are persistent: they are saved to `json` config file `NOTT/config/TargetFilterPreset.cfg` when modified and loaded on mission load.

If `Target Filter Preset - Maximize Targetable Markers - Enabled` is enabled, markers of units eligible for targeting by target filter configuration will be always maximized regardless of HUD settings (and when gears are deployed).

#### Alternative Target Selection

This component implements a simpler HMD target selection algorithm, which will select units:
 * that pass targeting filters;
 * that are within target selection distance (if it is set); and
 * whose HMD markers are on screen, enabled, and within target selection cone centered around direction designated by target selection marker.
 
 Clicking "Target Select" key engages single target selection/picking mode; holding "Target Select" key engages "paint" selection mode.

`Alternative Target Selection - Enabled` setting controls the state of the component (restart the game to apply changes).

`Alternative Target Selection - Camera FOV Fraction` sets the fraction of camera vertical FOV to be used as apex angle (aperture) of selection cone. I.e., if this fraction is 0.1 and FOV is 90 degrees, aperture of selection cone will be 9 degrees, and units 4.5 degrees to the left, right, up, or down relative to target selection marker will be considered for selection.  
`Alternative Target Selection - Max Distance` sets the max distance to unit that will be considered for selection, measured in meters. Set to 0 do select targets (with enabled HMD markers) at any distance. **Note:** `HMD Declutter` component indirectly controls target selection algorithm by setting HMD marker draw distance (units farther away will have their HMD markers disabled and thus won't be considered for selection).  
`Alternative Target Selection - Selection Mode` controls behaviour of single target selection mode. If set to 'distance', unit closest to camera will be selected. If set to 'angle', unit whose HMD marker is closest to target selection marker will be selected.  
If `Alternative Target Selection - Pick Active` is enabled, when no target can be selected in single target selection mode, best matching target from already selected ones becomes active target.  
If `Alternative Target Selection - Show Unit Info` is enabled, unit type and distance info will be displayed on HMD when target selection marker hovers above HMD marker of said unit. This essentially recreates "ally info" feature introduced in NO 0.34, but for markers of targetable units. Restart the game to apply changes. **Note:** if this setting is enabled, set `Alternative Target Selection - Selection Mode` to `angle` and enable `UI Adjustments - Disable Ally Info` to get expected behaviour.  
If `Alternative Target Selection - Show Unit Info For Selected Markers` is enabled, info will be displayed when target selection marker hovers above HMD marker of selected unit (except active target marker, which has default game target info). Disable this setting if two similarly looking unit infos cause confusion.

#### Alternative Map Target Selection

This component implements alternative way to select units on the maximized map (because selection radius of the built-in algorithm is too large).

`Alternative Map Target Selection - Enabled` controls the state of component (restart the game to apply changes).

`Alternative Map Target Selection - Selection Radius` sets selection radius (in pixels) around mouse cursor. To select the unit, click on the unit marker with the key bound to "Select" in controls settings; hold the key to select all units in the selection radius. If `Alternative Map Target Selection - Pick Active` is active, clicking on already selected unit designates is as active target.

### Weapons

#### Advanced Slot Selection extensions

Number of slots is configured by `Advanced Slot Selection - Number` variable (restart the game to apply changes). If `Advanced Slot Selection - Skip Empty Stations` is enabled, empty weapons will be skipped when changing active weapon with `Next Weapon` and `Previous Weapon` keys bound in game controls (it's still possible to select empty weapon by pressing the key bound to its slot in mod settings).

#### Weapon Manager Extensions

`Weapon Manager Extensions - Enabled` control the state of component (restart the game to apply changes).

##### Attack active target

If `Weapon Manager Extensions - Attack Active Target` is enabled, short press on `Fire` button will launch deliverable only at active target; long press will launch salvo of size equal to number of selected targets (or lased targets, see below). Uses `press time` as reference time. Restart the game to apply changes.  
If `Weapon Manager Extensions - Keep Firing` is disabled, will fire only one salvo after 'Fire' key was held for 'press time'. If this setting is enabled, will fire continuously after 'Fire' key was held for twice the 'press time'.

##### Attack only lased targets

If `Weapon Manager Extensions - Attack Only Lased Targets` is enabled, laser guided weapons will launch deliverables only at lased targets in the target list. Restart the game to apply changes.

##### Toggle jammer

If `Weapon Manager Extensions - Toggle Jammer Fire` is enabled, and current weapon is jammer, it will toggled by pressing `Fire` key. Jammer will be turned off on weapon switch or when there are no selected targets.

##### Toggle aim assist independently of flight assist

If `Weapon Manager Extensions - Aim Assist Toggle` is enabled, aim assist (if available, like in A-19 Brawler) can be toggled independently of flight assist by pressing key bound to `Weapon Manager Extensions - Toggle Aim Assist`. Restart the game to apply changes.

### HUD, HMD, MFD

#### HUD Options Preset

This component adds key-bound presets for HUD (actually, HMD) options, just like [Target Filter Preset](#target-filter-preset).

`HUD Options Preset - Enabled` controls the state (restart the game to apply changes).

`HUD Options Preset - Number` sets number of presets (restart the game to apply these settings).

`HUD Options Preset - Slot #` binds key to given preset, long press saves HUD settings to preset, short press loads.

`HUD Options Preset - Enable Builtin Settings` enables built-in switching HUD settings on selecting weapon. When using HUD options presets, this most likely needs to be false. Also, if this setting is false, loaded preset is shared between all HUD modes.

Presets are persistent: they are saved to `json` config file `NOTT/config/HUDOptionsPreset.cfg` when modified and loaded on mission load.

#### Persistent Map Options

This component stores Map Options state (a window opened by by **"MAP"** button on the left side of the maximized map) and applies it on mission load. Changing Map Options updates stored state. Map Options state is stored in dedicated `json` config (`NOTT/config/PersistentMapOptions.cfg`), so it is persistent across game launches.

`Persistent Map Options - Enabled` enables the component (restart the game to apply changes).

#### Ammo Conservation Indicator extensions

This feature of `Ammo Conservation Indicator` component allows to recolor HMD markers of selected targets that are being tracked by deliverables or lased. MFD box markers of selected targets can also be painted with a different color if those targets are being tracked. In addition to that, it's possible to turn of the dot markers under MFD boxes of tracked targets.

Feature state is controlled by `Ammo Conservation Indicator - HMD Markers Color - Enabled` setting. Colors are controlled by color settings under `Ammo Conservation Indicator` section.

#### Hide Objectives

This component allows to hide objective and airbase markers together with corresponding text on HMD.

Component state is controlled by `Hide Objectives - Enabled` setting.

If this component is enabled, turning off **"OBJ"** button in HUD settings (window opened by **"HUD"** button on the left side of maximized map) turns off objectives and airbase markers and text on HMD, in addition to hiding objective marker and text on the (mini)map.

#### HMD Unit Markers Recolor

A small convenience component that allows to recolor HMD unit markers.

`HMD Unit Markers Recolor - Enabled` controls the state, `HMD Unit Markers Recolor - Friendly|Enemy|Neutral Unit Color` control respective colors.

#### HMD Declutter

This component aims to declutter HMD by introducing marker draw distance and providing options to minimize markers that are supposed to be maximized and to hide markers that are supposed to be minimized. Selected and flashing icons will be drawn at any distance and regardless of marker minimizing and hiding options (see below).

`HMD Declutter - Enabled` controls the state of the component.

If `Target Filter Preset - Maximize Targetable Markers - Enabled` is enabled, targetable markers will be always maximized regardless of the following settings. 

##### HMD markers draw distance

`HMD Declutter - Marker Draw Distances` is a string of marker draw distances, measured in units specified by `HMD Declutter - Unit`. Distance values separator is ";", fraction separator is ".", "0.0" is unlimited distance. Example of the distances string: "0;1000.0;5000;50000". `HMD Declutter - Cycle Marker Draw Distance Up` and `HMD Declutter - Cycle Marker Draw Distance Down` are key bindings for cycling distance up or down the distances list. `HMD Declutter - Report` determines whether changing the marker draw distance will be reported on HMD.

##### Minimizing and hiding HMD markers

If `HMD Declutter - Not Always Maximized` is enabled, no markers will be always maximized by default (currently only aircraft markers are always maximized by game). If `HMD Declutter - Minimize Maximized` is enabled, markers that should be maximized according to HUD settings, will be minimized to dots. If `HMD Declutter - Hide Minimized` is enabled, markers that should be minimized, will be hidden instead. `HMD Declutter - Enemy Minimized Marker Scale` and `HMD Declutter - Friendly Minimized Marker Scale` set scales of enemy and friendly minimized markers respectively.

##### Outdated markers management

`HMD Declutter - Outdated Time` sets base time for outdated marker modifications (in seconds). Assign negative value to disable. `HMD Declutter - Show Outdated Time` shows time during which the marker was outdated besides it. `HMD Declutter - Hide Outdated Marker` hides outdated marker after `Outdated Time` has expired. `HMD Declutter - Set Outdated Icon` sets all outdated markers to "?" icon, not just selected ones. If positive, `HMD Declutter - End Outdated Marker Opacity` makes outdated marker more and more transparent as it reaches expiration time (set to negative value to disable, max opacity is 1).

##### Maximizing and recoloring markers of player-owned deliverables

If `HMD Declutter - Maximize Own Missiles` setting is enabled, HMD markers of player-owned guided deliverables (missiles, bombs, and guided shells) will be maximized.

If `HMD Declutter - Colorize Own Missiles` setting is enabled, HMD markers of player-owned guided deliverables will be colorized with `HMD Declutter - Own Missiles Color` and `HMD Declutter - Own Missed Missiles Color`. (Mini)map icons of these deliverables will inherit color of corresponding HMD markers.

If `HMD Declutter - Always Draw Own Missiles` is enabled, HMD markers of player-owned deliverables will be drawn regardless of current markers draw distance.

If `HMD Declutter - Include Derived Missiles` is enabled, missiles (and other deliverables) launched by player-owned units will be counted as belonging to player and colorized. If this setting is disabled, only deliverables launched by player will be colorized.

`HMD Declutter - Own Missiles Color` sets color of HMD markers and map icons belonging to own missiles.; `HMD Declutter - Own Missed Missiles Color` sets color of HMD markers and map icons belonging to own missiles that have missed target.

`HMD Declutter - Own Missiles Scale` sets the scale of HMD markers designating player-owned missiles.

!HMD Declutter - Own Missiles Map Scale` sets the cale of map icons belonging to own missiles if HMD markers of these missiles are maximized.

`HMD Declutter - Flash Before Impact Time` sets time in seconds before impact when deliverable marker starts flashing (set to negative value to disable).

**Note**: this feature correctly recognizes missiles that belong to player and are still in flight after player has respawned.

##### Maximizing and recoloring markers of player-owned units

Basically, the same as above feature for player-owned deliverables, but applied to player-owned units.

If `HMD Declutter - Maximize Own Units` setting is enabled, HMD markers of player-owned units will be maximized.

If `HMD Declutter - Colorize Own Units` setting is enabled, HMD markers of player-owned units will be colorized with `HMD Declutter - Own Units Color`. (Mini)map icons of these deliverables will inherit color of corresponding HMD markers.

`HMD Declutter - Own Units Scale` sets the scale of HMD markers designating player-owned units.

`HMD Declutter - Own Units Map Scale` sets the scale of map icons belonging to own units if HMD markers of these units are maximized.

**Note**: this feature correctly recognizes units that belong to player after player has respawned.

#### Early Missile Warning System (EMWS)

This component allows to display missile info and draw notch indicator on HMD, and to draw notch line on map for known enemy missiles that fly in general direction of players' aircraft. This component essentially semi-automates the tactic of looking at maximized map and figuring out which missiles have targeted player.

Different configurations for different missile seeker types are supported.

`EMWS - Enabled` setting controls the state of component (restart the game to apply changes).

`EMWS - Distance Unit` sets distance measurement unit.

If `EMWS - Process Only Enemy Missiles` is enabled, only enemy missiles will be processed (enabled by default).

If `EMWS - Hide On Missile Warning` is enabled, missile warnings of possible incoming missiles will be hidden when missile warning for actually incoming missile is active.

`EMWS - Number of Entries` sets the number of entries (restart the game to apply changes). If negative, or 0, will create default entry for ARH and SARH missiles.

Per-entry settings are the following:

 * `EMWS - Entry # - Seeker Types` specifies regex patterns of seeker types, separated by ';'. A pattern can be just a part or the whole of seeker name ("SARH", "ARH", "IR", etc.)
 * `EMWS - Entry # - Angle` sets doubled angle between missile velocity vector and direction from missile to players' aircraft. 
 * `EMWS - Entry # - Distance` sets maximal distance from missile to players' aircraft (set to negative value to disable this check).
 * `EMWS - Entry # - Item Color` color of missile info text, notch indicator and line, as well as the color of missile HMD marker.
 * `EMWS - Entry # - Flash Marker` will make missile HMD marker flash if enabled.
 * `EMWS - Entry # - Show Notch Line` will show notch line on the (mini)map if enabled.
 * `EMWS - Entry # - Show Vector Line` will show line from missile to players' aircraft on the (mini)map if enabled.
 * `EMWS - Entry # - Show Notch Indicator` will show notch indicator on HMD if enabled.
 * `EMWS - Entry # - Show Text` will show missile seeker type and distance on HMD above minimap if enabled.
 * `EMWS - Entry # - HMD Marker Scale` sets the scale of missile HMD marker.
 * `EMWS - Entry # - Map Icon Scale` sets the scale of missile map icon.

#### HMD Target Arrows

This component fixes position of active target arrow marker and adds an option to display arrow markers for other targets. Primary target arrow is designated by "TARGET" label.

`Target Arrows - Enabled` controls the state (restart the game to apply changes).

`Target Arrows - Number of arrows` set number of target arrows (0 is unlimited, 1 is default primary target arrow). `Target Arrows - Arrow Color` and `Target Arrows - Arrow Scale` set the color and scale of target arrows. But if `Target Arrows - Match Marker Color` is enabled, target arrow color (and the color of "TARGET" text for main target arrow) will match the color of corresponding target marker (useful if `Ammo Conservation Indicator - HMD Markers Color - Enabled` is enabled).

#### Target Velocity Indicator

This component adds the marker on HMD showing velocity vector of the current (active) target relative to current cockpit view. This vector is represented by a marker placed at the offset from marker of the current target. "x" marker means target moves toward the player aircraft, "o" - away from it. The size of offset depends only on the lateral movement of target and is not scaled by distance. The indicator and dots connecting it to target marker inherit current color of said marker.

`Target Velocity Indicator - Enabled` controls the state (restart the game to apply changes).

`Target Velocity Indicator - Max Speed` is the maximum speed (velocity magnitude; in km/h) for maximum offset of marker. `Target Velocity Indicator - Max Length` is the maximum offset of velocity marker (in pixels) for maximum speed. `Target Velocity Indicator - Dot Step` is distance between the dots connecting target and velocity markers.

#### HUD Center Direction

This small component adds an arrow pointing to HUD center.

`HUD Center Direction - Enabled` controls the state (restart the game to apply changes), `HUD Center Direction - Arrow Color` and `HUD Center Direction - Arrow Scale` set arrow color and scale.

#### Third Person HUD

This components enables HUD in third person mode (when camera is in "orbit" or "chase" modes).

`Third Person HUD - Enabled` controls the state (restart the game to apply changes).

If `Third Person HUD - HUD Roll - Enabled` is enabled, HUD will pivot with aircraft roll. If `Third Person HUD - HUD Bound To Screen - Enabled` is enabled, HUD will stay at `Third Person HUD - HUD Screen Offset` position relative to screen center. If `Third Person HUD - Set Target Designator Position - Enabled` is enabled, target designator will be placed at `Third Person HUD - Target Designator Screen Offset` from screen center.

#### UI Adjustments

This component adds various UI adjustments and fixes.

`UI Adjustments - Enabled` controls the state (restart the game to apply changes).

##### Fonts

Can adjust font sizes of various text fields:

 * (Mini)map
     + target marker info and tooltip
     + objective marker text
     + grid labels
 * HUD
     + time of flight
     + missile ranges
     + nozzle gauge
     + wing angle
 * HMD
     + notch indicator text

Numerical settings in `UI Adjustments` config section control text sizes. Setting font size to -1 is interpreted as "do not change size of this font".  

##### Fixes

All these fixes require to restart the game after enabling or disabling to apply changes.

`UI Adjustments - Fix Map Icon Color` fixes (mini)map icon colors after unit selection and deselection.  

`UI Adjustments - Fix Sticky Rearmer Display` fixes rearmer display (ammo info) sticking on screen when attached marker goes out of screen bounds.  

`UI Adjustments - Fix Multiple Rearmer Displays` fixes multiple rearmer displays being created for one HMD unit marker.  

`UI Adjustments - Fix HMD Unit Marker Selection` disallows selecting already selected HMD unit marker and deselecting already deselected one.  

`UI Adjustments - Fix Stale Target` removes target, if any, from target list (accessible by `TGT` button on the right side of maximized map) when player aircraft is spawned. Otherwise such stale target could be removed only if there are other targets on target list.

`UI Adjustments - Fix Target Deselection Sound` fixes uncomfortably loud sound on mass target deselection, i.e. when applying new target filter preset (restart the game to apply changes).  

`UI Adjustments - Fix HUDCargoState HUDFixedUpdate` fixes NRE in HUDCargoState.HUDFixedUpdate() (restart the game to apply changes).  

##### Other

`UI Adjustments - Disable Ally Info` disables ally aircraft info feature added in NO 0.34.  

If `UI Adjustments - Colorize Player-related Messages` is enabled, player name and names of player-owned munitions and vehicles in message feed will be colorized with `All Clear` color (restart the game to apply changes). (restart the game to apply changes).

If `UI Adjustments - Center On Jump Map` is enabled, pressing `Jump Map` key when map is maximized and players aircraft is spawned will center map on cursor coordinates (restart the game to apply changes).

If `UI Adjustments - Airbase Overlay - Always Display Glidepath` is enabled, airbase overlay will display glidepath and runway borders even for aircraft with vertical landing, like helicopters and VTOLs (restart the game to apply changes).

If `UI Adjustments - Airbase Overlay - Ignore Runway Limits` is enabled, runway landing speed and size limits will be ignored when airbase overlay selects which runway to display glidepath and borders for (restart the game to apply changes). Enabling this setting will, for example, display glidepath and runway borders when landing on K92 or Dustbowl in Ifrit.

If `UI Adjustments - Display NOAutopilot GCAS Chevron on HMD` is enabled, [NOAutopilot](https://github.com/qwerty1423/no-autopilot-mod/) Ground Collision Avoidance System (GCAS) Chevron will be displayed on HMD instead of HUD (restart the game to apply changes).

#### Loadout Preview extensions

If Loadout Preview cannot be displayed on MFD for specific aircraft, it still can be displayed on HMD.

Loadout Preview can be disabled for specific aircraft: `Loadout Preview - Disabled For` setting is a sequence of aircraft name patterns, separated by ';'. Aircraft name pattern is a regular expression pattern, so it can be full or partial aircraft name (i.e. 'CI-22' or 'Cricket', or 'CI-22 Cricket'). Respawn to apply changes.

Loadout Preview also can be explicitly displayed on HMD for specific aircraft, even if `Loadout Preview - Send To HMD` setting is disabled. `Loadout Preview - Send To HMD For` setting is a sequence of name patterns of such aircraft, format as above. Respawn to apply changes.

`Loadout Preview - Send To HMD - Font Size` sets HMD Loadout Preview font size. `Loadout Preview - Send To HMD - Main Color` sets HMD Layout Preview text and border color. `Loadout Preview - Send To HMD - Background Color` sets HMD Layout Preview background color.

#### MultiLevel Radial Menu

This component makes adds layers with NOTT+ commands to built-in main and weapons radial menus. This should help people playing NO with gamepads utilize full functionality of the mod.

Menu layers are selected by input on `Zoom View` axis. Additional command layers are configurable from mod configuration menu; sensible defaults are provided. Doubleclicking `Radial Menu` or `Weapon Wheel` key resets corresponding menu to default.

`MultiLevelMenu - Enabled controls the state of component (restart the game to apply changes).

`MultiLevelMenu - Number of Levels in Main Menu` sets the number of levels in Main radial menu (restart the game to apply changes). Set to -1 and restart to restore defaults.

`MultiLevelMenu - Main - Level # lists commands for level # of Main radial menu, separated by ';'.

`MultiLevelMenu - Number of Levels in Weapons Menu` sets the number of levels in Weapons radial menu (restart the game to apply changes). Set to -1 and restart to restore defaults.

`MultiLevelMenu - Weapons - Level # lists commands for level # of Weapons radial menu, separated by ';'.

`MultiLevelMenu - Deselection Delay` sets delay after which selected command is deselected if corresponding menu key was not released to invoke the command.

`MultiLevelMenu - Switch Mode` sets level switching mode.  
`Continuous` - levels are switched when accumulated input delta on `Zoom View` axis exceeds `Z Per Level` value.  
`Step` - level is switched if there is input on `Zoom View` axis on current frame, but not on previous.

`MultiLevelMenu - Level Switch Period` sets minimum time period that should pass after switching level before switching level again.

`MultiLevelMenu - Z Per Level` sets accumulated 'Zoom View` axis delta needed to switch level if `Switch Mode` is `Continuous`.

If `MultiLevelMenu - Reset Level` is enabled, menu will be reset to default layer on opening.

If `MultiLevelMenu - Open on Press` is enabled, corresponding menu will be opened on key press, instead of hold.

If `MultiLevelMenu - Show Preview` is enabled, previews for target lists, target filters and HUD options will be shown when radial menu segments that load corresponding items are selected.

`MultiLevelMenu - Default Color`, `MultiLevelMenu - Selected Color`, `MultiLevelMenu - Active Background Color`, `MultiLevelMenu - Inactive Background Color` set corresponding colors (changes will be applied after menu is recreated).

##### Available commands

Default - default menu. Doubleclicking 'Radial Menu' or 'Weapon Wheel' key resets corresponding menu to default.

 * HUD Options
    + `RememberHUDOptions(#)` - Remember HUD options preset #
    + `RecallHUDOptions(#)` - Recall HUD options preset # (Number of presets is set by `HUD Options Preset - Number` setting, so available preset numbers would be from 0 to Number-1.)

 * Target Filter Preset
    + `RememberFilter(#)` - Remember Target Filter preset #
    + `RecallFilter(#)` - Recall Target Filter preset # (Number of presets is set by `Target Filter Preset - Number` setting.)

 * Target List Controller
    + `RememberTargets(#)` - Remember Target List #
    + `RecallTargets(#)` - Recall Target List # (Number of lists is set by `MFD Nav - Extra Key - Number` setting.)
    + `PopTarget` - Pop current target
    + `KeepTarget` - Keep current target
    + `NextTarget` - Next target
    + `PrevTarget` - Prev target
    + `KeepDatalinked` - Keep datalinked targets
    + `KeepByAmmo` - Keep closest targets based on ammo
    + `SortName` - Sort targets by name
    + `SortDist` - Sort targets by distance
    + `KeepTracked` - Keep tracked targets
    + `PopTracked` - Pop tracked targets
    + `KeepSameName` - Keep targets with same name as current target
    + `PopSameName` - Pop targets with same name as current target
    + `KeepLased` - Keep lased targets
    + `PopLased` - Pop lased targets
    + `SelectClosestTarget` - Select closest target
    + `SelectClosestTargets` - Select closest targets
    + `MTSToggle` - MTS engage/disengage
    + `MTSAutoSelect` - MTS AutoSelect on/off

 * MiniMap Zoom
    + `MiniMapZoomUp` - Cycle MiniMap zoom up
    + `MiniMapZoomDown` - Cycle MiniMap zoom down
    + `MiniMapZoomReset` - Reset MiniMap zoom

 * HMD Declutter
    + `HMDMarkerDistanceUp` - Cycle HMD marker distance up
    + `HMDMarkerDistanceDown` - Cycle HMD marker distance down

 * Target Cam Mode
    + `TargetCamModeToggle` - Toggle Target Cam mode

Commands are distributed clockwise starting from top of the menu.

#### Autopilot Menu extensions

This component extends menu used to control [Nuclear Option Autopilot Mod](https://github.com/qwerty1423/no-autopilot-mod), allowing to adjust appearance and placement of this menu.

`Autopilot - Enabled` control the state of the Autopilot Menu component (restart the game to apply changes).

`Autopilot - Main Color` sets label text and border color when Autopilot Menu is displayed on MFD.

If `Autopilot - Send To HMD` is enabled, the Autopilot Menu will be sent to the HMD display (respawn to apply changes).

`Autopilot` - Send To HMD For` specifies aircraft for which Autopilot Menu will be displayed on HMD (regardless of `Send To HMD` setting). Is a sequence of aircraft platform name patterns, separated by ';'. Aircraft platform name pattern is a regular expression pattern, so it can be full or partial aircraft name (i.e. 'CI-22' or 'Cricket', or 'CI-22 Cricket'). Respawn to apply changes.

`Autopilot - Send To HMD - Main Color` sets label text and border color when Autopilot Menu is displayed on HMD.

`Autopilot - Send To HMD - Position X` and `Autopilot - Send To HMD - Position Y` set horizontal and vertical position for the Autopilot Menu when it is displayed on HMD.

`Autopilot - Send To HMD - Font Size` sets label font size when Autopilot Menu is displayed on HMD.

`Autopilot - Altitude Increment`, `Autopilot - Climb Rate Increment`, `Autopilot - Speed Increment`, `Autopilot - Mach Speed Increment`, `Autopilot - Roll Increment`, `Autopilot - Course Increment` set base increments for respective values. When changing value by holding `+` of `-` menu button, increment will increase tenfold after 10 successive increments.

This component uses the value of `Common - Press Delay` setting as base delay: hold delay is equal to this value; navigating menu and changing values by holding key happens at the period of half of this value.

#### Customize Missile String

This component allows to customize missile string in inbound missiles list entry and below notch indicator.

`Customize Missile String - Enabled` controls the state of component (restart the game to apply changes).

`Customize Missile String - Entry Format String` sets format string for inbound missiles list entry.

`Customize Missile String - Notch Format String` sets format string for notch indicator.

Available parameters:
  *  `{name}` - missile name;
  *  `{seeker}` - missile seeker type;
  *  `{distance}` - distance to missile;
  *  `{tti}` - time to impact in seconds.

\n is newline.

#### Delivery Checker Extensions

If `Delivery Checker - Show TTI` is enabled, target camera feed will display up to 4 time-to-impact values for missiles and bombs each.

### CM & Weapon Display extensions

CM & Weapon display can be disabled for specific aircraft: `CM & Weapon Display - Disabled For` setting is a sequence of aircraft name patterns, separated by ';'. Aircraft name pattern is a regular expression pattern, so it can be full or partial aircraft name (i.e. 'CI-22' or 'Cricket', or 'CI-22 Cricket'). Respawn to apply changes.

CM & Weapon display displays chaff amount for QoL CI-22 Cricket and UH-90 Ibis.

### Target and Landing Camera

#### Target Camera Mode

This component allows MFD Target Camera to be toggled between looking at all selected targets (the default behavior) and looking at active target only.

Component state is controlled by `Target Cam Mode - Enabled` setting (restart the game to apply changes), mode toggle key is bound to `Target Cam Mode - Toggle Mode Key`.

#### Dynamic Landing Camera

This component enables to pivot Landing Camera towards velocity vector.

`Dynamic Landing Cam - Enabled` controls the state of component (restart the game to apply changes).

`Dynamic Landing Cam - Keep On After Touchdown - Enabled` keeps landing camera on after touchdown and after spawning on ground. `Dynamic Landing Cam - Rotate - Enabled` makes landing camera rotate towards velocity vector at `Dynamic Landing Cam - Rotation Speed` and within `Dynamic Landing Cam - Tilt Limits` and `Dynamic Landing Cam - Pan Limits`. `Dynamic Landing Cam - Initial Angles` set initial landing camera tilt and pan angles, `Dynamic Landing Cam - Landing Cam FOV` sets landing camera FOV. `Dynamic Landing Cam - Deadzone` sets the deadzone angle (in degrees): landing camera won't rotate if angle between velocity vector and camera direction is less that this angle. `Dynamic Landing Cam - Fix A-19 Brawler Landing Cam` fixes A-19 Brawler landing cam position: as of NO 0.33.4, it is set inside the aircraft.

#### Target and Landing Camera on HMD

This component allows to move Target and Landing Camera from MFD to HMD.

`HMD Cam - Enabled` controls the state of component (restart the game to apply changes).

`HMD Cam - Position` and `HMD Cam - Size` set camera window position and size on screen. `HMD Cam - Opacity` sets opacity (0 - fully transparent). If `HMD Cam - Suppress MFD Camera` is enabled, cameras output is not displayed on MFD.

### Map and minimap

#### MiniMap Zoom

This component allows to change zoom level of the minimap.

`MiniMap Zoom - Enabled` setting controls the state (restart the game to apply changes).

`MiniMap Zoom - Zoom levels` is a semicolon-separated list of zoom levels with dot (.) acting as fraction separator. Default in-game minimap zoom level is 2.0.  

`MiniMap Zoom - Offset` is an offset from center of the minimap to player aircraft in meters for default zoom level.

Short press on key bound to `MiniMap Zoom - Cycle Zoom Key` cycles zoom levels towards next zoom level, long press restores default zoom level. Short press on key bound to `MiniMap Zoom - Cycle Zoom Down Key` cycles zoom levels towards previous zoom level, long press restores default zoom level.

If `MiniMap Zoom - Report` is enabled, minimap zoom level changes are reported on HMD.  

If `MiniMap Zoom - Independent Zoom Levels` is enabled, minimized and maximized states of dynamic map will have independent zoom levels.

If `MiniMap Zoom - Save Maximized Map Position` is enabled, center position of maximized map will be saved when minimizing and restored back when maximizing.

If `MiniMap Zoom - Run CenterMinimizedMap Patch in Prefix` is true, DynamicMap.CenterMinimizedMap() patch will be run in Prefix() and skip original function; if false - in Postfix(). Included for debug purposes, is supposed to be true; set to false to try to fix incompatibility with other mods.

#### Map Target Arrows

This component adds (mini)map arrows that point to selected targets that are out of map bounds. Active target is distinguished by different arrow and marker color and, optionally, by "T" marker.

`Map Target Arrows - Enabled` controls the state (restart the game to apply changes).

`Map Target Arrows - Arrow Scale` sets the arrows scale (relative to target arrow on HMD), `Map Target Arrows - Selected Color` and `Map Target Arrows - Active Color` set colors for selected targets and the active target respectively, `Map Target Arrows - Show T` determines whether to show "T" near the active target arrow.

### View control

#### Free Look Toggle

This component allows to toggle the free look mode and adds some view-related actions.

Uses keys bound to `Free Look` and `Center View` in game control bindings menu. Virtual Joystick is assumed to be enabled. `Target Padlock` option in Gameplay settings tab is also supposed to be enabled.

Clicking `Free Look` key toggles free look mode on (when mouse controls camera) and off (when mouse controls player aircraft). Holding `Free Look` key temporarily sets view to forward and disables free look on press and restores previous view direction on release.  
Clicking `Center view` key switches between target padlock mode and previous view (if `Target Padlock` option in Gameplay settings tab is enabled, otherwise does nothing). Holding `Center view` key sets view to forward.

In the mod settings, `Free Look Toggle - Enabled` controls the state of the component (restart the game to apply changes).

If `Free Look Toggle - Disable Free Look In Padlock` is enabled, free look mode is automatically disabled upon leaving padlock mode. Similarly, if `Free Look Toggle - Disable Free Look In Forwardlock mode` is enabled, free look mode is disabled upon leaving Forwadlock mode. If `Free Look Toggle - Disable Free Look On Centering View` is enabled, free look mode is disabled after centering view.

`Free Look Toggle - Center Key`, `Free Look Toggle - Padlock Key`, and `Free Look Toggle - FreeLook Key` provide additional key bindings for corresponding functions.

If `Free Look Toggle - FOV-dependent Sensitivity - Enabled` is enabled, mouse sensitivity in free look mode depends on the current FOV: the lesser the FOV, the lesser the sensitivity. If `Free Look Toggle - Report` is enabled, free look and padlock state changes are reported on HMD.

`Free Look Toggle - Centering Position Multiplier` sets vector that is multiplied component-by-component by cockpit head position vector on centering view. This can be used to retain (partially or fully) head cockpit position in this event, i.e. set `y` to 1 to preserve head elevation.

#### Head Axes

This component allows to bind in-cockpit camera view direction and FOV to absolute joystick axes, so that camera pan, tilt and FOV correspond to values of bound axes.

`Head Axes - Enabled` setting controls the state of component.

`Head Axes - Pan Axis`, `Head Axes - Tilt Axis`, and `Head Axes - FOV Axis` bind corresponding axes. Set deadzone to 0 in axis calibration settings.

`Head Axes - Pan Limit` sets pan (horizontal) view limit in degrees (- `Pan Limit` corresponds to negative limit of the bound axis, + `Pan Limit` corresponds to positive limit of bound axis). `Head Axes - Tilt Limit` sets tilt (vertical) view limit in degrees. `Head Axes - Min FOV` and `Head Axes - Max FOV` set FOV limits in degrees; `Head Axes - FOV Speed` sets FOV change speed.

#### Key View Control

This components allows to control cockpit camera with keys.

`Key View Control - Enabled` controls the state (restart the game to apply changes).

`Key View Control - Pan Left`, `Key View Control - Pan Right`, `Key View Control - Tilt Up`, `Key View Control - Tilt Down` are key bindings for pan (horizontal) and tilt (vertical) axes respectively. Short press will change angles by values set by `Key View Control - Pan Step` and `Key View Control - Tilt Step` config settings, long press will gradually change angles with speed set by `Key View Control - Pan Speed` and `Key View Control - Tilt Speed`. If `Key View Control - FOVDependent` is enabled, step and speed will be adjusted by FOV-dependent factor (lesser the FOV, lesser the speed and step). If `Key View Control - Stop At 0` is enabled, changing pan and tilt angles in steps will stop at zero angle values regardless of step size. 

### Aircraft control

#### Countermeasure Controls extensions

This feature extends `Countermeasure Controls` component by adding support for chaff (used in Primeva 2082) and enabling to actually bind countermeasure activation to a key.

`Countermeasure Controls - Enabled` controls the state of the component (restart the game to apply changes).

Keys bound to `Countermeasure Controls - Flares`, `Countermeasure Controls - Jammer`, and `Countermeasure Controls - Chaff` control corresponding countermeasures. If `Countermeasure Controls - Activate` is enabled, the selected countermeasure will be activated upon key press and deactivated on key release; if this setting is disabled, the key press will just switch to the bound countermeasure.

#### Virtual Joystick Extender

This component reimplements in-game Virtual Joystick to make it more customizable and convenient to use. Supports several modes of operation, response curves, general and per-mode sensitivity settings.

Input axes are `x`, `y`, and `z`; output axes are `yaw`, `pitch`, and `roll`.

Data flow is the following:

  * input axis value delta is first multiplied by general input sensitivity
  * then by per-mode input sensitivity
  * modified value delta is processed by per-mode acceleration (aka dynamic) curve
  * and accumulated into modified value
  * the modified value is mapped to output axis
  * mapped value is processed by response (aka static) curve
  * processed value is multiplied by per-mode output multiplier
  * finally, this value is assigned to in-game variable responsible for given output axis

Component state is controlled by `Virtual Joystick Extender - Enabled` setting (restart the game to apply changes).

`Virtual Joystick Extender - X Axis`, `Virtual Joystick Extender - Y Axis`, and `Virtual Joystick Extender - Z Axis` bind input axes. Note: may need to disable `Match modifiers exactly` when binding x and y input axes to make Virtual Joystick Extender process them with any modifiers pressed.  

`Virtual Joystick Extender - Input Sensitivity` sets general sensitivity for input axes. Note: virtual joystick sensitivity in game controls is not used. Also note, that `Invert virtual joystick pitch` setting in game controls is not used. To invert an input axis, set its sensitivity to negative value.

##### Mode-related settings

`Virtual Joystick Extender - Number of Modes` sets number of Virtual Joystick Extender modes (restart the game to apply changes). If set to -1, default modes will be created after restart. `Virtual Joystick Extender - Default Mode` sets number of default mode.

###### Common

If `Virtual Joystick Extender - Roll Controls Yaw On The Ground - Enabled` is enabled, roll output axis will control aircraft yaw when it is on the ground.

`Virtual Joystick Extender - Input Axes Reset Mode` sets how input axes will be reset upon entering virtual joystick mode. If set to `None`, to action will be taken. If set to `Scale`, values will be multiplied by `Input Multiplier On Entering Mode` of virtual joystick mode being entered. If set to `Recalculate`, values will be recalculated based on current values of output axes and `Output Multiplier` of virtual joystick mode being entered (most convenient).

###### Per-mode

Each mode has following settings:

  * `Virtual Joystick Extender - Mode # - Name` sets the mode name displayed in on-screen messages.  
  * `Virtual Joystick Extender - Mode # - Engage Key` engages this mode on press, restores previous mode on release.  
  * `Virtual Joystick Extender - Mode # - Toggle Key` toggles between this mode and previous mode on press.  
  * `Virtual Joystick Extender - Mode # - Yaw|Pitch|Roll Axis Mapping` sets which input axis maps to which output axis.
  * `Virtual Joystick Extender - Mode # - Input Sensitivity` sets mode-specific sensitivity for input axes.  
  * `Virtual Joystick Extender - Mode # - Input Multiplier On Entering Mode` sets multipliers applied to input axes upon entering mode if `Virtual Joystick Extender - Input Axes Reset Mode` general setting is set to `Scale`.  
  * `Virtual Joystick Extender - Mode {i} - Dynamic Curvature` sets curvature parameter of `dynamic` curves that process value deltas of input axes. Essentially, this implements input acceleration that depends on distance given axis has moved in one (positive or negative) direction; acceleration is reset when direction changes. The higher the curvature, the slower output axis value changes at the beginning of movement, and the faster it accelerates. Zero curvature means no acceleration.  
  * `Virtual Joystick Extender - Mode # - Curvature` sets curvature parameter of `static` curves that modify values of output axes. Essentially, this implements traditional response curves. The higher the curvature, the slower axis value changes near 0. Zero curvature means flat response curve.  
  * `Virtual Joystick Extender - Mode # - Output Multiplier` sets multipliers applied to modified values of output axes.

Shape of `dynamic` and `static` curves mentioned above is defined by equation: f = Curvature\*t<sup>3</sup> + (1 - Curvature)\*t. 

##### General settings

Key bound to `Virtual Joystick Extender - Toggle Key` toggles the state of virtual joystick.

###### Deflection

`Virtual Joystick Extender - Max Deflection` sets maximum length of virtual joystick on-screen vector (in pixels).

Pressing and releasing `Virtual Joystick Extender - Max Mode Key` engages and disengages maximum virtual joystick deflection mode.

`Virtual Joystick Extender - Centering Deflection` sets the virtual joystick vector centering threshold length: virtual joystick centering will be activated only when vector length is less than this value. Set to negative value to make centering active at any deflection. `Virtual Joystick Extender - Centering Speed` sets the centering speed, centering force set in game 'Controls' menu is not used.

`Virtual Joystick Extender - Decay Mode` sets virtual joystick decay mode when virtual joystick control is disabled by opening map, leaderboard, or radial menu. Available modes are `None`, `Instant`, and `Gradual`. `Virtual Joystick Extender - Decay Speed` sets decay speed for `Gradual` mode.

Short press on `Virtual Joystick Extender - Reset Key` sets Z input axis to 0; long press sets all input axes to 0.

`Virtual Joystick Extender - Limits Shape` describes how `Max Deflection` is used to limit the value of virtual joystick vector, defined by `x` and `y` input axes. If set to 'Circle', length of such vector is capped by `Max Deflection` (game default). If set to `Square`, values of both `x` and `y` axes are clamped independently within [-`Max Deflection`; `Max Deflection`]. Value of `z` axis is always clamped within [-`Max Deflection`; `Max Deflection`].

###### Input damping

This feature allows to decrease virtual joystick input sensitivity when using fixed gun and bore direction is close to direction to aiming point (so, i.e., aiming lag pip overlaps target).

`Virtual Joystick Extender - Damping Angle` sets input damping "FOV": if angle between direction from aircraft to aiming point and direction of gun bore is less than half of value of this setting, input damping will be activated. Set to negative value to disable.

`Virtual Joystick Extender - Damping Sens` sets minimal value of input sensitivity multiplier, applied when angle between direction from aircraft to aiming point and direction of gun bore is 0.

`Virtual Joystick Extender - Damping Curvature` sets damping curve curvature (0.0 - flat, 1.0 - cubic). **Note**: positive value interferes with A-19 Brawler gun aim assist, causing sideways wobble.

###### Execution control

If `Virtual Joystick Extender - Run In Graphics Update - Enabled` is enabled, code that reads and accumulates mouse input will run in graphics update loop, instead of physics update loop (idea borrowed from [Pauel's Random Fixes](https://github.com/pauel3312/PauelsRandomFixes)). This presumably fixes virtual joystick sensitivity issues on low FPS, but in my case effect was the opposite.

`Virtual Joystick Extender - Fixed DT` sets the fixed value of multiplier that is used in input calculation (in vanilla this multiplier depends on time delta and its max value is 3). Set to 0 or negative to use vanilla algorithm.

###### Visual representation

`Virtual Joystick Extender - Vector Placement` sets whether virtual joystick vector should be displayed on HUD (default), on HMD, or be turned off.

If `Virtual Joystick Extender - Shift Vector - Enabled` is enabled, virtual joystick vector will be shifted by z input axis value (beta feature, may not work as expected).

###### Enabled or disabled in ...

If `Virtual Joystick Extender - Control In Third Person Mode - Enabled` virtual joystick will control aircraft in third person mode. See [Third person HUD](#third-person-hud).

If `Virtual Joystick Extender - Disable in Invalid Camera Mode - Enabled` is enabled, virtual joystick will be enabled only when camera mode is 'cockpit', and, if `Control In Third Person Mode` is enabled, 'orbit' or 'chase'.

If `Virtual Joystick Extender - Disable In Free Look - Enabled` is enabled, virtual joystick will be disabled when free look mode is enabled.

If `Virtual Joystick Extender - Disable On Maximized Map - Enabled` is enabled, virtual joystick will be disabled when map is maximized.

If `Virtual Joystick Extender - Disable On UI Interaction - Enabled` is enabled, virtual joystick will be disabled when radial menu is active or leaderboard is open.

#### Key Axes

This components implements better control of output axes (`pitch`, `roll`, `yaw`, `brakes`, `throttle`, `custom axis 1`, as well as `zoom`) with keys and encoder input axes.

State of component is controlled by `Key Axes - Enabled` setting (as always, restart the game to apply changes).

##### Input bindings

Key bindings and curves settings are under `Key Axes` section. Each axis is assigned a pair of keys and an input axis.

Pressing `Decrease` key will decrease axis value, pressing `Increase` will increase it, both at the base `Build-Up speed` modified by response curves (see below). When both keys are pressed, if `Two Key Reset` is enabled, axis value is reset to `Default Value`, if it is disabled, the currently attained axis value is maintained. When both keys are released and there is no input from input axis, if `Decay Speed` parameter (see below) is greater than 0 and after `Decay Delay` time has passed (measured in seconds), the axis value will decay to `Default Value`, otherwise the attained axis value is maintained until further input. `Initial Value` sets the initial value of output axis. 

`Axis` variable binds encoder input axis to output axis.

Short press on key bound to `Key Axes - Yaw Pitch Roll Axes Reset Key` resets `yaw` axis to `Default value`, long press resets `yaw`, `pitch`, and `roll` axes. Short press on `All Axes Reset Key` resets all axes to their default values.

###### Throttle Axis: confirming airbrake deployment

If `Key Axes - Confirm Airbrake Deployment` is set to `Press`, airbrake deployment must be confirmed by releasing and pressing 'Decrease Throttle' key when throttle is zero. If set to `Hold`, airbrake will be engaged while throttle is zero and `Decrease Throttle` key is held, and disengaged after the key has been released. Input from bound axis is processed similarly: negative axis relative value is treated as pressing 'Decrease Throttle', zero or positive value - as releasing the key.

##### Axis response curves

Each axis is also assigned three response curves: `Dynamic` key curve, `Dynamic` input axis curve, and `Static` curve. Shapes of these response curves are defined by so-called depressed cubic equation of the form y = Curvature\*x<sup>3</sup> + (1 - Curvature)\*x + DefaultValue . `Curvature` parameters are adjusted by corresponding config variables. 
`Dynamic` key curve determines the rate at which axis value changes depending on the time during which one of controlling keys is pressed: the higher is the `Dynamic Curvature` the slower the axis value changes initially, but the higher it changes later.  
`Dynamic` axis curve determines the rate at which axis value changes depending on the input axis. `Encoder Axis Sensitivity` sets the input axis sensitivity.  
`Static` curve determines how fast the axis value changes around `Default Value`: the higher is the `Static Curvature`, the slower.

##### Technical notes

This component adds output from key (and encoder) controlled virtual axes to output from regular axes bound in in-game "Controls". As the result, these outputs can cancel each other or make total axis value get out of expected range [-1; +1] (fortunately, this does not cause the game crash).

## Compatibility

NOTT+ 0.7.33.0 was tested under Nuclear Option 0.34.2.

### NOAutopilot

Compatible with [NOAutopilot](https://github.com/qwerty1423/no-autopilot-mod) mod by qwerty1423 (5.5.3).

### NO_Optimisation mod by Appulcake

HUD optimization features in [NO_Optimisation](https://github.com/Appulcake/NO_Optimisation) mod by Appulcake (0.34.2.3) conflict with `HMD Declutter` component of NOTT+ by fighting over HMD marker state. This causes rapid blinking of said markers when `HMD Declutter` wants to disable markers beyond current HMD marker draw distance, and NO_Optimisation wants to enable these markers.

Solution: set all settings in `--- Client - HUD ---` section of `NO_Optimisation` configuration to 0 and enable `Show Objective HUD Markers`.

### Pauels Random Fixes mod fork by Appulcake

Disable `FPSBoundMouseFix` feature in [Pauels Random Fixes](https://github.com/Appulcake/PauelsRandomFixes) mod fork by Appulcake (0.34.2.2RC) if using `Sensitivity fix` component of NOTT+, or vice versa.

## On possible "Could not load file or assembly MonoMod.Backports" error

If the mod does not work and the error message *"Could not load file or assembly 'MonoMod.Backports...'"* is reported in `LogOutput.log`, place `MonoMod.Backports.dll` and `MonoMod.ILHelpers.dll` files from the folder `ON_ERROR_PLACE_IN_GAME_FOLDER` within the archive into the Nuclear Option folder. Don't place them into the NOTT folder, as BepInEx will delete them.

## Legal info

This project is an unofficial fan modification not affiliated with, sponsored by, or endorsed by Shockfront Studios. All original Nuclear Option assets and code are Copyright © 2026 Shockfront Studios. Shockfront Studios and Nuclear Option are trademarks of Shockfront Studios. All other trademarks and original mod content belong to their respective owners.
