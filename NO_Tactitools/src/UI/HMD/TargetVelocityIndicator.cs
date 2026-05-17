using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

[HarmonyPatch(typeof(MainMenu), "Start")]
public class TargetVelocityIndicatorPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[TVI] Target Velocity Indicator plugin starting !");
            Plugin.harmony.PatchAll(typeof(TargetVelocityIndicatorComponent.OnCombatHUDLateUpdate));

            BindingHelper.Binding[] bindings = new BindingHelper.Binding[] {
                new (typeof(TargetVelocityIndicatorComponent), "MaxSpeed", Plugin.TargetVelocityIndicator.MaxSpeed),
                new (typeof(TargetVelocityIndicatorComponent), "MaxLength", Plugin.TargetVelocityIndicator.MaxLength),
                new (typeof(TargetVelocityIndicatorComponent), "DotStep", Plugin.TargetVelocityIndicator.DotStep)
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log($"[TVI] Target Velocity Indicator plugin started !");
        }
    }
}

class TargetVelocityIndicatorComponent {
    private static readonly TraverseCache<CombatHUD, Dictionary<Unit, HUDUnitMarker>> markerLookupCache = new("markerLookup");
    private static CombatHUD combatHUD;
    private static RectTransform combatHUDRectTransform;
    private static UIBindings.Draw.UILabel indicator;
    private static List<UIBindings.Draw.UILabel> dots = new ();
    public static float MaxSpeed = 1000.0f; //kph
    public static float MaxLength = 100.0f; //pixels
    public static float DotStep = 10.0f; //pixels
    private static bool initialized = false;

    private static void Update() {
        void Disable() {
            indicator.GetGameObject().SetActive(false);
            foreach (var dot in dots)
                dot.GetGameObject().SetActive(false);
            return;
        }

        CombatHUD currentCombatHUD = UIBindings.Game.GetCombatHUDComponent();
        if (currentCombatHUD == null) {
            return;
        }
        if (combatHUD != currentCombatHUD) {
            initialized = false;
            combatHUD = currentCombatHUD;
        }

        Dictionary<Unit, HUDUnitMarker> markerLookup = markerLookupCache.GetValue(currentCombatHUD);

        var camera = SceneSingleton<CameraStateManager>.i.mainCamera;
        if (camera == null) {
            return;
        }

        if (!initialized) {
            combatHUDRectTransform = UIBindings.Game.GetCombatHUDTransform()?.GetComponent<RectTransform>();
            indicator = new UIBindings.Draw.UILabel (
                name: "TargetVelocityIndicatorMarker",
                position: new Vector2 (0,0),
                UIParent: combatHUDRectTransform,
                color: Color.green,
                backgroundOpacity: 0.0f);
            dots.Clear();

            initialized = true;
        }

        var targets = GameBindings.Player.TargetList.GetTargets();
        if (targets == null) {
            Disable();
            return;
        }
        if (targets.Count == 0) {
            Disable();
            return;
        }

        var target = targets[0];
        var marker = markerLookup[target];
        Rigidbody rb = target.rb;
        if (rb == null) {
            Disable();
            return;
        }
        if (marker.outdated) {
            Disable();
            return;
        }

        var velocity = rb.velocity;
        DirectionAndMagnitude(velocity, out var velocityDirection, out var speed);
        if (speed < 0.1f) {
            Disable();
            return;
        }
        //speed is in m/s, MaxSpeed is in km/h, 1 m/s = 3.6 km/h
        Vector3 screenOffset = 3.6f * speed / MaxSpeed * MaxLength * velocityDirection;
        DirectionAndMagnitude(screenOffset, out var screenOffsetDirection, out var screenOffsetMagnitude);
        screenOffsetMagnitude = Mathf.Clamp(screenOffsetMagnitude, 0.0f, MaxLength);
        screenOffset = screenOffsetDirection * screenOffsetMagnitude;
        screenOffset.x = Vector3.Dot(camera.transform.right, screenOffset);
        screenOffset.y = Vector3.Dot(camera.transform.up, screenOffset);
        screenOffset.z = Vector3.Dot(camera.transform.forward, screenOffset);
        DirectionAndMagnitude(screenOffset, out screenOffsetDirection, out screenOffsetMagnitude);
        var startScreenPosition = camera.WorldToScreenPoint(target.GlobalPosition().ToLocalPosition());
        var endScreenPosition = startScreenPosition + screenOffset;
        if (!IsPointOnscreen(startScreenPosition) && !IsPointOnscreen(endScreenPosition)) {
            Disable();
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            combatHUDRectTransform,
            endScreenPosition,
            null,
            out Vector2 endCanvasPosition);

        var indicatorMarker = Vector3.Dot(camera.transform.forward, velocity) > 0 ? "o" : "x";
        indicator.SetText(indicatorMarker);
        indicator.SetPosition(endCanvasPosition);
        indicator.GetGameObject().SetActive(true);

        var numDots = (int)(screenOffsetMagnitude / DotStep);
        int i = 0;
        for (; i < numDots; i++) {
            if (dots.Count == i) {
                var dot = new UIBindings.Draw.UILabel (
                    name: string.Format("TargetVelocityIndicatorDot{0}", i),
                    position: new Vector2 (0, 0),
                    UIParent: combatHUDRectTransform,
                    color: Color.green,
                    backgroundOpacity: 0.0f);
                dot.SetText("·");
                dots.Add(dot);
            }
            float currentOffsetMagnitude = DotStep * i;
            Vector3 dotScreenPosition = startScreenPosition + currentOffsetMagnitude * screenOffsetDirection;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                combatHUDRectTransform,
                dotScreenPosition,
                null,
                out Vector2 dotCanvasPosition);
            var currentDot = dots[i];
            currentDot.SetPosition(dotCanvasPosition);
            currentDot.SetOpacity(Mathf.InverseLerp(0.0f, screenOffsetMagnitude, currentOffsetMagnitude));
            currentDot.GetGameObject().SetActive(true);
        }
        for (int j = i; j < dots.Count; j++) {
            dots[j].GetGameObject().SetActive(false);
        }
    }

    private static bool IsPointOnscreen(Vector3 screenPoint, float margin = 10f) {
        return screenPoint.x > -margin && screenPoint.x < Screen.width + margin &&
            screenPoint.y > -margin && screenPoint.y < Screen.height + margin &&
            screenPoint.z > 0;
    }

    private static void DirectionAndMagnitude(Vector3 vector, out Vector3 direction, out float magnitude) {
        magnitude = Mathf.Sqrt(vector.x*vector.x + vector.y*vector.y + vector.z*vector.z);
        if (magnitude == 0.0f)
            direction = Vector3.zero;
        else {
            direction.x = vector.x / magnitude;
            direction.y = vector.y / magnitude;
            direction.z = vector.z / magnitude;
        }
    }

    [HarmonyPatch(typeof(CombatHUD), "LateUpdate")]
    public class OnCombatHUDLateUpdate {
        public static void Postfix(ref CombatHUD __instance) {
            Update();
        }
    }
}
