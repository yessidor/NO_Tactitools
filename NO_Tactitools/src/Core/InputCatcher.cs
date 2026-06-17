using HarmonyLib;
using Rewired;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NO_Tactitools.Core;

public struct Modifier {
    public static readonly Modifier Invalid = new Modifier("", -1);

    public string ControllerName;
    public int ButtonIndex;    

    public bool IsValid() {
        return ButtonIndex > 0;
    }

    public Modifier(string controllerName, int buttonIndex) {
        this.ControllerName = controllerName;
        this.ButtonIndex = buttonIndex;
    }

    public override string ToString() {
        var buttonName = (string.IsNullOrEmpty(ControllerName) || ButtonIndex < 0) ? "" : ModifierUtils.GetButtonName(ControllerName, ButtonIndex);
        return string.Format("{0} | {1} | {2}", ControllerName, buttonName, ButtonIndex);
    }

    public static Modifier Parse(string modifierString) {
        var elements = modifierString.Split("|");
        if (elements.Length < 3) {
            Plugin.Log(string.Format("[IC] Invalid modifierString: {0}", modifierString));
            return Modifier.Invalid;
        }
        var modifier = new Modifier(elements[0].Trim(), int.Parse(elements[2].Trim()));
        return modifier;
    }
};

public class ModifierUtils {
    public static readonly string separator = " + ";

    public static string ToString(HashSet<Modifier> modifiers) {
        List<string> temp = modifiers.Select(modifier => modifier.ToString()).ToList();
        return string.Join(separator, temp);
    }

    public static HashSet<Modifier> FromString(string modifiers) {
        modifiers = modifiers.Trim();
        if (modifiers.Length == 0)
            return new HashSet<Modifier> ();
        List<string> modifierStrings = modifiers.Split(separator).ToList();
        var modifiersSet = new HashSet<Modifier> ();
        foreach (var modifierString in modifierStrings)
            try {
                var modifier = Modifier.Parse(modifierString);
                if (!modifier.IsValid()) {
                    Plugin.Log(string.Format("[IC] Invalid modifier string: {0}", modifierString));
                    continue;
                }
                modifiersSet.Add(modifier);
            }
            catch (Exception e) {
                Plugin.Log(string.Format("[IC] Got exception: {0}", e));
            }
        return modifiersSet;
    }

    public static string GetButtonName(string controllerName, int buttonIndex) {
        Controller controller = GetController(controllerName);
        if (controller == null)
            return "";

        var buttons = controller.Buttons;
        if (buttonIndex < 0 || buttonIndex >= buttons.Count) {
            Plugin.Log(string.Format("[IC] Invalid button index {0} for controller {1}", buttonIndex, controllerName));
            return "";
        }
        string buttonName = buttons[buttonIndex].elementIdentifier.name;
        return buttonName;
    }

    public static int GetButtonIndex(string controllerName, string buttonName) {
        Controller controller = GetController(controllerName);
        if (controller == null)
            return -5;

        var buttons = controller.Buttons;
        for (int buttonIndex = 0; buttonIndex < buttons.Count; buttonIndex++)
            if (buttons[buttonIndex].name == buttonName)
                return buttonIndex;
        return -6;
    }

    private static Controller cachedController;

    //Relies on ReInput.controllers.Controllers
    //Accessing it on init stage will spam config with "Rewired is not initialized" messages,
    //but this avoids depending on InputCatcher
    private static Controller GetController(string controllerName) {
        Controller controller = null;

        if (cachedController != null && cachedController.name.Trim() == controllerName)
            controller = cachedController;
        else if (ReInput.controllers != null && ReInput.controllers.Controllers != null) {
            foreach (var cont in ReInput.controllers.Controllers)
                if (cont.name == controllerName) {
                    controller = cachedController = cont;
                    break;
                }
            if (controller == null) {
                Plugin.Log(string.Format("[IC] Cannot find controller {0}", controllerName ?? "null"));
            }
        }

        return controller;
    }
};

public class ModifiersTracker {
    public void SetModifiers(HashSet<Modifier> modifiers) {
        modifiersData.Clear();
        foreach (var modifier in modifiers) {
            AddModifier(modifier);
        }
    }

