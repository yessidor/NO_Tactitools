using HarmonyLib;
using UnityEngine;
using UnityEngine.UI; //Image
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

class TargetArrowsComponent {
    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class OnMainMenuStart {
        private static bool initialized = false;

        static void Postfix() {
            if (!initialized) {
                Plugin.Log($"[TAs] Target Arrows plugin starting !");

                Plugin.harmony.PatchAll(typeof(OnHUDFunctionsPinToScreenEdge));
                Plugin.harmony.PatchAll(typeof(OnCombatHUDUpdateMarkers));

                var bindings = new BindingHelper.Binding[] {
                    new (typeof(TargetArrowsComponent), "ArrowColor", Plugin.TargetArrows.ArrowColor),
                    new (typeof(TargetArrowsComponent), "MatchMarkerColor", Plugin.TargetArrows.MatchMarkerColor),
                    new (typeof(TargetArrowsComponent), "ArrowScale", Plugin.TargetArrows.ArrowScale),
                    new (typeof(TargetArrowsComponent), "NumArrows", Plugin.TargetArrows.NumArrows),
                };
                BindingHelper.ApplyBindings(bindings);

                initialized = true;

                Plugin.Log($"[TAs] Target Arrows plugin successfully started !");
            }
        }
    }

    public static int NumArrows {
        set {
            field = value;
            CleanUpArrows(value);
        }
        get;
    } = 1;
    public static Color ArrowColor {
        set {
            field = value;
            if (combatHUD != null) {
                targetArrowCache.GetValue(combatHUD).color = value;
                foreach (var arrow in arrows)
                    arrow.color = value;
            }
        }
        get;
    } = Color.green;
    public static float ArrowScale {
        set {
            field = value;
            if (combatHUD != null) {
                var scale = Vector3.one * value;
                targetArrowCache.GetValue(combatHUD).transform.localScale = scale;
                foreach (var arrow in arrows)
                    arrow.transform.localScale = scale;
            }
        }
        get;
    } = 1f;
    public static bool MatchMarkerColor = true;

    private static FieldInfo hudumTransformInfo = AccessTools.Field(typeof(HUDUnitMarker), "_transform");
    private static TraverseCache<CombatHUD, Image> targetArrowCache = new ("targetArrow");
    private static TraverseCache<CombatHUD, Text> targetTextCache = new ("targetText");
    private static TraverseCache<CombatHUD, Transform> targetArrowTailCache = new ("targetArrowTail");

    private static List<Image> arrows = new ();
    private static CombatHUD combatHUD = null;

    //angle is in radians
    private static void SetArrow(int i, bool enabled, Vector3 position, float angle, Color? color) {
        var pos = enabled ? position : Vector3.zero;
        var ang = enabled ? new Vector3(0f, 0f, angle * Mathf.Rad2Deg - 90f) : Vector3.zero;
        if (i == 0) {
            //set main arrow position and angle
            combatHUD.SetTargetArrow(enabled: enabled, pos, ang);
            targetArrowCache.GetValue(combatHUD).color = color ?? ArrowColor;
            targetTextCache.GetValue(combatHUD).color = color ?? ArrowColor;
            //Fixes TARGET text jumping
            //In SetTargetArrow(), targetText transform position should be assigned to targetArrowTail position after targetArrow transform position was assigned to new value
            targetTextCache.GetValue(combatHUD).transform.position = targetArrowTailCache.GetValue(combatHUD).position;
        }
        else {
            //set subsequent arrow position and angle
            //create arrow if needed
            if (i > arrows.Count) {
                var newArrow = GameObject.Instantiate(targetArrowCache.GetValue(combatHUD), combatHUD.iconLayer);
                newArrow.color = ArrowColor;
                newArrow.transform.localScale = Vector3.one * ArrowScale;
                newArrow.raycastTarget = false;
                newArrow.enabled = false;
                arrows.Add(newArrow);
            }
            var arrow = arrows[i - 1];
            arrow.enabled = enabled;
            arrow.transform.position = pos;
            arrow.transform.localEulerAngles = ang;
            arrow.color = color ?? ArrowColor;
        }
    }

    private static void CleanUpArrows(int startIdx) {
        if (startIdx == 0 && combatHUD != null)
            combatHUD.SetTargetArrow(enabled: false, Vector3.zero, Vector3.zero);
        var arrowIdx = startIdx == 0 ? 0 : startIdx - 1;
        for (int idx = arrowIdx; idx < arrows.Count; idx++) {
            var arrow = arrows[idx];
            if (arrow == null)
                continue;
            arrow.enabled = false;
            UnityEngine.Object.Destroy(arrow.gameObject);
        }
        if (arrowIdx < arrows.Count)
            arrows.RemoveRange(arrowIdx, arrows.Count - arrowIdx);
    }

    [HarmonyPatch(typeof(CombatHUD), "UpdateMarkers")]
    public class OnCombatHUDUpdateMarkers {
        public static void Postfix(CombatHUD __instance, ref Dictionary<Unit, HUDUnitMarker> ___markerLookup, ref List<Unit> ___targetList) {
            if (__instance != combatHUD) {
                combatHUD = __instance;
                CleanUpArrows(0);
                targetArrowCache.GetValue(combatHUD).color = ArrowColor;
                var scale = Vector3.one * ArrowScale;
                targetArrowCache.GetValue(combatHUD).transform.localScale = scale;
            }

            var hq = __instance.aircraft.NetworkHQ;
            if (hq == null || ___targetList.Count == 0) {
                CleanUpArrows(0);
            }
            else {
                int i = 0;
                foreach (var target in ___targetList) {
                    if (hq.TryGetKnownPosition(target, out var knownPosition)) {
                        bool arrowState = HUDFunctions.PinToScreenEdge(knownPosition.ToLocalPosition(), out var rayToScreen, out var arrowAngle);
                        if (___markerLookup.TryGetValue(target, out var targetMarker)) {
                            targetMarker.image.enabled = !arrowState;
                            if (!arrowState)
                                ((Transform)hudumTransformInfo.GetValue(targetMarker)).position = rayToScreen;
                            if (NumArrows != 0 && i < NumArrows) {
                                var arrowColor = MatchMarkerColor ? targetMarker.image.color : ArrowColor;
                                SetArrow(i, arrowState, rayToScreen, arrowAngle, arrowColor);
                                i++;
                            }
                        }
                    }
                }
                CleanUpArrows(i);
            }
        }
    }

    //Fix to avoid target arrow suddenly sticking to another screen side when angle between camera forward vector and direction to target passes 90 deg
    [HarmonyPatch(typeof(HUDFunctions), "PinToScreenEdge")]
    public class OnHUDFunctionsPinToScreenEdge {
        public static bool Prefix(ref bool __result, Vector3 coords, out Vector3 rayToScreen, out float arrowAngle) {
            __result = ArrowHelpers.PinToScreenEdge(coords, out rayToScreen, out arrowAngle);
            arrowAngle += 0.5f * Mathf.PI;
            return false;
        }
    }
}
