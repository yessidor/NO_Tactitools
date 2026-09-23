using BepInEx.Configuration;
using UnityEngine;
using Rewired;
using HarmonyLib;
using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NO_Tactitools.Controls;

namespace NO_Tactitools.Core;

internal sealed class ConfigurationManagerAttributes
/// <summary>
/// Class that can be used to customize how a setting is displayed in the configuration manager window.
/// </summary>
{
    public bool? IsAdvanced = null;
    public bool? Browsable = null;
    public string Category = null;
    public Action<ConfigEntryBase> CustomDrawer = null;
    public string DispName = null;
    public int? Order = null;
    public bool? ReadOnly = null;
    public bool? HideDefaultButton = null;
    public bool? HideSettingName = null;
    public object Config = null;
    public bool? ShowRangeAsPercent = null;
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
    public ConfigEntry<bool> ExactModifiers { get; private set; }

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

        ExactModifiers = config.Bind(
            category,
            $"{featureName} - Exact Modifiers",
            true,
            new ConfigDescription(
                "Should modifiers of the button be matched exactly",
                null,
                new ConfigurationManagerAttributes { Browsable = false }));

        // track initial state
        _wasBound = !string.IsNullOrEmpty(Input.Value);

        ButtonConfigs.Add(this);
    }

    public void Set(string controllerName, int buttonIndex, string buttonName, string modifiersString, bool exactModifiers = true) {
        base.Set(controllerName, buttonIndex);
        this.ModifiersString.BoxedValue = modifiersString;
        this.ExactModifiers.BoxedValue = exactModifiers;
        this.Input.BoxedValue = string.Format(
            "{0}{1}{2}",
            exactModifiers ? "" : "~",
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
        this.ExactModifiers.BoxedValue = true;
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
    public ConfigEntry<int> AxisDirection { get; private set; }
    public ConfigEntry<string> ModifiersString { get; private set; }
    public ConfigEntry<bool> ExactModifiers { get; private set; }

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

        AxisDirection = config.Bind(
            category,
            $"{featureName} - Axis Direction",
            1,
            new ConfigDescription(
                "Direction of the axis",
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

        ExactModifiers = config.Bind(
            category,
            $"{featureName} - Exact Modifiers",
            true,
            new ConfigDescription(
                "Should modifiers of the axis be matched exactly",
                null,
                new ConfigurationManagerAttributes { Browsable = false }));

        // track initial state
        _wasBound = !string.IsNullOrEmpty(Input.Value);

        AxisConfigs.Add(this);
    }

    public void Set(string controllerName, int axisIndex, string axisName, int axisDirection, string modifiersString, bool exactModifiers = true) {
        base.Set(controllerName);
        this.AxisIndex.BoxedValue = axisIndex;
        this.AxisDirection.BoxedValue = axisDirection;
        this.ModifiersString.BoxedValue = modifiersString;
        this.ExactModifiers.BoxedValue = exactModifiers;
        this.Input.BoxedValue = string.Format(
            "{0}{1}{2}",
            exactModifiers ? "" : "~",
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
        this.ExactModifiers.BoxedValue = true;
        this._wasBound = false;
    }

    private bool _wasBound = false;
};

internal sealed class RewiredConfigManager {
    private static bool _isListeningForInput = false;
    private static bool _exactModifiers = true;
    private static RewiredConfigBase _targetConfig = null;
    private static string _errorMessage = null;
    private static float _errorTimer = 0f;
    private enum Conflict { None, Detected, Ignore };
    private static Conflict _conflict = Conflict.None;
    private class ConflictData {};
    private class ButtonConflictData : ConflictData {
        public RewiredButtonConfig Config;
        public string ButtonName;
        public ButtonConflictData(RewiredButtonConfig config, string buttonName) {
            this.Config = config;
            this.ButtonName = buttonName;
        }
    };
    private class AxisConflictData : ConflictData {
        public RewiredAxisConfig Config;
        public string AxisName;
        public int AxisDirection;
        public AxisConflictData(RewiredAxisConfig config, string axisName, int axisDirection) {
            this.Config = config;
            this.AxisName = axisName;
            this.AxisDirection = axisDirection;
        }
    };
    private static ConflictData _conflictData = null;
    private static ConfigEntryBase _expandedEntry = null;

    public static ModifiersTracker ModsTracker = new ();

    public static void Reset() {
        _isListeningForInput = false;
        _exactModifiers = true;
        _targetConfig = null;
        _errorMessage = null;
        _errorTimer = 0f;
        _conflict = Conflict.None;
        _conflictData = null;
        _expandedEntry = null;
    }

    public static bool ShouldQuit() {
        if (!_isListeningForInput || ReInput.controllers == null)
            return true;

        //Update modifiers
        foreach (var controller in ReInput.controllers.Controllers) {
            ModsTracker.UpdateModifiersState(controller);
        }

        foreach (var controller in ReInput.controllers.Controllers) {
            string controllerName = controller.name.Trim();

            if (!(controller.type == ControllerType.Keyboard && controller.GetAnyButtonDown()))
                continue;

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

        if (ShouldQuit())
            return;

        if (_targetConfig is RewiredButtonConfig) ProcessButtonConfig();
        else if (_targetConfig is RewiredModifierConfig) ProcessModifierConfig();
        else if (_targetConfig is RewiredAxisConfig) ProcessAxisConfig();
    }

    public static void ProcessButtonConfig() {
        var targetConfig = _targetConfig as RewiredButtonConfig;
        Debug.Assert(targetConfig != null);

        string controllerName = "";
        int buttonIndex = -1;
        string buttonName = "";
        string activeModifiersString = ModifierUtils.ToString(ModsTracker.GetModifiers(activeOnly: true));

        if (_conflict == Conflict.Ignore) {
            var conflictData = _conflictData as ButtonConflictData;
            if (conflictData == null)
                throw new Exception ("Expected ButtonConflictData");
            var config = conflictData.Config;
            controllerName = config.ControllerName.Value;
            buttonIndex = config.ButtonIndex.Value;
            buttonName = conflictData.ButtonName;
            activeModifiersString = config.ModifiersString.Value;
            _exactModifiers = config.ExactModifiers.Value;
        }
        else if (_conflict == Conflict.None) {
            foreach (var controller in ReInput.controllers.Controllers) {
                if (!controller.GetAnyButtonDown())
                    continue;

                IList<Rewired.Controller.Button> buttons = controller.Buttons;

                controllerName = controller.name.Trim();

                for (buttonIndex = 0; buttonIndex < controller.buttonCount; buttonIndex++) {
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
                        buttonName = buttons[buttonIndex].elementIdentifier.name;
                        bool matchesAnotherFunctionalKey =
                            config.ControllerName.Value == controllerName &&
                            config.ButtonIndex.Value == buttonIndex &&
                            config.ModifiersString.Value == activeModifiersString;
                        if (matchesAnotherFunctionalKey) {
                            string conflictName = config.Input.Definition.Key;
                            if (conflictName.EndsWith(" - Input")) conflictName = conflictName.Substring(0, conflictName.Length - 8);
                            _conflict = Conflict.Detected;
                            _errorMessage = $"Conflict: {conflictName}";
                            _errorTimer = 3f;
                            _conflict = Conflict.Detected;
                            _conflictData = new ButtonConflictData (config, buttonName);
                            return;
                        }
                    }

                    break;
                }
            }
        }

        if (buttonIndex != -1) {
            targetConfig.Set(controllerName, buttonIndex, buttonName, activeModifiersString, _exactModifiers);
            Reset();
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

        string controllerName = "";
        int axisId = -1;
        string axisName = "";
        int axisDirection = 0;
        string activeModifiersString = "";

        if (_conflict == Conflict.Ignore) {
            var conflictData = _conflictData as AxisConflictData;
            if (conflictData == null)
                throw new Exception ("Expected AxisConflictData");
            var config = conflictData.Config;
            controllerName = config.ControllerName.Value;
            axisId = config.AxisIndex.Value;
            axisName = conflictData.AxisName;
            axisDirection = conflictData.AxisDirection;
            activeModifiersString = config.ModifiersString.Value;
            _exactModifiers = config.ExactModifiers.Value;
        }
        else if (_conflict == Conflict.None) {
            activeModifiersString = ModifierUtils.ToString(ModsTracker.GetModifiers(activeOnly: true));

            (var controller, var axis, axisDirection) = PickAxis(0.5f);
            if (axis == null) {
                _errorMessage = activeModifiersString.Length != 0 ? activeModifiersString : null;
                return;
            }

            controllerName = controller.name.Trim();
            axisId = axis.id;
            var elementIdentifier = axis.elementIdentifier;
            axisName = axisDirection > 0 ? elementIdentifier.positiveName : axisDirection < 0 ? elementIdentifier.negativeName : elementIdentifier.name;

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
                    _conflict = Conflict.Detected;
                    _conflictData = new AxisConflictData (config, axisName, axisDirection);
                    _axes.Clear();
                    return;
                }
            }
        }

        if (axisId != -1) {
            targetConfig.Set(controllerName, axisId, axisName, axisDirection, activeModifiersString, _exactModifiers);
            Reset();
            _axes.Clear();
        }
    }

    public static void RewiredButtonDrawer(ConfigEntryBase entry) {
        var exactModifiersContent = new GUIContent("Match modifiers exactly", "If disabled, extra modifiers pressed won't cause mismatch. Rebind to apply changes.");

        if (_isListeningForInput && _targetConfig.Input == entry) {
            GUIUtility.keyboardControl = -1;
            GUILayout.BeginVertical();
            string label = string.IsNullOrEmpty(_errorMessage) ? "Listening... (ESC to cancel or Suppr to unbind)" : _errorMessage;
            if (GUILayout.Button(label, GUILayout.ExpandWidth(true)))
                Reset();
            if (_conflict == Conflict.Detected) {
                if(GUILayout.Button(new GUIContent("Bind anyway", "Allow conflicting binds"), GUILayout.ExpandWidth(true)))
                    _conflict = Conflict.Ignore;
                if (!(_targetConfig is RewiredModifierConfig))
                    _exactModifiers = GUILayout.Toggle(_exactModifiers, exactModifiersContent, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndVertical();
        }
        else {
            string val = (string)entry.BoxedValue;
            if (string.IsNullOrEmpty(val))
                val = "None";

            GUILayout.BeginVertical();
            if (GUILayout.Button(val, GUILayout.ExpandWidth(true))) {
                _expandedEntry = _expandedEntry == null || _expandedEntry != entry ? entry : null;
                if (_expandedEntry != null) {
                    // lookup of the linked facultative entries
                    ConfigurationManagerAttributes attr = entry.Description.Tags?.OfType<ConfigurationManagerAttributes>().FirstOrDefault();
                    _targetConfig = attr?.Config as RewiredConfigBase;
                    if (_targetConfig is RewiredButtonConfig buttonConfig)
                        _exactModifiers = buttonConfig.ExactModifiers.Value;
                    else if (_targetConfig is RewiredAxisConfig axisConfig)
                        _exactModifiers = axisConfig.ExactModifiers.Value;
                }
            }
            if (_expandedEntry == entry) {
                if (GUILayout.Button("Bind", GUILayout.ExpandWidth(true))) {
                    _isListeningForInput = true;
                    _errorMessage = null;
                    _errorTimer = 0f;
                }
                if (!(_targetConfig is RewiredModifierConfig)) {
                    _exactModifiers = GUILayout.Toggle(_exactModifiers, exactModifiersContent, GUILayout.ExpandWidth(true));
                }
            }
            GUILayout.EndVertical();
        }
    }

    private static Dictionary<Controller, Dictionary<Controller.Axis, float>> _axes = new ();

    private static (Rewired.Controller, Rewired.Controller.Axis, int) PickAxis(float threshold) {
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

public class RegexEntries {
    public string Entries {
        get;
        set {
            field = value;
            needToUpdate = true;
        }
    } = "";

    public string Separator {
        get;
        set {
            field = value;
            needToUpdate = true;
        }
    } = ";";

    public bool Matches(string s) {
        if (needToUpdate) {
            regexes = Entries.Split(Separator).Where(ss => ss.Length > 0).Select(ss => new Regex(ss)).ToArray();
            needToUpdate = false;
        }
        foreach (var regex in regexes) {
            Match m = regex.Match(s);
            //Plugin.Log($"[RegexEntries] Matching {s} to {regex}; successfull: {m.Success}");
            if (m.Success)
                return true;
        }
        return false;
    }

    private Regex[] regexes;
    private bool needToUpdate = true;
}

public class AxesDrawer<T> {
    public AxesDrawer(string fmt) {
        this.fmt = fmt;
    }

    public void DrawAxes(ConfigEntryBase entry) {
        var axes = entry.BoxedValue as Axes<T>;
        if (axes == null)
            return;

        var axesString = new Axes<string> (
            (axes.Yaw as IFormattable)?.ToString(fmt, CultureInfo.InvariantCulture) ?? axes.Yaw.ToString(),
            (axes.Pitch as IFormattable)?.ToString(fmt, CultureInfo.InvariantCulture) ?? axes.Pitch.ToString(),
            (axes.Roll as IFormattable)?.ToString(fmt, CultureInfo.InvariantCulture) ?? axes.Roll.ToString());
        var axesStringCopy = new Axes<string> (axesString);

        // Yaw Input
        GUILayout.Label("Yaw:", GUILayout.ExpandWidth(true));
        axesString.Yaw = GUILayout.TextField(axesString.Yaw, GUILayout.ExpandWidth(true));

        // Pitch Input
        GUILayout.Label("Pitch:", GUILayout.ExpandWidth(true));
        axesString.Pitch = GUILayout.TextField(axesString.Pitch, GUILayout.ExpandWidth(true));

        // Roll Input
        GUILayout.Label("Roll:", GUILayout.ExpandWidth(true));
        axesString.Roll = GUILayout.TextField(axesString.Roll, GUILayout.ExpandWidth(true));

        if (axesString != axesStringCopy) {
            try {
                var typeOfT = typeof(T);
                var yaw = (T)Convert.ChangeType(axesString.Yaw, typeOfT, CultureInfo.InvariantCulture);
                var pitch = (T)Convert.ChangeType(axesString.Pitch, typeOfT, CultureInfo.InvariantCulture);
                var roll = (T)Convert.ChangeType(axesString.Roll, typeOfT, CultureInfo.InvariantCulture);
                entry.BoxedValue = new Axes<T>(yaw, pitch, roll);
            } catch (Exception ex) {
               Plugin.Log($"Failed to parse Axes values: {ex.Message}");
            }
        }
    }

    private string fmt;
}

public class ScrollableTextFieldDrawer {
    public static Action<ConfigEntryBase> CreateDelegate(int height = 50) {
        var drawer = new ScrollableTextFieldDrawer { height = height };
        return (ConfigEntryBase entry) => drawer.Draw(entry);
    }

    public void Draw(ConfigEntryBase entry) {
        var text = entry.BoxedValue as string;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(height));
        var newText = GUILayout.TextField(text, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
        if (text != newText) {
            entry.BoxedValue = newText;
        }
    }

    private Vector2 scrollPosition = Vector2.zero;
    private int height = 50;
}

public class RadialMenuEnableDrawer {
    public static Action<ConfigEntryBase> CreateDelegate() {
        var drawer = new RadialMenuEnableDrawer ();
        return (ConfigEntryBase entry) => drawer.Draw(entry);
    }

    public void Draw(ConfigEntryBase entry) {
        GUILayout.BeginVertical();
        bool state = (bool)entry.BoxedValue;
        bool newState = GUILayout.Toggle(state, state ? "Enabled" : "Disabled", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if (state != newState) {
            entry.BoxedValue = newState;
        }
        var buttonText = $"{(showHelp ? "Hide" : "Show")} help";
        if (GUILayout.Button(buttonText, GUILayout.ExpandWidth(true)))
            showHelp = !showHelp;
        if (showHelp)
            GUILayout.TextField(helpText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.EndVertical();
    }

    bool showHelp = false;

    private static string helpText = 
        """
        Available commands:
        Default - default menu. Doubleclicking 'Radial Menu' or 'Weapon Wheel' key resets corresponding menu to default.

        HUD Options:
        RememberHUDOptions(#) - Remember HUD options preset #
        RecallHUDOptions(#) - Recall HUD options preset #
        Number of presets is set by 'HUD Options Preset - Number' setting, so available preset numbers would be from 0 to Number-1.

        Target Filter Preset:
        RememberFilter(#) - Remember Target Filter preset #
        RecallFilter(#) - Recall Target Filter preset #
        Number of presets is set by 'Target Filter Preset - Number' setting.

        Target List Controller:
        RememberTargets(#) - Remember Target List #
        RecallTargets(#) - Recall Target List #
        Nubmer of lists is set by 'MFD Nav - Extra Key - Number' setting, 

        PopTarget - Pop current target
        KeepTarget - Keep current target
        NextTarget - Next target
        PrevTarget - Prev target
        KeepDatalinked - Keep datalinked targets
        KeepByAmmo - Keep closest targets based on ammo
        SortName - Sort targets by name
        SortDist - Sort targets by distance
        KeepTracked - Keep tracked targets
        PopTracked - Pop tracked targets
        KeepSameName - Keep targets with same name as current target
        PopSameName - Pop targets with same name as current target
        KeepLased - Keep lased targets
        PopLased - Pop lased targets
        SelectClosestTarget - Select closest target
        SelectClosestTargets - Select closest targets
        MTSToggle - MTS engage/disengage
        MTSAutoSelect - MTS AutoSelect on/off

        MiniMap Zoom:
        MiniMapZoomUp - Cycle MiniMap zoom up
        MiniMapZoomDown - Cycle MiniMap zoom down
        MiniMapZoomReset - Reset MiniMap zoom

        HMD Declutter:
        HMDMarkerDistanceUp - Cycle HMD marker distance up
        HMDMarkerDistanceDown - Cycle HMD marker distance down

        Target Cam Mode:
        TargetCamModeToggle - Toggle Target Cam mode

        Commands are distributed clockwise starting from top.
        """;
}
