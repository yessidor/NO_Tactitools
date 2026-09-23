using HarmonyLib;
using System;
using System.Reflection;
using Rewired;
using UnityEngine;
using NuclearOption.UI;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

[HarmonyPatch(typeof(MainMenu), "Start")]
public class FreeLookTogglePlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[FLT] FreeLook Toggle plugin starting !");

            var harmony = new Harmony("yessidor.no_tactitools_plus.free_look_toggle");
            harmony.PatchAll(typeof(FreeLookToggleComponent.OnPlayerGetButton));
            harmony.PatchAll(typeof(FreeLookToggleComponent.OnPlayerGetButtonDown));
            harmony.PatchAll(typeof(FreeLookToggleComponent.OnPlayerGetAxis));
            harmony.PatchAll(typeof(FreeLookToggleComponent.OnCameraCockpitStateUpdateState));
            harmony.PatchAll(typeof(FreeLookToggleComponent.OnCameraOrbitStateUpdateState));
            harmony.PatchAll(typeof(FreeLookToggleComponent.OnCameraStateManagerLateUpdate));
            harmony.PatchAll(typeof(FreeLookToggleComponent.OnDynamicMapMapControls));
            harmony.PatchAll(typeof(FreeLookToggleComponent.OnPilotPlayerStatePlayerAxisControls));

            InputCatcher.RegisterButtonInput(
                Plugin.FreeLookToggle.CenterKey,
                0.001f,
                onPress: () => { FreeLookToggleComponent.centerKeyEvent = FreeLookToggleComponent.KeyEvent.Press; }
                );

            InputCatcher.RegisterButtonInput(
                Plugin.FreeLookToggle.PadlockKey,
                0.001f,
                onPress: () => { FreeLookToggleComponent.padLockKeyEvent = FreeLookToggleComponent.KeyEvent.Press; }
                );
            InputCatcher.RegisterButtonInput(
                Plugin.FreeLookToggle.FreeLookKey,
                Plugin.PressDelay.Value,
                onReleased: () => { FreeLookToggleComponent.freeLookKeyEvent = FreeLookToggleComponent.KeyEvent.Release; },
                onLongPress: () => { FreeLookToggleComponent.freeLookKeyEvent = FreeLookToggleComponent.KeyEvent.LongPress; }
                );

            BindingHelper.Binding[] bindings = new BindingHelper.Binding[] {
                new (typeof(FreeLookToggleComponent), "Report", Plugin.FreeLookToggle.Report),
                new (typeof(FreeLookToggleComponent), "DisableFreeLookInPadlock", Plugin.FreeLookToggle.DisableFreeLookInPadlock),
                new (typeof(FreeLookToggleComponent), "DisableFreeLookInForwardlock", Plugin.FreeLookToggle.DisableFreeLookInForwardlock),
                new (typeof(FreeLookToggleComponent), "DisableFreeLookOnCenter", Plugin.FreeLookToggle.DisableFreeLookOnCenter),
                new (typeof(FreeLookToggleComponent), "FOVDependentSens", Plugin.FreeLookToggle.FOVDependentSens),
                new (typeof(FreeLookToggleComponent), "CenteringPositionMultiplier", Plugin.FreeLookToggle.CenteringPositionMultiplier)
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log($"[FLT] FreeLook Toggle plugin started !");
        }
    }
}

class FreeLookToggleComponent {
    public static bool Report { get; set; } = true;
    public static bool DisableFreeLookInPadlock { get; set; } = true;
    public static bool DisableFreeLookInForwardlock { get; set; } = true;
    public static bool DisableFreeLookOnCenter { get; set; } = true;
    public static bool FOVDependentSens { get; set; } = true;
    public static Vector3 CenteringPositionMultiplier { get; set; } = Vector3.zero;

    private static bool inCameraCockpitStateUpdateState = false;
    private static bool inCameraOrbitStateUpdateState = false;
    private static bool inDynamicMapMapControls = false;
    private static bool inPilotPlayerStatePlayerAxisControls = false;