    public void AddModifier(Modifier modifier) {
        if (!modifiersData.TryGetValue(modifier.ControllerName, out var controllerModifiers)) {
            controllerModifiers = new Dictionary<int, bool> ();
            modifiersData[modifier.ControllerName] = controllerModifiers;
        }
        controllerModifiers[modifier.ButtonIndex] = false;
    }

    public void RemoveModifier(Modifier modifier) {
        if (modifiersData.TryGetValue(modifier.ControllerName, out var controllerModifiers)) {
            if (controllerModifiers.TryGetValue(modifier.ButtonIndex, out var _))
                controllerModifiers.Remove(modifier.ButtonIndex);
            if (controllerModifiers.Count == 0)
                modifiersData.Remove(modifier.ControllerName);
        }
    }

    public bool HasModifier(string controllerName, int buttonIndex) {
        return modifiersData.TryGetValue(controllerName, out var controllerModifiers) && controllerModifiers.TryGetValue(buttonIndex, out var _);
    }

    public bool HasModifier(Modifier modifier) {
        return HasModifier(modifier.ControllerName, modifier.ButtonIndex);
    }

    public HashSet<Modifier> GetModifiers(bool activeOnly = false) {
        HashSet<Modifier> result = new ();
        foreach ((var controllerName, var controllerModifiers) in modifiersData)
            foreach ((var buttonIndex, var buttonState) in controllerModifiers)
                if (!activeOnly || buttonState)
                    result.Add(new Modifier(controllerName, buttonIndex));
        return result;
    }

    public void AddModifierBinding(RewiredModifierConfig config) {
        var controllerName = config.ControllerName.Value == null ? "" : config.ControllerName.Value;
        var buttonIndex = config.ButtonIndex.Value;
        var modifier = new Modifier (controllerName, buttonIndex);
        if (!modifier.IsValid()) {
            Plugin.Log(string.Format("[IC] Skipped invalid modifier {0}", modifier));
            return;
        }
        AddModifier(modifier);
        allModifiers[config] = modifier;
    }

    public void RemoveModifierBinding(RewiredModifierConfig config) {
        if (!allModifiers.TryGetValue(config, out var modifier))
            return;
        RemoveModifier(modifier);
        allModifiers.Remove(config);
    }

    public void ChangeModifierBinding(RewiredModifierConfig config) {
        RemoveModifierBinding(config);
        AddModifierBinding(config);
    }

    public void UpdateModifiersState(Controller controller) {
        //Update modifiers
        var controllerButtons = controller.Buttons;
        var controllerName = controller.name.Trim();
        if (modifiersData.TryGetValue(controllerName, out var controllerModifiers)) {
            foreach (int buttonIndex in controllerModifiers.Keys.ToList()) {
                try {
                    var buttonState = controllerButtons[buttonIndex].value;
                    controllerModifiers[buttonIndex] = buttonState;
                }
                catch (ArgumentOutOfRangeException e) {
                    Plugin.Log(string.Format("[IC] Got exception: {0}", e));
                    continue;
                }
            }
        }
    }

    private Dictionary<RewiredModifierConfig, Modifier> allModifiers = new ();
    private Dictionary<string, Dictionary<int, bool>> modifiersData = new ();
};

public class InputCatcher {
    public static ModifiersTracker ModsTracker = new();

    public static void Init(Harmony harmony) {
        harmony.PatchAll(typeof(RegisterControllerPatch));
        harmony.PatchAll(typeof(UnregisterControllerPatch));
        harmony.PatchAll(typeof(ControllerInputInterceptionPatch));
    }

