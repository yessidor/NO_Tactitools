using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

[HarmonyPatch(typeof(MainMenu), "Start")]
class AltTargetSelectionPlugin {
    static void Postfix() {
        AltTargetSelectionComponent.Init(); 
    }
}


class AltTargetSelectionComponent {
    private static bool initialized = false;
    public static void Init() {
        if (!initialized) {
            Plugin.Log("[TS] Alternative Target Selection Plugin initializing");

            Plugin.harmony.PatchAll(typeof(OnCombatHUDTargetSelect));

            var virtualJoystickBindings = new BindingHelper.Binding[] {
                new (typeof(AltTargetSelectionComponent), "FOVFraction", Plugin.AltTargetSelection.FOVFraction)
            };
            BindingHelper.ApplyBindings(virtualJoystickBindings);

            initialized = true;
            Plugin.Log("[TS] Alternative Target Selection Plugin initialized");
        }
    }

    public static float FOVFraction { set; get; } = 0.1f;

    private static TraverseCache<CombatHUD, List<HUDUnitMarker>> markersCache = new ("markers");

    public static bool TargetSelect(ref CombatHUD __instance, ref bool paint) {
        List<HUDUnitMarker> markers = markersCache.GetValue(__instance);

        var camera = SceneSingleton<CameraStateManager>.i.mainCamera;
        var cameraTransform = camera.transform;
        var cameraPosition = cameraTransform.position.ToGlobalPosition();
        var cameraForward = cameraTransform.forward;
        var dotProductThreshold = Mathf.Cos(0.5f * Mathf.Deg2Rad * camera.fieldOfView * FOVFraction);

        Unit target = null;
        float targetDistance = float.PositiveInfinity;

        foreach (var marker in markers) {
            var unit = marker.unit;
            if (marker.selected || SceneSingleton<TargetListSelector>.i.CheckExclusions(unit))
                continue;
            if (!__instance.aircraft.NetworkHQ.TryGetKnownPosition(unit, out var unitPosition))
                continue;
            Vector3 toUnit = unitPosition - cameraPosition;
            float distance = toUnit.magnitude;
            toUnit.Normalize();
            float dotProduct = Vector3.Dot(toUnit, cameraForward);
            if (dotProduct < dotProductThreshold) {
                continue;
            }
            if (paint)
                GameBindings.Player.TargetList.AddTarget(unit);
            else if (distance < targetDistance) {
                target = unit;
                targetDistance = distance;
            }
        }

        //add target to target list if not null
        if (!paint && target != null) {
            GameBindings.Player.TargetList.AddTarget(target);
        }

        return false;
    }

    [HarmonyPatch(typeof(CombatHUD), "TargetSelect")]
    public class OnCombatHUDTargetSelect {
        static bool Prefix(ref CombatHUD __instance, ref bool paint) {
            return TargetSelect(ref __instance, ref paint);
        }
    }
}
