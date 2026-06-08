using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Rewired;
using NO_Tactitools.Controls;
using NO_Tactitools.UI;
using NO_Tactitools.UI.HMD;
using NO_Tactitools.UI.MFD;
using NO_Tactitools.UI.HUD;
using BepInEx.Bootstrap;

namespace NO_Tactitools.Core {
    [BepInPlugin("com.yessidor.NO_Tactitools-plus", "NOTT-plus", "0.7.11.2")]
    public class Plugin : BaseUnityPlugin {
        public static Harmony harmony;
        public class Modifiers {
            public static ConfigEntry<int> ModifiersNum;
        }
        public static RewiredInputConfig MFDNavEnter;
        public static RewiredInputConfig MFDNavBack;
        public static RewiredInputConfig MFDNavUp;
        public static RewiredInputConfig MFDNavDown;
        public static RewiredInputConfig MFDNavLeft;
        public static RewiredInputConfig MFDNavRight;
        public static RewiredInputConfig MFDNavToggle;
        public static RewiredInputConfig MFDNavSelectByUnitName;
        public static RewiredInputConfig MFDNavSelectLased;
        public static RewiredInputConfig MFDNavMissileTargetingSystem;
        public static ConfigEntry<int> MFDNavExtraKeysNum;
        public static List<RewiredInputConfig> MFDNavExtraKeys;
        public static ConfigEntry<bool> targetListControllerEnabled;
        public static ConfigEntry<bool> tlcSwitchCurrentTargetEnabled;
        public static ConfigEntry<bool> interceptionVectorEnabled;
        public static ConfigEntry<bool> countermeasureControlsEnabled;
        public static RewiredInputConfig countermeasureControlsFlare;
        public static RewiredInputConfig countermeasureControlsJammer;
        public class AmmoConIndicator {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<bool> ColorHMDMarker;
            public static ConfigEntry<bool> ColorMFDBox;
            public static ConfigEntry<bool> DrawMFDDot;
            public static ConfigEntry<Color> HMDTrackedMarkerColor;
            public static ConfigEntry<Color> HMDDefaultMarkerColor;
            public static ConfigEntry<Color> MFDTrackedBoxColor;
            public static ConfigEntry<Color> MFDDefaultBoxColor;
            public static ConfigEntry<Color> MFDTrackedDotColor;
        };
        public class WeaponSwitcher {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<byte> SlotsNum;
            public static List<RewiredInputConfig> Slots;
            public static ConfigEntry<bool> SkipEmptyStations;
        };
        public class TargetFilterPreset {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<int> PresetsNum;
            public static List<RewiredInputConfig> Presets;
            public static ConfigEntry<bool> MaximizeTargetable;
            public static ConfigEntry<bool> NeutralsAreFriendly;
        };
        public class HMDDeclutter {
            public static ConfigEntry<bool> Enabled;
            public static RewiredInputConfig CycleHMDMarkerDrawDistanceUp;
            public static RewiredInputConfig CycleHMDMarkerDrawDistanceDown;
            public static ConfigEntry<string> DistancesString;
            public static ConfigEntry<bool> Report;
            public static ConfigEntry<bool> HideMinimized;
            public static ConfigEntry<bool> MinimizeMaximized;
            public static ConfigEntry<float> EnemyMinimizedMarkerScale;
            public static ConfigEntry<float> FriendlyMinimizedMarkerScale;
        };
        public class HUDOptionsPreset {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<int> PresetsNum;
            public static List<RewiredInputConfig> Presets;
            public static ConfigEntry<bool> EnableBuiltinSettings;
        };
        public class HUDCenterDirection {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<Color> ArrowColor;
            public static ConfigEntry<float> ArrowScale;
        };
        public class TargetArrows {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<bool> MatchMarkerColor;
            public static ConfigEntry<Color> ArrowColor;
            public static ConfigEntry<float> ArrowScale;
            public static ConfigEntry<int> NumArrows;
        };
        // Virtual Joystick Extender
        public class VirtualJoystickExtender {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<Controls.VirtualJoystickExtender.Modes> DefaultMode;
            public static ConfigEntry<bool> ToggleMode;
            public static RewiredInputConfig ToggleStateKey;
            public static RewiredInputConfig YawKey;
            public static RewiredInputConfig RollYawKey;
            public static ConfigEntry<float> YawMultiplier;
            public static ConfigEntry<float> RollYawMultiplier;
            public static ConfigEntry<float> YawCurvature;
            public static ConfigEntry<float> PitchCurvature;
            public static ConfigEntry<float> RollCurvature;
            public static ConfigEntry<Controls.VirtualJoystickExtender.DecayModes> DecayMode;
        }
        // Key axes
        public class KeyAxisData {
            public RewiredInputConfig IncKey;
            public RewiredInputConfig DecKey;
            public ConfigEntry<float> BuildUpSpeed;
            public ConfigEntry<float> DecaySpeed;
            public ConfigEntry<float> DynamicCurvature;
            public ConfigEntry<float> StaticCurvature;
            public ConfigEntry<float> StaticOffset;
            public string Name;
            public string IncKeyName;
            public string DecKeyName;
            public float Min;
            public float Max;