    public static void RegisterButtonInput(
        RewiredButtonConfig config,
        float longPressThreshold = 0.2f,
        System.Action onPress = null,
        System.Action onRelease = null,
        System.Action onHold = null,
        System.Action onLongPress = null,
        System.Action onShortPress = null,
        System.Action onReleased = null
        ) {
        
        ButtonRegistration reg = new() {
            config = config,
            longPressThreshold = longPressThreshold,
            onPress = onPress,
            onShortPress = onShortPress != null ? onShortPress : onRelease,
            onHold = onHold,
            onLongPress = onLongPress,
            onReleased = onReleased
        };
        buttonRegistrations.Add(reg);

        string controllerName = config.ControllerName.Value.Trim();
        int buttonIndex = config.ButtonIndex.Value;
        if (controllerName == "") {
            Plugin.Log("[IC] No controller name provided for button registration. Skipping.");
            return;
        }
        else if (buttonIndex < 0) {
            Plugin.Log("[IC] No input code string provided for button registration. Skipping.");
            return;
        }

        HashSet<Modifier> modifiers = ModifierUtils.FromString(config.ModifiersString.Value);

        TryRegisterButtonInputOrQueue(reg, controllerName, buttonIndex, modifiers);
    }

    //For backward compatibility
    //Have to maintain onRelease as action for short press for backward compatibility
    public static void RegisterNewInput(
        RewiredButtonConfig config,
        float longPressThreshold = 0.2f,
        System.Action onPress = null,
        System.Action onRelease = null,
        System.Action onHold = null,
        System.Action onLongPress = null,
        System.Action onShortPress = null,
        System.Action onReleased = null
        ) {
        RegisterButtonInput(config, longPressThreshold, onPress, onRelease, onHold, onLongPress, onShortPress, onReleased);
    }

    public static void RegisterButtonBinding(RewiredButtonConfig config) {
        string controllerName = config.ControllerName.Value.Trim();
        int buttonIndex = config.ButtonIndex.Value;
        HashSet<Modifier> modifiers = ModifierUtils.FromString(config.ModifiersString.Value);

        if (controllerName == "" || buttonIndex < 0) {
            Plugin.Log(
                string.Format(
                    "[IC] Cannot register button {0} with modifiers {1} constructed from string {2}",
                    new Modifier(controllerName, buttonIndex),
                    ModifierUtils.ToString(modifiers),
                    config.ModifiersString.Value
                )
            );
            return;
        }

        foreach (var reg in buttonRegistrations.Where(r => r.config == config)) {
            TryRegisterButtonInputOrQueue(reg, controllerName, buttonIndex, modifiers);
        }
    }
    
    public static void ModifyButtonBinding(RewiredButtonConfig config) {
        //Since clearLinkedEntries is false, config entries won't be modified, and OnSettingChanged() won't be called
        UnregisterButtonBinding(config, clearLinkedEntries: false);
        RegisterButtonBinding(config);
    }

    public static void UnregisterButtonBinding(RewiredButtonConfig config, bool clearLinkedEntries = true) {
        foreach (Controller controller in buttonInputs.Keys) {
            int removed = buttonInputs[controller].RemoveAll(input => input.registration.config == config);
            if (removed > 0) {
                Plugin.Log($"[IC] Unregistered {removed} button input(s) for config {config.Input.Definition.Key}");
            }
        }

        // Also remove from pending inputs
        foreach (string controllerName in pendingInputs.Keys.ToList()) {
            int removed = pendingInputs[controllerName].RemoveAll(p => { var pp = p as ButtonInput; return pp != null && pp.registration.config == config; });
            if (removed > 0) {
                Plugin.Log($"[IC] Removed {removed} pending button input(s) for config {config.Input.Definition.Key}");
            }
        }

        if (clearLinkedEntries) {
            // also clear the linked entries
            config.Reset(execOnSettingChanged: false);
        }
    }

    public static void RegisterAxisInput(
        RewiredAxisConfig config,
        System.Action<float, float, float> onMove = null,
        System.Action<float, float, float> onMoveRaw = null
        ) {
        
        AxisRegistration reg = new() {
            config = config,
            onMove = onMove,
            onMoveRaw = onMoveRaw
        };
        axisRegistrations.Add(reg);

        string controllerName = config.ControllerName.Value.Trim();
        int axisIndex = config.AxisIndex.Value;
        if (controllerName == "") {
            Plugin.Log("[IC] No controller name provided for axis registration. Skipping.");
            return;
        }
        else if (axisIndex < 0) {
            Plugin.Log("[IC] No input code string provided for axis registration. Skipping.");
            return;
        }

        HashSet<Modifier> modifiers = ModifierUtils.FromString(config.ModifiersString.Value);

        TryRegisterAxisInputOrQueue(reg, controllerName, axisIndex, modifiers);
    }

