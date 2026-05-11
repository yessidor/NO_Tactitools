using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Rewired;
using NO_Tactitools.Controls;
using NO_Tactitools.UI.HMD;
using NO_Tactitools.UI.MFD;
using NO_Tactitools.UI.HUD;
using BepInEx.Bootstrap;

namespace NO_Tactitools.Core {
    [BepInPlugin("com.yessidor.NO_Tactitools-plus", "NOTT-plus", "0.7.5.0")]
    public class Plugin : BaseUnityPlugin {
        public static Harmony harmony;
        public static RewiredInputConfig MFDNavEnter;
        public static RewiredInputConfig MFDNavBack;
        public static RewiredInputConfig MFDNavUp;
        public static RewiredInputConfig MFDNavDown;
        public static RewiredInputConfig MFDNavLeft;
        public static RewiredInputConfig MFDNavRight;
        public static RewiredInputConfig MFDNavToggle;
        public static RewiredInputConfig MFDNavPoptByUnitName;
        public static RewiredInputConfig MFDNavMissileTargetingSystem;
        public static ConfigEntry<int> MFDNavExtraKeysNum;
        public static List<RewiredInputConfig> MFDNavExtraKeys;
        public static ConfigEntry<bool> targetListControllerEnabled;
        public static ConfigEntry<bool> tlcSwitchCurrentTargetEnabled;
        public static ConfigEntry<bool> interceptionVectorEnabled;
        public static ConfigEntry<bool> countermeasureControlsEnabled;
        public static RewiredInputConfig countermeasureControlsFlare;
        public static RewiredInputConfig countermeasureControlsJammer;
        public static ConfigEntry<bool> weaponSwitcherEnabled;
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
        public static RewiredInputConfig weaponSwitcher0;
        public static RewiredInputConfig weaponSwitcher1;
        public static RewiredInputConfig weaponSwitcher2;
        public static RewiredInputConfig weaponSwitcher3;
        public static RewiredInputConfig weaponSwitcher4;
        public static RewiredInputConfig weaponSwitcher5;
        public static ConfigEntry<bool> targetFilterPresetEnabled;
        public static ConfigEntry<int> targetFilterPresetNum;
        public static List<RewiredInputConfig> targetFilterPresets;
        public static ConfigEntry<bool> targetFilterPresetMaximizeTargetable;
        // Virtual Joystick Extender
        public static ConfigEntry<bool> virtualJoystickExtenderEnabled;
        public static ConfigEntry<VirtualJoystickExtender.Modes> virtualJoystickExtenderDefaultMode;
        public static ConfigEntry<bool> virtualJoystickExtenderToggleMode;
        public static RewiredInputConfig virtualJoystickExtenderToggleStateKey;
        public static RewiredInputConfig virtualJoystickExtenderYawKey;
        public static RewiredInputConfig virtualJoystickExtenderRollYawKey;
        public static ConfigEntry<float> virtualJoystickExtenderYawMultiplier;
        public static ConfigEntry<float> virtualJoystickExtenderRollYawMultiplier;
        public static ConfigEntry<float> virtualJoystickExtenderYawCurvature;
        public static ConfigEntry<float> virtualJoystickExtenderPitchCurvature;
        public static ConfigEntry<float> virtualJoystickExtenderRollCurvature;
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
        // Target Cam Mode
        public static ConfigEntry<bool> targetCamModeEnabled;
        public static RewiredInputConfig targetCamModeToggleKey;
        // Alternative Target Selection
        public class AltTargetSelection {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<float> FOVFraction;
        }
        //
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
        public static ConfigEntry<bool> debugModeEnabled;
        internal static new ManualLogSource Logger;
        public static Plugin Instance;

        private void Update() {
            RewiredConfigManager.Update();
        }

