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
            initialized = true;
            Plugin.Log("[WS] Weapon Switcher plugin succesfully started !");
        }
    }
}
