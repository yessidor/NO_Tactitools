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
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[MMZ] MiniMap Zoom plugin starting !");

            Plugin.harmony.PatchAll(typeof(MiniMapZoomComponent.OnDynamicMapCenterMinimizedMap));
            Plugin.harmony.PatchAll(typeof(MiniMapZoomComponent.OnDynamicMapMinimize));

            InputCatcher.RegisterNewInput(
                Plugin.MiniMapZoom.CycleKey,
                PlayerSettings.pressDelay,
                onRelease: MiniMapZoomComponent.CycleZoom,
                onLongPress: MiniMapZoomComponent.ResetZoom);

            var virtualJoystickBindings = new BindingHelper.Binding[] {
                new (typeof(MiniMapZoomComponent), "ZoomsString", Plugin.MiniMapZoom.Zooms),
                new (typeof(MiniMapZoomComponent), "Offset", Plugin.MiniMapZoom.Offset),
                new (typeof(MiniMapZoomComponent), "Report", Plugin.MiniMapZoom.Report),
            };
            BindingHelper.ApplyBindings(virtualJoystickBindings);

            initialized = true;
            Plugin.Log($"[MMZ] MiniMap Zoom plugin started !");
        }
    }
}

class MiniMapZoomComponent {
    public static List<float> Zooms { set { field = [.. value]; } get; } = new ();
    public static string ZoomsString {
        set {
            List<float> values = new ();
            foreach (var v in value.Split(";")) {
                if (!float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) {
                    Plugin.Log(string.Format("[MMZ] Cannot parse {0} as float, skipping", v));
                    continue;
                }
                values.Add(f);
                Zooms = values;
                idx = Zooms.IndexOf(currentZoomLevel);
                if (idx == -1) idx = 0;
            }
            field = value;
        }
        private get;
    }
    public static float Offset { set; get; } = 4000f;
    public static bool Report { set; get; } = true;

    private static CombatHUD currentCombatHUD;
    private static int idx = 0;
    private static float minimizedZoomLevel = 2.0f;
    private static float currentZoomLevel = minimizedZoomLevel;
    private static FieldInfo OnMapChangedInfo = AccessTools.Field(typeof(DynamicMap), "onMapChanged");

    public static void CycleZoom() {
        if (Zooms.Count == 0)
            return;
        idx = (idx + 1) % Zooms.Count;
        var zoom = Zooms[idx];
        SetZoomLevel(zoom);
        if (Report)
          UIBindings.Game.DisplayToast(string.Format("Minimap zoom: <b>{0}</b>", zoom), 3f);
    }

    public static void ResetZoom() {
        var zoom = (float)minimizedZoomLevel;
        idx = Zooms.IndexOf(zoom);
        if (idx == -1) idx = 0;
        SetZoomLevel(zoom);
        if (Report)
          UIBindings.Game.DisplayToast(string.Format("Minimap zoom: <b>{0}</b>", zoom), 3f);
    }

    private static void SetZoomLevel(float zoomLevel)
    {
        var combatHUD = SceneSingleton<CombatHUD>.i;
        if (combatHUD == null)
            return;
        var aircraftTransform = combatHUD.aircraft?.transform;
        if (aircraftTransform == null)
            return;

        currentZoomLevel = zoomLevel;

        var dynamicMap = SceneSingleton<DynamicMap>.i;
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
        var combatHUD = SceneSingleton<CombatHUD>.i;
        if (combatHUD == null)
            return;
        else if (combatHUD != currentCombatHUD) {
            currentCombatHUD = combatHUD;
            ResetZoom();
        }

        var aircraftTransform = combatHUD.aircraft?.transform;
        if (aircraftTransform == null)
            return;

        var mapDisplayFactor = instance.mapDisplayFactor;
        var mapImage = instance.mapImage;
        var mapImageTransform = mapImage.transform;
        var viewIndicator = instance.viewIndicator;
        var viewIndicatorTransform = viewIndicator.transform;
        var mapBackground = instance.mapBackground;
        var cameraStateManagerTransform = SceneSingleton<CameraStateManager>.i.transform;

        //Default zoom in minimap mode = 2.0f
        float factor = (float)minimizedZoomLevel / currentZoomLevel;

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
            CenterMinimizedMap(ref __instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(DynamicMap), "Minimize")]
    public class OnDynamicMapMinimize {
        public static void Postfix(ref DynamicMap __instance) {
            SetZoomLevel(currentZoomLevel);
        }
    }
}
