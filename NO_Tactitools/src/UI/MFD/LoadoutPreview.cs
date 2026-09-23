using HarmonyLib;
using UnityEngine;
using NO_Tactitools.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace NO_Tactitools.UI.MFD;

[HarmonyPatch(typeof(MainMenu), "Start")]
class LoadoutPreviewPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[LP] Loadout Preview plugin starting !");

            Plugin.harmony.PatchAll(typeof(LoadoutPreviewComponent.OnPlatformStart));
            Plugin.harmony.PatchAll(typeof(LoadoutPreviewComponent.OnPlatformUpdate));
            // TODO: Register a button if needed for toggling or interaction

            var bindings = new BindingHelper.Binding[] {
                new (typeof(LoadoutPreviewComponent.InternalState), "DisabledFor", Plugin.LoadoutPreview.DisabledFor),
                new (typeof(LoadoutPreviewComponent.InternalState), "SendToHMDFor", Plugin.LoadoutPreview.SendToHMDFor),
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;

            Plugin.Log("[LP] Loadout Preview plugin succesfully started !");
        }
    }

    // TODO: Add handler methods for button presses if any
}

public class LoadoutPreviewComponent {
    // LOGIC ENGINE, INTERNAL STATE, DISPLAY ENGINE
    static class LogicEngine {
        static public void Init() {
            //if loadout preview is not null, destroy it properly
            InternalState.loadoutPreview?.Destroy();
            InternalState.loadoutPreview = null;
            InternalState.weaponStations.Clear();

            string platformName = GameBindings.Player.Aircraft.GetPlatformName();
            Plugin.Log($"[LP] Initializing Loadout Preview for {platformName}");

            InternalState.neverShown = true;
            InternalState.needsUpdate = false;
            InternalState.displayDuration = Plugin.LoadoutPreview.Duration.Value;
            InternalState.onlyShowOnBoot = Plugin.LoadoutPreview.OnlyShowOnBoot.Value;
            InternalState.sendToHMD = Plugin.LoadoutPreview.SendToHMD.Value;
            InternalState.hmdShowBorders = Plugin.LoadoutPreview.HMDShowBorders.Value;
            InternalState.vanillaUIEnabled = Plugin.weaponDisplayVanillaUIEnabled.Value; // WE READ THIS SETTING HERE BECAUSE WE NEED IT, WE COULD CALL IT FROM WEAPON DISPLAY BUT THIS LETS US AVOID LOAD ORDER ISSUES
            InternalState.manualPlacement = Plugin.LoadoutPreview.ManualPlacement.Value;
            InternalState.horizontalOffset = Plugin.LoadoutPreview.PositionX.Value;
            InternalState.verticalOffset = Plugin.LoadoutPreview.PositionY.Value;
            InternalState.hmdFontSize = Plugin.LoadoutPreview.HMDFontSize.Value;
            InternalState.hmdMainColor = Plugin.LoadoutPreview.HMDMainColor.Value;
            InternalState.hmdBackgroundColor = Plugin.LoadoutPreview.HMDBackgroundColor.Value;
            
            InternalState.isAircraftRecognized = AircraftRecognition.IsAircraftRecognized(platformName);
            
            InternalState.isDisabled = false;
            if (InternalState.disabledForIsStale) {
                InternalState.disabledForRegexes = InternalState.DisabledFor.Split(";").Where(s => s.Length > 0).Select(s => new Regex(s)).ToArray();
                InternalState.disabledForIsStale = false;
            }
            foreach (var regex in InternalState.disabledForRegexes) {
                Match m = regex.Match(platformName);
                if (m.Success) {
                    InternalState.isDisabled = true;
                    Plugin.Log($"[LP] Disabled for platform {platformName}");
                    break;
                }
            }

            if (InternalState.sendToHMDForHasUpdated) {
                InternalState.sendToHMDForRegexes = InternalState.SendToHMDFor.Split(";").Where(s => s.Length > 0).Select(s => new Regex(s)).ToArray();
                InternalState.sendToHMDForHasUpdated = false;
            }
            foreach (var regex in InternalState.sendToHMDForRegexes) {
                Match m = regex.Match(platformName);
                if (m.Success) {
                    InternalState.sendToHMD = true;
                    Plugin.Log($"[LP] Sending to HMD for platform {platformName}");
                    break;
                }
            }

            InternalState.hasStations = GameBindings.Player.Aircraft.Weapons.GetStationCount() > 1;
            if (InternalState.hasStations) {
                InternalState.currentWeaponStationIndex = GameBindings.Player.Aircraft.Weapons.GetActiveStationIndex();
                for (int i = 0; i < GameBindings.Player.Aircraft.Weapons.GetStationCount(); i++) {
                InternalState.WeaponStationInfo stationInfo = new() {
                    stationName = GameBindings.Player.Aircraft.Weapons.GetStationNameByIndex(i),
                    stationIndex = i,
                    ammo = GameBindings.Player.Aircraft.Weapons.GetStationAmmoByIndex(i),
                    maxAmmo = GameBindings.Player.Aircraft.Weapons.GetStationMaxAmmoByIndex(i)
                };
                InternalState.weaponStations.Add(stationInfo);
            }
            }
        }

