using System;
using HarmonyLib;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

[HarmonyPatch(typeof(MainMenu), "Start")]
class WeaponSwitcherPlugin {
    private static bool initialized = false;

    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[WS] Weapon Switcher plugin starting !");
            for (byte i = 0; i < Plugin.WeaponSwitcher.SlotsNum.Value; i++) {
              var j = i;
              InputCatcher.RegisterNewInput(
                  Plugin.WeaponSwitcher.Slots[i],
                  0.0001f, 
                  onLongPress: () => GameBindings.Player.Aircraft.Weapons.SetActiveStation(j)
              );
            }

            Plugin.harmony.PatchAll(typeof(WeaponSwitcherComponent.OnWeaponManagerNextWeaponStation));
            Plugin.harmony.PatchAll(typeof(WeaponSwitcherComponent.OnWeaponManagerPreviousWeaponStation));

            var bindings = new BindingHelper.Binding[] {
                new (typeof(WeaponSwitcherComponent), "SkipEmptyStations", Plugin.WeaponSwitcher.SkipEmptyStations),
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log("[WS] Weapon Switcher plugin succesfully started !");
        }
    }
}


public class WeaponSwitcherComponent {
    public static bool SkipEmptyStations = false;

    public static void CycleWeaponStation(WeaponManager weaponManager, Aircraft aircraft, bool up) {
        var stationsCount = aircraft.weaponStations.Count;
        if (stationsCount != 0 && stationsCount != 1) {
            int currentStationNumber = weaponManager.currentWeaponStation.Number;
            int i = currentStationNumber;
            while (true) {
                i = (up ? (i + 1) : (i - 1 + stationsCount)) % stationsCount;
                if (i == currentStationNumber)
                    break;
                else if (SkipEmptyStations && aircraft.weaponStations[i].Ammo == 0)
                    continue;
                else
                    break;
            }
            weaponManager.currentWeaponStation = aircraft.weaponStations[i];
            aircraft.SetActiveStation((byte)i);
            SceneSingleton<CombatHUD>.i.ShowWeaponStation(weaponManager.currentWeaponStation);
        }
    }

    [HarmonyPatch(typeof(WeaponManager), "NextWeaponStation")]
    public class OnWeaponManagerNextWeaponStation {
        public static bool Prefix(WeaponManager __instance, Aircraft ___aircraft) {
            CycleWeaponStation(__instance, ___aircraft, true);
            return false;
        }
    }

    [HarmonyPatch(typeof(WeaponManager), "PreviousWeaponStation")]
    public class OnWeaponManagerPreviousWeaponStation {
        public static bool Prefix(WeaponManager __instance, Aircraft ___aircraft) {
            CycleWeaponStation(__instance, ___aircraft, false);
            return false;
        }
    }
}