            public KeyAxisData (string name, string incKeyName, string decKeyName, float min, float max) {
                Name = name;
                IncKeyName = incKeyName;
                DecKeyName = decKeyName;
                Min = min;
                Max = max;
            }
        };
        public static ConfigEntry<bool> keyAxesEnabled;
        public static KeyAxisData[] keyAxes;
        public static ConfigEntry<bool> targetCamModeEnabled;
        public static RewiredInputConfig targetCamModeToggleKey;
        public class AltTargetSelection {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<float> FOVFraction;
            public static ConfigEntry<float> MaxDistance;
            public static ConfigEntry<bool> PickActive;
        };
        public class TargetVelocityIndicator {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<float> MaxSpeed;
            public static ConfigEntry<float> MaxLength;
            public static ConfigEntry<float> DotStep;
        };
        public class MiniMapZoom {
            public static ConfigEntry<bool> Enabled;
            public static RewiredInputConfig CycleUpKey;
            public static RewiredInputConfig CycleDownKey;
            public static ConfigEntry<string> Zooms;
            public static ConfigEntry<float> Offset;
            public static ConfigEntry<bool> Report;
        };
        public class MapTargetArrows {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<float> ArrowScale;
            public static ConfigEntry<Color> SelectedColor;
            public static ConfigEntry<Color> ActiveColor;
            public static ConfigEntry<bool> ShowT;
        };
        public class UIAdjustments {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<int> TargetMarkerFontSize;
            public static ConfigEntry<int> ToolTipFontSize;
            public static ConfigEntry<int> ObjectiveMarkerFontSize;
            public static ConfigEntry<int> GridLabelsFontSize;
            public static ConfigEntry<int> BombingStateFontSize;
            public static ConfigEntry<int> MissileStateFontSize;
            public static ConfigEntry<int> LaserGuidedStateFontSize;
        };
        public class AltMapTargetSelection {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<int> SelectionRadius;
            public static ConfigEntry<bool> PickActive;
        };
        public class FreeLookToggle {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<bool> Report;
            public static ConfigEntry<bool> DisableFreeLookInPadlock;
            public static ConfigEntry<bool> FOVDependentSens;
        }
        public static ConfigEntry<bool> weaponDisplayEnabled;
        public static ConfigEntry<bool> weaponDisplayVanillaUIEnabled;
        public static ConfigEntry<bool> unitDistanceEnabled;
        public static ConfigEntry<int> unitDistanceThreshold;
        public static ConfigEntry<bool> unitDistanceSoundEnabled;
        public static ConfigEntry<bool> deliveryCheckerEnabled;
        public static ConfigEntry<bool> MFDColorEnabled;
        public static ConfigEntry<Color> MFDColor;
        public static ConfigEntry<Color> MFDTextColor;
        public static ConfigEntry<bool> MFDAlternativeAttitudeEnabled;
        public static ConfigEntry<bool> unitIconRecolorEnabled;
        public static ConfigEntry<Color> unitIconRecolorEnemyColor;
        public class HMDUnitMarkerRecolor {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<Color> FriendlyColor;
            public static ConfigEntry<Color> EnemyColor;
            public static ConfigEntry<Color> NeutralColor;
        };
        public static ConfigEntry<bool> bootScreenEnabled;
        public static ConfigEntry<bool> artificialHorizonEnabled;
        public static ConfigEntry<float> artificialHorizonTransparency;
        public static ConfigEntry<bool> bankIndicatorEnabled;
        public static ConfigEntry<int> bankIndicatorMaxBank;
        public static ConfigEntry<bool> bankIndicatorShowLabel;
        public static ConfigEntry<float> bankIndicatorTransparency;
        public static ConfigEntry<int> bankIndicatorPositionX;
        public static ConfigEntry<int> bankIndicatorPositionY;
        public static ConfigEntry<bool> hideObjectivesEnabled;
        public static ConfigEntry<bool> slipIndicatorEnabled;
        public static ConfigEntry<float> slipIndicatorTransparency;
        public static ConfigEntry<int> slipIndicatorPositionX;
        public static ConfigEntry<int> slipIndicatorPositionY;
        public static ConfigEntry<float> slipIndicatorDamping;
        public static ConfigEntry<float> slipIndicatorSensitivity;
        public static ConfigEntry<bool> autopilotMenuEnabled;
        public static ConfigEntry<bool> loadoutPreviewEnabled;
        public static ConfigEntry<bool> loadoutPreviewOnlyShowOnBoot;
        public static ConfigEntry<float> loadoutPreviewDuration;
        public static ConfigEntry<bool> loadoutPreviewSendToHMD;
        public static ConfigEntry<bool> loadoutPreviewHMDShowBorders;
        public static ConfigEntry<bool> loadoutPreviewManualPlacement;
        public static ConfigEntry<int> loadoutPreviewPositionX;
        public static ConfigEntry<int> loadoutPreviewPositionY;
        public static ConfigEntry<float> loadoutPreviewBackgroundTransparency;
        public static ConfigEntry<float> loadoutPreviewTextAndBorderTransparency;
        public static ConfigEntry<bool> cameraTweaksEnabled;
        public static ConfigEntry<int> resetCockpitFOVSpeed;
        public static RewiredInputConfig resetCockpitFOV;
        public static RewiredInputConfig lookAtNearestAirbase;
        public static ConfigEntry<bool> ILSWidgetEnabled;
        public static ConfigEntry<float> ILSIndicatorMaxAngle;
        public static ConfigEntry<int> ILSIndicatorPositionX;
        public static ConfigEntry<int> ILSIndicatorPositionY;
        public static ConfigEntry<bool> gameBindingsPatchEnabled;
        public static ConfigEntry<bool> debugModeEnabled;
        internal static new ManualLogSource Logger;
        public static Plugin Instance;

        private void Update() {
            RewiredConfigManager.Update();
        }

