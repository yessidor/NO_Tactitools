using HarmonyLib;
using System;
using System.Reflection;
using Rewired;
using UnityEngine;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

[HarmonyPatch(typeof(MainMenu), "Start")]
public class FreeLookTogglePlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[FLT] FreeLook Toggle plugin starting !");

            Plugin.harmony.PatchAll(typeof(FreeLookToggleComponent.OnPlayerGetButton));
            Plugin.harmony.PatchAll(typeof(FreeLookToggleComponent.OnPlayerGetButtonDown));
            Plugin.harmony.PatchAll(typeof(FreeLookToggleComponent.OnPlayerGetAxis));
            Plugin.harmony.PatchAll(typeof(FreeLookToggleComponent.OnCameraCockpitStateUpdateState));

            BindingHelper.Binding[] bindings = new BindingHelper.Binding[] {
                new (typeof(FreeLookToggleComponent), "Report", Plugin.FreeLookToggle.Report),
                new (typeof(FreeLookToggleComponent), "DisableFreeLookInPadlock", Plugin.FreeLookToggle.DisableFreeLookInPadlock)
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

    private static bool inCameraCockpitStateUpdateState = false;
    private static bool freeLook = false;
    private static bool padLock = false;

    private class PadLockState {
        public float panView, tiltView;
        public bool ignoreCenterUp, freeLook;
    };
    private static PadLockState padLockState = new ();

    private class TempLockState {
        public float panView, tiltView;
        public bool padLock, freeLook, enabled;
    };
    private static TempLockState tempLockState = new ();

    private static FieldInfo padLockInfo = AccessTools.Field(typeof(CameraCockpitState), "padLock");
    private static FieldInfo tiltViewInfo = AccessTools.Field(typeof(CameraCockpitState), "tiltView");
    private static FieldInfo panViewInfo = AccessTools.Field(typeof(CameraCockpitState), "panView");

    [HarmonyPatch(typeof(Player), "GetButton", typeof(string))]
    public class OnPlayerGetButton {
        public static void Postfix(ref bool __result, ref string actionName, Player __instance) {
            if (actionName == "Free Look") {
                //Returning true when Player.GetButton("Free Look") is called from inside of CameraCockpitState.UpdateState()
                //avoids setting CameraCockpitState.panView and .tiltView to 0
                //(it will be done by Player.GetAxis() patch)
                __result = inCameraCockpitStateUpdateState ? true : freeLook;
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
            if (inCameraCockpitStateUpdateState && !freeLook && (actionName == "Pan View" || actionName == "Tilt View"))
                __result = 0.0f;
        }
    }

    [HarmonyPatch(typeof(CameraCockpitState), "UpdateState")]
    public class OnCameraCockpitStateUpdateState {
        public static void Prefix() {
            inCameraCockpitStateUpdateState = true;
        }

        public static void Postfix(CameraCockpitState __instance) {
            inCameraCockpitStateUpdateState = false;

            var cameraCockpitState = __instance;
            var player = GameManager.playerInput;
            padLock = (bool)padLockInfo.GetValue(cameraCockpitState);
            bool hasTargets = SceneSingleton<CombatHUD>.i.GetTargetList().Count > 0;
            bool oldPadLock = padLock;
            bool oldFreeLook = freeLook;
            if (player.GetButtonTimedPressDown("Center", PlayerSettings.pressDelay)) {
                Plugin.Log("[FLT] UpdateState: disable FreeLook and PadLock, look forward");
                padLock = freeLook = false;
                padLockState.panView = padLockState.tiltView = 0.0f;
                if (PlayerSettings.padLockTarget)
                    padLockInfo.SetValue(cameraCockpitState, padLock);
                panViewInfo.SetValue(cameraCockpitState, padLockState.panView);
                tiltViewInfo.SetValue(cameraCockpitState, padLockState.tiltView);
                padLockState.ignoreCenterUp = true;
            }
            else if (player.GetButtonUp("Center")) {
                Plugin.Log("[FLT] UpdateState: toggle PadLock");
                if (padLockState.ignoreCenterUp)
                    padLockState.ignoreCenterUp = false;
                else if (PlayerSettings.padLockTarget && hasTargets && SceneSingleton<CombatHUD>.i.aircraft != null) {
                    padLock = !padLock;
                    padLockInfo.SetValue(cameraCockpitState, padLock);
                    if (padLock) {
                        Plugin.Log("[FLT] UpdateState: save view");
                        padLockState.panView = (float)panViewInfo.GetValue(cameraCockpitState);
                        padLockState.tiltView = (float)tiltViewInfo.GetValue(cameraCockpitState);
                        if (DisableFreeLookInPadlock) {
                            padLockState.freeLook = freeLook;
                            freeLook = false;
                        }
                    }
                    else {
                        Plugin.Log("[FLT] UpdateState: restore view");
                        panViewInfo.SetValue(cameraCockpitState, padLockState.panView);
                        tiltViewInfo.SetValue(cameraCockpitState, padLockState.tiltView);
                        if (DisableFreeLookInPadlock)
                            freeLook = padLockState.freeLook;
                    }
                }
            }
            else if (player.GetButtonTimedPressDown("Free Look", PlayerSettings.pressDelay)) {
                Plugin.Log("[FLT] UpdateState: entering temp lock");
                tempLockState.panView = (float)panViewInfo.GetValue(cameraCockpitState);
                tempLockState.tiltView = (float)tiltViewInfo.GetValue(cameraCockpitState);
                tempLockState.padLock = padLock;
                tempLockState.freeLook = freeLook;
                padLock = freeLook = false;
                if (PlayerSettings.padLockTarget)
                    padLockInfo.SetValue(cameraCockpitState, padLock);
                panViewInfo.SetValue(cameraCockpitState, 0.0f);
                tiltViewInfo.SetValue(cameraCockpitState, 0.0f);
                tempLockState.enabled = true;
            }
            else if (player.GetButtonUp("Free Look")) {
                if (tempLockState.enabled) {
                    Plugin.Log("[FLT] UpdateState: leaving temp lock");
                    padLock = tempLockState.padLock;
                    freeLook = tempLockState.freeLook;
                    if (PlayerSettings.padLockTarget)
                        padLockInfo.SetValue(cameraCockpitState, padLock);
                    panViewInfo.SetValue(cameraCockpitState, tempLockState.panView);
                    tiltViewInfo.SetValue(cameraCockpitState, tempLockState.tiltView);
                    tempLockState.enabled = false;
                }
                else {
                    Plugin.Log("[FLT] UpdateState: toggle FreeLook");
                    freeLook = !freeLook;
                }
            }

            if (padLock && !hasTargets) {
                Plugin.Log("[FLT] UpdateState: no targets, disable padLock and restore view");
                padLock = false;
                if (PlayerSettings.padLockTarget)
                    padLockInfo.SetValue(cameraCockpitState, padLock);
                panViewInfo.SetValue(cameraCockpitState, padLockState.panView);
                tiltViewInfo.SetValue(cameraCockpitState, padLockState.tiltView);
                if (DisableFreeLookInPadlock)
                    freeLook = padLockState.freeLook;
            }

            if (Report) {
                if (padLock != oldPadLock)
                    UIBindings.Game.DisplayToast(string.Format("PadLock: <b>{0}</b>", padLock ? "activated" : "deactivated"), 3f);
                if (freeLook != oldFreeLook)
                    UIBindings.Game.DisplayToast(string.Format("FreeLook: <b>{0}</b>", freeLook ? "activated" : "deactivated"), 3f);
            }

        }
    }
}
