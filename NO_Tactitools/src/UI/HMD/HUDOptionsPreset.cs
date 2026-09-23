using HarmonyLib;
using UnityEngine;
using UnityEngine.UI; //Image
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

[HarmonyPatch(typeof(MainMenu), "Start")]
class HUDOptionsPresetPlugin {
    public static bool initialized = false;

    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[HOP] HUD Options Preset plugin starting !");

            Plugin.harmony.PatchAll(typeof(HUDOptionsPresetComponent.OnHUDOptionsStart));
            Plugin.harmony.PatchAll(typeof(HUDOptionsPresetComponent.OnHUDOptionsLoadValues));
            Plugin.harmony.PatchAll(typeof(HUDOptionsPresetComponent.OnHUDOptionsSaveValues));

            for (int i = 0; i < Plugin.HUDOptionsPreset.PresetsNum.Value; i++) {
              int j = i;
              InputCatcher.RegisterNewInput(
                  Plugin.HUDOptionsPreset.Presets[i],
                  Plugin.PressDelay.Value,
                  onRelease: () => HUDOptionsPresetComponent.Recall(j),
                  onLongPress: () => HUDOptionsPresetComponent.Remember(j)
              );
            }

            var bindings = new BindingHelper.Binding[] {
                new (typeof(HUDOptionsPresetComponent), "EnableBuiltinSettings", Plugin.HUDOptionsPreset.EnableBuiltinSettings),
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;

            Plugin.Log($"[HOP] HUD Options Preset plugin successfully started !");
        }
    }
}


public class HUDOptionsPresetComponent {
    public static bool EnableBuiltinSettings = false;

    private static float reportDelay = 2f;
    private static string configName = "HUDOptionsPreset.cfg";
    private static Dictionary<int, HUDOptions_Priorities> presets = new ();
    private static string entryPattern = @" *""(\d*?)"" *: *({.*}) *";
    private static FieldInfo currentSettingInfo = AccessTools.Field(typeof(HUDOptions), "currentSetting");

    public static void Recall(int i) {
        Plugin.Log(string.Format("[HOP] Recall({0})", i));

        if (!HUDOptionsPresetPlugin.initialized) {
            Plugin.Log("[HOP] Not initialized");
            return;
        }

        string report = null;
        if (presets.TryGetValue(i, out var preset)) {
            var hudOptions = UIBindings.Game.GetHUDOptionsComponent();
            if (hudOptions == null) {
                Plugin.Log("[HOP] hudOptions is null");
                return;
            }
            var clone = ClonePriorities(preset);
            hudOptions.ApplySettings(clone);
            if (!EnableBuiltinSettings) {
                foreach (var listMode in hudOptions.listModes) {
                    listMode.settings = clone;
                }
            }
            report = string.Format("Recalled HUD options preset <b>{0}</b> <b>({1})</b>", i, GetShown(preset));
        }
        else {
            report = string.Format("HUD options preset <b>{0}</b> not found", i);
        }
        UIBindings.Game.DisplayToast(report, reportDelay);
        UIBindings.Sound.PlaySound("beep_scroll");
    }

    public static void Remember(int i) {
        Plugin.Log(string.Format("[HOP] Remember({0})", i));

        if (!HUDOptionsPresetPlugin.initialized) {
            Plugin.Log("[HOP] Not initialized");
            return;
        }

        presets[i] = ClonePriorities((HUDOptions_Priorities)currentSettingInfo.GetValue(UIBindings.Game.GetHUDOptionsComponent()));
        string report = string.Format("Saved HUD options preset <b>{0}</b> <b>({1})</b>", i, GetShown(presets[i]));
        UIBindings.Game.DisplayToast(report, reportDelay);
        UIBindings.Sound.PlaySound("beep_remember");
        SaveConfig();
    }

    public static void Preview(int i) {
        Plugin.Log(string.Format("[HOP] Preview({0})", i));

        if (!HUDOptionsPresetPlugin.initialized) {
            Plugin.Log("[HOP] Not initialized");
            return;
        }

        string report = null;
        if (presets.TryGetValue(i, out var preset)) {
            report = string.Format("HUD options preset <b>{0}</b> <b>({1})</b>", i, GetShown(preset));
        }
        else {
            report = string.Format("HUD options preset <b>{0}</b> not found", i);
        }
        UIBindings.Game.DisplayToast(report, reportDelay);
    }

    private static void SaveConfig() {
        Plugin.Log("[HOP] SaveConfig");
        var resolver = new FileUtilities.IgnorePropertiesResolver(["encyclopedia", "name", "hideFlags"]);
        var settings = new JsonSerializerSettings { ContractResolver = resolver };
        var json = JsonConvert.SerializeObject(presets, Formatting.Indented, settings);
        File.WriteAllText(FileUtilities.GetConfigPath(configName), json);
    }

