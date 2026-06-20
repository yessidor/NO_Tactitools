using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

public class ResponseCurve {
    public float Curvature { set; get; } = 0.0f;
    public float ResultOffset { set; get; } = 0.0f;
    public float ArgumentOffset { set; get; } = 0.0f;

    public float Calc(float x) {
        var xx = x - ArgumentOffset;
        return (float)(Curvature*Math.Pow(xx, 3) + (1.0f - Curvature)*xx + ResultOffset);
    }

    public float CalcArg(float y, float x0 = -1.0f, float x1 = 1.0f, float eps = 1e-4f, int steps = 10) {
        float sign(float x) { return x < 0.0f ? -1.0f : 1.0f; };
        var y0 = Calc(x0);
        var y1 = Calc(x1);
        var s = sign(y - y0);
        if (s != sign(y1 - y))
            throw new Exception ("Invalid limits");
        var x = 0.0f;
        for (int i = 0; i < steps; i++) {
            x = 0.5f*x0 + 0.5f*x1;
            var yx = Calc(x);
            if (Math.Abs(y - yx) <= eps)
                break;
            if (yx < y && s > 0) x0 = x;
            else x1 = x;
        }
        return x;
    }
};

public class KeyboardAxis {
    public float Min { set; get; } = -1.0f;
    public float Max { set; get; } = 1.0f;
    public float BuildUpSpeed { set; get; } = 1.0f;
    public float DecaySpeed { set; get; } = 1.0f;
    public ResponseCurve DynamicCurve { get; } = new ResponseCurve ();
    public ResponseCurve StaticCurve { get; } = new ResponseCurve ();

    public bool IncKeyPressed { set; private get; } = false;
    public bool DecKeyPressed { set; private get; } = false;

    public float Result { private set { result = value; } get { return result; } }

    private float? prevTime = null;
    private float initial = 0.0f;
    private float intermediate = 0.0f;
    private float accumulated = 0.0f;
    private float result = 0.0f;
    private enum State { idle, paused, accumulating, decaying };
    private State state = State.idle;

    public KeyboardAxis() {
        result = StaticCurve.ResultOffset;
    }

    public float Compute() {
        if (!IncKeyPressed && !DecKeyPressed && (DecaySpeed == 0.0f || accumulated == 0.0f)) {
            prevTime = null;
            state = State.idle;
        }
        else if (IncKeyPressed && DecKeyPressed) {
            prevTime = null;
            state = State.paused;
        }
        else {
            //accumulating or decaying
            //in either case need to compute timeDelta
            var timeDelta = 0.0f;
            if (prevTime is null)
                prevTime = Time.time;
            else {
                var now = Time.time;
                timeDelta = now - (float)prevTime;
                if (now != prevTime)
                    prevTime = now;
            }

            //skips the first iteration where timeDelta == 0.0f
            if (timeDelta > 0.0f) {
                //accumulating
                if (IncKeyPressed || DecKeyPressed) {
                    if (state != State.accumulating) {
                        initial = 0.0f;
                        intermediate = 0.0f;
                    }
                    var initialDelta = (IncKeyPressed ? 1.0f : -1.0f) * timeDelta * BuildUpSpeed;
                    initial = Mathf.Sign(initialDelta) == Mathf.Sign(initial) ? initial + initialDelta : initialDelta;
                    var newIntermediate = DynamicCurve.Calc(initial);
                    var accumulatedDelta = newIntermediate - intermediate;
                    intermediate = newIntermediate;
                    var newAccumulated = accumulated + accumulatedDelta;
                    var newResult = StaticCurve.Calc(newAccumulated);
                    var clampedNewResult = Math.Clamp(newResult, Min, Max);
                    if (newResult == clampedNewResult) {
                        accumulated = newAccumulated;
                        result = newResult;
                    }
                    else {
                        accumulated = StaticCurve.CalcArg(clampedNewResult, Min, Max);
                        result = clampedNewResult;
                    }
                    state = State.accumulating;
                }
                //decaying
                else {
                    initial = 0.0f;
                    intermediate = 0.0f;
                    var accumulatedDelta = (accumulated < 0.0f ? 1.0f : -1.0f) * timeDelta * DecaySpeed;
                    if (Math.Abs(accumulated) < Math.Abs(accumulatedDelta)) {
                        accumulated = 0.0f;
                        result = StaticCurve.ResultOffset;
                        prevTime = null;
                        state = State.idle;
                    }
                    else {
                        accumulated += accumulatedDelta;
                        //TODO Not needed when decaying?
                        result = Math.Clamp(StaticCurve.Calc(accumulated), Min, Max);
                        state = State.decaying;
                    }
                }
            }
        }
        return result;
    }

