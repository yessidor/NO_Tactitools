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

public abstract class RewiredConfigBase {
    public static List<RewiredConfigBase> AllConfigs = [];

    public ConfigEntry<string> ControllerName { get; private set; }
    public ConfigEntry<string> Input { get; private set; }

    public RewiredConfigBase(ConfigFile config, string category, string featureName, string description, int order) {
        ControllerName = config.Bind(
            category,
            $"{featureName} - Controller Name", "",
            new ConfigDescription(
                "Name of the peripheral",
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

        // FIXME? Config is addded to AllConfigs before child ctor has finished...
        AllConfigs.Add(this);

        // Since modifying all other entries modify Input entry, tracking just Input contens change
        Input.SettingChanged += (s, e) => OnSettingChanged();
    }

    public bool ExecOnSettingChanged = true;

    public void OnSettingChanged() {
        if (ExecOnSettingChanged)
            DoOnSettingChanged();
    }

    public void Set(string controllerName) {
        this.ControllerName.BoxedValue = controllerName;
        //Input is set in derived class
    }

    public void Reset(bool execOnSettingChanged = true) {
        var exec = this.ExecOnSettingChanged;
        this.ExecOnSettingChanged = execOnSettingChanged;
        try {
            DoReset();
        }
        finally {
            this.ExecOnSettingChanged = exec;
        }
    }

    protected abstract void DoOnSettingChanged();

    protected virtual void DoReset() {
        this.ControllerName.BoxedValue = "";
        this.Input.BoxedValue = "";
    }
}

public abstract class RewiredButtonConfigBase : RewiredConfigBase {
    public ConfigEntry<int> ButtonIndex { get; private set; }

    public RewiredButtonConfigBase(ConfigFile config, string category, string featureName, string description, int order) :
        base(config, category, featureName, description, order) {
        ButtonIndex = config.Bind(
            category,
            $"{featureName} - Button Index",
            -2,
            new ConfigDescription(
                "Index of the button",
                null,
                new ConfigurationManagerAttributes { Browsable = false }));
    }

    public void Set(string controllerName, int buttonIndex) {
        base.Set(controllerName);
        this.ButtonIndex.BoxedValue = buttonIndex;
    }

    protected override void DoReset() {
        base.DoReset();
        this.ButtonIndex.BoxedValue = -3;
    }
}

public class RewiredButtonConfig : RewiredButtonConfigBase {
    public static List<RewiredButtonConfig> ButtonConfigs = [];

    public ConfigEntry<string> ModifiersString { get; private set; }

    public RewiredButtonConfig(ConfigFile config, string category, string featureName, string description, int order) :
        base(config, category, featureName, description, order) {

        ModifiersString = config.Bind(
            category,
            $"{featureName} - Modifiers",
            "",
            new ConfigDescription(
                "Modifiers of the button",
                null,
                new ConfigurationManagerAttributes { Browsable = false }));

        // track initial state
        _wasBound = !string.IsNullOrEmpty(Input.Value);

        ButtonConfigs.Add(this);
    }

    public void Set(string controllerName, int buttonIndex, string buttonName, string modifiersString) {
        base.Set(controllerName, buttonIndex);
        this.ModifiersString.BoxedValue = modifiersString;
        this.Input.BoxedValue = string.Format(
            "{0}{1}",
            modifiersString.Length > 0 ? modifiersString + " + " : "",
            string.Format("{0} | {1} | {2}", controllerName, buttonName, buttonIndex));
    }

    protected override void DoOnSettingChanged() {
        bool isBound = !string.IsNullOrEmpty(Input.Value);

        if (isBound && !_wasBound) {
            InputCatcher.RegisterButtonBinding(this);
        }
        else if (isBound && _wasBound) {
            InputCatcher.ModifyButtonBinding(this);
        }
        else if (!isBound && _wasBound) {
            InputCatcher.UnregisterButtonBinding(this, clearLinkedEntries: true);
        }

        _wasBound = isBound;
    }

    protected override void DoReset() {
        base.DoReset();
        this.ModifiersString.BoxedValue = "";
        this._wasBound = false;
    }

    private bool _wasBound = false;
}

// For backward compatibility
public class RewiredInputConfig : RewiredButtonConfig {
    public RewiredInputConfig(ConfigFile config, string category, string featureName, string description, int order) :
        base(config, category, featureName, description, order)
    {}
}

public class RewiredModifierConfig : RewiredButtonConfigBase {
    public static List<RewiredModifierConfig> ModifierConfigs = [];

    public RewiredModifierConfig(ConfigFile config, string category, string featureName, string description, int order) :
        base(config, category, featureName, description, order) {
        // track initial state
        _wasBound = !string.IsNullOrEmpty(Input.Value);

        ModifierConfigs.Add(this);
    }

    public void Set(string controllerName, int buttonIndex, string buttonName) {
        base.Set(controllerName, buttonIndex);
        this.Input.BoxedValue = string.Format("{0} | {1} | {2}", controllerName, buttonName, buttonIndex);
    }

    protected override void DoOnSettingChanged() {
        bool isBound = !string.IsNullOrEmpty(Input.Value);

        if (isBound && !_wasBound) {
            InputCatcher.ModsTracker.AddModifierBinding(this);
            RewiredConfigManager.ModsTracker.AddModifierBinding(this);
        }
        else if (isBound && _wasBound) {
            InputCatcher.ModsTracker.ChangeModifierBinding(this);
            RewiredConfigManager.ModsTracker.ChangeModifierBinding(this);
        }
        else if (!isBound && _wasBound) {
            InputCatcher.ModsTracker.RemoveModifierBinding(this);
            RewiredConfigManager.ModsTracker.RemoveModifierBinding(this);
        }

        _wasBound = isBound;
    }

    protected override void DoReset() {
        base.DoReset();
        this._wasBound = false;
    }

    private bool _wasBound = false;
}

public class RewiredAxisConfig : RewiredConfigBase {
    public static List<RewiredAxisConfig> AxisConfigs = [];

    public ConfigEntry<int> AxisIndex { get; private set; }
    public ConfigEntry<string> ModifiersString { get; private set; }

    public RewiredAxisConfig(ConfigFile config, string category, string featureName, string description, int order) :
        base(config, category, featureName, description, order) {
        AxisIndex = config.Bind(
            category,
            $"{featureName} - Axis Index",
            -2,
            new ConfigDescription(
                "Index of the axis",
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

        // track initial state
        _wasBound = !string.IsNullOrEmpty(Input.Value);

        AxisConfigs.Add(this);
    }

    public void Set(string controllerName, int axisIndex, string axisName, string modifiersString) {
        base.Set(controllerName);
        this.AxisIndex.BoxedValue = axisIndex;
        this.ModifiersString.BoxedValue = modifiersString;
        this.Input.BoxedValue = string.Format(
            "{0}{1}",
            modifiersString.Length > 0 ? modifiersString + " + " : "",
            string.Format("{0} | {1} | {2}", controllerName, axisName, axisIndex));
    }

    protected override void DoOnSettingChanged() {
        bool isBound = !string.IsNullOrEmpty(Input.Value);

        if (isBound && !_wasBound) {
            InputCatcher.RegisterAxisBinding(this);
        }
        else if (isBound && _wasBound) {
            InputCatcher.ModifyAxisBinding(this);
        }
        else if (!isBound && _wasBound) {
            InputCatcher.UnregisterAxisBinding(this, clearLinkedEntries: true);
        }

        _wasBound = isBound;
    }

    protected override void DoReset() {
        base.DoReset();
        this.AxisIndex.BoxedValue = -3;
        this.ModifiersString.BoxedValue = "";
        this._wasBound = false;
    }

    private bool _wasBound = false;
};

internal sealed class RewiredConfigManager {
    private static bool _isListeningForInput = false;
    private static RewiredConfigBase _targetConfig = null;
    private static string _errorMessage = null;
    private static float _errorTimer = 0f;

    public static ModifiersTracker ModsTracker = new ();

    public static void Reset() {
        _isListeningForInput = false;
        _targetConfig = null;
        _errorMessage = null;
        _errorTimer = 0f;
    }

    public static bool ShouldQuit() {
        foreach (var controller in ReInput.controllers.Controllers) {
            if (!(controller.type == ControllerType.Keyboard && controller.GetAnyButtonDown()))
                continue;

            string controllerName = controller.name.Trim();

            IList<Rewired.Controller.Button> buttons = controller.Buttons;
            for (int buttonIndex = 0; buttonIndex < controller.buttonCount; buttonIndex++) {
                if (!controller.GetButtonDown(buttonIndex))
                    continue;
                string lowerName = buttons[buttonIndex].elementIdentifier.name.ToLower();
                switch (lowerName) {
                    case "escape":
                    case "esc":
                        Reset();
                        return true;
                    case "delete":
                    case "backspace":
                    case "suppr":
                    case "del":
                        if (_targetConfig != null)
                            _targetConfig.Reset();
                        Reset();
                        return true;
                }
            }
        }
        return false;
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

        if (ShouldQuit())
            return;

        if (_targetConfig as RewiredButtonConfig != null) ProcessButtonConfig();
        else if (_targetConfig as RewiredModifierConfig != null) ProcessModifierConfig();
        else if (_targetConfig as RewiredAxisConfig != null) ProcessAxisConfig();
    }

    public static void ProcessButtonConfig() {
        var targetConfig = _targetConfig as RewiredButtonConfig;
        Debug.Assert(targetConfig != null);

        string activeModifiersString = ModifierUtils.ToString(ModsTracker.GetModifiers(activeOnly: true));

        foreach (var controller in ReInput.controllers.Controllers) {
            if (!controller.GetAnyButtonDown())
                continue;

            string controllerName = controller.name.Trim();

            IList<Rewired.Controller.Button> buttons = controller.Buttons;
            for (int buttonIndex = 0; buttonIndex < controller.buttonCount; buttonIndex++) {
                if (!controller.GetButtonDown(buttonIndex))
                    continue;

                // Don't bind key as functional key if it is already registered as modifier
                if(ModsTracker.HasModifier(controllerName, buttonIndex)) {
                    _errorMessage = activeModifiersString;
                    _errorTimer = 3f;
                    return;
                }

                // Conflict check
                foreach (var config in RewiredButtonConfig.ButtonConfigs) {
                    if (config == _targetConfig)
                        continue;
                    bool matchesAnotherFunctionalKey =
                        config.ControllerName.Value == controllerName &&
                        config.ButtonIndex.Value == buttonIndex &&
                        config.ModifiersString.Value == activeModifiersString;
                    if (matchesAnotherFunctionalKey) {
                        string conflictName = config.Input.Definition.Key;
                        if (conflictName.EndsWith(" - Input")) conflictName = conflictName.Substring(0, conflictName.Length - 8);
                        _errorMessage = $"Conflict: {conflictName}";
                        _errorTimer = 3f;
                        return;
                    }
                }

                string buttonName = buttons[buttonIndex].elementIdentifier.name;

                Debug.Assert(targetConfig != null);
                targetConfig.Set(controllerName, buttonIndex, buttonName, activeModifiersString);
                Reset();
                return;
            }
        }
    }

    public static void ProcessModifierConfig() {
        var targetConfig = _targetConfig as RewiredModifierConfig;
        Debug.Assert(targetConfig != null);

        foreach (var controller in ReInput.controllers.Controllers) {
            if (!controller.GetAnyButtonDown())
                continue;

            string controllerName = controller.name.Trim();

            IList<Rewired.Controller.Button> buttons = controller.Buttons;
            for (int buttonIndex = 0; buttonIndex < controller.buttonCount; buttonIndex++) {
                if (!controller.GetButtonDown(buttonIndex))
                    continue;

                if(ModsTracker.HasModifier(controllerName, buttonIndex)) {
                    _errorMessage = $"Conflict: {new Modifier (controllerName, buttonIndex)}";
                    _errorTimer = 3f;
                    return;
                }

                foreach (var config in RewiredButtonConfig.ButtonConfigs) {
                    bool matchesAnotherFunctionalKey =
                        config.ControllerName.Value == controllerName &&
                        config.ButtonIndex.Value == buttonIndex;
                    if (matchesAnotherFunctionalKey) {
                        string conflictName = config.Input.Definition.Key;
                        if (conflictName.EndsWith(" - Input")) conflictName = conflictName.Substring(0, conflictName.Length - 8);
                        _errorMessage = $"Conflict: {conflictName}";
                        _errorTimer = 3f;
                        return;
                    }
                }

                string buttonName = buttons[buttonIndex].elementIdentifier.name;

                Debug.Assert(targetConfig != null);
                targetConfig.Set(controllerName, buttonIndex, buttonName);
                Reset();
                return;
            }
        }
    }

    public static void ProcessAxisConfig() {
        var targetConfig = _targetConfig as RewiredAxisConfig;
        Debug.Assert(targetConfig != null);

        string activeModifiersString = ModifierUtils.ToString(ModsTracker.GetModifiers(activeOnly: true));

        (var controller, var axis, var _) = PickAxis(0.5f);
        if (axis == null) {
            _errorMessage = activeModifiersString.Length != 0 ? activeModifiersString : null;
            return;
        }

        string controllerName = controller.name.Trim();
        int axisId = axis.id;
        string axisName = axis.elementIdentifier.name;

        // Conflict check
        foreach (var config in RewiredAxisConfig.AxisConfigs) {
            if (config == _targetConfig)
                continue;
            bool matchesAnotherAxisBinding =
                config.ControllerName.Value == controllerName &&
                config.AxisIndex.Value == axisId &&
                config.ModifiersString.Value == activeModifiersString;
            if (matchesAnotherAxisBinding) {
                string conflictName = config.Input.Definition.Key;
                if (conflictName.EndsWith(" - Input")) conflictName = conflictName.Substring(0, conflictName.Length - 8);
                _errorMessage = $"Conflict: {conflictName}";
                _errorTimer = 3f;
                _axes.Clear();
                return;
            }
        }

        Debug.Assert(targetConfig != null);
        targetConfig.Set(controllerName, axisId, axisName, activeModifiersString);
        Reset();
        _axes.Clear();
        return;
    }

    public static void RewiredButtonDrawer(ConfigEntryBase entry) {
        if (_isListeningForInput && _targetConfig.Input == entry) {
            GUIUtility.keyboardControl = 0;
            string label = string.IsNullOrEmpty(_errorMessage) ? "Listening... (ESC to cancel or Suppr to unbind)" : _errorMessage;
            if (GUILayout.Button(label, GUILayout.ExpandWidth(true))) {
                Reset();
            }
        }
        else {
            string val = (string)entry.BoxedValue;
            if (string.IsNullOrEmpty(val)) val = "None - Click to bind";
            if (GUILayout.Button(val, GUILayout.ExpandWidth(true))) {
                _isListeningForInput = true;
                _errorMessage = null;
                _errorTimer = 0f;

                // lookup of the linked facultative entries
                ConfigurationManagerAttributes attr = entry.Description.Tags?.OfType<ConfigurationManagerAttributes>().FirstOrDefault();
                _targetConfig = attr?.Config as RewiredConfigBase;
            }
        }
    }

    private static Dictionary<Controller, Dictionary<Controller.Axis, float>> _axes = new ();

    private static (Rewired.Controller, Rewired.Controller.Axis, float) PickAxis(float threshold) {
        foreach (var controller in ReInput.controllers.Controllers) {
            if (!_axes.TryGetValue(controller, out var controllerAxes)) {
                controllerAxes = new Dictionary<Controller.Axis, float> ();
                _axes[controller] = controllerAxes;
            }

            IList<Rewired.Controller.Element> elements = controller.Elements;
            foreach (var element in elements) {
                var axis = element as Rewired.Controller.Axis;
                if (axis == null)
                    continue;
                float valueDelta = 0;
                controllerAxes.TryGetValue(axis, out valueDelta);
                //Plugin.Log($"PickAxis(): controller:{controller.name}; axis:{axis.elementIdentifier.name}; axis.valueDelta:{axis.valueDelta}; axis.valueDeltaRaw:{axis.valueDeltaRaw}; axis.pollingDeadZone:{axis.pollingDeadZone}; valueDelta:{valueDelta}");
                valueDelta += axis.valueDelta;
                if (Mathf.Abs(valueDelta) > threshold)
                    return (controller, axis, valueDelta > 0 ? 1 : -1);
                else
                    controllerAxes[axis] = valueDelta;
            }
        }
        return (null, null, 0);
    }
}