        private void Awake() {
            Instance = this;
            // Logger and Debug Mode settings
            debugModeEnabled = Config.Bind("Debug Mode",
                "Debug Mode - Enabled",
                true,
                "Enable or disable the debug mode for logging");
            Logger = base.Logger;
            // Plugin startup logic
            harmony = new Harmony("yessidro.no_tactitools_plus");
            // CORE PATCHES
            harmony.PatchAll(typeof(RegisterControllerPatch));
            harmony.PatchAll(typeof(ControllerInputInterceptionPatch));
            //
            int order = 100;
            //Modifiers
            Modifiers.ModifiersNum = Config.Bind("Modifiers",
                "Modifiers - Number",
                4,
                new ConfigDescription(
                    "Number of modifier buttons (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            for (int i = 0; i < Modifiers.ModifiersNum.Value; i++)
            {
                string name = string.Format("Modifiers - Modifier {0}", i);
                string description = string.Format("Button to act as modifier {0}", i);
                var modifierConfig = new RewiredInputConfig(Config, "Modifiers", name, description, order--, isModifier: true);
                RewiredConfigManager.ModsTracker.AddModifierBinding(modifierConfig);
                InputCatcher.ModsTracker.AddModifierBinding(modifierConfig);
            }
            // MFD Nav
            MFDNavEnter = new RewiredInputConfig(Config, "MFD Nav", "MFD Nav - Enter", "Input you want to assign for MFD Nav - Enter", order--);
            MFDNavBack = new RewiredInputConfig(Config, "MFD Nav", "MFD Nav - Backspace", "Input you want to assign for MFD Nav - Backspace", order--);
            MFDNavUp = new RewiredInputConfig(Config, "MFD Nav", "MFD Nav - Up", "Input you want to assign for MFD Nav - Up", order--);
            MFDNavDown = new RewiredInputConfig(Config, "MFD Nav", "MFD Nav - Down", "Input you want to assign for MFD Nav - Down", order--);
            MFDNavLeft = new RewiredInputConfig(Config, "MFD Nav", "MFD Nav - Left", "Input you want to assign for MFD Nav - Left", order--);
            MFDNavRight = new RewiredInputConfig(Config, "MFD Nav", "MFD Nav - Right", "Input you want to assign for MFD Nav - Right", order--);
            MFDNavToggle = new RewiredInputConfig(Config, "MFD Nav", "MFD Nav - Toggle Screens", "Input you want to assign for toggling MFD screens", order--);
            MFDNavExtraKeysNum = Config.Bind("MFD Nav",
                "MFD Nav - Extra Key - Number",
                10,
                new ConfigDescription(
                    "Number of MFD Nav extra keys (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MFDNavExtraKeys = new ();
            for (int i = 0; i < MFDNavExtraKeysNum.Value; i++) {
                MFDNavExtraKeys.Add(new RewiredInputConfig(Config, "MFD Nav", $"MFD Nav - Extra Key {i.ToString()}", "", order--));
            }
            // Target Recall settings
            MFDNavSelectByUnitName = new RewiredInputConfig(Config,
              "MFD Nav",
              "MFD Nav - Select Targets By Unit Name",
              "Input you want to assign for deselecting targets based on the unit name of current target",
              order--);
            MFDNavSelectLased = new RewiredInputConfig(Config,
              "MFD Nav",
              "MFD Nav - Select Targets By Lased status",
              "Input you want to assign for deselecting targets based on lased status",
              order--);
            MFDNavMissileTargetingSystem = new RewiredInputConfig(Config,
              "MFD Nav",
              "MFD Nav - Missile Targeting System",
              "Input you want to assign for selecting or deselecting incoming missiles",
              order--);
            targetListControllerEnabled = Config.Bind("Target List Controller", //Category
                "Target List Controller - Enabled", // Setting name
                true, // Default value
                new ConfigDescription(
                    "Enable or disable the Target Recall feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    })); // Description of the setting
            tlcSwitchCurrentTargetEnabled = Config.Bind("Target List Controller", //Category
                "Target List Controller - Switch Current Target - Enabled", // Setting name
                true, // Default value
                new ConfigDescription(
                    "Enable or disable switching current (active) target when iterating over selected targets in Target List Controller.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    })); // Description of the setting
            // Interception Vector settings
            interceptionVectorEnabled = Config.Bind("Interception Vector",
                "Interception Vector - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Interception Vector feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // Countermeasure Controls settings
            countermeasureControlsEnabled = Config.Bind("Countermeasures",
                "Countermeasure Controls - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Countermeasure Controls feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 4
                    }));
            countermeasureControlsFlare = new RewiredInputConfig(Config, "Countermeasures", "Countermeasure Controls - Flares", "Input you want to assign for selecting Flares", 2);
            countermeasureControlsJammer = new RewiredInputConfig(Config, "Countermeasures", "Countermeasure Controls - Jammer", "Input you want to assign for selecting Jammer", 0);
            // Weapon Switcher settings
            order = 100;
            WeaponSwitcher.Enabled = Config.Bind("Advanced Slot Selection",
                "Advanced Slot Selection - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Advanced Slot Selection feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            WeaponSwitcher.SlotsNum = Config.Bind("Advanced Slot Selection",
                "Advanced Slot Selection - Number",
                (byte)6,
                new ConfigDescription(
                    "Number of slots (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            WeaponSwitcher.Slots = new();
            for (byte i = 0; i < WeaponSwitcher.SlotsNum.Value; i++)
            {
                string name = string.Format("Advanced Slot Selection - Slot {0}", i);
                string description = string.Format("Input for slot {0}", i);
                WeaponSwitcher.Slots.Add(new RewiredInputConfig(Config, "Advanced Slot Selection", name, description, order--));
            }
            WeaponSwitcher.SkipEmptyStations = Config.Bind("Advanced Slot Selection",
                "Advanced Slot Selection - Skip Empty Stations",
                false,
                new ConfigDescription(
                    "When cycling through weapon stations, stations with no ammo will be skipped.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // Target Filter Preset settings
            order = 100;
            TargetFilterPreset.Enabled = Config.Bind("Target Filter Preset",
                "Target Filter Preset - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Target Filter Preset feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            TargetFilterPreset.PresetsNum = Config.Bind("Target Filter Preset",
                "Target Filter Preset - Number",
                10,
                new ConfigDescription(
                    "Number of target filter presets (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            TargetFilterPreset.Presets = new();
            for (int i = 0; i < TargetFilterPreset.PresetsNum.Value; i++)
            {
                string name = string.Format("Target Filter Preset - Slot {0}", i);
                string description = string.Format("Input for slot {0} (Long press to save, short press to restore)", i);
                TargetFilterPreset.Presets.Add(new RewiredInputConfig(Config, "Target Filter Preset", name, description, order--));
            }
            TargetFilterPreset.MaximizeTargetable = Config.Bind("Target Filter Preset",
                "Target Filter Preset - Maximize Targetable Markers - Enabled",
                true,
                new ConfigDescription(
                    "If enabled, maximize markers of targetable units regardless of HUD settings",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            TargetFilterPreset.NeutralsAreFriendly = Config.Bind("Target Filter Preset",
                "Target Filter Preset - Neutrals Are Friendly - Enabled",
                true,
                new ConfigDescription(
                    "If enabled, neutral units and buildings are considered friendly (hostile otherwise) for the purposes of target selection (game-wide change!).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            //HMD Declutter
            order = 100;
            HMDDeclutter.Enabled = Config.Bind("HMD Declutter",
                "HMD Declutter - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the HMD Declutter feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.CycleHMDMarkerDrawDistanceUp = new RewiredInputConfig(Config, "HMD Declutter", "HMD Declutter - Cycle Marker Draw Distance Up", "", order--);
            HMDDeclutter.CycleHMDMarkerDrawDistanceDown = new RewiredInputConfig(Config, "HMD Declutter", "HMD Declutter - Cycle Marker Draw Distance Down", "", order--);
            HMDDeclutter.DistancesString = Config.Bind("HMD Declutter",
                "HMD Declutter - Marker Draw Distances",
                "0.0",
                new ConfigDescription(
                    "List of HMD marker draw distances, separated by \";\", fraction separator is \".\". 0.0 is unlimited distance.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.Report = Config.Bind("HMD Declutter",
                "HMD Declutter - Report",
                true,
                new ConfigDescription(
                    "Should changing the HMD marker draw distance be reported on HMD.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.MinimizeMaximized = Config.Bind("HMD Declutter",
                "HMD Declutter - Minimize Maximized",
                false,
                new ConfigDescription(
                    "Should maximized markers be minimized.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.HideMinimized = Config.Bind("HMD Declutter",
                "HMD Declutter - Hide Minimized",
                false,
                new ConfigDescription(
                    "Should minimized markers be hidden.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.EnemyMinimizedMarkerScale = Config.Bind("HMD Declutter",
                "HMD Declutter - Enemy Minimized Marker Scale",
                6f,
                new ConfigDescription(
                    "Enemy Minimized Marker Scale (if 'Minimize Maximized' setting is enabled).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.FriendlyMinimizedMarkerScale = Config.Bind("HMD Declutter",
                "HMD Declutter - Friendly Minimized Marker Scale",
                3f,
                new ConfigDescription(
                    "Friendly Minimized Marker Scale (if 'Minimize Maximized' setting is enabled).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            // HUD Options Preset settings
            order = 100;
            HUDOptionsPreset.Enabled = Config.Bind("HUD Options Preset",
                "HUD Options Preset - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the HUD Options Preset feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HUDOptionsPreset.PresetsNum = Config.Bind("HUD Options Preset",
                "HUD Options Preset - Number",
                10,
                new ConfigDescription(
                    "Number of target filter presets (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HUDOptionsPreset.Presets = new();
            for (int i = 0; i < HUDOptionsPreset.PresetsNum.Value; i++)
            {
                string name = string.Format("HUD Options Preset - Slot {0}", i);
                string description = string.Format("Input for slot {0} (Long press to save, short press to restore)", i);
                HUDOptionsPreset.Presets.Add(new RewiredInputConfig(Config, "HUD Options Preset", name, description, order--));
            }
            HUDOptionsPreset.EnableBuiltinSettings = Config.Bind("HUD Options Preset",
                "HUD Options Preset - Enable Builtin Settings",
                false,
                new ConfigDescription(
                    "Enable or disable built-in HUD Options settings saving and loading on mode switch (likely needs to be false if using HUD Options presets).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            // HUD Center Direction settings
            order = 100;
            HUDCenterDirection.Enabled = Config.Bind("HUD Center Direction",
                "HUD Center Direction - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the HUD Center Direction feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HUDCenterDirection.ArrowColor = Config.Bind("HUD Center Direction",
                "HUD Center Direction - Arrow Color",
                Color.yellow,
                new ConfigDescription(
                    "Color of the HUD center direction arrow.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HUDCenterDirection.ArrowScale = Config.Bind("HUD Center Direction",
                "HUD Center Direction - Arrow Scale",
                1.0f,
                new ConfigDescription(
                    "Scale of the HUD center direction arrow.",
                    new AcceptableValueRange<float>(0f, 10f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // Target Arrows settings
            order = 100;
            TargetArrows.Enabled = Config.Bind("Target Arrows",
                "Target Arrows - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Target Arrows feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            TargetArrows.ArrowColor = Config.Bind("Target Arrows",
                "Target Arrows - Arrow Color",
                Color.green,
                new ConfigDescription(
                    "Target arrow color.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            TargetArrows.MatchMarkerColor = Config.Bind("Target Arrows",
                "Target Arrows - Match Marker Color",
                true,
                new ConfigDescription(
                    "If enabled, target arrow color will match target marker color.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            TargetArrows.ArrowScale = Config.Bind("Target Arrows",
                "Target Arrows - Arrow Scale",
                1.0f,
                new ConfigDescription(
                    "Target Arrow Scale.",
                    new AcceptableValueRange<float>(0f, 10f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            TargetArrows.NumArrows = Config.Bind("Target Arrows",
                "Target Arrows - Number of arrows",
                1,
                new ConfigDescription(
                    "Number of target arrows (0 is unlimited, 1 is default target arrow).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // Virtual Joystick Extender
            order = 100;
            VirtualJoystickExtender.Enabled = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Enabled",
                true, 
                new ConfigDescription(
                    "Enable or disable the Virtual Joystick Extender feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.DefaultMode = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Default Mode",
                Controls.VirtualJoystickExtender.Modes.Roll, 
                new ConfigDescription(
                    "Virtual Joystick Extender default mode.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.ToggleMode = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Toggle Mode - Enabled",
                true, 
                new ConfigDescription(
                    "Enable or disable Virtual Joystick Extender mode toggle.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.ToggleStateKey = new RewiredInputConfig(Config,
                "Virtual Joystick Extender",
                "Virtual Joystick Extender - Toggle Key",
                "Key to turn on/off",
                order--);
            VirtualJoystickExtender.YawKey = new RewiredInputConfig(Config,
                "Virtual Joystick Extender",
                "Virtual Joystick Extender - Yaw Mode Key",
                "Key to toggle/switch to yaw mode",
                order--);
            VirtualJoystickExtender.RollYawKey = new RewiredInputConfig(Config,
                "Virtual Joystick Extender",
                "Virtual Joystick Extender - Roll&Yaw Mode Key",
                "Key to toggle/switch to roll&yaw mode",
                order--);
            VirtualJoystickExtender.YawMultiplier = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Yaw Mode Multiplier",
                1.0f,
                new ConfigDescription(
                    "Multiplier of yaw axis value in yaw mode.",
                    new AcceptableValueRange<float>(-10.0f, 10.0f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            VirtualJoystickExtender.RollYawMultiplier = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Roll&Yaw Mode Multiplier",
                0.5f,
                new ConfigDescription(
                    "Multiplier of yaw axis value in roll&yaw mode.",
                    new AcceptableValueRange<float>(-10.0f, 10.0f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            VirtualJoystickExtender.YawCurvature = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Yaw Curvature",
                0.2f,
                new ConfigDescription(
                    "Curvature of yaw axis response curve",
                    new AcceptableValueRange<float>(0.0f, 0.99f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            VirtualJoystickExtender.PitchCurvature = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Pitch Curvature",
                0.2f,
                new ConfigDescription(
                    "Curvature of pitch axis response curve",
                    new AcceptableValueRange<float>(0.0f, 0.99f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            VirtualJoystickExtender.RollCurvature = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Roll Curvature",
                0.2f,
                new ConfigDescription(
                    "Curvature of roll axis response curve",
                    new AcceptableValueRange<float>(0.0f, 0.99f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            VirtualJoystickExtender.DecayMode = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Decay Mode",
                Controls.VirtualJoystickExtender.DecayModes.None,
                new ConfigDescription(
                    "Decay mode of pitch, roll, and yaw axes when virtual joystick control is disabled by opening map, leaderboard, or radial menu.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // Keyboard-controlled axes
            keyAxes = new KeyAxisData[6] {
                new ("Pitch", "Down", "Up", -0.999f, 0.999f),
                new ("Roll", "Right", "Left", -0.999f, 0.999f),
                new ("Yaw", "Right", "Left", -0.999f, 0.999f),
                new ("Throttle", "Up", "Down", 0.0f, 0.999f),
                new ("Brake", "Apply", "Release", 0.0f, 0.999f),
                new ("CustomAxis1", "Up", "Down", 0.0f, 0.999f)
            };
            keyAxesEnabled = Config.Bind("Key Axes",
                "Key Axes - Enabled",
                true, 
                new ConfigDescription(
                    "Enable or disable the Key Axes feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            for (int i = 0; i < keyAxes.Length; i++) {
                var vars = keyAxes[i];
                var name = vars.Name;
                vars.IncKey = new RewiredInputConfig(Config, "Key Axes", string.Format("Key Axes - {0} {1} Key", vars.Name, vars.IncKeyName), "Key binding", order--);
                vars.DecKey= new RewiredInputConfig(Config, "Key Axes", string.Format("Key Axes - {0} {1} Key", vars.Name, vars.DecKeyName), "Key binding", order--);
                vars.BuildUpSpeed = Config.Bind("Key Axes",
                    string.Format("Key Axes - {0} - Build-Up Speed", name),
                    1.0f,
                    new ConfigDescription(
                        "How fast the axis value changes when either of controlled keys is pressed",
                        new AcceptableValueRange<float>(0.0f, 10.0f),
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
                vars.DecaySpeed = Config.Bind("Key Axes",
                    string.Format("Key Axes - {0} - Decay Speed", name),
                    1.0f,
                    new ConfigDescription(
                        "How fast the axis value returns to Default Value if neither key is pressed",
                        new AcceptableValueRange<float>(0.0f, 10.0f),
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
                vars.DynamicCurvature = Config.Bind("Key Axes",
                    string.Format("Key Axes - {0} - Dynamic Curvature", name),
                    0.0f,
                    new ConfigDescription(
                        "How fast the axis value accelerates when either of controlled keys is pressed",
                        new AcceptableValueRange<float>(0.0f, 0.99f),
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
                vars.StaticCurvature = Config.Bind("Key Axes",
                    string.Format("Key Axes - {0} - Static Curvature", name),
                    0.0f,
                    new ConfigDescription(
                        "How fast the axis value changes near Default Value when either of controlled keys is pressed",
                        new AcceptableValueRange<float>(0.0f, 0.99f),
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
                vars.StaticOffset = Config.Bind("Key Axes",
                    string.Format("Key Axes - {0} - Default Value", name),
                    0.0f,
                    new ConfigDescription(
                        "The value axis value will decay to if neither key is pressed and Decay Speed is not 0",
                        new AcceptableValueRange<float>(vars.Min, vars.Max),
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
            }
            // Target Cam Mode
            order = 100;
            targetCamModeEnabled = Config.Bind("Target Cam Mode",
                "Target Cam Mode - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Target Cam Mode feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            targetCamModeToggleKey = new RewiredInputConfig(Config, "Target Cam Mode", "Target Cam Mode - Toggle Mode Key", "", -5);
            // Alternative Target Selection
            order = 100;
            AltTargetSelection.Enabled = Config.Bind("Alternative Target Selection",
                "Alternative Target Selection - Enabled",
                false,
                new ConfigDescription(
                    "Enable or disable the Alternative Target Selection feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AltTargetSelection.FOVFraction = Config.Bind("Alternative Target Selection",
                "Alternative Target Selection - Camera FOV Fraction",
                0.15f,
                new ConfigDescription(
                    "Fraction multiplied by camera vertical FOV to get apex angle of selection cone.",
                    new AcceptableValueRange<float>(0.0f, 0.999f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AltTargetSelection.MaxDistance = Config.Bind("Alternative Target Selection",
                "Alternative Target Selection - Max Distance",
                0f,
                new ConfigDescription(
                    "Max distance to select target at, in meters (set to 0 do select targets at any distance)",
                    new AcceptableValueRange<float>(0.0f, float.PositiveInfinity),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AltTargetSelection.PickActive = Config.Bind("Alternative Target Selection",
                "Alternative Target Selection - Pick Active",
                false,
                new ConfigDescription(
                    "If no new target was selected, pick active target from already selected ones.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            //Target Cam Mode
            order = 100;
            targetCamModeToggleKey = new RewiredInputConfig(Config, "Target Cam Mode", "Target Cam Mode - Toggle Mode Key", "", order--);
            // Target Velocity Indicator settings
            order = 100;
            TargetVelocityIndicator.Enabled = Config.Bind("Target Velocity Indicator",
                "Target Velocity Indicator - Enabled",
                false,
                new ConfigDescription(
                    "Enable or disable the Target Velocity Indicator feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            TargetVelocityIndicator.MaxSpeed = Config.Bind("Target Velocity Indicator",
                "Target Velocity Indicator - Max Speed",
                1000.0f,
                new ConfigDescription(
                    "Max speed (kph)",
                    new AcceptableValueRange<float>(0.0f, 10000.0f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            TargetVelocityIndicator.MaxLength = Config.Bind("Target Velocity Indicator",
                "Target Velocity Indicator - Max Length",
                200.0f,
                new ConfigDescription(
                    "Max indicator offset length (pixels)",
                    new AcceptableValueRange<float>(0.0f, 10000.0f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            TargetVelocityIndicator.DotStep = Config.Bind("Target Velocity Indicator",
                "Target Velocity Indicator - Dot Step",
                10.0f,
                new ConfigDescription(
                    "Dot step (pixels)",
                    new AcceptableValueRange<float>(0.0f, 100.0f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // MiniMap Zoom
            order = 100;
            MiniMapZoom.Enabled = Config.Bind("MiniMap Zoom",
                "MiniMap Zoom - Enabled",
                false,
                new ConfigDescription(
                    "Enable or disable the MiniMap Zoom feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            MiniMapZoom.Zooms = Config.Bind("MiniMap Zoom",
                "MiniMap Zoom - Zoom levels",
                "0.5;1.0;2.0;4.0;6.0;8.0",
                new ConfigDescription(
                    "List of minimap zoom levels separated by \";\", fraction separator is \".\"",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            MiniMapZoom.Offset = Config.Bind(
                "MiniMap Zoom",
                "MiniMap Zoom - Offset",
                4000.0f,
                new ConfigDescription(
                    "Offset from center to aircraft marker (in meters). The greater, the lower is aircraft marker.",
                    new AcceptableValueRange<float>(0.0f, 10000.0f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            MiniMapZoom.Report = Config.Bind(
                "MiniMap Zoom",
                "MiniMap Zoom - Report",
                true,
                new ConfigDescription(
                    "Should zoom change be reported on HMD.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            MiniMapZoom.CycleUpKey = new RewiredInputConfig(
                Config,
                "MiniMap Zoom",
                "MiniMap Zoom - Cycle Zoom Key",
                "Cycle up through zoom levels on short press, reset to default zoom level on long press",
                order--);
            MiniMapZoom.CycleDownKey = new RewiredInputConfig(
                Config,
                "MiniMap Zoom",
                "MiniMap Zoom - Cycle Zoom Down Key",
                "Cycle down through zoom levels on short press, reset to default zoom level on long press",
                order--);
            //Map Target Arrows
            MapTargetArrows.Enabled = Config.Bind("Map Target Arrows",
                "Map Target Arrows - Enabled",
                false,
                new ConfigDescription(
                    "Enable or disable the Map Target Arrows feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            MapTargetArrows.ArrowScale = Config.Bind(
                "Map Target Arrows",
                "Map Target Arrows - Arrow Scale",
                0.5f,
                new ConfigDescription(
                    "Scale of the minimap target arrows.",
                    new AcceptableValueRange<float>(0.0f, 10.0f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            MapTargetArrows.SelectedColor = Config.Bind(
                "Map Target Arrows",
                "Map Target Arrows - Selected Color",
                Color.white,
                new ConfigDescription(
                    "Arrow color for the selected targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            MapTargetArrows.ActiveColor = Config.Bind(
                "Map Target Arrows",
                "Map Target Arrows - Active Color",
                Color.green,
                new ConfigDescription(
                    "Arrow color for the active target.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            MapTargetArrows.ShowT = Config.Bind("Map Target Arrows",
                "Map Target Arrows - Show T",
                true,
                new ConfigDescription(
                    "Should a \"T\" be shown for active target.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            //UI Adjustments
            order = 100;
            UIAdjustments.Enabled = Config.Bind("UI Adjustments",
                "UI Adjustments - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the UI Adjustments feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.TargetMarkerFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Target Marker - Font Size",
                20,
                new ConfigDescription(
                    "Target marker text font size.",
                    new AcceptableValueRange<int>(0, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.ToolTipFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - ToolTip - Font Size",
                20,
                new ConfigDescription(
                    "ToolTip text font size.",
                    new AcceptableValueRange<int>(0, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.ObjectiveMarkerFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Objective Marker - Font Size",
                20,
                new ConfigDescription(
                    "Objective marker text font size.",
                    new AcceptableValueRange<int>(0, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.GridLabelsFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Grid Labels - Font Size",
                20,
                new ConfigDescription(
                    "Grid labels text font size.",
                    new AcceptableValueRange<int>(0, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.BombingStateFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Bombing State - Font Size",
                20,
                new ConfigDescription(
                    "HUD Bombing State text font size.",
                    new AcceptableValueRange<int>(0, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.MissileStateFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Missile State - Font Size",
                20,
                new ConfigDescription(
                    "HUD Missile State text font size.",
                    new AcceptableValueRange<int>(0, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.LaserGuidedStateFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Laser Guided State - Font Size",
                20,
                new ConfigDescription(
                    "HUD Laser Guided State text font size.",
                    new AcceptableValueRange<int>(0, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            //Alternative Map Target Selection
            order = 100;
            AltMapTargetSelection.Enabled = Config.Bind("Alternative Map Target Selection",
                "Alternative Map Target Selection - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Alternative Map Target Selection feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AltMapTargetSelection.SelectionRadius = Config.Bind("Alternative Map Target Selection",
                "Alternative Map Target Selection - Selection Radius",
                20,
                new ConfigDescription(
                    "Selection radius (in pixels).",
                    new AcceptableValueRange<int>(0, 500),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AltMapTargetSelection.PickActive = Config.Bind("Alternative Map Target Selection",
                "Alternative Map Target Selection - Pick Active",
                true,
                new ConfigDescription(
                    "If no new target was selected, pick active target from already selected ones.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // Free Look Toggle
            order = 100;
            FreeLookToggle.Enabled = Config.Bind("Free Look Toggle",
                "Free Look Toggle - Enabled",
                false,
                new ConfigDescription(
                    "Enable or disable the Free Look Toggle feature (restart the game to apply changes).\n" +
                    "Uses keys bound to Free Look and Center view in game controls settings.\n" +
                    "Press Free Look key to toggle Free Look mode.\n" +
                    "Hold Free Look key to temporarily look forward.\n" +
                    "Click Center view key to toggle padlock mode (if Target Padlock option in Gameplay settings is enabled).\n" +
                    "Hold Center view key to look forward.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            FreeLookToggle.Report = Config.Bind("Free Look Toggle",
                "Free Look Toggle - Report",
                false,
                new ConfigDescription(
                    "Enable or disable reports of the free look and padlock state changes.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            FreeLookToggle.DisableFreeLookInPadlock = Config.Bind("Free Look Toggle",
                "Free Look Toggle - Disable Free Look In Padlock",
                false,
                new ConfigDescription(
                    "Automatically disable free look state in padlock mode (so the mouse controls aircraft).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            FreeLookToggle.FOVDependentSens = Config.Bind("Free Look Toggle",
                "Free Look Toggle - FOV-dependent Sensitivity - Enabled",
                false,
                new ConfigDescription(
                    "Enable or disable FOV-dependent sensitivity in Free Look mode",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // Ammo Conservation Indicator settings
            AmmoConIndicator.Enabled = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Ammo Conservation Indicator feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 0
                    }));
            AmmoConIndicator.ColorHMDMarker = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - HMD Markers Color - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable coloring HMD markers of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -1
                    }));
            AmmoConIndicator.ColorMFDBox = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - MFD Box Color - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable coloring MFD Boxes of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -2
                    }));
            AmmoConIndicator.DrawMFDDot = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - MFD Dot - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable drawing MFD dots under the boxes of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -3
                    }));
            AmmoConIndicator.HMDTrackedMarkerColor = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - HMD Tracked Marker Color - Color",
                Color.yellow,
                new ConfigDescription(
                    "Color of HMD markers of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -4
                    }));
            AmmoConIndicator.HMDDefaultMarkerColor = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - HMD Default Marker Color - Color",
                Color.green,
                new ConfigDescription(
                    "Color of HMD markers of non-attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -5
                    }));
            AmmoConIndicator.MFDTrackedBoxColor = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - MFD Tracked Box Color - Color",
                Color.yellow,
                new ConfigDescription(
                    "Color of MFD boxes of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -6
                    }));
            AmmoConIndicator.MFDDefaultBoxColor = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - MFD Default Box Color - Color",
                Color.white,
                new ConfigDescription(
                    "Color of MFD boxes of non-attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -7
                    }));
            AmmoConIndicator.MFDTrackedDotColor = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - MFD Tracked Dot Color - Color",
                new Color(0.0f, 1.0f, 0.0f, 0.95f),
                new ConfigDescription(
                    "Color of MFD dots of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -8
                    }));
            // Weapon Display settings
            weaponDisplayEnabled = Config.Bind("CM & Weapon Display",
                "CM & Weapon Display - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the CM & Weapon Display feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 3
                    }));
            weaponDisplayVanillaUIEnabled = Config.Bind("CM & Weapon Display",
                "CM & Weapon Display - Vanilla UI - Enabled",
                false,
                new ConfigDescription(
                    "Enable or disable the vanilla weapon display UI when using the weapon display feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 2
                    }));
            // Unit Distance settings
            unitDistanceEnabled = Config.Bind("Unit Marker Distance Indicator",
                "Unit Marker Distance Indicator - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Unit Marker Distance Indicator feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 2
                    }));
            unitDistanceThreshold = Config.Bind("Unit Marker Distance Indicator",
                "Unit Marker Distance Indicator - Threshold",
                10,
                new ConfigDescription(
                    "Distance threshold in kilometers for the Unit Marker Distance Indicator to change the marker's orientation.",
                    new AcceptableValueRange<int>(5, 50),
                    new ConfigurationManagerAttributes {
                        Order = 1
                    }));
            unitDistanceSoundEnabled = Config.Bind("Unit Marker Distance Indicator",
                "Unit Marker Distance Sound - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the sound notification indicating that an enemy unit has crossed the distance threshold.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 0
                    }));
            // Delivery Checker settings
            deliveryCheckerEnabled = Config.Bind("Delivery Checker",
                "Delivery Checker - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Delivery Checker feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 0
                    }));
            // MFD Color settings
            MFDColorEnabled = Config.Bind("MFD Color",
                "MFD Color - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the MFD Color feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 3
                    }));
            MFDColor = Config.Bind("MFD Color",
                "MFD Color - MFD Main Color",
                new Color(0f, 1f, 0f), // Default color in RGB
                new ConfigDescription(
                    "Main color for the MFD elements in RGB format.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 2
                    }));
            MFDTextColor = Config.Bind("MFD Color",
                "MFD Color - MFD Text Color",
                new Color(0f, 1f, 0f), // Default color in RGB
                new ConfigDescription(
                    "Color for the MFD text elements in RGB format.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 1
                    }));
            MFDAlternativeAttitudeEnabled = Config.Bind("MFD Color",
                "MFD Color - MFD Alternative Attitude - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the alternative attitude indicator color on the MFD.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 0
                    }));
            // Unit Icon Recolor settings
            unitIconRecolorEnabled = Config.Bind("AA Units Icon Recolor",
                "AA Units Icon Recolor - Enabled",
                false,
                new ConfigDescription(
                    "Enable or disable the AA Units Icon Recolor feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 1
                    }));
            unitIconRecolorEnemyColor = Config.Bind("AA Units Icon Recolor",
                "AA Units Icon Recolor - Enemy Unit Color",
                new Color(0.8f, 0.2f, 1f),
                new ConfigDescription(
                    "Color for enemy AA unit icons in RGB format.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 0
                    }));
            // HMD Unit Marker Recolor settings
            order = 100;
            HMDUnitMarkerRecolor.Enabled = Config.Bind("HMD Unit Markers Recolor",
                "HMD Unit Markers Recolor - Enabled",
                false,
                new ConfigDescription(
                    "Enable or disable the HMD Unit Markers Recolor feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HMDUnitMarkerRecolor.FriendlyColor = Config.Bind("HMD Unit Markers Recolor",
                "HMD Unit Markers Recolor - Friendly Unit Color",
                new Color(0.0f, 0.0f, 1.0f, 1.0f),
                new ConfigDescription(
                    "Friendly unit marker color in RGBA format.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HMDUnitMarkerRecolor.EnemyColor = Config.Bind("HMD Unit Markers Recolor",
                "HMD Unit Markers Recolor - Enemy Unit Color",
                new Color(1.0f, 0.0f, 0.0f, 1.0f),
                new ConfigDescription(
                    "Enemy unit marker color in RGBA format.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HMDUnitMarkerRecolor.NeutralColor = Config.Bind("HMD Unit Markers Recolor",
                "HMD Unit Markers Recolor - Neutral Unit Color",
                new Color(0.5f, 0.5f, 0.5f, 1.0f),
                new ConfigDescription(
                    "Neutral unit marker color in RGBA format.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // Boot Screen settings
            bootScreenEnabled = Config.Bind("Boot Screen Animation",
                "Boot Screen Animation - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Boot Screen Animation feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 0
                    }));
            // Artificial Horizon settings
            artificialHorizonEnabled = Config.Bind("Artificial Horizon",
                "Artificial Horizon - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Artificial Horizon feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 1
                    }));
            artificialHorizonTransparency = Config.Bind("Artificial Horizon",
                "Artificial Horizon - Transparency",
                0.4f,
                new ConfigDescription(
                    "Transparency level for the Artificial Horizon display (0.2 = almost transparent, 0.8 = vanilla opaque).",
                    new AcceptableValueRange<float>(0.2f, 0.8f),
                    new ConfigurationManagerAttributes {
                        Order = 0
                    }));
            // Bank Indicator settings
            bankIndicatorEnabled = Config.Bind("Bank Indicator",
                "Bank Indicator - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Bank Indicator feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 1
                    }));
            bankIndicatorMaxBank = Config.Bind("Bank Indicator",
                "Bank Indicator - Max Bank Angle",
                45,
                new ConfigDescription(
                    "Maximum bank angle shown on the Bank Indicator (Default is 15 degrees, value is rounded to 5).",
                    new AcceptableValueRange<int>(5, 45),
                    new ConfigurationManagerAttributes {
                        Order = 0
                    }));
            bankIndicatorShowLabel = Config.Bind("Bank Indicator",
                "Bank Indicator - Show Label",
                true,
                new ConfigDescription(
                    "Show the bank angle label below the indicator.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -1
                    }));
            bankIndicatorTransparency = Config.Bind("Bank Indicator",
                "Bank Indicator - Transparency",
                0.8f,
                new ConfigDescription(
                    "Transparency level for the Bank Indicator display (0.2 = almost transparent, 0.8 = vanilla opaque).",
                    new AcceptableValueRange<float>(0.2f, 0.8f),
                    new ConfigurationManagerAttributes {
                        Order = -2
                    }));
            bankIndicatorPositionX = Config.Bind("Bank Indicator",
                "Bank Indicator - Position X",
                0,
                new ConfigDescription(
                    "X position of the Bank Indicator in the HUD.",
                    new AcceptableValueRange<int>(-1000, 1000),
                    new ConfigurationManagerAttributes {
                        Order = -3
                    }));
            bankIndicatorPositionY = Config.Bind("Bank Indicator",
                "Bank Indicator - Position Y",
                0,
                new ConfigDescription(
                    "Y position of the Bank Indicator in the HUD.",
                    new AcceptableValueRange<int>(-1000, 1000),
                    new ConfigurationManagerAttributes {
                        Order = -4
                    }));
            //Hide Objectives settings
            hideObjectivesEnabled = Config.Bind("Hide Objectives",
                "Hide Objectives - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Hide Objectives feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 1
                    }));
            // Slip/Skid Indicator settings
            slipIndicatorEnabled = Config.Bind("Slip/Skid Indicator",
                "Slip/Skid Indicator - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Slip/Skid Indicator feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 1
                    }));
            slipIndicatorDamping = Config.Bind("Slip/Skid Indicator",
                "Slip/Skid Indicator - Damping",
                0.5f,
                new ConfigDescription(
                    "Ball damping time in seconds (0.1 = snappy, 1.0 = sluggish).",
                    new AcceptableValueRange<float>(0.1f, 1.0f),
                    new ConfigurationManagerAttributes {
                        Order = 0
                    }));
            slipIndicatorSensitivity = Config.Bind("Slip/Skid Indicator",
                "Slip/Skid Indicator - Sensitivity ratio",
                0.25f,
                new ConfigDescription(
                    "Lateral/vertical force ratio at which the ball hits max deflection (0.05 = very sensitive, 0.5 = very dull).",
                    new AcceptableValueRange<float>(0.05f, 0.5f),
                    new ConfigurationManagerAttributes {
                        Order = -1
                    }));
            slipIndicatorTransparency = Config.Bind("Slip/Skid Indicator",
                "Slip/Skid Indicator - Transparency",
                0.8f,
                new ConfigDescription(
                    "Transparency level for the Slip/Skid Indicator display (0.2 = almost transparent, 0.8 = vanilla opaque).",
                    new AcceptableValueRange<float>(0.2f, 0.8f),
                    new ConfigurationManagerAttributes {
                        Order = -2
                    }));
            slipIndicatorPositionX = Config.Bind("Slip/Skid Indicator",
                "Slip/Skid Indicator - Position X",
                0,
                new ConfigDescription(
                    "X position center of the Slip/Skid Indicator in the HUD.",
                    new AcceptableValueRange<int>(-1000, 1000),
                    new ConfigurationManagerAttributes {
                        Order = -3
                    }));
            slipIndicatorPositionY = Config.Bind("Slip/Skid Indicator",
                "Slip/Skid Indicator - Position Y",
                230,
                new ConfigDescription(
                    "Y position center of the Slip/Skid Indicator in the HUD.",
                    new AcceptableValueRange<int>(-1000, 1000),
                    new ConfigurationManagerAttributes {
                        Order = -4
                    }));
            // Autopilot settings
            autopilotMenuEnabled = Config.Bind("Autopilot",
                "Autopilot - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Autopilot Menu feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 7
                    }));
            // Camera Tweaks settings
            cameraTweaksEnabled = Config.Bind("Camera Tweaks",
                "Camera Tweaks - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Reset Cockpit FOV feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 2
                    }));
            resetCockpitFOVSpeed = Config.Bind("Camera Tweaks",
                "Camera Tweaks - Reset Cockpit FOV - Speed",
                150,
                new ConfigDescription(
                    "Speed at which the FOV resets (50 - 300).",
                    new AcceptableValueRange<int>(50, 300),
                    new ConfigurationManagerAttributes {
                        Order = 1
                    }));
            resetCockpitFOV = new RewiredInputConfig(Config, "Camera Tweaks", "Camera Tweaks - Reset Cockpit FOV", "Input you want to assign for Resetting Cockpit FOV", 0);
            lookAtNearestAirbase = new RewiredInputConfig(Config, "Camera Tweaks", "Camera Tweaks - Look At Nearest Airbase", "Input for pointing the camera at the nearest Airbase.", 0);
            // ILS Widget settings
            ILSWidgetEnabled = Config.Bind("ILS Widget",
                "ILS Widget - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the LS Widget feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 2
                    }));
            ILSIndicatorPositionX = Config.Bind("ILS Widget",
                "ILS Widget - Position X",
                430,
                new ConfigDescription(
                    "X position of the ILS Widget on the HUD.",
                    new AcceptableValueRange<int>(-1000, 1000),
                    new ConfigurationManagerAttributes {
                        Order = 1
                    }));
            ILSIndicatorPositionY = Config.Bind("ILS Widget",
                "ILS Widget - Position Y",
                10,
                new ConfigDescription(
                    "Y position of the ILS Widget on the HUD.",
                    new AcceptableValueRange<int>(-1000, 1000),
                    new ConfigurationManagerAttributes {
                        Order = 0
                    }));
            ILSIndicatorMaxAngle = Config.Bind("ILS Widget",
                "ILS Widget - Max Glideslope Error Angle",
                1f,
                new ConfigDescription(
                    "Maximum glideslope error angle shown on the ILS Widget (Default is 1 degree).",
                    new AcceptableValueRange<float>(0.5f, 5f),
                    new ConfigurationManagerAttributes {
                        Order = -1
                    }));
            // Loadout Preview settings
            loadoutPreviewEnabled = Config.Bind("Loadout Preview",
                "Loadout Preview - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Loadout Preview feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 3
                    }));
            loadoutPreviewOnlyShowOnBoot = Config.Bind("Loadout Preview",
                "Loadout Preview - Only Show On Boot",
                false,
                new ConfigDescription(
                    "If enabled, the loadout preview will only be shown on aircraft startup.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 1
                    }));
            loadoutPreviewDuration = Config.Bind("Loadout Preview",
                "Loadout Preview - Duration",
                1f,
                new ConfigDescription(
                    "Duration (in seconds) for which the loadout preview is displayed.",
                    new AcceptableValueRange<float>(0.5f, 3f),
                    new ConfigurationManagerAttributes {
                        Order = 0
                    }));
            loadoutPreviewSendToHMD = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD",
                false,
                new ConfigDescription(
                    "If enabled, the loadout preview will also be sent to the HMD display.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 2
                    }));
            loadoutPreviewHMDShowBorders = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Show Borders",
                true,
                new ConfigDescription(
                    "If enabled, shows the borders for the loadout preview when sent to the HMD.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 1
                    }));
            loadoutPreviewManualPlacement = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Manual Placement",
                false,
                new ConfigDescription(
                    "If enabled, allows manual placement of the loadout preview on the MFD.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -1
                    }));
            loadoutPreviewPositionX = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Position X",
                0,
                new ConfigDescription(
                    "X position offset for the loadout preview when manual placement is enabled.",
                    new AcceptableValueRange<int>(-1920 / 2, +1920 / 2),
                    new ConfigurationManagerAttributes {
                        Order = -2
                    }));
            loadoutPreviewPositionY = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Position Y",
                0,
                new ConfigDescription(
                    "Y position offset for the loadout preview when manual placement is enabled.",
                    new AcceptableValueRange<int>(-(int)1080 / 2, +(int)1080 / 2),
                    new ConfigurationManagerAttributes {
                        Order = -3
                    }));
            loadoutPreviewBackgroundTransparency = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Background Transparency",
                0.6f,
                new ConfigDescription(
                    "Transparency level for the Loadout Preview display's background when sent to the HMD (0 = transparent, 0.8 = vanilla opaque).",
                    new AcceptableValueRange<float>(0.0f, 0.8f),
                    new ConfigurationManagerAttributes {
                        Order = -4
                    }));
            loadoutPreviewTextAndBorderTransparency = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Text and Border Transparency",
                0.8f,
                new ConfigDescription(
                    "Transparency level for the Loadout Preview text and border when sent to the HMD (0 = transparent, 0.8 = vanilla opaque).",
                    new AcceptableValueRange<float>(0.0f, 0.8f),
                    new ConfigurationManagerAttributes {
                        Order = -5
                    }));
            // GameBindings patch
            gameBindingsPatchEnabled = Config.Bind("Game Bindings Patch",
                "Game Bindings Patch - Enabled",
                true,
                "Turn off only in case of problems.");
            //harmony.PatchAll(typeof(TestInput));
            // Patch MFD Color
            if (MFDColorEnabled.Value) {
                Log($"MFD Color is enabled, patching...");
                harmony.PatchAll(typeof(MFDColorPlugin));
            }
            // CONTROL PATCHES
            // Patch Target List Controller
            if (targetListControllerEnabled.Value) {
                Log($"Target Recall is enabled, patching...");
                harmony.PatchAll(typeof(TargetListControllerPlugin));
            }
            TargetListControllerPlugin.switchCurrentTarget = tlcSwitchCurrentTargetEnabled.Value;
            // Patch Countermeasure Controls
            if (countermeasureControlsEnabled.Value) {
                Log($"Countermeasure Controls is enabled, patching...");
                harmony.PatchAll(typeof(CountermeasureControlsPlugin));
            }
            // Patch Weapon Switcher
            if (WeaponSwitcher.Enabled.Value) {
                Log($"Weapon Switcher is enabled, patching...");
                harmony.PatchAll(typeof(WeaponSwitcherPlugin));
            }
            // Patch Target Filter Preset
            if (TargetFilterPreset.Enabled.Value) {
                Log($"Target Filter Preset is enabled, patching...");
                harmony.PatchAll(typeof(TargetFilterPresetPlugin));
            }
            // Patch HMD Declutter
            if (HMDDeclutter.Enabled.Value) {
                Log($"HMD Declutter is enabled, patching...");
                harmony.PatchAll(typeof(HMDDeclutterPlugin));
            }
            // Patch HUD Options Preset
            if (HUDOptionsPreset.Enabled.Value) {
                Log($"HUD Options Preset is enabled, patching...");
                harmony.PatchAll(typeof(HUDOptionsPresetPlugin));
            }
            // HUD CENTER DIRECTION
            if (HUDCenterDirection.Enabled.Value) {
                Log($"HUD Center Direction patch is enabled, patching...");
                harmony.PatchAll(typeof(HUDCenterDirectionPlugin));
            }
            // TARGET ARROWS
            if (TargetArrows.Enabled.Value) {
                Log($"Target Arrows patch is enabled, patching...");
                harmony.PatchAll(typeof(TargetArrowsComponent.OnMainMenuStart));
            }
            // COCKPIT DISPLAY PATCHES
            // Patch Interception Vector
            if (interceptionVectorEnabled.Value) {
                Log($"Interception Vector is enabled, patching...");
                harmony.PatchAll(typeof(InterceptionVectorPlugin));
            }
            // Patch Weapon Display
            if (weaponDisplayEnabled.Value) {
                Log($"Weapon Display is enabled, patching...");
                harmony.PatchAll(typeof(WeaponDisplayPlugin));
            }
            // Patch Loadout Preview
            if (loadoutPreviewEnabled.Value) {
                Log($"Loadout Preview is enabled, patching...");
                harmony.PatchAll(typeof(LoadoutPreviewPlugin));
            }
            // Patch Delivery Checker
            if (deliveryCheckerEnabled.Value) {
                Log($"Delivery Checker is enabled, patching...");
                harmony.PatchAll(typeof(DeliveryCheckerPlugin));
            }
            // Patch Ammo Conservation Indicator
            if (AmmoConIndicator.Enabled.Value) {
                Log("Ammo Conservation Indicator is enabled, patching...");
                harmony.PatchAll(typeof(AmmoConIndicatorPlugin));
            }
            // we load this one last so that the boot applies to the elements we add to the cockpit as well
            // Patch Boot Screen
            if (bootScreenEnabled.Value) {
                Log($"Boot Screen is enabled, patching...");
                harmony.PatchAll(typeof(BootScreenPlugin));
            }
            // HMD DISPLAY PATCHES
            // Patch Unit Distance
            if (unitDistanceEnabled.Value) {
                Log($"Unit Marker Distance Indicator is enabled, patching...");
                harmony.PatchAll(typeof(UnitDistancePlugin));
            }
            // Patch Artificial Horizon
            if (artificialHorizonEnabled.Value) {
                Log($"Artificial Horizon is enabled, patching...");
                harmony.PatchAll(typeof(ArtificialHorizonPlugin));
            }
            // Patch Hide Objectives
            if (hideObjectivesEnabled.Value) {
              Log($"Hide Objectives Plugin is enabled, patching...");
              harmony.PatchAll(typeof(HideObjectivesPlugin));
            }
            // HUD DISPLAY PATCHES
            // Patch ILS
            if (ILSWidgetEnabled.Value) {
                Log($"ILS is enabled, patching...");
                harmony.PatchAll(typeof(ILSIndicatorPlugin));
            }
            // Patch Bank Indicator
            if (bankIndicatorEnabled.Value) {
                Log($"Bank Indicator is enabled, patching...");
                harmony.PatchAll(typeof(BankIndicatorPlugin));
            }
            // Patch Slip/Skid Indicator
            if (slipIndicatorEnabled.Value) {
                Log($"Slip/Skid Indicator is enabled, patching...");
                harmony.PatchAll(typeof(SlipIndicatorPlugin));
            }
            // MAP DISPLAY PATCHES
            // Patch Unit Icon Recolor
            if (unitIconRecolorEnabled.Value) {
                Log($"Unit Icon Recolor is enabled, patching...");
                harmony.PatchAll(typeof(UnitIconRecolorPlugin));
            }
            // Patch HMD Unit Marker Recolor
            if (HMDUnitMarkerRecolor.Enabled.Value) {
                Log($"HMD Unit Marker Recolor is enabled, patching...");
                harmony.PatchAll(typeof(HMDUnitMarkerRecolorPlugin));
            }
            // CAMERA TWEAKS PATCHES
            // Patch Camera Tweaks
            if (cameraTweaksEnabled.Value) {
                Log($"Camera Tweaks is enabled, patching...");
                harmony.PatchAll(typeof(CameraTweaksPlugin));
            }
            // MOD COMPAT PATCHES
            if (autopilotMenuEnabled.Value) {
                Log($"Autopilot Menu is enabled, patching...");
                harmony.PatchAll(typeof(NOAutopilotControlPlugin));
            }
            // VIRTUAL JOYSTICK EXTENDER PATCHES
            if (VirtualJoystickExtender.Enabled.Value) {
                Log($"Virtual Joystick Extender is enabled, patching...");
                harmony.PatchAll(typeof(VirtualJoystickExtenderPlugin));
            }
            // KEY AXES PATCHES
            if (keyAxesEnabled.Value) {
                Log($"Key Axes are enabled, patching...");
                harmony.PatchAll(typeof(KeyAxesPlugin));
            }
            // TARGET CAM MODE PATCHES
            if (targetCamModeEnabled.Value) {
                Log($"Target Cam Mode Plugin is enabled, patching...");
                harmony.PatchAll(typeof(TargetCamModePlugin));
            }
            // ALTERNATIVE TARGET SELECTION PATCHES
            if (AltTargetSelection.Enabled.Value) {
                Log($"Alternative Target Selection Plugin is enabled, patching...");
                harmony.PatchAll(typeof(AltTargetSelectionPlugin));
            }
            // TARGET VELOCITY INDICATOR PATCHES
            if (TargetVelocityIndicator.Enabled.Value) {
                Log($"Target Velocity Indicator is enabled, patching...");
                harmony.PatchAll(typeof(TargetVelocityIndicatorPlugin));
            }
            // MINIMAP ZOOM
            if (MiniMapZoom.Enabled.Value) {
                Log($"Minimap Zoom is enabled, patching...");
                harmony.PatchAll(typeof(MiniMapZoomPlugin));
            }
            // MAP TARGET ARROWS
            if (MapTargetArrows.Enabled.Value) {
                Log($"Map Target Arrows are enabled, patching...");
                harmony.PatchAll(typeof(MapTargetArrowsPlugin));
            }
            // FONT FIX
            if (UIAdjustments.Enabled.Value) {
                Log($"UI Adjustments is enabled, patching...");
                harmony.PatchAll(typeof(UIAdjustmentsPlugin));
            }
            // MAP SELECT FIX
            if (AltMapTargetSelection.Enabled.Value) {
                Log($"Alternative Map Target Selection is enabled, patching...");
                harmony.PatchAll(typeof(AltMapTargetSelectionPlugin));
            }
            // FREE LOOK TOGGLE
            if (FreeLookToggle.Enabled.Value) {
                Log($"Free Look toggle is enabled, patching...");
                harmony.PatchAll(typeof(FreeLookTogglePlugin));
            }
            // GAMEBINDINGS
            if (gameBindingsPatchEnabled.Value) {
                Log($"Game Bindings patch is enabled, patching...");
                harmony.PatchAll(typeof(GameBindingsPlugin));
            }
            //Finished patching
            //Load audio assets
            Log("Loading audio assets...");
            UIBindings.Sound.LoadAllSounds();
            // Log completion
            Log("NO Tactitools loaded successfully !");
        }


        public static void Log(string message) {
            if (debugModeEnabled.Value) {
                TimeSpan timeSpan = TimeSpan.FromSeconds(Time.realtimeSinceStartup);
                string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                Logger.LogInfo("[" + formattedTime + "] " + message);
            }
        }
    }
}
