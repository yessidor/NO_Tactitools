using System;
using HarmonyLib;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

[HarmonyPatch(typeof(MainMenu), "Start")]
class CountermeasureControlsPlugin {
    private static bool initialized = false;
    public static bool Activate = true;

    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[CC] Countermeasure Controls plugin starting !");

            InputCatcher.RegisterNewInput(
                Plugin.countermeasureControlsFlare,
                0.001f,
                onPress: HandleOnPressFlare,
                onReleased: HandleOnReleased
                );
            InputCatcher.RegisterNewInput(
                Plugin.countermeasureControlsJammer,
                0.001f,
                onPress: HandleOnPressJammer,
                onReleased: HandleOnReleased
                );
            InputCatcher.RegisterNewInput(
                Plugin.countermeasureControlsChaff,
                0.001f,
                onPress: HandleOnPressChaff,
                onReleased: HandleOnReleased
                );

            BindingHelper.Binding[] bindings = new BindingHelper.Binding[] {
                new (typeof(CountermeasureControlsPlugin), "Activate", Plugin.countermeasureControlsActivate)
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

    private static void HandleOnReleased() {
        if (Activate)
            GameBindings.Player.Aircraft.Countermeasures.SetState(false);
    }
}
