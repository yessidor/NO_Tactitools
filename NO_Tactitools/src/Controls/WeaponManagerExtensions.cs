using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Rewired;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

public class WeaponManagerExtensionsComponent {
    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class OnMainMenuStart {
        private static bool initialized = false;

        static void Postfix() {
            if (!initialized) {
                Plugin.Log($"[WME] Weapon Manager Extensions plugin starting !");

                if (Plugin.WeaponManagerExtensions.AttackActiveTarget.Value) {
                    Plugin.harmony.PatchAll(typeof(WeaponManagerExtensionsComponent.OnPlayerGetButton));
                    Plugin.harmony.PatchAll(typeof(WeaponManagerExtensionsComponent.OnPilotPlayerStatePlayersControls));
                    Plugin.Log($"[WME] Attack Active Target feature enabled");
                }

                if (Plugin.WeaponManagerExtensions.AttackOnlyLasedTargets.Value) {
                    Plugin.harmony.PatchAll(typeof(WeaponManagerExtensionsComponent.OnWeaponStationLaunchMount));
                    Plugin.harmony.PatchAll(typeof(WeaponManagerExtensionsComponent.OnWeaponManagerSalvoFire));
                    Plugin.Log($"[WME] Attack Only Lased Targets feature enabled");
                }

                if (Plugin.WeaponManagerExtensions.AimAssistToggle.Value) {
                    InputCatcher.RegisterButtonInput(
                        Plugin.WeaponManagerExtensions.ToggleAimAssist,
                        0.0f,
                        onPress: ToggleAimAssist
                    );
                    Plugin.harmony.PatchAll(typeof(WeaponManagerExtensionsComponent.OnControlsFilterSetFlightAssist));
                    Plugin.Log($"[WME] Aim Assist Toggle feature enabled");
                }

                var bindings = new BindingHelper.Binding[] {
                    new (typeof(WeaponManagerExtensionsComponent), "ToggleJammerFire", Plugin.WeaponManagerExtensions.ToggleJammerFire),
                    new (typeof(WeaponManagerExtensionsComponent), "KeepFiring", Plugin.WeaponManagerExtensions.KeepFiring),
                };
                BindingHelper.ApplyBindings(bindings);

                initialized = true;

                Plugin.Log($"[WME] Weapon Manager Extensions plugin started !");
            }
        }
    }

    public static bool ToggleJammerFire = false;
    public static bool KeepFiring = true;

    private static bool inPilotPlayerStatePlayersControls = false;
    private static bool toggledFire = false;
    private static TraverseCache<WeaponManager, List<Unit>> targetListCache = new ("targetList");

    [HarmonyPatch(typeof(Player), "GetButton", typeof(string))]
    public class OnPlayerGetButton {
        public static void Postfix(ref bool __result, ref string actionName) {
            if (inPilotPlayerStatePlayersControls && actionName == "Fire")
                __result = false;
        }
    }

    [HarmonyPatch(typeof(PilotPlayerState), "PlayerControls")]
    public class OnPilotPlayerStatePlayersControls {
        static void Prefix(ref Pilot ___pilot, ref Player ___player) {
            inPilotPlayerStatePlayersControls = true;
        }

        static void Postfix(ref Pilot ___pilot, ref Player ___player) {
            inPilotPlayerStatePlayersControls = false;

            var aircraft = ___pilot.aircraft;
            if (aircraft == null)
                return;
            var weaponManager = aircraft.weaponManager;
            if (weaponManager == null)
                return;
            var currentWeaponStation = weaponManager.currentWeaponStation;
            if (currentWeaponStation == null)
                return;
            var weaponInfo = currentWeaponStation.WeaponInfo;
            var regularFire = weaponInfo.gun || weaponInfo.sling || weaponInfo.fireInterval == 0f;

            var targets = targetListCache.GetValue(weaponManager);
            if (ToggleJammerFire) {
                if(weaponInfo.jammer && targets.Count != 0) {
                    if (___player.GetButtonDown("Fire"))
                        toggledFire = !toggledFire;
                }
                else
                    toggledFire = false;
            }

            if ((regularFire && ___player.GetButton("Fire")) ||
                toggledFire ||
                ___player.GetButtonTimedPressDown("Fire", PlayerSettings.pressDelay) ||
                (KeepFiring && ___player.GetButtonTimedPress("Fire", 2.0f * PlayerSettings.pressDelay))) {
                ___pilot.Fire();
            }
            else if (___player.GetButtonTimedPressUp("Fire", 0f, PlayerSettings.pressDelay)) {
                //to be able dumbfire Eyeball Mk2
                var singleTarget = new List<Unit> ();
                if (targets.Count != 0)
                    singleTarget.Add(targets[0]);
                try {
                    targetListCache.SetValue(weaponManager, singleTarget);
                    ___pilot.Fire();
                }
                finally {
                    targetListCache.SetValue(weaponManager, targets);
                }
            }
        }

