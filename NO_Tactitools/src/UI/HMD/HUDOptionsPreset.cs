using HarmonyLib;
using UnityEngine;
using UnityEngine.UI; //Image
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

[HarmonyPatch(typeof(MainMenu), "Start")]
class HUDOptionsPresetPlugin {
    private static bool initialized = false;

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
                  PlayerSettings.pressDelay,
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


class HUDOptionsPresetComponent {
    public static bool EnableBuiltinSettings = false;

    private static float reportDelay = 2f;
    private static string configName = "HUDOptionsPreset.cfg";
    private static Dictionary<int, HUDOptions_Priorities> presets = new ();
    private static string entryFormat = @"""{0}"" : {1}";
    private static string entryPattern = @" *""(\d*?)"" *: *({.*}) *";
    private static FieldInfo currentSettingInfo = AccessTools.Field(typeof(HUDOptions), "currentSetting");

    public static void Recall(int i) {
        Plugin.Log(string.Format("[HOP] Recall({0})", i));
        string report = null;
        if (presets.TryGetValue(i, out var preset)) {
            SceneSingleton<HUDOptions>.i.ApplySettings(ClonePriorities(preset));
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
        presets[i] = ClonePriorities((HUDOptions_Priorities)currentSettingInfo.GetValue(SceneSingleton<HUDOptions>.i));
        string report = string.Format("Saved HUD options preset <b>{0}</b> <b>({1})</b>", i, GetShown(presets[i]));
        UIBindings.Game.DisplayToast(report, reportDelay);
        UIBindings.Sound.PlaySound("beep_remember");
        SaveConfig();
    }

    private static void SaveConfig() {
      List<string> entries = new ();
      foreach (var idAndPreset in presets) {
          var id = idAndPreset.Key;
          var preset = idAndPreset.Value;
          var jsonString = JsonUtility.ToJson(preset, prettyPrint: false);
          string entry = string.Format(entryFormat, id, jsonString);
          entries.Add(entry);
      }
      FileUtilities.WriteListToConfigFile(configName, entries);
    }

    private static void LoadConfig() {
        List<string> entries = FileUtilities.GetListFromConfigFile(configName);
        foreach (var entry in entries) {
            Match m = Regex.Match(entry, entryPattern);
            if (m.Success) {
                if (!int.TryParse(m.Groups[1].Value, out var id)) {
                    Plugin.Log(string.Format("[HOP] Cannot parse {0} as preset id}", m.Groups[1].Value));
                    continue;
                }
                HUDOptions_Priorities preset = (HUDOptions_Priorities)ScriptableObject.CreateInstance(typeof(HUDOptions_Priorities));
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
