using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Rewired;
using NuclearOption.MissionEditorScripts;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

[HarmonyPatch(typeof(MainMenu), "Start")]
public class SensitivityFixPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[SF] Sensitivity Fix plugin starting !");

            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnCameraCockpitStateUpdateState));
            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnCameraControlledStateUpdateState));
            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnCameraFreeStateUpdateState));
            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnCameraOrbitStateUpdateState));
            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnCameraSelectionStateUpdateState));
            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnCameraTVStateUpdateState));
            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnCameraEncyclopediaStateUpdateState));
            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnEditorCameraNavigatorUpdate));
            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnDynamicMapMapControls));
            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnPilotPlayerStatePlayerAxisControls));
            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnRadialMenuMainControlAxis));
            Plugin.harmony.PatchAll(typeof(SensitivityFixComponent.OnPlayerGetAxis));

            var bindings = new List<BindingHelper.Binding> {
                new (typeof(SensitivityFixComponent), "CommonSens", Plugin.SensitivityFix.CommonSens),
            };
            foreach (int i in Enum.GetValues(typeof(SensitivityFixComponent.Scope))) {
                if (i == (int)SensitivityFixComponent.Scope.None)
                    continue;
                bindings.Add(
                    new (typeof(SensitivityFixComponent), "Sensitivities", i, Plugin.SensitivityFix.Sensitivities[i])
                );
            }
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log($"[SF] Sensitivity Fix plugin started !");
        }
    }
}

//[NO 0.34.1] Patches to fix mouse sensitivity dependency on FPS (lower FPS = higher sensitivity)
class SensitivityFixComponent {
    public enum Scope { Cockpit, Controlled, Free, Orbit, Selection, TV, Encyclopedia, Navigator, MapControls, PlayerAxisControls, RadialMenu, None }
    
    public static Vector3 CommonSens = Vector3.one;
    public static Vector3[] Sensitivities = new Vector3 [Enum.GetValues(typeof(Scope)).Length - 1];

    private static Scope scope = Scope.None;

    [HarmonyPatch(typeof(CameraCockpitState), "UpdateState")]
    public class OnCameraCockpitStateUpdateState {
        public static void Prefix() {
            scope = Scope.Cockpit;
        }

        public static void Finalizer() {
            scope = Scope.None;
        }
    }

    [HarmonyPatch(typeof(CameraControlledState), "UpdateState")]
    public class OnCameraControlledStateUpdateState {
        public static void Prefix() {
            scope = Scope.Controlled;
        }

        public static void Finalizer() {
            scope = Scope.None;
        }
    }

    [HarmonyPatch(typeof(CameraFreeState), "UpdateState")]
    public class OnCameraFreeStateUpdateState {
        public static void Prefix() {
            scope = Scope.Free;
        }

        public static void Finalizer() {
            scope = Scope.None;
        }
    }

    [HarmonyPatch(typeof(CameraOrbitState), "UpdateState")]
    public class OnCameraOrbitStateUpdateState {
        public static void Prefix() {
            scope = Scope.Orbit;
        }

        public static void Finalizer() {
            scope = Scope.None;
        }
    }

    [HarmonyPatch(typeof(CameraSelectionState), "UpdateState")]
    public class OnCameraSelectionStateUpdateState {
        public static void Prefix() {
            scope = Scope.Selection;
        }

        public static void Finalizer() {
            scope = Scope.None;
        }
    }

    [HarmonyPatch(typeof(CameraTVState), "UpdateState")]
    public class OnCameraTVStateUpdateState {
        public static void Prefix() {
            scope = Scope.TV;
        }

        public static void Finalizer() {
            scope = Scope.None;
        }
    }

    [HarmonyPatch(typeof(CameraEncyclopediaState), "UpdateState")]
    public class OnCameraEncyclopediaStateUpdateState {
        public static void Prefix() {
            scope = Scope.Encyclopedia;
        }

        public static void Finalizer() {
            scope = Scope.None;
        }
    }

    [HarmonyPatch(typeof(EditorCameraNavigator), "Update")]
    public class OnEditorCameraNavigatorUpdate {
        public static void Prefix() {
            scope = Scope.Navigator;
        }

        public static void Finalizer() {
            scope = Scope.None;
        }
    }

    [HarmonyPatch(typeof(DynamicMap), "MapControls")]
    public class OnDynamicMapMapControls {
        public static void Prefix() {
            scope = Scope.MapControls;
        }

        public static void Finalizer() {
            scope = Scope.None;
        }
    }

    [HarmonyPatch(typeof(PilotPlayerState), "PlayerAxisControls")]
    public class OnPilotPlayerStatePlayerAxisControls {
        public static void Prefix() {
            scope = Scope.PlayerAxisControls;
        }

        public static void Finalizer() {
            scope = Scope.None;
        }
    }

    [HarmonyPatch(typeof(RadialMenuMain), "ControlAxis")]
    public class OnRadialMenuMainControlAxis {
        public static void Prefix() {
            scope = Scope.RadialMenu;
        }

        public static void Finalizer() {
            scope = Scope.None;
        }
    }

    private enum Action { Pan, Tilt, Zoom, Other }

    [HarmonyBefore(["yessidor.no_tactitools_plus.key_axes"])]
    [HarmonyPatch(typeof(Player), "GetAxis", typeof(string))]
    public class OnPlayerGetAxis {
        public static void Postfix(Player __instance, ref float __result, ref string actionName) {
            Action action = Action.Other;
            float sens = 0f;

            if (scope != Scope.None) {
                var sensVec = Sensitivities[(int)scope];
                switch (actionName) {
                    case "Pan View":
                        action = Action.Pan;
                        sens = sensVec.x * CommonSens.x;
                        break;
                    case "Tilt View":
                        action = Action.Tilt;
                        sens = sensVec.y * CommonSens.y;
                        break;
                    case "Zoom View":
                    case "FOV":
                        action = Action.Zoom;
                        sens = sensVec.z * CommonSens.z;
                        break;
                    default:
                        return;
                }
            }

            if (sens == 0f)
                return;
            else
                __result *= sens * 0.01f;

            if (action != Action.Other && __instance.GetAxisCoordinateMode(actionName) == AxisCoordinateMode.Relative) {
                if (action == Action.Pan || action == Action.Tilt) {
                    switch (scope) {
                        case Scope.Cockpit:
                        case Scope.Orbit:
                        case Scope.Selection:
                        case Scope.TV:
                            __result /= Time.unscaledDeltaTime;
                            break;
                        case Scope.Encyclopedia:
                            __result /= Time.deltaTime;
                            break;
                        case Scope.MapControls:
                            __result /= Mathf.Min(Time.unscaledDeltaTime, 0.03f);
                            break;
                        case Scope.PlayerAxisControls:
                            __result /= Mathf.Min(Time.unscaledDeltaTime, 0.1f);
                            break;
                        default:
                            break;
                    }

                }
                else if (action == Action.Zoom) {
                    switch (scope) {
                        case Scope.Orbit:
                            __result /= Time.unscaledDeltaTime;
                            break;
                        default:
                            break;
                    }
                }
                //other axes input is unchanged
            }
        }
    }
}