    public enum KeyEvent { None, Press, Release, LongPress };
    public static KeyEvent centerKeyEvent = KeyEvent.None;
    public static KeyEvent padLockKeyEvent = KeyEvent.None;
    public static KeyEvent freeLookKeyEvent = KeyEvent.None;

    public enum Modes { Locked, FreeLook, PadLock, ForwardLock };
    public static Modes Mode { get; private set; } = Modes.Locked;

    private static Vector3 position = new ();
    private static Quaternion rotation = new ();

    private class PadLockState {
        public Vector3 position;
        public Quaternion rotation;
        public bool ignoreCenterUp;
        public Modes mode;
    };
    private static PadLockState padLockState = new ();

    private class ForwardLockState {
        public Vector3 position;
        public Quaternion rotation;
        public Modes mode;
    };
    private static ForwardLockState forwardLockState = new ();

    private static TraverseCache<CameraCockpitState, bool> padLockCache = new ("padLock");
    private static TraverseCache<CameraCockpitState, float> tiltViewCache = new ("tiltView");
    private static TraverseCache<CameraCockpitState, float> panViewCache = new ("panView");

    private static bool ShouldProcess() {
        return !(DynamicMap.mapMaximized || GameBindings.GameState.IsChatboxActive() || GameBindings.GameState.IsGamePaused() || LeaderboardMenu.IsOpen());
    }

    [HarmonyPatch(typeof(Player), "GetButton", typeof(string))]
    public class OnPlayerGetButton {
        public static void Postfix(ref bool __result, ref string actionName, Player __instance) {
            if (actionName == "Free Look") {
                //Returning true when Player.GetButton("Free Look") is called from inside of CameraCockpitState.UpdateState()
                //avoids setting CameraCockpitState.panView and .tiltView to 0
                //(it will be done by Player.GetAxis() patch)
                //Returning false when called from inside of PilotPlayerState.PlayerAxisControls() and PlayerSettings.useTrackIR is true
                //This makes virtual joystick work regardless of mode
                __result = inCameraCockpitStateUpdateState ? true : inPilotPlayerStatePlayerAxisControls && PlayerSettings.useTrackIR ? false : Mode == Modes.FreeLook;
            }
        }
    }

    [HarmonyPatch(typeof(Player), "GetButtonDown", typeof(string))]
    public class OnPlayerGetButtonDown {
        public static void Postfix(ref bool __result, ref string actionName) {
            if (inCameraCockpitStateUpdateState && actionName == "Center")
                __result = false;
        }
    }

    [HarmonyPatch(typeof(Player), "GetAxis", typeof(string))]
    public class OnPlayerGetAxis {
        public static void Postfix(ref float __result, ref string actionName) {
            if (actionName == "Pan View" || actionName == "Tilt View") {
                if (inDynamicMapMapControls)
                { /* noop */ }
                else if (ShouldProcess()) {
                    if (inCameraCockpitStateUpdateState || inCameraOrbitStateUpdateState) {
                        //since GetButton("FreeLook") always returns true when in CameraCockpitState.UpdateState() to avoid resetting view to center,
                        //handling "Pan View" and "Tilt View" axes output here based on freeLook
                        if (Mode == Modes.FreeLook && !RadialMenuMain.IsInUse()) {
                            if (FOVDependentSens)
                                __result *= UIBindings.Game.GetCameraStateManager().mainCamera.fieldOfView / PlayerSettings.defaultFoV;
                        }
                        else
                            __result = 0.0f;
                    }
                    //"Pan View" and "Tilt View" axes in other camera states and in PilotPlayerState.PlayerAxisControls() is unchanged
                    //Other camera states (except CameraOrbitState) process these axes if GetButton("FreeLook") returns true
                    //PilotPlayerState.PlayerAxisControls() processes these axes if GetButton("FreeLook") returns false

                }
                else
                    __result = 0.0f;
            }
            //other axes input is unchanged
        }
    }

    [HarmonyPatch(typeof(CameraCockpitState), "UpdateState")]
    public class OnCameraCockpitStateUpdateState {
        public static void Prefix(ref bool __state) {
            inCameraCockpitStateUpdateState = true;
            __state = PlayerSettings.useTrackIR;
            PlayerSettings.useTrackIR = false;
        }

