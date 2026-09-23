using HarmonyLib;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

[HarmonyPatch(typeof(MainMenu), "Start")]
public class MiniMapZoomPlugin {
    public static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[MMZ] MiniMap Zoom plugin starting !");

            Plugin.harmony.PatchAll(typeof(MiniMapZoomComponent.OnDynamicMapCenterMinimizedMap));
            Plugin.harmony.PatchAll(typeof(MiniMapZoomComponent.OnDynamicMapMinimize));
            Plugin.harmony.PatchAll(typeof(MiniMapZoomComponent.OnDynamicMapMaximize));

            InputCatcher.RegisterNewInput(
                Plugin.MiniMapZoom.CycleUpKey,
                Plugin.PressDelay.Value,
                onRelease: () => MiniMapZoomComponent.CycleZoom(up: true),
                onLongPress: MiniMapZoomComponent.ResetZoom);

            InputCatcher.RegisterNewInput(
                Plugin.MiniMapZoom.CycleDownKey,
                Plugin.PressDelay.Value,
                onRelease: () => MiniMapZoomComponent.CycleZoom(up: false),
                onLongPress: MiniMapZoomComponent.ResetZoom);

            var bindings = new BindingHelper.Binding[] {
                new (typeof(MiniMapZoomComponent), "ZoomsString", Plugin.MiniMapZoom.Zooms),
                new (typeof(MiniMapZoomComponent), "Offset", Plugin.MiniMapZoom.Offset),
                new (typeof(MiniMapZoomComponent), "Report", Plugin.MiniMapZoom.Report),
                new (typeof(MiniMapZoomComponent), "CenterMinimizedMapInPrefix", Plugin.MiniMapZoom.CenterMinimizedMapInPrefix),
                new (typeof(MiniMapZoomComponent), "IndependentZoomLevels", Plugin.MiniMapZoom.IndependentZoomLevels),
                new (typeof(MiniMapZoomComponent), "SaveMaximizedPosition", Plugin.MiniMapZoom.SaveMaximizedPosition)
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log($"[MMZ] MiniMap Zoom plugin started !");
        }
    }
}

public class MiniMapZoomComponent {
    public static List<float> Zooms {
         set {
              field = [.. value];
              idx = field.IndexOf(currentZoomLevel);
              if (idx == -1) idx = 0;
         }
         get;
    } = new ();
    public static string ZoomsString {
        set {
            List<float> values = new ();
            foreach (var v in value.Split(";")) {
                if (!float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) {
                    Plugin.Log(string.Format("[MMZ] Cannot parse {0} as float, skipping", v));
                    continue;
                }
                values.Add(f);
            }
            Zooms = values;
            field = value;
        }
        private get;
    }
    public static float Offset { set; get; } = 4000f;
    public static bool Report { set; get; } = true;
    public static bool CenterMinimizedMapInPrefix { set; get; } = true;
    public static bool IndependentZoomLevels = true;
    public static bool SaveMaximizedPosition = true;

    private static CombatHUD currentCombatHUD;
    private static FieldInfo OnMapChangedInfo = AccessTools.Field(typeof(DynamicMap), "onMapChanged");

    private static int idx = 0;
    private static float defaultZoomLevel = 2.0f;
    private static float currentZoomLevel = defaultZoomLevel;
    private static float minimizedZoomLevel = defaultZoomLevel;
    private static float maximizedZoomLevel = defaultZoomLevel;
    private static Vector2 positionOffset = Vector2.zero;
    private static Vector2 stationaryOffset = Vector2.zero;


    public static void CycleZoom(bool up = true) {
        if (!MiniMapZoomPlugin.initialized) {
            Plugin.Log("[MMZ] Not initialized");
            return;
        }

        if (Zooms.Count == 0)
            return;
        idx = (up ? (idx + 1) : (idx - 1 + Zooms.Count)) % Zooms.Count;
        var zoom = Zooms[idx];
        SetZoomLevel(zoom);
        if (Report)
          UIBindings.Game.DisplayToast(string.Format("Minimap zoom: <b>{0}</b>", zoom), 3f);
    }

    public static void ResetZoom() {
        if (!MiniMapZoomPlugin.initialized) {
            Plugin.Log("[MMZ] Not initialized");
            return;
        }

        var zoom = defaultZoomLevel;
        idx = Zooms.IndexOf(zoom);
        if (idx == -1) idx = 0;
        SetZoomLevel(zoom);
        if (Report)
          UIBindings.Game.DisplayToast(string.Format("Minimap zoom: <b>{0}</b>", zoom), 3f);
    }

