using HarmonyLib;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using NuclearOption.UI;
using NO_Tactitools.Core;

namespace NO_Tactitools.Controls;

public abstract class Curve {
    public abstract float Calc(float x);

    public float CalcArg(float y, float x0 = -1.0f, float x1 = 1.0f, float eps = 1e-4f, int steps = 10) {
        float sign(float x) { return x < 0.0f ? -1.0f : 1.0f; };
        var y0 = Calc(x0);
        var y1 = Calc(x1);
        y = Math.Clamp(y, y0, y1);
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
}

public class CubicResponseCurve : Curve {
    public float X0 = 0.0f;
    public float X1 = 1.0f;
    public float Y0 = 0.0f;
    public float Y1 = 1.0f;
    public float C = 0.0f;

    public override float Calc(float x) {
        var u = (x - X0) / (X1 - X0);
        return Y0 + (Y1 - Y0) * (C*u*u*u + (1.0f - C)*u);
    }

    public CubicResponseCurve(float c = 0f, float x0 = 0f, float x1 = 1f, float y0 = 0f, float y1 = 1f) {
        C = c; X0 = x0; X1 = x1; Y0 = y0; Y1 = y1; 
    }
}

public class KeyAndEncoderAxis {
    public CubicResponseCurve EncoderDynamicCurve { get; } = new ();
    public CubicResponseCurve KeyDynamicCurve { get; } = new ();
    public CubicResponseCurve StaticCurve { get; } = new ();

    public float Min {
        set {
            if (value > Max)
               throw new ArgumentException ("Invalid Min");
            StaticCurve.Y0 = Math.Clamp(StaticCurve.Y0, value, Max);
            StaticCurve.Y1 = StaticCurve.Y0 + 1.0f;
            var clampedResult = Math.Clamp(result, value, Max);
            if (result != clampedResult) {
                accumulated = StaticCurve.CalcArg(clampedResult, -accumulatedLimit, accumulatedLimit);
                result = clampedResult;
            }
            field = value;
        }
        get;
    } = -1.0f;
    public float Max {
        set {
            if (value < Min)
               throw new ArgumentException ("Invalid Max");
            StaticCurve.Y0 = Math.Clamp(StaticCurve.Y0, Min, value);
            StaticCurve.Y1 = StaticCurve.Y0 + 1.0f;
            var clampedResult = Math.Clamp(result, Min, value);
            if (result != clampedResult) {
                accumulated = StaticCurve.CalcArg(clampedResult, -accumulatedLimit, accumulatedLimit);
                result = clampedResult;
            }
            field = value;
        }
        get;
    } = 1.0f;
    public float EncoderSens { set; get; } = 1.0f;
    public float BuildUpSpeed { set; get; } = 1.0f;
    public float DecaySpeed { set; get; } = 1.0f;
    public float DecayDelay = 0.0f;
    public bool TwoKeyReset = false;

    public bool IncKeyPressed { set; get; } = false;
    public bool DecKeyPressed { set; get; } = false;
    public float EncoderDelta { private set; get { field = encoderInitialDelta; return encoderInitialDelta; } } = 0;

    public float Result { private set { result = value; } get { return result; } }

    public KeyAndEncoderAxis() {
        result = StaticCurve.Y0;
    }

    public void OnMoveRaw(float delta) {
        encoderInitialDelta += delta;
    }