    private class HUDOptions_PrioritiesConverter : CustomCreationConverter<HUDOptions_Priorities> {
        public override HUDOptions_Priorities Create(Type type) {
            return ScriptableObject.CreateInstance<HUDOptions_Priorities>();
        }
    }

    private static void LoadConfig() {
        Plugin.Log("[HOP] LoadConfig");
        try {
            var hudOptions = UIBindings.Game.GetHUDOptionsComponent();
            if (hudOptions == null) {
                Plugin.Log("[HOP] HUDOptions is null");
                return;
            }
            var currentPriorities = (HUDOptions_Priorities)currentSettingInfo.GetValue(hudOptions);
            var encyclopedia = currentPriorities != null ? currentPriorities.encyclopedia : null;

            var configPath = FileUtilities.GetConfigPath(configName);
            if (!File.Exists(configPath)) {
                Plugin.Log($"[HOP] File {configPath} does not exist");
                return;
            }

            var json = File.ReadAllText(configPath);
            var settings = new JsonSerializerSettings ();
            settings.Converters.Add(new HUDOptions_PrioritiesConverter ());
            presets = JsonConvert.DeserializeObject<Dictionary<int, HUDOptions_Priorities>>(json, settings);
            foreach (var preset in presets.Values)
                preset.encyclopedia = encyclopedia;
        }
        catch (Exception e) when (e is JsonReaderException || e is JsonSerializationException) {
            Plugin.Log($"[HOP] Failed to load JSON config. Trying legacy config parser.");
            LoadLegacyConfig();
        }
        catch (Exception e) {
            Plugin.Log($"[HOP] Unexpected exception when trying to load JSON config: {e}.");
        }
    }

    private static void LoadLegacyConfig() {
        Plugin.Log("[HOP] LoadLegacyConfig");
        List<string> entries = FileUtilities.GetListFromConfigFile(configName);
        foreach (var entry in entries) {
            Match m = Regex.Match(entry, entryPattern);
            if (m.Success) {
                var presetIdString = m.Groups[1].Value;
                if (!int.TryParse(presetIdString, out var id)) {
                    Plugin.Log($"[HOP] Cannot parse {presetIdString} as preset id");
                    continue;
                }
                var preset = ScriptableObject.CreateInstance<HUDOptions_Priorities>();
                JsonUtility.FromJsonOverwrite(m.Groups[2].Value, preset);
                presets[id] = preset;
            }
        }
    }

    private static void OnHUDOptionsStartCallback() {
        Plugin.Log($"[HOP] HUD Options Preset plugin update started !");
        presets.Clear();
        LoadConfig();
        Plugin.Log($"[HOP] HUD Options Preset plugin update successful !");
    }

    private static string GetShown(HUDOptions_Priorities preset) {
          void AddShown(List<string> shown, List<HUDOptions_Priorities.Setting> settings) {
              foreach (var setting in settings)
                  if (setting.typePriority)
                      shown.Add(setting.typeName);
          }
          List<string> shown = new ();
          AddShown(shown, preset.listCategories);
          AddShown(shown, preset.listVehicles);
          AddShown(shown, preset.listBuildings);
          return string.Join(", ", shown);
    }

    private static HUDOptions_Priorities ClonePriorities(HUDOptions_Priorities priorities) {
        HUDOptions_Priorities.Setting CloneSetting(HUDOptions_Priorities.Setting setting) {
            return new HUDOptions_Priorities.Setting { typeName = setting.typeName, typePriority = setting.typePriority};
        };

        HUDOptions_Priorities cloned = (HUDOptions_Priorities)ScriptableObject.CreateInstance(typeof(HUDOptions_Priorities));
        cloned.listCategories = priorities.listCategories.ConvertAll(CloneSetting);
        cloned.listVehicles = priorities.listVehicles.ConvertAll(CloneSetting);
        cloned.listBuildings = priorities.listBuildings.ConvertAll(CloneSetting);
        cloned.encyclopedia = priorities.encyclopedia;

        return cloned;
    }

    [HarmonyPatch(typeof(HUDOptions), "Start")]
    public class OnHUDOptionsStart {
        public static void Postfix() {
            OnHUDOptionsStartCallback();
        }
    }

    [HarmonyPatch(typeof(HUDOptions), "LoadValues")]
    public class OnHUDOptionsLoadValues {
        public static bool Prefix() {
            return EnableBuiltinSettings;
        }
    }

    [HarmonyPatch(typeof(HUDOptions), "SaveValues")]
    public class OnHUDOptionsSaveValues {
        public static bool Prefix() {
            return EnableBuiltinSettings;
        }
    }
}
