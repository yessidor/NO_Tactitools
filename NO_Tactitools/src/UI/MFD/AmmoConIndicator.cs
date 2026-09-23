using HarmonyLib;
using UnityEngine;
using NO_Tactitools.Core;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace NO_Tactitools.UI.MFD;

[HarmonyPatch(typeof(MainMenu), "Start")]
class AmmoConIndicatorPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[AC] Ammo Conservation Indicator plugin starting !");
            Plugin.harmony.PatchAll(typeof(AmmoConIndicatorComponent.OnPlatformStart));
            Plugin.harmony.PatchAll(typeof(AmmoConIndicatorComponent.OnPlatformUpdate));
            Plugin.harmony.PatchAll(typeof(AmmoConIndicatorComponent.OnMissileStart));
            Plugin.harmony.PatchAll(typeof(AmmoConIndicatorComponent.OnMissileSetTarget));
            Plugin.harmony.PatchAll(typeof(AmmoConIndicatorComponent.OnHUDUnitMarkerUpdatePosition));
            Plugin.harmony.PatchAll(typeof(AmmoConIndicatorComponent.OnCombatHUDShowTargetInfo));

            var bindings = new BindingHelper.Binding[] {
                new (typeof(AmmoConIndicatorComponent.InternalState), "ColorHMDMarker", Plugin.AmmoConIndicator.ColorHMDMarker),
                new (typeof(AmmoConIndicatorComponent.InternalState), "ColorHMDLasedMarker", Plugin.AmmoConIndicator.ColorHMDLasedMarker),
                new (typeof(AmmoConIndicatorComponent.InternalState), "ColorMFDBox", Plugin.AmmoConIndicator.ColorMFDBox),
                new (typeof(AmmoConIndicatorComponent.InternalState), "DrawMFDDot", Plugin.AmmoConIndicator.DrawMFDDot),
                new (typeof(AmmoConIndicatorComponent.InternalState), "HMDTrackedMarkerColor", Plugin.AmmoConIndicator.HMDTrackedMarkerColor),
                new (typeof(AmmoConIndicatorComponent.InternalState), "HMDLasedMarkerColor", Plugin.AmmoConIndicator.HMDLasedMarkerColor),
                new (typeof(AmmoConIndicatorComponent.InternalState), "HMDDefaultMarkerColor", Plugin.AmmoConIndicator.HMDDefaultMarkerColor),
                new (typeof(AmmoConIndicatorComponent.InternalState), "MFDTrackedBoxColor", Plugin.AmmoConIndicator.MFDTrackedBoxColor),
                new (typeof(AmmoConIndicatorComponent.InternalState), "MFDDefaultBoxColor", Plugin.AmmoConIndicator.MFDDefaultBoxColor),
                new (typeof(AmmoConIndicatorComponent.InternalState), "MFDTrackedDotColor", Plugin.AmmoConIndicator.MFDTrackedDotColor)
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log("[AC] Ammo Conservation Indicator plugin successfully started !");
        }
    }
}

class AmmoConIndicatorComponent {
    static public List<Unit> GetTrackedTargets() {
        return InternalState.trackedTargets;
    }

    static public class LogicEngine {
        public static void Init() {
            InternalState.activeMissiles.Clear();
        }

        public static void Update() {
            if (
                GameBindings.GameState.IsGamePaused()
                || GameBindings.Player.Aircraft.GetAircraft() == null)
                return;

            // Prune null or inactive missiles
            List<Missile> toRemove = [];
            foreach (Missile missile in InternalState.activeMissiles.Keys) {
                if (missile == null) {
                    toRemove.Add(missile);
                }
            }
            foreach (var missile in toRemove) {
                InternalState.activeMissiles.Remove(missile);
            }

            List<Unit> toRemoveUnits = [];
            foreach (Unit unit in InternalState.trackedUnits) {
                if (unit == null) {
                    toRemoveUnits.Add(unit);
                }
            }
            foreach (var unit in toRemoveUnits) {
                InternalState.trackedUnits.Remove(unit);
            }
        }

