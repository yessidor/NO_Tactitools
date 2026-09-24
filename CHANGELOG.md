# Nuclear Option Tactical Tools (plus)

[NOTT (plus) Codeberg repository](https://codeberg.org/yessidor/NO_Tactitools). [NOTT (plus) GitHub repository](https://github.com/yessidor/NO_Tactitools).

[Original NOTT repository](https://github.com/clumzy/NO_Tactitools).

# Changelog

## 0.7.33.0

  * UI Adjustments
    + added option to always show glidepath when landing, even for helicopters and VTOLs
    + added option to make airbase overlay ignore runway limits when selecting runway to draw glidepath and borders for
  * HMD Declutter
    + added separate option to enable or disable colorizing markers of player-owned deliverables
    + added options to maximize and/or colorize markers of player-owned units
  * Early Missile Warning System (EMWS)
    + added option to hide missile warning for possible incoming missiles when missile warning for actually incoming missile is active

## 0.7.32.1

  * Delivery Checker
    + information about player-launched deliverables is retained across respawns
  * HMD Declutter, Delivery Checker
    + fixed deliverable TTI calculation

## 0.7.32.0

  * UI Adjustments
    + added option to center maximized map on cursor coordinates by pressing `Jump Map` key when players aircraft is spawned
  * HMD Declutter
    + added option to colorize (or not) deliverables launched by player-owned units
  * Alternative Target Selection
    + fixed distance updates in previewed unit info
  * Delivery Checker
    + capped displayed deliverable TTI by 999 seconds

## 0.7.31.0

  * Customize Missile string
    + added component that allows to customize missile description string in inbound missile list and notch indicator
    + replaces UI Adjustments feature that shows inbound missile TTI
  * Delivery Checker
    + added option to show deliverable TTI
    + refactored hit check
    + fixed to make `M` and `B` labels initially disabled
  * Early Missile Warning System
    + added settings for scales of missile HMD marker and map icon
    + added setting to process only enemy missiles (enabled by default)
    + fixed vector line
    + fixed and refactored to ensure that scale, color, and flashing state of HMD marker and map icon are correctly saved and restored
  * UI Adjustments
    + removed feature that shows inbound missile TTI

## 0.7.30.2

  * UI Adjustments
    + fixed NRE in feature showing inbound missile TTI

## 0.7.30.1

  * Target Filter Preset, HUD Options Preset, Persistent Map Options
    + fixed "File Not Found" exception when corresponding configs are missing

## 0.7.30.0

  * UI Adjustments
    + added option to show inbound missile TTI
    + added option to colorize player-related messages in message feed
  * MultiLevel Radial Menu
    + fixed NRE in encyclopedia
  * Third Person HUD
    + fixed NRE on mission unload

## 0.7.29.0

  * Target List Controller
    + added option to _select_ new targets based on current ammo, instead of filtering already selected targets
    + added option to highlight active target in target list
    + added option to enable or disable extra target entry checkbox functions (previously was always enabled)
  * Target List Controller
    + added option to toggle aim assist independently of flight assist

## 0.7.28.0

  * Persistent Map Options
    + implemented component that stores Map Options state and applies it on mission load
        + changing Map Options updates stored state
        + Map Options state is stored in dedicated config, so it is persistent across game launches
  * HUD Options Preset
    + if `Enable Builtin Settings` is false, loaded preset is shared between all HUD modes (as it was intended)
    + refactored to use json configs
    + supports loading legacy config
  * Target Filter Preset
    + refactored to use json configs
    + supports loading legacy config
  * Early Missile Warning System
    + added another check for stale missile threat visualizations
  * Free Look Toggle
    + added `Centering Position Multiplier` setting
        + on centering view head position coordinates are multiplied by this value
        + can be used i.e. to retain head cockpit elevation when centering view by setting `y` to 1
  * UI Adjustments
    + added target deselection sound fix
        + previously was in Target Filter Preset component and always enabled
    + added fix for NRE in HUD Cargo state
  * Virtual Joystick Extender
    + added dynamic input curves
    + refactored
  * Key Axes
    + refactored

## 0.7.27.1

  * Virtual Joystick Extender
    + fixed centering not applied to `x` and `y` input axes if limits shape was set to square

## 0.7.27.0

  * Virtual Joystick Extender
    + refactored to process `z` input axis independently
    + added virtual joystick limits shape setting
  * UI Adjustments
    + added option to display NOAutopilot GCAS Chevron on HMD (instead of HUD)
  * HMD Cam
    + refactored to make this component work correctly with [F-117A](https://github.com/blacknight2u/nuclear-option-f117) and [WingCommand](https://github.com/GrabowMar/NuclearOption-WingCommand) mods

## 0.7.26.1

  * UI Adjustments
    + added stale target fix
  * HMD Declutter
    + fixed HMD markers being always maximized if `Target Filter Preset - Maximize Targetable Markers - Enabled` was enabled [#6](https://codeberg.org/yessidor/NO_Tactitools/issues/6)
    + fixed blinking of minimized HMD markers
    + fixed incorrect sprites of minimized HMD markers
    + `HMD Declutter - Enemy Minimized Marker Scale` and `HMD Declutter - Friendly Minimized Marker Scale` now apply to all minimized markers (whether minimized by HUD settings or by `HMD Declutter - Minimize Maximized`)
  * Head Axes
    + fixed initialization

## 0.7.26.0

  * Autopilot Menu
    + enabled to place on HMD
    + added settings that allow to adjust colors and placement
    + fixed input: holding keys is now correctly processed
  * Loadout Preview
    + menu display duration is updated live
    + using real time
  * Input Subsystem
    + using real time

## 0.7.25.0

  * MultiLevel Radial Menu
    + added preview for target lists, target filters, and HMD options to be loaded
  * Virtual Joystick Extender
    + added input damping when using fixed guns
  * HMD Declutter
    + colorizing (mini)map icons of player-owned deliverables

## 0.7.24.1

  * MultiLevel Radial Menu
    + fixed to always show Weapon Wheel menu (game show it only if number of different weapons is 3 or more)

## 0.7.24.0

  * MultiLevel Radial Menu
    + added configurable multi-level radial menus with mod commands

## 0.7.23.1

  * Sensitivity Fix
    + added sensitivity settings for remaining camera modes and radial menu

## 0.7.23.0

  * Sensitivity Fix
    + new component that fixes FPS-bound mouse sensitivity in various camera modes, while controlling maximized map, and while using built-in virtual joystick
  * MiniMap Zoom
    + added setting that allows to save position of maximized map when minimizing and restore it when maximizing
  * Key Axes
    + setting 'Use Throttle Axis Negative Region' in game settings is not required anymore, the component should work with any state of this option

  Tested under NO 0.34.2.

## 0.7.22.0

  * Virtual Joystick Extender
    + added setting that controls how input axes are reset when changing mode
    + added setting that replaces time-based multiplier used in input calculations with fixed value
    + added settings that control when virtual joystick is enabled or disabled
  * Early Missile Warning System (EMWS)
    + added options to customize threat indicator for different missile seeker types

## 0.7.21.5

  * Artificial Horizon, Slip Indicator, Bank Indicator
    + added config setting for platform name patterns
  * Unit Icon Recolor
    + added config setting for unit name patterns
  * Early Missile Warning System
    + fixed some more NREs

## 0.7.21.1

  * Early Missile Warning System (EMWS)
    + fixed some NREs

## 0.7.21.0

  * Early Missile Warning System (EMWS)
    + new component that allows to draw notch indicator and line for missiles that have likely targeted player
  * Key Axes
    + added airbrake deployment confirmation

## 0.7.20.1

  * Target List Controller
    + added option to select closest friendly units that need ammo
    + updating target info texts on target camera feed even if there is one target selected
    + displaying ammo level of selected friendly unit on target camera feed
    + fixed NRE when trying to disable map icons of downed pilots, but there are not any
  * Key Axes
    + fixed key axes not working in mission editor

## 0.7.20.0

  * Virtual Joystick Extender
    + completely refactored
    + 3 input axes bindings independent of game input settings
    + user-defined number of modes of operation
    + each mode has input-to-output axes mapping and response curves
    + can specify whether roll axis controls yaw when aircraft is on the ground
    + can move virtual joystick vector from HUD to HMD or disable it at all
    + also can shift virtual joystick vector by z input axis value
    + can run virtual joystick calculations in graphics updates instead of physics (presumably fixes increasing sensitivity on low FPS, but not in my case)
  * Key Axes
    + can specify whether pressing 2 keys bound to controlling axis at once resets axis value or stops changing it
  * Input Subsystem
    + can bind several actions to one key or axis
    + can specify whether modifiers of bound key should be matched exactly (or extra modifiers are allowed)
  * Target List Controller
    + added key-bound function that selects closest target(s) that match targeting filters and (optionally) are within targeting-related distances set by other components
  * MiniMap Zoom
    + added option to independently set zoom levels for minimized and maximized states of map
  * Multiple components
    + updated default values of config settings
  * Fixes
    + fixed weapon switching (thx Appulcake) [#4](https://codeberg.org/yessidor/NO_Tactitools/issues/4)

  Just in case, **backup your config** before updating the mod (`[your steam folder]/steamapps/common/Nuclear Option/BepInEx/config/com.yessidor.NO_Tactitools-plus.cfg`).

## 0.7.19.2

 * Ammo Conservation Indicator
    + optimized to process only friendly deliverables
 * Delivery Checker
    + refactored
 * Target List Controller, Alternative Target Selection
    + updated player name retrieval

## 0.7.19.1

 * updated to NO 0.34.1
 * Ammo Conservation Indicator
    + can recolor HMD markers of lased targets
 * HMD Camera
    + implemented component that moves targeting and landing camera from MFD to HMD
 * Alternative Target Selection
    + can disable displaying preview unit info for selected markers (other than active target marker)

## 0.7.18.6

 * Target List Controller
    + fixed missile targeting system so it does not use missile selection angle when current weapon is jammer

## 0.7.18.5

 * Weapon Manager Extensions
    + added setting that allows to keep firing while 'Fire' key is being held
 * Weapon Display
    + fixed for Darkreach
    + fixed flare and jammer labels position and size so they don't overlap (for multiple aircrafts)
 * Target Velocity Indicator
    + now inherits current marker color
 * Multiple components
    + refactored to avoid unneeded target list copies

## 0.7.18.1

 * UI Adjustments
    + fix for sticky rearmer display fix
 * Game Bindings
    + added null reference check

## 0.7.18.0

 * UI Adjustments
    + can disable ally info
    + added more fixes (i.e., for rearmer display)
 * Alternative Target Selection
    + can choose unit selection criteria: by world distance to unit or by screen distance between unit HMD marker and target selection marker
    + added option to display unit type and distance info when target selection marker hovers over targetable unit HMD marker (essentially recreates ally info, but for markers of targetable units)
 * Weapon Display
    + small adjustments in VT-7 Vagrant weapon display
 * various refactorings

## 0.7.17.0

 * adapted to **Nuclear Option 0.34**
 * UI Adjustments
    + added rearmer display fix

## 0.7.16.0

**Last version for Nuclear Option 0.33.4.**

 * Virtual Joystick Extender
    + added horizontal (pan) and vertical (tilt) sensitivity multipliers
 * Loadout Preview
    + added font size setting for HMD Loadout Preview
    + added color settings for HMD Loadout Preview
    + removed transparency settings (transparencies are now controlled by colors)
 * Weapon Manager Extensions
    + added single/salvo attack mode
    + added setting to attack only lased targets with laser-guided weapons
    + added toggle fire mode for jammer
 * Target List Controller
    + added an option for Missile Targeting System to target only missiles that can be intercepted

## 0.7.15.0

 * Virtual Joystick Extender
     + added centering deflection threshold setting: Virtual Joystick centering will be activated only if Virtual Joystick screen vector length is less than this value
 * HMD Declutter
     + added an option to maximize, rescale and recolor HMD markers of player-owned deliverables
 * Loadout Preview
     + can be displayed on HMD even if displaying MFD is not supported
     + can disable or send to HMD for chosen aircrafts
     + fixed so that manual placement settings affect only HMD Loadout Preview
     + removed MFD support for [Aryx](https://github.com/Aryx3D) aircrafts (except MiG-15), because updates kept breaking Loadout Preview
     + Loadout Preview is displayed on HMD for Aryx aircrafts (except MiG-15) instead
 * Weapon Display
     + can disable for chosen aircrafts
     + refactored and optimized
     + added chaff support (used in QoL aka Primeva 2082 Cricket and Ibis)
     + removed support for Aryx aircrafts (except MiG-15), because updates kept breaking Weapon Display
 * Documentation
     + added `CHANGELOG.md`

## 0.7.14.1

**New & changed:**

 * Virtual Joystick Extender
     + added max deflection mode
     + added gradual decay mode
     + added roll mode key binding
     + completely refactored
 * Key Axes
     + added encoder axis binding
     + added reset key bindings
     + added zoom axis binding
 * FreeLook Toggle
     + added TrackIR integration
     + added separate key bindings for padlock and view centering 
     + added various settings
     + refactored
 * Countermeasure Control
     + added support for chaff (makes compatible with QoL aka Primeva 2082)
     + can activate countermeasure by pressing key bound to it
 * Target List Controller
     + added checkbox-related functions
 * UI Adjustments
     + added setting for notch indicator text size
     + setting font size to -1 means "do not change"
 * Documentation
     + reworked **Additional features** section of `README.md`

**Bugfixes:**

  * various bugfixes

## 0.7.13.0

**Changed:**

 * HMD Declutter: added options to dim and hide outdated markers, as well as an option to show outdated time

**Bugfixes:**

 * Various bugfixes

## 0.7.12.0

**New:**

 * Key View Control: implemented alternative option to control cockpit camera with keys
 * Third Person HUD: implemented displaying HUD in third person mode
 * Dynamic Landing Camera: implemened landing camera that pivots toward velocity vector

**Changes:**

 * InputCatcher: axes binding
 * InputCatcher: onReleased and onShortPress actions
 * HMD Declutter: added distance Unit config option
 * HMD Declutter: added NotAlwaysMaximized config option
 * UI Adjustmets: added font sizes for wing and nozzle gauges
 * Virtual Joystick: enabled to control aircraft in third person mode

**Bugfixes:**

 * various bugfixes

## 0.7.11.2

 **Changes:**

 * Loadout Preview: reenabled for [RAH-72 Knockout](https://github.com/Aryx3D/Aryx-RAH-72-Knockout), added for [F-99 Shrike](https://github.com/Aryx3D/Aryx-s-F-99-Shrike)

## 0.7.11.1

 **Changes:**

 * Multiple HMD target arrows: added "Match Marker Color" option

**Bugfixes:**

 * Multiple HMD target arrows: correctly displaying target info text and target arrow
 * Ammo Conservation Indicator: correctly coloring target marker

## 0.7.11.0

**New:**

 * HUD options presets
 * HMD Declutter (HMD markers draw distance, minimizing and hiding HMD markers)
 * Multiple HMD target arrows (also fixes target arrow position)
 * HUD center direction arrow

 **Changes:**

 * Advanced slot selection: added an option to skip empty weapons

## 0.7.10.1

**New:**

 * MiniMap Zoom: Added key binding to cycle to the previous zoom level

**Bugfixes:**

 * various bugfixes


## 0.7.10.0

**New:**

 * Text size adjustments for HUD and map
 * Alternative algorithm for target selection on map
 * Alternative target selection (on HMD): added max target selection distance

**Bugfixes:**

 * Map target arrows: fixed arrows not removing from map after leaving aircraft
 * Map unit icon: fixed erroneous recoloring of icons belonging to untargetable units after target selection/deselection

## 0.7.9.0

**New:**

 * Target List Controller: filtering lased or unlased targets out of selected targets
 * Alternative Target Selection: picking active target out of selected ones by clicking select target key (if no new target can be selected)
 * Map Target Arrows: map and minimap arrows pointing to targets that are out of map bounds

**Changes:**

 * Virtual Joystick Extender: can enable or disable resetting pitch, yaw, and roll axes to 0 when virtual joystick is temporarily disabled

## 0.7.8.6

**Bugfixes:

  * Free Look Toggle component: fixed map dragging not working with LMB pressed

## 0.7.8.5

**Changes:**

 * Pulled and adapted changes from the [upstream](https://github.com/clumzy/NO_Tactitools)
 * Since I was getting "cannot find MonoMod.Backports" runtime error if trying to use string interpolation in code, will supply related MonoMod dlls with the mod just in case

**Bugfixes:**

 * Removed flashing of target info icons on the minimap when loading targt list or switching active target
 * Fixed Free Look Toggle component for all camera states

## 0.7.8.0

**New:**

 * Customizable modifier keys for actions implemented in the mod

**Added:**

 * Updated Loadout Preview and adapted Weapon Display components for [RAH-72 Knockout](https://github.com/Aryx3D/Aryx-RAH-72-Knockout).

## 0.7.7.0

**New:**

 * Free Look Toggle: Added an option to make mouse sensitivity FOV-dependent.

**Added:**

 * Target Filter Preset: Can treat neutral units and buildings as friendly.
 * Advanced Slot Selection: Made number of slots configurable.
 * Adding target selects in on minimap regardless of filter settings.

**Bugfixes:**

 * Target Filter Preset: Fixed applying filters to new markers.

## 0.7.6.1

**Bufgixes:**

 * Removed excessive logging from Free Look Toggle component
 * Fixed Loadout Preview and Weapon Display components for FS-3 Ternion

## 0.7.6.0

**New:** 

 * Adjustable minimap zoom
 * Target velocity indicator
 * Togglable free look

## 0.7.5.4

**Bugfixes:** 

 * Fixed Loadout Preview and Weapon Display plugins for Aryx MC-260 Chimera.

## 0.7.5.3

**Bugfixes:**

 * If WeaponDisplay plugin cannot initialize, it will fail gracefully

## 0.7.5.2

**Bugfixes:**

 * Fixed key-controlled axes
 * Fixed HMD markers erroneously staying maximized

## 0.7.5.0

**New:**

 * Alternative target selection
 * HMD unit marker recoloring

Optimizations.

## 0.7.4.0

**New:**

 * Filtering targets based on the unit name of the current target

## 0.7.3.0

**New:**

 * Incoming missiles targeting

## 0.7.2.1

**Bugfix:**

 * Removed unneeded logging.

## 0.7.2.0

**New:**

 * Active target selection
 * Additional target lists
 * Target filter presets
 * Maximizing markers of targetable units
 * Filtering targets tracked or not tracked by deliverables
 * Ammo Conservation indicator extended
 * MFD Target camera mode toggle
 * Hide objectives and airbase markers with text on HMD
 * Extended Virtual Joystick
 * Better axis control with keys

See Additional features section in README.md for details.
