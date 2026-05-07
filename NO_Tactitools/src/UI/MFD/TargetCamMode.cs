using HarmonyLib;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.MFD;

[HarmonyPatch(typeof(TargetCam), "GetPositionAndSize")]
class TargetCamModePlugin {
    public static bool State {
        set {
            field = value;
            var msg = string.Format("Target Camera: <b>{0}</b>", (value ? "Focus on active target" : "Look at all targets"));
            UIBindings.Game.DisplayToast(msg, 3f);
        }
        get;
    } = false;
    private static bool initialized = false;

    static bool Prefix(ref List<Unit> targets) {
        if (!initialized) {
            Plugin.Log("[TCM] Initializing Target Cam Mode plugin");
            InputCatcher.RegisterNewInput(
                Plugin.targetCamModeToggleKey,
                0.0f,
                onPress: () => { State = !State; }
            );
            BindingHelper.Binding[] bindings = new BindingHelper.Binding[] {
                new (typeof(TargetCamModePlugin), "State", Plugin.targetCamModeEnabled)
            };
            BindingHelper.ApplyBindings(bindings);
            initialized = true;
            Plugin.Log("[TCM] Initialized Target Cam Mode plugin");
        }
        if (State)
          targets = [targets[0]];
        return true;
    }
};
