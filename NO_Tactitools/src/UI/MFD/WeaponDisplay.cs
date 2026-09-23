using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.MFD;

[HarmonyPatch(typeof(MainMenu), "Start")]
class WeaponDisplayPlugin {
    private static bool initialized = false;

    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[WD] Weapon Display plugin starting !");

            Plugin.harmony.PatchAll(typeof(WeaponDisplayComponent.OnPlatformStart));
            Plugin.harmony.PatchAll(typeof(WeaponDisplayComponent.OnPlatformUpdate));

            // Register the new button for toggling the weapon display
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavToggle,
                Plugin.PressDelay.Value,
                onLongPress: HandleDisplayToggle
            );

            var bindings = new BindingHelper.Binding[] {
                new (WeaponDisplayComponent.InternalState.DisabledFor, "Entries", Plugin.weaponDisplayDisabledFor),
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log("[WD] Weapon Display plugin successfully started !");
        }
    }

    private static void HandleDisplayToggle() {
        if (WeaponDisplayComponent.InternalState.weaponDisplay != null) {
            if (WeaponDisplayComponent.InternalState.weaponDisplay.removeOriginalMFDContent) {
                WeaponDisplayComponent.InternalState.weaponDisplay.ToggleChildrenActiveState();

                UIBindings.Sound.PlaySound("beep_scroll");
                Plugin.Log("[WD] Weapon Display toggled.");
            }
        }
        else {
            Plugin.Log("[WD] Weapon Display not initialized, cannot toggle.");
        }
    }
}

public class WeaponDisplayComponent {
    // LOGIC ENGINE, INTERNAL STATE, DISPLAY ENGINE
    static class LogicEngine {
        static public void Init() {
            string name = GameBindings.Player.Aircraft.GetPlatformName();
            Plugin.Log($"[WD] Initializing Weapon Display for platform {name}");

            InternalState.isAircraftRecognized = AircraftRecognition.IsAircraftRecognized(name);
            if (!InternalState.isAircraftRecognized) {
                Plugin.Log($"[WD] Platform {name} is not recognizedd");
                return;
            }

            InternalState.isDisabled = InternalState.DisabledFor.Matches(name);
            if (InternalState.isDisabled) {
                Plugin.Log($"[WD] Disabled for platform {name}");
                return;
            }

            InternalState.hasFlare = GameBindings.Player.Aircraft.Countermeasures.HasIRFlare();
            InternalState.hasJammer = GameBindings.Player.Aircraft.Countermeasures.HasJammer();
            InternalState.hasChaff = GameBindings.Player.Aircraft.Countermeasures.HasChaff();

            InternalState.flareAmmo01 = -1;
            InternalState.jammerAmmo01 = -1;
            InternalState.chaffAmmo01 = -1;

            InternalState.countermeasureIndex = -1;

            InternalState.hasStations = GameBindings.Player.Aircraft.Weapons.GetStationCount() > 0;
            InternalState.activeStation = null;

            InternalState.vanillaUIEnabled = Plugin.weaponDisplayVanillaUIEnabled.Value;

            Plugin.Log($"[WD] Weapon Display initialized for platform {name}");
        }

