using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //Text
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.MFD;

public class DynamicLandingCamComponent {
    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class OnMainMenuStart {
        static void Postfix() {
            if (!initialized) {
                Plugin.Log($"[DTC] Dynamic Target Cam plugin starting !");

                Plugin.harmony.PatchAll(typeof(OnTargetCamTargetCam_OnTouchdown));
                Plugin.harmony.PatchAll(typeof(OnTargetCamInitialize));
                Plugin.harmony.PatchAll(typeof(OnTargetCamUpdate));
                Plugin.harmony.PatchAll(typeof(OnTargetCamSetLandingCam));
                Plugin.harmony.PatchAll(typeof(OnTargetCamCancelTarget));
                Plugin.harmony.PatchAll(typeof(OnLandingScreenUILateUpdate));

                BindingHelper.Binding[] bindings = new BindingHelper.Binding[] {
                    new (typeof(DynamicLandingCamComponent), "KeepOnAfterTouchDown", Plugin.DynamicLandingCam.KeepOnAfterTouchDown),
                    new (typeof(DynamicLandingCamComponent), "Rotate", Plugin.DynamicLandingCam.Rotate),
                    new (typeof(DynamicLandingCamComponent), "RotationSpeed", Plugin.DynamicLandingCam.RotationSpeed),
                    new (typeof(DynamicLandingCamComponent), "TiltLimits", Plugin.DynamicLandingCam.TiltLimits),
                    new (typeof(DynamicLandingCamComponent), "PanLimits", Plugin.DynamicLandingCam.PanLimits),
                    new (typeof(DynamicLandingCamComponent), "InitialAngles", Plugin.DynamicLandingCam.InitialAngles),
                    new (typeof(DynamicLandingCamComponent), "LandingCamFOV", Plugin.DynamicLandingCam.LandingCamFOV),
                    new (typeof(DynamicLandingCamComponent), "Deadzone", Plugin.DynamicLandingCam.Deadzone),
                    new (typeof(DynamicLandingCamComponent), "FixBrawlerLandingCam", Plugin.DynamicLandingCam.FixBrawlerLandingCam),
                };
                BindingHelper.ApplyBindings(bindings);

                Deadzone = Deadzone;

                initialized = true;

                Plugin.Log($"[DTC] Dynamic Target Cam plugin started !");
            }
        }
    }

    public static bool KeepOnAfterTouchDown = true;
    public static bool Rotate = true;
    public static float RotationSpeed = 1f;
    public static Vector2 TiltLimits = new Vector2 (0, 90);
    public static Vector2 PanLimits = new Vector2 (-45, 45);
    public static Vector2 InitialAngles = new Vector2 (0, 0);
    public static float LandingCamFOV = 90f;
    public static float Deadzone { set { field = value; deadzoneDotProductThreshold = Mathf.Cos(0.5f * Mathf.Deg2Rad * field); } get; } = 10f;
    public static bool FixBrawlerLandingCam = true;

    private static bool initialized = false;
    private static float deadzoneDotProductThreshold = 0f;

	[HarmonyPatch(typeof(TargetCam), "TargetCam_OnTouchdown")]
    public class OnTargetCamTargetCam_OnTouchdown {
        public static bool Prefix(TargetCam __instance, ref Camera ___cam, ref TargetCam.CamMode ___currentMode, ref Aircraft ___aircraft) {
            return !KeepOnAfterTouchDown;
        }
    }


	[HarmonyPatch(typeof(TargetCam), "Initialize")]
    public class OnTargetCamInitialize {
        public static void Postfix(TargetCam __instance, ref TargetCam.CamMode ___currentMode, ref UnitPart ___attachedPart) {
            Plugin.Log($"OnTargetCamInitialize.Postfix()");

            var aircraft = ___attachedPart?.parentUnit as Aircraft;
            if (!KeepOnAfterTouchDown || aircraft == null || !aircraft.Identity.HasAuthority || !(aircraft.gearState == LandingGear.GearState.Extending || aircraft.gearState == LandingGear.GearState.LockedExtended))
                return;

			WeaponManager weaponManager = aircraft.weaponManager;
			if (weaponManager != null && weaponManager.GetTargetList().Count > 0) {
                InvokeOnCamToggle(__instance, false, TargetCam.CamMode.targetForward);
			}

            __instance.SetLandingCam();
        }
    }