    public float SetResult(float r) {
        if (result == r)
            return result;
        result = Math.Clamp(r, Min, Max);
        accumulated = StaticCurve.CalcArg(result, Min, Max);
        return result;
    }
};

public class VirtualJoystickExtender {
    public enum Modes { Roll, Yaw, RollYaw };
    public Modes Mode {
        set {
            if (ToggleMode) {
                if (value != Modes.Roll) {
                    field = field == Modes.Roll ? value : field == value ? Modes.Roll : value;
                    OnSetMode();
                }
            }
            else {
                field = value;
                OnSetMode();
            }
        }
        private get;
    } = Modes.Roll;
    public bool Enabled {
      set {
        field = value;
        OnEnabled();
      }
      get;
    } = true;
    public bool ToggleMode { set; get; } = false;
    public float YawMultiplier { set; get; } = 1.0f;
    public float RollYawMultiplier { set; get; } = 1.0f;

    public enum DecayModes { None, Instant };
    public DecayModes DecayMode { set;  get; } = DecayModes.None;

    public ResponseCurve YawCurve = new ResponseCurve ();
    public ResponseCurve PitchCurve = new ResponseCurve ();
    public ResponseCurve RollCurve = new ResponseCurve ();

    public void Update(ref ControlInputs controlInputs) {
        if (!Enabled || GameManager.playerInput.GetButton("Free Look") || DynamicMap.mapMaximized || Leaderboard.IsOpen() || RadialMenuMain.IsInUse()) {
            if (DecayMode == DecayModes.Instant) {
                controlInputs.yaw = 0.0f;
                controlInputs.pitch = 0.0f;
                controlInputs.roll = 0.0f;

                var flightHud = SceneSingleton<FlightHud>.i;
                if (flightHud is not null)
                    flightHud.SetVirtualJoystick(new Vector3 (0.0f, 0.0f, 0.0f));
            }
            return;
        }

        if (Mode == Modes.Yaw) {
            controlInputs.yaw = controlInputs.roll * YawMultiplier;
            controlInputs.yaw = YawCurve.Calc(controlInputs.yaw);
            controlInputs.roll = 0.0f;
        }
        else {
            controlInputs.roll = RollCurve.Calc(controlInputs.roll);
            if (Mode == Modes.RollYaw) {
                controlInputs.yaw = controlInputs.roll * RollYawMultiplier;
            }
        }
        controlInputs.pitch = PitchCurve.Calc(controlInputs.pitch);
    }

    private void OnSetMode() {
        var flightHud = SceneSingleton<FlightHud>.i;
        if (flightHud != null) {
          var localPos = flightHud.virtualJoystickPos.transform.localPosition;
          localPos.x = 0.0f;
          flightHud.SetVirtualJoystick(localPos);
        }

        string[] modeNames = { "Roll", "Yaw", "Roll&Yaw" };
        string report = $"Virtual joystick: <b>{modeNames[(int)Mode]}</b>";
        UIBindings.Game.DisplayToast(report, 2f);
    }

    private void OnEnabled() {
        string report = $"Virtual joystick: <b>{(Enabled ? "enabled" : "disabled")}</b>";
        UIBindings.Game.DisplayToast(report, 2f);
    }
};

[HarmonyPatch(typeof(MainMenu), "Start")]
class VirtualJoystickExtenderPlugin {
    static void Postfix() {
        VirtualJoystickExtenderComponent.Init(); 
    }
}

class VirtualJoystickExtenderComponent {
    public static bool ControlInThirdPersonMode = true;

    private static bool initialized = false;
    private static VirtualJoystickExtender virtualJoystickExtender = new ();