        static public void Update() {
            if (GameBindings.Player.Aircraft.GetAircraft() == null || !InternalState.isAircraftRecognized) return;

            //Countermeasures

            var countermeasureIndex = GameBindings.Player.Aircraft.Countermeasures.GetCurrentIndex();
            InternalState.countermeasureHasChanged = countermeasureIndex != InternalState.countermeasureIndex;
            InternalState.countermeasureIndex = countermeasureIndex;

            //Flare
            if (InternalState.hasFlare) {
                InternalState.isFlareSelected = GameBindings.Player.Aircraft.Countermeasures.IsFlareSelected();
                var flareAmmo01 = Mathf.Clamp01(
                    (float)GameBindings.Player.Aircraft.Countermeasures.GetIRFlareAmmo() /
                    GameBindings.Player.Aircraft.Countermeasures.GetIRFlareMaxAmmo());
                InternalState.flareHasChanged = flareAmmo01 != InternalState.flareAmmo01;
                InternalState.flareAmmo01 = flareAmmo01;
            }

            //Jammer
            if (InternalState.hasJammer) {
                InternalState.isJammerSelected = GameBindings.Player.Aircraft.Countermeasures.IsJammerSelected();
                var jammerAmmo01 = Mathf.Clamp01(
                    (float)GameBindings.Player.Aircraft.Countermeasures.GetJammerAmmo() / 100f);
                InternalState.jammerHasChanged = jammerAmmo01 != InternalState.jammerAmmo01;
                InternalState.jammerAmmo01 = jammerAmmo01;
            }

            //Chaff
            if (InternalState.hasChaff) {
                InternalState.isChaffSelected = GameBindings.Player.Aircraft.Countermeasures.IsChaffSelected();
                var chaffAmmo01 = Mathf.Clamp01(
                    (float)GameBindings.Player.Aircraft.Countermeasures.GetChaffAmmo() /
                    GameBindings.Player.Aircraft.Countermeasures.GetChaffMaxAmmo());
                InternalState.chaffHasChanged = chaffAmmo01 != InternalState.chaffAmmo01;
                InternalState.chaffAmmo01 = chaffAmmo01;
            }

            //Weapons
            if (InternalState.hasStations) {
                // WRITE WEAPON STATE ONLY IF THE PLAYER HAS WEAPON STATIONS
                var activeStation = GameBindings.Player.Aircraft.Weapons.GetActiveStation();
                InternalState.stationHasChanged = InternalState.activeStation != activeStation;
                InternalState.activeStation = activeStation;

                var activeStationAmmo = GameBindings.Player.Aircraft.Weapons.GetActiveStationAmmo();
                InternalState.ammoHasChanged = InternalState.stationHasChanged || InternalState.activeStationAmmo != activeStationAmmo;
                InternalState.activeStationAmmo = activeStationAmmo;

                InternalState.isOutOfAmmo = activeStationAmmo == 0;

                InternalState.reduceWeaponFontSize =
                    GameBindings.Player.Aircraft.Weapons.GetActiveStationAmmoString().Contains("/");

                InternalState.isReloading = GameBindings.Player.Aircraft.Weapons.GetActiveStationReloadProgress() > 0f;
            }
        }
    }

    public static class InternalState {
        static public WeaponDisplay weaponDisplay = null;
        static public bool hasFlare;
        static public bool hasJammer;
        static public bool hasChaff;

        static public bool isFlareSelected;
        static public bool isJammerSelected;
        static public bool isChaffSelected;

        static public float flareAmmo01 = -1;
        static public float jammerAmmo01 = -1;
        static public float chaffAmmo01 = -1;

        static public bool chaffHasChanged = true;
        static public bool jammerHasChanged = true;
        static public bool flareHasChanged = true;

        static public int countermeasureIndex = -1;
        static public bool countermeasureHasChanged = true;

        static public bool hasStations;
        static public WeaponStation activeStation = null;
        static public bool stationHasChanged = true;
        static public int activeStationAmmo = -1;
        static public bool ammoHasChanged;
        static public bool isOutOfAmmo;
        static public bool isReloading = false;

        static public bool reduceWeaponFontSize = false;

        static public bool vanillaUIEnabled = true; // true by default since we need to check this value elsewhere

        static public bool isAircraftRecognized = true;

        static public RegexEntries DisabledFor = new ();
        static public bool isDisabled = false;