        static public void Update() {
            if (GameBindings.GameState.IsGamePaused() || GameBindings.Player.Aircraft.GetAircraft() == null || InternalState.loadoutPreview == null)
                return;
            if (InternalState.onlyShowOnBoot 
                && InternalState.neverShown 
                && BootScreenComponent.InternalState.hasBooted) {
                InternalState.lastUpdateTime = Time.realtimeSinceStartup;
                InternalState.currentWeaponStationIndex = GameBindings.Player.Aircraft.Weapons.GetActiveStationIndex();
                InternalState.neverShown = false;
            }
            else if (
                InternalState.currentWeaponStationIndex != GameBindings.Player.Aircraft.Weapons.GetActiveStationIndex()
                && BootScreenComponent.InternalState.hasBooted 
                && !InternalState.onlyShowOnBoot) {
                InternalState.lastUpdateTime = Time.realtimeSinceStartup;
                InternalState.currentWeaponStationIndex = GameBindings.Player.Aircraft.Weapons.GetActiveStationIndex();
            }
            InternalState.needsUpdate = ((Time.realtimeSinceStartup - InternalState.lastUpdateTime) < InternalState.displayDuration);
            if (InternalState.needsUpdate) {
                for (int i = 0; i < GameBindings.Player.Aircraft.Weapons.GetStationCount(); i++) {
                    InternalState.weaponStations[i].stationName = GameBindings.Player.Aircraft.Weapons.GetStationNameByIndex(i);
                    InternalState.weaponStations[i].ammo = GameBindings.Player.Aircraft.Weapons.GetStationAmmoByIndex(i);
                    InternalState.weaponStations[i].maxAmmo = GameBindings.Player.Aircraft.Weapons.GetStationMaxAmmoByIndex(i);
                }
            }
            InternalState.configNeedsUpdate = (
                InternalState.displayDuration != Plugin.LoadoutPreview.Duration.Value ||
                InternalState.horizontalOffset != Plugin.LoadoutPreview.PositionX.Value ||
                InternalState.verticalOffset != Plugin.LoadoutPreview.PositionY.Value ||
                InternalState.manualPlacement != Plugin.LoadoutPreview.ManualPlacement.Value ||
                InternalState.hmdFontSize != Plugin.LoadoutPreview.HMDFontSize.Value ||
                InternalState.hmdMainColor != Plugin.LoadoutPreview.HMDMainColor.Value ||
                InternalState.hmdBackgroundColor != Plugin.LoadoutPreview.HMDBackgroundColor.Value
            );
            if (InternalState.configNeedsUpdate) {
                InternalState.displayDuration = Plugin.LoadoutPreview.Duration.Value;
                InternalState.horizontalOffset = Plugin.LoadoutPreview.PositionX.Value;
                InternalState.verticalOffset = Plugin.LoadoutPreview.PositionY.Value;
                InternalState.manualPlacement = Plugin.LoadoutPreview.ManualPlacement.Value;
                InternalState.hmdFontSize = Plugin.LoadoutPreview.HMDFontSize.Value;
                InternalState.hmdMainColor = Plugin.LoadoutPreview.HMDMainColor.Value;
                InternalState.hmdBackgroundColor = Plugin.LoadoutPreview.HMDBackgroundColor.Value;
            }
        }
    }


