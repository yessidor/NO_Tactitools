using HarmonyLib;
using UnityEngine;
using UnityEngine.UI; //Text
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;
using System;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

[HarmonyPatch(typeof(MainMenu), "Start")]
class HMDDeclutterPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[HMDD] HMD Declutter plugin starting !");

            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnTargetListSelectorStart));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnHUDUnitMarkerUpdatePosition));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnHUDUnitMarkerUpdateMaximized));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnHUDUnitMarkerUpdateHidden));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnTargetListSelectorCheckAllExclusions));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnHUDUnitMarkerSetNew));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnTargetListSelectorToggleButtonCheckFactions));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnHUDUnitMarkerSetOutdated));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnHUDUnitMarkerSelectMarker));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnHUDUnitMarkerDeselectMarker));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnHUDUnitMarkerRemoveIcon));

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
                new (typeof(HMDDeclutterComponent), "OutdatedTime", Plugin.HMDDeclutter.OutdatedTime),
                new (typeof(HMDDeclutterComponent), "ShowOutdatedTime", Plugin.HMDDeclutter.ShowOutdatedTime),
                new (typeof(HMDDeclutterComponent), "HideOutdatedMarker", Plugin.HMDDeclutter.HideOutdatedMarker),
                new (typeof(HMDDeclutterComponent), "SetOutdatedIcon", Plugin.HMDDeclutter.SetOutdatedIcon),
                new (typeof(HMDDeclutterComponent), "EndOutdatedMarkerOpacity", Plugin.HMDDeclutter.EndOutdatedMarkerOpacity),
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
    public static float OutdatedTime = -1;
    public static bool ShowOutdatedTime = true;
    public static bool HideOutdatedMarker = false;
    public static bool SetOutdatedIcon = false;
    public static float EndOutdatedMarkerOpacity = 0.25f;

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
            bool enabled = !___hidden;

            if (__instance.selected || ___flashing) {
                enabled = true;
            }
            else {
                var unit = __instance.unit;
                if (!hq.TryGetKnownPosition(unit, out var knownPosition)) {
                    enabled = false;
                }
                else {
                    float squaredComparedDistance = squaredDistances[idx];
                    float squaredCurrentDistance = FastMath.SquareDistance(viewPosition, knownPosition);

                    enabled &= !(squaredComparedDistance != 0f && squaredCurrentDistance > squaredComparedDistance);
                }
            }

            if (__instance.outdated) {
                if (infos.TryGetValue(__instance, out var info)) {
                    var timeSinceOutdated = Time.time - info.lastSeen;
                    enabled &= !(HideOutdatedMarker && OutdatedTime > 0 && timeSinceOutdated > OutdatedTime);
                }
            }

            __instance.image.enabled = enabled;
            ___hidden = !enabled;
            return enabled;
        }

        public static void Postfix(ref HUDUnitMarker __instance, ref bool ___hidden, ref bool __state, ref Sprite ___icon) {
            var enabled = __instance.image.enabled;
            var outdated = __instance.outdated;

            if (infos.TryGetValue(__instance, out var info)) {
                var timeSinceOutdated = Time.time - info.lastSeen;

                var text = info.text;
                if (text != null) {
                    text.enabled = enabled;
                    if (enabled) {
                        text.transform.position = __instance.image.transform.position;
                        text.text = $"{timeSinceOutdated:F0}";
                        text.color = __instance.image.color;
                    }
                }

                if (enabled && outdated && EndOutdatedMarkerOpacity > 0 && OutdatedTime > 0) {
                    var opacity = Mathf.Lerp(1.0f, EndOutdatedMarkerOpacity, timeSinceOutdated / OutdatedTime);
                    var color = __instance.image.color;
                    __instance.image.color = new Color (color.r, color.g, color.b, opacity);
                }
            }

            if (enabled && SetOutdatedIcon)
                __instance.image.sprite = (outdated ? GameAssets.i.targetUnitSpriteOld : ___icon);

            ___hidden = __state;
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "UpdateMaximized")]
    public class OnHUDUnitMarkerUpdateMaximized {
        public static void Postfix(bool enemy, ref HUDUnitMarker __instance, ref bool ___hidden, ref bool ___flashing, ref bool ___maximized, ref Transform ____transform, ref Sprite ___icon) {
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

            //Needed to override UpdateMaximized() setting image.sprite to icon if alwaysMaximized == true
            if (SetOutdatedIcon)
                __instance.image.sprite = (__instance.outdated ? GameAssets.i.targetUnitSpriteOld : ___icon);

            //OnHUDUnitMarkerUpdatePosition.Prefix() will set image.enabled based on value of 'hidden'
            __instance.image.enabled = false;
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "UpdateHidden")]
    public class OnHUDUnitMarkerUpdateHidden {
        public static void Postfix(ref HUDUnitMarker __instance, ref Sprite ___icon) {
            //Needed to override UpdateHidden() setting image.sprite to icon if hidden != false
            if (SetOutdatedIcon)
                __instance.image.sprite = (__instance.outdated ? GameAssets.i.targetUnitSpriteOld : ___icon);
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

    private struct MarkerInfo {
        public Text text;
        public float lastSeen;

        public MarkerInfo (Text text, float lastSeen) {
            this.text = text;
            this.lastSeen = lastSeen;
        }
    }

    private static Dictionary<HUDUnitMarker, MarkerInfo> infos = new ();
    private static TraverseCache<CombatHUD, Text> targetInfoCache = new ("targetInfo");

    private static void RemoveFromInfos(HUDUnitMarker marker) {
        var text = infos[marker].text;
        if (text != null)
            UnityEngine.Object.Destroy(text.gameObject);
        infos.Remove(marker);
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "SetOutdated")]
    public class OnHUDUnitMarkerSetOutdated {
        private static TraverseCache<CombatHUD, Text> targetTextCache = new ("targetText");
        public static void Postfix(bool newState, ref HUDUnitMarker __instance, ref Sprite ___icon) {
            if (newState) {
                Text text = null;
                if (ShowOutdatedTime) {
                    var combatHUD = UIBindings.Game.GetCombatHUDComponent();
                    text = GameObject.Instantiate(targetInfoCache.GetValue(combatHUD), combatHUD.iconLayer.transform);
                    text.color = __instance.image.color;
                    text.text = "";
                    text.alignment = TextAnchor.UpperRight;
                    text.raycastTarget = false;
                    text.enabled = true;
                }
                infos[__instance] = new MarkerInfo (text, Time.time);
            }
            else {
                //technically, in this case infos should contain __instance
                if (infos.ContainsKey(__instance)) {
                    RemoveFromInfos(__instance);
                }
            }

            if (SetOutdatedIcon)
                __instance.image.sprite = (newState ? GameAssets.i.targetUnitSpriteOld : ___icon);
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "SelectMarker")]
    public class OnHUDUnitMarkerSelectMarker {
        public static void Postfix(ref HUDUnitMarker __instance, ref Sprite ___icon) {
            if (SetOutdatedIcon)
                __instance.image.sprite = (__instance.outdated ? GameAssets.i.targetUnitSpriteOld : ___icon);
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "DeselectMarker")]
    public class OnHUDUnitMarkerDeselectMarker {
        public static void Postfix(ref HUDUnitMarker __instance, ref Sprite ___icon) {
            if (SetOutdatedIcon)
                __instance.image.sprite = (__instance.outdated ? GameAssets.i.targetUnitSpriteOld : ___icon);
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "RemoveIcon")]
    public class OnHUDUnitMarkerRemoveIcon {
        public static void Postfix(ref HUDUnitMarker __instance) {
            if (infos.ContainsKey(__instance)) {
                RemoveFromInfos(__instance);
            }
        }
    }
}
