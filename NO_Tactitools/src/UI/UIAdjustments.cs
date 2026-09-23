using HarmonyLib;
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NuclearOption.UIStyleSystem;
using NuclearOption.Networking;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI;

[HarmonyPatch(typeof(MainMenu), "Start")]
public class UIAdjustmentsPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[UIA] UI Adjustments plugin starting !");

            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnDynamicMapAwake));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnDynamicMapLoadMapImage));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnObjectiveMarkerManagerUpdateObjectiveMarkers));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnHUDBombingStateUpdateWeaponDisplay));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnHUDMissileStateUpdateWeaponDisplay));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnHUDLaserGuidedStateUpdateWeaponDisplay));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnFlightHudEnableCanvas));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnThreatListSetAircraft));

            if (Plugin.UIAdjustments.MapIconColorFix.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.UnitMapIconColorFix.OnUnitMapIconOnSelectIcon));
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.UnitMapIconColorFix.OnUnitMapIconOnDeselectIcon));
            }
            if (Plugin.UIAdjustments.StickyRearmerDisplayFix.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.StickyRearmerDisplayFix.OnRearmerDisplayUpdate));
            }
            if (Plugin.UIAdjustments.MultipleRearmerDisplaysFix.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.MultipleRearmerDisplaysFix.OnHUDUnitMarkerDeselectMarker));
            }
            if (Plugin.UIAdjustments.HUDUnitMarkerSelectionFix.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.MultipleSelectDeselectMarkerFix.OnHUDUnitMarkerSelectMarker));
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.MultipleSelectDeselectMarkerFix.OnHUDUnitMarkerDeselectMarker));
            }
            if (Plugin.UIAdjustments.StaleTargetFix.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.StaleTargetFix.OnCombatHUDSetAircraft));
            }
            if (Plugin.UIAdjustments.TargetDeselectionSoundFix.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.TargetDeselectionSoundFix.OnTargetListSelectorCheckAllExclusions));
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.TargetDeselectionSoundFix.OnSoundManagerPlayInterfaceOneShot));
            }
            if (Plugin.UIAdjustments.HUDCargoStateHUDFixedUpdateFix.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.HUDCargoStateHUDFixedUpdateFix.OnHUDCargoStateHUDFixedUpdate));
            }
            if (Plugin.UIAdjustments.DisableAllyInfo.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.DisableAllyInfo.OnAllyInfoUpdateNearestAlly));
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.DisableAllyInfo.OnAllyInfoUpdateAllyInfoOnHover));
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.DisableAllyInfo.OnAllyInfoLateUpdate));
            }
            if (Plugin.UIAdjustments.ColorizePlayerRelatedMessages.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.ColorizePlayerRelatedMessages.On_MessageManager_UserCode_RpcKillMessage_635947223));
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.ColorizePlayerRelatedMessages.On_MessageManager_UserCode_RpcBombFailMessage__002D1758898816));
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.ColorizePlayerRelatedMessages.On_MessageManager_AircraftDeployedMessage));
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.ColorizePlayerRelatedMessages.On_MessageManager_UserCode_RpcRepairMessage_1444052622));
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.ColorizePlayerRelatedMessages.On_MessageManager_UserCode_RpcPilotCaptureMessage_1554742742));
            }
            if (Plugin.UIAdjustments.CenterOnJumpMap.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.CenterOnJumpMap.OnDynamicMapMapControls));
            }
            if (Plugin.UIAdjustments.AirbaseOverlay.AlwaysDisplayGlidepath.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.AirbaseOverlayPatches.AlwaysDisplayGlidepath.OnAirbaseOverlayUpdateNearestAirbase));
            }
            if (Plugin.UIAdjustments.AirbaseOverlay.IgnoreRunwayLimits.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.AirbaseOverlayPatches.IgnoreRunwayLimits.OnAirbaseOverlayUpdateNearestAirbase));
            }
            if (Plugin.UIAdjustments.NOAutopilotGCASChevron.Value == true) {
                Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.NOAutopilotGCASChevron.OnFlightHudUpdate));
            }


            var bindings = new List<BindingHelper.Binding> {
                new (typeof(UIAdjustmentsComponent), "TargetMarkerFontSize", Plugin.UIAdjustments.TargetMarkerFontSize),
                new (typeof(UIAdjustmentsComponent), "ToolTipFontSize", Plugin.UIAdjustments.ToolTipFontSize),
                new (typeof(UIAdjustmentsComponent), "ObjectiveMarkerFontSize", Plugin.UIAdjustments.ObjectiveMarkerFontSize),
                new (typeof(UIAdjustmentsComponent), "GridLabelsFontSize", Plugin.UIAdjustments.GridLabelsFontSize),
                new (typeof(UIAdjustmentsComponent), "BombingStateFontSize", Plugin.UIAdjustments.BombingStateFontSize),
                new (typeof(UIAdjustmentsComponent), "MissileStateFontSize", Plugin.UIAdjustments.MissileStateFontSize),
                new (typeof(UIAdjustmentsComponent), "LaserGuidedStateFontSize", Plugin.UIAdjustments.LaserGuidedStateFontSize),
                new (typeof(UIAdjustmentsComponent), "WingAngleGaugeFontSize", Plugin.UIAdjustments.WingAngleGaugeFontSize),
                new (typeof(UIAdjustmentsComponent), "NozzleGaugeFontSize", Plugin.UIAdjustments.NozzleGaugeFontSize),
                new (typeof(UIAdjustmentsComponent), "NotchIndicatorLabelFontSize", Plugin.UIAdjustments.NotchIndicatorLabelFontSize),
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log($"[UIA] UI Adjustments plugin started !");
        }
    }
}