    public float Compute() {
        var timeDelta = 0.0f;
        var now = Time.unscaledTime;
        if (prevTime == null)
            prevTime = now;
        else {
            timeDelta = now - (float)prevTime;
            if (now != prevTime)
                prevTime = now;
        }

        if (result_ != null) {
            result = (float)result_;
            accumulated = StaticCurve.CalcArg(result, -accumulatedLimit, accumulatedLimit, steps: accumulatedSteps);
            result_ = null;
        }

        //accumulating or decaying
        //in either case need to compute timeDelta
        if (!IncKeyPressed && !DecKeyPressed && (DecaySpeed == 0.0f || accumulated == 0.0f) && encoderInitialDelta == 0.0f) {
            prevTime = null;
            state = State.idle;
        }
        else if (IncKeyPressed && DecKeyPressed && encoderInitialDelta == 0.0f) {
            prevTime = null;
            if (TwoKeyReset) {
                keyInitial = 0.0f;
                keyIntermediate = 0.0f;
                encoderInitial = 0.0f;
                encoderIntermediate = 0.0f;
                accumulated = 0.0f;
                result = StaticCurve.Y0;
                state = State.idle;
            }
            else  {
                state = State.paused;
            }
        }
        else {
            if (timeDelta > 0.0f) {
                //accumulating
                if (IncKeyPressed || DecKeyPressed || encoderInitialDelta != 0.0f) {
                    if (state != State.accumulating) {
                        keyInitial = 0.0f;
                        keyIntermediate = 0.0f;
                        encoderInitial = 0.0f;
                        encoderIntermediate = 0.0f;
                    }

                    var accumulatedDelta = 0.0f;

                    if (IncKeyPressed || DecKeyPressed) {
                        var keyInitialDelta = (IncKeyPressed ? 1.0f : -1.0f) * timeDelta * BuildUpSpeed;
                        if (Mathf.Sign(keyInitialDelta) == Mathf.Sign(keyInitial))
                            keyInitial += keyInitialDelta;
                        else {
                            keyInitial = keyInitialDelta;
                            keyIntermediate = KeyDynamicCurve.Calc(0);
                        }
                        var newKeyIntermediate = KeyDynamicCurve.Calc(keyInitial);
                        accumulatedDelta += newKeyIntermediate - keyIntermediate;
                        keyIntermediate = newKeyIntermediate;
                        lastInputTime = now;
                    }

                    if (encoderInitialDelta != 0.0f) {
                        encoderInitialDelta *= EncoderSens * 0.033f; //30 FPS
                        if (Mathf.Sign(encoderInitialDelta) == Mathf.Sign(encoderInitial))
                            encoderInitial += encoderInitialDelta;
                        else {
                            encoderInitial = encoderInitialDelta;
                            encoderIntermediate = EncoderDynamicCurve.Calc(0);
                        }
                        encoderInitialDelta = 0.0f;
                        var newEncoderIntermediate = EncoderDynamicCurve.Calc(encoderInitial);
                        accumulatedDelta += newEncoderIntermediate - encoderIntermediate;
                        encoderIntermediate = newEncoderIntermediate;
                        lastInputTime = now;
                    }

                    if (accumulatedDelta != 0.0f) {
                        var newAccumulated = accumulated + accumulatedDelta;
                        var newResult = StaticCurve.Calc(newAccumulated);
                        var clampedNewResult = Math.Clamp(newResult, Min, Max);
                        if (newResult == clampedNewResult) {
                            accumulated = newAccumulated;
                            result = newResult;
                        }
                        else {
                            accumulated = StaticCurve.CalcArg(clampedNewResult, -accumulatedLimit, accumulatedLimit);
                            result = clampedNewResult;
                        }
                    }

                    state = State.accumulating;
                }
                //decaying
                else if (DecaySpeed > 0 && (DecayDelay == 0.0f || now - lastInputTime > DecayDelay)) {
                    keyInitial = 0.0f;
                    keyIntermediate = 0.0f;
                    encoderInitial = 0.0f;
                    encoderIntermediate = 0.0f;
                    var accumulatedDelta = -Mathf.Sign(accumulated) * timeDelta * DecaySpeed;
                    if (Math.Abs(accumulated) < Math.Abs(accumulatedDelta)) {
                        accumulated = 0.0f;
                        result = StaticCurve.Y0;
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
        result_ = Math.Clamp(r, Min, Max);
        return (float)result_;
    }

    private readonly float accumulatedLimit = 10.0f;
    private readonly int accumulatedSteps = 100;
    private float? prevTime = null;
    private float lastInputTime = 0.0f;
    private float keyInitial = 0.0f;
    private float keyIntermediate = 0.0f;
    private float encoderInitialDelta = 0.0f;
    private float encoderInitial = 0.0f;
    private float encoderIntermediate = 0.0f;
    private float accumulated = 0.0f;
    private float result = 0.0f;
    private float? result_ = null;
    private enum State { idle, paused, accumulating, decaying };
    private State state = State.idle;
};


[HarmonyPatch(typeof(MainMenu), "Start")]
class VirtualJoystickExtenderPlugin {
    static void Postfix() {
        VirtualJoystickExtenderComponent.Init();
    }
}

public class Axes<T> : IFormattable {
    public T Yaw;
    public T Pitch;
    public T Roll;

    public Axes(T yaw, T pitch, T roll) {
        this.Yaw = yaw;
        this.Pitch = pitch;
        this.Roll = roll;
    }

    public Axes(Axes<T> axes) {
        this.Yaw = axes.Yaw;
        this.Pitch = axes.Pitch;
        this.Roll = axes.Roll;
    }

    public override bool Equals(object obj) {
        var other = obj as Axes<T>;
        if (other == null)
            return false;
        return this.Yaw.Equals(other.Yaw) && this.Pitch.Equals(other.Pitch) && this.Roll.Equals(other.Roll);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Yaw, Pitch, Roll);
    }

    public static bool operator== (Axes<T> a, Axes<T> b) {
        return (a is null || b is null) ? false : a.Equals(b);
    }

    public static bool operator!= (Axes<T> a, Axes<T> b) {
        return !(a == b);
    }

    public static Axes<T> Parse(string s) {
        // Pattern matches "Yaw: <value>; Pitch: <value>; Roll: <value>"
        var pattern = @"Yaw:\s*(\S+)\s*;\s*Pitch:\s*(\S+)\s*;\s*Roll:\s*(\S+)";
        var match = System.Text.RegularExpressions.Regex.Match(s, pattern);

        if (!match.Success) {
            throw new ArgumentException($"Invalid format: {s}");
        }

        var groups = match.Groups;
        var typeOfT = typeof(T);

        var yaw = (T)Convert.ChangeType(groups[1].Value, typeOfT);
        var pitch = (T)Convert.ChangeType(groups[2].Value, typeOfT);
        var roll = (T)Convert.ChangeType(groups[3].Value, typeOfT);

        return new Axes<T>(yaw, pitch, roll);
    }

    public override string ToString() {
        return $"Yaw: {Yaw}; Pitch: {Pitch}; Roll: {Roll}";
    }

    public string ToString(string fmt, IFormatProvider fp) {
        var ys = (Yaw as IFormattable)?.ToString(fmt, fp) ?? Yaw.ToString();
        var ps = (Pitch as IFormattable)?.ToString(fmt, fp) ?? Pitch.ToString();
        var rs = (Roll as IFormattable)?.ToString(fmt, fp) ?? Roll.ToString();
        return $"Yaw: {ys}; Pitch: {ps}; Roll: {ps}";
    }
}

public class VirtualJoystickExtenderComponent {
    public static void SetMode(int mode) {
        lastMode = Mode;
        Mode = mode;
        OnSetMode();
    }

    public static void ReturnMode() {
       Mode = lastMode;
       OnSetMode();
    }

    public static void ToggleMode(int mode) {
        if (Mode == mode)
            Mode = lastMode;
        else {
            lastMode = Mode;
            Mode = mode;
        }
        OnSetMode();
    }

    public static int Mode {
        set {
            if (value < 0 || value >= modeDatum.Count) {
                throw new Exception ($"Invalid mode: {value}");
            }
            field = value;
        }
        get;
    } = 0;

    public static bool Enabled {
      set {
        field = value;
        OnEnabled();
      }
      get;
    } = true;

    public static bool MaxDeflectionMode {
        set {
            if (field == value)
                return;
            field = value;
            if (field) {
                prevDeflection = pos.magnitude;
                pos = pos.normalized;
            }
            else {
                pos = pos.normalized * prevDeflection;
            }
            SetVirtualJoystickPos(pos);
        }
        get;
    } = false;

    public enum InputAxis { X, Y, Z }
    public enum VectorPlacements { OFF, HMD, HUD };

    public static bool ShiftVector = true;
    public static VectorPlacements VectorPlacement {
        get;
        set {
            field = value;
            UpdateVirtualJoystickPlacement();
        }
    } = VectorPlacements.HUD;
    public static bool RollControlsYawOnTheGround = true;

    public static float MaxDeflection = 150f;
    public static float CenteringSpeed {
        set {
            field = value;
            centeringSpeed = value / MaxDeflection;
        }
        get;
    } = 1f;
    public static float CenteringDeflection {
        set {
            field = value;
            centeringDeflection = value / MaxDeflection;
        }
        get;
    } = -1f;

    public enum LimitsShapes { Circle, Square };
    public static LimitsShapes LimitsShape = LimitsShapes.Circle;

    public static Vector3 Sens = Vector3.one;

    public enum DecayModes { None, Instant, Gradual };
    public static DecayModes DecayMode { set;  get; } = DecayModes.None;
    public static float DecaySpeed = 1.0f;

    public enum InputAxesResetModes { None, Scale, Recalculate };
    public static InputAxesResetModes InputAxesResetMode = InputAxesResetModes.None;

    public static bool ControlInThirdPersonMode = true;
    public static bool DisableInInvalidCameraMode = true;
    public static bool DisableInFreeLook = true;
    public static bool DisableOnMaximizedMap = true;
    public static bool DisableOnUIInteraction = true;
    public static bool RunInGraphicsUpdate = true;
    public static float FixedDT = 0f;

    public static float DampingAngle {
        set {
            field = value;
            halfDampingAngle = value < 0 ? -1f : 0.5f * value;
        }
        get;
    } = -1f;

    public static void Init() {
        if (!initialized) {
            Plugin.Log("[VJE] Initializing Virtual Joystick Extender Component");

            Plugin.harmony.PatchAll(typeof(OnPilotPlayerStatePlayerAxisControls));
            Plugin.harmony.PatchAll(typeof(OnPilotPlayerStateUpdateState));
            Plugin.harmony.PatchAll(typeof(OnHUDBoresightStateDisplayLead));
            Plugin.harmony.PatchAll(typeof(OnControlsFilterGetAim));

            InputCatcher.RegisterButtonInput(
                Plugin.VirtualJoystickExtender.ToggleStateKey,
                0.0f,
                onPress: () => { Enabled = !Enabled; }
                );
            InputCatcher.RegisterButtonInput(
                Plugin.VirtualJoystickExtender.MaxDeflectionModeKey,
                0.0f,
                onRelease: () => { MaxDeflectionMode = false; },
                onPress: () => { MaxDeflectionMode = true; }
                );
            InputCatcher.RegisterButtonInput(
                Plugin.VirtualJoystickExtender.ResetKey,
                Plugin.PressDelay.Value,
                onShortPress: () => { pos.z = 0f; },
                onLongPress: () => { pos.x = pos.y = pos.z = 0f; }
                );
            //roll
            InputCatcher.RegisterAxisInput(
                Plugin.VirtualJoystickExtender.AxisX,
                onMoveRaw: (float value, float valuePrev, float delta) => { deltas.x = value; },
                convertAbsToRel: true
                );
            //pitch
            InputCatcher.RegisterAxisInput(
                Plugin.VirtualJoystickExtender.AxisY,
                onMoveRaw: (float value, float valuePrev, float delta) => { deltas.y = value; },
                convertAbsToRel: true
                );
            //yaw
            InputCatcher.RegisterAxisInput(
                Plugin.VirtualJoystickExtender.AxisZ,
                onMoveRaw: (float value, float valuePrev, float delta) => { deltas.z = value; },
                convertAbsToRel: true
                );

            var bindings = new List<BindingHelper.Binding> () {
                new (typeof(VirtualJoystickExtenderComponent), "Sens", Plugin.VirtualJoystickExtender.Sens),
                new (typeof(VirtualJoystickExtenderComponent), "Mode", Plugin.VirtualJoystickExtender.DefaultMode),
                new (typeof(VirtualJoystickExtenderComponent), "DecayMode", Plugin.VirtualJoystickExtender.DecayMode),
                new (typeof(VirtualJoystickExtenderComponent), "DecaySpeed", Plugin.VirtualJoystickExtender.DecaySpeed),
                new (typeof(VirtualJoystickExtenderComponent), "InputAxesResetMode", Plugin.VirtualJoystickExtender.InputAxesResetMode),
                new (typeof(VirtualJoystickExtenderComponent), "RollControlsYawOnTheGround", Plugin.VirtualJoystickExtender.RollControlsYawOnTheGround),
                new (typeof(VirtualJoystickExtenderComponent), "MaxDeflection", Plugin.VirtualJoystickExtender.MaxDeflection),
                new (typeof(VirtualJoystickExtenderComponent), "LimitsShape", Plugin.VirtualJoystickExtender.LimitsShape),
                new (typeof(VirtualJoystickExtenderComponent), "CenteringSpeed", Plugin.VirtualJoystickExtender.CenteringSpeed),
                new (typeof(VirtualJoystickExtenderComponent), "CenteringDeflection", Plugin.VirtualJoystickExtender.CenteringDeflection),
                new (typeof(VirtualJoystickExtenderComponent), "RunInGraphicsUpdate", Plugin.VirtualJoystickExtender.RunInGraphicsUpdate),
                new (typeof(VirtualJoystickExtenderComponent), "FixedDT", Plugin.VirtualJoystickExtender.FixedDT),
                new (typeof(VirtualJoystickExtenderComponent), "ControlInThirdPersonMode", Plugin.VirtualJoystickExtender.ControlInThirdPersonMode),
                new (typeof(VirtualJoystickExtenderComponent), "DisableInInvalidCameraMode", Plugin.VirtualJoystickExtender.DisableInInvalidCameraMode),
                new (typeof(VirtualJoystickExtenderComponent), "DisableInFreeLook", Plugin.VirtualJoystickExtender.DisableInFreeLook),
                new (typeof(VirtualJoystickExtenderComponent), "DisableOnMaximizedMap", Plugin.VirtualJoystickExtender.DisableOnMaximizedMap),
                new (typeof(VirtualJoystickExtenderComponent), "DisableOnUIInteraction", Plugin.VirtualJoystickExtender.DisableOnUIInteraction),
                new (typeof(VirtualJoystickExtenderComponent), "VectorPlacement", Plugin.VirtualJoystickExtender.VectorPlacement),
                new (typeof(VirtualJoystickExtenderComponent), "ShiftVector", Plugin.VirtualJoystickExtender.ShiftVector),
                new (typeof(VirtualJoystickExtenderComponent), "DampingAngle", Plugin.VirtualJoystickExtender.DampingAngle),
                new (VirtualJoystickExtenderComponent.dampingCurve, "Y0", Plugin.VirtualJoystickExtender.DampingSens),
                new (VirtualJoystickExtenderComponent.dampingCurve, "C", Plugin.VirtualJoystickExtender.DampingCurvature),
            };

            for (int i = 0; i < Plugin.VirtualJoystickExtender.Modes.Count; i++) {
                var modeDataConfig = Plugin.VirtualJoystickExtender.Modes[i];

                //New variable for key callbacks, otherwise all callbacks will have reference to i
                int modeNum = i;

                InputCatcher.RegisterNewInput(
                    modeDataConfig.EngageKey,
                    0.0f,
                    onRelease: () => { ReturnMode(); },
                    onPress: () => { SetMode(modeNum); }
                    );
                InputCatcher.RegisterNewInput(
                    modeDataConfig.ToggleKey,
                    0.0f,
                    onPress: () => { ToggleMode(modeNum); }
                    );

                ModeData modeData = new ();

                bindings.Add(new BindingHelper.Binding (modeData, "Name", modeDataConfig.Name));
                bindings.Add(new BindingHelper.Binding (modeData.AxisMap, "Yaw", modeDataConfig.YawAxisMapping));
                bindings.Add(new BindingHelper.Binding (modeData.AxisMap, "Pitch", modeDataConfig.PitchAxisMapping));
                bindings.Add(new BindingHelper.Binding (modeData.AxisMap, "Roll", modeDataConfig.RollAxisMapping));
                bindings.Add(new BindingHelper.Binding (modeData, "Sens", modeDataConfig.Sens));
                bindings.Add(new BindingHelper.Binding (modeData, "InputMultiplierOnEnter", modeDataConfig.InputMultiplierOnEnter));
                bindings.Add(new BindingHelper.Binding (modeData, "OutputMultiplier", modeDataConfig.OutputMultiplier));
                bindings.Add(new BindingHelper.Binding (modeData, "StaticCurvature", modeDataConfig.StaticCurvature));
                bindings.Add(new BindingHelper.Binding (modeData, "DynamicCurvature", modeDataConfig.DynamicCurvature));

                modeDatum.Add(modeData);
            }

            BindingHelper.ApplyBindings(bindings);

            initialized = true;

            Plugin.Log("[VJE] Initialized Virtual Joystick Extender Component");
        }
    }

    [HarmonyPatch(typeof(PilotPlayerState), "PlayerAxisControls")]
    public class OnPilotPlayerStatePlayerAxisControls {
        static bool Prefix(
            PilotPlayerState __instance, Pilot ___pilot, Player ___player, float ___pilotStrength,
            ref ControlInputs ___controlInputs, ref float ___pitchInput, ref float ___rollInput, ref float ___yawInput) {

            if (!RunInGraphicsUpdate)
                CalcPos(___player, Time.unscaledDeltaTime);

            CalcInputs(__instance, ___pilot, ___player, ___pilotStrength, ref ___controlInputs, ref ___pitchInput, ref ___rollInput, ref ___yawInput);

            if (___pilot.aircraft.IsAutoHoverEnabled())
                playerThrottleAxis1ControlsInfo.Invoke(__instance, null);

            UpdateVirtualJoystickState();

            return false;
        }
    }

    [HarmonyPatch(typeof(PilotPlayerState), "UpdateState")]
    public class OnPilotPlayerStateUpdateState {
        static void Postfix(Player ___player) {
            if (RunInGraphicsUpdate)
                CalcPos(___player, Time.deltaTime);
        }
    }

    private static void CalcPos(Player player, float deltaTime) {
        needToUpdateInputs = false;

        var cameraMode = CameraStateManager.cameraMode;
        bool validCameraMode = cameraMode == CameraMode.cockpit || (ControlInThirdPersonMode && (cameraMode == CameraMode.orbit || cameraMode == CameraMode.chase));
        bool inactive = 
            (DisableInInvalidCameraMode && !validCameraMode) ||
            (DisableInFreeLook && player.GetButton("Free Look")) ||
            (DisableOnMaximizedMap && DynamicMap.mapMaximized) ||
            (DisableOnUIInteraction && (RadialMenuMain.IsInUse() || LeaderboardMenu.IsOpen()));

        if (inactive) {
            if (DecayMode == DecayModes.None) {
                /* noop */
            }
            else if (DecayMode == DecayModes.Instant) {
                pos = Vector3.zero;
                needToUpdateInputs = true;
            }
            else if (DecayMode == DecayModes.Gradual) {
                pos = Vector3.Lerp(pos, Vector3.zero, DecaySpeed * deltaTime);
                needToUpdateInputs = true;
            }

            initial = Vector3.zero;
            intermediate = Vector3.zero;
        }
        else {
            var modeData = modeDatum[Mode];

            var dt = FixedDT > 0 ? FixedDT : Mathf.Min(deltaTime, 0.1f) * 30f;

            if (angle > 0 && halfDampingAngle > 0) {
                var x = Mathf.InverseLerp(0.0f, halfDampingAngle, angle);
                var df = dampingCurve.Calc(x);
                dt *= df;
                angle = -1;
            }

            deltas = deltas / MaxDeflection * dt;
            deltas.Scale(Sens);
            deltas.Scale(modeData.Sens);

            void UpdateInputAxis(ref float init, float delta, ref float inter) {
                if (Mathf.Sign(init) != Mathf.Sign(delta)) {
                    init = delta;
                    inter = 0f;
                }
                else {
                    init += delta;
                }
            }

            UpdateInputAxis(ref initial.x, deltas.x, ref intermediate.x);
            UpdateInputAxis(ref initial.y, deltas.y, ref intermediate.y);
            UpdateInputAxis(ref initial.z, deltas.z, ref intermediate.z);

            Vector3 newIntermediate = new (
                modeData.DynamicCurves.x.Calc(initial.x),
                modeData.DynamicCurves.y.Calc(initial.y),
                modeData.DynamicCurves.z.Calc(initial.z)
            );

            pos += (newIntermediate - intermediate);
            intermediate = newIntermediate;

            var z = pos.z;
            pos.z = 0;
            if (MaxDeflectionMode) {
                pos = pos.normalized;
                z = 0;
                switch (LimitsShape) {
                    case LimitsShapes.Circle:
                        /* noop */
                        break;
                    case LimitsShapes.Square:
                        pos = MathUtils.GetRectRayIntersection(rect, Vector3.zero, pos, out var _);
                        break;
                    default:
                        throw new Exception ("Invalid limits shape");
                }
            }
            else {
                z = Mathf.Clamp(z, -1.0f, 1.0f);
                if (centeringDeflection < 0f || Mathf.Abs(z) < centeringDeflection)
                    z = Mathf.Lerp(z, 0, centeringSpeed * deltaTime);

                switch (LimitsShape) {
                    case LimitsShapes.Circle:
                        pos = Vector2.ClampMagnitude(pos, 1.0f);
                        if (centeringDeflection < 0f || pos.magnitude < centeringDeflection)
                            pos = Vector3.Lerp(pos, Vector3.zero, centeringSpeed * deltaTime);
                        break;
                    case LimitsShapes.Square:
                        pos = MathUtils.GetRectLineIntersection(rect, Vector3.zero, pos);
                        if (centeringDeflection < 0f || Mathf.Abs(pos.x) < centeringDeflection)
                            pos.x = Mathf.Lerp(pos.x, 0, centeringSpeed * deltaTime);
                        if (centeringDeflection < 0f || Mathf.Abs(pos.y) < centeringDeflection)
                            pos.y = Mathf.Lerp(pos.y, 0, centeringSpeed * deltaTime);
                        break;
                    default:
                        throw new Exception ("Invalid limits shape");
                }
            }
            pos.z = z;

            needToUpdateInputs = true;
        }

        deltas = Vector3.zero;
    }

    private static void CalcInputs(
        PilotPlayerState pilotPlayerState, Pilot pilot, Player player, float pilotStrength,
        ref ControlInputs controlInputs, ref float pitchInput, ref float rollInput, ref float yawInput) {

        if (!needToUpdateInputs || modeDatum.Count == 0 || UIBindings.Game.GetFlightHUDComponent() == null || pilot.aircraft.cockpit.IsDetached()) {
            return;
        }
        else if (pilotStrength < 0.2f) {
            controlInputs.yaw = controlInputs.pitch = controlInputs.roll = 0.0f;
            return;
        }

        var posNorm = pos;
        posNorm.y *= -1;

        float GetValue(InputAxis inputAxis) {
            switch (inputAxis) {
                case InputAxis.X:
                    return posNorm.x;
                case InputAxis.Y:
                    return posNorm.y;
                case InputAxis.Z:
                    return posNorm.z;
                default:
                    throw new Exception ("invalid input axis");
            }
        }

        float CalcInput(float multiplier, CubicResponseCurve curve, InputAxis inputAxis) {
            if (multiplier == 0)
                return 0;
            else
                return multiplier * curve.Calc(GetValue(inputAxis));
        }

        var modeData = modeDatum[Mode];

        rollInput = CalcInput(modeData.OutputMultiplier.Roll, modeData.StaticCurves.Roll, modeData.AxisMap.Roll);
        pitchInput = CalcInput(modeData.OutputMultiplier.Pitch, modeData.StaticCurves.Pitch, modeData.AxisMap.Pitch);
        yawInput = CalcInput(modeData.OutputMultiplier.Yaw, modeData.StaticCurves.Yaw, modeData.AxisMap.Yaw);

        if (RollControlsYawOnTheGround && pilot.aircraft.radarAlt < pilot.aircraft.definition.spawnOffset.y + 0.2f) {
            yawInput = rollInput;
            rollInput = 0;
        }

        lastOutputs.Roll = rollInput;
        lastOutputs.Pitch = pitchInput;
        lastOutputs.Yaw = yawInput;

        var axisPitch = player.GetAxis("Pitch");
        var axisRoll = player.GetAxis("Roll");
        var axisYaw = player.GetAxis("Yaw");

        pitchInput = Mathf.Clamp(pitchInput + axisPitch, -1f, 1f);
        rollInput = Mathf.Clamp(rollInput + axisRoll, -1f, 1f);
        yawInput = Mathf.Clamp(yawInput + axisYaw, -1f, 1f);

        controlInputs.yaw = yawInput;
        controlInputs.pitch = pitchInput;
        controlInputs.roll = rollInput;
    }

    private static void UpdateVirtualJoystickState() {
        if (VectorPlacement != VectorPlacements.OFF && !IsVirtualJoystickActive())
            SetVirtualJoystickActive(true);
        else if (VectorPlacement == VectorPlacements.OFF && IsVirtualJoystickActive())
            SetVirtualJoystickActive(false);
        if (IsVirtualJoystickActive()) {
            SetVirtualJoystickPos(pos);
        }
    }

    private static Vector3 GetVirtualJoystickPos() {
        var flightHUD = UIBindings.Game.GetFlightHUDComponent();
        if (flightHUD == null)
            return Vector3.zero;
        return flightHUD.virtualJoystickPos.gameObject.transform.localPosition;
    }

    private static void SetVirtualJoystickPos(Vector3 pos) {
        pos *= MaxDeflection;

        if (VectorPlacement == VectorPlacements.OFF)
            return;
        var flightHUD = UIBindings.Game.GetFlightHUDComponent();
        if (flightHUD != null) {
            if (flightHUD != currentFlightHUD) {
                currentFlightHUD = flightHUD;
                UpdateVirtualJoystickPlacement();
            }
            var z = pos.z;
            pos.z = 0;
            flightHUD.SetVirtualJoystick(pos);
            if (ShiftVector) {
                var transform = flightHUD.virtualJoystickPos.gameObject.transform;
                var lp = transform.localPosition;
                lp.x = lp.x + z;
                transform.localPosition = lp;
            }
        }
    }

    private static bool IsVirtualJoystickActive() {
        var flightHUD = UIBindings.Game.GetFlightHUDComponent();
        if (flightHUD == null)
            return false;
        return flightHUD.virtualJoystickPos.gameObject.activeSelf;
    }

    private static void UpdateVirtualJoystickPlacement() {
        Transform parent = null;
        switch (VectorPlacement) {
            case VectorPlacements.HUD:
                parent = UIBindings.Game.GetFlightHUDCenterTransform();
                break;
            case VectorPlacements.HMD:
                parent = UIBindings.Game.GetCombatHUDTransform();
                break;
        }
        if (parent != null) {
            var flightHUD = UIBindings.Game.GetFlightHUDComponent();
            flightHUD.virtualJoystickPos.gameObject.transform.SetParent(parent, true);
        }
    }

    private static void SetVirtualJoystickActive(bool active) {
        var flightHUD = UIBindings.Game.GetFlightHUDComponent();
        if (flightHUD != null) {
            flightHUD.virtualJoystickPos.gameObject.SetActive(value: active);
            if (active)
                UpdateVirtualJoystickPlacement();
        }
    }

    private static void OnSetMode() {
        var modeData = modeDatum[Mode];

        initial = Vector3.zero;
        intermediate = Vector3.zero;

        if (InputAxesResetMode == InputAxesResetModes.None) {
            /* noop */
        }
        else if (InputAxesResetMode == InputAxesResetModes.Scale) {
            pos = Vector3.Scale(modeData.InputMultiplierOnEnter, pos);
            SetVirtualJoystickPos(pos);
        }
        else if (InputAxesResetMode == InputAxesResetModes.Recalculate) {
            Vector3 posNorm = Vector3.zero;

            /* In case several output axes are bound to one input axis,
               choose recalculated input axis value that is smallest by absolute amount. */
            void SetValue(InputAxis sourceAxis, float value) {
                switch (sourceAxis) {
                    case InputAxis.X:
                        if (posNorm.x == 0.0f || Mathf.Abs(value) < Mathf.Abs(posNorm.x)) {
                            posNorm.x = value;
                        }
                        break;
                    case InputAxis.Y:
                        if (posNorm.y == 0.0f || Mathf.Abs(value) < Mathf.Abs(posNorm.y)) {
                            posNorm.y = value;
                        }
                        break;
                    case InputAxis.Z:
                        if (posNorm.z == 0.0f || Mathf.Abs(value) < Mathf.Abs(posNorm.z)) {
                            posNorm.z = value;
                        }
                        break;
                    default:
                        throw new Exception ("invalid source axis");
                }
            }

            void UpdateAxis(InputAxis inputAxis, float multiplier, CubicResponseCurve curve, float lastOutput) {
                var value = multiplier == 0.0f ? 0.0f : curve.CalcArg(lastOutput / multiplier);
                SetValue(inputAxis, value);
            }

            UpdateAxis(modeData.AxisMap.Roll, modeData.OutputMultiplier.Roll, modeData.StaticCurves.Roll, lastOutputs.Roll);
            UpdateAxis(modeData.AxisMap.Pitch, modeData.OutputMultiplier.Pitch, modeData.StaticCurves.Pitch, lastOutputs.Pitch);
            UpdateAxis(modeData.AxisMap.Yaw, modeData.OutputMultiplier.Yaw, modeData.StaticCurves.Yaw, lastOutputs.Yaw);

            posNorm.y *= -1;
            SetVirtualJoystickPos(posNorm);
        }
        else
            throw new Exception ($"Unknown input axes reset mode: {InputAxesResetMode}");

        string report = $"Virtual joystick: <b>{modeData.Name}</b>";
        UIBindings.Game.DisplayToast(report, 2f);
    }

    private static void OnEnabled() {
        string report = $"Virtual joystick: <b>{(Enabled ? "enabled" : "disabled")}</b>";
        UIBindings.Game.DisplayToast(report, 2f);
    }

    [HarmonyPatch(typeof(HUDBoresightState), "DisplayLead")]
    public class OnHUDBoresightStateDisplayLead {
        static void Prefix() {
            inHUDBoresightStateDisplayLead = true;
        }
        static void Postfix() {
            inHUDBoresightStateDisplayLead = false;
        }
    }

    [HarmonyPatch(typeof(ControlsFilter), "GetAim")]
    public class OnControlsFilterGetAim {
        static void Postfix(Unit target, ref GlobalPosition? aimPoint, ref GlobalPosition? impactPoint, Aircraft ___aircraft) {
            if (!inHUDBoresightStateDisplayLead)
                return;
            else if (DampingAngle < 0 || aimPoint == null || !aimPoint.HasValue) {
                angle = -1f;
            }
            else {
                var weapon = ___aircraft.weaponManager.currentWeaponStation.Weapons[0];
                var forward = weapon.transform.forward;
                var toAimPoint = aimPoint.Value.ToLocalPosition() - ___aircraft.transform.position;
                angle = Vector3.Angle(toAimPoint, forward);
            }
        }
    }

    private class ModeData {
        public string Name = "";
        public Axes<InputAxis> AxisMap = new (InputAxis.Z, InputAxis.Y, InputAxis.X);
        //Input axes (x, y, z)
        public Vector3 Sens = Vector3.one;
        public Vector3 InputMultiplierOnEnter = Vector3.one;
        public Axes<float> OutputMultiplier = new (1, 1, 1);
        public Axes<float> StaticCurvature {
            get;
            set {
                field = value;
                StaticCurves.Roll.C = value.Roll;
                StaticCurves.Pitch.C = value.Pitch;
                StaticCurves.Yaw.C = value.Yaw;
            }
        } = new (0, 0, 0);
        public Vector3 DynamicCurvature {
            get;
            set {
                field = value;
                DynamicCurves.x.C = value.x;
                DynamicCurves.y.C = value.y;
                DynamicCurves.z.C = value.z;
            }
        } = Vector3.zero;

        public Axes<CubicResponseCurve> StaticCurves = new (new(), new(), new());
        public class DynamicCurvesClass {
            public CubicResponseCurve x = new ();
            public CubicResponseCurve y = new ();
            public CubicResponseCurve z = new ();
        }
        public DynamicCurvesClass DynamicCurves = new ();
    }

    private static List<ModeData> modeDatum = new ();

    private static MethodInfo playerThrottleAxis1ControlsInfo = AccessTools.Method(typeof(PilotPlayerState), "PlayerThrottleAxis1Controls");
    private static int lastMode = Mode;
    private static float prevDeflection = 1.0f;
    private static float centeringSpeed = 0.0f;
    private static float centeringDeflection = 0.0f;
    private static Rect rect = new (-1.0f, -1.0f, 2.0f, 2.0f);
    private static Vector3 initial = Vector3.zero;
    private static Vector3 intermediate = Vector3.zero;
    private static Vector3 deltas = Vector3.zero;
    private static Vector3 pos = Vector3.zero;
    private static Axes<float> lastOutputs = new (0, 0, 0);
    private static bool needToUpdateInputs = false;
    private static FlightHud currentFlightHUD = null;
    private static CubicResponseCurve dampingCurve = new ();
    private static float halfDampingAngle = -1f;
    private static float angle = -1f;
    private static bool inHUDBoresightStateDisplayLead = false;

    private static bool initialized = false;
}

[HarmonyPatch(typeof(MainMenu), "Start")]
class KeyAxesPlugin {
    static void Postfix() {
        KeyAxesComponent.Init(); 
    }
}

public class KeyAxesComponent {
    public static void Init() {
        if (!initialized) {
            Plugin.Log("[KA] Initializing Key axes component");

            var harmony = new Harmony("yessidor.no_tactitools_plus.key_axes");
            harmony.PatchAll(typeof(OnPilotPlayerStatePlayersControls));
            harmony.PatchAll(typeof(OnPlayerGetAxis));
            harmony.PatchAll(typeof(OnPlayerGetAxisPrev));
            harmony.PatchAll(typeof(OnPlayerGetAxisRaw));
            harmony.PatchAll(typeof(OnPlayerGetAxisRawPrev));

            var bindings = new List<BindingHelper.Binding> ();
            for (int i = 0; i < axisNames.Length; i++) {
                var name = axisNames[i];
                var kaxis = axisData[name].axis;
                var vars = Plugin.keyAxes[i];
                InputCatcher.RegisterButtonInput(
                    vars.IncKey,
                    0.0f,
                    onRelease: () => { kaxis.IncKeyPressed = false; },
                    onPress: () => { kaxis.IncKeyPressed = true; }
                    );
                InputCatcher.RegisterButtonInput(
                    vars.DecKey,
                    0.0f,
                    onRelease: () => { kaxis.DecKeyPressed = false; },
                    onPress: () => { kaxis.DecKeyPressed = true; }
                    );
                InputCatcher.RegisterAxisInput(
                    vars.EncoderAxis,
                    onMoveRaw: (float value, float valuePrev, float delta) => { kaxis.OnMoveRaw(value); },
                    convertAbsToRel: true
                    );
                bindings.Add(new BindingHelper.Binding (kaxis, "BuildUpSpeed", vars.BuildUpSpeed));
                bindings.Add(new BindingHelper.Binding (kaxis, "DecaySpeed", vars.DecaySpeed));
                bindings.Add(new BindingHelper.Binding (kaxis, "DecayDelay", vars.DecayDelay));
                bindings.Add(new BindingHelper.Binding (kaxis, "TwoKeyReset", vars.TwoKeyReset));
                bindings.Add(new BindingHelper.Binding (kaxis, "EncoderSens", vars.EncoderSens));
                bindings.Add(new BindingHelper.Binding (kaxis.KeyDynamicCurve, "C", vars.DynamicCurvature));
                bindings.Add(new BindingHelper.Binding (kaxis.EncoderDynamicCurve, "C", vars.EncoderDynamicCurvature));
                bindings.Add(new BindingHelper.Binding (kaxis.StaticCurve, "C", vars.StaticCurvature));

                kaxis.Min = -1f;
                kaxis.Max = 1f;

                vars.StaticOffset.Value = Math.Clamp(vars.StaticOffset.Value, kaxis.Min, kaxis.Max);
                bindings.Add(new BindingHelper.Binding (kaxis.StaticCurve, "Y0", vars.StaticOffset));

                vars.InitialValue.Value = Math.Clamp(vars.InitialValue.Value, kaxis.Min, kaxis.Max);
                kaxis.SetResult(vars.InitialValue.Value);
            }

            bindings.Add(new BindingHelper.Binding (typeof(KeyAxesComponent), "ConfirmAirbrakeDeployment", Plugin.keyAxesConfirmAirbrakeDeployment));

            BindingHelper.ApplyBindings(bindings);

            var yawAxis = axisData["Yaw"].axis;
            var pitchAxis = axisData["Pitch"].axis;
            var rollAxis = axisData["Roll"].axis;
            InputCatcher.RegisterButtonInput(
                Plugin.yprAxesResetKey,
                Plugin.PressDelay.Value,
                onPress: () => { yawAxis.SetResult(yawAxis.StaticCurve.Y0); },
                onLongPress: () => {
                    pitchAxis.SetResult(pitchAxis.StaticCurve.Y0);
                    rollAxis.SetResult(rollAxis.StaticCurve.Y0);
                }
            );
            InputCatcher.RegisterButtonInput(
                Plugin.allAxesResetKey,
                Plugin.PressDelay.Value,
                onPress: () => { foreach (var data in axisData.Values) data.axis.SetResult(data.axis.StaticCurve.Y0); }
            );

            InputCatcher.onUpdate += UpdateAxes;

            initialized = true;

            Plugin.Log("[KA] Initialized Key axes component");
        }
    }

    private static void AdjustResult(string actionName, ref float result) {
        if (actionName == "Custom Axis 1" || (!PlayerSettings.throttleUseNegative && actionName == "Throttle"))
            result = Mathf.InverseLerp(-1f, 1f, result);
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(PilotPlayerState), "PlayerControls")]
    public class OnPilotPlayerStatePlayersControls {
        static void Postfix(ref ControlInputs ___controlInputs, ref Player ___player) {
            ___controlInputs.brake = Mathf.InverseLerp(-1f, 1f, ___player.GetAxis("Brake"));
        }
    }

    [HarmonyPatch(typeof(Player), "GetAxis", typeof(string))]
    public class OnPlayerGetAxis {
        public static void Postfix(ref float __result, ref string actionName) {
            if (UpdateAxis(actionName))
                __result += axisData[actionName].raw;
            AdjustResult(actionName, ref __result);
        }
    }

    [HarmonyPatch(typeof(Player), "GetAxisPrev", typeof(string))]
    public class OnPlayerGetAxisPrev {
        public static void Postfix(ref float __result, ref string actionName) {
            if (UpdateAxis(actionName))
                __result += axisData[actionName].rawPrev;
            AdjustResult(actionName, ref __result);
        }
    }

    [HarmonyPatch(typeof(Player), "GetAxisRaw", typeof(string))]
    public class OnPlayerGetAxisRaw {
        public static void Postfix(ref float __result, ref string actionName) {
            if (UpdateAxis(actionName))
                __result += axisData[actionName].raw;
            AdjustResult(actionName, ref __result);
        }
    }

    [HarmonyPatch(typeof(Player), "GetAxisRawPrev", typeof(string))]
    public class OnPlayerGetAxisRawPrev {
        public static void Postfix(ref float __result, ref string actionName) {
            if (UpdateAxis(actionName))
                __result += axisData[actionName].rawPrev;
            AdjustResult(actionName, ref __result);
        }
    }

    private class AxisData {
        public KeyAndEncoderAxis axis;
        public float raw, rawPrev, time;

        public AxisData (KeyAndEncoderAxis axis = null, float raw = 0, float rawPrev = 0, float time = 0) {
            this.axis = axis ?? new KeyAndEncoderAxis ();
            this.raw = raw;
            this.rawPrev = rawPrev;
            this.time = time;
        }
    }

    public enum ConfirmAirbrakeDeploymentMode { None, Press, Hold };
    public static ConfirmAirbrakeDeploymentMode ConfirmAirbrakeDeployment = ConfirmAirbrakeDeploymentMode.Press;

    private static bool initialized = false;
    private static string[] axisNames = new string [] { "Pitch", "Roll", "Yaw", "Throttle", "Brake", "Custom Axis 1", "Zoom View" };
    private static Dictionary<string, AxisData> axisData = axisNames.ToDictionary(name => name, name => new AxisData ());

    private enum AirBrakeState { Retracted, WaitingForRelease, WaitingForPress, Deployed };
    private static AirBrakeState airBrakeState = AirBrakeState.Retracted;
    private static readonly float throttleDelta = 0.001f;
    private static bool prevDecKeyPressed = false;
    private static float prevEncoderDelta = 0f;

    private static void ProcessAxisData(string actionName, AxisData data) {
        var time = Time.unscaledTime;

        if (data.time != time) {
            var raw = data.axis.Compute();
            var hasChanged = raw != data.raw;
            if (actionName == "Throttle") {
                var axis = data.axis;
                var adjustedThrottle = axis.Min + throttleDelta;
                if (ConfirmAirbrakeDeployment == ConfirmAirbrakeDeploymentMode.Press) {
                    if (axis.IncKeyPressed || (airBrakeState != AirBrakeState.WaitingForRelease && axis.EncoderDelta > 0)) {
                        airBrakeState = AirBrakeState.Retracted;
                        prevDecKeyPressed = false;
                    }
                    else if (airBrakeState == AirBrakeState.Retracted) {
                        if ((axis.DecKeyPressed || axis.EncoderDelta <= 0) && raw == axis.Min) {
                            raw = adjustedThrottle;
                            hasChanged = true;
                            airBrakeState = AirBrakeState.WaitingForRelease;
                        }
                    }
                    else if (airBrakeState == AirBrakeState.WaitingForRelease) {
                        if ((prevDecKeyPressed && !axis.DecKeyPressed) || (prevEncoderDelta < 0 && axis.EncoderDelta >= 0)) {
                            airBrakeState = AirBrakeState.WaitingForPress;
                        }
                        raw = adjustedThrottle;
                        hasChanged = true;
                    }
                    else if (airBrakeState == AirBrakeState.WaitingForPress) {
                        if ((!prevDecKeyPressed && axis.DecKeyPressed) || axis.EncoderDelta < 0) {
                            raw = axis.Min;
                            airBrakeState = AirBrakeState.Deployed;
                        }
                        else {
                            raw = adjustedThrottle;
                        }
                        hasChanged = true;
                    }
                    else if (airBrakeState == AirBrakeState.Deployed) {
                        raw = axis.Min;
                        hasChanged = true;
                    }
                }
                else if (ConfirmAirbrakeDeployment == ConfirmAirbrakeDeploymentMode.Hold) {
                    if ((!axis.DecKeyPressed || axis.EncoderDelta >= 0) && raw == data.axis.Min) {
                        raw = adjustedThrottle;
                        hasChanged = true;
                    }
                }

                prevDecKeyPressed = axis.DecKeyPressed;
                prevEncoderDelta = axis.EncoderDelta;
            }

            if (hasChanged) {
                data.time = time;
                data.rawPrev = data.raw;
                data.raw = raw;
            }
        }
    }

    private static bool UpdateAxis(string actionName) {
        var r = axisData.TryGetValue(actionName, out var data);
        if (r) {
            ProcessAxisData(actionName, data);
        }
        return r;
    }

    private static void UpdateAxes() {
        foreach ((var actionName, var data) in axisData) {
            ProcessAxisData(actionName, data);
        }
    }
};
