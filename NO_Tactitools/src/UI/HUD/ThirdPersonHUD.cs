using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI; //Text
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HUD;

public class ThirdPersonHUDComponent {
    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class OnMainMenuStart {
        static void Postfix() {
            if (!initialized) {
                Plugin.Log($"[TPH] Third Person HUD plugin starting !");

                Plugin.harmony.PatchAll(typeof(OnDynamicMapMinimize));
                Plugin.harmony.PatchAll(typeof(OnDynamicMapMaximize));
                Plugin.harmony.PatchAll(typeof(OnCameraCockpitStateEnterState));
                Plugin.harmony.PatchAll(typeof(OnCameraOrbitStateEnterState));
                Plugin.harmony.PatchAll(typeof(OnCameraChaseStateEnterState));
                Plugin.harmony.PatchAll(typeof(OnGameplayUIResumeGame));
                Plugin.harmony.PatchAll(typeof(OnFlightHudUpdate));
                Plugin.harmony.PatchAll(typeof(OnFuelGaugeRefresh));
                Plugin.harmony.PatchAll(typeof(OnThrottleGaugeRefresh));

                BindingHelper.Binding[] bindings = new BindingHelper.Binding[] {
                    new (typeof(ThirdPersonHUDComponent), "HUDRoll", Plugin.ThirdPersonHUD.HUDRoll),
                    new (typeof(ThirdPersonHUDComponent), "HUDBoundToScreen", Plugin.ThirdPersonHUD.HUDBoundToScreen),
                    new (typeof(ThirdPersonHUDComponent), "HUDScreenOffset", Plugin.ThirdPersonHUD.HUDScreenOffset),
                    new (typeof(ThirdPersonHUDComponent), "SetTargetDesignatorPos", Plugin.ThirdPersonHUD.SetTargetDesignatorPos),
                    new (typeof(ThirdPersonHUDComponent), "TargetDesignatorScreenOffset", Plugin.ThirdPersonHUD.TargetDesignatorScreenOffset),
                };
                BindingHelper.ApplyBindings(bindings);

                initialized = true;

                Plugin.Log($"[TPH] Third Person HUD plugin started !");
            }
        }
    }

    public static bool HUDRoll = false;
    public static bool HUDBoundToScreen = true;
    public static Vector2 HUDScreenOffset = Vector2.zero;
    public static bool SetTargetDesignatorPos = true;
    public static Vector2 TargetDesignatorScreenOffset = Vector2.zero;

    private static bool initialized = false;

    private static void UpdateCanvas() {
        var cameraMode = CameraStateManager.cameraMode;
        var cameraModeMatch = cameraMode == CameraMode.orbit || cameraMode == CameraMode.chase;

        if (SceneSingleton<CombatHUD>.i.aircraft == SceneSingleton<CameraStateManager>.i.followingUnit && cameraModeMatch) {
            FlightHud.EnableCanvas(true);
            DynamicMap.EnableCanvas(true);
        }
    }

    private static void UpdateTargetDesignator() {
        if (!SetTargetDesignatorPos)
            return;
        var pos = new Vector2 (0.5f * Screen.width, 0.5f * Screen.height);
        if (SetTargetDesignatorPos && CameraStateManager.cameraMode != CameraMode.cockpit) {
            pos += TargetDesignatorScreenOffset;
        }
        SceneSingleton<CombatHUD>.i.targetDesignator.gameObject.transform.position = pos;
    }

	[HarmonyPatch(typeof(DynamicMap), "Minimize")]
    public class OnDynamicMapMinimize {
        public static void Postfix() {
            UpdateCanvas();
        }
    }

	[HarmonyPatch(typeof(DynamicMap), "Maximize")]
    public class OnDynamicMapMaximize {
        public static void Postfix() {
            UpdateCanvas();
        }
    }