class UIAdjustmentsComponent {
    //Properties
    public static int TargetMarkerFontSize {
        set {
            field = value;
            SetupTargetMarkersFonts();
        }
        get;
    }

    public static int ToolTipFontSize {
        set {
            field = value;
            SetupToolTipFonts();
        }
        get;
    }

    public static int ObjectiveMarkerFontSize {
        set {
            field = value;
            SetupObjectiveMarkersFonts();
        }
        get;
    }

    public static int GridLabelsFontSize {
        set {
            field = value;
            SetupGridLabelsFonts();
        }
        get;
    }

    public static int BombingStateFontSize {
        set {
            field = value;
            SetupBombingStateFonts();
        }
        get;
    }

    public static int MissileStateFontSize {
        set {
            field = value;
            SetupMissileStateFonts();
        }
        get;
    }

    public static int LaserGuidedStateFontSize {
        set {
            field = value;
            SetupLaserGuidedStateFonts();
        }
        get;
    }

    public static int WingAngleGaugeFontSize {
        set {
            field = value;
            SetupWingAngleGaugeFont();
        }
        get;
    }

    public static int NozzleGaugeFontSize {
        set {
            field = value;
            SetupNozzleGaugeFont();
        }
        get;
    }

    public static int NotchIndicatorLabelFontSize {
        set {
            field = value;
            SetupNotchIndicatorLabelFont();
        }
        get;
    }

    //Workers
    //Map
    //Target Markers
    private static void SetupTargetMarkersFonts() {
        void SetupTargetMarkerFonts(TargetMarker targetMarker) {
            foreach (var info in targetMarkerInfos)
                ((Text)info.GetValue(targetMarker)).fontSize = TargetMarkerFontSize;
        }

        if (TargetMarkerFontSize == -1)
            return;

        DynamicMap dynamicMap = UIBindings.Game.GetDynamicMapComponent();
        if (dynamicMap == null)
            return;

        var targetMarker = dynamicMap.targetMarker.GetComponent<TargetMarker>();
        SetupTargetMarkerFonts(targetMarker);

        var iconLookup = (Dictionary<Unit, UnitMapIcon>)dynamicMapIconLookupInfo.GetValue(dynamicMap);
        foreach ((var unit, var icon) in iconLookup) {
            targetMarker = (TargetMarker)unitMapIconTargetMarkerInfo.GetValue(icon);
            if (targetMarker == null)
                continue;
            SetupTargetMarkerFonts(targetMarker);
        }
    }

    //NO 0.34 uses Text for TargetMarker text fields
    private static string[] targetMarkerInfoNames = new string[] {
        "infoPlayer", "infoName", "infoRange", "infoSpeed", "infoAlt", "infoHeading"
    };
    private static FieldInfo[] targetMarkerInfos = Array.ConvertAll(targetMarkerInfoNames, name => AccessTools.Field(typeof(TargetMarker), name));
    private static FieldInfo dynamicMapIconLookupInfo = AccessTools.Field(typeof(DynamicMap), "iconLookup");
    private static FieldInfo unitMapIconTargetMarkerInfo = AccessTools.Field(typeof(UnitMapIcon), "targetMarker");

    //Tooltip
    private static void SetupToolTipFonts() {
        if (ToolTipFontSize == -1)
            return;

        DynamicMap dynamicMap = UIBindings.Game.GetDynamicMapComponent();
        if (dynamicMap == null)
            return;

        var mapToolTip = (MapToolTip)dynamicMapMapToolTipInfo.GetValue(dynamicMap);

        var infoText = (Text)mapToolTipInfoTextInfo.GetValue(mapToolTip);
        infoText.fontSize = ToolTipFontSize;

        var listToolTips = (List<TooltipItem>)mapToolTipListToolTipsInfo.GetValue(mapToolTip); 
        foreach (var toolTipItem in listToolTips) {
            toolTipItem.label.fontSize = ToolTipFontSize;
            toolTipItem.value.fontSize = ToolTipFontSize;
        }
    }

    private static FieldInfo dynamicMapMapToolTipInfo = AccessTools.Field(typeof(DynamicMap), "toolTip");
    private static FieldInfo mapToolTipInfoTextInfo = AccessTools.Field(typeof(MapToolTip), "infoText");
    private static FieldInfo mapToolTipListToolTipsInfo = AccessTools.Field(typeof(MapToolTip), "listToolTips");


