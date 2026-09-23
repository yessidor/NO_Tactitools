using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;
using System;
using NuclearOption.Networking;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

[HarmonyPatch(typeof(MainMenu), "Start")]
class HMDDeclutterPlugin {
    public static bool initialized = false;
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
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnUnitMapIconSetIcon));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnUnitMapIconUpdateIcon));
            Plugin.harmony.PatchAll(typeof(HMDDeclutterComponent.OnAllyInfoLateUpdate));

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
                new (typeof(HMDDeclutterComponent), "DistanceUnit", Plugin.HMDDeclutter.DistanceUnit),
                new (typeof(HMDDeclutterComponent), "Report", Plugin.HMDDeclutter.Report),
                new (typeof(HMDDeclutterComponent), "NotAlwaysMaximized", Plugin.HMDDeclutter.NotAlwaysMaximized),
                new (typeof(HMDDeclutterComponent), "HideMinimized", Plugin.HMDDeclutter.HideMinimized),
                new (typeof(HMDDeclutterComponent), "MinimizeMaximized", Plugin.HMDDeclutter.MinimizeMaximized),
                new (typeof(HMDDeclutterComponent), "EnemyMinimizedMarkerScale", Plugin.HMDDeclutter.EnemyMinimizedMarkerScale),
                new (typeof(HMDDeclutterComponent), "FriendlyMinimizedMarkerScale", Plugin.HMDDeclutter.FriendlyMinimizedMarkerScale),
                new (typeof(HMDDeclutterComponent), "MaximizeOwnMissiles", Plugin.HMDDeclutter.MaximizeOwnMissiles),
                new (typeof(HMDDeclutterComponent), "AlwaysDrawOwnMissiles", Plugin.HMDDeclutter.AlwaysDrawOwnMissiles),
                new (typeof(HMDDeclutterComponent), "IncludeDerivedMissiles", Plugin.HMDDeclutter.IncludeDerivedMissiles),
                new (typeof(HMDDeclutterComponent), "OwnMissilesColor", Plugin.HMDDeclutter.OwnMissilesColor),
                new (typeof(HMDDeclutterComponent), "OwnMissedMissilesColor", Plugin.HMDDeclutter.OwnMissedMissilesColor),
                new (typeof(HMDDeclutterComponent), "OwnMissilesScale", Plugin.HMDDeclutter.OwnMissilesScale),
                new (typeof(HMDDeclutterComponent), "OwnMissilesMapScale", Plugin.HMDDeclutter.OwnMissilesMapScale),
                new (typeof(HMDDeclutterComponent), "FlashBeforeImpactTime", Plugin.HMDDeclutter.FlashBeforeImpactTime),
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
            field = value;
            if (!value)
                foreach (var p in prevAlwaysMaximized)
                    if (p.Key != null)
                        p.Key.alwaysMaximized = p.Value;
            prevAlwaysMaximized.Clear();
            if (value)
                ProcessMarkers();
        }
        get;
    } = false;

    public static bool MaximizeOwnMissiles = false;
    public static bool AlwaysDrawOwnMissiles = false;
    public static bool IncludeDerivedMissiles = false;
    public static Color OwnMissilesColor = Color.cyan;
    public static Color OwnMissedMissilesColor = Color.magenta;
    public static float OwnMissilesScale = 1f;
    public static float OwnMissilesMapScale = 1f;
    public static float FlashBeforeImpactTime = 3f;

    public static bool NeutralsAreFriendly = true;

    public static List<float> Distances {
        set {
            field = [.. value];
            squaredDistances = field.ConvertAll(x => Mathf.Pow(GameBindings.Units.ConvertToMeters(x, DistanceUnit), 2));
            distancesStrings = field.ConvertAll(x => x.ToString());
            idx = 0;
        }
        get;
    } = new ();

    public static float GetCurrentDistance() {
        return Distances.Count > 0 ? GameBindings.Units.ConvertToMeters(Distances[idx], DistanceUnit) : 0;
    }

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

    public static GameBindings.Units.DistanceUnits DistanceUnit {
        set {
            field = value;
            squaredDistances = Distances.ConvertAll(x => Mathf.Pow(GameBindings.Units.ConvertToMeters(x, field), 2));
        }
        get;
    } = GameBindings.Units.DistanceUnits.m;

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
        if (!HMDDeclutterPlugin.initialized) {
            Plugin.Log("[HMDD] Not initialized");
            return;
        }

        if (Distances.Count == 0)
            return;
        idx = (up ? (idx + 1) : (idx - 1 + Distances.Count))  % Distances.Count;
        if (Report) {
            var distanceString = Distances[idx] == 0f ? "unlimited" : string.Format("{0} {1}", distancesStrings[idx], DistanceUnit.ToString());
            var message = string.Format("HMD markers draw distance: <b>{0}</b>", distanceString);
            UIBindings.Game.DisplayToast(message, 3f);
        }
    }

    public static bool IsSettingMarkerColor(HUDUnitMarker marker) {
        return marker.unit is Missile missile && ownMissiles.ContainsKey(missile);
    }

    public static void OnTargetListSelectorStartCallback() {
        inProcess = false;
        prevAlwaysMaximized.Clear();

        List<Missile> toRemove = new ();
        foreach (var missile in ownMissiles.Keys) {
            if (missile == null)
                toRemove.Add(missile);
        }
        foreach (var missile in toRemove)
            ownMissiles.Remove(missile);
    }

    private static TraverseCache<CombatHUD, List<HUDUnitMarker>> markersCache = new ("markers");
    private static MethodInfo updateHiddenInfo = AccessTools.Method(typeof(HUDUnitMarker), "UpdateHidden");
    private static FieldInfo colorInfo = AccessTools.Field(typeof(HUDUnitMarker), "color");
    private static Dictionary<HUDUnitMarker, bool> prevAlwaysMaximized = new ();
    private static bool inProcess = false;

    private static int idx = 0;
    private static List<float> squaredDistances = new ();
    private static List<string> distancesStrings = new ();

    private static void ProcessMarker(HUDUnitMarker marker) {
        var unit = marker.unit;
        if (unit == null)
            return;

        if (MaximizeOwnMissiles && unit is Missile missile && IsPlayersMissile(missile, false)) {
            marker.alwaysMaximized = true;
            colorInfo.SetValue(marker, OwnMissilesColor);
            marker.image.color = OwnMissilesColor;
            marker.image.transform.localScale = Vector3.one * OwnMissilesScale;
            if (TryGetMissileData(missile, out var missileData)) {
                if (missileData.hudUnitMarker == null)
                    missileData.hudUnitMarker = marker;
            }
            else
                ownMissiles[missile] = new MissileData { hudUnitMarker = marker, unitMapIcon = null };
        }
        else if (MaximizeTargetableMarkers) {
            if (!prevAlwaysMaximized.TryGetValue(marker, out bool alwaysMaximized)) {
                alwaysMaximized = NotAlwaysMaximized ? false : marker.alwaysMaximized;
                prevAlwaysMaximized[marker] = alwaysMaximized;
            }

            marker.alwaysMaximized = GameBindings.Player.TargetFilter.CheckExclusions(unit) ? alwaysMaximized : true;
        }
    }

    private static void ProcessMarkers() {
        try {
            //TODO Check whether ProcessMarkers() recursion is possible, and remove inProcess check if not.
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
            var aircraft = GameBindings.Player.Aircraft.GetAircraft();
            bool gearDeployed = aircraft != null ? aircraft.gearDeployed : false;
            List<HUDUnitMarker> markers = markersCache.GetValue(combatHUD);
            foreach (var marker in markers) {
                ProcessMarker(marker);
                updateHiddenInfo.Invoke(marker, new object [] { gearDeployed });
            }
        }
        finally {
            inProcess = false;
        }
    }

    private static void SetIconIfOutdated(HUDUnitMarker marker, ref bool maximized, ref Sprite icon) {
        if (!SetOutdatedIcon)
            return;
        if (marker.alwaysMaximized || (maximized && !MinimizeMaximized))
            marker.image.sprite = (marker.outdated ? GameAssets.i.targetUnitSpriteOld : icon);
    }

    private static bool TryGetMissileData(Unit unit, out MissileData missileData) {
        missileData = null;
        if (unit is Missile missile) {
            if (missile == null) {
                if (ownMissiles.ContainsKey(missile)) {
                    ownMissiles.Remove(missile);
                }
                return false;
            }
            else {
                return ownMissiles.TryGetValue(missile, out missileData);
            }
        }
        else
            return false;
    }

    private static bool TryGetMissileData(UnitMapIcon unitMapIcon, out MissileData missileData) {
        missileData = null;
        return TryGetMissileData(unitMapIcon.unit, out missileData);
    }

    private static bool TryGetMissileData(HUDUnitMarker hudUnitMarker, out MissileData missileData) {
        missileData = null;
        return TryGetMissileData(hudUnitMarker.unit, out missileData);
    }

    /* If includeDerived is true, deliverables launched by player-owned units will also count as belonging to player */
    private static bool IsPlayersMissile(Missile missile, bool includeDerived = true) {
        if (missile == null)
            return false;

        if (includeDerived) {
            if (UnitRegistry.TryGetPersistentUnit(missile.ownerID, out var missilePersistentUnit) &&
                GameManager.GetLocalPlayer<NuclearOption.Networking.BasePlayer>(out var localPlayer))
                return missilePersistentUnit.player == localPlayer;
            else
                return false;
        }
        else {
            var aircraft = GameBindings.Player.Aircraft.GetAircraft();
            return aircraft == null ? false : missile.owner == aircraft;
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
        public static void Prefix(FactionHQ hq, GlobalPosition viewPosition, ref HUDUnitMarker __instance, ref Color ___color, ref bool ___hidden, ref bool ___flashing, ref bool __state) {
            //HUDUnitMarker.UpdatePosition() will return immediately (and not update position) if hidden == true
            //so saving and restoring 'hidden' and setting it to false to force updating position when marker is 'hidden' and 'selected' or 'flashing' at the same time
            __state = ___hidden;
            bool enabled = !___hidden;

            bool? isPlayersMissileMarker = null;
            bool IsPlayersMissileMarker(HUDUnitMarker marker) {
                if (isPlayersMissileMarker == null)
                    isPlayersMissileMarker = marker.unit is Missile missile && ownMissiles.ContainsKey(missile);
                return (bool)isPlayersMissileMarker;
            }

            if (__instance.selected || ___flashing || EMWSComponent.ProcessingMarker(__instance)) {
                enabled = true;
            }
            else {
                var unit = __instance.unit;
                if (!hq.TryGetKnownPosition(unit, out var knownPosition)) {
                    enabled = false;
                }
                else {
                    if (MaximizeOwnMissiles && AlwaysDrawOwnMissiles && IsPlayersMissileMarker(__instance))
                        enabled = true;
                    else {
                        float squaredComparedDistance = squaredDistances[idx];
                        float squaredCurrentDistance = FastMath.SquareDistance(viewPosition, knownPosition);

                        enabled &= !(squaredComparedDistance != 0f && squaredCurrentDistance > squaredComparedDistance);
                    }
                }
            }

            if (__instance.outdated) {
                if (infos.TryGetValue(__instance, out var info)) {
                    var timeSinceOutdated = Time.time - info.lastSeen;
                    enabled &= !(HideOutdatedMarker && OutdatedTime > 0 && timeSinceOutdated > OutdatedTime);
                }
            }

            if (MaximizeOwnMissiles && IsPlayersMissileMarker(__instance)) {
                if (FlashBeforeImpactTime > 0f) {
                    var timeToImpact = (__instance.unit is Missile missile) ? GameBindings.Helpers.ComputeMissileTTI(missile) : -1f;

                    if (timeToImpact > 0f) {
                        ___color = OwnMissilesColor;
                        __instance.image.color = OwnMissilesColor;
                        if (timeToImpact < FlashBeforeImpactTime)
                            ___flashing = true;
                    }
                    else {
                        ___color = OwnMissedMissilesColor;
                        __instance.image.color = OwnMissedMissilesColor;
                    }
                }
            }

            __instance.image.enabled = enabled;
            ___hidden = !enabled;
        }

        //HUDUnitMarker.UpdatePosition() sets HUDUnitMarker.image.enabled to false if marker is off-screen and to true otherwise

        public static void Postfix(ref HUDUnitMarker __instance, ref Color ___color, ref bool ___hidden, ref bool ___maximized, ref bool ___flashing, ref Sprite ___icon, ref bool __state) {
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

            if (enabled)
               SetIconIfOutdated(__instance, ref ___maximized, ref ___icon);

            ___hidden = __state;
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "UpdateMaximized")]
    public class OnHUDUnitMarkerUpdateMaximized {
        public static void Postfix(bool enemy, ref HUDUnitMarker __instance, ref bool ___hidden, ref bool ___flashing, ref bool ___maximized, ref Transform ____transform, ref Sprite ___icon) {
            //Overriding scale set by UpdateMaximized()
            if (MaximizeOwnMissiles && __instance.unit is Missile missile && ownMissiles.ContainsKey(missile)) {
                __instance.alwaysMaximized = true;
                ___hidden = false;
                __instance.image.transform.localScale = Vector3.one * OwnMissilesScale;
                return;
            }

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
                    var aircraft = GameBindings.Player.Aircraft.GetAircraft();
                    ___hidden = aircraft != null ? aircraft.gearDeployed : false;
                }
            }

            if (!___hidden && (!___maximized || MinimizeMaximized)) {
                ____transform.localScale = (enemy ? EnemyMinimizedMarkerScale : FriendlyMinimizedMarkerScale) * Vector3.one;
                __instance.image.sprite = enemy ? combatHUD.minimizedHostile : combatHUD.minimizedFriendly;
            }

            SetIconIfOutdated(__instance, ref ___maximized, ref ___icon);

            /* For now 'image.enabled' will be true only if it was true and 'hidden' was false.
               Later 'image.enabled' will be set by OnHUDUnitMarkerUpdatePosition.Prefix() and HUDUnitMarker.UpdatePosition().
               If 'image.enabled' is just set to false here, minimized markers will blink. */
            __instance.image.enabled &= !___hidden;
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "UpdateHidden")]
    public class OnHUDUnitMarkerUpdateHidden {
        public static void Postfix(ref HUDUnitMarker __instance, ref bool ___maximized, ref Sprite ___icon) {
            //Needed to override UpdateHidden() setting image.sprite to icon if hidden != false
            SetIconIfOutdated(__instance, ref ___maximized, ref ___icon);
        }
    }

    [HarmonyPatch(typeof(TargetListSelector), "CheckAllExclusions")]
    public class OnTargetListSelectorCheckAllExclusions {
        public static void Postfix() {
            ProcessMarkers();
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "SetNew")]
    public class OnHUDUnitMarkerSetNew {
        public static void Postfix(ref HUDUnitMarker __instance) {
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
        public TextMeshProUGUI text;
        public float lastSeen;

        public MarkerInfo (TextMeshProUGUI text, float lastSeen) {
            this.text = text;
            this.lastSeen = lastSeen;
        }
    }

    private static Dictionary<HUDUnitMarker, MarkerInfo> infos = new ();

    private static void RemoveFromInfos(HUDUnitMarker marker) {
        var text = infos[marker].text;
        if (text != null)
            UnityEngine.Object.Destroy(text.gameObject);
        infos.Remove(marker);
    }

    private class MissileData {
        public HUDUnitMarker hudUnitMarker;
        public UnitMapIcon unitMapIcon;
    }
    private static Dictionary<Missile, MissileData> ownMissiles = new ();

    [HarmonyPatch(typeof(HUDUnitMarker), "SetOutdated")]
    public class OnHUDUnitMarkerSetOutdated {
        private static TraverseCache<CombatHUD, Text> targetTextCache = new ("targetText");
        public static void Postfix(bool newState, ref HUDUnitMarker __instance, ref bool ___maximized, ref Sprite ___icon) {
            if (newState) {
                TextMeshProUGUI text = null;
                if (ShowOutdatedTime) {
                    text = UIBindings.Game.DuplicateCombatHUDTargetInfo();
                    text.color = __instance.image.color;
                    text.text = "";
                    text.alignment = TextAlignmentOptions.TopRight;
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

            SetIconIfOutdated(__instance, ref ___maximized, ref ___icon);
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "SelectMarker")]
    public class OnHUDUnitMarkerSelectMarker {
        public static void Postfix(ref HUDUnitMarker __instance, ref bool ___maximized, ref Sprite ___icon) {
            SetIconIfOutdated(__instance, ref ___maximized, ref ___icon);
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "DeselectMarker")]
    public class OnHUDUnitMarkerDeselectMarker {
        public static void Postfix(ref HUDUnitMarker __instance, ref bool ___maximized, ref Sprite ___icon) {
            SetIconIfOutdated(__instance, ref ___maximized, ref ___icon);
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

    [HarmonyPatch(typeof(UnitMapIcon), "SetIcon")]
    public class OnUnitMapIconSetIcon {
        public static void Postfix(ref UnitMapIcon __instance, ref float ___unitSizeFactor) {
            if (MaximizeOwnMissiles) {
                var icon = __instance;
                if (icon.unit is Missile missile && IsPlayersMissile(missile, IncludeDerivedMissiles)) {
                    icon.iconImage.color = OwnMissilesColor;
                    ___unitSizeFactor *= OwnMissilesMapScale;
                    if (TryGetMissileData(missile, out var missileData))
                        missileData.unitMapIcon = icon;
                    else
                        ownMissiles[missile] = new MissileData { hudUnitMarker = null, unitMapIcon = icon };
                }
            }
        }
    }

    [HarmonyPatch(typeof(UnitMapIcon), "UpdateIcon")]
    public class OnUnitMapIconUpdateIcon {
        public static void Postfix(ref UnitMapIcon __instance) {
            //TODO Optimize?
            var icon = __instance;
            if (MaximizeOwnMissiles && TryGetMissileData(icon, out var missileData)) {
                var hudUnitMarker = missileData.hudUnitMarker;
                if (hudUnitMarker != null)
                    icon.iconImage.color = hudUnitMarker.image.color;
            }
        }
    }

    [HarmonyPatch(typeof(AllyInfo), "LateUpdate")]
    public class OnAllyInfoLateUpdate {
        public static void Postfix(ref TextMeshProUGUI ___hoveredAllyInfo, ref HUDUnitMarker ___hoveredAllyMarker) {
            if (___hoveredAllyMarker != null)
                ___hoveredAllyInfo.enabled = ___hoveredAllyMarker.image.enabled;
        }
    }
}
