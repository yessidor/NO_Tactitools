using System;
using HarmonyLib;
using BepInEx.Configuration;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

[HarmonyPatch(typeof(MainMenu), "Start")]
class CountermeasureControlsPlugin {
    private static bool initialized = false;
    public static bool Activate = true;

    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[CC] Countermeasure Controls plugin starting !");

            foreach (var countermeasureData in Plugin.CountermeasureControls.CountermeasureDatum) {
                var nameVar = countermeasureData.Name;
                var key = countermeasureData.Key;
                InputCatcher.RegisterNewInput(
                    key,
                    0.001f,
                    onPress: () => HandleOnPress(nameVar),
                    onReleased: HandleOnReleased
                );
            }

            BindingHelper.Binding[] bindings = new BindingHelper.Binding[] {
                new (typeof(CountermeasureControlsPlugin), "Activate", Plugin.CountermeasureControls.Activate)
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;

            Plugin.Log("[CC] Countermeasure Controls plugin succesfully started !");
        }
    }

    private static void HandleOnPressFlare() {
        if (GameBindings.Player.Aircraft.Countermeasures.HasIRFlare()) {
            bool set = GameBindings.Player.Aircraft.Countermeasures.GetCurrentIndex() == GameBindings.Player.Aircraft.Countermeasures.GetIRStationIndex();
            if (!set)
                GameBindings.Player.Aircraft.Countermeasures.SetIRFlare();
            if (Activate)
                GameBindings.Player.Aircraft.Countermeasures.SetState(true);
        }
    }

    private static void HandleOnPressJammer() {
        if (GameBindings.Player.Aircraft.Countermeasures.HasJammer()) {
            bool set = GameBindings.Player.Aircraft.Countermeasures.GetCurrentIndex() == GameBindings.Player.Aircraft.Countermeasures.GetJammerStationIndex();
            if (!set)
                GameBindings.Player.Aircraft.Countermeasures.SetJammer();
            if (Activate)
                GameBindings.Player.Aircraft.Countermeasures.SetState(true);
        }
    }

    private static void HandleOnPressChaff() {
        if (GameBindings.Player.Aircraft.Countermeasures.HasChaff()) {
            bool set = GameBindings.Player.Aircraft.Countermeasures.GetCurrentIndex() == GameBindings.Player.Aircraft.Countermeasures.GetChaffStationIndex();
            if (!set)
                GameBindings.Player.Aircraft.Countermeasures.SetChaff();
            if (Activate)
                GameBindings.Player.Aircraft.Countermeasures.SetState(true);
        }
    }

    private static void HandleOnPress(ConfigEntry<string> nameVar) {
        var name = nameVar.Value;
        if (GameBindings.Player.Aircraft.Countermeasures.HasCountermeasureByName(name)) {
            var index = GameBindings.Player.Aircraft.Countermeasures.GetStationIndexByName(name);
            if (GameBindings.Player.Aircraft.Countermeasures.GetCurrentIndex() != index)
                GameBindings.Player.Aircraft.Countermeasures.SetCurrentIndex(index);
            if (Activate)
                GameBindings.Player.Aircraft.Countermeasures.SetState(true);
        }
    }

    private static void HandleOnReleased() {
        if (Activate)
            GameBindings.Player.Aircraft.Countermeasures.SetState(false);
    }
}
