using HarmonyLib;
using UnityEngine;
using NO_Tactitools.Core;
using System.Collections.Generic;
using UnityEngine.UI;

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
            Plugin.harmony.PatchAll(typeof(AmmoConIndicatorComponent.OnHUDUnitMarkerUpdateColor));
            Plugin.harmony.PatchAll(typeof(AmmoConIndicatorComponent.OnCombatHUDShowTargetInfo));

            var bindings = new BindingHelper.Binding[] {
                new (typeof(AmmoConIndicatorComponent.InternalState), "ColorHMDMarker", Plugin.AmmoConIndicator.ColorHMDMarker),
                new (typeof(AmmoConIndicatorComponent.InternalState), "ColorMFDBox", Plugin.AmmoConIndicator.ColorMFDBox),
                new (typeof(AmmoConIndicatorComponent.InternalState), "DrawMFDDot", Plugin.AmmoConIndicator.DrawMFDDot),
                new (typeof(AmmoConIndicatorComponent.InternalState), "HMDTrackedMarkerColor", Plugin.AmmoConIndicator.HMDTrackedMarkerColor),
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
        }

        public static void OnMissileStart(Missile missile) {
            if (UIBindings.Game.GetTargetScreenTransform(true) == null ||
                GameBindings.Player.Aircraft.GetAircraft() == null ||
                UIBindings.Game.GetTacScreenTransform(true) == null) {
                return;
            }
            // no target
            if (missile.targetID == null) return;
            if (missile.targetID.TryGetUnit(out Unit unit)) {
                InternalState.activeMissiles[missile] = unit;
            }
        }

        public static void OnMissileSetTarget(Missile missile, Unit unit) {
            if (UIBindings.Game.GetTargetScreenTransform(true) == null ||
                GameBindings.Player.Aircraft.GetAircraft() == null ||
                UIBindings.Game.GetTacScreenTransform(true) == null) {
                return;
            }
            // make the mod update proof
            if (unit == null) // this always happens when a missile detonates
                InternalState.activeMissiles.Remove(missile);
            else
                InternalState.activeMissiles[missile] = unit;
        }
    }

    static public class InternalState {
        static public Dictionary<Missile, Unit> activeMissiles = [];
        static public readonly TraverseCache<TargetScreenUI, List<Image>> _targetBoxesCache = new("targetBoxes");
        static public readonly TraverseCache<CombatHUD, Dictionary<Unit, HUDUnitMarker>> _markerLookupCache = new("markerLookup");
        static public readonly TraverseCache<CombatHUD, Text> _targetInfoCache = new("targetInfo");
        static public List<Unit> trackedTargets = [];
        static public List<HUDUnitMarker> trackedMarkers = [];
        static public HUDUnitMarker? activeTrackedMarker = null;
        static public bool ColorHMDMarker = true;
        static public bool ColorMFDBox = true;
        static public bool DrawMFDDot = true;
        static public Color HMDTrackedMarkerColor = Color.yellow;
        static public Color HMDDefaultMarkerColor = Color.green;
        static public Color MFDTrackedBoxColor = Color.yellow;
        static public Color MFDDefaultBoxColor = Color.white;
        static public Color MFDTrackedDotColor = new Color(0f, 1f, 0f, 0.95f);
    }

    static public class DisplayEngine {
        public static void Init() {
        }

        public static void Update() {
            if (
                GameBindings.GameState.IsGamePaused()
                || GameBindings.Player.Aircraft.GetAircraft() == null
                || UIBindings.Game.GetTargetScreenTransform(silent: true) == null) {
                return;
            }

            TargetScreenUI targetScreen = UIBindings.Game.GetTargetScreenUIComponent();
            if (targetScreen == null) return;

            List<Unit> targets = GameBindings.Player.TargetList.GetTargets();
            List<Image> targetIcons = InternalState._targetBoxesCache.GetValue(targetScreen);
            HashSet<Unit> trackedUnits = new HashSet<Unit> (InternalState.activeMissiles.Values);

            if (targetIcons == null || targetIcons.Count < targets.Count) return;

            Dictionary<Unit, HUDUnitMarker>? markerLookup = null;
            CombatHUD currentCombatHUD;
            if (InternalState.ColorHMDMarker) {
                currentCombatHUD = UIBindings.Game.GetCombatHUDComponent();
                markerLookup = InternalState._markerLookupCache.GetValue(currentCombatHUD);
                InternalState.trackedTargets.Clear();
                InternalState.trackedMarkers.Clear();
                InternalState.activeTrackedMarker = null;
            }

            //'targetIcons' is indexed by i, so have to walk 'targets' list
            for (int i = 0; i < targets.Count; i++) {
                bool isTracked = trackedUnits.Contains(targets[i]);

                if (isTracked)
                    InternalState.trackedTargets.Add(targets[i]);

                if (InternalState.DrawMFDDot) {
                    UIBindings.Draw.UIRectangle trackerDot = new(
                        "TrackerDot",
                        new Vector2(-5, -30),
                        new Vector2(5, -40),
                        fillColor: InternalState.MFDTrackedDotColor,
                        UIParent: targetIcons[i].rectTransform
                    );
                    trackerDot.GetGameObject().SetActive(isTracked);
                    trackerDot.GetImageComponent().raycastTarget = false;
                }

                if (InternalState.ColorMFDBox)
                    targetIcons[i].color = isTracked ? InternalState.MFDTrackedBoxColor : InternalState.MFDDefaultBoxColor;

                if (InternalState.ColorHMDMarker) {
                    HUDUnitMarker? marker = null;
                    if (isTracked && markerLookup is not null && (bool)markerLookup?.TryGetValue(targets[i], out marker)) {
                        marker.image.color = InternalState.HMDTrackedMarkerColor;
                        InternalState.trackedMarkers.Add(marker);
                        if (i == 0) InternalState.activeTrackedMarker = marker;
                    }
                }
            }
        }

        public static void OnHUDUnitMarkerUpdateColor(HUDUnitMarker marker) {
            if (InternalState.ColorHMDMarker && marker.selected && InternalState.trackedMarkers.Contains(marker))
                marker.image.color = InternalState.HMDTrackedMarkerColor;
        }

        public static void OnCombatHUDShowTargetInfo(CombatHUD combatHUD) {
            Text targetInfo = InternalState._targetInfoCache.GetValue(combatHUD);
            if (InternalState.ColorHMDMarker) {
                HUDUnitMarker? activeTrackedMarker = InternalState.activeTrackedMarker;
                if (activeTrackedMarker != null) {
                    activeTrackedMarker?.image.color = InternalState.HMDTrackedMarkerColor;
                    targetInfo.color = InternalState.HMDTrackedMarkerColor;
                }
                else {
                    targetInfo.color = InternalState.HMDDefaultMarkerColor;
                }
            }
            else {
                targetInfo.color = InternalState.HMDDefaultMarkerColor;
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

    [HarmonyPatch(typeof(HUDUnitMarker), "UpdateColor")]
    public static class OnHUDUnitMarkerUpdateColor {
        static void Postfix(HUDUnitMarker __instance) {
            DisplayEngine.OnHUDUnitMarkerUpdateColor(__instance);
        }
    }

    [HarmonyPatch(typeof(CombatHUD), "ShowTargetInfo")]
    public static class OnCombatHUDShowTargetInfo {
        static void Postfix(CombatHUD __instance) {
            DisplayEngine.OnCombatHUDShowTargetInfo(__instance);
        }
    }
}