    [HarmonyPatch(typeof(DynamicMap), "Awake")]
    public class OnDynamicMapAwake {
        public static void Postfix() {
            SetupTargetMarkersFonts();
            SetupToolTipFonts();
        }
    }

      
    //Objective Markers
    private static void SetupObjectiveMarkersFonts() {
        if (ObjectiveMarkerFontSize == -1)
            return;
        if (objectiveMarkerManager == null)
            return;

        var markerPrefab = (ObjectiveMarker)markerPrefabInfo.GetValue(objectiveMarkerManager);
        var objName = (Text)objNameInfo.GetValue(markerPrefab);
        objName.fontSize = ObjectiveMarkerFontSize;

        var objectiveMarkers = (List<ObjectiveMarker>)objectiveMarkersInfo.GetValue(objectiveMarkerManager);
        foreach (var objectiveMarker in objectiveMarkers) {
            objName = (Text)objNameInfo.GetValue(objectiveMarker);
            objName.fontSize = ObjectiveMarkerFontSize;
        }
    }

    private static FieldInfo markerPrefabInfo = AccessTools.Field(typeof(ObjectiveMarkerManager), "markerPrefab");
    private static FieldInfo objectiveMarkersInfo = AccessTools.Field(typeof(ObjectiveMarkerManager), "objectiveMarkers");
    private static FieldInfo objNameInfo = AccessTools.Field(typeof(ObjectiveMarker), "objName");
    private static ObjectiveMarkerManager objectiveMarkerManager;

    [HarmonyPatch(typeof(ObjectiveMarkerManager), "UpdateObjectiveMarkers")]
    public class OnObjectiveMarkerManagerUpdateObjectiveMarkers {
        public static void Postfix(ObjectiveMarkerManager __instance) {
            if (objectiveMarkerManager != __instance) {
                objectiveMarkerManager = __instance;
                SetupObjectiveMarkersFonts();
            }
        }
    }

    //Grid
    private static void SetupGridLabelsFonts() {
        if (GridLabelsFontSize == -1)
            return;

        DynamicMap instance = UIBindings.Game.GetDynamicMapComponent();
        if (instance == null)
            return;

        var gridLabels = instance.gridLabels;
        ((Text)gridLabelsGridToolTipInfo.GetValue(gridLabels)).fontSize = GridLabelsFontSize;
        ((Text)gridLabelsGridAircraftInfo.GetValue(gridLabels)).fontSize = GridLabelsFontSize;
        foreach (var textArrayInfo in gridLabelsTextArrayInfos) {
            var textArray = (Text[])textArrayInfo.GetValue(gridLabels);
            foreach (var text in textArray)
                text.fontSize = GridLabelsFontSize;
        }
    }

    //Text
    private static FieldInfo gridLabelsGridToolTipInfo = AccessTools.Field(typeof(GridLabels), "gridToolTip");
    //Text
    private static FieldInfo gridLabelsGridAircraftInfo = AccessTools.Field(typeof(GridLabels), "gridAircraft");
    //Text[]
    private static string[] gridLabelsTextArraysNames = new string[] {
        "listHorizontal", "listHorizontalMinor", "listVertical", "listVerticalMinor"
    };
    private static FieldInfo[] gridLabelsTextArrayInfos = Array.ConvertAll(gridLabelsTextArraysNames, name => AccessTools.Field(typeof(GridLabels), name));

    //Method is static, no __instance available
    [HarmonyPatch(typeof(DynamicMap), "LoadMapImage")]
    public class OnDynamicMapLoadMapImage {
        public static void Postfix() {
            SetupGridLabelsFonts();
        }
    }

    //Weapon states
    //For NO 0.34 had to move setting fonts from SetHUDWeaponState() to UpdateWeaponDisplay(), because font sizes kept resetting

    private static FieldInfo weaponStateInfo = AccessTools.Field(typeof(CombatHUD), "weaponState");

    //Bombing state
    private static void SetupBombingStateFonts(HUDBombingState state = null) {
        if (state != null) {
           if (state != hudBombingState)
               hudBombingState = state;
           else
               return;
        }
        else {
            var combatHUD = UIBindings.Game.GetCombatHUDComponent();
            if (combatHUD == null || hudBombingState == null || hudBombingState != (HUDWeaponState)weaponStateInfo.GetValue(combatHUD) as HUDBombingState)
                return;
        }

        if (BombingStateFontSize == -1)
            return;

        foreach (var textInfo in hudBombingStateInfos) {
            var textMesh = (TextMeshProUGUI)textInfo.GetValue(hudBombingState);
            textMesh.fontSize = BombingStateFontSize;
        }
    }

    private static string[] hudBombingStateInfoNames = new string[] {
        "dropCountdown", "ccipFallTime", "ccrpFallTime"
    };
    private static FieldInfo[] hudBombingStateInfos = Array.ConvertAll(hudBombingStateInfoNames, name => AccessTools.Field(typeof(HUDBombingState), name));
    private static HUDBombingState hudBombingState;

    [HarmonyPatch(typeof(HUDBombingState), "UpdateWeaponDisplay")]
    public class OnHUDBombingStateUpdateWeaponDisplay {
        public static void Prefix(HUDBombingState __instance) {
            SetupBombingStateFonts(__instance);
        }
    }

