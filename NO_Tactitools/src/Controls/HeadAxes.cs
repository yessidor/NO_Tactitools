using HarmonyLib;
using System;
using UnityEngine;
using Rewired;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

public class HeadAxesComponent {
    public static float PanLimit = 165f;
    public static float TiltLimit = 65f;

    public static float MinFOV = 20f;
    public static float MaxFOV = 120f;
    public static float FOVSpeed = 0.2f;

    private static float pan = 0f, currentPan = 0f;
    private static float tilt = 0f, currentTilt = 0f;
    private static float fov = MaxFOV;

    private static bool inCameraOrbitStateInputs = false;

    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class OnMainMenuStart {
        private static bool initialized = false;

        static void Postfix() {
            if (!initialized) {
                Plugin.Log($"[HA] Head Axes plugin starting !");

                Plugin.harmony.PatchAll(typeof(OnCameraCockpitStateUpdateState));
                Plugin.harmony.PatchAll(typeof(OnCameraOrbitStateInputs));
                Plugin.harmony.PatchAll(typeof(OnPlayerGetAxis));

                InputCatcher.RegisterAxisInput(
                    Plugin.HeadAxes.PanAxis,
                    onMoveRaw: (float value, float valuePrev, float delta) => { SetPan(value); }
                );
                InputCatcher.RegisterAxisInput(
                    Plugin.HeadAxes.TiltAxis,
                    onMoveRaw: (float value, float valuePrev, float delta) => { SetTilt(value); }
                );
                InputCatcher.RegisterAxisInput(
                    Plugin.HeadAxes.FOVAxis,
                    onMoveRaw: (float value, float valuePrev, float delta) => { SetFOV(value); }
                );

                BindingHelper.Binding[] bindings = new BindingHelper.Binding[] {
                    new (typeof(HeadAxesComponent), "PanLimit", Plugin.HeadAxes.PanLimit),
                    new (typeof(HeadAxesComponent), "TiltLimit", Plugin.HeadAxes.TiltLimit),
                    new (typeof(HeadAxesComponent), "MinFOV", Plugin.HeadAxes.MinFOV),
                    new (typeof(HeadAxesComponent), "MaxFOV", Plugin.HeadAxes.MaxFOV),
                    new (typeof(HeadAxesComponent), "FOVSpeed", Plugin.HeadAxes.FOVSpeed)
                };
                BindingHelper.ApplyBindings(bindings);

                initialized = true;
                Plugin.Log($"[HA] Head Axes plugin started !");
            }
        }
    }

    private static void SetPan(float value) {
        pan = MathUtils.ClampAngle(Mathf.Lerp(-PanLimit, PanLimit, Mathf.InverseLerp(-1f, 1f, value)));
    }

    private static void SetTilt(float value) {
        tilt = MathUtils.ClampAngle(Mathf.Lerp(-TiltLimit, TiltLimit, Mathf.InverseLerp(-1f, 1f, value)));
    }

    private static void SetFOV(float value) {
        fov = Mathf.Lerp(MinFOV, MaxFOV, Mathf.InverseLerp(-1f, 1f, value));
    }

    [HarmonyAfter(["yessidor.no_tactitools_plus.free_look_toggle"])]
    [HarmonyPatch(typeof(CameraCockpitState), "UpdateState")]
    public class OnCameraCockpitStateUpdateState {
        public static void Postfix(CameraStateManager cam, ref float ___panView, ref float ___tiltView, ref bool ___padLock) {
            if (!___padLock) {
                var angleSpeed = Mathf.Min(2f * Time.unscaledDeltaTime / Mathf.Max(PlayerSettings.viewSmoothing, 0.01f), 1f);
                currentPan = Mathf.Lerp(currentPan, pan, angleSpeed);
                currentTilt = Mathf.Lerp(currentTilt, tilt, angleSpeed);
                ___panView = currentPan;
                ___tiltView = currentTilt;
                cam.transform.localRotation = Quaternion.Euler(currentTilt, currentPan, 0);
                cam.mainCamera.fieldOfView = Mathf.Lerp(cam.mainCamera.fieldOfView, fov, FOVSpeed);
                cam.cockpitCamRender.fieldOfView = cam.mainCamera.fieldOfView;
            }
        }
    }

    [HarmonyAfter(["yessidor.no_tactitools_plus.free_look_toggle"])]
    [HarmonyPatch(typeof(CameraOrbitState), "Inputs")]
    public class OnCameraOrbitStateInputs {
        public static void Prefix(CameraStateManager cam, ref float ___panView, ref float ___tiltView) {
            var angleSpeed = Mathf.Min(2f * Time.unscaledDeltaTime / Mathf.Max(PlayerSettings.viewSmoothing, 0.01f), 1f);
            currentPan = Mathf.Lerp(currentPan, pan, angleSpeed);
            currentTilt = Mathf.Lerp(currentTilt, tilt, angleSpeed);
            ___panView = currentPan;
            ___tiltView = currentTilt;
            inCameraOrbitStateInputs = true;
        }

        public static void Postfix(CameraStateManager cam) {
            cam.mainCamera.fieldOfView = Mathf.Lerp(cam.mainCamera.fieldOfView, fov, 1f / (1f + 100f * cam.fovChangeInertia));
        }

        public static void Finalizer() {
            inCameraOrbitStateInputs = false;
        }
    }

    [HarmonyAfter(["yessidor.no_tactitools_plus.free_look_toggle"])]
    [HarmonyPatch(typeof(Player), "GetAxis", typeof(string))]
    public class OnPlayerGetAxis {
        public static void Postfix(ref float __result, ref string actionName) {
            if (inCameraOrbitStateInputs && (actionName == "Pan View" || actionName == "Tilt View")) {
                __result = 0.0f;
            }
        }
    }
}
