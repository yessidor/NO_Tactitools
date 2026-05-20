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

        IList<Controller.Element> elements = elementsCache.GetValue((Controller)controller);
        if (buttonIndex < 0 || buttonIndex >= elements.Count) {
            Plugin.Log(string.Format("[IC] Invalid button index {0} for controller {1}", buttonIndex, controllerName));
            return "";
        }
        string buttonName = elements[buttonIndex].elementIdentifier.name;
        return buttonName;
    }

    public static int GetButtonIndex(string controllerName, string buttonName) {
        Controller controller = GetController(controllerName);
        if (controller == null)
            return -5;

        IList<Controller.Element> elements = elementsCache.GetValue((Controller)controller);
        for (int buttonIndex = 0; buttonIndex < elements.Count; buttonIndex++)
            if (elements[buttonIndex].name == buttonName)
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

    private static TraverseCache<Controller, IList<Controller.Element>> elementsCache = new("KHksquAJKcDEUkNfJQjMANjDEBFB");
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

    public bool HasModifier(Modifier modifier) {
        return modifiersData.TryGetValue(modifier.ControllerName, out var controllerModifiers) && controllerModifiers.TryGetValue(modifier.ButtonIndex, out var _);
    }

    public HashSet<Modifier> GetModifiers(bool activeOnly = false) {
        HashSet<Modifier> result = new ();
        foreach ((var controllerName, var controllerModifiers) in modifiersData)
            foreach ((var buttonIndex, var buttonState) in controllerModifiers)
                if (!activeOnly || buttonState)
                    result.Add(new Modifier(controllerName, buttonIndex));
        return result;
    }

    public void AddModifierBinding(RewiredInputConfig config) {
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

    public void RemoveModifierBinding(RewiredInputConfig config) {
        if (!allModifiers.TryGetValue(config, out var modifier))
            return;
        RemoveModifier(modifier);
        allModifiers.Remove(config);
    }

    public void ChangeModifierBinding(RewiredInputConfig config) {
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

    private Dictionary<RewiredInputConfig, Modifier> allModifiers = new ();
    private Dictionary<string, Dictionary<int, bool>> modifiersData = new ();
};

public class InputRegistration {
    public RewiredInputConfig config;
    public float longPressThreshold;
    public System.Action onPress;
    public System.Action onShortPress;
    public System.Action onHold;
    public System.Action onLongPress;
}

public class InputCatcher {
    public static List<InputRegistration> allRegistrations = [];
    // Dictionary mapping each controller to its list of buttons
    public static Dictionary<Controller, List<ControllerInput>> controllerInputs = [];
    // Dictionary mapping controller names to pending buttons
    public static Dictionary<string, List<PendingInput>> pendingControllerInputs = [];
    public static ModifiersTracker ModsTracker = new();

    public static void RegisterNewInput(
        RewiredInputConfig config,
        float longPressThreshold = 0.2f,
        System.Action onPress = null,
        System.Action onRelease = null,
        System.Action onHold = null,
        System.Action onLongPress = null
        ) {
        
        InputRegistration reg = new() {
            config = config,
            longPressThreshold = longPressThreshold,
            onPress = onPress,
            onShortPress = onRelease,
            onHold = onHold,
            onLongPress = onLongPress
        };
        allRegistrations.Add(reg);

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

        TryRegisterOrQueue(reg, controllerName, buttonIndex, modifiers);
    }

    public static IEnumerator RegisterPendingInputsRoutine(Controller controller, List<PendingInput> pendingInputs) {
        yield return null;
        foreach (PendingInput pending in pendingInputs) {
            RegisterInputNow(
                pending.registration,
                controller,
                pending.inputIndex,
                pending.modifiers);
        }
    }

    public static void RegisterInputNow(
        InputRegistration registration,
        Controller controller,
        int inputIndex,
        HashSet<Modifier> modifiers) {
        string controllerName = controller.name.Trim();

        ControllerInput newInput = new(
                    registration,
                    controller,
                    inputIndex,
                    modifiers
                    );

        controllerInputs[controller].Add(newInput);
    }

    public static void RegisterNewBinding(RewiredInputConfig config) {
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

        foreach (var reg in allRegistrations.Where(r => r.config == config)) {
            TryRegisterOrQueue(reg, controllerName, buttonIndex, modifiers);
        }
    }
    
    public static void ModifyInputAfterNewConfig(RewiredInputConfig config) {
        //Since clearLinkedEntries is false, config entries won't be modified, and OnSettingChanged() won't be called
        //So no need to set ExecOnSettingChanged to true
        UnregisterInput(config, clearLinkedEntries: false);
        RegisterNewBinding(config);
    }

    public static void UnregisterInput(RewiredInputConfig config, bool clearLinkedEntries = true) {
        foreach (Controller controller in controllerInputs.Keys) {
            int removed = controllerInputs[controller].RemoveAll(input => input.registration.config == config);
            if (removed > 0) {
                Plugin.Log("[IC] Unregistered " + removed + " input(s) for config " + config.Input.Definition.Key);
            }
        }

        // Also remove from pending inputs
        foreach (string controllerName in pendingControllerInputs.Keys.ToList()) {
            int removed = pendingControllerInputs[controllerName].RemoveAll(p => p.registration.config == config);
            if (removed > 0) {
                Plugin.Log("[IC] Removed " + removed + " pending input(s) for config " + config.Input.Definition.Key);
            }
        }

        if (clearLinkedEntries) {
            // also clear the linked entries
            // save, set to false, and restore ExecOnSettingChanged to avoid unneeded OnSettingChanged() call
            var execOnSettingChanged = config.ExecOnSettingChanged;
            config.ExecOnSettingChanged = false;
            try {
                config.ControllerName.BoxedValue = "";
                config.ButtonIndex.BoxedValue = -4;
                config.ModifiersString.BoxedValue = "";
                config.Input.BoxedValue = "";
            }
            finally {
                config.ExecOnSettingChanged = execOnSettingChanged;
            }
        }
    }

    private static void TryRegisterOrQueue(
        InputRegistration registration,
        string controllerName,
        int buttonIndex,
        HashSet<Modifier> modifiers
        ) {
        foreach (Controller controller in controllerInputs.Keys) {
            if (controller.name.Trim() != controllerName) continue;

            RegisterInputNow(registration, controller, buttonIndex, modifiers);
            return;
        }

        // controller not connected yet: queue as pending
        if (!pendingControllerInputs.ContainsKey(controllerName))
            pendingControllerInputs[controllerName] = [];

        pendingControllerInputs[controllerName].Add(new PendingInput(registration, buttonIndex, modifiers));
        Plugin.Log("[IC] Controller not connected, input " + buttonIndex + " added to pending list for " + controllerName);
    }
}

public class ControllerInput {
    public InputRegistration registration;
    public int buttonNumber;
    public HashSet<Modifier> modifiers;
    public bool currentButtonState;
    public bool previousButtonState;
    public float buttonPressTime;
    public bool longPressHandled;
    public bool holdLongHandled;

    public ControllerInput(
        InputRegistration registration,
        Controller controller,
        int buttonNumber,
        HashSet<Modifier> modifiers
        ) {
        this.registration = registration;
        this.buttonNumber = buttonNumber;
        this.modifiers = modifiers;
        this.currentButtonState = controller.Buttons[buttonNumber].value;
        this.previousButtonState = this.currentButtonState;
        this.buttonPressTime = Time.time;
        this.longPressHandled = true; // Assume it's already handled if they're holding it down on registration
        this.holdLongHandled = true;
        if (registration.onPress == null && registration.onShortPress == null && registration.onLongPress == null && registration.onHold == null) {
            Plugin.Logger.LogError("[IC] No actions provided for button " + buttonNumber);
        }
        else {
            Plugin.Log($"[IC] Creating input {buttonNumber.ToString()} with actions");
        }
    }
}

public class PendingInput(InputRegistration registration, int inputIndex, HashSet<Modifier> modifiers) {
    public InputRegistration registration = registration;
    public int inputIndex = inputIndex;
    public HashSet<Modifier> modifiers = modifiers;
}

[HarmonyPatch(typeof(Rewired.Controller), "pBrAJYWOGkILyqjLrMpmCdajATI")]
class ControllerInputInterceptionPatch {
    static void Prefix(Controller __instance) {
        InputCatcher.ModsTracker.UpdateModifiersState(__instance);
        HashSet<Modifier> activeModifiers = InputCatcher.ModsTracker.GetModifiers(activeOnly: true);
        HashSet<Modifier> allModifiers = InputCatcher.ModsTracker.GetModifiers(activeOnly: false);

        if (GameBindings.Player.Aircraft.GetAircraft(silent: true) == null || GameBindings.GameState.IsGamePaused()) {
            return;
        }

        foreach (Controller controller in InputCatcher.controllerInputs.Keys) {
            var controllerName = controller.name.Trim();
            if (GameBindings.GameState.IsChatboxActive() && (controllerName == "Keyboard" || controllerName == "Mouse")) {
                continue; // Don't process keyboard inputs if chatbox is active
            }
            if (__instance == controller) {
                //Assuming modifier key won't be in controllerInputs[controller]
                foreach (ControllerInput button in InputCatcher.controllerInputs[controller]) {
                    try {
                        button.currentButtonState = __instance.Buttons[button.buttonNumber].value;
                        if (!button.previousButtonState && button.currentButtonState && button.modifiers.SetEquals(activeModifiers)) {
                            Plugin.Log(
                                string.Format(
                                    "[IC] Press detected on button {0} with modifiers {1}",
                                    new Modifier(controllerName, button.buttonNumber),
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
                                        new Modifier(controllerName, button.buttonNumber),
                                        ModifierUtils.ToString(button.modifiers)));
                                button.registration.onLongPress?.Invoke();
                                button.longPressHandled = true;
                            }
                            else if (holdDuration < button.registration.longPressThreshold && button.registration.onHold != null) {
                                if (!button.holdLongHandled) {
                                    Plugin.Log(
                                        string.Format(
                                            "[IC] Hold detected on button {0} with modifiers {1}",
                                            new Modifier(controllerName, button.buttonNumber),
                                            ModifierUtils.ToString(button.modifiers)));
                                    button.holdLongHandled = true;
                                }
                                button.registration.onHold?.Invoke();
                            }
                            button.previousButtonState = button.currentButtonState;
                        }
                        else if (button.previousButtonState && !button.currentButtonState) {
                            if (!button.longPressHandled && button.registration.onShortPress != null) {
                                // Button just released
                                Plugin.Log(
                                    string.Format(
                                        "[IC] Short press detected on button {0} with modifiers {1}",
                                        new Modifier(controllerName, button.buttonNumber),
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
                                button.buttonNumber,
                                __instance.name.Trim()));
                        InputCatcher.controllerInputs[controller].Remove(button);
                    }
                }
            }
        }
        return;
    }
}

[HarmonyPatch(typeof(Rewired.Controller), "Connected")]
class RegisterControllerPatch {
    static void Postfix(Controller __instance) {
        string cleanedName = __instance.name.Trim();
        Plugin.Log("[IC] Controller connected: " + cleanedName);
        if (!InputCatcher.controllerInputs.ContainsKey(__instance)) {
            InputCatcher.controllerInputs[__instance] = [];
            Plugin.Log("[IC] Controller structure initialized for: " + cleanedName);
        }

        if (InputCatcher.pendingControllerInputs.ContainsKey(cleanedName)) {
            List<PendingInput> pendingInputs = InputCatcher.pendingControllerInputs[cleanedName];
            Plugin.Instance.StartCoroutine(InputCatcher.RegisterPendingInputsRoutine(__instance, pendingInputs));
            InputCatcher.pendingControllerInputs.Remove(cleanedName);
        }
    }
}