        public static void Postfix(CameraStateManager cam, CameraCockpitState __instance, ref bool __state) {
            PlayerSettings.useTrackIR = __state;
            inCameraCockpitStateUpdateState = false;

            if (!PlayerSettings.useTrackIR && !ShouldProcess())
                return;

            var cameraCockpitState = __instance;
            var player = GameManager.playerInput;
            bool hasTargets = GameBindings.Player.TargetList.GetTargetCount() > 0;
            Modes oldMode = Mode;

            void SetLocalPositionAndRotation(Vector3 position, Quaternion rotation) {
                cam.transform.localPosition = position;
                cam.transform.localRotation = rotation;
                var panView = MathUtils.ClampAngle(rotation.eulerAngles.y);
                var tiltView = MathUtils.ClampAngle(rotation.eulerAngles.x);
                panViewCache.SetValue(cameraCockpitState, panView);
                tiltViewCache.SetValue(cameraCockpitState, tiltView);
            }

            if (player.GetButtonTimedPressDown("Center", PlayerSettings.pressDelay) || centerKeyEvent == KeyEvent.Press) {
                Plugin.Log("[FLT] UpdateState: disabling free look and pad lock, setting view to forward");
                if (PlayerSettings.padLockTarget)
                    padLockCache.SetValue(cameraCockpitState, false);
                position.Scale(CenteringPositionMultiplier);
                rotation = Quaternion.Euler(0, 0, 0);
                SetLocalPositionAndRotation(position, rotation);
                if (centerKeyEvent == KeyEvent.None)
                    padLockState.ignoreCenterUp = true;
                Mode = DisableFreeLookOnCenter ? Modes.Locked : Modes.FreeLook;
                centerKeyEvent = KeyEvent.None;
            }
            else if (player.GetButtonUp("Center") || padLockKeyEvent == KeyEvent.Press) {
                Plugin.Log("[FLT] UpdateState: toggling pad lock");
                if (padLockState.ignoreCenterUp && padLockKeyEvent == KeyEvent.None)
                    padLockState.ignoreCenterUp = false;
                else if (PlayerSettings.padLockTarget && hasTargets && GameBindings.Player.Aircraft.GetAircraft(silent: true) != null) {
                    if (Mode != Modes.PadLock) {
                        Plugin.Log("[FLT] UpdateState: enabling padlock");
                        padLockState.position = cam.transform.localPosition;
                        padLockState.rotation = cam.transform.localRotation;
                        padLockState.mode = DisableFreeLookInPadlock && Mode == Modes.FreeLook ? Modes.Locked : Mode;
                        padLockCache.SetValue(cameraCockpitState, true);
                        Mode = Modes.PadLock;
                    }
                    else {
                        Plugin.Log($"[FLT] UpdateState: disabling padlock and setting mode to {padLockState.mode}");
                        SetLocalPositionAndRotation(padLockState.position, padLockState.rotation);
                        padLockCache.SetValue(cameraCockpitState, false);
                        Mode = padLockState.mode;
                    }
                }
                padLockKeyEvent = KeyEvent.None;
            }
            else if (player.GetButtonTimedPressDown("Free Look", PlayerSettings.pressDelay) || freeLookKeyEvent == KeyEvent.LongPress) {
                Plugin.Log("[FLT] UpdateState: entering forward lock");
                forwardLockState.position = cam.transform.localPosition;
                forwardLockState.rotation = cam.transform.localRotation;
                forwardLockState.mode = DisableFreeLookInForwardlock && Mode == Modes.FreeLook ? Modes.Locked : Mode;
                if (PlayerSettings.padLockTarget)
                    padLockCache.SetValue(cameraCockpitState, false);
                var pos = Vector3.Scale(position, CenteringPositionMultiplier);
                SetLocalPositionAndRotation(pos, Quaternion.Euler(0, 0, 0));
                Mode = Modes.ForwardLock;
                freeLookKeyEvent = KeyEvent.None;
            }
            else if (player.GetButtonUp("Free Look") || freeLookKeyEvent == KeyEvent.Release) {
                if (Mode == Modes.ForwardLock) {
                    Plugin.Log($"[FLT] UpdateState: leaving forward lock, restoring view and setting mode to {forwardLockState.mode}");
                    if (PlayerSettings.padLockTarget)
                        padLockCache.SetValue(cameraCockpitState, forwardLockState.mode == Modes.PadLock);
                    SetLocalPositionAndRotation(forwardLockState.position, forwardLockState.rotation);
                    Mode = forwardLockState.mode;
                }
                else {
                    Plugin.Log("[FLT] UpdateState: toggle FreeLook");
                    Mode = Mode == Modes.Locked ? Modes.FreeLook : Modes.Locked;
                }
                freeLookKeyEvent = KeyEvent.None;
            }

            if (Mode == Modes.PadLock && !hasTargets) {
                Plugin.Log($"[FLT] UpdateState: no targets, disabling padlock, restoring view and setting mode to {padLockState.mode}");
                if (PlayerSettings.padLockTarget)
                    padLockCache.SetValue(cameraCockpitState, false);
                SetLocalPositionAndRotation(padLockState.position, padLockState.rotation);
                Mode = padLockState.mode;
            }

            if (PlayerSettings.useTrackIR && !(Mode == Modes.PadLock || Mode == Modes.ForwardLock)) {
                (var tirLocalPosition, var tirLocalRotation) = HeadTrackerManager.GetOffset(cam.transform.localPosition, cam.transform.localRotation);

                tirLocalPosition.x = Mathf.Clamp(tirLocalPosition.x, -0.25f, 0.25f);
                tirLocalPosition.y = Mathf.Clamp(tirLocalPosition.y, -0.15f, 0.15f);
                tirLocalPosition.z = Mathf.Clamp(tirLocalPosition.z, -0.1f, 0.45f);

                SetLocalPositionAndRotation(tirLocalPosition, tirLocalRotation);
            }

            if (Mode == Modes.FreeLook) {
                position = cam.transform.localPosition;
                rotation = cam.transform.localRotation;
            }
            else if (Mode == Modes.Locked) {
                SetLocalPositionAndRotation(position, rotation);
            }

            if (Report && Mode != oldMode)
                UIBindings.Game.DisplayToast($"Camera mode: <b>{Mode}</b>", 3f);
        }