        static public Color mainColor = Color.green;
        static public Color textColor = Color.green;
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
                "MiG-15" => true,
                _ => false
            };
        }
    }

    static class DisplayEngine {
        static public void Init() {
            if (!InternalState.isDisabled && InternalState.isAircraftRecognized) {
                // In reality, this checks if the player's plane has spawned
                try {
                    InternalState.weaponDisplay = null;
                    InternalState.weaponDisplay = new WeaponDisplay();
                }
                catch (NotImplementedException e) {
                    Plugin.Log(string.Format("[WD] Cockpit Weapon Display is not implemented for platform {0}", e.Message));
                    InternalState.weaponDisplay = null;
                }
                catch (NullReferenceException e) {
                    Plugin.Log(string.Format("[WD] Got exception: {0}", e));
                    InternalState.weaponDisplay = null;
                }
                if (!InternalState.vanillaUIEnabled) UIBindings.Game.HideWeaponPanel();
                else UIBindings.Game.ShowWeaponPanel();
            }

            Plugin.Log("[WD] Display Engine initialized for platform " +
                       GameBindings.Player.Aircraft.GetPlatformName());
        }

        static public void Update() {
            if (GameBindings.GameState.IsGamePaused() ||
                GameBindings.Player.Aircraft.GetAircraft() == null ||
                UIBindings.Game.GetCombatHUDTransform() == null ||
                InternalState.weaponDisplay == null)
                return; // do not refresh anything if the game is paused or the player aircraft is not available

            //Countermeasures
            // REFRESH FLARE
            if (InternalState.hasFlare) {
                if (InternalState.countermeasureHasChanged) {
                    var fontStyle = InternalState.isFlareSelected ? FontStyle.Bold : FontStyle.Normal;
                    InternalState.weaponDisplay.flareLabel.SetFontStyle(fontStyle);

                    var fontSize = InternalState.weaponDisplay.originalFlareFontSize + (InternalState.isFlareSelected ? 10 : 0);
                    InternalState.weaponDisplay.flareLabel.SetFontSize(fontSize);
                }

                if (InternalState.flareHasChanged) {
                    var text = "IR:" + GameBindings.Player.Aircraft.Countermeasures.GetIRFlareAmmo().ToString();
                    InternalState.weaponDisplay.flareLabel.SetText(text);

                    var color = Color.Lerp(Color.red, InternalState.textColor, InternalState.flareAmmo01);
                    InternalState.weaponDisplay.flareLabel.SetColor(color);
                }
            }

            // REFRESH JAMMER
            if (InternalState.hasJammer) {
                if (InternalState.countermeasureHasChanged) {
                    var fontStyle = InternalState.isJammerSelected ? FontStyle.Bold : FontStyle.Normal;
                    InternalState.weaponDisplay.jammerLabel.SetFontStyle(fontStyle);

                    var fontSize = InternalState.weaponDisplay.originalJammerFontSize + (InternalState.isJammerSelected ? 10 : 0);
                    InternalState.weaponDisplay.jammerLabel.SetFontSize(fontSize);
                }

                if (InternalState.jammerHasChanged) {
                    var text = "EW:" +GameBindings.Player.Aircraft.Countermeasures.GetJammerAmmo().ToString() + "%";
                    InternalState.weaponDisplay.jammerLabel.SetText(text);

                    var color = Color.Lerp(Color.red, InternalState.textColor, InternalState.jammerAmmo01);
                    InternalState.weaponDisplay.jammerLabel.SetColor(color);
                }
            }

            // REFRESH CHAFF
            if (InternalState.hasChaff) {
                if (InternalState.countermeasureHasChanged) {
                    var fontStyle = InternalState.isChaffSelected ? FontStyle.Bold : FontStyle.Normal;
                    InternalState.weaponDisplay.chaffLabel.SetFontStyle(fontStyle);

                    var fontSize = InternalState.weaponDisplay.originalChaffFontSize + (InternalState.isChaffSelected ? 10 : 0);
                    InternalState.weaponDisplay.chaffLabel.SetFontSize(fontSize);
                }

                if (InternalState.chaffHasChanged) {
                    var text = "CF:" + GameBindings.Player.Aircraft.Countermeasures.GetChaffAmmo().ToString();
                    InternalState.weaponDisplay.chaffLabel.SetText(text);

                    var color = Color.Lerp(Color.red, InternalState.textColor, InternalState.chaffAmmo01);
                    InternalState.weaponDisplay.chaffLabel.SetColor(color);
                }
            }

            // REFRESH WEAPON
            if (InternalState.hasStations) {
                Image cloneImg = null;
                // do not refresh weapon info if the player has no weapon stations
                if (InternalState.stationHasChanged) {
                    var stationName = GameBindings.Player.Aircraft.Weapons.GetActiveStationName();
                    InternalState.weaponDisplay.weaponNameLabel.SetText(stationName);

                    cloneImg = InternalState.weaponDisplay.weaponImageClone.GetComponent<Image>();
                    Image srcImg = GameBindings.Player.Aircraft.Weapons.GetActiveStationImage();
                    cloneImg.sprite = srcImg.sprite;
                }

                if (InternalState.stationHasChanged || InternalState.ammoHasChanged || InternalState.isReloading) {
                    string text = null;
                    if (InternalState.stationHasChanged || InternalState.ammoHasChanged)
                        text = GameBindings.Player.Aircraft.Weapons.GetActiveStationAmmoString().Replace(" ", "");
                    else if (InternalState.isReloading)
                        text = ((int)(100f - GameBindings.Player.Aircraft.Weapons.GetActiveStationReloadProgress() * 100f)).ToString() + "%";
                    InternalState.weaponDisplay.weaponAmmoLabel.SetText(text != null ? (string)text : "");
                }

                if (InternalState.stationHasChanged || InternalState.ammoHasChanged) {
                    var fontSize = InternalState.weaponDisplay.originalWeaponAmmoFontSize + (InternalState.reduceWeaponFontSize ? -15 : 0);
                    InternalState.weaponDisplay.weaponAmmoLabel.SetFontSize(fontSize);

                    var fontColor = InternalState.isOutOfAmmo ? Color.red : InternalState.textColor;
                    InternalState.weaponDisplay.weaponNameLabel.SetColor(fontColor);
                    InternalState.weaponDisplay.weaponAmmoLabel.SetColor(fontColor);

                    if (cloneImg == null)
                        cloneImg = InternalState.weaponDisplay.weaponImageClone.GetComponent<Image>();
                    cloneImg.color = InternalState.isOutOfAmmo ? Color.red : InternalState.mainColor;
                }

                // TODO : ENCAPSULATE IMAGES IN MY OWN CODE
            }
        }
    }

    public class WeaponDisplay {
        public Transform weaponDisplay_transform;
        public UIBindings.Draw.UILabel flareLabel;
        public UIBindings.Draw.UILabel jammerLabel;
        public UIBindings.Draw.UILabel chaffLabel;
        public UIBindings.Draw.UILine MFD_systemsLine;
        public UIBindings.Draw.UILabel weaponNameLabel;
        public UIBindings.Draw.UILabel weaponAmmoLabel;

        public GameObject weaponImageClone;

        // Store original font sizes
        public int originalFlareFontSize;
        public int originalJammerFontSize;
        public int originalChaffFontSize;

        public int originalWeaponAmmoFontSize;

        //Store the main color for the MFD, can be set by the MFDColorPlugin
        public bool removeOriginalMFDContent = true; // by default, we remove the original MFD content


        public WeaponDisplay() {
            static Transform Get(string path) {
                return UIBindings.Game.GetTacScreenTransform()?.Find(path)?.transform;
            }

            string platformName = GameBindings.Player.Aircraft.GetPlatformName();
            Plugin.Log($"Platform name: {platformName.ToString()}");
            Transform destination = platformName switch {
                "EW-1 Medusa" => Get("engPanel1"),
                "CI-22 Cricket" => Get("EngPanel"),
                "SAH-46 Chicane" => Get("BasicFlightInstrument"),
                "VL-49 Tarantula" => Get("RightScreenBorder/WeaponPanel"),
                "SFB-81" => Get("StatusGauges"),
                "FastBomber1" or "AB-4 Alkyon" or "Alkyon AB-4" => Get("weaponPanel/frontProfile"),
                "MiG-15" => Get("StatusGauges/FrontView"),
                _ => Get("SystemStatus") // all the others
            };
            if (destination == null)
                throw new NullReferenceException (string.Format("Cannot get transform for {0}", platformName));
            weaponDisplay_transform = destination;
            // Default settings for the weapon display
            bool rotateWeaponImage = false;
            float imageScaleFactor = 0.6f;
            // Layout settings for each supported platform
            Vector2 flarePos, jammerPos, chaffPos = new (0, 0), lineStart, lineEnd, weaponNamePos, weaponAmmoPos, weaponImagePos;
            int flareFont, jammerFont, chaffFont = 0, weaponNameFont, weaponAmmoFont;
            switch (platformName) {
                case "CI-22 Cricket":
                    if (InternalState.hasChaff) {
                        flarePos = new Vector2(0, -20);
                        chaffPos = new Vector2(0, -50);
                    }
                    else {
                        flarePos = new Vector2(0, -40);
                    }
                    jammerPos = new Vector2(0, -80);
                    lineStart = new Vector2(-60, 0);
                    lineEnd = new Vector2(60, 0);
                    weaponNamePos = new Vector2(0, 60);
                    weaponAmmoPos = new Vector2(0, 30);
                    weaponImagePos = new Vector2(0, 80);
                    flareFont = 28;
                    jammerFont = 28;
                    chaffFont = 28;
                    weaponNameFont = 18;
                    weaponAmmoFont = 40;
                    break;
                case "SAH-46 Chicane":
                    flarePos = new Vector2(-45, -105);
                    jammerPos = new Vector2(30, -105);
                    lineStart = new Vector2(-80, -10);
                    lineEnd = new Vector2(80, -10);
                    weaponNamePos = new Vector2(0, -45);
                    weaponAmmoPos = new Vector2(0, -70);
                    weaponImagePos = new Vector2(0, -25);
                    flareFont = 16;
                    jammerFont = 14;
                    weaponNameFont = 20;
                    weaponAmmoFont = 35;
                    imageScaleFactor = 0.4f; // Scale the image for SAH-46 Chicane
                    removeOriginalMFDContent = false; // Do not remove original MFD content for SAH-46 Chicane
                    rotateWeaponImage = false;
                    break;
                case "T/A-30 Compass":
                    flarePos = new Vector2(0, -30);
                    jammerPos = new Vector2(0, -70);
                    lineStart = new Vector2(-60, 0);
                    lineEnd = new Vector2(60, 0);
                    weaponNamePos = new Vector2(0, 60);
                    weaponAmmoPos = new Vector2(0, 30);
                    weaponImagePos = new Vector2(0, 80);
                    flareFont = 24;
                    jammerFont = 20;
                    weaponNameFont = 18;
                    weaponAmmoFont = 40;
                    break;
                case "FS-12 Revoker":
                    flarePos = new Vector2(0, -40);
                    jammerPos = new Vector2(0, -80);
                    lineStart = new Vector2(-100, -10);
                    lineEnd = new Vector2(100, -10);
                    weaponNamePos = new Vector2(0, 50);
                    weaponAmmoPos = new Vector2(0, 20);
                    weaponImagePos = new Vector2(0, 80);
                    flareFont = 28;
                    jammerFont = 28;
                    weaponNameFont = 25;
                    weaponAmmoFont = 35;
                    imageScaleFactor = 0.8f; // Scale the image for FS-12 Revoker
                    break;
                case "FS-20 Vortex":
                    flarePos = new Vector2(-80, -70);
                    jammerPos = new Vector2(40, -70);
                    lineStart = new Vector2(-120, -20);
                    lineEnd = new Vector2(120, -20);
                    weaponNamePos = new Vector2(0, 70);
                    weaponAmmoPos = new Vector2(70, 20);
                    weaponImagePos = new Vector2(-60, 20);
                    flareFont = 30;
                    jammerFont = 26;
                    weaponNameFont = 30;
                    weaponAmmoFont = 50;
                    imageScaleFactor = 0.7f; // Scale the image for FS-20 Vortex
                    break;
                case "KR-67 Ifrit":
                    flarePos = new Vector2(-100, -60);
                    jammerPos = new Vector2(60, -60);
                    lineStart = new Vector2(-100, -20);
                    lineEnd = new Vector2(100, -20);
                    weaponNamePos = new Vector2(0, 70);
                    weaponAmmoPos = new Vector2(80, 20);
                    weaponImagePos = new Vector2(-70, 20);
                    flareFont = 40;
                    jammerFont = 36;
                    weaponNameFont = 45;
                    weaponAmmoFont = 55;
                    imageScaleFactor = 0.8f; // Scale the image for KR-67 Ifrit
                    break;
                case "VL-49 Tarantula":
                    flarePos = new Vector2(105, 40);
                    jammerPos = new Vector2(105, -40);
                    lineStart = new Vector2(30, -60);
                    lineEnd = new Vector2(30, 60);
                    weaponNamePos = new Vector2(-60, -10);
                    weaponAmmoPos = new Vector2(-60, -50);
                    weaponImagePos = new Vector2(-60, 40);
                    flareFont = 23;
                    jammerFont = 18;
                    weaponNameFont = 18;
                    weaponAmmoFont = 40;
                    imageScaleFactor = 0.6f;
                    break;
                case "EW-1 Medusa":
                    flarePos = new Vector2(-80, -70);
                    jammerPos = new Vector2(55, -70);
                    lineStart = new Vector2(-100, -20);
                    lineEnd = new Vector2(100, -20);
                    weaponNamePos = new Vector2(0, 70);
                    weaponAmmoPos = new Vector2(80, 20);
                    weaponImagePos = new Vector2(-60, 20);
                    flareFont = 33;
                    jammerFont = 28;
                    weaponNameFont = 30;
                    weaponAmmoFont = 50;
                    imageScaleFactor = 0.6f; // Scale the image for EW-1 Medusa
                    break;
                case "SFB-81":
                    flarePos = new Vector2(0, -160);
                    jammerPos = new Vector2(0, -240);
                    lineStart = new Vector2(-160, -100);
                    lineEnd = new Vector2(160, -100);
                    weaponNamePos = new Vector2(0, 120);
                    weaponAmmoPos = new Vector2(0, 0);
                    weaponImagePos = new Vector2(0, 230);
                    flareFont = 60;
                    jammerFont = 60;
                    weaponNameFont = 60;
                    weaponAmmoFont = 150;
                    imageScaleFactor = 1.4f;
                    break;
                case "UH-80 Ibis":
                    flarePos = new Vector2(-80, -70);
                    jammerPos = new Vector2(50, -70);
                    chaffPos = new Vector2(0, -40);
                    lineStart = new Vector2(-100, -20);
                    lineEnd = new Vector2(100, -20);
                    weaponNamePos = new Vector2(0, 70);
                    weaponAmmoPos = new Vector2(80, 20);
                    weaponImagePos = new Vector2(-60, 20);
                    flareFont = 30;
                    jammerFont = 30;
                    chaffFont = 30;
                    weaponNameFont = 35;
                    weaponAmmoFont = 45;
                    imageScaleFactor = 0.6f;
                    break;
                case "A-19 Brawler":
                    flarePos = new Vector2(0, -30);
                    jammerPos = new Vector2(0, -60);
                    lineStart = new Vector2(-80, -10);
                    lineEnd = new Vector2(80, -10);
                    weaponNamePos = new Vector2(0, 45);
                    weaponAmmoPos = new Vector2(0, 15);
                    weaponImagePos = new Vector2(0, 75);
                    flareFont = 28;
                    jammerFont = 20;
                    weaponNameFont = 25;
                    weaponAmmoFont = 40;
                    break;
                case "Alkyon AB-4":
                case "AB-4 Alkyon":
                case "FastBomber1":
                    flarePos = new Vector2(0, -160);
                    jammerPos = new Vector2(0, -300);
                    lineStart = new Vector2(-320, -40);
                    lineEnd = new Vector2(320, -40);
                    weaponNamePos = new Vector2(0, 180);
                    weaponAmmoPos = new Vector2(0, 60);
                    weaponImagePos = new Vector2(0, 300);
                    flareFont = 120;
                    jammerFont = 120;
                    weaponNameFont = 100;
                    weaponAmmoFont = 160;
                    imageScaleFactor = 2.0f;
                    break;
                case "VT-7 Vagrant" :
                    flarePos = new Vector2(0, -30);
                    jammerPos = new Vector2(0, -65);
                    lineStart = new Vector2(-60, -5);
                    lineEnd = new Vector2(60, -5);
                    weaponNamePos = new Vector2(0, 50);
                    weaponAmmoPos = new Vector2(0, 20);
                    weaponImagePos = new Vector2(0, 70);
                    flareFont = 28;
                    jammerFont = 20;
                    weaponNameFont = 18;
                    weaponAmmoFont = 40;
                    break;
                //modded planes
                case "FS-3 Ternion":
                    flarePos = new Vector2(15, -20);
                    jammerPos = new Vector2(15, -50);
                    lineStart = new Vector2(-50, 5);
                    lineEnd = new Vector2(80, 5);
                    weaponAmmoPos = new Vector2(15, 25);
                    weaponNamePos = new Vector2(15, 50);
                    weaponImagePos = new Vector2(-80, 0);
                    flareFont = 26;
                    jammerFont = 26;
                    weaponNameFont = 20;
                    weaponAmmoFont = 35;
                    imageScaleFactor = 0.6f;
                    rotateWeaponImage = true;
                    break;
                case "MiG-15":
                    flarePos = new Vector2(80, 30);
                    jammerPos = new Vector2(80, -40);
                    lineStart = new Vector2(5, -70);
                    lineEnd = new Vector2(5, 70);
                    weaponNamePos = new Vector2(-90, 0);
                    weaponAmmoPos = new Vector2(-90, -40);
                    weaponImagePos = new Vector2(-90, 40);
                    flareFont = 28;
                    jammerFont = 28;
                    weaponNameFont = 30;
                    weaponAmmoFont = 40;
                    imageScaleFactor = 0.75f;
                    break;
                case "FQ-106 Kestrel":
                default:
                    flarePos = new Vector2(0, -40);
                    jammerPos = new Vector2(0, -80);
                    lineStart = new Vector2(-60, -7);
                    lineEnd = new Vector2(60, -7);
                    weaponNamePos = new Vector2(0, 60);
                    weaponAmmoPos = new Vector2(0, 22);
                    weaponImagePos = new Vector2(0, 87);
                    flareFont = 33;
                    jammerFont = 33;
                    weaponNameFont = 25;
                    weaponAmmoFont = 45;
                    break;
            }

            // Store original font sizes
            originalFlareFontSize = flareFont;
            originalJammerFontSize = jammerFont;
            originalChaffFontSize = chaffFont;
            originalWeaponAmmoFontSize = weaponAmmoFont;

            // Hide the existing MFD content and kill the layout
            if (removeOriginalMFDContent) {
                UIBindings.Generic.HideChildren(destination);
            }

            UIBindings.Generic.KillLayout(destination);

            switch (platformName) {
                case "MiG-15":
                case "Alkyon AB-4":
                case "AB-4 Alkyon":
                case "FastBomber1":
                    var image = weaponDisplay_transform.GetComponent<Image>();
                    image.enabled = false;
                    break;
            }

            // move the BasicFlightInstruments higher on Chicane screen
            if (platformName == "SAH-46 Chicane") {
                Transform toMove;
                toMove = destination.Find("Heading");
                toMove.transform.localPosition += new Vector3(-40, 40, 0);
                toMove.transform.localScale *= 0.8f;
                toMove = destination.Find("Airspeed");
                toMove.transform.localPosition += new Vector3(40, 60, 0);
                toMove.transform.localScale *= 0.8f;
                toMove = destination.Find("RadarAlt");
                toMove.transform.localPosition += new Vector3(-40, 80, 0);
                toMove.transform.localScale *= 0.8f;
                toMove = destination.Find("Horizon");
                toMove.transform.localPosition += new Vector3(0, 60, 0);
                toMove.transform.localScale *= 0.8f;
                toMove = destination.Find("ClimbRate");
                toMove.transform.localPosition += new Vector3(40, 60, 0);
                toMove.transform.localScale *= 0.8f;
                toMove = destination.Find("VerticalLadder");
                toMove.transform.localPosition += new Vector3(0, 55, 0);
                toMove.transform.localScale *= 0.8f;
                toMove = destination.Find("AoAlLadder");
                toMove.transform.localPosition += new Vector3(0, 55, 0);
                toMove.transform.localScale *= 0.8f;
            }

            // Create the labels and line for the systems MFD
            flareLabel = new(
                "flareLabel",
                flarePos,
                destination,
                FontStyle.Normal,
                InternalState.textColor,
                flareFont,
                0f
            );
            flareLabel.SetText("⇌");
            jammerLabel = new(
                "jammerLabel",
                jammerPos,
                destination,
                FontStyle.Normal,
                InternalState.textColor,
                jammerFont,
                0f
            );
            jammerLabel.SetText("⇌");
            if (InternalState.hasChaff) {
                chaffLabel = new(
                    "chaffLabel",
                    chaffPos,
                    destination,
                    FontStyle.Normal,
                    InternalState.textColor,
                    chaffFont,
                    0f
                );
                chaffLabel.SetText("⇌");
            }
            MFD_systemsLine = new(
                "MFD_systemsLine",
                lineStart,
                lineEnd,
                destination,
                InternalState.mainColor,
                1f
            );
            weaponNameLabel = new(
                "weaponNameLabel",
                weaponNamePos,
                destination,
                FontStyle.Normal,
                InternalState.textColor,
                weaponNameFont,
                0f
            );
            weaponNameLabel.SetText("");
            weaponAmmoLabel = new(
                "weaponAmmoLabel",
                weaponAmmoPos,
                destination,
                FontStyle.Normal,
                InternalState.textColor,
                weaponAmmoFont,
                0f
            );
            weaponAmmoLabel.SetText("");
            // Clone the weapon image and set it as a child of the systems MFD
            if (GameBindings.Player.Aircraft.Weapons.GetStationCount() != 0)
                weaponImageClone =
                    GameObject.Instantiate(GameBindings.Player.Aircraft.Weapons.GetActiveStationImage().gameObject,
                        destination);
            else
                weaponImageClone = new UIBindings.Draw.UIRectangle(
                    "empty_texture",
                    new Vector2(0, 0),
                    new Vector2(1, 1),
                    destination,
                    Color.black
                ).GetGameObject();
            var cloneImg = weaponImageClone.GetComponent<Image>();
            cloneImg.rectTransform.sizeDelta = new Vector2(
                cloneImg.rectTransform.sizeDelta.x * imageScaleFactor,
                cloneImg.rectTransform.sizeDelta.y * imageScaleFactor);
            cloneImg.rectTransform.anchoredPosition = weaponImagePos;
            //rotate the image 90 degrees clockwise
            if (rotateWeaponImage) cloneImg.rectTransform.localRotation = Quaternion.Euler(0, 0, -90);
        }

        public void ToggleChildrenActiveState() {
            if (weaponDisplay_transform == null)
                return;

            switch (GameBindings.Player.Aircraft.GetPlatformName()) {
                case "MiG-15":
                case "Alkyon AB-4":
                case "AB-4 Alkyon":
                case "FastBomber1":
                    var image = weaponDisplay_transform.GetComponent<Image>();
                    image.enabled = !image.enabled;
                    break;
            }

            LayoutGroup lg = weaponDisplay_transform.GetComponent<LayoutGroup>();
            if (lg != null)
                lg.enabled = !lg.enabled;
            foreach (Transform childTransform in weaponDisplay_transform) {
                GameObject child = childTransform.gameObject;
                //Specific fix for the Medusa, ThrottleGauge1 was initially hidden
                if (child.name != "ThrottleGauge1") {
                    child.SetActive(!child.activeSelf);
                }
            }
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