    public static void Init() {
        if (!initialized) {
            Plugin.Log("[VJE] Initializing Virtual Joystick Extender Component");

            Plugin.harmony.PatchAll(typeof(OnPilotPlayerStatePlayerAxisControls));

            InputCatcher.RegisterNewInput(
                Plugin.VirtualJoystickExtender.ToggleStateKey,
                0.0f,
                onPress: () => { virtualJoystickExtender.Enabled = !virtualJoystickExtender.Enabled; }
                );
            InputCatcher.RegisterNewInput(
                Plugin.VirtualJoystickExtender.YawKey,
                0.0f,
                onRelease: () => { virtualJoystickExtender.Mode = VirtualJoystickExtender.Modes.Roll; },
                onPress: () => { virtualJoystickExtender.Mode = VirtualJoystickExtender.Modes.Yaw; }
                );
            InputCatcher.RegisterNewInput(
                Plugin.VirtualJoystickExtender.RollYawKey,
                0.0f,
                onRelease: () => { virtualJoystickExtender.Mode = VirtualJoystickExtender.Modes.Roll; },
                onPress: () => { virtualJoystickExtender.Mode = VirtualJoystickExtender.Modes.RollYaw; }
                );

            var virtualJoystickBindings = new BindingHelper.Binding[] {
                new (virtualJoystickExtender, "ToggleMode", Plugin.VirtualJoystickExtender.ToggleMode),
                new (virtualJoystickExtender, "Mode", Plugin.VirtualJoystickExtender.DefaultMode),
                new (virtualJoystickExtender, "YawMultiplier", Plugin.VirtualJoystickExtender.YawMultiplier),
                new (virtualJoystickExtender, "RollYawMultiplier", Plugin.VirtualJoystickExtender.RollYawMultiplier),
                new (virtualJoystickExtender.YawCurve, "Curvature", Plugin.VirtualJoystickExtender.YawCurvature),
                new (virtualJoystickExtender.PitchCurve, "Curvature", Plugin.VirtualJoystickExtender.PitchCurvature),
                new (virtualJoystickExtender.RollCurve, "Curvature", Plugin.VirtualJoystickExtender.RollCurvature),
                new (virtualJoystickExtender, "DecayMode", Plugin.VirtualJoystickExtender.DecayMode),
                new (typeof(VirtualJoystickExtenderComponent), "ControlInThirdPersonMode", Plugin.VirtualJoystickExtender.ControlInThirdPersonMode),
            };
            BindingHelper.ApplyBindings(virtualJoystickBindings);

            initialized = true;

            Plugin.Log("[VJE] Initialized Virtual Joystick Extender Component");
        }
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(PilotPlayerState), "PlayerAxisControls")]
    public class OnPilotPlayerStatePlayerAxisControls {
        static void Prefix(ref CameraMode __state) {
            __state = CameraStateManager.cameraMode;
            var cameraMode = CameraStateManager.cameraMode;
            var freeLook = GameManager.playerInput.GetButton("Free Look");
            //Setting cameraMode to CameraMode.cockpit to force PlayerAxisControls() process virtual joystick in camera orbit and chase modes
            if (ControlInThirdPersonMode && !freeLook && (cameraMode == CameraMode.orbit || cameraMode == CameraMode.chase))
                CameraStateManager.cameraMode = CameraMode.cockpit;
        }

        static void Postfix(ref ControlInputs ___controlInputs, ref CameraMode __state) {
            CameraStateManager.cameraMode = __state;
            virtualJoystickExtender.Update(ref ___controlInputs);
        }
    }
}

[HarmonyPatch(typeof(MainMenu), "Start")]
class KeyAxesPlugin {
    static void Postfix() {
        KeyAxesComponent.Init(); 
    }
}

class KeyAxesComponent {
    public static void Init() {
        if (!initialized) {
            Plugin.Log("[KA] Initializing Key axes component");

            Plugin.harmony.PatchAll(typeof(OnPilotPlayerStatePlayerAxisControls));
            Plugin.harmony.PatchAll(typeof(OnPilotPlayerStatePlayerThrottleAxis1Controls));
            Plugin.harmony.PatchAll(typeof(OnPilotPlayerStatePlayersControls));

            var keyboardAxesBindings = new List<BindingHelper.Binding> ();
            for (int i = 0; i < keyboardControlledAxes.Length; i++) {
                var kaxis = keyboardControlledAxes[i];
                var vars = Plugin.keyAxes[i];
                InputCatcher.RegisterNewInput(
                    Plugin.keyAxes[i].IncKey,
                    0.0f,
                    onRelease: () => { kaxis.IncKeyPressed = false; },
                    onPress: () => { kaxis.IncKeyPressed = true; }
                    );
                InputCatcher.RegisterNewInput(
                    Plugin.keyAxes[i].DecKey,
                    0.0f,
                    onRelease: () => { kaxis.DecKeyPressed = false; },
                    onPress: () => { kaxis.DecKeyPressed = true; }
                    );
                keyboardAxesBindings.Add(new BindingHelper.Binding (kaxis, "BuildUpSpeed", vars.BuildUpSpeed));
                keyboardAxesBindings.Add(new BindingHelper.Binding (kaxis, "DecaySpeed", vars.DecaySpeed));
                keyboardAxesBindings.Add(new BindingHelper.Binding (kaxis.DynamicCurve, "Curvature", vars.DynamicCurvature));
                keyboardAxesBindings.Add(new BindingHelper.Binding (kaxis.StaticCurve, "Curvature", vars.StaticCurvature));
                keyboardAxesBindings.Add(new BindingHelper.Binding (kaxis.StaticCurve, "ResultOffset", vars.StaticOffset));

                kaxis.Min = vars.Min;
                kaxis.Max = vars.Max;
            }
            BindingHelper.ApplyBindings([.. keyboardAxesBindings]);

            initialized = true;

            Plugin.Log("[KA] Initialized Key axes component");
        }
    }

