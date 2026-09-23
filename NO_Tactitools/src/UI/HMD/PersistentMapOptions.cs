using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Rewired;
using UnityEngine;
using Newtonsoft.Json;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

class PersistentMapOptionsComponent {
    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class OnMainMenuStart {
        private static void Postfix() {
            if (!initialized) {
                Plugin.Log($"[PMO] Persistent Map Options plugin starting !");

                Plugin.harmony.PatchAll(typeof(OnMainMenuQuitGame));
                Plugin.harmony.PatchAll(typeof(OnMapOptions_ToggleButtonStart));
                Plugin.harmony.PatchAll(typeof(OnMapOptions_ToggleButtonSet));

                LoadConfig();

                initialized = true;
                Plugin.Log($"[PMO] Persistent Map Options plugin started !");
            }
        }

        private static bool initialized = false;
    }

    [HarmonyPatch(typeof(MainMenu), "QuitGame")]
    public class OnMainMenuQuitGame {
        private static void Prefix() {
            SaveConfig();
        }
    }

    [HarmonyPatch(typeof(MapOptions_ToggleButton), "Start")]
    private class OnMapOptions_ToggleButtonStart {
        private static void Prefix() {
            inStart = true;
        }

        private static void Postfix(MapOptions_ToggleButton __instance) {
            var name = __instance.name;
            var status = __instance.status;
            if (buttonStatuses.TryGetValue(name, out var s)) {
                if (status != s)
                    __instance.Toggle();
            }
            else
               buttonStatuses[name] = status;
        }

        private static void Finalizer() {
            inStart = false;
        }
    }

    [HarmonyPatch(typeof(MapOptions_ToggleButton), "Set")]
    private class OnMapOptions_ToggleButtonSet {
        private static void Postfix(MapOptions_ToggleButton __instance) {
            if (inStart)
                return;
            buttonStatuses[__instance.name] = __instance.status;
            SaveConfig();
        }
    }

    private static void SaveConfig() {
        Plugin.Log($"[PMO] SaveConfig()");
        var json = JsonConvert.SerializeObject(buttonStatuses, Formatting.Indented);
        File.WriteAllText(FileUtilities.GetConfigPath(configName), json);
    }

    private static void LoadConfig() {
        Plugin.Log($"[PMO] LoadConfig()");
        var configPath = FileUtilities.GetConfigPath(configName);
        if (!File.Exists(configPath)) {
            Plugin.Log($"[PMO] File {configPath} does not exist");
            return;
        }
        var json = File.ReadAllText(configPath);
        buttonStatuses = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);
    }

    private static bool inStart = false;
    private static Dictionary<string, bool> buttonStatuses = new ();
    private static string configName = "PersistentMapOptions.cfg";
}
