using HarmonyLib;
using UnityEngine;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System;
using Newtonsoft.Json;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

using Preset = Dictionary<TargetListSelector_ToggleButton, bool>;

[HarmonyPatch(typeof(MainMenu), "Start")]
class TargetFilterPresetPlugin {
    public static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[TFP] Target Filter Preset plugin starting !");

            Plugin.harmony.PatchAll(typeof(TargetFilterPresetComponent.OnTargetListSelectorStart));

            for (int i = 0; i < Plugin.TargetFilterPreset.PresetsNum.Value; i++) {
              int j = i;
              InputCatcher.RegisterNewInput(
                  Plugin.TargetFilterPreset.Presets[i],
                  Plugin.PressDelay.Value,
                  onRelease: () => TargetFilterPresetComponent.Recall(j),
                  onLongPress: () => TargetFilterPresetComponent.Remember(j)
              );
            }

            initialized = true;

            Plugin.Log($"[TFP] Target Filter Preset plugin successfully started !");
        }
    }
}


public class TargetFilterPresetComponent {
    public static float reportDelay = 2f;
    public static string configName = "TargetFilterPreset.cfg";
    private static Dictionary<int, Preset> presets = new ();
    private static Dictionary<string, TargetListSelector_ToggleButton> buttons = new ();
    private static string entryPattern = @" *""(\d*?)"" *: *{(.*?)} *";

    public static void Recall(int i) {
        Plugin.Log(string.Format("[TFP] Recall({0})", i));

        if (!TargetFilterPresetPlugin.initialized) {
            Plugin.Log("[TFP] Not initialized");
            return;
        }

        if (UIBindings.Game.GetDynamicMapComponent() == null)
            return;

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

        if (!TargetFilterPresetPlugin.initialized) {
            Plugin.Log("[TFP] Not initialized");
            return;
        }

        if (UIBindings.Game.GetDynamicMapComponent() == null)
            return;

        Preset preset = new Preset ();
        foreach (var button in buttons.Values)
            preset[button] = button.status;
        presets[i] = preset;
        string report = string.Format("Saved target filter preset <b>{0}</b> <b>({1})</b>", i, GetTargetables(preset));
        UIBindings.Game.DisplayToast(report, reportDelay);
        UIBindings.Sound.PlaySound("beep_remember");
        SaveConfig();
    }

    public static void Preview(int i) {
        Plugin.Log(string.Format("[TFP] Preview({0})", i));

        if (!TargetFilterPresetPlugin.initialized) {
            Plugin.Log("[TFP] Not initialized");
            return;
        }

        if (UIBindings.Game.GetDynamicMapComponent() == null)
            return;

        string report = null;
        if (presets.TryGetValue(i, out var preset)) {
            report = string.Format("Target filter preset <b>{0}</b> <b>({1})</b>", i, GetTargetables(preset));
        }
        else {
            report = string.Format("Target filter preset <b>{0}</b> not found", i);
        }
        UIBindings.Game.DisplayToast(report, reportDelay);
    }

    private static void SaveConfig() {
        Plugin.Log($"[TFP] SaveConfig()");
        Dictionary<string, Dictionary<string, bool>> entries = new ();
        foreach (var idAndPreset in presets) {
            Dictionary<string, bool> entryElements = new ();
            var id = idAndPreset.Key;
            var preset = idAndPreset.Value;
            foreach (var buttonAndStatus in preset) {
                var button = buttonAndStatus.Key;
                var buttonStatus = buttonAndStatus.Value;
                //As of NO 0.33.2, spaces in Target List Controller button names are replaced with newlines
                var buttonName = button.label.text.Replace("\n", " ").Trim();
                entryElements[buttonName] = buttonStatus;
            }
            entries[id.ToString()] = entryElements;
        }
        var json = JsonConvert.SerializeObject(entries, Formatting.Indented);
        File.WriteAllText(FileUtilities.GetConfigPath(configName), json);
    }

    private static void LoadConfig() {
        Plugin.Log($"[TFP] LoadConfig()");
        try {
            var configPath = FileUtilities.GetConfigPath(configName);
            if (!File.Exists(configPath)) {
                Plugin.Log($"[TFP] File {configPath} does not exist");
                return;
            }
            var json = File.ReadAllText(configPath);
            var entries = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, bool>>>(json);

            foreach ((var idString, var entry) in entries) {
                if (!int.TryParse(idString, out var id)) {
                    Plugin.Log($"[TFP] Cannot parse {idString} as preset id");
                    continue;
                }

                Preset preset = new ();
                foreach ((var buttonName, var buttonStatus) in entry) {
                    if (!buttons.TryGetValue(buttonName, out var button)) {
                        Plugin.Log($"[TFP] Cannot find button for {buttonName}");
                        continue;
                    }
                    preset[button] = buttonStatus;
                }
                presets[id] = preset;
            }
        }
        catch (Exception e) when (e is JsonReaderException || e is JsonSerializationException) {
            Plugin.Log($"[TFP] Failed to load JSON config. Trying legacy config parser.");
            LoadLegacyConfig();
        }
        catch (Exception e) {
            Plugin.Log($"[TFP] Unexpected exception when trying to load JSON config: {e}.");
        }
    }

    private static void LoadLegacyConfig() {
        List<string> entries = FileUtilities.GetListFromConfigFile(configName);
        foreach (var entry in entries) {
            Match m = Regex.Match(entry, entryPattern);
            if (m.Success) {
                if (!int.TryParse(m.Groups[1].Value, out var id)) {
                    Plugin.Log(string.Format("[TFP] Cannot parse {0} as preset id}", m.Groups[1].Value));
                    continue;
                }

                Preset preset = new ();
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
        buttons.Clear();
        presets.Clear();
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
}
