using HarmonyLib;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

using Preset = Dictionary<TargetListSelector_ToggleButton, bool>;

[HarmonyPatch(typeof(MainMenu), "Start")]
class TargetFilterPresetPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[TFP] Target Filter Preset plugin starting !");
            Plugin.harmony.PatchAll(typeof(TargetFilterPresetComponent.OnTargetListSelectorStart));
            Plugin.harmony.PatchAll(typeof(TargetFilterPresetComponent.OnHUDUnitMarkerUpdateMaximized));

            for (int i = 0; i < Plugin.targetFilterPresetNum.Value; i++)
            {
              int j = i;
              InputCatcher.RegisterNewInput(
                  Plugin.targetFilterPresets[i],
                  TargetFilterPresetComponent.longPressDelay,
                  onRelease: () => TargetFilterPresetComponent.Recall(j),
                  onLongPress: () => TargetFilterPresetComponent.Remember(j)
              );
            }

            BindingHelper.ApplyBindings(new BindingHelper.Binding(typeof(TargetFilterPresetComponent), "MaximizeTargetableMarkers", Plugin.targetFilterPresetMaximizeTargetable));
            initialized = true;
            Plugin.Log($"[TFP] Target Filter Preset plugin successfully started !");
        }
    }
}


class TargetFilterPresetComponent {
    public static bool MaximizeTargetableMarkers { set; get; } = false;
    public static float longPressDelay = 0.2f;
    public static float reportDelay = 2f;
    public static string configName = "TargetFilterPreset.cfg";
    private static Dictionary<int, Preset> presets;
    private static Dictionary<string, TargetListSelector_ToggleButton> buttons;
    private static string entryFormat = @"""{0}"" : {{ {1} }}";
    private static string entryPattern = @" *""(\d*?)"" *: *{(.*?)} *";

    public static void Recall(int i) {
        Plugin.Log(string.Format("[TFP] Recall({0})", i));
        string report = null;
        if (presets.TryGetValue(i, out var preset)) {
            foreach (var buttonAndStatus in preset) {
                var button = buttonAndStatus.Key;
                var status = buttonAndStatus.Value;
                button.Set(status);
            }
            report = string.Format("Recalled target filter preset <b>{0}</b> <b>({1})</b>", i, GetTargetables(preset));
        }
        else {
            report = string.Format("Target filter preset <b>{0}</b> not found", i);
        }
        UIBindings.Game.DisplayToast(report, reportDelay);
        UIBindings.Sound.PlaySound("beep_scroll");
    }

    public static void Remember(int i) {
        Plugin.Log(string.Format("[TFP] Remember({0})", i));
        Preset preset = new Preset ();
        foreach (var button in buttons.Values)
            preset[button] = button.status;
        presets[i] = preset;
        string report = string.Format("Saved target filter preset <b>{0}</b> <b>({1})</b>", i, GetTargetables(preset));
        UIBindings.Game.DisplayToast(report, reportDelay);
        UIBindings.Sound.PlaySound("beep_remember");
        SaveConfig();
    }

    private static void SaveConfig() {
      List<string> entries = new ();
      foreach (var idAndPreset in presets) {
          var id = idAndPreset.Key;
          var preset = idAndPreset.Value;
          List<string> entryElements = new ();
          foreach (var buttonAndStatus in preset) {
              var button = buttonAndStatus.Key;
              var status = buttonAndStatus.Value;
              //As of NO 0.33.2, spaces in Target List Controller button names are replaced with newlines
              var buttonName = button.label.text.Replace("\n", " ").Trim();
              var s = string.Format("{0} : {1}", buttonName, status);
              entryElements.Add(s);
          }
          string entry = string.Format(entryFormat, id, string.Join(", ", entryElements));
          entries.Add(entry);
      }
      FileUtilities.WriteListToConfigFile(configName, entries);
    }

    private static void LoadConfig() {
        List<string> entries = FileUtilities.GetListFromConfigFile(configName);
        foreach (var entry in entries) {
            Match m = Regex.Match(entry, entryPattern);
            if (m.Success) {
                Preset preset = new ();
                if (!int.TryParse(m.Groups[1].Value, out var id)) {
                    Plugin.Log(string.Format("[TFP] Cannot parse {0} as preset id}", m.Groups[1].Value));
                    continue;
                }
                string presetContents = m.Groups[2].Value;
                string[] namesAndValues = presetContents.Split(",");
                foreach (var bns in namesAndValues) {
                    string[] parts = bns.Split(":");
                    string name = parts[0].Trim();
                    string value_ = parts[1].Trim();
                    if (!bool.TryParse(value_, out var status)) { 
                        Plugin.Log(string.Format("[TFP] Cannot parse {0} as bool status for button {1}", value_, name));
                        continue;
                    }
                    if (!buttons.TryGetValue(name, out var button)) {
                        Plugin.Log(string.Format("[TFP] Cannot find button for {0}", name));
                        continue;
                    }
                    preset[button] = status;
                }
                presets[id] = preset;
            }
        }
    }

    private static void OnTargetListSelectorStartCallback() {
        Plugin.Log($"[TFP] Target Filter Preset plugin update started !");
        buttons = new ();
        presets = new ();
        TargetListSelector tls = UIBindings.Game.GetTargetListSelectorComponent();
        List<TargetListSelector_ToggleButton> buttonsList = [tls.toggleFollowHUD, tls.toggleLaser];
        buttonsList.AddRange(tls.toggleFactionItems);
        buttonsList.AddRange(tls.toggleUnitTypesItems);
        buttonsList.AddRange(tls.toggleVehicleTypesItems);
        foreach (var button in buttonsList) {
          var buttonName = button.label.text.Replace("\n", " ");
          buttons[buttonName] = button;
        }
        LoadConfig();
        Plugin.Log($"[TFP] Target Filter Preset plugin update successful !");
    }

    private static string GetTargetables(Preset preset) {
          List<string> targetables = new ();
          foreach (var buttonAndStatus in preset) {
              var status = buttonAndStatus.Value;
              if (!status)
                  continue;
              //As of NO 0.33.2, spaces in Target List Controller button names are replaced with newlines
              var button = buttonAndStatus.Key;
              var buttonName = button.label.text.Replace("\n", " ").Trim();
              targetables.Add(buttonName);
          }
          return string.Join(", ", targetables);
    }

    [HarmonyPatch(typeof(TargetListSelector), "Start")]
    public class OnTargetListSelectorStart {
        public static void Postfix() {
            OnTargetListSelectorStartCallback();
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "UpdateMaximized")]
    public class OnHUDUnitMarkerUpdateMaximized {
        public static void Prefix(ref HUDUnitMarker __instance, out bool __state) {
            __state = __instance.alwaysMaximized;
            if (MaximizeTargetableMarkers) {
                TargetListSelector tls = UIBindings.Game.GetTargetListSelectorComponent();
                if (!tls.CheckExclusions(__instance.unit))
                    __instance.alwaysMaximized = true;
            }
        }

        public static void Postfix(ref HUDUnitMarker __instance, ref bool __state) {
            __instance.alwaysMaximized = __state;
        }
    }
}
