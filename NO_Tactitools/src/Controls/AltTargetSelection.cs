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
                new (typeof(AltTargetSelectionComponent), "FOVFraction", Plugin.AltTargetSelection.FOVFraction),
                new (typeof(AltTargetSelectionComponent), "MaxDistance", Plugin.AltTargetSelection.MaxDistance),
                new (typeof(AltTargetSelectionComponent), "PickActive", Plugin.AltTargetSelection.PickActive)
            };
            BindingHelper.ApplyBindings(virtualJoystickBindings);

            initialized = true;
            Plugin.Log("[TS] Alternative Target Selection Plugin initialized");
        }
    }

    public static float FOVFraction { set; get; } = 0.1f;
    public static float MaxDistance { set; get; } = 0f;
    public static bool PickActive = false;

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

        Unit selectedTarget = null;
        float selectedTargetDistance = float.PositiveInfinity;

        foreach (var marker in markers) {
            var unit = marker.unit;
            if (!marker.selected && SceneSingleton<TargetListSelector>.i.CheckExclusions(unit))
                continue;
            if (!__instance.aircraft.NetworkHQ.TryGetKnownPosition(unit, out var unitPosition))
                continue;
            Vector3 toUnit = unitPosition - cameraPosition;
            float distance = toUnit.magnitude;
            toUnit.Normalize();
            float dotProduct = Vector3.Dot(toUnit, cameraForward);
            if (dotProduct < dotProductThreshold || (MaxDistance != 0f && distance > MaxDistance)) {
                continue;
            }
            if (!marker.selected) {
                if (paint)
                    GameBindings.Player.TargetList.AddTarget(unit);
                else if (distance < targetDistance) {
                    target = unit;
                    targetDistance = distance;
                }
            }
            else if (PickActive && distance < selectedTargetDistance) {
                selectedTarget = unit;
                selectedTargetDistance = distance;
            }
        }

        if (!paint) {
            if (target != null)
                GameBindings.Player.TargetList.AddTarget(target);
            else if (PickActive && selectedTarget != null) {
                GameBindings.Player.TargetList.DeselectUnit(selectedTarget);
                GameBindings.Player.TargetList.AddTarget(selectedTarget);
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
}