    //Missile state
    private static void SetupMissileStateFonts(HUDMissileState state = null) {
        if (state != null) {
           if (state != hudMissileState)
               hudMissileState = state;
           else
               return;
        }
        else {
            var combatHUD = UIBindings.Game.GetCombatHUDComponent();
            if (combatHUD == null || hudMissileState == null || hudMissileState != (HUDWeaponState)weaponStateInfo.GetValue(combatHUD) as HUDMissileState)
                return;
        }

        if (MissileStateFontSize == -1) {
            return;
        }

        foreach (var textInfo in hudMissileStateInfos) {
            ((TextMeshProUGUI)textInfo.GetValue(hudMissileState)).fontSize = MissileStateFontSize;
        }
    }

    private static string[] hudMissileStateInfoNames = new string[] {
        "maxRangeText", "minRangeText", "noEscapeRangeText", "targetText", "hint"
    };
    private static FieldInfo[] hudMissileStateInfos = Array.ConvertAll(hudMissileStateInfoNames, name => AccessTools.Field(typeof(HUDMissileState), name));
    private static HUDMissileState hudMissileState;

    [HarmonyPatch(typeof(HUDMissileState), "UpdateWeaponDisplay")]
    public class OnHUDMissileStateUpdateWeaponDisplay {
        public static void Prefix(HUDMissileState __instance) {
            SetupMissileStateFonts(__instance);
        }
    }

    //Laser guided state
    private static void SetupLaserGuidedStateFonts(HUDLaserGuidedState state = null) {
        if (state != null) {
           if (state != hudLaserGuidedState)
               hudLaserGuidedState = state;
           else
               return;
        }
        else {
            var combatHUD = UIBindings.Game.GetCombatHUDComponent();
            if (combatHUD == null || hudLaserGuidedState == null || hudLaserGuidedState != (HUDWeaponState)weaponStateInfo.GetValue(combatHUD) as HUDLaserGuidedState)
                return;
        }

        if (LaserGuidedStateFontSize == -1)
            return;

        foreach (var textInfo in hudLaserGuidedStateInfos) {
            ((TextMeshProUGUI)textInfo.GetValue(hudLaserGuidedState)).fontSize = LaserGuidedStateFontSize;
        }
    }

    private static string[] hudLaserGuidedStateInfoNames = new string[] {
        "maxRangeText", "hint"
    };
    private static FieldInfo[] hudLaserGuidedStateInfos = Array.ConvertAll(hudLaserGuidedStateInfoNames, name => AccessTools.Field(typeof(HUDLaserGuidedState), name));
    private static HUDLaserGuidedState hudLaserGuidedState;

    [HarmonyPatch(typeof(HUDLaserGuidedState), "UpdateWeaponDisplay")]
    public class OnHUDLaserGuidedStateUpdateWeaponDisplay {
        public static void Prefix(HUDLaserGuidedState __instance) {
            SetupLaserGuidedStateFonts(__instance);
        }
    }

    //Wing angle gauge
    private static void SetupWingAngleGaugeFont() {
        if (WingAngleGaugeFontSize == -1)
            return;
        if (wingAngleGauge == null)
            return;

        ((TextMeshProUGUI)wingAngleGaugeLabelInfo.GetValue(wingAngleGauge)).fontSize = WingAngleGaugeFontSize;
    }

    private static FieldInfo wingAngleGaugeLabelInfo = AccessTools.Field(typeof(WingAngleGauge), "label");
    private static WingAngleGauge wingAngleGauge;

    //Nozzle gauge
    private static void SetupNozzleGaugeFont() {
        if (NozzleGaugeFontSize == -1)
            return;
        if (nozzleGauge == null)
            return;

        ((TextMeshProUGUI)nozzleGaugeLabelInfo.GetValue(nozzleGauge)).fontSize = NozzleGaugeFontSize;
    }

    private static FieldInfo nozzleGaugeLabelInfo = AccessTools.Field(typeof(NozzleGauge), "label");
    private static NozzleGauge nozzleGauge;

    [HarmonyPatch(typeof(FlightHud), "EnableCanvas")]
    public class OnFlightHudEnableCanvas {
        public static void Postfix() {
            var flightHUD = SceneSingleton<FlightHud>.i;
            if (flightHUD == null)
                return;
            var hudCenterTransform = flightHUD.GetHUDCenter();

            nozzleGauge = hudCenterTransform.GetComponentInChildren<NozzleGauge>(includeInactive: true);
            if (nozzleGauge != null)
                SetupNozzleGaugeFont();

            wingAngleGauge = hudCenterTransform.GetComponentInChildren<WingAngleGauge>(includeInactive: true);
            if (wingAngleGauge != null)
                SetupWingAngleGaugeFont();
        }
    }

    //Notch indicator
    private static FieldInfo threatListInfo = AccessTools.Field(typeof(CombatHUD), "threatList");
    private static FieldInfo threatItemPrefabInfo = AccessTools.Field(typeof(ThreatList), "threatItemPrefab");
    private static FieldInfo itemLookupInfo = AccessTools.Field(typeof(ThreatList), "itemLookup");
    private static FieldInfo notchIndicatorLabelInfo = AccessTools.Field(typeof(ThreatItem), "notchIndicatorLabel");

