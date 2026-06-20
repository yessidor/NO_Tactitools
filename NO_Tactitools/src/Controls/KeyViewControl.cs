using HarmonyLib;
using System;
using UnityEngine;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

public class KeyViewControlComponent {
    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class OnMainMenuStart {
        static void Postfix() {
            if (!initialized) {
                Plugin.Log($"[KVC] Key View Control plugin starting !");

                Plugin.harmony.PatchAll(typeof(OnCameraCockpitStateUpdateState));
                Plugin.harmony.PatchAll(typeof(OnCameraOrbitStateUpdateState));

                InputCatcher.RegisterButtonInput(
                    Plugin.KeyViewControl.PanLeftKey,
                    PlayerSettings.pressDelay,
                    onReleased: () => { pan = pan == -2 ? 0 : -1; },
                    onLongPress: () => { pan = -2; }
                    );
                InputCatcher.RegisterButtonInput(
                    Plugin.KeyViewControl.PanRightKey,
                    PlayerSettings.pressDelay,
                    onReleased: () => { pan = pan == 2 ? 0 : 1; },
                    onLongPress: () => { pan = 2; }
                    );
                InputCatcher.RegisterButtonInput(
                    Plugin.KeyViewControl.TiltUpKey,
                    PlayerSettings.pressDelay,
                    onReleased: () => { tilt = tilt == -2 ? 0 : -1; },
                    onLongPress: () => { tilt = -2; }
                    );
                InputCatcher.RegisterButtonInput(
                    Plugin.KeyViewControl.TiltDownKey,
                    PlayerSettings.pressDelay,
                    onReleased: () => { tilt = tilt == 2 ? 0 : 1; },
                    onLongPress: () => { tilt = 2; }
                    );

                BindingHelper.Binding[] bindings = new BindingHelper.Binding[] {
                    new (typeof(KeyViewControlComponent), "FOVDependent", Plugin.KeyViewControl.FOVDependent),
                    new (typeof(KeyViewControlComponent), "StopAt0", Plugin.KeyViewControl.StopAt0),
                    new (typeof(KeyViewControlComponent), "PanStep", Plugin.KeyViewControl.PanStep),
                    new (typeof(KeyViewControlComponent), "PanStep", Plugin.KeyViewControl.PanStep),
                    new (typeof(KeyViewControlComponent), "TiltStep", Plugin.KeyViewControl.TiltStep),
                    new (typeof(KeyViewControlComponent), "PanSpeed", Plugin.KeyViewControl.PanSpeed),
                    new (typeof(KeyViewControlComponent), "TiltSpeed", Plugin.KeyViewControl.TiltSpeed)
                };
                BindingHelper.ApplyBindings(bindings);

                initialized = true;

                Plugin.Log($"[KVC] Key View Control plugin started !");
            }
        }
    }

    public static bool FOVDependent = true;

    public static bool StopAt0 = true;

    public static float PanStep = 45;
    public static float TiltStep = 45;

    public static float PanSpeed = 45;
    public static float TiltSpeed = 45;

    private static int pan = 0;
    private static int tilt = 0;

    private static bool initialized = false;

    private static readonly float panLimit = 165f;
    private static readonly float tiltLimit = 65f;

    [HarmonyPatch(typeof(CameraCockpitState), "UpdateState")]
    public class OnCameraCockpitStateUpdateState {
        public static void Postfix(ref float ___panView, ref float ___tiltView) {
            //Requires FreeLookToggle to be Enabled, for else panView and tiltView are zeroed if free look is disabled
            if (pan != 0) {
                var fovFactor = FOVDependent ? UIBindings.Game.GetCameraStateManager().mainCamera.fieldOfView / PlayerSettings.defaultFoV : 1f;
                var ap = Math.Abs(pan);
                if (ap == 1) {
                    var sp = Mathf.Sign(___panView);
                    var n = ___panView + pan * PanStep * fovFactor;
                    ___panView = Mathf.Clamp(StopAt0 && !Mathf.Approximately(___panView, 0f) && sp != Mathf.Sign(n) ? 0 : n, -panLimit, panLimit);
                    pan = 0;
                }
                else if (ap == 2) {
                    var n = ___panView + Mathf.Sign(pan) * PanSpeed * Time.unscaledDeltaTime * fovFactor;
                    ___panView = Mathf.Clamp(n, -panLimit, panLimit);
                }
            }
            if (tilt != 0) {
                var fovFactor = FOVDependent ? UIBindings.Game.GetCameraStateManager().mainCamera.fieldOfView / PlayerSettings.defaultFoV : 1f;
                var at = Mathf.Abs(tilt);
                if (at == 1) {
                    var st = Mathf.Sign(___tiltView);
                    var n = ___tiltView + tilt * TiltStep * fovFactor;
                    ___tiltView = Mathf.Clamp(StopAt0 && !Mathf.Approximately(___tiltView, 0f) && st != Mathf.Sign(n) ? 0 : n, -tiltLimit, tiltLimit);
                    tilt = 0;
                }
                else if (at == 2) {
                    var n = ___tiltView + Mathf.Sign(tilt) * TiltSpeed * Time.unscaledDeltaTime * fovFactor;
                    ___tiltView = Mathf.Clamp(n, -tiltLimit, tiltLimit);
                }
            }
        }
    }

    [HarmonyPatch(typeof(CameraOrbitState), "UpdateState")]
    public class OnCameraOrbitStateUpdateState {
        public static void Postfix(ref float ___panView, ref float ___tiltView) {
            if (pan != 0) {
                var ap = Math.Abs(pan);
                if (ap == 1) {
                    var sp = Mathf.Sign(___panView);
                    var n = ___panView + pan * PanStep;
                    ___panView = StopAt0 && !Mathf.Approximately(___panView, 0f) && sp != Mathf.Sign(n) ? 0 : n;
                    pan = 0;
                }
                else if (ap == 2) {
                    ___panView += Mathf.Sign(pan) * PanSpeed * Time.unscaledDeltaTime;
                }
                ___panView = MathUtils.ClampAngle(___panView);
            }
            if (tilt != 0) {
                var at = Mathf.Abs(tilt);
                if (at == 1) {
                    var st = Mathf.Sign(___tiltView);
                    var n = ___tiltView + tilt * TiltStep;
                    ___tiltView = StopAt0 && !Mathf.Approximately(___tiltView, 0f) && st != Mathf.Sign(n) ? 0 : n;
                    tilt = 0;
                }
                else if (at == 2) {
                    ___tiltView += Mathf.Sign(tilt) * TiltSpeed * Time.unscaledDeltaTime;
                }
                ___tiltView = MathUtils.ClampAngle(___tiltView);
            }
        }
    }
}