	[HarmonyPatch(typeof(TargetCam), "Update")]
    public class OnTargetCamUpdate {
        public static void Postfix(ref Camera ___cam, ref TargetCam.CamMode ___currentMode, ref Aircraft ___aircraft) {
            if (Rotate &&___aircraft != null && ___currentMode == TargetCam.CamMode.landingMode) {
                var velocity = ___aircraft.rb.velocity;
                if (velocity.magnitude > 1f && Vector3.Dot(velocity.normalized, ___cam.transform.forward) < deadzoneDotProductThreshold) {
                    ___cam.transform.rotation = Quaternion.Slerp(___cam.transform.rotation, Quaternion.LookRotation(velocity, Vector3.up), Time.deltaTime * RotationSpeed);
                    var eulerAngles = ___cam.transform.localEulerAngles;
                    eulerAngles.x = MathUtils.ClampAngle(eulerAngles.x);
                    eulerAngles.x = Mathf.Clamp(eulerAngles.x, TiltLimits.x, TiltLimits.y);
                    eulerAngles.y = MathUtils.ClampAngle(eulerAngles.y);
                    eulerAngles.y = Mathf.Clamp(eulerAngles.y, PanLimits.x, PanLimits.y);
                    eulerAngles.z = 0f;
                    ___cam.transform.localEulerAngles = eulerAngles;
                }
            }
        }
    }

	[HarmonyPatch(typeof(TargetCam), "SetLandingCam")]
    public class OnTargetCamSetLandingCam {
        public static void Prefix(ref float ___landingCamFoV) {
            ___landingCamFoV = LandingCamFOV;
        }

        public static void Postfix(ref Camera ___cam, ref Aircraft ___aircraft) {
            var tilt = Mathf.Clamp(MathUtils.ClampAngle(InitialAngles.x), TiltLimits.x, TiltLimits.y);
            var pan = Mathf.Clamp(MathUtils.ClampAngle(InitialAngles.y), PanLimits.x, PanLimits.y);
            ___cam.transform.localEulerAngles = new Vector3(tilt, pan, 0);

            //FIX: A-19 Brawler landing cam fix
            if (FixBrawlerLandingCam && ___aircraft.GetAircraftParameters().aircraftName == "A-19 Brawler") {
                Plugin.Log("Applying A-19 Brawler landing cam fix.");
                ___cam.transform.localPosition = new Vector3 (0, -0.5f, 0);
            }
        }
    }

    /* FIX: TargetCam.CancelTarget() disables cam regardless of currentMode, so when camera is in landing mode and targeted unit is deselected, camera freezes.
       This patch fixes erroneous freezing (disabling) of landing cam on target deselect. */
	[HarmonyPatch(typeof(TargetCam), "CancelTarget")]
    public class OnTargetCamCancelTarget {
        public static bool Prefix(ref TargetCam.CamMode ___currentMode, ref GlobalPosition ___targetPosition) {
            if (___currentMode == TargetCam.CamMode.landingMode) {
                ___targetPosition = default(GlobalPosition);
                return false;
            }
            else
                return true;
        }
    }

	[HarmonyPatch(typeof(LandingScreenUI), "LateUpdate")]
    public class OnLandingScreenUILateUpdate {
        public static void Postfix(ref Image ___velocity) {
            var aircraft = SceneSingleton<CombatHUD>.i?.aircraft;
            if (aircraft != null) {
                ___velocity.enabled = aircraft.rb.velocity.magnitude > 1f;
            }
        }
    }


    private static FieldInfo onCamToggleInfo = AccessTools.Field(typeof(TargetCam), "onCamToggle");

    private static void InvokeOnCamToggle (TargetCam targetCam, bool enabled, TargetCam.CamMode camMode) {
        var onCamToggleArg = new TargetCam.OnCamToggle {
            enabled = enabled,
            camMode = camMode
        };
        EventHandler onCamToggleEventHandler = onCamToggleInfo.GetValue(targetCam) as EventHandler;
        if (onCamToggleEventHandler != null) {
            Delegate[] subscribers = onCamToggleEventHandler.GetInvocationList();
            foreach (Delegate subscriber in subscribers)
                if (subscriber != null)
                    subscriber.DynamicInvoke(new object[] {onCamToggleArg});
        }
    }
}
