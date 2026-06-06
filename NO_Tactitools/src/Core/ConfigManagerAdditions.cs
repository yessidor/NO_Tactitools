using System;
using BepInEx.Configuration;
using UnityEngine;
using Rewired;
using HarmonyLib;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace NO_Tactitools.Core;

internal sealed class ConfigurationManagerAttributes
/// <summary>
/// Class that can be used to customize how a setting is displayed in the configuration manager window.
/// </summary>
{
    public bool? IsAdvanced;
    public bool? Browsable;
    public string Category;
    public Action<ConfigEntryBase> CustomDrawer;
    public string DispName;
    public int? Order;
    public bool? ReadOnly;
    public bool? HideDefaultButton;
    public bool? HideSettingName;
    public object Config;
}

public class RewiredInputConfig {
    public static List<RewiredInputConfig> AllConfigs = [];
    public ConfigEntry<string> Input { get; private set; }
    public ConfigEntry<string> ControllerName { get; private set; }
    public ConfigEntry<int> ButtonIndex { get; private set; }
    public ConfigEntry<string> ModifiersString { get; private set; }
    public bool IsModifier = false;

    private bool _wasBound = false;

    public RewiredInputConfig(ConfigFile config, string category, string featureName, string description, int order, bool isModifier = false) {
        ControllerName = config.Bind(
            category,
            $"{featureName} - Controller Name", "",
            new ConfigDescription(
                "Name of the peripheral",
                null,
                new ConfigurationManagerAttributes { Browsable = false }));
        ButtonIndex = config.Bind(
            category,
            $"{featureName} - Button Index",
            -2,
            new ConfigDescription(
                "Index of the button",
                null,
                new ConfigurationManagerAttributes { Browsable = false }));
        ModifiersString = config.Bind(
            category,
            $"{featureName} - Modifiers",
            "",
            new ConfigDescription(
                "Modifiers of the button",
                null,
                new ConfigurationManagerAttributes { Browsable = false }));
        Input = config.Bind(
            category,
            $"{featureName} - Input",
            "",
            new ConfigDescription(description, null,
                new ConfigurationManagerAttributes {
                    Order = order,
                    CustomDrawer = RewiredConfigManager.RewiredButtonDrawer,
                    Config = this }));

        IsModifier = isModifier;

        if (!AllConfigs.Contains(this))
            AllConfigs.Add(this);

        // track initial state
        _wasBound = !string.IsNullOrEmpty(Input.Value);

        // Since modifying all other entries modify Input entry, tracking just Input contens change
        Input.SettingChanged += (s, e) => OnSettingChanged();
    }

    public bool ExecOnSettingChanged = true;

    public void OnSettingChanged() {
        if (!ExecOnSettingChanged) {
            return;
        }

        bool isBound = !string.IsNullOrEmpty(Input.Value);

        if (isBound && !_wasBound) {
            if (IsModifier) {
                InputCatcher.ModsTracker.AddModifierBinding(this);
                RewiredConfigManager.ModsTracker.AddModifierBinding(this);
            }
            else 
                InputCatcher.RegisterNewBinding(this);
        }
        else if (isBound && _wasBound) {
            if (IsModifier) {
                InputCatcher.ModsTracker.ChangeModifierBinding(this);
                RewiredConfigManager.ModsTracker.ChangeModifierBinding(this);
            }
            else 
                InputCatcher.ModifyInputAfterNewConfig(this);
        }
        else if (!isBound && _wasBound) {
            if (IsModifier) {
                InputCatcher.ModsTracker.RemoveModifierBinding(this);
                RewiredConfigManager.ModsTracker.RemoveModifierBinding(this);
            }
            else 
                InputCatcher.UnregisterInput(this, clearLinkedEntries: true);
        }
        _wasBound = isBound;
    }
}

internal sealed class RewiredConfigManager {
    private static bool _isListeningForInput = false;
    private static RewiredInputConfig _targetConfig = null;
    private static ConfigEntryBase _targetInputEntry = null;
    private static string _errorMessage = null;
    private static float _errorTimer = 0f;
    private static TraverseCache<Controller, IList<Controller.Element>> controllerElementsCache = new("KHksquAJKcDEUkNfJQjMANjDEBFB");

    public static ModifiersTracker ModsTracker = new ();

    public static void ResetInputCatcherState() {
        _isListeningForInput = false;
        _targetConfig = null;
        _targetInputEntry = null;
        _errorMessage = null;
        _errorTimer = 0f;
    }