    private static void SetupNotchIndicatorLabelFont() {
        if (NotchIndicatorLabelFontSize == -1)
            return;

        var combatHUD = UIBindings.Game.GetCombatHUDComponent();
        if (combatHUD == null) {
            return;
        }
        var notchIndicatorPrefab = combatHUD.notchIndicatorPrefab;
        if (notchIndicatorPrefab == null) {
            Plugin.Log("[UIA] SetupNotchIndicatorLabelFont(): notchIndicatorPrefab is null.");
            return;
        }
        var notchIndicatorLabel = notchIndicatorPrefab.GetComponentInChildren<Text>();
        if (notchIndicatorLabel == null) {
            Plugin.Log("[UIA] SetupNotchIndicatorLabelFont(): notchIndicatorLabel is null.");
            return;
        }
        notchIndicatorLabel.fontSize = NotchIndicatorLabelFontSize;

        var threatList = (ThreatList)threatListInfo.GetValue(combatHUD);
        if (threatList == null) {
            Plugin.Log("[UIA] SetupNotchIndicatorLabelFont(): threatList is null.");
            return;
        }
        var itemLookup = (Dictionary<PersistentID, ThreatItem>)itemLookupInfo.GetValue(threatList);
        if (itemLookup == null) {
            Plugin.Log("[UIA] SetupNotchIndicatorLabelFont(): itemLookup is null.");
            return;
        }
        foreach (var threatItem in itemLookup.Values) {
            var label = (Text)notchIndicatorLabelInfo.GetValue(threatItem);
            label.fontSize = NotchIndicatorLabelFontSize;
        }
    }

    [HarmonyPatch(typeof(ThreatList), "SetAircraft")]
    public class OnThreatListSetAircraft {
        public static void Postfix() {
            SetupNotchIndicatorLabelFont();
        }
    }

    //Fixes
    //[NO 0.33.4] Fix for unit map icon color not updating after selecting and deselecting
    public class UnitMapIconColorFix {
        //Unit map icon color
        [HarmonyPatch(typeof(UnitMapIcon), "OnSelectIcon")]
        public class OnUnitMapIconOnSelectIcon {
            public static void Postfix(UnitMapIcon __instance) {
                __instance.UnitMapIcon_UpdateColor();
            }
        }

        [HarmonyPatch(typeof(UnitMapIcon), "OnDeselectIcon")]
        public class OnUnitMapIconOnDeselectIcon {
            public static void Postfix(UnitMapIcon __instance) {
                __instance.UnitMapIcon_UpdateColor();
            }
        }
    }

    //[NO 0.34] Fix for sticky RearmerDisplay info on HMD
    //Disables all children objects if marker image is disabled (i.e. if marker is off-screen)
    public class StickyRearmerDisplayFix {
        [HarmonyPatch(typeof(RearmerDisplay), "Update")]
        public class OnRearmerDisplayUpdate {
            public static void Postfix(RearmerDisplay __instance, HUDUnitMarker ___marker) {
                if (___marker == null)
                    return;
                if (!___marker.selected)
                    return;
                foreach (Transform transform in __instance.gameObject.transform)
                    transform.gameObject.SetActive(___marker.image.enabled);
            }
        }
    }

    //[NO 0.34] Fix for multiple RearmerDisplay objects on one HUDUnitMarker object
    public class MultipleRearmerDisplaysFix {
        [HarmonyPatch(typeof(HUDUnitMarker), "DeselectMarker")]
        public class OnHUDUnitMarkerDeselectMarker {
            public static void Postfix(Transform ____transform) {
                var rearmerDisplayComponent = ____transform.GetComponentInChildren<RearmerDisplay>(includeInactive: true);
                if (rearmerDisplayComponent != null)
                    UnityEngine.Object.Destroy(rearmerDisplayComponent.gameObject);
            }
        }
    }

    //[NO 0.34] Fix for multiple HUDUnitMarker selection and deselection
    //Disallows selecting already selected marker and deselecting already deselected one
    public class MultipleSelectDeselectMarkerFix {
        [HarmonyPatch(typeof(HUDUnitMarker), "SelectMarker")]
        public class OnHUDUnitMarkerSelectMarker {
            public static bool Prefix(HUDUnitMarker __instance) {
                return !__instance.selected;
            }
        }

        [HarmonyPatch(typeof(HUDUnitMarker), "DeselectMarker")]
        public class OnHUDUnitMarkerDeselectMarker {
            public static bool Prefix(HUDUnitMarker __instance) {
                return __instance.selected;
            }
        }
    }

    //[NO 0.34.2] Patch to remove selected target from target list on spawn
    public static class StaleTargetFix {
        [HarmonyPatch(typeof(CombatHUD), "SetAircraft")]
        public class OnCombatHUDSetAircraft {
            public static void Postfix() {
                var targetListSelector = SceneSingleton<TargetListSelector>.i;
                if (targetListSelector != null)
                    targetListSelector.RemoveAll();
            }
        }
    }