        public static void OnMissileStart(Missile missile) {
            if (UIBindings.Game.GetTargetScreenTransform(true) == null ||
                GameBindings.Player.Aircraft.GetAircraft() == null ||
                UIBindings.Game.GetTacScreenTransform(true) == null) {
                return;
            }
            // no target
            if (missile.targetID == null)
                return;
            // processing only friendly missiles
            var owner = missile.owner;
            if (owner == null)
                return;
            if (GameBindings.GameState.GetFactionMode(owner) != FactionMode.Friendly)
                return;
            if (missile.targetID.TryGetUnit(out Unit unit)) {
                InternalState.activeMissiles[missile] = unit;
                InternalState.trackedUnits.Add(unit);
            }
        }

        public static void OnMissileSetTarget(Missile missile, Unit unit) {
            if (UIBindings.Game.GetTargetScreenTransform(true) == null ||
                GameBindings.Player.Aircraft.GetAircraft() == null ||
                UIBindings.Game.GetTacScreenTransform(true) == null) {
                return;
            }
            // make the mod update proof
            // unit == null always happens when a missile detonates
            if (unit == null) {
                if (InternalState.activeMissiles.TryGetValue(missile, out var prevUnit))
                    InternalState.trackedUnits.Remove(prevUnit);
                InternalState.activeMissiles.Remove(missile);
            }
            else {
                // processing only friendly missiles
                if (GameBindings.GameState.GetFactionMode(missile.owner) == FactionMode.Friendly) {
                    Unit prevUnit = null;
                    InternalState.activeMissiles.TryGetValue(missile, out prevUnit);
                    if (unit != prevUnit) {
                        InternalState.activeMissiles[missile] = unit;
                        InternalState.trackedUnits.Remove(prevUnit);
                        InternalState.trackedUnits.Add(unit);
                    }
                }
            }
        }
    }

    static public class InternalState {
        static public readonly TraverseCache<TargetScreenUI, List<Image>> _targetBoxesCache = new("targetBoxes");
        static public readonly TraverseCache<CombatHUD, Dictionary<Unit, HUDUnitMarker>> _markerLookupCache = new("markerLookup");

        //missile to target relation
        static public Dictionary<Missile, Unit> activeMissiles = [];
        //targets of currently active missiles
        static public HashSet<Unit> trackedUnits = new ();
        //player-selected targets that at the same time are targets of active missiles
        static public List<Unit> trackedTargets = new ();
        //markers of the above
        static public HashSet<HUDUnitMarker> trackedMarkers = new ();
        //marker of active target (whether it's tracked by missile or not)
        static public HUDUnitMarker activeMarker = null;

        static public Dictionary<Image, UIBindings.Draw.UIRectangle> trackerDots = new ();

        static public bool ColorHMDMarker = true;
        static public bool ColorHMDLasedMarker = true;
        static public bool ColorMFDBox = true;
        static public bool DrawMFDDot = true;
        static public Color HMDTrackedMarkerColor = Color.yellow;
        static public Color HMDLasedMarkerColor = Color.red;
        static public Color HMDDefaultMarkerColor = Color.green;
        static public Color MFDTrackedBoxColor = Color.yellow;
        static public Color MFDDefaultBoxColor = Color.white;
        static public Color MFDTrackedDotColor = new Color(0f, 1f, 0f, 0.95f);
    }

    static public class DisplayEngine {
        public static void Init() {
        }