    public static void RegisterAxisBinding(RewiredAxisConfig config) {
        string controllerName = config.ControllerName.Value.Trim();
        int axisIndex = config.AxisIndex.Value;
        HashSet<Modifier> modifiers = ModifierUtils.FromString(config.ModifiersString.Value);

        if (controllerName == "" || axisIndex < 0) {
            Plugin.Log(
                string.Format(
                    "[IC] Cannot register axis {0} with modifiers {1} constructed from string {2}",
                    new Modifier(controllerName, axisIndex),
                    ModifierUtils.ToString(modifiers),
                    config.ModifiersString.Value
                )
            );
            return;
        }

        foreach (var reg in axisRegistrations.Where(r => r.config == config)) {
            TryRegisterAxisInputOrQueue(reg, controllerName, axisIndex, modifiers);
        }
    }

    public static void ModifyAxisBinding(RewiredAxisConfig config) {
        //Since clearLinkedEntries is false, config entries won't be modified, and OnSettingChanged() won't be called
        UnregisterAxisBinding(config, clearLinkedEntries: false);
        RegisterAxisBinding(config);
    }

    public static void UnregisterAxisBinding(RewiredAxisConfig config, bool clearLinkedEntries = true) {
        foreach (Controller controller in axisInputs.Keys) {
            int removed = axisInputs[controller].RemoveAll(input => input.registration.config == config);
            if (removed > 0) {
                Plugin.Log($"[IC] Unregistered {removed} axis input(s) for config {config.Input.Definition.Key}");
            }
        }

        // Also remove from pending inputs
        foreach (string controllerName in pendingInputs.Keys.ToList()) {
            int removed = pendingInputs[controllerName].RemoveAll(p => { var pp = p as AxisInput; return pp != null && pp.registration.config == config; });
            if (removed > 0) {
                Plugin.Log($"[IC] Removed {removed} pending axis input(s) for config {config.Input.Definition.Key}");
            }
        }

        if (clearLinkedEntries) {
            // also clear the linked entries
            config.Reset(execOnSettingChanged: false);
        }
    }

    private abstract class Input {
        public abstract void OnController(Controller controller);
    }

    private class ButtonRegistration {
        public RewiredButtonConfig config;
        public float longPressThreshold;
        public System.Action onPress;
        public System.Action onShortPress;
        public System.Action onHold;
        public System.Action onLongPress;
        public System.Action onReleased;
    }

    private class ButtonInput : Input {
        public ButtonRegistration registration;
        public int buttonIndex;
        public HashSet<Modifier> modifiers;
        public bool currentButtonState;
        public bool previousButtonState;
        public float buttonPressTime;
        public bool longPressHandled;
        public bool holdLongHandled;

        public Controller.Button button;
        public override void OnController(Controller controller) {
            if (controller != null) {
                this.button = controller.Buttons[buttonIndex];
                this.currentButtonState = button.value;
                this.previousButtonState = this.currentButtonState;
            }
            else {
                this.button = null;
                this.previousButtonState = false;
                this.currentButtonState = false;
            }
        }

        public ButtonInput(ButtonRegistration registration, Controller controller, int buttonIndex, HashSet<Modifier> modifiers) {
            this.registration = registration;
            this.buttonIndex = buttonIndex;
            this.modifiers = modifiers;
            this.buttonPressTime = Time.time;
            this.longPressHandled = true; // Assume it's already handled if they're holding it down on registration
            this.holdLongHandled = true;
            this.OnController(controller);

            if (registration.onPress == null && registration.onShortPress == null && registration.onLongPress == null && registration.onHold == null) {
                Plugin.Logger.LogError($"[IC] No actions provided for button {buttonIndex}");
            }
            else {
                Plugin.Log($"[IC] Creating button input {buttonIndex} with actions");
            }
        }
    }