    /* [NO 0.34.2] Patch to fix loud sound on mass target deselection (i.e. when applying new target filter preset).
       By default game plays target deselection sound for each deselected target.
       If many targets are deselected, multiple deselection sounds pile up in one loud sound.
       This fix plays just one target deselection sound for target deselection act, no matter how many targets were deselected. */
    public static class TargetDeselectionSoundFix {
        private static bool disableInterfaceOneShot = false;
        private static TraverseCache<CombatHUD, AudioClip> deselectSoundCache = new ("deselectSound");

        [HarmonyPatch(typeof(TargetListSelector), "CheckAllExclusions")]
        public class OnTargetListSelectorCheckAllExclusions {
            public static void Prefix(ref int __state) {
                disableInterfaceOneShot = true;
                __state = GameBindings.Player.TargetList.GetTargetCount();
            }

            public static void Postfix(ref int __state) {
                disableInterfaceOneShot = false;
                int targetCount = GameBindings.Player.TargetList.GetTargetCount();
                if (targetCount < __state) {
                    var combatHUD = UIBindings.Game.GetCombatHUDComponent();
                    if (combatHUD != null) {
                        var deselectSound = deselectSoundCache.GetValue(combatHUD);
                        SoundManager.PlayInterfaceOneShot(deselectSound);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SoundManager), "PlayInterfaceOneShot")]
        public class OnSoundManagerPlayInterfaceOneShot {
            public static bool Prefix() {
                return !disableInterfaceOneShot;
            }
        }

    }

    //[NO 0.43.2] Patch to fix NRE in HUDCargoState.HUDFixedUpdate() caused by accessing currentAirbase field without first checking it for null
    public static class HUDCargoStateHUDFixedUpdateFix {
        [HarmonyPatch(typeof(HUDCargoState), "HUDFixedUpdate")]
        public static class OnHUDCargoStateHUDFixedUpdate {
            private static FieldInfo currentAirbaseInfo = AccessTools.Field(typeof(HUDCargoState), "currentAirbase");

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                var continueLabel = new Label();
                var labelTarget = new CodeInstruction(OpCodes.Nop);
                labelTarget.labels.Add(continueLabel);

                CodeInstruction[] injection = {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, currentAirbaseInfo),
                    new CodeInstruction(OpCodes.Brtrue, continueLabel),
                    new CodeInstruction(OpCodes.Ret),
                    labelTarget
                };