        public static void Update() {
            if (GameBindings.GameState.IsGamePaused() || GameBindings.Player.Aircraft.GetAircraft() == null)
                return;

            List<Unit> targets = GameBindings.Player.TargetList.GetTargets(copy: false);

            TargetScreenUI targetScreen = UIBindings.Game.GetTargetScreenUIComponent();
            List<Image> targetIcons = targetScreen != null ? InternalState._targetBoxesCache.GetValue(targetScreen) : null;

            bool targetIconsValid = !(targetIcons == null || targetIcons.Count < targets.Count);

            if (InternalState.DrawMFDDot) {
                List<Image> toRemove = new ();
                foreach (var icon in InternalState.trackerDots.Keys) {
                    if (icon == null)
                        toRemove.Add(icon);
                }
                foreach (var icon in toRemove)
                    InternalState.trackerDots.Remove(icon);
            }

            Dictionary<Unit, HUDUnitMarker> markerLookup = null;
            if (InternalState.ColorHMDMarker) {
                var currentCombatHUD = UIBindings.Game.GetCombatHUDComponent();
                markerLookup = InternalState._markerLookupCache.GetValue(currentCombatHUD);
                InternalState.trackedMarkers.Clear();
                InternalState.activeMarker = null;
            }
            InternalState.trackedTargets.Clear();

            //'targetIcons' is indexed by i, so have to walk 'targets' list
            for (int i = 0; i < targets.Count; i++) {
                var target = targets[i];

                bool targetIconExists = targetIcons != null && i < targetIcons.Count;

                bool isTracked = InternalState.trackedUnits.Contains(target);

                if (isTracked)
                    InternalState.trackedTargets.Add(target);

                if (InternalState.DrawMFDDot && targetIconExists) {
                    if (!InternalState.trackerDots.TryGetValue(targetIcons[i], out var trackerDot)) {
                        var parentTransform = targetIcons[i].rectTransform;
                        trackerDot = new(
                            "TrackerDot",
                            new Vector2(-5, -30),
                            new Vector2(5, -40),
                            fillColor: InternalState.MFDTrackedDotColor,
                            UIParent: parentTransform
                        );
                        trackerDot.GetImageComponent().raycastTarget = false;
                    }
                    trackerDot.GetGameObject().SetActive(isTracked);
                }

                if (InternalState.ColorMFDBox && targetIconExists)
                    targetIcons[i].color = isTracked ? InternalState.MFDTrackedBoxColor : InternalState.MFDDefaultBoxColor;

                if (InternalState.ColorHMDMarker && markerLookup.TryGetValue(target, out var marker)) {
                    if (i == 0)
                        InternalState.activeMarker = marker;
                    if (isTracked) {
                        InternalState.trackedMarkers.Add(marker);
                    }
                }
            }
        }

        public static void UpdateMarkerColor(HUDUnitMarker marker) {
            if (InternalState.ColorHMDMarker) {
                Color? color = null;
                if (InternalState.trackedMarkers.Contains(marker))
                    color = InternalState.HMDTrackedMarkerColor;
                else if (marker.selected) {
                    if (InternalState.ColorHMDLasedMarker) {
                        var currentWeaponStation = GameBindings.Player.Aircraft.Weapons.GetActiveStation();
                        if (currentWeaponStation != null && currentWeaponStation.WeaponInfo.laserGuided) {
                            var hq = GameBindings.GameState.GetCurrentFactionHQ();
                            if (hq.IsTargetLased(marker.unit))
                                color = InternalState.HMDLasedMarkerColor;
                        }
                    }
                    //not 'else if' !
                    if (color == null)
                        color = InternalState.HMDDefaultMarkerColor;
                }
                if (color != null)
                    marker.image.color = (Color)color;
            }
        }
    }

    // HARMONY PATCHES
    [HarmonyPatch(typeof(Missile), "StartMissile")]
    public static class OnMissileStart {
        static void Postfix(Missile __instance) {
            LogicEngine.OnMissileStart(__instance);
        }
    }

    [HarmonyPatch(typeof(Missile), "SetTarget")]
    public static class OnMissileSetTarget {
        static void Postfix(Missile __instance, Unit target) {
            LogicEngine.OnMissileSetTarget(__instance, target);
        }
    }

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

    [HarmonyPatch(typeof(HUDUnitMarker), "UpdatePosition")]
    public static class OnHUDUnitMarkerUpdatePosition {
        static void Postfix(HUDUnitMarker __instance) {
            DisplayEngine.UpdateMarkerColor(__instance);
        }
    }

    private struct State {
        public HUDUnitMarker marker;
        public Color color;
    }

    //Make ShowTargetInfo() not change active marker image color and make targetInfo inherit that color
    [HarmonyPatch(typeof(CombatHUD), "ShowTargetInfo")]
    public static class OnCombatHUDShowTargetInfo {
        static void Prefix(ref State __state) {
            if (InternalState.ColorHMDMarker) {
                var marker = InternalState.activeMarker;
                if (marker != null) {
                    __state.marker = marker;
                    __state.color = marker.image.color;
                }
                else
                    __state.marker = null;
            }
        }

        static void Postfix(ref TextMeshProUGUI ___targetInfo, ref State __state) {
            if (InternalState.ColorHMDMarker && __state.marker != null) {
                __state.marker.image.color = __state.color;
                ___targetInfo.color = __state.color;
            }
        }
    }
}
