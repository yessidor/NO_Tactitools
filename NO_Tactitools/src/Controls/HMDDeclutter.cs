using HarmonyLib;
using UnityEngine;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;
using System;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

[HarmonyPatch(typeof(MainMenu), "Start")]
class HMDDeclutterPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[HMDD] HMD Declutter plugin starting !");

            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnTargetListSelectorStart));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnHUDUnitMarkerUpdatePosition));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnHUDUnitMarkerUpdateMaximized));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnTargetListSelectorCheckAllExclusions));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnHUDUnitMarkerSetNew));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnTargetListSelectorToggleButtonCheckFactions));

            InputCatcher.RegisterNewInput(
                Plugin.HMDDeclutter.CycleHMDMarkerDrawDistanceUp,
                0.0001f,
                onRelease: () => HMDDeclutterComponent.CycleDistance(up: true)
            );
            InputCatcher.RegisterNewInput(
                Plugin.HMDDeclutter.CycleHMDMarkerDrawDistanceDown,
                0.0001f,
                onRelease: () => HMDDeclutterComponent.CycleDistance(up: false)
            );

            var bindings = new BindingHelper.Binding[] {
                //MaximizeTargetable and NeutralsAreFriendly are in Plugin.TargetFilterPreset class for backward compatibility
                new (typeof(HMDDeclutterComponent), "MaximizeTargetableMarkers", Plugin.TargetFilterPreset.MaximizeTargetable),
                new (typeof(HMDDeclutterComponent), "NeutralsAreFriendly", Plugin.TargetFilterPreset.NeutralsAreFriendly),
                new (typeof(HMDDeclutterComponent), "DistancesString", Plugin.HMDDeclutter.DistancesString),
                new (typeof(HMDDeclutterComponent), "Unit", Plugin.HMDDeclutter.Unit),
                new (typeof(HMDDeclutterComponent), "Report", Plugin.HMDDeclutter.Report),
                new (typeof(HMDDeclutterComponent), "NotAlwaysMaximized", Plugin.HMDDeclutter.NotAlwaysMaximized),
                new (typeof(HMDDeclutterComponent), "HideMinimized", Plugin.HMDDeclutter.HideMinimized),
                new (typeof(HMDDeclutterComponent), "MinimizeMaximized", Plugin.HMDDeclutter.MinimizeMaximized),
                new (typeof(HMDDeclutterComponent), "EnemyMinimizedMarkerScale", Plugin.HMDDeclutter.EnemyMinimizedMarkerScale),
                new (typeof(HMDDeclutterComponent), "FriendlyMinimizedMarkerScale", Plugin.HMDDeclutter.FriendlyMinimizedMarkerScale),
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;

            Plugin.Log($"[HMDD] HMD Declutter plugin successfully started !");
        }
    }
}


public class HMDDeclutterComponent {
    public static bool NotAlwaysMaximized = false;

    public static bool MaximizeTargetableMarkers {
        set {
            if (!value)
                foreach (var p in prevAlwaysMaximized)
                    if (p.Key != null)
                        p.Key.alwaysMaximized = p.Value;
            prevAlwaysMaximized.Clear();
            field = value;
        }
        get;
    } = false;

    public static bool NeutralsAreFriendly = true;

    public static List<float> Distances {
        set {
            field = [.. value];
            squaredDistances = field.ConvertAll(x => Mathf.Pow(ConvertToMeters(x, Unit), 2));
            distancesStrings = field.ConvertAll(x => x.ToString());
            idx = 0;
        }
        get;
    } = new ();

    public static string DistancesString {
        set {
            List<float> values = new ();
            foreach (var v in value.Split(";")) {
                if (!float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) {
                    Plugin.Log(string.Format("[HMDD] Cannot parse {0} as float, skipping", v));
                    continue;
                }
                if (f < 0f) {
                    Plugin.Log(string.Format("[HMDD] Distance cannot be negative, skipping {0}", f));
                    continue;
                }
                values.Add(f);
            }
            Distances = values;
            field = value;
        }
        private get;
    }

    public enum Units { m, km, ft, mi };

    public static Units Unit {
        set {
            field = value;
            squaredDistances = Distances.ConvertAll(x => Mathf.Pow(ConvertToMeters(x, field), 2));
        }
        get;
    } = Units.m;

    public static bool Report = true;
    public static bool HideMinimized = false;
    public static bool MinimizeMaximized = false;
    public static float EnemyMinimizedMarkerScale = 6f;
    public static float FriendlyMinimizedMarkerScale = 3f;

    public static void CycleDistance(bool up = true) {
        if (Distances.Count == 0)
            return;
        idx = (up ? (idx + 1) : (idx - 1 + Distances.Count))  % Distances.Count;
        if (Report) {
          UIBindings.Game.DisplayToast(string.Format("HMD markers draw distance: <b>{0}</b>", Distances[idx] == 0f ? "unlimited" : string.Format("{0} {1}", distancesStrings[idx], Unit.ToString())), 3f);
        }
    }

    public static void OnTargetListSelectorStartCallback() {
        inProcess = false;
        prevAlwaysMaximized.Clear();
    }

    private static TraverseCache<CombatHUD, List<HUDUnitMarker>> markersCache = new ("markers");
    private static MethodInfo updateHiddenInfo = AccessTools.Method(typeof(HUDUnitMarker), "UpdateHidden");
    private static Dictionary<HUDUnitMarker, bool> prevAlwaysMaximized = new ();
    private static bool inProcess = false;

    private static int idx = 0;
    private static List<float> squaredDistances = new ();
    private static List<string> distancesStrings = new ();