    public static class InternalState {
        public class WeaponStationInfo {
            public string stationName;
            public int stationIndex;
            public int ammo;
            public int maxAmmo;
        }
        public static int currentWeaponStationIndex = -1;
        public static float lastUpdateTime = 0;
        public static bool needsUpdate = false;
        public static bool configNeedsUpdate = false;
        public static bool onlyShowOnBoot;
        public static bool neverShown = true;
        public static List<WeaponStationInfo> weaponStations = [];
        public static LoadoutPreview loadoutPreview = null;
        public static bool sendToHMD = false;
        public static bool hmdShowBorders = true;
        public static bool vanillaUIEnabled = true;
        public static bool manualPlacement = false;
        public static int horizontalOffset = 0;
        public static int verticalOffset = 0;
        public static int hmdFontSize = 34;
        public static Color hmdMainColor = Color.green;
        public static Color hmdBackgroundColor = Color.black;
        public static bool hasStations = true;

        public static bool isAircraftRecognized = true;

        static public Regex[] disabledForRegexes;
        static public string DisabledFor {
            set {
                field = value;
                InternalState.disabledForIsStale = true;
            }
            get;
        } = new ("");
        static public bool disabledForIsStale = true;
        static public bool isDisabled = false;

        static public Regex[] sendToHMDForRegexes;
        static public string SendToHMDFor {
            set {
                field = value;
                InternalState.sendToHMDForHasUpdated = true;
            }
            get;
        } = new ("");
        static public bool sendToHMDForHasUpdated = true;

        public static float displayDuration = 1f;
        public static Color mainColor = Color.green;
        public static Color textColor = Color.green;
        public static Color backgroundColor = Color.black;
    }

    static class AircraftRecognition {
        static public bool IsAircraftRecognized(string platformName) {
            return platformName switch {
                "CI-22 Cricket" or
                "SAH-46 Chicane" or
                "T/A-30 Compass" or
                "FS-3 Ternion" or
                "FS-12 Revoker" or
                "FS-20 Vortex" or
                "KR-67 Ifrit" or
                "VL-49 Tarantula" or
                "EW-1 Medusa" or
                "SFB-81" or
                "UH-80 Ibis" or
                "A-19 Brawler" or
                "Alkyon AB-4" or
                "AB-4 Alkyon" or
                "FastBomber1" or
                "VT-7 Vagrant" or
                "FQ-106 Kestrel" or
                "MiG-15" or
                "HMD" => true,
                _ => false
            };
        }
    }

    static class DisplayEngine {
        static public void Init() {
            if (InternalState.hasStations) {
                Plugin.Log("[LP] Initializing Loadout Preview for Tac Screen");
                if (InternalState.sendToHMD) {
                    InternalState.mainColor = InternalState.hmdMainColor;
                    InternalState.textColor = InternalState.hmdMainColor;
                    InternalState.backgroundColor = InternalState.hmdBackgroundColor;
                }
                else if (!InternalState.isAircraftRecognized) {
                    Plugin.Log("[LP] Aircraft is not recognized, only HMD Loadout Preview is available.");
                    return;
                }
                else if (InternalState.isDisabled) {
                    Plugin.Log("[LP] Loadout Preview is disabled");
                    return;
                }
                InternalState.loadoutPreview = new LoadoutPreview();
                Plugin.Log("[LP] Loadout Preview initialized.");
            }
        }

        static public void Update() {
            if (GameBindings.GameState.IsGamePaused() ||
                GameBindings.Player.Aircraft.GetAircraft() == null ||
                InternalState.loadoutPreview == null)
                return;
            if (InternalState.configNeedsUpdate && InternalState.sendToHMD) {
                InternalState.loadoutPreview.UpdatePlacement();
                InternalState.loadoutPreview.UpdateColors();
                InternalState.loadoutPreview.borderRect.SetFillColor(InternalState.backgroundColor);
                InternalState.loadoutPreview.borderRect.SetBorderColor(InternalState.mainColor);
                //Label colors will be updated in UpdateLabels()
            }
            if (InternalState.needsUpdate) {
                InternalState.loadoutPreview.SetActive(true);
                InternalState.loadoutPreview.UpdateLabels();
                InternalState.loadoutPreview.containerTransform.SetAsLastSibling();
            }
            else {
                InternalState.loadoutPreview.SetActive(false);
            }
        }
    }

    public class LoadoutPreview {
        public GameObject containerObject;
        public Transform containerTransform;
        public List<UIBindings.Draw.UILabel> stationLabels = [];
        public UIBindings.Draw.UIAdvancedRectangle borderRect;
        public float maxLabelWidth;
        public float verticalOffset = 0;
        public float horizontalOffset = 0;
        public float padding = 0;
        public float border = 0;
        public int fontSize = 34;

