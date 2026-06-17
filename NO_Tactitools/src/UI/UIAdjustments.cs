using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
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
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnHUDBombingStateSetHUDWeaponState));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnHUDMissileStateSetHUDWeaponState));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnHUDLaserGuidedStateSetHUDWeaponState));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnWingAngleGaugeInitialize));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnNozzleGaugeInitialize));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnUnitMapIconOnSelectIcon));
            Plugin.harmony.PatchAll(typeof(UIAdjustmentsComponent.OnUnitMapIconOnDeselectIcon));

            var bindings = new BindingHelper.Binding[] {
                new (typeof(UIAdjustmentsComponent), "TargetMarkerFontSize", Plugin.UIAdjustments.TargetMarkerFontSize),
                new (typeof(UIAdjustmentsComponent), "ToolTipFontSize", Plugin.UIAdjustments.ToolTipFontSize),
                new (typeof(UIAdjustmentsComponent), "ObjectiveMarkerFontSize", Plugin.UIAdjustments.ObjectiveMarkerFontSize),
                new (typeof(UIAdjustmentsComponent), "GridLabelsFontSize", Plugin.UIAdjustments.GridLabelsFontSize),
                new (typeof(UIAdjustmentsComponent), "BombingStateFontSize", Plugin.UIAdjustments.BombingStateFontSize),
                new (typeof(UIAdjustmentsComponent), "MissileStateFontSize", Plugin.UIAdjustments.MissileStateFontSize),
                new (typeof(UIAdjustmentsComponent), "LaserGuidedStateFontSize", Plugin.UIAdjustments.LaserGuidedStateFontSize),
                new (typeof(UIAdjustmentsComponent), "WingAngleGaugeFontSize", Plugin.UIAdjustments.WingAngleGaugeFontSize),
                new (typeof(UIAdjustmentsComponent), "NozzleGaugeFontSize", Plugin.UIAdjustments.NozzleGaugeFontSize),
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

    //Workers
    //Map
    //Target Markers
    private static void SetupTargetMarkersFonts() {
        void SetupTargetMarkerFonts(TargetMarker targetMarker) {
            foreach (var info in targetMarkerInfos)
                ((Text)info.GetValue(targetMarker)).fontSize = TargetMarkerFontSize;
        }

        DynamicMap dynamicMap = SceneSingleton<DynamicMap>.i;
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

    private static string[] targetMarkerInfoNames = new string[] {
        "infoPlayer", "infoName", "infoRange", "infoSpeed", "infoAlt", "infoHeading"
    };
    private static FieldInfo[] targetMarkerInfos = Array.ConvertAll(targetMarkerInfoNames, name => AccessTools.Field(typeof(TargetMarker), name));
    private static FieldInfo dynamicMapIconLookupInfo = AccessTools.Field(typeof(DynamicMap), "iconLookup");
    private static FieldInfo unitMapIconTargetMarkerInfo = AccessTools.Field(typeof(UnitMapIcon), "targetMarker");

    //Tooltip
    private static void SetupToolTipFonts() {
        DynamicMap dynamicMap = SceneSingleton<DynamicMap>.i;
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
        DynamicMap instance = SceneSingleton<DynamicMap>.i;
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

    //Bombing state
    private static void SetupBombingStateFonts() {
        if (hudBombingState == null)
            return;

        foreach (var textInfo in hudBombingStateInfos) {
            ((Text)textInfo.GetValue(hudBombingState)).fontSize = BombingStateFontSize;
        }
    }

    private static string[] hudBombingStateInfoNames = new string[] {
        "dropCountdown", "ccipFallTime", "ccrpFallTime"
    };
    private static FieldInfo[] hudBombingStateInfos = Array.ConvertAll(hudBombingStateInfoNames, name => AccessTools.Field(typeof(HUDBombingState), name));
    private static HUDBombingState hudBombingState;

    [HarmonyPatch(typeof(HUDBombingState), "SetHUDWeaponState")]
    public class OnHUDBombingStateSetHUDWeaponState {
        public static void Postfix(HUDBombingState __instance) {
            if (hudBombingState != __instance) {
                hudBombingState = __instance;
                SetupBombingStateFonts();
            }
        }
    }

    //Missile state
    private static void SetupMissileStateFonts() {
        if (hudMissileState == null)
            return;

        foreach (var textInfo in hudMissileStateInfos) {
            ((Text)textInfo.GetValue(hudMissileState)).fontSize = MissileStateFontSize;
        }
    }

    private static string[] hudMissileStateInfoNames = new string[] {
        "maxRangeText", "minRangeText", "noEscapeRangeText", "targetText", "hint"
    };
    private static FieldInfo[] hudMissileStateInfos = Array.ConvertAll(hudMissileStateInfoNames, name => AccessTools.Field(typeof(HUDMissileState), name));
    private static HUDMissileState hudMissileState;

    [HarmonyPatch(typeof(HUDMissileState), "SetHUDWeaponState")]
    public class OnHUDMissileStateSetHUDWeaponState {
        public static void Postfix(HUDMissileState __instance) {
            hudMissileState = __instance;
            SetupMissileStateFonts();
        }
    }

    //Laser guided state
    private static void SetupLaserGuidedStateFonts() {
        if (hudLaserGuidedState == null)
            return;

        foreach (var textInfo in hudLaserGuidedStateInfos) {
            ((Text)textInfo.GetValue(hudLaserGuidedState)).fontSize = LaserGuidedStateFontSize;
        }
    }

    private static string[] hudLaserGuidedStateInfoNames = new string[] {
        "maxRangeText", "hint"
    };
    private static FieldInfo[] hudLaserGuidedStateInfos = Array.ConvertAll(hudLaserGuidedStateInfoNames, name => AccessTools.Field(typeof(HUDLaserGuidedState), name));
    private static HUDLaserGuidedState hudLaserGuidedState;

    [HarmonyPatch(typeof(HUDLaserGuidedState), "SetHUDWeaponState")]
    public class OnHUDLaserGuidedStateSetHUDWeaponState {
        public static void Postfix(HUDLaserGuidedState __instance) {
            if (hudLaserGuidedState != __instance) {
                hudLaserGuidedState = __instance;
                SetupLaserGuidedStateFonts();
            }
        }
    }

    //Wing angle gauge
    private static void SetupWingAngleGaugeFont() {
        if (wingAngleGauge == null)
            return;

        ((Text)wingAngleGaugeLabelInfo.GetValue(wingAngleGauge)).fontSize = WingAngleGaugeFontSize;
    }

    private static FieldInfo wingAngleGaugeLabelInfo = AccessTools.Field(typeof(WingAngleGauge), "label");
    private static WingAngleGauge wingAngleGauge;

    [HarmonyPatch(typeof(WingAngleGauge), "Initialize")]
    public class OnWingAngleGaugeInitialize {
        public static void Postfix(WingAngleGauge __instance) {
            if (wingAngleGauge != __instance) {
                wingAngleGauge = __instance;
                SetupWingAngleGaugeFont();
            }
        }
    }

    //Nozzle gauge
    private static void SetupNozzleGaugeFont() {
        if (nozzleGauge == null)
            return;

        ((Text)nozzleGaugeLabelInfo.GetValue(nozzleGauge)).fontSize = NozzleGaugeFontSize;
    }

    private static FieldInfo nozzleGaugeLabelInfo = AccessTools.Field(typeof(NozzleGauge), "label");
    private static NozzleGauge nozzleGauge;

    [HarmonyPatch(typeof(NozzleGauge), "Initialize")]
    public class OnNozzleGaugeInitialize {
        public static void Postfix(NozzleGauge __instance) {
            if (nozzleGauge != __instance) {
                nozzleGauge = __instance;
                SetupNozzleGaugeFont();
            }
        }
    }

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