    private static void ProcessMarker(HUDUnitMarker marker) {
        var unit = marker.unit;
        if (unit == null)
            return;
        var targetListSelector = SceneSingleton<TargetListSelector>.i;
        if (!prevAlwaysMaximized.TryGetValue(marker, out bool _))
            prevAlwaysMaximized[marker] = NotAlwaysMaximized ? false : marker.alwaysMaximized;
        if (targetListSelector.CheckExclusions(unit)) {
            if (prevAlwaysMaximized.TryGetValue(marker, out bool alwaysMaximized))
                marker.alwaysMaximized = alwaysMaximized;
        }
        else {
            marker.alwaysMaximized = true;
        }
    }

    private static void ProcessMarkers() {
        try {
            if (!MaximizeTargetableMarkers || inProcess)
                return;

            inProcess = true;

            List<HUDUnitMarker> toDelete = new ();
            foreach (HUDUnitMarker marker in prevAlwaysMaximized.Keys)
                if (marker == null)
                    toDelete.Add(marker);
            foreach (var marker in toDelete)
                prevAlwaysMaximized.Remove(marker);

            var combatHUD = UIBindings.Game.GetCombatHUDComponent();
            if (combatHUD == null)
                return;
            List<HUDUnitMarker> markers = markersCache.GetValue(combatHUD);
            bool gearDeployed = combatHUD?.aircraft?.gearDeployed ?? false;
            foreach (var marker in markers) {
                ProcessMarker(marker);
                updateHiddenInfo.Invoke(marker, new object [] { gearDeployed });
            }
        }
        finally {
            inProcess = false;
        }
    }

    private static float ConvertToMeters(float distance, Units unit) {
        switch (unit) {
            case Units.m:
                return distance;
            case Units.km:
                return distance * 1000f;
            case Units.ft:
                return distance * 0.3048f;
            case Units.mi:
                return distance * 1609.344f;
            default:
                throw new ArgumentException("Unsupported unit");
        }
    }

    [HarmonyPatch(typeof(TargetListSelector), "Start")]
    public class OnTargetListSelectorStart {
        public static void Postfix() {
            OnTargetListSelectorStartCallback();
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "UpdatePosition")]
    public class OnHUDUnitMarkerUpdatePosition {
        public static bool Prefix(FactionHQ hq, GlobalPosition viewPosition, ref HUDUnitMarker __instance, ref bool ___hidden, ref bool ___flashing, ref bool __state) {
            //HUDUnitMarker.UpdatePosition() will return immediately (and not update position) if hidden == true
            //so saving and restoring 'hidden' and setting it to false to force updating position when marker is 'hidden' and selected' or 'flashing' at the same time
            __state = ___hidden;
            if (__instance.selected || ___flashing) {
                __instance.image.enabled = true; //TODO or = !___hidden ?
                ___hidden = false;
                return true;
            }
            else {
                var unit = __instance.unit;
                if (!hq.TryGetKnownPosition(unit, out var knownPosition))
                    return false;
                float squaredComparedDistance = squaredDistances[idx];
                float squaredCurrentDistance = FastMath.SquareDistance(viewPosition, knownPosition);
                bool r = !(___hidden || (squaredComparedDistance != 0f && squaredCurrentDistance > squaredComparedDistance));
                __instance.image.enabled = r;
                return r;
            }
        }

        public static void Postfix(ref bool ___hidden, ref bool __state) {
            ___hidden = __state;
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "UpdateMaximized")]
    public class OnHUDUnitMarkerUpdateMaximized {
        public static void Postfix(bool enemy, ref HUDUnitMarker __instance, ref bool ___hidden, ref bool ___flashing, ref bool ___maximized, ref Transform ____transform) {
            if (__instance.alwaysMaximized || __instance.selected || ___flashing) {
                return;
            }

            var combatHUD = UIBindings.Game.GetCombatHUDComponent();
            if (combatHUD == null)
                return;

            if (HideMinimized) {
                if (!___maximized) {
                    ___hidden = true;
                }
                else {
                    ___hidden = combatHUD?.aircraft?.gearDeployed ?? false;
                }
            }

            if (MinimizeMaximized && ___maximized) {
                ____transform.localScale = (enemy ? EnemyMinimizedMarkerScale : FriendlyMinimizedMarkerScale) * Vector3.one;
                __instance.image.sprite = enemy ? combatHUD.minimizedHostile : combatHUD.minimizedFriendly;
            }

            //OnHUDUnitMarkerUpdatePosition.Prefix() will set image.enabled based on value of 'hidden'
            __instance.image.enabled = false;
        }
    }

    [HarmonyPatch(typeof(TargetListSelector), "CheckAllExclusions")]
    public class OnTargetListSelectorCheckAllExclusions {
        public static void Postfix() {
            Plugin.Log("[TFP] CheckAllExclusions");
            ProcessMarkers();
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "SetNew")]
    public class OnHUDUnitMarkerSetNew {
        public static void Postfix(ref HUDUnitMarker __instance) {
            Plugin.Log("[TFP] SetNew");
            ProcessMarker(__instance);
        }
    }

    [HarmonyPatch(typeof(TargetListSelector_ToggleButton), "CheckFactions")]
    public class OnTargetListSelectorToggleButtonCheckFactions {
        public static void Postfix(ref bool __result, ref TargetListSelector_ToggleButton __instance, ref Unit u) {
            var factionMode = DynamicMap.GetFactionMode(u.NetworkHQ);
            if (factionMode == FactionMode.NoFaction) {
                var sameFaction = __instance.sameFaction;
                if (!NeutralsAreFriendly)
                    sameFaction = !sameFaction;
                if (sameFaction)
                    __result = !__instance.status;
            }
        }
    }
}