        public LoadoutPreview() {
            string platformName;
            Transform parentTransform;
            if (!InternalState.sendToHMD) {
                parentTransform = UIBindings.Game.GetTacScreenTransform();
                platformName = GameBindings.Player.Aircraft.GetPlatformName();
            }
            else {
                parentTransform = UIBindings.Game.GetCombatHUDTransform();
                platformName = "HMD";
            }
            
            // Create container GameObject to hold all LoadoutPreview elements
            containerObject = new GameObject("i_lp_LoadoutPreviewContainer");
            containerObject.AddComponent<RectTransform>();
            containerTransform = containerObject.transform;
            containerTransform.SetParent(parentTransform, false);

            switch (platformName) {
                case "CI-22 Cricket":
                    horizontalOffset = -105;
                    verticalOffset = 0;
                    fontSize = 44;
                    break;
                case "SAH-46 Chicane":
                    horizontalOffset = -130;
                    verticalOffset = 65;
                    break;
                case "T/A-30 Compass":
                    horizontalOffset = 0;
                    verticalOffset = 80;
                    break;
                case "FS-12 Revoker":
                    horizontalOffset = 0;
                    verticalOffset = 75;
                    break;
                case "FS-20 Vortex":
                    horizontalOffset = 0;
                    verticalOffset = 75;
                    break;
                case "KR-67 Ifrit":
                    horizontalOffset = -130;
                    verticalOffset = 65;
                    break;
                case "VL-49 Tarantula":
                    horizontalOffset = -255;
                    verticalOffset = 60;
                    fontSize = 28;
                    break;
                case "EW-1 Medusa":
                    horizontalOffset = -225;
                    verticalOffset = 65;
                    break;
                case "SFB-81":
                    horizontalOffset = -180;
                    verticalOffset = 60;
                    break;
                case "UH-80 Ibis":
                    horizontalOffset = -245;
                    verticalOffset = 65;
                    break;
                case "A-19 Brawler":
                    verticalOffset = 70;
                    break;
                case "Alkyon AB-4":
                case "AB-4 Alkyon":
                case "FastBomber1":
                    horizontalOffset = -180;
                    verticalOffset = 60;
                    break;
                case "VT-7 Vagrant":
                    horizontalOffset = 0;
                    verticalOffset = 75;
                    break;
                //modded planes
                case "FS-3 Ternion":
                    horizontalOffset = -215;
                    verticalOffset = 80;
                    fontSize = 30;
                    break;
                case "FQ-106 Kestrel":
                    verticalOffset = 75;
                    break;
                case "MiG-15":
                    horizontalOffset = -250;
                    verticalOffset = 120;
                    fontSize = 20;
                    break;
                case "HMD":
                    horizontalOffset = 0;
                    verticalOffset = 0;
                    fontSize = InternalState.hmdFontSize;
                    break;
                default:
                    break;
            }

            border = InternalState.sendToHMD && !InternalState.hmdShowBorders ? 0 : 2;

            UpdateColors();

            // Create background rectangle
            borderRect = new(
                "i_lp_LoadoutPreviewBorder",
                new Vector2(-1, -1),
                new Vector2(1, 1),
                InternalState.mainColor,
                border,
                containerTransform,
                InternalState.backgroundColor
            );

            // Create labels
            maxLabelWidth = 0;
            for (int i = 0; i < InternalState.weaponStations.Count; i++) {
                UIBindings.Draw.UILabel stationLabel = new(
                    "i_lp_Slot " + i,
                    new Vector2(0, 0),
                    containerTransform,
                    fontStyle: FontStyle.Bold, // Default to bold; will be updated in UpdateLabels()
                    color: InternalState.textColor,
                    fontSize: fontSize + 6, // Default to 40; will be updated in UpdateLabels()
                    backgroundOpacity: 0f
                );
                stationLabels.Add(stationLabel);
            }

            UpdatePlacement();
            UpdateLabelPositions();
        }