    private static void SetZoomLevel(float zoomLevel) {
        var combatHUD = UIBindings.Game.GetCombatHUDComponent();
        if (combatHUD == null)
            return;
        var aircraft = combatHUD.aircraft;
        if (aircraft == null)
            return;
        var aircraftTransform = aircraft.transform;
        if (aircraftTransform == null)
            return;

        currentZoomLevel = zoomLevel;

        var dynamicMap = UIBindings.Game.GetDynamicMapComponent();
        var mapScaleProxy = dynamicMap.mapScaleProxy;
        var mapScaleCenter = dynamicMap.mapScaleCenter;
        var mapImage = dynamicMap.mapImage;
        var mapBackground = dynamicMap.mapBackground;

        mapScaleProxy.position = mapImage.transform.position;
        mapScaleProxy.localScale = mapScaleCenter.localScale;
        mapScaleProxy.transform.SetParent(mapScaleCenter);
        mapScaleCenter.localScale = Vector3.one * zoomLevel;
        mapScaleProxy.SetParent(mapBackground.transform);
        mapImage.transform.position = mapScaleProxy.position;
        mapImage.transform.localScale = mapScaleProxy.localScale;

        CenterMinimizedMap(ref dynamicMap);

        EventHandler onMapChangedEventHandler = OnMapChangedInfo.GetValue(null) as EventHandler;
        if (onMapChangedEventHandler != null) {
            Delegate[] subscribers = onMapChangedEventHandler.GetInvocationList();
            foreach (Delegate subscriber in subscribers)
                if (subscriber != null)
                    subscriber.DynamicInvoke(null);
        }
    }

    private static void CenterMinimizedMap(ref DynamicMap instance) {
        var combatHUD = UIBindings.Game.GetCombatHUDComponent();
        if (combatHUD == null)
            return;
        else if (combatHUD != currentCombatHUD) {
            currentCombatHUD = combatHUD;
            ResetZoom();
            minimizedZoomLevel = maximizedZoomLevel = instance.mapScaleCenter.localScale.x;
        }

        var aircraft = combatHUD.aircraft;
        if (aircraft == null)
            return;
        var aircraftTransform = aircraft.transform;
        if (aircraftTransform == null)
            return;

        var mapDisplayFactor = instance.mapDisplayFactor;
        var mapImage = instance.mapImage;
        var mapImageTransform = mapImage.transform;
        var viewIndicator = instance.viewIndicator;
        var viewIndicatorTransform = viewIndicator.transform;
        var mapBackground = instance.mapBackground;
        var cameraStateManagerTransform = UIBindings.Game.GetCameraStateManager().transform;

        //Default zoom in minimap mode = 2.0f
        float factor = defaultZoomLevel / currentZoomLevel;

        Vector3 cameraPos = cameraStateManagerTransform.position.ToGlobalPosition().AsVector3() * mapDisplayFactor;
        Vector3 forward = aircraftTransform.forward;
        forward.y = 0f;
        //Moves center of the minimap forward relative to player aircraft
        Vector3 center = cameraPos + forward.normalized * mapDisplayFactor * Offset * factor;
        mapImageTransform.eulerAngles = new Vector3(0.0f, 0.0f, aircraftTransform.eulerAngles.y);
        var localScaleMultiplier = mapImageTransform.localScale.x * mapBackground.transform.localScale.x;
        mapImageTransform.localPosition = localScaleMultiplier * (-center.x * mapImageTransform.right + -center.z * mapImageTransform.up);
        viewIndicatorTransform.eulerAngles = new Vector3(0.0f, 0.0f, mapImageTransform.eulerAngles.z - cameraStateManagerTransform.eulerAngles.y);
        viewIndicatorTransform.localPosition = new Vector3(cameraPos.x, cameraPos.z, 0.0f);
    }

    [HarmonyPatch(typeof(DynamicMap), "CenterMinimizedMap")]
    public class OnDynamicMapCenterMinimizedMap {
        public static bool Prefix(ref DynamicMap __instance) {
            if (CenterMinimizedMapInPrefix) {
                CenterMinimizedMap(ref __instance);
                return false;
            }
            else
                return true;
        }

        public static void Postfix(ref DynamicMap __instance) {
            if (!CenterMinimizedMapInPrefix)
                CenterMinimizedMap(ref __instance);
        }
    }

    [HarmonyPatch(typeof(DynamicMap), "Minimize")]
    public class OnDynamicMapMinimize {
        public static void Prefix(ref DynamicMap __instance, ref Vector2 ___positionOffset, ref Vector2 ___stationaryOffset) {
            if (IndependentZoomLevels)
                maximizedZoomLevel = __instance.mapScaleCenter.localScale.x;
            if (SaveMaximizedPosition) {
                stationaryOffset = ___stationaryOffset;
                positionOffset = ___positionOffset;
            }
        }

        public static void Postfix(ref DynamicMap __instance) {
            SetZoomLevel(IndependentZoomLevels ? minimizedZoomLevel : currentZoomLevel);
        }
    }

    [HarmonyPatch(typeof(DynamicMap), "Maximize")]
    public class OnDynamicMapMaximize {
        public static void Prefix(ref DynamicMap __instance) {
            if (IndependentZoomLevels)
                minimizedZoomLevel = __instance.mapScaleCenter.localScale.x;
        }

        public static void Postfix(ref DynamicMap __instance, ref Vector2 ___positionOffset, ref Vector2 ___stationaryOffset) {
            SetZoomLevel(IndependentZoomLevels ? maximizedZoomLevel : currentZoomLevel);
            if (SaveMaximizedPosition) {
                ___stationaryOffset = stationaryOffset;
                ___positionOffset = positionOffset;
            }
        }
    }
}
