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
class HUDCenterDirectionPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[HCD] HUD Center Direction plugin starting !");

            Plugin.harmony.PatchAll(typeof(HUDCenterDirectionComponent.OnCombatHUDLateUpdate));

            var bindings = new BindingHelper.Binding[] {
                new (typeof(HUDCenterDirectionComponent), "ArrowColor", Plugin.HUDCenterDirection.ArrowColor),
                new (typeof(HUDCenterDirectionComponent), "ArrowScale", Plugin.HUDCenterDirection.ArrowScale),
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;

            Plugin.Log($"[HCD] HUD Center Direction plugin successfully started !");
        }
    }
}


class HUDCenterDirectionComponent {
    public static Color ArrowColor { set { if (arrow != null) arrow.color = value; field = value; } get; } = Color.yellow;
    public static float ArrowScale { set { if (arrow != null) arrow.transform.localScale = Vector3.one * value; field = value; } get; } = 1f;

    private static TraverseCache<CombatHUD, Image> targetArrowCache = new ("targetArrow");
    private static Image arrow;
    private static CombatHUD combatHUD;

    private static void Init() {
        var currentCombatHUD = UIBindings.Game.GetCombatHUDComponent();
        if (currentCombatHUD != combatHUD) {
            combatHUD = currentCombatHUD;

            if (arrow != null) {
                arrow.enabled = false;
                UnityEngine.Object.Destroy(arrow.gameObject);
            }
            arrow = GameObject.Instantiate(targetArrowCache.GetValue(combatHUD), combatHUD.iconLayer);
            arrow.color = ArrowColor;
            arrow.transform.localScale = Vector3.one * ArrowScale;
            arrow.raycastTarget = false;
            arrow.enabled = false;
        }
    }

    private static void Update() {
        Init();
        var aircraft = combatHUD.aircraft;
        if (aircraft == null) {
            SetArrow(enabled: false, Vector3.zero, 0f);
            return;
        }
        var aircraftTransform = aircraft.transform;
        if (aircraftTransform == null) {
            SetArrow(enabled: false, Vector3.zero, 0f);
            return;
        }
        Vector3 position = aircraftTransform.position + aircraftTransform.forward * 1000f;
        if (ArrowHelpers.PinToScreenEdge(position, out var arrowPos, out var z)) {
            SetArrow(enabled: true, arrowPos, z * Mathf.Rad2Deg);
        }
        else {
            SetArrow(enabled: false, Vector3.zero, 0f);
        }
    }

    private static void SetArrow(bool enabled, Vector3 position, float angle) {
        arrow.enabled = enabled;
        if (enabled) {
            arrow.transform.position = position;
            arrow.transform.localEulerAngles = new Vector3 (0f, 0f, angle);
        }
    }

    [HarmonyPatch(typeof(CombatHUD), "LateUpdate")]
    public class OnCombatHUDLateUpdate {
        public static void Postfix() {
            Update();
        }
    }
}