        /* Postfix() may depend on inCameraCockpitStateUpdateState being false, so it is set at beginning of Postfix().
           Setting it once again in Finalizer() in case exception is thrown from wrapped function or Postfix(). */
        public static void Finalizer() {
            inCameraCockpitStateUpdateState = false;
        }
    }

    [HarmonyPatch(typeof(CameraOrbitState), "UpdateState")]
    public class OnCameraOrbitStateUpdateState {
        public static void Prefix() {
            inCameraOrbitStateUpdateState = true;
        }

        public static void Finalizer() {
            inCameraOrbitStateUpdateState = false;
        }
    }

    [HarmonyPatch(typeof(CameraStateManager), "LateUpdate")]
    public class OnCameraStateManagerLateUpdate {
        public static void Postfix(CameraStateManager __instance) {
            //Toggling free look in other camera modes (except camera cockpit state mode)
            //CameraCockpitState.UpdateState() hook handles GetButtonUp("Free Look") by itself
            if (__instance.currentState != __instance.cockpitState && ShouldProcess() && GameManager.playerInput.GetButtonUp("Free Look"))
                Mode = Mode != Modes.Locked ? Modes.Locked : Modes.FreeLook;
        }
    }

    [HarmonyPatch(typeof(DynamicMap), "MapControls")]
    public class OnDynamicMapMapControls {
        public static void Prefix() {
            inDynamicMapMapControls = true;
        }

        public static void Finalizer() {
            inDynamicMapMapControls = false;
        }
    }

    [HarmonyPatch(typeof(PilotPlayerState), "PlayerAxisControls")]
    public class OnPilotPlayerStatePlayerAxisControls {
        public static void Prefix() {
            inPilotPlayerStatePlayerAxisControls = true;
        }

        public static void Finalizer() {
            inPilotPlayerStatePlayerAxisControls = false;
        }
    }
}