        public void UpdatePlacement() {
            if (InternalState.sendToHMD)
                fontSize = InternalState.hmdFontSize;

            maxLabelWidth = 0;

            var weaponStations = InternalState.weaponStations;
            for (int i = 0; i < weaponStations.Count; i++) {
                var stationLabel = stationLabels[i];
                UpdateLabelText(i);
                stationLabel.SetFontSize(fontSize+6); // WE FORCE IT, otherwise the max size might not get taken into account
                Vector2 textSize = stationLabel.GetTextSize();
                if (textSize.x > maxLabelWidth) {
                    maxLabelWidth = textSize.x;
                }
            }

            padding = (fontSize + 6) / 4;
            float rectHalfWidth = maxLabelWidth / 2f;
            float rectHalfHeight = weaponStations.Count / 2f * (fontSize + 6);

            if (InternalState.sendToHMD) {
                if (InternalState.manualPlacement) {
                    horizontalOffset = InternalState.horizontalOffset;
                    verticalOffset = InternalState.verticalOffset;
                }
                else {
                    if (InternalState.vanillaUIEnabled) {
                        // The new way to punctually directly reflect elements
                        GameObject topRightPanel = new TraverseCache<CombatHUD, GameObject>("topRightPanel").GetValue(UIBindings.Game.GetCombatHUDComponent());
                        GameObject powerPanel = topRightPanel.transform.Find("PowerPanel").gameObject;
                        horizontalOffset = 
                            topRightPanel.transform.localPosition.x 
                            - rectHalfWidth - border * 2 - padding;
                        verticalOffset = 
                            topRightPanel.transform.localPosition.y
                            - rectHalfHeight - border - padding
                            - 170;
                        if (GameBindings.Player.Aircraft.Countermeasures.HasJammer() || powerPanel.activeSelf) {
                            Plugin.Log("[LP] Adjusting Loadout Preview position to avoid jammer display overlap.");
                            verticalOffset -= 40;
                        }
                    }
                    else {
                        horizontalOffset = (1920 / 2) - rectHalfWidth - border - padding;
                        verticalOffset = (1080 / 2) - rectHalfHeight - border - padding;
                    }
                }
            }

            // Set background size
            borderRect.SetCorners(
                a: new Vector2(-rectHalfWidth - padding, -rectHalfHeight - padding),
                b: new Vector2(rectHalfWidth + padding, rectHalfHeight + padding)
            );

            if (containerTransform != null) {
                containerTransform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0);
            }
        }

        public void SetActive(bool active) {
            containerObject?.SetActive(active);
        }

        public void Update() {
            UpdateLabels();
            containerTransform.SetAsLastSibling();
        }

        public void UpdateColors() {
            if (InternalState.sendToHMD) {
                InternalState.mainColor = InternalState.hmdMainColor;
                InternalState.textColor = InternalState.hmdMainColor;
                InternalState.backgroundColor = InternalState.hmdBackgroundColor;
            }
        }

        public void UpdateLabels() {
            for (int i = 0; i < InternalState.weaponStations.Count; i++) {
                UpdateLabelText(i);
                var ws = InternalState.weaponStations[i];
                // keep color/size adjustments minimal here; DisplayEngine handles color each frame
                var label = stationLabels[i];
                var isActiveStation = GameBindings.Player.Aircraft.Weapons.GetActiveStationIndex() == ws.stationIndex;
                label.SetFontSize(isActiveStation ? (fontSize + 6) : fontSize);
                label.SetFontStyle(isActiveStation ? FontStyle.Bold : FontStyle.Normal);
                label.SetColor((ws.ammo == 0) ? Color.red : InternalState.textColor);
            }
            InternalState.loadoutPreview.UpdateLabelPositions();
        }

        public void UpdateLabelPositions() {
            for (int i = 0; i < stationLabels.Count; i++) {
                Vector2 textSize = stationLabels[i].GetTextSize();
                stationLabels[i].SetPosition(
                    new Vector2(
                        x: -(maxLabelWidth - textSize.x) / 2f - padding/2,
                        y: (stationLabels.Count - 1) * padding * 2f - i * (fontSize + 6)));
            }
        }

        public void UpdateLabelText(int index) {
            var weaponStation = InternalState.weaponStations[index];
            var stationLabel = stationLabels[index];
            stationLabel.SetText($"[{index.ToString()}] {weaponStation.stationName}: {weaponStation.ammo}/{weaponStation.maxAmmo}");
        }

        public void Destroy() {
            if (containerObject != null) {
                Object.Destroy(containerObject);
                containerObject = null;
            }
            stationLabels.Clear();
            borderRect = null;
        }
    }

    // INIT AND REFRESH LOOP
    [HarmonyPatch(typeof(TacScreen), "Initialize")]
    public static class OnPlatformStart {
        static void Postfix() {
            LogicEngine.Init();
            DisplayEngine.Init();
        }
    }

    [HarmonyPatch(typeof(TacScreen), "Update")]
    public static class OnPlatformUpdate {
        static void Postfix() {
            LogicEngine.Update();
            DisplayEngine.Update();
        }
    }
}