	[HarmonyPatch(typeof(CameraCockpitState), "EnterState")]
    public class OnCameraCockpitStateEnterState {
        public static void Postfix() {
            UpdateCanvas();
            UpdateTargetDesignator();
        }
    }

	[HarmonyPatch(typeof(CameraOrbitState), "EnterState")]
    public class OnCameraOrbitStateEnterState {
        public static void Postfix() {
            UpdateCanvas();
        }
    }

	[HarmonyPatch(typeof(CameraChaseState), "EnterState")]
    public class OnCameraChaseStateEnterState {
        public static void Postfix() {
            UpdateCanvas();
        }
    }

	[HarmonyPatch(typeof(GameplayUI), "ResumeGame")]
    public class OnGameplayUIResumeGame {
        public static void Postfix() {
            UpdateCanvas();
        }
    }

    private static FieldInfo flightHudPitchCompassCenterInfo = AccessTools.Field(typeof(FlightHud), "pitchCompassCenter");
    private static FieldInfo flightHudCockpitTransformInfo = AccessTools.Field(typeof(FlightHud), "cockpitTransform");

	[HarmonyPatch(typeof(FlightHud), "Update")]
    public class OnFlightHudUpdate {
        public static void Postfix(FlightHud __instance) {
            var cameraMode = CameraStateManager.cameraMode;

            if (!HUDRoll && cameraMode == CameraMode.orbit) {
                var HUDCenter = __instance.GetHUDCenter();
                var angles = HUDCenter.transform.eulerAngles;
                angles.z = 0f;
                HUDCenter.transform.eulerAngles = angles;

                var pitchCompassCenter = (GameObject)flightHudPitchCompassCenterInfo.GetValue(__instance);
                angles = pitchCompassCenter.transform.eulerAngles;
                angles.z = ((Transform)flightHudCockpitTransformInfo.GetValue(__instance)).eulerAngles.z;
                pitchCompassCenter.transform.eulerAngles = angles;
            }

            if (cameraMode == CameraMode.orbit || cameraMode == CameraMode.chase) {
                if (HUDBoundToScreen) {
                    var HUDCenter = __instance.GetHUDCenter();
                    HUDCenter.transform.position = new Vector3 (0.5f * Screen.width + HUDScreenOffset.x, 0.5f * Screen.height + HUDScreenOffset.y, 0);
                }

                UpdateTargetDesignator();
            }
        }
    }

    private static FieldInfo fuelGaugeFuelReadingInfo = AccessTools.Field(typeof(FuelGauge), "fuelReading");

	[HarmonyPatch(typeof(FuelGauge), "Refresh")]
    public class OnFuelGaugeRefresh {
        public static void Postfix(FuelGauge __instance) {
            var cameraMode = CameraStateManager.cameraMode;
            var cameraModeMatch = cameraMode == CameraMode.orbit || cameraMode == CameraMode.chase;

            if (!HUDRoll && cameraModeMatch) {
                var fuelReading = (Text)fuelGaugeFuelReadingInfo.GetValue(__instance);
                var angles = fuelReading.transform.eulerAngles;
                angles.z = 0f;
                fuelReading.transform.eulerAngles = angles;
            }
        }
    }

    private static FieldInfo throttleGaugeThrottleReadingInfo = AccessTools.Field(typeof(ThrottleGauge), "throttleReading");

	[HarmonyPatch(typeof(ThrottleGauge), "Refresh")]
    public class OnThrottleGaugeRefresh {
        public static void Postfix(ThrottleGauge __instance) {
            var cameraMode = CameraStateManager.cameraMode;
            var cameraModeMatch = cameraMode == CameraMode.orbit || cameraMode == CameraMode.chase;

            if (!HUDRoll && cameraModeMatch) {
                var throttleReading = (Text)throttleGaugeThrottleReadingInfo.GetValue(__instance);
                var angles = throttleReading.transform.eulerAngles;
                angles.z = 0f;
                throttleReading.transform.eulerAngles = angles;
            }
        }
    }
}