    public static void ProcessPrimaryInputs(ref PilotPlayerState instance, ref ControlInputs controlInputs, ref Pilot pilot) {
        float min = -1.0f, max = 1.0f;
        controlInputs.pitch = Math.Clamp(controlInputs.pitch + keyboardControlledAxes[(int)KeyboardAxisID.Pitch].Compute(), min, max);
        controlInputs.roll = Math.Clamp(controlInputs.roll + keyboardControlledAxes[(int)KeyboardAxisID.Roll].Compute(), min, max);
        controlInputs.yaw = Math.Clamp(controlInputs.yaw + keyboardControlledAxes[(int)KeyboardAxisID.Yaw].Compute(), min, max);
    }

    public static void ProcessSecondaryInputs(ref PilotPlayerState instance, ref ControlInputs controlInputs, ref Pilot pilot) {
        float min = 0.0f, max = 1.0f;

        bool hover = pilot.aircraft.IsAutoHoverEnabled();
        bool leftHover = !hover && prevHover;
        prevHover = hover;

        if (hover)
            return;
        else if (leftHover) {
            float simulatedThrottle = (float)simulatedThrottle_.GetValue(instance);
            var throttleKeyAxis = keyboardControlledAxes[(int)KeyboardAxisID.Throttle];
            Plugin.Log(string.Format("[KA] Left hover mode; simulated throttle = {0}; throttle = {1}", simulatedThrottle, throttleKeyAxis.Result));
            throttleKeyAxis.SetResult(Math.Clamp(simulatedThrottle, min, max));
        }
        else
            controlInputs.throttle = Math.Clamp(keyboardControlledAxes[(int)KeyboardAxisID.Throttle].Compute(), min, max);

        controlInputs.customAxis1 = Math.Clamp(keyboardControlledAxes[(int)KeyboardAxisID.CustomAxis1].Compute(), min, max);
    }

    public static void ProcessBrakeInputs(ref ControlInputs controlInputs) {
        float min = 0.0f, max = 1.0f;
        controlInputs.brake = Math.Clamp(keyboardControlledAxes[(int)KeyboardAxisID.Brake].Compute(), min, max);
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(PilotPlayerState), "PlayerControls")]
    public class OnPilotPlayerStatePlayersControls {
        static void Postfix(ref ControlInputs ___controlInputs) {
            ProcessBrakeInputs(ref ___controlInputs);
        }
    }

    [HarmonyPriority(Priority.Low + 1)]
    [HarmonyPatch(typeof(PilotPlayerState), "PlayerAxisControls")]
    public class OnPilotPlayerStatePlayerAxisControls {
        static void Postfix(ref PilotPlayerState __instance, ref ControlInputs ___controlInputs, ref Pilot ___pilot) {
            ProcessPrimaryInputs(ref __instance, ref ___controlInputs, ref ___pilot);
        }
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(PilotPlayerState), "PlayerThrottleAxis1Controls")]
    public class OnPilotPlayerStatePlayerThrottleAxis1Controls {
        static void Postfix(ref PilotPlayerState __instance, ref ControlInputs ___controlInputs, ref Pilot ___pilot) {
            ProcessSecondaryInputs(ref __instance, ref ___controlInputs, ref ___pilot);
        }
    }

    private static bool initialized = false;
    private enum KeyboardAxisID { Pitch, Roll, Yaw, Throttle, Brake, CustomAxis1 };
    private static KeyboardAxis[] keyboardControlledAxes = new KeyboardAxis[6] { new(), new(), new(), new(), new(), new() };
    private static FieldInfo simulatedThrottle_ = AccessTools.Field(typeof(PilotPlayerState), "simulatedThrottle");
    private static bool prevHover = false;
};