                foreach (var instruction in instructions) {
                    // Yield the current instruction regardless
                    yield return instruction;

                    // Check if this is the call we are looking for
                    if (instruction.opcode == OpCodes.Call &&
                        (instruction.operand?.ToString().Contains("RefreshNearestAirbase") ?? false)) {
                        // Inject the array elements immediately after the call
                        foreach (var injectInst in injection)
                            yield return injectInst;
                    }
                }
            }
        }
    }

    //[NO 0.34] Patches to disable AllyInfo
    public class DisableAllyInfo {
        [HarmonyPatch(typeof(AllyInfo), "UpdateNearestAlly")]
        public class OnAllyInfoUpdateNearestAlly {
            public static bool Prefix() {
                return false;
            }
        }

        [HarmonyPatch(typeof(AllyInfo), "UpdateAllyInfoOnHover")]
        public class OnAllyInfoUpdateAllyInfoOnHover {
            public static bool Prefix() {
                return false;
            }
        }

        [HarmonyPatch(typeof(AllyInfo), "LateUpdate")]
        public class OnAllyInfoLateUpdate {
            public static bool Prefix() {
                return false;
            }
        }
    }

    public class ColorizePlayerRelatedMessages {
        private static MethodInfo messageManagerColorFromFactionInfo = AccessTools.Method(typeof(MessageManager), "ColorFromFaction");
        private static MethodInfo colorFromUnitInfo = AccessTools.Method(typeof(ColorizePlayerRelatedMessages), nameof(ColorizePlayerRelatedMessages.ColorFromUnit));
        private static FieldInfo persistentUnitUnitInfo = AccessTools.Field(typeof(PersistentUnit), nameof(PersistentUnit.unit));
        private static MethodInfo persistentUnitGetHQInfo = AccessTools.Method(typeof(PersistentUnit), nameof(PersistentUnit.GetHQ));

        private static Color ColorFromUnit(Unit unit) {
            if (!GameManager.GetLocalPlayer<Player>(out var localPlayer)) {
                Plugin.Log("[UIA] Cannot get local player");
                return (Color)messageManagerColorFromFactionInfo.Invoke(NetworkSceneSingleton<MessageManager>.i, new object[] { unit.NetworkHQ });
            }

            var isPlayerRelated =
                (UnitRegistry.TryGetPersistentUnit(unit.persistentID, out var persistentUnit) && persistentUnit.player == localPlayer) ||
                (unit is Missile missile && UnitRegistry.TryGetPersistentUnit(missile.ownerID, out var missilePersistentUnit) && missilePersistentUnit.player == localPlayer) ||
                (unit is GroundVehicle groundVehicle && groundVehicle.Networkowner == localPlayer);

            return isPlayerRelated ?
                ThemeManager.Active.ColorTheme.AllClear :
                (Color)messageManagerColorFromFactionInfo.Invoke(NetworkSceneSingleton<MessageManager>.i, new object [] { unit.NetworkHQ });
        }

        [HarmonyPatch(typeof(MessageManager), nameof(MessageManager.UserCode_RpcKillMessage_635947223))]
        public class On_MessageManager_UserCode_RpcKillMessage_635947223 {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {

                var matcher = new CodeMatcher(instructions);

                // First replacement: GetHQ() -> ColorFromFaction() pattern
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Call, messageManagerColorFromFactionInfo)
                )
                .ThrowIfNotMatch("Could not find first ColorFromFaction call")
                .RemoveInstructions(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Pop),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Ldfld, persistentUnitUnitInfo),
                    new CodeInstruction(OpCodes.Call, colorFromUnitInfo)
                );

                // Second replacement: same pattern
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Call, messageManagerColorFromFactionInfo)
                )
                .ThrowIfNotMatch("Could not find second ColorFromFaction call")
                .RemoveInstructions(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Pop),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldfld, persistentUnitUnitInfo),
                    new CodeInstruction(OpCodes.Call, colorFromUnitInfo)
                );

                return matcher.InstructionEnumeration();
            }
        }

        [HarmonyPatch(typeof(MessageManager), "UserCode_RpcBombFailMessage_-1758898816")]
        public class On_MessageManager_UserCode_RpcBombFailMessage__002D1758898816 {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                var matcher = new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Call, messageManagerColorFromFactionInfo)
                    )
                    .ThrowIfNotMatch("Could not find ColorFromFaction call")
                    .RemoveInstructions(1)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Pop),
                        new CodeInstruction(OpCodes.Ldloc_0),
                        new CodeInstruction(OpCodes.Ldfld, persistentUnitUnitInfo),
                        new CodeInstruction(OpCodes.Call, colorFromUnitInfo)
                    );

                return matcher.InstructionEnumeration();
            }
        }

        [HarmonyPatch(typeof(MessageManager), nameof(MessageManager.AircraftDeployedMessage))]
        public class On_MessageManager_AircraftDeployedMessage {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                var matcher = new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Call, messageManagerColorFromFactionInfo)
                    )
                    .ThrowIfNotMatch("Could not find ColorFromFaction call");

                matcher.RemoveInstructions(1)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Pop),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, colorFromUnitInfo)
                    );

                return matcher.InstructionEnumeration();
            }
        }

        [HarmonyPatch(typeof(MessageManager), nameof(MessageManager.UserCode_RpcRepairMessage_1444052622))]
        public class On_MessageManager_UserCode_RpcRepairMessage_1444052622 {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                var matcher = new CodeMatcher(instructions);

                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Call, messageManagerColorFromFactionInfo)
                )
                .ThrowIfNotMatch("Could not find first ColorFromFaction call")
                .RemoveInstructions(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Pop),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Ldfld, persistentUnitUnitInfo),
                    new CodeInstruction(OpCodes.Call, colorFromUnitInfo)
                );

                return matcher.InstructionEnumeration();
            }
        }

        [HarmonyPatch(typeof(MessageManager), nameof(MessageManager.UserCode_RpcPilotCaptureMessage_1554742742))]
        public class On_MessageManager_UserCode_RpcPilotCaptureMessage_1554742742 {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                var matcher = new CodeMatcher(instructions);

                // First replacement: ldloc.2 + call ColorFromFaction
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Call, messageManagerColorFromFactionInfo)
                )
                .ThrowIfNotMatch("Could not find first ColorFromFaction call")
                .RemoveInstructions(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Pop),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Ldfld, persistentUnitUnitInfo),
                    new CodeInstruction(OpCodes.Call, colorFromUnitInfo)
                );

                // Second replacement: ldloc.1 + callvirt GetHQ + call ColorFromFaction
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Call, messageManagerColorFromFactionInfo)
                )
                .ThrowIfNotMatch("Could not find second ColorFromFaction call")
                .RemoveInstructions(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Pop),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldfld, persistentUnitUnitInfo),
                    new CodeInstruction(OpCodes.Call, colorFromUnitInfo)
                );

                return matcher.InstructionEnumeration();
            }
        }
    }

    //Patch to enable centering maximized map on pressing 'Jump Map' key when players aircraft is spawned
    public class CenterOnJumpMap {
        [HarmonyPatch(typeof(DynamicMap), "MapControls")]
        public class OnDynamicMapMapControls {
            public static void Postfix(DynamicMap __instance, Rewired.Player ___player, ref Vector2 ___positionOffset, ref Vector2 ___stationaryOffset, float ___mapDisplayFactor) {
                if (DynamicMap.mapMaximized && ___player.GetButtonDown("Jump Map") && GameManager.GetLocalAircraft(out var _) && __instance.TryGetCursorCoordinates(out var position)) {
                    ___positionOffset.x = position.x * ___mapDisplayFactor;
                    ___positionOffset.y = position.z * ___mapDisplayFactor;
                    ___stationaryOffset = Vector2.zero;
                }
            }
        }
    }

    public class AirbaseOverlayPatches {
        public class AlwaysDisplayGlidepath {
            [HarmonyPatch(typeof(AirbaseOverlay), "UpdateNearestAirbase")]
            public static class OnAirbaseOverlayUpdateNearestAirbase {
                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    var matcher = new CodeMatcher(instructions);

                    while (matcher.MatchForward(false,
                        new CodeMatch(OpCodes.Ldfld, verticalLandingInfo),
                        new CodeMatch(code => code.opcode == OpCodes.Brtrue || code.opcode == OpCodes.Brtrue_S)
                    ).IsValid) {
                        matcher.SetOpcodeAndAdvance(OpCodes.Pop);
                        matcher.RemoveInstruction();
                    }

                    return matcher.InstructionEnumeration();
                }

                private static FieldInfo verticalLandingInfo = AccessTools.Field(typeof(AircraftParameters), "verticalLanding");
            }
        }

        public class IgnoreRunwayLimits {
            [HarmonyPatch(typeof(AirbaseOverlay), "UpdateNearestAirbase")]
            public static class OnAirbaseOverlayUpdateNearestAirbase {
                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    var matcher = new CodeMatcher(instructions);

                    matcher.MatchStartForward(
                        new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(FactionHQ), "GetNearestAirbase", [typeof(Vector3), typeof(RunwayQuery)]))
                    )
                    .ThrowIfNotMatch("Could not find GetNearestAirbase call")
                    .MatchStartBackwards(
                        new CodeMatch(OpCodes.Initobj, typeof(RunwayQuery))
                    )
                    .ThrowIfNotMatch("Could not find RunwayQuery init");

                    var pos = matcher.Pos;

                    matcher.MatchStartForward(
                        new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(RunwayQuery), "MinSize"))
                    )
                    .ThrowIfNotMatch("Could not find MinSize assignment")
                    .Insert(
                        new CodeInstruction(OpCodes.Pop),
                        new CodeInstruction(OpCodes.Ldc_R4, 10.0f)
                    );

                    matcher.Advance(matcher.Pos - pos);

                    matcher.MatchStartForward(
                        new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(RunwayQuery), "LandingSpeed"))
                    )
                    .ThrowIfNotMatch("Could not find LandingSpeed assignment")
                    .Insert(
                        new CodeInstruction(OpCodes.Pop),
                        new CodeInstruction(OpCodes.Ldc_R4, 1.0f)
                    );

                    return matcher.InstructionEnumeration();
                }
            }
        }
    }

    public class NOAutopilotGCASChevron {
        [HarmonyPatch(typeof(FlightHud), "Update")]
        public class OnFlightHudUpdate {
            public static void Postfix() {
                if (noAutopilotAssembly == null)
                    return;
                var combatHUDTransform = UIBindings.Game.GetCombatHUDTransform();
                if (combatHUDTransform == null)
                    return;

                var gcasLeftObj = (GameObject)s_gcasLeftObjInfo.GetValue(null);
                if (gcasLeftObj != s_gcasLeftObj && gcasLeftObj != null) {
                    gcasLeftObj.transform.SetParent(combatHUDTransform, true);
                    gcasLeftObj.transform.localRotation = Quaternion.identity;

                    var gcasRightObj = ((GameObject)s_gcasRightObjInfo.GetValue(null));
                    if (gcasRightObj != null) {
                        gcasRightObj.transform.SetParent(combatHUDTransform, true);
                        gcasRightObj.transform.localRotation = Quaternion.identity;
                    }

                    var gcasTopObj = ((GameObject)s_gcasTopObjInfo.GetValue(null));
                    if (gcasTopObj != null) {
                        gcasTopObj.transform.SetParent(combatHUDTransform, true);
                        gcasTopObj.transform.localRotation = Quaternion.identity;
                    }

                    s_gcasLeftObj = gcasLeftObj;
                }
            }

            static OnFlightHudUpdate() {
                try {
                    noAutopilotAssembly = Assembly.Load("com.qwerty1423.NOAutopilot");
                    HUDVisualsPatchType = noAutopilotAssembly.GetType("NOAutopilot.Core.HUD.HUDVisualsPatch");
                    s_gcasLeftObjInfo = AccessTools.Field(HUDVisualsPatchType, "s_gcasLeftObj");
                    s_gcasRightObjInfo = AccessTools.Field(HUDVisualsPatchType, "s_gcasRightObj");
                    s_gcasTopObjInfo = AccessTools.Field(HUDVisualsPatchType, "s_gcasTopObj");
                }
                catch (Exception e) {
                    Plugin.Log($"[UIA] Got exception: {e}");
                    noAutopilotAssembly = null;
                }
            }
        }

        private static GameObject s_gcasLeftObj = null;

        private static Assembly noAutopilotAssembly = null;
        private static Type HUDVisualsPatchType = null;
        private static FieldInfo s_gcasLeftObjInfo = null;
        private static FieldInfo s_gcasRightObjInfo = null;
        private static FieldInfo s_gcasTopObjInfo = null;
    }
}