        /* Just in case exception in thrown in wrapped function. */
        static void Finalizer() {
            inPilotPlayerStatePlayersControls = false;
        }
    }

    [HarmonyPatch(typeof(WeaponStation), "LaunchMount")]
    public class OnWeaponStationLaunchMount {
        static bool Prefix(WeaponStation __instance, Unit owner, Unit target) {
            if (__instance.WeaponInfo.laserGuided && owner == GameBindings.Player.Aircraft.GetAircraft()) {
                var hq = GameBindings.GameState.GetCurrentFactionHQ();
                //allowing to dumbfire laser-guided missiles
                return target == null || (target.LineOfSight(owner.transform.position, 1000f) && hq.IsTargetLased(target));
            }
            return true;
        }
    }

    /* WeaponManager.SalvoFire() iterates over targets by calling currentWeaponStation.LaunchMount() for each target and waiting.
       So if WeaponStation.LaunchMount() does not fire for laser-guided weapon at unlased target, delay still remains.
       To avoid this, if laser-guided weapon is firing salvo, unlased targets are filtered out first. */
    [HarmonyPatch(typeof(WeaponManager), "SalvoFire")]
    public class OnWeaponManagerSalvoFire {
        static void Prefix(WeaponManager __instance, ref List<Unit> targets, Aircraft ___aircraft) {
            var currentWeaponStation = __instance.currentWeaponStation;
            if (currentWeaponStation == null)
                return;
            if (currentWeaponStation.WeaponInfo.laserGuided && ___aircraft == GameBindings.Player.Aircraft.GetAircraft()) {
                var hq = GameBindings.GameState.GetCurrentFactionHQ();
                var lasedTargets = targets.FindAll(target => hq.IsTargetLased(target));
                targets = lasedTargets;
            }
        }
    }

    private static void ToggleAimAssist() {
        if (aimAssist == null) {
            Plugin.Log("[WME] aimAssist is null (Aim Assist Toggle feature is not enabled?)");
            return;
        }

        string report = "";
        if (!aimAssistEnabledInitial)
            report = "Aim assist is <b>Not available</b>";
        else {
            aimAssistEnabled = !aimAssistEnabled;
            enabledInfo.SetValue(aimAssist, aimAssistEnabled);
            report = $"Aim assist <b>{(aimAssistEnabled ? "Enabled" : "Disabled")}</b>";
        }
        UIBindings.Game.DisplayToast(report, 3f);
    }

    [HarmonyPatch(typeof(ControlsFilter), "SetFlightAssist")]
    private static class OnControlsFilterSetFlightAssist {
        public static void Postfix(ControlsFilter __instance, bool enabled, Aircraft aircraft) {
            if (GameManager.IsLocalAircraft(aircraft)) {
                var aa = aimAssistInfo.GetValue(__instance);
                if (aa != null && aa != aimAssist) {
                    aimAssist = aa;
                    aimAssistEnabledInitial = (bool)enabledInfo.GetValue(aimAssist);
                    string report = $"Aim assist <b>{(!aimAssistEnabledInitial ? "Not available" : aimAssistEnabled ? "Enabled" : "Disabled")}</b>";
                    UIBindings.Game.DisplayToast(report, 3f);
                }
            }
        }
    }

    private static bool aimAssistEnabled = true;
    private static bool aimAssistEnabledInitial = false;
    private static object aimAssist = null;

    private static readonly FieldInfo aimAssistInfo = AccessTools.Field(typeof(ControlsFilter), "aimAssist");
    private static readonly Type aimAssistType = typeof(ControlsFilter).GetNestedType("AimAssist", BindingFlags.NonPublic);
    private static readonly FieldInfo enabledInfo = AccessTools.Field(aimAssistType, "Enabled");
}
