using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NuclearOption.Networking;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

[HarmonyPatch(typeof(MainMenu), "Start")]
class AltTargetSelectionPlugin {
    static void Postfix() {
        AltTargetSelectionComponent.Init(); 
    }
}


public enum AltTargetSelectionModes { distance, angle };

class AltTargetSelectionComponent {
    private static bool initialized = false;
    public static void Init() {
        if (!initialized) {
            Plugin.Log("[TS] Alternative Target Selection Plugin initializing");

            TargetableMarkersCacheComponent.Init();

            Plugin.harmony.PatchAll(typeof(OnCombatHUDTargetSelect));

            if (Plugin.AltTargetSelection.ShowUnitInfo.Value) {
                Plugin.harmony.PatchAll(typeof(OnCombatHUDSetAircraft));
                Plugin.harmony.PatchAll(typeof(OnCombatHUDUpdateMarkers));
            }

            var bindings = new BindingHelper.Binding[] {
                new (typeof(AltTargetSelectionComponent), "FOVFraction", Plugin.AltTargetSelection.FOVFraction),
                new (typeof(AltTargetSelectionComponent), "MaxDistance", Plugin.AltTargetSelection.MaxDistance),
                new (typeof(AltTargetSelectionComponent), "PickActive", Plugin.AltTargetSelection.PickActive),
                new (typeof(AltTargetSelectionComponent), "SelectionMode", Plugin.AltTargetSelection.SelectionMode),
                new (typeof(AltTargetSelectionComponent), "ShowUnitInfoForSelected", Plugin.AltTargetSelection.ShowUnitInfoForSelected)
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log("[TS] Alternative Target Selection Plugin initialized");
        }
    }

    public static float FOVFraction { set; get; } = 0.1f;
    public static float MaxDistance { set; get; } = 0f;
    public static bool PickActive = false;
    public static AltTargetSelectionModes SelectionMode = AltTargetSelectionModes.distance;
    public static bool ShowUnitInfoForSelected = false;

    public struct PreviewedMarkerData {
        public HUDUnitMarker marker;
        public float distance;
    }

    public static event Action<PreviewedMarkerData> OnPreviewedMarkerUpdate;

    public static bool TargetSelect(ref CombatHUD __instance, ref bool paint) {
        if (DynamicMap.mapMaximized)
            return false;

        if (paint) {
            var targetableMarkers = TargetableMarkersCacheComponent.GetTargetableMarkers();
            var markerDatum = PickMarkers(markers: targetableMarkers, pickActive: PickActive, paint: true);
            if (markerDatum.Count > 0)
                GameBindings.Player.TargetList.AddTargets(markerDatum.ConvertAll(d => d.marker.unit));
        }
        else {
            HUDUnitMarker marker = null;

            if (previewedMarker != null)
                marker = previewedMarker;
            else {
                var targetableMarkers = TargetableMarkersCacheComponent.GetTargetableMarkers();
                var markerDatum = PickMarkers(markers: targetableMarkers, pickActive: PickActive, paint: false);
                if (markerDatum.Count > 0)
                    marker = markerDatum[0].marker;
            }

            if (marker != null) {
                var unit = marker.unit;
                if (!marker.selected)
                    GameBindings.Player.TargetList.AddTarget(unit);
                else if (PickActive) {
                    GameBindings.Player.TargetList.DeselectUnit(unit);
                    GameBindings.Player.TargetList.AddTarget(unit);
                }
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(CombatHUD), "TargetSelect")]
    public class OnCombatHUDTargetSelect {
        static bool Prefix(ref CombatHUD __instance, ref bool paint) {
            return TargetSelect(ref __instance, ref paint);
        }
    }

    private static void RefreshPreviewedMarker() {
        HUDUnitMarker marker = null;
        float distance = -1f;

        var targetableMarkers = TargetableMarkersCacheComponent.GetTargetableMarkers();
        var markerDatum = PickMarkers(markers: targetableMarkers, pickActive: true, paint: false);
        if (markerDatum.Count == 0) {
            marker = null;
        }
        else if (markerDatum.Count == 1) {
            var markerData = markerDatum[0];
            marker = markerData.marker;
            distance = markerData.worldDistance;
        }
        else {
            Plugin.Log($"[TS] Unexpected number of markers: {markerDatum.Count}");
            return;
        }

        if (marker != previewedMarker || distance != previewedMarkerWorldDistance) {
            previewedMarker = marker;
            previewedMarkerWorldDistance = distance;
            OnPreviewedMarkerUpdate?.Invoke (new PreviewedMarkerData { marker = marker, distance = distance });
        }
    }

    private struct MarkerData {
        public HUDUnitMarker marker;
        public float worldDistance;
        public float screenDistance;
    };

    private static List<MarkerData> PickMarkers(IEnumerable<HUDUnitMarker> markers, bool pickActive, bool paint) {
        bool ShouldSelect(float worldDistanceCurrent, float worldDistanceCompared, float screenDistanceCurrent, float screenDistanceCompared) {
            if (SelectionMode == AltTargetSelectionModes.distance)
                return worldDistanceCurrent < worldDistanceCompared;
            else if (SelectionMode == AltTargetSelectionModes.angle)
                return screenDistanceCurrent < screenDistanceCompared;
            else
                throw new Exception ($"Unknown selection mode: {SelectionMode}");
        }

        List<MarkerData> result = new ();

        var combatHUD = UIBindings.Game.GetCombatHUDComponent();
        if (combatHUD == null)
            return result;
        var aircraft = combatHUD.aircraft;
        if (aircraft == null)
            return result;
        var hq = aircraft.NetworkHQ;
        if (hq == null)
            return result;

        var camera = UIBindings.Game.GetCameraStateManager().mainCamera;

        var halfFOV = 0.5f * Mathf.Deg2Rad * camera.fieldOfView;
        var angle = halfFOV * FOVFraction;
        var yn = Mathf.Tan(angle) / Mathf.Tan(halfFOV);
        var screenDistanceThreshold = 0.5f * camera.pixelHeight * yn;
        var screenDistanceSquaredThreshold = screenDistanceThreshold * screenDistanceThreshold;

        var cameraTransform = camera.transform;
        var worldCameraPosition = cameraTransform.position.ToGlobalPosition();
        var worldDistanceSquaredThreshold = MaxDistance * MaxDistance;

        var targetDesignatorPosition = combatHUD.targetDesignator.gameObject.transform.position;

        HUDUnitMarker markerUnselected = null;
        var screenDistanceSquaredUnselected = float.PositiveInfinity;
        var worldDistanceSquaredUnselected = float.PositiveInfinity;

        HUDUnitMarker markerSelected = null;
        var screenDistanceSquaredSelected = float.PositiveInfinity;
        var worldDistanceSquaredSelected = float.PositiveInfinity;

        foreach (var marker in markers) {
            if (!marker.image.enabled)
                continue;
            var markerPosition = marker.image.transform.position;
            var screenDistanceSquared = FastMath.SquareDistance(targetDesignatorPosition, markerPosition);
            if (screenDistanceSquared > screenDistanceSquaredThreshold)
                continue;
            if (!hq.TryGetKnownPosition(marker.unit, out var worldUnitPosition))
                continue;
            var worldDistanceSquared = FastMath.SquareDistance(worldCameraPosition, worldUnitPosition);
            if (MaxDistance != 0f && worldDistanceSquared > worldDistanceSquaredThreshold)
                continue;

            if (!marker.selected) {
                if (paint)
                    result.Add(new MarkerData { marker = marker, worldDistance = Mathf.Sqrt(worldDistanceSquared), screenDistance = Mathf.Sqrt(screenDistanceSquared) });
                else if (ShouldSelect(worldDistanceSquared, worldDistanceSquaredUnselected, screenDistanceSquared, screenDistanceSquaredUnselected)) {
                    markerUnselected = marker;
                    worldDistanceSquaredUnselected = worldDistanceSquared;
                    screenDistanceSquaredUnselected = screenDistanceSquared;
                }
            }
            else if (pickActive && ShouldSelect(worldDistanceSquared, worldDistanceSquaredSelected, screenDistanceSquared, screenDistanceSquaredSelected)) {
                markerSelected = marker;
                worldDistanceSquaredSelected = worldDistanceSquared;
                screenDistanceSquaredSelected = screenDistanceSquared;
            }
        }

        if (!paint) {
            if (markerUnselected != null)
                result.Add(new MarkerData { marker = markerUnselected, worldDistance = Mathf.Sqrt(worldDistanceSquaredUnselected), screenDistance = Mathf.Sqrt(screenDistanceSquaredUnselected) });
            else if (pickActive && markerSelected != null)
                result.Add(new MarkerData { marker = markerSelected, worldDistance = Mathf.Sqrt(worldDistanceSquaredSelected), screenDistance = Mathf.Sqrt(screenDistanceSquaredSelected) });
        }

        return result;
    }

    private static TraverseCache<CombatHUD, List<HUDUnitMarker>> markersCache = new ("markers");
    private static TextMeshProUGUI previewedMarkerInfo = null;

    private static HUDUnitMarker previewedMarker = null;
    private static float previewedMarkerWorldDistance = -1f;

    private static void CreatePreviewedInfo() {
        if (previewedMarkerInfo != null)
            UnityEngine.Object.Destroy(previewedMarkerInfo.gameObject);
        previewedMarkerInfo = UIBindings.Game.DuplicateCombatHUDTargetInfo();
        previewedMarkerInfo.alignment = TextAlignmentOptions.Center;
        previewedMarkerInfo.enabled = false;
    }

    private static void RefreshPreviewedInfo() {
        RefreshPreviewedMarker();

        if (ShowUnitInfoForSelected) {
            var activeTarget = GameBindings.Player.TargetList.GetActiveTarget();
            previewedMarkerInfo.enabled = previewedMarker != null && previewedMarker.unit != activeTarget;
        }
        else
            previewedMarkerInfo.enabled = previewedMarker != null && !previewedMarker.selected;

        if (previewedMarkerInfo.enabled) {
            previewedMarkerInfo.text = "";
            var unit = previewedMarker.unit;
            Aircraft aircraft = unit as Aircraft;
            if (aircraft != null && aircraft.pilots[0] != null && aircraft.Player != null) {
                previewedMarkerInfo.text = aircraft.Player.GetDisplayName(PlayerNameContext.Other) + "\n";
            }
            previewedMarkerInfo.text = previewedMarkerInfo.text + unit.definition.code + "\n\n\n" + (previewedMarkerWorldDistance > 0f ? UnitConverter.DistanceReading(previewedMarkerWorldDistance) : "?");
            previewedMarkerInfo.color = previewedMarker.image.color;
            previewedMarkerInfo.transform.position = previewedMarker.image.transform.position;
        }
    }

    [HarmonyPatch(typeof(CombatHUD), "SetAircraft")]
    public class OnCombatHUDSetAircraft {
        public static void Postfix() {
            CreatePreviewedInfo();
        }
    }

    //TODO Update in async that is called less frequently ?
    [HarmonyPatch(typeof(CombatHUD), "UpdateMarkers")]
    public class OnCombatHUDUpdateMarkers {
        public static void Postfix() {
            RefreshPreviewedInfo();
        }
    }
}


class TargetableMarkersCacheComponent {
    public static void Init() {
        if (!initialized) {
            Plugin.harmony.PatchAll(typeof(OnTargetListSelectorSetFilters));
            Plugin.harmony.PatchAll(typeof(OnTargetListSelectorCheckAllExclusions));
            Plugin.harmony.PatchAll(typeof(OnHUDUnitMarkerSetNew));
            Plugin.harmony.PatchAll(typeof(OnHUDUnitMarkerRemoveIcon));

            initialized = true;
        }
    }

    public static HashSet<HUDUnitMarker> GetTargetableMarkers() {
        return targetableMarkers;
    }

    public static void RefreshTargetableMarkers() {
        targetableMarkers.Clear();

        var combatHUD = UIBindings.Game.GetCombatHUDComponent();
        if (combatHUD == null)
            return;
        var markers = markersCache.GetValue(combatHUD);
        foreach (var marker in markers) {
            var unit = marker.unit;
            if (unit == null)
                continue;
            if (!GameBindings.Player.TargetFilter.CheckExclusions(unit))
                targetableMarkers.Add(marker);
        }
    }

    private static bool initialized = false;
    private static HashSet<HUDUnitMarker> targetableMarkers = new ();
    private static TraverseCache<CombatHUD, List<HUDUnitMarker>> markersCache = new ("markers");

    [HarmonyPatch(typeof(TargetListSelector), "SetFilters")]
    private class OnTargetListSelectorSetFilters {
        public static void Postfix() {
            RefreshTargetableMarkers();
        }
    }

    [HarmonyPatch(typeof(TargetListSelector), "CheckAllExclusions")]
    private class OnTargetListSelectorCheckAllExclusions {
        public static void Postfix() {
            RefreshTargetableMarkers();
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "SetNew")]
    private class OnHUDUnitMarkerSetNew {
        public static void Postfix(HUDUnitMarker __instance) {
            var marker = __instance;
            if (!GameBindings.Player.TargetFilter.CheckExclusions(marker.unit))
                targetableMarkers.Add(marker);
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "RemoveIcon")]
    private class OnHUDUnitMarkerRemoveIcon {
        public static void Postfix(HUDUnitMarker __instance) {
            var marker = __instance;
            targetableMarkers.Remove(marker);
        }
    }
}