    private class AxisRegistration {
        public RewiredAxisConfig config;
        public System.Action<float, float, float> onMove;
        public System.Action<float, float, float> onMoveRaw;
    }

    private class AxisInput : Input {
        public AxisRegistration registration;
        public int axisIndex;
        public HashSet<Modifier> modifiers;

        public Controller.Axis axis;
        public override void OnController(Controller controller) {
            if (controller != null)
                foreach (var element in controller.Elements)
                    if (element.type == ControllerElementType.Axis && element.id == axisIndex) {
                        this.axis = (Controller.Axis)element;
                        break;
                    }
            else
                this.axis = null;
        }

        public AxisInput (AxisRegistration registration, Controller controller, int axisIndex, HashSet<Modifier> modifiers) {
            this.registration = registration;
            this.axisIndex = axisIndex;
            this.modifiers = modifiers;
            this.OnController(controller);

            if (registration.onMove == null && registration.onMoveRaw == null) {
                Plugin.Logger.LogError($"[IC] No actions provided for axis {axisIndex}");
            }
            else {
                Plugin.Log($"[IC] Creating axis input {axisIndex} with actions");
            }
        }
    }

    private static void TryRegisterButtonInputOrQueue(ButtonRegistration registration, string controllerName, int buttonIndex, HashSet<Modifier> modifiers) {
        ButtonInput buttonInput = new (registration, null, buttonIndex, modifiers);

        foreach (Controller controller in buttonInputs.Keys) {
            if (controller.name.Trim() != controllerName) continue;

            RegisterButtonInputNow(controller, buttonInput);
            return;
        }

        // controller not connected yet: queue as pending
        if (!pendingInputs.ContainsKey(controllerName))
            pendingInputs[controllerName] = [];

        pendingInputs[controllerName].Add(buttonInput);
        Plugin.Log($"[IC] Controller not connected, button input {buttonIndex} added to pending list for {controllerName}");
    }

    private static void RegisterButtonInputNow(Controller controller, ButtonInput buttonInput) {
        buttonInput.OnController(controller);

        if (!buttonInputs.TryGetValue(controller, out var controllerButtonInputs)) {
            controllerButtonInputs = [];
            buttonInputs[controller] = controllerButtonInputs;
            Plugin.Log($"[IC] Controller button structure initialized for: {controller.name.Trim()}");
        }

        controllerButtonInputs.Add(buttonInput);
    }

    private static void TryRegisterAxisInputOrQueue(AxisRegistration registration, string controllerName, int axisIndex, HashSet<Modifier> modifiers) {
        AxisInput axisInput = new (registration, null, axisIndex, modifiers);

        foreach (Controller controller in axisInputs.Keys) {
            if (controller.name.Trim() != controllerName) continue;

            RegisterAxisInputNow(controller, axisInput);
            return;
        }

        // controller not connected yet: queue as pending
        if (!pendingInputs.ContainsKey(controllerName))
            pendingInputs[controllerName] = [];

        pendingInputs[controllerName].Add(axisInput);
        Plugin.Log($"[IC] Controller not connected, axis input {axisIndex} added to pending list for {controllerName}");
    }

    private static void RegisterAxisInputNow(Controller controller, AxisInput axisInput) {
        axisInput.OnController(controller);

        if (!axisInputs.TryGetValue(controller, out var controllerAxisInputs)) {
            controllerAxisInputs = [];
            axisInputs[controller] = controllerAxisInputs;
            Plugin.Log($"[IC] Controller axis structure initialized for: {controller.name.Trim()}");
        }

        controllerAxisInputs.Add(axisInput);
    }

    private static IEnumerator RegisterPendingInputsRoutine(Controller controller, List<Input> pendingInputs) {
        yield return null;
        foreach (Input input in pendingInputs) {
            var buttonInput = input as ButtonInput;
            if (buttonInput != null) {
                RegisterButtonInputNow(controller, buttonInput);
                continue;
            }
            var axisInput = input as AxisInput;
            if (axisInput != null) {
                RegisterAxisInputNow(controller, axisInput);
                continue;
            }
            Plugin.Log($"[IC] Unexpected pending input type: {input}");
        }
    }

