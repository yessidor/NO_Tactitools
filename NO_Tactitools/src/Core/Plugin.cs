using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace NO_Tactitools.Core {
    [BepInPlugin("com.yessidor.NO_Tactitools-plus", "NOTT-plus", "0.7.33.0")]
    public class Plugin : BaseUnityPlugin {
        public static Harmony harmony;
        public class Modifiers {
            public static ConfigEntry<int> ModifiersNum;
        }
        public static ConfigEntry<float> PressDelay;
        public static RewiredInputConfig MFDNavEnter;
        public static RewiredInputConfig MFDNavBack;
        public static RewiredInputConfig MFDNavUp;
        public static RewiredInputConfig MFDNavDown;
        public static RewiredInputConfig MFDNavLeft;
        public static RewiredInputConfig MFDNavRight;
        public static RewiredInputConfig MFDNavToggle;
        public static RewiredInputConfig MFDNavSelectByUnitName;
        public static RewiredInputConfig MFDNavSelectLased;
        public static RewiredInputConfig MFDNavSelectClosest;
        public static RewiredInputConfig MFDNavMissileTargetingSystem;
        public static ConfigEntry<int> MFDNavExtraKeysNum;
        public static List<RewiredInputConfig> MFDNavExtraKeys;
        public static ConfigEntry<bool> targetListControllerEnabled;
        public static ConfigEntry<bool> tlcSwitchCurrentTargetEnabled;
        public static ConfigEntry<bool> tlcMTSSelectOnlyInterceptableMissiles;
        public static ConfigEntry<float> tlcMTSAngleThreshold;
        public static ConfigEntry<bool> tlcRespectTargetingDistances;
        public static ConfigEntry<bool> tlcSelectUnitsToReload;
        public static ConfigEntry<bool> tlcSelectClosestTargetsByAmmo;
        public static ConfigEntry<bool> tlcExtraEntryCheckboxFunctions;
        public static ConfigEntry<bool> tlcHighlightActiveEntry;
        public static ConfigEntry<bool> interceptionVectorEnabled;
        public static ConfigEntry<bool> countermeasureControlsEnabled;
        public static ConfigEntry<bool> countermeasureControlsActivate;
        public static RewiredInputConfig countermeasureControlsFlare;
        public static RewiredInputConfig countermeasureControlsJammer;
        public static RewiredInputConfig countermeasureControlsChaff;
        public class AmmoConIndicator {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<bool> ColorHMDMarker;
            public static ConfigEntry<bool> ColorHMDLasedMarker;
            public static ConfigEntry<bool> ColorMFDBox;
            public static ConfigEntry<bool> DrawMFDDot;
            public static ConfigEntry<Color> HMDTrackedMarkerColor;
            public static ConfigEntry<Color> HMDLasedMarkerColor;
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
        public class WeaponManagerExtensions {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<bool> AttackActiveTarget;
            public static ConfigEntry<bool> AttackOnlyLasedTargets;
            public static ConfigEntry<bool> ToggleJammerFire;
            public static ConfigEntry<bool> KeepFiring;
            public static ConfigEntry<bool> AimAssistToggle;
            public static RewiredButtonConfig ToggleAimAssist;
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
            public static ConfigEntry<GameBindings.Units.DistanceUnits> DistanceUnit;
            public static ConfigEntry<bool> Report;
            public static ConfigEntry<bool> NotAlwaysMaximized;
            public static ConfigEntry<bool> MinimizeMaximized;
            public static ConfigEntry<bool> HideMinimized;
            public static ConfigEntry<float> EnemyMinimizedMarkerScale;
            public static ConfigEntry<float> FriendlyMinimizedMarkerScale;

            public static ConfigEntry<float> OutdatedTime;
            public static ConfigEntry<bool> ShowOutdatedTime;
            public static ConfigEntry<bool> HideOutdatedMarker;
            public static ConfigEntry<bool> SetOutdatedIcon;
            public static ConfigEntry<float> EndOutdatedMarkerOpacity;

            public static ConfigEntry<bool> MaximizeOwnMissiles;
            public static ConfigEntry<bool> ColorizeOwnMissiles;
            public static ConfigEntry<bool> AlwaysDrawOwnMissiles;
            public static ConfigEntry<bool> IncludeDerivedMissiles;
            public static ConfigEntry<Color> OwnMissilesColor;
            public static ConfigEntry<Color> OwnMissedMissilesColor;
            public static ConfigEntry<float> OwnMissilesHMDMarkerScale;
            public static ConfigEntry<float> OwnMissilesMapIconScale;
            public static ConfigEntry<float> FlashBeforeImpactTime;

            public static ConfigEntry<bool> MaximizeOwnUnits;
            public static ConfigEntry<bool> ColorizeOwnUnits;
            public static ConfigEntry<Color> OwnUnitsColor;
            public static ConfigEntry<float> OwnUnitsHMDMarkerScale;
            public static ConfigEntry<float> OwnUnitsMapIconScale;
        };
        public class HUDOptionsPreset {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<int> PresetsNum;
            public static List<RewiredInputConfig> Presets;
            public static ConfigEntry<bool> EnableBuiltinSettings;
        };
        public class PersistentMapOptions {
            public static ConfigEntry<bool> Enabled;
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
            public class ModeData {
                public ConfigEntry<string> Name;

                public RewiredInputConfig EngageKey;
                public RewiredInputConfig ToggleKey;

                public ConfigEntry<VirtualJoystickExtenderComponent.InputAxis> YawAxisMapping;
                public ConfigEntry<VirtualJoystickExtenderComponent.InputAxis> PitchAxisMapping;
                public ConfigEntry<VirtualJoystickExtenderComponent.InputAxis> RollAxisMapping;

                public ConfigEntry<Vector3> Sens;
                public ConfigEntry<Vector3> InputMultiplierOnEnter;

                public ConfigEntry<Vector3> DynamicCurvature;
                public ConfigEntry<Axes<float>> StaticCurvature;
                public ConfigEntry<Axes<float>> OutputMultiplier;
            }
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<Vector3> Sens;
            public static RewiredAxisConfig AxisX;
            public static RewiredAxisConfig AxisY;
            public static RewiredAxisConfig AxisZ;
            public static RewiredButtonConfig ToggleStateKey;
            public static RewiredButtonConfig ResetKey;
            public static RewiredButtonConfig MaxDeflectionModeKey;
            public static ConfigEntry<int> NumModes;
            public static List<ModeData> Modes;
            public static ConfigEntry<int> DefaultMode;
            public static ConfigEntry<VirtualJoystickExtenderComponent.DecayModes> DecayMode;
            public static ConfigEntry<VirtualJoystickExtenderComponent.InputAxesResetModes> InputAxesResetMode;
            public static ConfigEntry<float> DecaySpeed;
            public static ConfigEntry<bool> ControlInThirdPersonMode;
            public static ConfigEntry<bool> DisableInInvalidCameraMode;
            public static ConfigEntry<bool> DisableInFreeLook;
            public static ConfigEntry<bool> DisableOnMaximizedMap;
            public static ConfigEntry<bool> DisableOnUIInteraction;
            public static ConfigEntry<bool> RunInGraphicsUpdate;
            public static ConfigEntry<float> FixedDT;
            public static ConfigEntry<VirtualJoystickExtenderComponent.VectorPlacements> VectorPlacement;
            public static ConfigEntry<bool> ShiftVector;
            public static ConfigEntry<bool> RollControlsYawOnTheGround;
            public static ConfigEntry<float> MaxDeflection;
            public static ConfigEntry<VirtualJoystickExtenderComponent.LimitsShapes> LimitsShape;
            public static ConfigEntry<float> CenteringSpeed;
            public static ConfigEntry<float> CenteringDeflection;
            public static ConfigEntry<float> DampingAngle;
            public static ConfigEntry<float> DampingSens;
            public static ConfigEntry<float> DampingCurvature;
        }
        // Key axes
        public class KeyAxisData {
            public RewiredButtonConfig IncKey;
            public RewiredButtonConfig DecKey;
            public RewiredAxisConfig EncoderAxis;
            public ConfigEntry<float> BuildUpSpeed;
            public ConfigEntry<float> DecaySpeed;
            public ConfigEntry<float> DecayDelay;
            public ConfigEntry<bool> TwoKeyReset;
            public ConfigEntry<float> EncoderSens;
            public ConfigEntry<float> DynamicCurvature;
            public ConfigEntry<float> EncoderDynamicCurvature;
            public ConfigEntry<float> StaticCurvature;
            public ConfigEntry<float> StaticOffset;
            public ConfigEntry<float> InitialValue;
            public string Name;
            public string IncKeyName;
            public string DecKeyName;

            public KeyAxisData (string name, string incKeyName, string decKeyName) {
                Name = name;
                IncKeyName = incKeyName;
                DecKeyName = decKeyName;
            }
        };
        public static ConfigEntry<bool> keyAxesEnabled;
        public static KeyAxisData[] keyAxes;
        public static RewiredButtonConfig yprAxesResetKey;
        public static RewiredButtonConfig allAxesResetKey;
        public static ConfigEntry<KeyAxesComponent.ConfirmAirbrakeDeploymentMode> keyAxesConfirmAirbrakeDeployment;
        public static ConfigEntry<bool> targetCamModeEnabled;
        public static RewiredInputConfig targetCamModeToggleKey;
        public class AltTargetSelection {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<float> FOVFraction;
            public static ConfigEntry<float> MaxDistance;
            public static ConfigEntry<bool> PickActive;
            public static ConfigEntry<AltTargetSelectionModes> SelectionMode;
            public static ConfigEntry<bool> ShowUnitInfo;
            public static ConfigEntry<bool> ShowUnitInfoForSelected;
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
            public static ConfigEntry<bool> CenterMinimizedMapInPrefix;
            public static ConfigEntry<bool> IndependentZoomLevels;
            public static ConfigEntry<bool> SaveMaximizedPosition;
        };
        public class MapTargetArrows {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<float> ArrowScale;
            public static ConfigEntry<Color> SelectedColor;
            public static ConfigEntry<Color> ActiveColor;
            public static ConfigEntry<bool> ShowT;
        };
        public class CustomizeMissileString {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<string> EntryFormatString;
            public static ConfigEntry<string> NotchFormatString;
        }
        public class UIAdjustments {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<int> TargetMarkerFontSize;
            public static ConfigEntry<int> ToolTipFontSize;
            public static ConfigEntry<int> ObjectiveMarkerFontSize;
            public static ConfigEntry<int> GridLabelsFontSize;
            public static ConfigEntry<int> BombingStateFontSize;
            public static ConfigEntry<int> MissileStateFontSize;
            public static ConfigEntry<int> LaserGuidedStateFontSize;
            public static ConfigEntry<int> WingAngleGaugeFontSize;
            public static ConfigEntry<int> NozzleGaugeFontSize;
            public static ConfigEntry<int> NotchIndicatorLabelFontSize;
            public static ConfigEntry<bool> MapIconColorFix;
            public static ConfigEntry<bool> StickyRearmerDisplayFix;
            public static ConfigEntry<bool> MultipleRearmerDisplaysFix;
            public static ConfigEntry<bool> HUDUnitMarkerSelectionFix;
            public static ConfigEntry<bool> StaleTargetFix;
            public static ConfigEntry<bool> TargetDeselectionSoundFix;
            public static ConfigEntry<bool> HUDCargoStateHUDFixedUpdateFix;
            public static ConfigEntry<bool> DisableAllyInfo;
            public static ConfigEntry<bool> ColorizePlayerRelatedMessages;
            public static ConfigEntry<bool> CenterOnJumpMap;
            public class AirbaseOverlay {
                public static ConfigEntry<bool> AlwaysDisplayGlidepath;
                public static ConfigEntry<bool> IgnoreRunwayLimits;
            }
            public static ConfigEntry<bool> NOAutopilotGCASChevron;
        };
        public class SensitivityFix {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<Vector3> CommonSens;
            public static ConfigEntry<Vector3>[] Sensitivities;
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
            public static ConfigEntry<bool> DisableFreeLookInForwardlock;
            public static ConfigEntry<bool> DisableFreeLookOnCenter;
            public static ConfigEntry<bool> FOVDependentSens;
            public static ConfigEntry<Vector3> CenteringPositionMultiplier;
            public static RewiredButtonConfig CenterKey;
            public static RewiredButtonConfig PadlockKey;
            public static RewiredButtonConfig FreeLookKey;
        }
        public class KeyViewControl {
            public static ConfigEntry<bool> Enabled;
            public static RewiredButtonConfig PanLeftKey;
            public static RewiredButtonConfig PanRightKey;
            public static RewiredButtonConfig TiltUpKey;
            public static RewiredButtonConfig TiltDownKey;
            public static ConfigEntry<bool> FOVDependent;
            public static ConfigEntry<bool> StopAt0;
            public static ConfigEntry<float> PanStep;
            public static ConfigEntry<float> TiltStep;
            public static ConfigEntry<float> PanSpeed;
            public static ConfigEntry<float> TiltSpeed;
        }
        public class HeadAxes {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<float> PanLimit;
            public static ConfigEntry<float> TiltLimit;
            public static ConfigEntry<float> MinFOV;
            public static ConfigEntry<float> MaxFOV;
            public static ConfigEntry<float> FOVSpeed;
            public static RewiredAxisConfig PanAxis;
            public static RewiredAxisConfig TiltAxis;
            public static RewiredAxisConfig FOVAxis;
        }
        public class ThirdPersonHUD {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<bool> HUDRoll;
            public static ConfigEntry<bool> HUDBoundToScreen;
            public static ConfigEntry<Vector2> HUDScreenOffset;
            public static ConfigEntry<bool> SetTargetDesignatorPos;
            public static ConfigEntry<Vector2> TargetDesignatorScreenOffset;
        }
        public class DynamicLandingCam {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<bool> KeepOnAfterTouchDown;
            public static ConfigEntry<bool> Rotate;
            public static ConfigEntry<float> RotationSpeed;
            public static ConfigEntry<Vector2> TiltLimits;
            public static ConfigEntry<Vector2> PanLimits;
            public static ConfigEntry<Vector2> InitialAngles;
            public static ConfigEntry<float> LandingCamFOV;
            public static ConfigEntry<float> Deadzone;
            public static ConfigEntry<bool> FixBrawlerLandingCam;
        }
        public class HMDCam {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<Vector2> Position;
            public static ConfigEntry<Vector2> Size;
            public static ConfigEntry<float> Transparency;
            public static ConfigEntry<bool> SuppressMFDCam;
        }
        public class EMWS {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<GameBindings.Units.DistanceUnits> DistanceUnit;
            public static ConfigEntry<bool> ProcessOnlyEnemyMissiles;
            public static ConfigEntry<bool> HideOnMissileWarning;
            public static ConfigEntry<int> NumEntries;

            public class MissileConfigData {
                public ConfigEntry<string> SeekerTypes;
                public ConfigEntry<float> Angle;
                public ConfigEntry<float> Distance;
                public ConfigEntry<Color> ItemColor;
                public ConfigEntry<bool> FlashMarker;
                public ConfigEntry<bool> ShowNotchLine;
                public ConfigEntry<bool> ShowVectorLine;
                public ConfigEntry<bool> ShowNotchIndicator;
                public ConfigEntry<bool> ShowText;
                public ConfigEntry<float> HMDMarkerScale;
                public ConfigEntry<float> MapIconScale;
            }
            public static List<MissileConfigData> MissileConfigDatum;
        }
        public class MultiLevelMenu {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<int> NumMain;
            public static List<ConfigEntry<string>> Main;
            public static ConfigEntry<int> NumWeapons;
            public static List<ConfigEntry<string>> Weapons;
            public static ConfigEntry<float> MenuDelay;
            public static ConfigEntry<float> ZPerLevel;
            public static ConfigEntry<bool> ResetLevel;
            public static ConfigEntry<bool> OpenOnPress;
            public static ConfigEntry<bool> Toggle;
            public static ConfigEntry<bool> ShowPreview;
            public static ConfigEntry<global::NO_Tactitools.Controls.MultiLevelMenu.SwitchModes> SwitchMode;
            public static ConfigEntry<float> LevelSwitchPeriod;
            public static ConfigEntry<Color> DefaultColor;
            public static ConfigEntry<Color> SelectedColor;
            public static ConfigEntry<Color> BackgroundColorActive;
            public static ConfigEntry<Color> BackgroundColorInactive;
        }
        public static ConfigEntry<bool> weaponDisplayEnabled;
        public static ConfigEntry<bool> weaponDisplayVanillaUIEnabled;
        public static ConfigEntry<string> weaponDisplayDisabledFor;
        public static ConfigEntry<bool> unitDistanceEnabled;
        public static ConfigEntry<int> unitDistanceThreshold;
        public static ConfigEntry<bool> unitDistanceSoundEnabled;
        public static ConfigEntry<bool> deliveryCheckerEnabled;
        public static ConfigEntry<bool> deliveryCheckerShowTTI;
        public static ConfigEntry<bool> MFDColorEnabled;
        public static ConfigEntry<Color> MFDColor;
        public static ConfigEntry<Color> MFDTextColor;
        public static ConfigEntry<bool> MFDAlternativeAttitudeEnabled;
        public static ConfigEntry<bool> unitIconRecolorEnabled;
        public static ConfigEntry<string> unitIconRecolorUnits;
        public static ConfigEntry<Color> unitIconRecolorEnemyColor;
        public class HMDUnitMarkerRecolor {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<Color> FriendlyColor;
            public static ConfigEntry<Color> EnemyColor;
            public static ConfigEntry<Color> NeutralColor;
        };
        public static ConfigEntry<bool> bootScreenEnabled;
        public static ConfigEntry<bool> artificialHorizonEnabled;
        public static ConfigEntry<string> artificialHorizonAuthorizedFor;
        public static ConfigEntry<float> artificialHorizonTransparency;
        public static ConfigEntry<bool> bankIndicatorEnabled;
        public static ConfigEntry<string> bankIndicatorAuthorizedFor;
        public static ConfigEntry<int> bankIndicatorMaxBank;
        public static ConfigEntry<bool> bankIndicatorShowLabel;
        public static ConfigEntry<float> bankIndicatorTransparency;
        public static ConfigEntry<int> bankIndicatorPositionX;
        public static ConfigEntry<int> bankIndicatorPositionY;
        public static ConfigEntry<bool> slipIndicatorEnabled;
        public static ConfigEntry<string> slipIndicatorAuthorizedFor;
        public static ConfigEntry<float> slipIndicatorTransparency;
        public static ConfigEntry<int> slipIndicatorPositionX;
        public static ConfigEntry<int> slipIndicatorPositionY;
        public static ConfigEntry<float> slipIndicatorDamping;
        public static ConfigEntry<float> slipIndicatorSensitivity;
        public class NOAutopilotControl {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<bool> SendToHMD;
            public static ConfigEntry<string> SendToHMDFor;
            public static ConfigEntry<Color> MFDMainColor;
            public static ConfigEntry<Color> HMDMainColor;
            public static ConfigEntry<int> HMDFontSize;
            public static ConfigEntry<int> HMDPositionX;
            public static ConfigEntry<int> HMDPositionY;
            public static ConfigEntry<float> AltIncrement;
            public static ConfigEntry<float> ClimbIncrement;
            public static ConfigEntry<float> SpeedIncrement;
            public static ConfigEntry<float> MachSpeedIncrement;
            public static ConfigEntry<float> RollIncrement;
            public static ConfigEntry<float> CourseIncrement;
        }
        public static ConfigEntry<bool> hideObjectivesEnabled;

        public class LoadoutPreview {
            public static ConfigEntry<bool> Enabled;
            public static ConfigEntry<bool> OnlyShowOnBoot;
            public static ConfigEntry<float> Duration;
            public static ConfigEntry<bool> SendToHMD;
            public static ConfigEntry<bool> HMDShowBorders;
            public static ConfigEntry<bool> ManualPlacement;
            public static ConfigEntry<int> PositionX;
            public static ConfigEntry<int> PositionY;
            public static ConfigEntry<int> HMDFontSize;
            public static ConfigEntry<Color> HMDMainColor;
            public static ConfigEntry<Color> HMDBackgroundColor;
            public static ConfigEntry<string> DisabledFor;
            public static ConfigEntry<string> SendToHMDFor;
        }

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

        private void PreinitConfig() {
            TomlTypeConverter.AddConverter(
                typeof(Axes<float>),
                new TypeConverter { ConvertToString = (obj, type) => obj.ToString(), ConvertToObject = (str, type) => Axes<float>.Parse(str) }
            );
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
            harmony = new Harmony("yessidor.no_tactitools_plus");
            // INPUT CATCHER
            InputCatcher.Init(harmony);
            //
            PreinitConfig();
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
                var modifierConfig = new RewiredModifierConfig(Config, "Modifiers", name, description, order--);
                RewiredConfigManager.ModsTracker.AddModifierBinding(modifierConfig);
                InputCatcher.ModsTracker.AddModifierBinding(modifierConfig);
            }
            // Press delay
            PressDelay = Config.Bind("Common",
                "Common - Press Delay",
                0.15f,
                new ConfigDescription(
                    "Press delay for NOTT+ key bindings (restart the game to apply changes). Note that in-game key bindings still use 'Press delay' setting in game 'Controls' menu.",
                    new AcceptableValueRange<float>(0f, 10f),
                    new ConfigurationManagerAttributes { Order = order-- }));
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
              "Input you want to assign for selecting or deselecting targets based on lased status",
              order--);
            MFDNavSelectClosest = new RewiredInputConfig(Config,
              "MFD Nav",
              "MFD Nav - Select Closest Targets",
              "Input you want to assign for selecting closest targets that pass current target filters. Short press selects one closes target, long press selects as many targets as possible and sorts them by distance. If one or several closest targets are found, they are loaded in target list, replacing currently selected targets (if any). If no closest targets could be found, current target list is left intact.",
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
            tlcMTSSelectOnlyInterceptableMissiles = Config.Bind("MFD Nav",
                "Target List Controller - Select Only Interceptable Missiles",
                true,
                new ConfigDescription(
                    "Missile targeting system will select only missiles that can be intercepted by current weapon. If current weapon is jammer, ARH and SARH missiles will be selected. If current weapon is missile or laser, missiles within 'MTS Angle Threshold' will be selected. If weapon type is other, all missiles will be selected. When weapon is switched, incoming missiles are appropriately reselected.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            tlcMTSAngleThreshold = Config.Bind("MFD Nav",
                "Target List Controller - MTS Angle",
                180f,
                new ConfigDescription(
                    "Angle (in degrees) around aircraft forward direction within which missile targeting system will select missiles.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            tlcRespectTargetingDistances = Config.Bind("MFD Nav",
                "Target List Controller - Respect Targeting Distances",
                true,
                new ConfigDescription(
                    """
                    If enabled, when calling 'Select Closest Targets' command, units (that pass current targeting filters) will be selected within minimum of
                    HMDDeclutter marker drawing distance and AltTargetSelection distance.
                    If either of these distances is 0, it is treated as 'not set'.
                    """,
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            tlcSelectUnitsToReload = Config.Bind("MFD Nav",
                "Target List Controller - Select Units To Reload",
                true,
                new ConfigDescription(
                    "If enabled, when map options tooltip is set to 'AMMO', 'Select Closest Targets' command will select friendly unit(s) that need ammo. Targeting filters won't be used. Targeting distances WILL be used if 'Respect Targeting Distances' is enabled.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            tlcSelectClosestTargetsByAmmo = Config.Bind("MFD Nav",
                "Target List Controller - Select New Closest Targets By Ammo",
                true,
                new ConfigDescription(
                    "If enabled, long press on 'MFD Nav - Down' key will select new closest targets (matching targeting filters and distance) based on ammo. If disabled, long press on said key will filter closest targets based on ammo from already selected targets.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            tlcExtraEntryCheckboxFunctions = Config.Bind("MFD Nav",
                "Target List Controller - Extra Entry Checkbox Functions",
                true,
                new ConfigDescription(
                    "If enabled, extra functions to target entry checkbox will be added: right-click on checkbox deselects other targets; shift-leftclick removes targets with the same name; shift-rightclick removes targets with names that differ (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            tlcHighlightActiveEntry = Config.Bind("MFD Nav",
                "Target List Controller - Highlight Active Target List Entry",
                true,
                new ConfigDescription(
                    "If enabled, target list entry belonging to active target will be highlighted (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
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
            countermeasureControlsActivate = Config.Bind("Countermeasures",
                "Countermeasure Controls - Activate",
                true,
                new ConfigDescription(
                    "Activate selected countermeasure on bound key press and deactivate it on key release.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = 3
                    }));
            countermeasureControlsFlare = new RewiredInputConfig(Config, "Countermeasures", "Countermeasure Controls - Flares", "Input you want to assign for selecting Flares", 2);
            countermeasureControlsJammer = new RewiredInputConfig(Config, "Countermeasures", "Countermeasure Controls - Jammer", "Input you want to assign for selecting Jammer", 0);
            countermeasureControlsChaff = new RewiredInputConfig(Config, "Countermeasures", "Countermeasure Controls - Chaff", "Input you want to assign for selecting Chaff", 0);
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
            //Weapon Manager Extensions
            order = 100;
            WeaponManagerExtensions.Enabled = Config.Bind("Weapon Manager Extensions",
                "Weapon Manager Extensions - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Weapon Manager Extensions feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            WeaponManagerExtensions.AttackActiveTarget = Config.Bind("Weapon Manager Extensions",
                "Weapon Manager Extensions - Attack Active Target",
                false,
                new ConfigDescription(
                    "If enabled, short press on 'Fire' button will launch deliverable only at active target; long press will fire salvo. Uses 'press time' as reference time. Restart the game to apply changes.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            WeaponManagerExtensions.AttackOnlyLasedTargets = Config.Bind("Weapon Manager Extensions",
                "Weapon Manager Extensions - Attack Only Lased Targets",
                false,
                new ConfigDescription(
                    "If enabled, laser guided weapons will launch deliverables only at lased targets in the target list. Restart the game to apply changes.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            WeaponManagerExtensions.ToggleJammerFire = Config.Bind("Weapon Manager Extensions",
                "Weapon Manager Extensions - Toggle Jammer Fire",
                false,
                new ConfigDescription(
                    "If enabled, jammer activation will be toggled by 'Fire' key press. Jammer will be deactivated if switched to another weapon or if there are no targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            WeaponManagerExtensions.KeepFiring = Config.Bind("Weapon Manager Extensions",
                "Weapon Manager Extensions - Keep Firing",
                true,
                new ConfigDescription(
                    "If disabled, will fire only one salvo after 'Fire' key was held for 'press time'. If enabled, will fire continuously after 'Fire' key was held for twice the 'press time'.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            WeaponManagerExtensions.AimAssistToggle = Config.Bind("Weapon Manager Extensions",
                "Weapon Manager Extensions - Aim Assist Toggle",
                true,
                new ConfigDescription(
                    "Enables to toggle aim assist (if available) independently of flight assist by pressing 'Toggle Aim Assist' button in component settings. Restart the game to apply changes.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            WeaponManagerExtensions.ToggleAimAssist = new RewiredButtonConfig(Config, "Weapon Manager Extensions", "Weapon Manager Extensions - Toggle Aim Assist", "Button you want to assign to toggle aim assist", order--);
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
                "1;5;10;25;50",
                new ConfigDescription(
                    "List of HMD marker draw distances, separated by \";\", fraction separator is \".\". 0.0 is unlimited distance.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.DistanceUnit = Config.Bind("HMD Declutter",
                "HMD Declutter - Unit",
                GameBindings.Units.DistanceUnits.km,
                new ConfigDescription(
                    "Distance measurement unit.",
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
            HMDDeclutter.NotAlwaysMaximized = Config.Bind("HMD Declutter",
                "HMD Declutter - Not Always Maximized",
                false,
                new ConfigDescription(
                    "If enabled, no markers will be always maximized by default.",
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
                    "Enemy Minimized Marker Scale.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.FriendlyMinimizedMarkerScale = Config.Bind("HMD Declutter",
                "HMD Declutter - Friendly Minimized Marker Scale",
                3f,
                new ConfigDescription(
                    "Friendly Minimized Marker Scale.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            //Outdated
            HMDDeclutter.OutdatedTime = Config.Bind("HMD Declutter",
                "HMD Declutter - Outdated Time",
                -1.0f,
                new ConfigDescription(
                    "Base time for outdated marker modifications (in seconds). Set to negative value to disable.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.ShowOutdatedTime = Config.Bind("HMD Declutter",
                "HMD Declutter - Show Outdated Time",
                false,
                new ConfigDescription(
                    "Show time during which a marker was outdated.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.HideOutdatedMarker = Config.Bind("HMD Declutter",
                "HMD Declutter - Hide Outdated Marker",
                false,
                new ConfigDescription(
                    "Hide outdated marker after 'Outdated Time'.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.SetOutdatedIcon = Config.Bind("HMD Declutter",
                "HMD Declutter - Set Outdated Icon",
                false,
                new ConfigDescription(
                    "Set outdated icon for all outdated markers, not just selected ones.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.EndOutdatedMarkerOpacity = Config.Bind("HMD Declutter",
                "HMD Declutter - End Outdated Marker Opacity",
                0.25f,
                new ConfigDescription(
                    "Outdated marker opacity when it's been outdated for 'Outdated Time'.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            //Missiles
            HMDDeclutter.MaximizeOwnMissiles = Config.Bind("HMD Declutter",
                "HMD Declutter - Maximize Own Missiles",
                true,
                new ConfigDescription(
                    "Should markers of own missiles be maximized.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.ColorizeOwnMissiles = Config.Bind("HMD Declutter",
                "HMD Declutter - Colorize Own Missiles",
                true,
                new ConfigDescription(
                    "Should markers of own missiles be maximized.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.AlwaysDrawOwnMissiles = Config.Bind("HMD Declutter",
                "HMD Declutter - Always Draw Own Missiles",
                true,
                new ConfigDescription(
                    "Always draw markers of own missiles, regardless of distance.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.IncludeDerivedMissiles = Config.Bind("HMD Declutter",
                "HMD Declutter - Include Derived Missiles",
                false,
                new ConfigDescription(
                    "If true, missiles (and other deliverables) launched by player-owned units will be counted as belonging to player. If false, only deliverables launched by player will be colorized.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.OwnMissilesColor = Config.Bind("HMD Declutter",
                "HMD Declutter - Own Missiles Color",
                Color.cyan,
                new ConfigDescription(
                    "Color of HMD markers and map icons belonging to own missiles.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.OwnMissedMissilesColor = Config.Bind("HMD Declutter",
                "HMD Declutter - Own Missed Missiles Color",
                Color.magenta,
                new ConfigDescription(
                    "Color of HMD markers and map icons belonging to own missiles that have missed target.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.OwnMissilesHMDMarkerScale = Config.Bind("HMD Declutter",
                "HMD Declutter - Own Missiles Scale",
                5f,
                new ConfigDescription(
                    "Scale of HMD markers designating player-owned missiles.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.OwnMissilesMapIconScale = Config.Bind("HMD Declutter",
                "HMD Declutter - Own Missiles Map Scale",
                1.2f,
                new ConfigDescription(
                    "Scale of map icons belonging to own missiles if HMD markers of these missiles are maximized.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.FlashBeforeImpactTime = Config.Bind("HMD Declutter",
                "HMD Declutter - Flash Before Impact Time",
                3f,
                new ConfigDescription(
                    "Time in seconds to flash own missile marker before impact (set to negative value to disable).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            //Units
            HMDDeclutter.MaximizeOwnUnits = Config.Bind("HMD Declutter",
                "HMD Declutter - Maximize Own Units",
                true,
                new ConfigDescription(
                    "Should markers of player-owned units be maximized.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.ColorizeOwnUnits = Config.Bind("HMD Declutter",
                "HMD Declutter - Colorize Own Units",
                true,
                new ConfigDescription(
                    "Should markers of player-owned units be maximized.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.OwnUnitsColor = Config.Bind("HMD Declutter",
                "HMD Declutter - Own Units Color",
                Color.cyan,
                new ConfigDescription(
                    "Color of HMD markers and map icons belonging to player-owned units.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.OwnUnitsHMDMarkerScale = Config.Bind("HMD Declutter",
                "HMD Declutter - Own Units Scale",
                5f,
                new ConfigDescription(
                    "Scale of HMD markers designating player-owned units.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            HMDDeclutter.OwnUnitsMapIconScale = Config.Bind("HMD Declutter",
                "HMD Declutter - Own Units Map Scale",
                1.2f,
                new ConfigDescription(
                    "Scale of map icons belonging to player-owned units if HMD markers of these units are maximized.",
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
            // Persistent Map Options settings
            order = 100;
            PersistentMapOptions.Enabled = Config.Bind("Persistent Map Options",
                "Persistent Map Options - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Persistent Map Options feature (restart the game to apply changes).",
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

            VirtualJoystickExtender.AxisX = new RewiredAxisConfig(Config, "Virtual Joystick Extender", $"Virtual Joystick Extender - X Axis", "", order--);
            VirtualJoystickExtender.AxisY = new RewiredAxisConfig(Config, "Virtual Joystick Extender", $"Virtual Joystick Extender - Y Axis", "", order--);
            VirtualJoystickExtender.AxisZ = new RewiredAxisConfig(Config, "Virtual Joystick Extender", $"Virtual Joystick Extender - Z Axis", "", order--);

            VirtualJoystickExtender.Sens = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Input Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Input axes (x, y, z) sensitivity.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            VirtualJoystickExtender.NumModes = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Number of Modes",
                -1,
                new ConfigDescription(
                    "Number of Virtual Joystick Extender modes (restart the game to apply changes). If set to -1, default modes will be created after restart.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));

            var axesDrawer = new AxesDrawer<float> ("F2");
            VirtualJoystickExtender.ModeData CreateData (
                int i,
                string name = null,
                VirtualJoystickExtenderComponent.InputAxis yawAxisMapping = VirtualJoystickExtenderComponent.InputAxis.X,
                VirtualJoystickExtenderComponent.InputAxis pitchAxisMapping = VirtualJoystickExtenderComponent.InputAxis.Y,
                VirtualJoystickExtenderComponent.InputAxis rollAxisMapping = VirtualJoystickExtenderComponent.InputAxis.X,
                Vector3? sensitivity = null, Vector3? inputMultiplierOnEnter = null,
                Vector3? dynamicCurvature = null, Axes<float> curvature = null, Axes<float> multiplier = null) {
                var modeData = new VirtualJoystickExtender.ModeData {
                    Name = Config.Bind("Virtual Joystick Extender", $"Virtual Joystick Extender - Mode {i} - Name", name == null ? $"Mode {i}" : (string)name,
                        new ConfigDescription("Mode name.", null, new ConfigurationManagerAttributes { Order = order-- })),

                    EngageKey = new RewiredInputConfig(Config, "Virtual Joystick Extender", $"Virtual Joystick Extender - Mode {i} - Engage Key",
                        "Engages this mode on press, restores previous mode on release", order--),
                    ToggleKey = new RewiredInputConfig(Config, "Virtual Joystick Extender", $"Virtual Joystick Extender - Mode {i} - Toggle Key",
                        "Toggles between this mode and previous mode on press.", order--),

                    YawAxisMapping = Config.Bind("Virtual Joystick Extender", $"Virtual Joystick Extender - Mode {i} - Yaw Axis Mapping", yawAxisMapping,
                        new ConfigDescription("Which of the input axes maps to yaw output axis.", null, new ConfigurationManagerAttributes { Order = order-- })),
                    PitchAxisMapping = Config.Bind("Virtual Joystick Extender", $"Virtual Joystick Extender - Mode {i} - Pitch Axis Mapping", pitchAxisMapping,
                        new ConfigDescription("Which of the input axes maps to pitch output axis.", null, new ConfigurationManagerAttributes { Order = order-- })),
                    RollAxisMapping = Config.Bind("Virtual Joystick Extender", $"Virtual Joystick Extender - Mode {i} - Roll Axis Mapping", rollAxisMapping,
                        new ConfigDescription("Which of the input axes maps to roll output axis.", null, new ConfigurationManagerAttributes { Order = order-- })),

                    Sens = Config.Bind(
                        "Virtual Joystick Extender",
                        $"Virtual Joystick Extender - Mode {i} - Input Sensitivity",
                        sensitivity != null ? (Vector3)sensitivity : Vector3.one,
                        new ConfigDescription("Mode-specific sensitivity for input axes (x, y, z).",
                            null, new ConfigurationManagerAttributes { Order = order-- })),

                    InputMultiplierOnEnter = Config.Bind(
                        "Virtual Joystick Extender",
                        $"Virtual Joystick Extender - Mode {i} - Input Multiplier On Entering Mode",
                        inputMultiplierOnEnter != null ? (Vector3)inputMultiplierOnEnter : Vector3.one,
                        new ConfigDescription("Multipliers applied to input axes (x, y, z) upon entering mode.",
                            null, new ConfigurationManagerAttributes { Order = order-- })),

                    DynamicCurvature = Config.Bind(
                        "Virtual Joystick Extender",
                        $"Virtual Joystick Extender - Mode {i} - Dynamic Curvature",
                        dynamicCurvature != null ? (Vector3)dynamicCurvature : Vector3.zero,
                        new ConfigDescription(
                            "Sets curvature parameters of dynamic curves that map values of input axes to values of output axes. Output axis value is calculated as Curvature*x^3 + (1-Curvature)*x, where x is input axis value.",
                            null,
                            new ConfigurationManagerAttributes { Order = order-- })),

                    StaticCurvature = Config.Bind(
                        "Virtual Joystick Extender",
                        $"Virtual Joystick Extender - Mode {i} - Curvature",
                        !(curvature is null) ? (Axes<float>)curvature : new Axes<float> (0, 0, 0),
                        new ConfigDescription(
                            "Sets curvature parameters of static curves that map values of input axes to values of output axes. Output axis value is calculated as Curvature*x^3 + (1-Curvature)*x, where x is input axis value.",
                            null,
                            new ConfigurationManagerAttributes { CustomDrawer = setting => axesDrawer.DrawAxes(setting), Order = order-- })),

                    OutputMultiplier = Config.Bind(
                        "Virtual Joystick Extender",
                        $"Virtual Joystick Extender - Mode {i} - Output Multiplier",
                        !(multiplier is null) ? (Axes<float>)multiplier : new Axes<float> (1, 1, 1),
                        new ConfigDescription("Multipliers applied to output axes (yaw, pitch, roll).",
                            null, new ConfigurationManagerAttributes { CustomDrawer = setting => axesDrawer.DrawAxes(setting), Order = order-- })),
                };
                return modeData;
            }

            VirtualJoystickExtender.Modes = new ();

            if (VirtualJoystickExtender.NumModes.Value != -1) {
                for (int i = 0; i < VirtualJoystickExtender.NumModes.Value; i++) {
                    VirtualJoystickExtender.Modes.Add(CreateData(i));
                }
            }
            else {
                VirtualJoystickExtender.Modes.Add(CreateData(0, name: "Roll", multiplier: new Axes<float> (0f, 1f, 1f), inputMultiplierOnEnter: new Vector3 (0, 1, 1)));
                VirtualJoystickExtender.Modes.Add(CreateData(1, name: "Yaw", multiplier: new Axes<float> (1f, 1f, 0f), inputMultiplierOnEnter: new Vector3 (0, 1, 1)));
                VirtualJoystickExtender.Modes.Add(CreateData(2, name: "Roll&Yaw", multiplier: new Axes<float> (0.5f, 1f, 1f), inputMultiplierOnEnter: new Vector3 (0, 1, 1)));
                VirtualJoystickExtender.NumModes.Value = VirtualJoystickExtender.Modes.Count;
            }

            VirtualJoystickExtender.DefaultMode = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Default Mode",
                0,
                new ConfigDescription(
                    "Virtual Joystick Extender default mode.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.ToggleStateKey = new RewiredInputConfig(Config,
                "Virtual Joystick Extender",
                "Virtual Joystick Extender - Toggle Key",
                "Key to turn on/off",
                order--);
            VirtualJoystickExtender.ResetKey = new RewiredButtonConfig(Config,
                "Virtual Joystick Extender",
                "Virtual Joystick Extender - Reset Key",
                "Short press sets Z input axis to 0; long press sets all input axes to 0.",
                order--);
            VirtualJoystickExtender.MaxDeflectionModeKey = new RewiredButtonConfig(Config,
                "Virtual Joystick Extender",
                "Virtual Joystick Extender - Max Mode Key",
                "Key to enable max deflection mode on press and disable on release",
                order--);
            VirtualJoystickExtender.DecayMode = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Decay Mode",
                Controls.VirtualJoystickExtenderComponent.DecayModes.None,
                new ConfigDescription(
                    "Decay mode of pitch, roll, and yaw axes when virtual joystick control is disabled by opening map, leaderboard, or radial menu.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            VirtualJoystickExtender.DecaySpeed = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Decay Speed",
                1.0f,
                new ConfigDescription(
                    "Virtual joystick vector decay speed in gradual decay mode when virtual joystick control is disabled by opening map, leaderboard, or radial menu.",
                    new AcceptableValueRange<float>(0.0f, 1000.0f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            VirtualJoystickExtender.InputAxesResetMode = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Input Axes Reset Mode",
                Controls.VirtualJoystickExtenderComponent.InputAxesResetModes.Recalculate,
                new ConfigDescription(
                    """
                    Input axes (x, y, z) reset mode upon entering virtual joystick mode.
                    If set to 'None', to action will be taken.
                    If set to 'Scale', values will be multiplied by 'Input Multiplier On Entering Mode' of virtual joystick mode being entered.
                    If set to 'Recalculate', values will be recalculated based on current values of output axes and 'Output Multiplier' of virtual joystick mode being entered (most convenient).
                    """,
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            VirtualJoystickExtender.MaxDeflection = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Max Deflection",
                150f,
                new ConfigDescription(
                    "Max virtual joystick deflection (in pixels).",
                    new AcceptableValueRange<float>(1.0f, 1000.0f),
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.LimitsShape = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Limits Shape",
                VirtualJoystickExtenderComponent.LimitsShapes.Circle,
                new ConfigDescription(
                    "Describes how Max Deflection is used to limit the value of virtual joystick vector, defined by x and y input axes (z axis is limited independently).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.CenteringDeflection = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Centering Deflection",
                -1f,
                new ConfigDescription(
                    """
                    Threshold of virtual joystick deflection from screen center (in pixels), below which centering will be activated.
                    Set to negative value to enable centering at any deflection.
                    """,
                    new AcceptableValueRange<float>(-1.0f, 1000.0f),
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.CenteringSpeed = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Centering Speed",
                0f,
                new ConfigDescription(
                    "Centering speed.",
                    new AcceptableValueRange<float>(0.0f, 100.0f),
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.DampingAngle = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Damping Angle",
                10.0f,
                new ConfigDescription(
                    """
                    Input damping 'FOV': if angle between direction from aircraft to aiming point and direction of gun bore is less than half of this value, input damping will be activated.
                    Set to negative value to disable.
                    """,
                    new AcceptableValueRange<float>(-1.0f, 90.0f),
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.DampingSens = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Damping Sens",
                0.75f,
                new ConfigDescription(
                    "Minimal value of input sensitivity multiplier, applied when angle between direction from aircraft to aiming point and direction of gun bore is 0",
                    new AcceptableValueRange<float>(0.0f, 1.0f),
                    new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = order-- }));
            VirtualJoystickExtender.DampingCurvature = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Damping Curvature",
                0.0f,
                new ConfigDescription(
                    "Damping curve curvature (0.0 - flat, 1.0 - cubic). Note: positive value interferes with A-19 Brawler gun aim assist, causing sideways wobble.",
                    new AcceptableValueRange<float>(0.0f, 1.0f),
                    new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = order-- }));
            VirtualJoystickExtender.VectorPlacement = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Vector Placement",
                VirtualJoystickExtenderComponent.VectorPlacements.HUD,
                new ConfigDescription(
                    "Should virtual joystick vector be displayed on HUD (default), on HMD, or be turned off.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.ShiftVector = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Shift Vector - Enabled",
                true,
                new ConfigDescription(
                    "Shift virtual joystick vector by z input axis value.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.RollControlsYawOnTheGround = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Roll Controls Yaw On The Ground - Enabled",
                true,
                new ConfigDescription(
                    "Should roll axis control yaw when aircraft is on the ground.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.ControlInThirdPersonMode = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Control In Third Person Mode - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable controlling aircraft with mouse joystick in third person mode. If enabled, Third Person HUD typically needs to be also enabled.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.DisableInInvalidCameraMode = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Disable in Invalid Camera Mode - Enabled",
                true,
                new ConfigDescription(
                    "If enabled, virtual joystick will be enabled only when camera mode is 'cockpit', and, if 'Control In Third Person Mode' is enabled, 'orbit' or 'chase'.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.DisableInFreeLook = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Disable In Free Look - Enabled",
                true,
                new ConfigDescription(
                    "If enabled, virtual joystick will be disabled when free look mode is enabled.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.DisableOnMaximizedMap = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Disable On Maximized Map - Enabled",
                true,
                new ConfigDescription(
                    "If enabled, virtual joystick will be disabled when map is maximized.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.DisableOnUIInteraction = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Disable On UI Interaction - Enabled",
                true,
                new ConfigDescription(
                    "If enabled, virtual joystick will be disabled when radial menu is active or leaderboard is open.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.RunInGraphicsUpdate  = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Run In Graphics Update - Enabled",
                false,
                new ConfigDescription(
                    "If enabled, worker function will run in graphics update.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            VirtualJoystickExtender.FixedDT  = Config.Bind("Virtual Joystick Extender",
                "Virtual Joystick Extender - Fixed DT",
                3f,
                new ConfigDescription(
                    "Fixed multiplier that is used in input calculation (in vanilla this multiplier depends on time delta and its max value is 3). Set to 0 or negative to use vanilla algorithm.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            // Keyboard-controlled axes
            keyAxes = new KeyAxisData[] {
                new ("Pitch", "Down", "Up"),
                new ("Roll", "Right", "Left"),
                new ("Yaw", "Right", "Left"),
                new ("Throttle", "Up", "Down"),
                new ("Brake", "Apply", "Release"),
                new ("CustomAxis1", "Up", "Down"),
                new ("Zoom", "In", "Out"),
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
                vars.IncKey = new RewiredButtonConfig(Config, "Key Axes", string.Format("Key Axes - {0} {1} Key", vars.Name, vars.IncKeyName), "Key binding", order--);
                vars.DecKey= new RewiredButtonConfig(Config, "Key Axes", string.Format("Key Axes - {0} {1} Key", vars.Name, vars.DecKeyName), "Key binding", order--);
                vars.EncoderAxis = new RewiredAxisConfig(Config, "Key Axes", string.Format("Key Axes - {0} Axis", vars.Name), "Axis binding", order--);
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
                        "How fast the axis value decays, that is, returns to Default Value if neither key is pressed and there is no input from input axis",
                        new AcceptableValueRange<float>(0.0f, 10.0f),
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
                vars.DecayDelay = Config.Bind("Key Axes",
                    string.Format("Key Axes - {0} - Decay Delay", name),
                    0.0f,
                    new ConfigDescription(
                        "Time passed since last key or axis input before axis value starts decaying.",
                        new AcceptableValueRange<float>(0.0f, 10.0f),
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
                vars.TwoKeyReset = Config.Bind("Key Axes",
                    string.Format("Key Axes - {0} - Two Key Reset", name),
                    false,
                    new ConfigDescription(
                        "If enabled, axis value will reset to default value when both keys are pressed, if disabled, axis value will pause.",
                        null,
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
                vars.EncoderSens = Config.Bind("Key Axes",
                    string.Format("Key Axes - {0} - Encoder Axis Sensitivity", name),
                    1.0f,
                    new ConfigDescription(
                        "Encoder axis sensitivity",
                        null,
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
                vars.EncoderDynamicCurvature = Config.Bind("Key Axes",
                    string.Format("Key Axes - {0} - Encoder Dynamic Curvature", name),
                    0.0f,
                    new ConfigDescription(
                        "How fast the axis value accelerates depending on encoder axis value delta",
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
                        "The value axis will decay to if neither key is pressed and Decay Speed is not 0",
                        new AcceptableValueRange<float>(-1f, 1f),
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
                vars.InitialValue = Config.Bind("Key Axes",
                    string.Format("Key Axes - {0} - Initial Value", name),
                    0.0f,
                    new ConfigDescription(
                        "The initial axis value",
                        new AcceptableValueRange<float>(-1f, 1f),
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
            }
            yprAxesResetKey = new RewiredButtonConfig(Config, "Key Axes", "Key Axes - Yaw Pitch Roll Axes Reset Key", "Short press resets yaw, long press resets pitch and roll.", order--);
            allAxesResetKey = new RewiredButtonConfig(Config, "Key Axes", "Key Axes - All Axes Reset Key", "Short press resets all axes.", order--);
            keyAxesConfirmAirbrakeDeployment = Config.Bind("Key Axes",
                "Key Axes - Confirm Airbrake Deployment",
                KeyAxesComponent.ConfirmAirbrakeDeploymentMode.Press, 
                new ConfigDescription(
                    "If set to 'Press', airbrake deployment must be confirmed by releasing and pressing 'Decrease Throttle' key when throttle is zero. If set to 'Hold', airbrake will be engaged while throttle is zero and 'Decrease Throttle` key is held.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
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
                    "Max distance to select target at, in meters (set to 0 do select targets at any distance). Note that HMD Declutter can set HMD marker draw distance, and units with invisible markers won't be selected.",
                    new AcceptableValueRange<float>(0.0f, float.PositiveInfinity),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AltTargetSelection.PickActive = Config.Bind("Alternative Target Selection",
                "Alternative Target Selection - Pick Active",
                false,
                new ConfigDescription(
                    "If no unit was selected as new target, pick active target from already selected ones.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AltTargetSelection.SelectionMode = Config.Bind("Alternative Target Selection",
                "Alternative Target Selection - Selection Mode",
                AltTargetSelectionModes.angle,
                new ConfigDescription(
                    "If set to 'distance', unit closest to camera will be selected. If set to 'angle', unit whose HMD marker is closest to target selection marker will be selected.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AltTargetSelection.ShowUnitInfo = Config.Bind("Alternative Target Selection",
                "Alternative Target Selection - Show Unit Info",
                true,
                new ConfigDescription(
                    "If true, unit type and distance info will be displayed when HMD target selection marker hovers over HMD marker of said unit (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AltTargetSelection.ShowUnitInfoForSelected = Config.Bind("Alternative Target Selection",
                "Alternative Target Selection - Show Unit Info For Selected Markers",
                true,
                new ConfigDescription(
                    "If true, unit type and distance info will be displayed for selected markers.",
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
            MiniMapZoom.CenterMinimizedMapInPrefix = Config.Bind(
                "MiniMap Zoom",
                "MiniMap Zoom - Run CenterMinimizedMap Patch in Prefix",
                true,
                new ConfigDescription(
                    "If true, DynamicMap.CenterMinimizedMap() patch will be run in Prefix() and skip original function; if false - in Postfix().",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            MiniMapZoom.IndependentZoomLevels = Config.Bind(
                "MiniMap Zoom",
                "MiniMap Zoom - Independent Zoom Levels",
                true,
                new ConfigDescription(
                    "If true, minimized and maximized states of dynamic map will have independent zoom levels.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            MiniMapZoom.SaveMaximizedPosition = Config.Bind(
                "MiniMap Zoom",
                "MiniMap Zoom - Save Maximized Map Position",
                true,
                new ConfigDescription(
                    "If true, center position of maximized map will be saved when minimizing map and restored back when maximizing.",
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
            // Customize Missile String
            order = 100;
            CustomizeMissileString.Enabled = Config.Bind("Customize Missile String",
                "Customize Missile String - Enabled",
                true,
                new ConfigDescription(
                    """
                    Customize missile string in inbound missiles list entry and below notch indicator (restart the game to apply changes).
                    Available parameters:
                    {name} - missile name;
                    {seeker} - missile seeker type;
                    {distance} - distance to missile;
                    {tti} - time to impact.
                    \n is newline.
                    """,
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            CustomizeMissileString.EntryFormatString = Config.Bind("Customize Missile String",
                "Customize Missile String - Entry Format String",
                "{name} [{seeker}] {distance} {tti}",
                new ConfigDescription(
                    "Format string for inbound missiles list entry.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            CustomizeMissileString.NotchFormatString = Config.Bind("Customize Missile String",
                "Customize Missile String - Notch Format String",
                "\n{name} [{seeker}]\n{distance} {tti}",
                new ConfigDescription(
                    "Format string for notch indicator.",
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
                    "Target marker text font size (-1 = do not change).",
                    new AcceptableValueRange<int>(-1, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.ToolTipFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - ToolTip - Font Size",
                20,
                new ConfigDescription(
                    "ToolTip text font size (-1 = do not change).",
                    new AcceptableValueRange<int>(-1, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.ObjectiveMarkerFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Objective Marker - Font Size",
                20,
                new ConfigDescription(
                    "Objective marker text font size (-1 = do not change).",
                    new AcceptableValueRange<int>(-1, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.GridLabelsFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Grid Labels - Font Size",
                20,
                new ConfigDescription(
                    "Grid labels text font size (-1 = do not change).",
                    new AcceptableValueRange<int>(-1, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.BombingStateFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Bombing State - Font Size",
                20,
                new ConfigDescription(
                    "HUD Bombing State text font size (-1 = do not change).",
                    new AcceptableValueRange<int>(-1, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.MissileStateFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Missile State - Font Size",
                20,
                new ConfigDescription(
                    "HUD Missile State text font size (-1 = do not change).",
                    new AcceptableValueRange<int>(-1, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.LaserGuidedStateFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Laser Guided State - Font Size",
                20,
                new ConfigDescription(
                    "HUD Laser Guided State text font size (-1 = do not change).",
                    new AcceptableValueRange<int>(-1, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.WingAngleGaugeFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Wing Angle Gauge - Font Size",
                20,
                new ConfigDescription(
                    "Wing Angle Gauge text font size (-1 = do not change). Leave and reenter aircraft to apply changes.",
                    new AcceptableValueRange<int>(-1, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.NozzleGaugeFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Nozzle Gauge - Font Size",
                20,
                new ConfigDescription(
                    "Nozzle Gauge text font size (-1 = do not change). Leave and reenter aircraft to apply changes.",
                    new AcceptableValueRange<int>(-1, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.NotchIndicatorLabelFontSize = Config.Bind("UI Adjustments",
                "UI Adjustments - Notch Indicator - Font Size",
                30,
                new ConfigDescription(
                    "Notch Indicator text font size (-1 = do not change).",
                    new AcceptableValueRange<int>(-1, 110),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.MapIconColorFix = Config.Bind("UI Adjustments",
                "UI Adjustments - Fix Map Icon Color",
                true,
                new ConfigDescription(
                    "[NO 0.33.4] Update map and minimap icon color after selection and deselection (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.StickyRearmerDisplayFix = Config.Bind("UI Adjustments",
                "UI Adjustments - Fix Sticky Rearmer Display",
                true,
                new ConfigDescription(
                    "[NO 0.34] Fix rearmer display (ammo info) sticking on screen when attached marker goes out of screen (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.MultipleRearmerDisplaysFix = Config.Bind("UI Adjustments",
                "UI Adjustments - Fix Multiple Rearmer Displays",
                true,
                new ConfigDescription(
                    "[NO 0.34] Fix multiple rearmer displays being created for one HMD unit marker (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.HUDUnitMarkerSelectionFix = Config.Bind("UI Adjustments",
                "UI Adjustments - Fix HMD Unit Marker Selection",
                true,
                new ConfigDescription(
                    "[NO 0.34] Disallow selecting already selected HMD unit marker and deselecting already deselected one (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.StaleTargetFix = Config.Bind("UI Adjustments",
                "UI Adjustments - Fix Stale Target",
                true,
                new ConfigDescription(
                    "[NO 0.34.2] When player aircraft is spawned, remove target, if any, from target list accessible by 'TGT' button on the right side of maximized map (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.TargetDeselectionSoundFix = Config.Bind("UI Adjustments",
                "UI Adjustments - Fix Target Deselection Sound",
                true,
                new ConfigDescription(
                    "[NO 0.34.2] Fix loud sound on mass target deselection, i.e. when applying new target filter preset (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.HUDCargoStateHUDFixedUpdateFix = Config.Bind("UI Adjustments",
                "UI Adjustments - Fix HUDCargoState HUDFixedUpdate",
                true,
                new ConfigDescription(
                    "[NO 0.34.2] Fix NRE in HUDCargoState.HUDFixedUpdate() caused by accessing currentAirbase field without first checking it for null (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.DisableAllyInfo = Config.Bind("UI Adjustments",
                "UI Adjustments - Disable Ally Info",
                false,
                new ConfigDescription(
                    "[NO 0.34] Disable ally info (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.ColorizePlayerRelatedMessages = Config.Bind("UI Adjustments",
                "UI Adjustments - Colorize Player-related Messages",
                true,
                new ConfigDescription(
                    "Colorize player name and names of player-owned munitions and vehicles in message feed with `All Clear` color (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.CenterOnJumpMap = Config.Bind("UI Adjustments",
                "UI Adjustments - Center On Jump Map",
                true,
                new ConfigDescription(
                    "If enabled, pressing 'Jump Map' key when map is maximized and players aircraft is spawned will center map on cursor coordinates (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.AirbaseOverlay.AlwaysDisplayGlidepath = Config.Bind("UI Adjustments",
                "UI Adjustments - Airbase Overlay - Always Display Glidepath",
                true,
                new ConfigDescription(
                    "If enabled, airbase overlay will display glidepath and runway borders even for aircraft with vertical landing (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.AirbaseOverlay.IgnoreRunwayLimits = Config.Bind("UI Adjustments",
                "UI Adjustments - Airbase Overlay - Ignore Runway Limits",
                true,
                new ConfigDescription(
                    "If enabled, runway landing speed and size limits will be ignored when airbase overlay selects which runway to display glidepath and borders for (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            UIAdjustments.NOAutopilotGCASChevron = Config.Bind("UI Adjustments",
                "UI Adjustments - Display NOAutopilot GCAS Chevron on HMD",
                false,
                new ConfigDescription(
                    "Display NOAutopilot GCAS Chevron on HMD instead of HUD (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            //Sensitivity Fix
            order = 100;
            SensitivityFix.Enabled = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Enabled",
                true,
                new ConfigDescription(
                    "[NO 0.34.1] Fix FPS-bound mouse sensitivity for axes bound to 'Pan View', 'Tilt View', and 'Zoom View' in various camera modes, while controlling maximized map, and while using built-in virtual joystick (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.CommonSens = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Common Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Common sensitivity multiplier for all cases (x - 'Pan View' axis, y - 'Tilt View' axis, z - 'Zoom View' or 'FOV' axis, where applicable; 0 means 'don't modify').",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.Sensitivities = new ConfigEntry<Vector3> [Enum.GetValues(typeof(SensitivityFixComponent.Scope)).Length - 1];
            SensitivityFix.Sensitivities[(int)SensitivityFixComponent.Scope.Cockpit] = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Cockpit Camera Mouse Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Sensitivity multiplier for cockpit camera (x - 'Pan View' axis, y - 'Tilt View' axis, z - 'Zoom View' axis; 0 means 'don't modify').",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.Sensitivities[(int)SensitivityFixComponent.Scope.Controlled] = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Controlled Camera Mouse Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Sensitivity multiplier for controlled camera (x - 'Pan View' axis, y - 'Tilt View' axis, z - 'FOV' axis; 0 means 'don't modify').",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.Sensitivities[(int)SensitivityFixComponent.Scope.Free] = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Free Camera Mouse Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Sensitivity multiplier for free camera (x - 'Pan View' axis, y - 'Tilt View' axis, z - 'FOV' axis; 0 means 'don't modify').",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.Sensitivities[(int)SensitivityFixComponent.Scope.Orbit] = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Orbit Camera Mouse Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Sensitivity multiplier for orbiting camera (x - 'Pan View' axis, y - 'Tilt View' axis, z - 'FOV' axis; 0 means 'don't modify').",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.Sensitivities[(int)SensitivityFixComponent.Scope.Selection] = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Selection Camera Mouse Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Sensitivity multiplier for selection camera (x - 'Pan View' axis, y - 'Tilt View' axis; 0 means 'don't modify').",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.Sensitivities[(int)SensitivityFixComponent.Scope.TV] = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - TV Camera Mouse Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Sensitivity multiplier for TV camera (x - 'Pan View' axis, y - 'Tilt View' axis, z - 'Zoom View' axis; 0 means 'don't modify').",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.Sensitivities[(int)SensitivityFixComponent.Scope.Encyclopedia] = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Encyclopedia Camera Mouse Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Sensitivity multiplier for encyclopedia camera (x - 'Pan View' axis, y - 'Tilt View' axis; 0 means 'don't modify').",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.Sensitivities[(int)SensitivityFixComponent.Scope.Navigator] = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Editor Camera Navigator Mouse Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Sensitivity multiplier for editor camera navigator (x - 'Pan View' axis, y - 'Tilt View' axis; 0 means 'don't modify').",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.Sensitivities[(int)SensitivityFixComponent.Scope.MapControls] = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Map Controls Mouse Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Sensitivity multiplier applied when controlling maximized map with mouse(x - 'Pan View' axis (horizontal map movement), y - 'Tilt View' axis (vertical map movement), z - 'Zoom View' axis; 0 means 'don't modify').",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.Sensitivities[(int)SensitivityFixComponent.Scope.PlayerAxisControls] = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Built-in Virtual Joystick Mouse Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Sensitivity multiplier for built-in virtual joystick (x - 'Pan View' axis, y - 'Tilt View' axis; 0 means 'don't modify').",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            SensitivityFix.Sensitivities[(int)SensitivityFixComponent.Scope.RadialMenu] = Config.Bind("Sensitivity Fix",
                "Sensitivity Fix - Radial Menu Mouse Sensitivity",
                Vector3.one,
                new ConfigDescription(
                    "Sensitivity multiplier for radial menu (x - 'Pan View' axis, y - 'Tilt View' axis; 0 means 'don't modify').",
                    null,
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
                "Free Look Toggle - Disable Free Look In Padlock mode",
                false,
                new ConfigDescription(
                    "Automatically disable free look upon leaving pad lock.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            FreeLookToggle.DisableFreeLookInForwardlock = Config.Bind("Free Look Toggle",
                "Free Look Toggle - Disable Free Look In Forwardlock mode",
                false,
                new ConfigDescription(
                    "Automatically disable free look upon leaving forward lock.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            FreeLookToggle.DisableFreeLookOnCenter = Config.Bind("Free Look Toggle",
                "Free Look Toggle - Disable Free Look On Centering View",
                true,
                new ConfigDescription(
                    "Automatically disable free look when centering view.",
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
            FreeLookToggle.CenteringPositionMultiplier = Config.Bind("Free Look Toggle",
                "Free Look Toggle - Centering Position Multiplier",
                Vector3.zero,
                new ConfigDescription(
                    "On centering view head position vector is multiplied by this vector component-by-component. I.e. set y to 1 to preserve head elevation when centering view.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            FreeLookToggle.CenterKey = new RewiredButtonConfig(Config, "Free Look Toggle", "Free Look Toggle - Center Key", "Pressing this key will center view", order--);
            FreeLookToggle.PadlockKey = new RewiredButtonConfig(Config, "Free Look Toggle", "Free Look Toggle - Padlock Key", "Pressing this key will toggle padlock", order--);
            FreeLookToggle.FreeLookKey = new RewiredButtonConfig(Config, "Free Look Toggle", "Free Look Toggle - FreeLook Key", "Short press will toggle free look, long press will toggle forward lock", order--);
            // Key View Control
            order = 100;
            KeyViewControl.Enabled = Config.Bind("Key View Control",
                "Key View Control - Enabled",
                false,
                new ConfigDescription(
                    "Enable controlling view with keys (restart the game to apply changes). If enabled, Free Look Toggle also needs to be enabled.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            KeyViewControl.PanLeftKey = new RewiredButtonConfig(Config, "Key View Control", "Key View Control - Pan Left", "", order--);
            KeyViewControl.PanRightKey = new RewiredButtonConfig(Config, "Key View Control", "Key View Control - Pan Right", "", order--);
            KeyViewControl.TiltUpKey = new RewiredButtonConfig(Config, "Key View Control", "Key View Control - Tilt Up", "", order--);
            KeyViewControl.TiltDownKey = new RewiredButtonConfig(Config, "Key View Control", "Key View Control - Tilt Down", "", order--);
            KeyViewControl.FOVDependent = Config.Bind("Key View Control",
                "Key View Control - FOVDependent",
                true,
                new ConfigDescription(
                    "Should pan and tilt step and speed depend on current FOV.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            KeyViewControl.StopAt0 = Config.Bind("Key View Control",
                "Key View Control - Stop At 0",
                true,
                new ConfigDescription(
                    "Should stop at 0 angles while changing pan and tilt angles in steps.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            KeyViewControl.PanStep = Config.Bind("Key View Control",
                "Key View Control - Pan Step",
                45.0f,
                new ConfigDescription(
                    "Pan angle change step in degrees.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            KeyViewControl.TiltStep = Config.Bind("Key View Control",
                "Key View Control - Tilt Step",
                45.0f,
                new ConfigDescription(
                    "Tilt angle change step in degrees.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            KeyViewControl.PanSpeed = Config.Bind("Key View Control",
                "Key View Control - Pan Speed",
                45.0f,
                new ConfigDescription(
                    "Pan angle change speed in degrees per second.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            KeyViewControl.TiltSpeed = Config.Bind("Key View Control",
                "Key View Control - Tilt Speed",
                45.0f,
                new ConfigDescription(
                    "Tilt angle change speed in degrees per second.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // Head Axes
            order = 100;
            HeadAxes.Enabled = Config.Bind("Head Axes",
                "Head Axes - Enabled",
                false,
                new ConfigDescription(
                    "Enable controlling view pan, tilt, and zoom with absolute axes (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HeadAxes.PanLimit = Config.Bind("Head Axes",
                "Head Axes - Pan Limit",
                165f,
                new ConfigDescription(
                    "Pan (horizontal) axis limit",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HeadAxes.TiltLimit = Config.Bind("Head Axes",
                "Head Axes - Tilt Limit",
                65f,
                new ConfigDescription(
                    "Tilt (vertical) axis limit",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HeadAxes.MinFOV = Config.Bind("Head Axes",
                "Head Axes - Min FOV",
                20f,
                new ConfigDescription(
                    "Min FOV",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HeadAxes.MaxFOV = Config.Bind("Head Axes",
                "Head Axes - Max FOV",
                120f,
                new ConfigDescription(
                    "Max FOV",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HeadAxes.FOVSpeed = Config.Bind("Head Axes",
                "Head Axes - FOV Speed",
                0.2f,
                new ConfigDescription(
                    "FOV change speed",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HeadAxes.PanAxis = new RewiredAxisConfig(Config, "Head Axes", "Head Axes - Pan Axis", "Pan (horizontal) axis. Set deadzone to 0 in axis calibration settings.", order--);
            HeadAxes.TiltAxis = new RewiredAxisConfig(Config, "Head Axes", "Head Axes - Tilt Axis", "Pan (vertical) axis. Set deadzone to 0 in axis calibration settings.", order--);
            HeadAxes.FOVAxis = new RewiredAxisConfig(Config, "Head Axes", "Head Axes - FOV Axis", "FOV axis. Set deadzone to 0 in axis calibration settings.", order--);
            // Third Person HUD
            order = 100;
            ThirdPersonHUD.Enabled = Config.Bind("Third Person HUD",
                "Third Person HUD - Enabled",
                false,
                new ConfigDescription(
                    "Enable HUD and HMD in camera orbit and chase modes (restart the game to apply changes). If enabled, Virtual Joystick Extender - Control In Third Person Mode typically needs to be also enabled.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            ThirdPersonHUD.HUDRoll = Config.Bind("Third Person HUD",
                "Third Person HUD - HUD Roll - Enabled",
                false,
                new ConfigDescription(
                    "Should HUD pivot with the aircraft roll.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            ThirdPersonHUD.HUDBoundToScreen = Config.Bind("Third Person HUD",
                "Third Person HUD - HUD Bound To Screen - Enabled",
                false,
                new ConfigDescription(
                    "Should HUD be bound to screen position instead of aircraft direction.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            ThirdPersonHUD.HUDScreenOffset = Config.Bind("Third Person HUD",
                "Third Person HUD - HUD Screen Offset",
                Vector2.zero,
                new ConfigDescription(
                    "HUD position offset relative to screen center (positive x is right, positive y is up).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            ThirdPersonHUD.SetTargetDesignatorPos = Config.Bind("Third Person HUD",
                "Third Person HUD - Set Target Designator Position - Enabled",
                false,
                new ConfigDescription(
                    "If enabled, target designator will be placed at screen center in camera cockpit mode, and at offset from screen center in camera orbit and chase modes.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            ThirdPersonHUD.TargetDesignatorScreenOffset = Config.Bind("Third Person HUD",
                "Third Person HUD - Target Designator Screen Offset",
                Vector2.zero,
                new ConfigDescription(
                    "Target Designator position offset relative to screen center (positive x is right, positive y is up).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // Dynamic Landing Cam
            order = 100;
            DynamicLandingCam.Enabled = Config.Bind("Dynamic Landing Cam",
                "Dynamic Landing Cam - Enabled",
                false,
                new ConfigDescription(
                    "Enable or disable Dynamic Landing Cam feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            DynamicLandingCam.KeepOnAfterTouchDown = Config.Bind("Dynamic Landing Cam",
                "Dynamic Landing Cam - Keep On After Touchdown - Enabled",
                true,
                new ConfigDescription(
                    "Should landing cam be kept on after touchdown.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            DynamicLandingCam.Rotate = Config.Bind("Dynamic Landing Cam",
                "Dynamic Landing Cam - Rotate - Enabled",
                false,
                new ConfigDescription(
                    "If enabled, landing cam will be rotated (within limits) towards velocity vector.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            DynamicLandingCam.RotationSpeed = Config.Bind("Dynamic Landing Cam",
                "Dynamic Landing Cam - Rotation Speed",
                1f,
                new ConfigDescription(
                    "Landing Cam rotation speed.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            DynamicLandingCam.TiltLimits = Config.Bind("Dynamic Landing Cam",
                "Dynamic Landing Cam - Tilt Limits",
                new Vector2 (0f, 30f),
                new ConfigDescription(
                    "Landing Cam tilt angle limits (0 is horizontal, 90 is down).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            DynamicLandingCam.PanLimits = Config.Bind("Dynamic Landing Cam",
                "Dynamic Landing Cam - Pan Limits",
                new Vector2 (-10f, 10f),
                new ConfigDescription(
                    "Landing Cam pan angle limits (0 is forward, -90 is left, 90 is right).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            DynamicLandingCam.InitialAngles = Config.Bind("Dynamic Landing Cam",
                "Dynamic Landing Cam - Initial Angles",
                new Vector2 (0, 0),
                new ConfigDescription(
                    "Initial landing camera angles (x is tilt angle, y is pan angle).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            DynamicLandingCam.LandingCamFOV = Config.Bind("Dynamic Landing Cam",
                "Dynamic Landing Cam - Landing Cam FOV",
                90f,
                new ConfigDescription(
                    "Landing Cam FOV.",
                    new AcceptableValueRange<float>(0f, 180f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            DynamicLandingCam.Deadzone = Config.Bind("Dynamic Landing Cam",
                "Dynamic Landing Cam - Deadzone",
                10f,
                new ConfigDescription(
                    "Deadzone angle (in degrees): landing camera won't rotate if angle between velocity vector and camera direction is less that this angle.",
                    new AcceptableValueRange<float>(0f, 180f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            DynamicLandingCam.FixBrawlerLandingCam = Config.Bind("Dynamic Landing Cam",
                "Dynamic Landing Cam - Fix A-19 Brawler Landing Cam",
                true,
                new ConfigDescription(
                    "Apply the A-19 Brawler landing cam position fix (as of NO 0.33.4, it is set inside the aircraft).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // HMD Cam
            order = 100;
            HMDCam.Enabled = Config.Bind("HMD Cam",
                "HMD Cam - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable HMD Cam feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HMDCam.Position = Config.Bind("HMD Cam",
                "HMD Cam - Position",
                new Vector2 (0, 400),
                new ConfigDescription(
                    "HMD Cam position.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HMDCam.Size = Config.Bind("HMD Cam",
                "HMD Cam - Size",
                new Vector2 (360, 240),
                new ConfigDescription(
                    "HMD Cam size.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HMDCam.Transparency = Config.Bind("HMD Cam",
                "HMD Cam - Transparency",
                0.75f,
                new ConfigDescription(
                    "HMD Cam transparency (0 - fully tranparent, 1 - fully opaque).",
                    new AcceptableValueRange<float>(0.0f, 0.99f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            HMDCam.SuppressMFDCam = Config.Bind("HMD Cam",
                "HMD Cam - Suppress MFD Camera",
                true,
                new ConfigDescription(
                    "Should MFD target and landing camera be suppressed.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            // EMWS
            order = 100;
            EMWS.Enabled = Config.Bind("EMWS",
                "EMWS - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable Early Missile Warning System feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            EMWS.DistanceUnit = Config.Bind("EMWS",
                "EMWS - Distance Unit",
                GameBindings.Units.DistanceUnits.km,
                new ConfigDescription(
                    "Distance measurement unit.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            EMWS.ProcessOnlyEnemyMissiles = Config.Bind("EMWS",
                "EMWS - Process Only Enemy Missiles",
                true,
                new ConfigDescription(
                    "Should Early Missile Warning System process only enemy missiles.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            EMWS.HideOnMissileWarning = Config.Bind("EMWS",
                "EMWS - Hide On Missile Warning",
                false,
                new ConfigDescription(
                    "Should missile warnings of possible incoming missiles be hidden when missile warning for actually incoming missile is active.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            EMWS.NumEntries = Config.Bind("EMWS",
                "EMWS - Number of Entries",
                -1,
                new ConfigDescription(
                    "Number of entries (restart the game to apply changes). If negative, or 0, will create default entry for ARH and SARH missiles.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            EMWS.MissileConfigDatum = new ();
            //Should init default ARH/SARH entry
            if (EMWS.NumEntries.Value <= 0)
                EMWS.NumEntries.Value = 1;
            var numEntries = EMWS.NumEntries.Value;
            for (int i = 0; i < numEntries; i++) {
                var missileConfigData = new EMWS.MissileConfigData ();
                EMWS.MissileConfigDatum.Add(missileConfigData);

                missileConfigData.SeekerTypes = Config.Bind("EMWS",
                    $"EMWS - Entry {i} - Seeker Types",
                    "ARH;SARH",
                    new ConfigDescription(
                        "Regex patterns of seeker types, separated by ';'.",
                        null,
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
                missileConfigData.Angle = Config.Bind("EMWS",
                    $"EMWS - Entry {i} - Angle",
                    30f,
                    new ConfigDescription(
                        "Double angle between missile velocity vector and vector from missile to players' aircraft.",
                        new AcceptableValueRange<float>(0f, 360f),
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
                missileConfigData.Distance = Config.Bind("EMWS",
                    $"EMWS - Entry {i} - Distance",
                    30f,
                    new ConfigDescription(
                        "Max distance from missile to players' aircraft for missile to be considered. Set to negative value to disable distance check.",
                        null,
                        new ConfigurationManagerAttributes {
                            Order = order--
                        }));
                missileConfigData.ItemColor = Config.Bind("EMWS",
                    $"EMWS - Entry {i} - Item Color",
                    Color.grey,
                    new ConfigDescription(
                        "Color of notch indicator rectangle, label, map lines and missile text.",
                        null,
                        new ConfigurationManagerAttributes {
                            Order = order--
                    }));
                missileConfigData.FlashMarker = Config.Bind("EMWS",
                    $"EMWS - Entry {i} - Flash Marker",
                    true,
                    new ConfigDescription(
                        "If enabled, missile HMD marker will flash.",
                        null,
                        new ConfigurationManagerAttributes {
                            Order = order--
                    }));
                missileConfigData.ShowNotchLine = Config.Bind("EMWS",
                    $"EMWS - Entry {i} - Show Notch Line",
                    true,
                    new ConfigDescription(
                        "If enabled, notch line will be shown on map.",
                        null,
                        new ConfigurationManagerAttributes {
                            Order = order--
                    }));
                missileConfigData.ShowVectorLine = Config.Bind("EMWS",
                    $"EMWS - Entry {i} - Show Vector Line",
                    true,
                    new ConfigDescription(
                        "If enabled, line from missile to players' aircraft will be shown on map.",
                        null,
                        new ConfigurationManagerAttributes {
                            Order = order--
                    }));
                missileConfigData.ShowNotchIndicator = Config.Bind("EMWS",
                    $"EMWS - Entry {i} - Show Notch Indicator",
                    true,
                    new ConfigDescription(
                        "If enabled, notch indicator will be shown on HMD.",
                        null,
                        new ConfigurationManagerAttributes {
                            Order = order--
                    }));
                missileConfigData.ShowText = Config.Bind("EMWS",
                    $"EMWS - Entry {i} - Show Text",
                    true,
                    new ConfigDescription(
                        "If enabled, missile seeker type and distance will be shown on HMD above minimap.",
                        null,
                        new ConfigurationManagerAttributes {
                            Order = order--
                    }));
                missileConfigData.HMDMarkerScale = Config.Bind("EMWS",
                    $"EMWS - Entry {i} - HMD Marker Scale",
                    1.0f,
                    new ConfigDescription(
                        "HMD Marker scale.",
                        null,
                        new ConfigurationManagerAttributes {
                            Order = order--
                    }));
                missileConfigData.MapIconScale = Config.Bind("EMWS",
                    $"EMWS - Entry {i} - Map Icon Scale",
                    1.0f,
                    new ConfigDescription(
                        "Map icon scale.",
                        null,
                        new ConfigurationManagerAttributes {
                            Order = order--
                    }));
            }
            // MultiLevelMenu
            order = 100;
            MultiLevelMenu.Enabled = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable MultiLevelMenu feature (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        CustomDrawer = RadialMenuEnableDrawer.CreateDelegate(),
                        Order = order--
                    }));

            MultiLevelMenu.NumMain = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Number of Levels in Main Menu",
                -1,
                new ConfigDescription(
                    "Number of levels in Main radial menu (restart the game to apply changes). Set to -1 and restart to restore defaults.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));

            string[] defaultsMain = new string[] {
                "MiniMapZoomReset;MiniMapZoomDown;MiniMapZoomUp;TargetCamModeToggle;HMDMarkerDistanceDown;HMDMarkerDistanceUp",
                "Default",
                "MTSToggle;MTSAutoSelect"
            };

            if (MultiLevelMenu.NumMain.Value <= 0)
                MultiLevelMenu.NumMain.Value = defaultsMain.Length;

            MultiLevelMenu.Main = new ();
            for (int i = 0; i < MultiLevelMenu.NumMain.Value; i++) {
                var entry = Config.Bind("MultiLevelMenu",
                    $"MultiLevelMenu - Main - Level {i}",
                    i < defaultsMain.Length ? defaultsMain[i] : "",
                    new ConfigDescription(
                        $"Commands for level {i} of Main radial menu, separated by ';'.",
                        null,
                        new ConfigurationManagerAttributes {
                            CustomDrawer = ScrollableTextFieldDrawer.CreateDelegate(),
                            Order = order--
                        }));
                MultiLevelMenu.Main.Add(entry);
            }

            MultiLevelMenu.NumWeapons = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Number of Levels in Weapons Menu",
                -1,
                new ConfigDescription(
                    "Number of levels in Weapons radial menu (restart the game to apply changes). Set to -1 and restart to restore defaults.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));

            string[] defaultsWeapons = new string[] {
                "RememberHUDOptions(0);RememberHUDOptions(1);RememberHUDOptions(2);RememberHUDOptions(3);RememberHUDOptions(4);RememberHUDOptions(5);RememberHUDOptions(6)",
                "RememberFilter(0);RememberFilter(1);RememberFilter(2);RememberFilter(3);RememberFilter(4);RememberFilter(5);RememberFilter(6)",
                "RecallHUDOptions(0);RecallHUDOptions(1);RecallHUDOptions(2);RecallHUDOptions(3);RecallHUDOptions(4);RecallHUDOptions(5);RecallHUDOptions(6)",
                "RecallFilter(0);RecallFilter(1);RecallFilter(2);RecallFilter(3);RecallFilter(4);RecallFilter(5);RecallFilter(6)",
                "Default",
                "KeepTarget;SelectClosestTargets;NextTarget;SortDist;PopTarget;SortName;PrevTarget;SelectClosestTarget","KeepDatalinked;KeepTracked;PopTracked;KeepByAmmo;PopSameName;KeepSameName",
                "RememberTargets(0);RecallTargets(0);RememberTargets(1);RecallTargets(1);RememberTargets(2);RecallTargets(2);RememberTargets(3);RecallTargets(3)"
            };

            if (MultiLevelMenu.NumWeapons.Value <= 0)
                MultiLevelMenu.NumWeapons.Value = defaultsWeapons.Length;

            MultiLevelMenu.Weapons = new ();
            for (int i = 0; i < MultiLevelMenu.NumWeapons.Value; i++) {
                var entry = Config.Bind("MultiLevelMenu",
                    $"MultiLevelMenu - Weapons - Level {i}",
                    i < defaultsWeapons.Length ? defaultsWeapons[i] : "",
                    new ConfigDescription(
                        $"Commands for level {i} of Weapons radial menu, separated by ';'.",
                        null,
                        new ConfigurationManagerAttributes {
                            CustomDrawer = ScrollableTextFieldDrawer.CreateDelegate(),
                            Order = order--
                        }));
                MultiLevelMenu.Weapons.Add(entry);
            }

            MultiLevelMenu.MenuDelay = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Deselection Delay",
                2.0f,
                new ConfigDescription(
                    "Delay after which selected command is deselected if corresponding menu key was not released to invoke the command.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MultiLevelMenu.ZPerLevel = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Z Per Level",
                0.0f,
                new ConfigDescription(
                    "Accumulated 'Zoom View` axis delta needed to switch level if 'Switch Mode' is 'Continuous'.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MultiLevelMenu.ResetLevel = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Reset Level",
                true,
                new ConfigDescription(
                    "If enabled, menu will be reset to default layer on opening.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MultiLevelMenu.OpenOnPress = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Open on Press",
                false,
                new ConfigDescription(
                    "If enabled, corresponding menu will be opened on 'Radial Menu' or 'Weapon Wheel` key press, instead of hold.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MultiLevelMenu.Toggle = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Toggle",
                false,
                new ConfigDescription(
                    "If enabled, corresponding menu will be toggled by key press, instead of being opened on key hold (or press, if 'Open on Press' is enabled) and closed on key release.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MultiLevelMenu.ShowPreview = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Show Preview",
                true,
                new ConfigDescription(
                    "If enabled, previews for target lists, target filters and HUD options will be shown when radial menu segments that load corresponding items are selected.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MultiLevelMenu.SwitchMode = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Switch Mode",
                global::NO_Tactitools.Controls.MultiLevelMenu.SwitchModes.Step,
                new ConfigDescription(
                    """
                    Level switching mode.
                    'Continuous' - levels are switched when accumulated input delta on 'Zoom View' axis exceeds 'Z Per Level' value;
                    'Step' - level is switched if there is input on `Zoom View` axis on current frame, but not on previous.
                    """,
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MultiLevelMenu.LevelSwitchPeriod = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Level Switch Period",
                0.25f,
                new ConfigDescription(
                    "Minimum time period that should pass after switching level before switching level again.",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MultiLevelMenu.DefaultColor = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Default Color",
                Color.gray,
                new ConfigDescription(
                    "Default color (changes will be applied after menu is recreated).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MultiLevelMenu.SelectedColor = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Selected Color",
                Color.green,
                new ConfigDescription(
                    "Selected color (changes will be applied after menu is recreated).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MultiLevelMenu.BackgroundColorActive = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Active Background Color",
                new Color (0, 0, 0, 0.8f),
                new ConfigDescription(
                    "Active background color (changes will be applied after menu is recreated).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            MultiLevelMenu.BackgroundColorInactive = Config.Bind("MultiLevelMenu",
                "MultiLevelMenu - Inactive Background Color",
                new Color (0, 0, 0, 0.5f),
                new ConfigDescription(
                    "Inactive background color (changes will be applied after menu is recreated).",
                    null,
                    new ConfigurationManagerAttributes { Order = order-- }));

            // Ammo Conservation Indicator settings
            order = 100;
            AmmoConIndicator.Enabled = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Ammo Conservation Indicator feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AmmoConIndicator.ColorHMDMarker = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - HMD Markers Color - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable coloring HMD markers of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AmmoConIndicator.ColorHMDLasedMarker = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - HMD Lased Markers Color - Enabled",
                true,
                new ConfigDescription(
                    "If enabled, and if laser-guided weapon is active, HMD markers of units that are both selected and lased will be colored with 'HMD Lased Marker Color'.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AmmoConIndicator.ColorMFDBox = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - MFD Box Color - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable coloring MFD Boxes of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AmmoConIndicator.DrawMFDDot = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - MFD Dot - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable drawing MFD dots under the boxes of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AmmoConIndicator.HMDTrackedMarkerColor = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - HMD Tracked Marker Color - Color",
                Color.yellow,
                new ConfigDescription(
                    "Color of HMD markers of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AmmoConIndicator.HMDLasedMarkerColor = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - HMD Lased Marker Color - Color",
                new Color (1.0f, 0.65f, 0.0f, 1.0f),
                new ConfigDescription(
                    "Color of HMD markers of selected and lased targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AmmoConIndicator.HMDDefaultMarkerColor = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - HMD Default Marker Color - Color",
                Color.green,
                new ConfigDescription(
                    "Color of HMD markers of non-attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AmmoConIndicator.MFDTrackedBoxColor = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - MFD Tracked Box Color - Color",
                Color.yellow,
                new ConfigDescription(
                    "Color of MFD boxes of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AmmoConIndicator.MFDDefaultBoxColor = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - MFD Default Box Color - Color",
                Color.white,
                new ConfigDescription(
                    "Color of MFD boxes of non-attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            AmmoConIndicator.MFDTrackedDotColor = Config.Bind("Ammo Conservation Indicator",
                "Ammo Conservation Indicator - MFD Tracked Dot Color - Color",
                new Color(0.0f, 1.0f, 0.0f, 0.95f),
                new ConfigDescription(
                    "Color of MFD dots of attacked targets.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
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
            weaponDisplayDisabledFor = Config.Bind("CM & Weapon Display",
                "CM & Weapon Display - Disabled For",
                "",
                new ConfigDescription(
                    "CM & Weapon Display is disabled for aircrafts, specified by this sequence of aircraft platform name patterns, separated by ';'. Aircraft platform name pattern is a regual expression pattern, so it can be full or partial aircaft name (i.e. 'CI-22' or 'Cricket', or 'CI-22 Cricket'). If aircraft platform name does not match aricraft displayed name, enable logging, and look for platform name in 'LogOutput.log' messages '[WD] Initializing Weapon Display for platform'. Respawn to apply changes.",
                    null,
                    new ConfigurationManagerAttributes {
                        CustomDrawer = ScrollableTextFieldDrawer.CreateDelegate(),
                        Order = 1
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
            deliveryCheckerShowTTI = Config.Bind("Delivery Checker",
                "Delivery Checker - Show TTI",
                true,
                new ConfigDescription(
                    "Enable or disable displaying deliverables TTI.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = -1
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
            unitIconRecolorUnits = Config.Bind("AA Units Icon Recolor",
                "AA Units Icon Recolor - Unit Names",
                "",
                new ConfigDescription(
                    """
                    Regex patterns of names of units which icons will be recolored, separated by ';'.
                    If empty, file 'UnitIconRecolor_TargetUnits.txt' will be used.
                    Respawn to apply changes.
                    """,
                    null,
                    new ConfigurationManagerAttributes {
                        CustomDrawer = ScrollableTextFieldDrawer.CreateDelegate(),
                        Order = 0
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
            artificialHorizonAuthorizedFor = Config.Bind("Artificial Horizon",
                "Artificial Horizon - Authorized For",
                "",
                new ConfigDescription(
                    """
                    Regex patterns of platform names Artificial Horizon is authorized for, separated by ';'.
                    If empty, file 'ArtificialHorizon_AuthorizedPlatforms.txt' will be searched for exact platform name.
                    Respawn to apply changes.
                    """,
                    null,
                    new ConfigurationManagerAttributes {
                        CustomDrawer = ScrollableTextFieldDrawer.CreateDelegate(),
                        Order = 0
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
            bankIndicatorAuthorizedFor = Config.Bind("Bank Indicator",
                "Bank Indicator - Authorized For",
                "",
                new ConfigDescription(
                    """
                    Regex patterns of platform names Bank Indicator is authorized for, separated by ';'.
                    If empty, file 'BankIndicator_AuthorizedPlatforms.txt' will be searched for exact platform name.
                    Respawn to apply changes.
                    """,
                    null,
                    new ConfigurationManagerAttributes {
                        CustomDrawer = ScrollableTextFieldDrawer.CreateDelegate(),
                        Order = 0
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
            slipIndicatorAuthorizedFor = Config.Bind("Slip/Skid Indicator",
                "Slip/Skid Indicator - Authorized For",
                "",
                new ConfigDescription(
                    """
                    Regex patterns of platform names Slip Indicator is authorized for, separated by ';'.
                    If empty, file 'SlipIndicator_AuthorizedPlatforms.txt' will be searched for exact platform name.
                    Respawn to apply changes.
                    """,
                    null,
                    new ConfigurationManagerAttributes {
                        CustomDrawer = ScrollableTextFieldDrawer.CreateDelegate(),
                        Order = 0
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
            order = 100;
            NOAutopilotControl.Enabled = Config.Bind("Autopilot",
                "Autopilot - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Autopilot Menu component (restart the game to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.MFDMainColor = Config.Bind("Autopilot",
                "Autopilot - Main Color",
                Color.green,
                new ConfigDescription(
                    "Label text and border color when Autopilot Menu is displayed on MFD.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.SendToHMD = Config.Bind("Autopilot",
                "Autopilot - Send To HMD",
                false,
                new ConfigDescription(
                    "If enabled, the Autopilot Menu will be sent to the HMD display (respawn to apply changes).",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.SendToHMDFor = Config.Bind("Autopilot",
                "Autopilot` - Send To HMD For",
                "F-16M King Viper;RAH-72 Knockout;F-99 Shrike;MC-260 Chimera;FS-41 Eclipse",
                new ConfigDescription(
                    "Autopilot Menu is displayed on HMD (regardless of 'Send To HMD' setting) for aircrafts, specified by this sequence of aircraft platform name patterns, separated by ';'. Aircraft platform name pattern is a regular expression pattern, so it can be full or partial aircraft name (i.e. 'CI-22' or 'Cricket', or 'CI-22 Cricket'). Respawn to apply changes.",
                    null,
                    new ConfigurationManagerAttributes {
                        CustomDrawer = ScrollableTextFieldDrawer.CreateDelegate(),
                        Order = order--
                    }));
            NOAutopilotControl.HMDMainColor = Config.Bind("Autopilot",
                "Autopilot - Send To HMD - Main Color",
                Color.green,
                new ConfigDescription(
                    "Label text and border color when Autopilot Menu is displayed on HMD.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.HMDPositionX = Config.Bind("Autopilot",
                "Autopilot - Send To HMD - Position X",
                0,
                new ConfigDescription(
                    "X position offset for the Autopilot Menu when it is displayed on HMD.",
                    new AcceptableValueRange<int>(-1920 / 2, +1920 / 2),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.HMDPositionY = Config.Bind("Autopilot",
                "Autopilot - Send To HMD - Position Y",
                0,
                new ConfigDescription(
                    "Y position offset for the Autopilot Menu when manual placement is enabled.",
                    new AcceptableValueRange<int>(-(int)1080 / 2, +(int)1080 / 2),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.HMDFontSize = Config.Bind("Autopilot",
                "Autopilot - Send To HMD - Font Size",
                34,
                new ConfigDescription(
                    "Label font size when Autopilot Menu is displayed on HMD.",
                    new AcceptableValueRange<int>(1, 101),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.AltIncrement = Config.Bind("Autopilot",
                "Autopilot - Altitude Increment",
                1.0f,
                new ConfigDescription(
                    "Altitude increment.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.ClimbIncrement = Config.Bind("Autopilot",
                "Autopilot - Climb Rate Increment",
                1.0f,
                new ConfigDescription(
                    "Climb Rate increment.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.SpeedIncrement = Config.Bind("Autopilot",
                "Autopilot - Speed Increment",
                1.0f,
                new ConfigDescription(
                    "Speed increment.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.MachSpeedIncrement = Config.Bind("Autopilot",
                "Autopilot - Mach Speed Increment",
                1.0f,
                new ConfigDescription(
                    "Mach Speed increment.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.RollIncrement = Config.Bind("Autopilot",
                "Autopilot - Roll Increment",
                1.0f,
                new ConfigDescription(
                    "Roll increment.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            NOAutopilotControl.CourseIncrement = Config.Bind("Autopilot",
                "Autopilot - Course Increment",
                1.0f,
                new ConfigDescription(
                    "Course increment.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
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
            order = 100;
            LoadoutPreview.Enabled = Config.Bind("Loadout Preview",
                "Loadout Preview - Enabled",
                true,
                new ConfigDescription(
                    "Enable or disable the Loadout Preview feature.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            LoadoutPreview.OnlyShowOnBoot = Config.Bind("Loadout Preview",
                "Loadout Preview - Only Show On Boot",
                false,
                new ConfigDescription(
                    "If enabled, the loadout preview will only be shown on aircraft startup.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            LoadoutPreview.Duration = Config.Bind("Loadout Preview",
                "Loadout Preview - Duration",
                1f,
                new ConfigDescription(
                    "Duration (in seconds) for which the loadout preview is displayed.",
                    new AcceptableValueRange<float>(0.5f, 10f),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            LoadoutPreview.SendToHMD = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD",
                false,
                new ConfigDescription(
                    "If enabled, the loadout preview will be sent to the HMD display.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            LoadoutPreview.HMDShowBorders = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Show Borders",
                true,
                new ConfigDescription(
                    "If enabled, shows the borders for the loadout preview when sent to the HMD.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            LoadoutPreview.ManualPlacement = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Manual Placement",
                false,
                new ConfigDescription(
                    "If enabled, allows manual placement of the loadout preview on the MFD.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            LoadoutPreview.PositionX = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Position X",
                0,
                new ConfigDescription(
                    "X position offset for the loadout preview when manual placement is enabled.",
                    new AcceptableValueRange<int>(-1920 / 2, +1920 / 2),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            LoadoutPreview.PositionY = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Position Y",
                0,
                new ConfigDescription(
                    "Y position offset for the loadout preview when manual placement is enabled.",
                    new AcceptableValueRange<int>(-(int)1080 / 2, +(int)1080 / 2),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            LoadoutPreview.HMDFontSize = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Font Size",
                34,
                new ConfigDescription(
                    "Label font size when Loadout Preview is displayed on HMD.",
                    new AcceptableValueRange<int>(1, 101),
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            LoadoutPreview.HMDMainColor = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Main Color",
                Color.green,
                new ConfigDescription(
                    "Label and border color when Loadout Preview is displayed on HMD.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            LoadoutPreview.HMDBackgroundColor = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD - Background Color",
                Color.black,
                new ConfigDescription(
                    "Background color when Loadout Preview is displayed on HMD.",
                    null,
                    new ConfigurationManagerAttributes {
                        Order = order--
                    }));
            LoadoutPreview.DisabledFor = Config.Bind("Loadout Preview",
                "Loadout Preview - Disabled For",
                "",
                new ConfigDescription(
                    "Loadout Preview is disabled for aircrafts, specified by this sequence of aircraft plarform name patterns, separated by ';'. Aircraft plarform name pattern is a regual expression pattern, so it can be full or partial aircaft name (i.e. 'CI-22' or 'Cricket', or 'CI-22 Cricket'). If aircraft platform name does not match aricraft displayed name, enable logging, and look for platform name in 'LogOutput.log' '[LP] Initializing Loadout Preview for' messages. Respawn to apply changes.",
                    null,
                    new ConfigurationManagerAttributes {
                        CustomDrawer = ScrollableTextFieldDrawer.CreateDelegate(),
                        Order = order--
                    }));
            LoadoutPreview.SendToHMDFor = Config.Bind("Loadout Preview",
                "Loadout Preview - Send To HMD For",
                "F-16M King Viper;RAH-72 Knockout;F-99 Shrike;MC-260 Chimera;FS-41 Eclipse",
                new ConfigDescription(
                    "Loadout Preview is displayed on HMD (regardless of 'Send To HMD' setting) for aircrafts, specified by this sequence of aircraft platform name patterns, separated by ';'. Aircraft platform name pattern is a regual expression pattern, so it can be full or partial aircaft name (i.e. 'CI-22' or 'Cricket', or 'CI-22 Cricket'). If aircraft platform name does not match aricraft displayed name, enable logging, and look for platform name in 'LogOutput.log' '[LP] Initializing Loadout Preview for' messages. Respawn to apply changes.",
                    null,
                    new ConfigurationManagerAttributes {
                        CustomDrawer = ScrollableTextFieldDrawer.CreateDelegate(),
                        Order = order--
                    }));

            //PATCHES
            // GameBindings patch
            gameBindingsPatchEnabled = Config.Bind("Game Bindings Patch",
                "Game Bindings Patch - Enabled",
                true,
                "Turn off only in case of problems.");
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
            // Patch Weapon Manager Extensions
            if (WeaponManagerExtensions.Enabled.Value) {
                Log($"Weapon Manager Extensions are enabled, patching...");
                harmony.PatchAll(typeof(WeaponManagerExtensionsComponent.OnMainMenuStart));
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
            // Patch Persistent Map Options
            if (PersistentMapOptions.Enabled.Value) {
                Log("Persistent Map Options is enabled, patching...");
                harmony.PatchAll(typeof(PersistentMapOptionsComponent.OnMainMenuStart));
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
            if (LoadoutPreview.Enabled.Value) {
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
            if (NOAutopilotControl.Enabled.Value) {
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
            // CUSTOMIZE MISSILE STRING
            if (CustomizeMissileString.Enabled.Value) {
                Log($"Customize Missile String component is enabled, patching...");
                harmony.PatchAll(typeof(CustomizeMissileStringComponent.OnMainMenuStart));
            }
            // UI ADJUSTMENTS
            if (UIAdjustments.Enabled.Value) {
                Log($"UI Adjustments is enabled, patching...");
                harmony.PatchAll(typeof(UIAdjustmentsPlugin));
            }
            // SENSITIVITY FIX
            if (SensitivityFix.Enabled.Value) {
                Log($"Sensitivity Fix is enabled, patching...");
                harmony.PatchAll(typeof(SensitivityFixPlugin));
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
            // KEY VIEW CONTROL
            if (KeyViewControl.Enabled.Value) {
                Log($"Key View Control is enabled, patching...");
                harmony.PatchAll(typeof(KeyViewControlComponent.OnMainMenuStart));
            }
            // HEAD AXES
            if (HeadAxes.Enabled.Value) {
                Log($"Head Axes are enabled, patching...");
                harmony.PatchAll(typeof(HeadAxesComponent.OnMainMenuStart));
            }
            // THIRD PERSON HUD
            if (ThirdPersonHUD.Enabled.Value) {
                Log($"Third Person HUD is enabled, patching...");
                harmony.PatchAll(typeof(ThirdPersonHUDComponent.OnMainMenuStart));
            }
            //DYNAMIC LANDING CAM
            if (DynamicLandingCam.Enabled.Value) {
                Log($"Dynamic Landing Cam is enabled, patching...");
                harmony.PatchAll(typeof(DynamicLandingCamComponent.OnMainMenuStart));
            }
            // HMD CAM
            if (HMDCam.Enabled.Value) {
                Log($"HMD Cap patch is enabled, patching...");
                harmony.PatchAll(typeof(HMDCamComponent.OnMainMenuStart));
            }
            // EMWS
            if (EMWS.Enabled.Value) {
                Log($"EMWS patch is enabled, patching...");
                harmony.PatchAll(typeof(EMWSComponent.OnMainMenuStart));
            }
            // RADIAL MENU
            if (MultiLevelMenu.Enabled.Value) {
                harmony.PatchAll(typeof(MultiLevelMenuComponent.OnMainMenuStart));
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