        private void Awake() {
            Instance = this;
            int order = 100;
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
            MFDNavPoptByUnitName = new RewiredInputConfig(Config,
              "MFD Nav",
              "MFD Nav - Select Targets By Unit Name",
              "Input you want to assign for deselecting targets based on the unit name of current target",
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
            weaponSwitcherEnabled = Config.Bind("Advanced Slot Selection",
                "Advanced Slot Selection - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Advanced Slot Selection feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 7
                    }));
            weaponSwitcher0 = new RewiredInputConfig(Config, "Advanced Slot Selection", "Advanced Slot Selection - Slot 0", "Input for slot 0 (Long press to toggle Turret Auto Control)", 5);
            weaponSwitcher1 = new RewiredInputConfig(Config, "Advanced Slot Selection", "Advanced Slot Selection - Slot 1", "Input for slot 1", 4);
            weaponSwitcher2 = new RewiredInputConfig(Config, "Advanced Slot Selection", "Advanced Slot Selection - Slot 2", "Input for slot 2", 3);
            weaponSwitcher3 = new RewiredInputConfig(Config, "Advanced Slot Selection", "Advanced Slot Selection - Slot 3", "Input for slot 3", 2);
            weaponSwitcher4 = new RewiredInputConfig(Config, "Advanced Slot Selection", "Advanced Slot Selection - Slot 4", "Input for slot 4", 1);
            weaponSwitcher5 = new RewiredInputConfig(Config, "Advanced Slot Selection", "Advanced Slot Selection - Slot 5", "Input for slot 5", 0);
            // Target Filter Preset settings
            order = 100;
            targetFilterPresetEnabled = Config.Bind("Target Filter Preset",
                "Target Filter Preset - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Target Filter Preset feature.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            targetFilterPresetNum = Config.Bind("Target Filter Preset",
                "Target Filter Preset - Number",
                10,
                new ConfigDescription(
                    "Number of target filter presets (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            targetFilterPresets = new();
            for (int i = 0; i < targetFilterPresetNum.Value; i++)
            {
                string name = string.Format("Target Filter Preset - Slot {0}", i);
                string description = string.Format("Input for slot {0} (Long press to save, short press to restore)", i);
                targetFilterPresets.Add(new RewiredInputConfig(Config, "Target Filter Preset", name, description, order--));
            }
            targetFilterPresetMaximizeTargetable = Config.Bind("Target Filter Preset",
                "Target Filter Preset - Maximize Targetable Markers - Enabled",
                true,
                new ConfigDescription(
                    "If enabled, maximize markers of targetable units regardless of HUD settings",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            // Virtual Joystick Extender
            order = 0;
            virtualJoystickExtenderEnabled = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Enabled",
                true, 
                new ConfigDescription(
                    "Enable or disable the Virtual Joystick Extender feature.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            virtualJoystickExtenderDefaultMode = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Default Mode",
                VirtualJoystickExtender.Modes.Roll, 
                new ConfigDescription(
                    "Virtual Joystick Extender default mode.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            virtualJoystickExtenderToggleMode = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Toggle Mode - Enabled",
                true, 
                new ConfigDescription(
                    "Enable or disable Virtual Joystick Extender mode toggle.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            virtualJoystickExtenderToggleStateKey = new RewiredInputConfig(Config,
                "Virtual Joystick Extender",
                "Virtual Joystick Extender - Toggle Key",
                "Key to turn on/off",
                order--);
            virtualJoystickExtenderYawKey = new RewiredInputConfig(Config,
                "Virtual Joystick Extender",
                "Virtual Joystick Extender - Yaw Mode Key",
                "Key to toggle/switch to yaw mode",
                order--);
            virtualJoystickExtenderRollYawKey = new RewiredInputConfig(Config,
                "Virtual Joystick Extender",
                "Virtual Joystick Extender - Roll&Yaw Mode Key",
                "Key to toggle/switch to roll&yaw mode",
                order--);
            virtualJoystickExtenderYawMultiplier = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Yaw Mode Multiplier",
                1.0f,
                new ConfigDescription(
                    "Multiplier of yaw axis value in yaw mode.",
                    new AcceptableValueRange<float>(-10.0f, 10.0f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            virtualJoystickExtenderRollYawMultiplier = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Roll&Yaw Mode Multiplier",
                0.5f,
                new ConfigDescription(
                    "Multiplier of yaw axis value in roll&yaw mode.",
                    new AcceptableValueRange<float>(-10.0f, 10.0f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            virtualJoystickExtenderYawCurvature = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Yaw Curvature",
                0.2f,
                new ConfigDescription(
                    "Curvature of yaw axis response curve",
                    new AcceptableValueRange<float>(0.0f, 0.99f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            virtualJoystickExtenderPitchCurvature = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Pitch Curvature",
                0.2f,
                new ConfigDescription(
                    "Curvature of pitch axis response curve",
                    new AcceptableValueRange<float>(0.0f, 0.99f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            virtualJoystickExtenderRollCurvature = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Roll Curvature",
                0.2f,
                new ConfigDescription(
                    "Curvature of roll axis response curve",
                    new AcceptableValueRange<float>(0.0f, 0.99f),
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
                    "Enable or disable the Key Axes feature.",
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
            targetCamModeEnabled = Config.Bind("Target Cam Mode",
                "Target Cam Mode - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Target Cam Mode feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -4
                    }));
            targetCamModeToggleKey = new RewiredInputConfig(Config, "Target Cam Mode", "Target Cam Mode - Toggle Mode Key", "", -5);
            // Alternative Target Selection
            AltTargetSelection.Enabled = Config.Bind("Alternative Target Selection",
                "Alternative Target Selection - Enabled",
                false,
                new ConfigDescription(
                    "Enable or disable the Alternative Target Selection feature (restart the game to apply change).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -4
                    }));
            AltTargetSelection.FOVFraction = Config.Bind("Alternative Target Selection",
                "Alternative Target Selection - Camera FOV Fraction",
                0.15f,
                new ConfigDescription(
                    "Fraction multiplied by camera vertical FOV to get apex angle of selection cone.",
                    new AcceptableValueRange<float>(0.0f, 0.999f),
                    new ConfigurationManagerAttributes {
                        Order = -5
                    }));
            targetCamModeToggleKey = new RewiredInputConfig(Config, "Target Cam Mode", "Target Cam Mode - Toggle Mode Key", "", -5);
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
            // Debug Mode settings
            debugModeEnabled = Config.Bind("Debug Mode",
                "Debug Mode - Enabled",
                true,
                "Enable or disable the debug mode for logging");
            // Plugin startup logic
            harmony = new Harmony("george.no_tactitools");
            Logger = base.Logger;
            // CORE PATCHES
            harmony.PatchAll(typeof(RegisterControllerPatch));
            harmony.PatchAll(typeof(ControllerInputInterceptionPatch));
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
            if (weaponSwitcherEnabled.Value) {
                Log($"Weapon Switcher is enabled, patching...");
                harmony.PatchAll(typeof(WeaponSwitcherPlugin));
            }
            // Patch Target Filter Preset
            if (targetFilterPresetEnabled.Value) {
                Log($"Target Filter Preset is enabled, patching...");
                harmony.PatchAll(typeof(TargetFilterPresetPlugin));
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
            if (virtualJoystickExtenderEnabled.Value) {
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