    private static List<ButtonRegistration> buttonRegistrations = [];
    private static Dictionary<Controller, List<ButtonInput>> buttonInputs = [];

    private static List<AxisRegistration> axisRegistrations = [];
    private static Dictionary<Controller, List<AxisInput>> axisInputs = [];

    private static Dictionary<string, List<Input>> pendingInputs = [];

    [HarmonyPatch(typeof(Rewired.Controller), "pBrAJYWOGkILyqjLrMpmCdajATI")]
    private class ControllerInputInterceptionPatch {
        static void Prefix(Controller __instance) {
            ModsTracker.UpdateModifiersState(__instance);
            HashSet<Modifier> activeModifiers = ModsTracker.GetModifiers(activeOnly: true);

            if (GameBindings.Player.Aircraft.GetAircraft(silent: true) == null || GameBindings.GameState.IsGamePaused()) {
                return;
            }

            foreach (Controller controller in buttonInputs.Keys) {
                var controllerName = controller.name.Trim();
                if (GameBindings.GameState.IsChatboxActive() && (controllerName == "Keyboard" || controllerName == "Mouse")) {
                    continue; // Don't process keyboard inputs if chatbox is active
                }
                if (__instance == controller) {
                    //Assuming modifier key won't be in buttonInputs[controller]
                    foreach (ButtonInput button in buttonInputs[controller]) {
                        try {
                            button.currentButtonState = button.button.value;
                            if (!button.previousButtonState && button.currentButtonState && button.modifiers.SetEquals(activeModifiers)) {
                                Plugin.Log(
                                    string.Format(
                                        "[IC] Press detected on button {0} with modifiers {1}",
                                        new Modifier(controllerName, button.buttonIndex),
                                        ModifierUtils.ToString(button.modifiers)));
                                // Button just pressed
                                button.buttonPressTime = Time.time;
                                button.longPressHandled = false;
                                button.holdLongHandled = false;
                                button.registration.onPress?.Invoke();
                                // button.previousButtonState should not be changed from false to true if modifiers don't match
                                // so setting button.previousButtonState is moved inside 'if' branches
                                button.previousButtonState = button.currentButtonState;
                            }
                            else if (button.previousButtonState && button.currentButtonState) {
                                // Button is being held down
                                float holdDuration = Time.time - button.buttonPressTime;
                                if (holdDuration >= button.registration.longPressThreshold && !button.longPressHandled && button.registration.onLongPress != null) {
                                    Plugin.Log(
                                        string.Format(
                                            "[IC] Long press detected on button {0} with modifiers {1}",
                                            new Modifier(controllerName, button.buttonIndex),
                                            ModifierUtils.ToString(button.modifiers)));
                                    button.registration.onLongPress?.Invoke();
                                    button.longPressHandled = true;
                                }
                                else if (holdDuration < button.registration.longPressThreshold && button.registration.onHold != null) {
                                    if (!button.holdLongHandled) {
                                        Plugin.Log(
                                            string.Format(
                                                "[IC] Hold detected on button {0} with modifiers {1}",
                                                new Modifier(controllerName, button.buttonIndex),
                                                ModifierUtils.ToString(button.modifiers)));
                                        button.holdLongHandled = true;
                                    }
                                    button.registration.onHold?.Invoke();
                                }
                                button.previousButtonState = button.currentButtonState;
                            }
                            else if (button.previousButtonState && !button.currentButtonState) {
                                if (button.registration.onReleased != null) {
                                    button.registration.onReleased?.Invoke();
                                }
                                if (!button.longPressHandled && button.registration.onShortPress != null) {
                                    // Button just released
                                    Plugin.Log(
                                        string.Format(
                                            "[IC] Short press detected on button {0} with modifiers {1}",
                                            new Modifier(controllerName, button.buttonIndex),
                                            ModifierUtils.ToString(button.modifiers)));
                                    button.registration.onShortPress?.Invoke();
                                }
                                button.previousButtonState = button.currentButtonState;
                            }
                        }
                        catch (ArgumentOutOfRangeException) {
                            Plugin.Log(
                                string.Format(
                                    "[IC] Error processing button {0} on controller {1}. Removing from registered inputs.",
                                    button.buttonIndex,
                                    __instance.name.Trim()));
                            buttonInputs[controller].Remove(button);
                        }
                    }
                }
            }

            foreach (Controller controller in axisInputs.Keys) {
                var controllerName = controller.name.Trim();
                if (GameBindings.GameState.IsChatboxActive() && (controllerName == "Mouse"))
                    continue;
                if (__instance == controller) {
                    foreach (AxisInput axisInput in axisInputs[controller]) {
                        var axis = axisInput.axis;
                        if (axis != null && axis.valueDelta != 0 && axisInput.modifiers.SetEquals(activeModifiers))
                            axisInput.registration.onMove?.Invoke(axis.value, axis.valuePrev, axis.valueDelta);
                            axisInput.registration.onMoveRaw?.Invoke(axis.valueRaw, axis.valueRawPrev, axis.valueDeltaRaw);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Rewired.Controller), "Connected")]
    private class RegisterControllerPatch {
        static void Postfix(Controller __instance) {
            string cleanedName = __instance.name.Trim();
            Plugin.Log($"[IC] Controller connected: {cleanedName}");

            if (pendingInputs.TryGetValue(cleanedName, out var pendingControllerInputs)) {
                Plugin.Instance.StartCoroutine(RegisterPendingInputsRoutine(__instance, pendingControllerInputs));
                pendingInputs.Remove(cleanedName);
            }
        }
    }

    [HarmonyPatch(typeof(Rewired.Controller), "Disconnected")]
    private class UnregisterControllerPatch {
        static void Prefix(Controller __instance) {
            string cleanedName = __instance.name.Trim();
            Plugin.Log($"[IC] Controller disconnected: {cleanedName}");

            if (!pendingInputs.TryGetValue(cleanedName, out var pendingControllerInputs)) {
                pendingControllerInputs = [];
                pendingInputs[cleanedName] = pendingControllerInputs;
            }

            if (buttonInputs.TryGetValue(__instance, out var controllerButtonInputs)) {
                foreach (var input in controllerButtonInputs) {
                    input.OnController(null);
                    pendingControllerInputs.Add(input);
                }

                buttonInputs.Remove(__instance);
            }

            if (axisInputs.TryGetValue(__instance, out var controllerAxisInputs)) {
                foreach (var input in controllerAxisInputs) {
                    input.OnController(null);
                    pendingControllerInputs.Add(input);
                }

                axisInputs.Remove(__instance);
            }
        }
    }
}

public class ControllerHelpers {
    public static string ControllerToString(Controller controller) {
        string result = $"===Controller data:===\nController: name:{controller.name}; enabled:{controller.enabled}; tag:{controller.tag}; hardwareName:{controller.hardwareName}; type:{controller.type}; hardwareIdentifier:{controller.hardwareIdentifier}; buttonCount:{controller.buttonCount}; elementCount:{controller.elementCount}\n";
        result += "==Elements:==\n";
        foreach (var element in controller.Elements) {
            result += $"element id:{element.id}; name:{element.name}; type:{element.type}; elementIdentifier:{ControllerElementIdentifierToString(element.elementIdentifier)}; isMemberElement:{element.isMemberElement}; compoundElement:{element.compoundElement}\n";
        }
        result += "==Elements end==\n";
        result += "==Compund elements:==\n";
        foreach (var element in controller.CompoundElements) {
            result += $"element id:{element.id}; name:{element.name}; type:{element.type}; hasElements:{element.hasElements}; elementCount:{element.elementCount}; elementIdentifier:{ControllerElementIdentifierToString(element.elementIdentifier)};\n";
        }
        result += "==Compund elements end==\n";
        result += "===Controller data end===";
        return result;
    }
    private static string ControllerElementIdentifierToString(ControllerElementIdentifier elementIdentifier) {
        return $"(id:{elementIdentifier.id}; name:{elementIdentifier.name}; positiveName:{elementIdentifier.positiveName}; negativeName: {elementIdentifier.negativeName}; elementType:{elementIdentifier.elementType}; compoundElementType:{elementIdentifier.compoundElementType})";
    }
}