    public static void Update() {
        if (_errorTimer > 0) {
            _errorTimer -= Time.unscaledDeltaTime;
            if (_errorTimer <= 0) _errorMessage = null;
        }

        if (!_isListeningForInput || ReInput.controllers == null) return;

        //Update modifiers
        foreach (var controller in ReInput.controllers.Controllers) {
            ModsTracker.UpdateModifiersState(controller);
        }

        bool isModifier = _targetConfig != null && (bool)(_targetConfig.IsModifier);

        // Modifiers don't have modifiers
        string activeModifiersString = isModifier ? "" : ModifierUtils.ToString(ModsTracker.GetModifiers(activeOnly: true));

        foreach (var controller in ReInput.controllers.Controllers) {
            if (!controller.GetAnyButtonDown())
                continue;
            string controllerName = controller.name.Trim();
            IList<Rewired.Controller.Element> elements = controllerElementsCache.GetValue((Controller)controller);
            for (int i = 0; i < controller.buttonCount; i++) {
                if (!controller.GetButtonDown(i))
                    continue;
                string buttonName = elements[i].elementIdentifier.name;

                // Handle special management keys for the config drawer
                if (controller.type == ControllerType.Keyboard) {
                    string lowerName = buttonName.ToLower();
                    switch (lowerName) {
                        case "escape":
                        case "esc":
                            ResetInputCatcherState();
                            return;
                        case "delete":
                        case "backspace":
                        case "suppr":
                        case "del":
                            _targetConfig.ControllerName.BoxedValue = "";
                            _targetConfig.ButtonIndex.BoxedValue = -3;
                            _targetInputEntry.BoxedValue = "";
                            _targetConfig.ModifiersString.BoxedValue = "";
                            ResetInputCatcherState();
                            return;
                        default:
                            break;
                    }
                }

                // Don't bind key as functional key if it is already registered as modifier
                if (!isModifier) {
                    var possibleModifier = new Modifier (controllerName, i);
                    if(ModsTracker.HasModifier(possibleModifier)) {
                        _errorMessage = activeModifiersString;
                        _errorTimer = 3f;
                        return;
                    }
                }

                // Conflict check
                foreach (var config in RewiredInputConfig.AllConfigs) {
                    if (config.Input == _targetInputEntry)
                        continue;
                    bool matchesAnotherFunctionalKey =
                        config.ControllerName.Value == controllerName &&
                        config.ButtonIndex.Value == i &&
                        config.ModifiersString.Value == activeModifiersString;
                    if (matchesAnotherFunctionalKey) {
                        string conflictName = config.Input.Definition.Key;
                        if (conflictName.EndsWith(" - Input")) conflictName = conflictName.Substring(0, conflictName.Length - 8);
                        _errorMessage = $"Conflict: {conflictName}";
                        _errorTimer = 3f;
                        return;
                    }
                }

                if (_targetConfig != null) {
                    try {
                        _targetConfig.ExecOnSettingChanged = false;
                        _targetInputEntry.BoxedValue = string.Format(
                            "{0}{1}",
                            activeModifiersString.Length > 0 ? activeModifiersString + " + " : "",
                            new Modifier(controllerName, i));
                        _targetConfig.ModifiersString.BoxedValue = activeModifiersString;
                        _targetConfig.ControllerName.BoxedValue = controllerName;
                        _targetConfig.ButtonIndex.BoxedValue = i;
                    }
                    finally {
                        _targetConfig.ExecOnSettingChanged = true;
                    }
                    _targetConfig.OnSettingChanged();
                }

                ResetInputCatcherState();
                return;
            }
        }
    }

    public static void RewiredButtonDrawer(ConfigEntryBase entry) {
        if (_isListeningForInput && _targetInputEntry == entry) {
            GUIUtility.keyboardControl = 0;
            string label = string.IsNullOrEmpty(_errorMessage) ? "Listening... (ESC to cancel or Suppr to unbind)" : _errorMessage;
            if (GUILayout.Button(label, GUILayout.ExpandWidth(true))) {
                ResetInputCatcherState();
            }
        }
        else {
            string val = (string)entry.BoxedValue;
            if (string.IsNullOrEmpty(val)) val = "None - Click to bind";
            if (GUILayout.Button(val, GUILayout.ExpandWidth(true))) {
                _isListeningForInput = true;
                _targetInputEntry = entry;
                _errorMessage = null;
                _errorTimer = 0f;

                // lookup of the linked facultative entries
                ConfigurationManagerAttributes attr = entry.Description.Tags?.OfType<ConfigurationManagerAttributes>().FirstOrDefault();
                _targetConfig = attr?.Config as RewiredInputConfig;
            }
        }
    }
}
