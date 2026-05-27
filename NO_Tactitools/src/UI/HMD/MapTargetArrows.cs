using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NO_Tactitools.Core;

[HarmonyPatch(typeof(MainMenu), "Start")]
public class MapTargetArrowsPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[MTA] Map Target Arrows plugin starting !");

            Plugin.harmony.PatchAll(typeof(MapTargetArrowsComponent.OnDynamicMapUpdateIcons));

            var virtualJoystickBindings = new BindingHelper.Binding[] {
                new (typeof(MapTargetArrowsComponent), "ArrowScale", Plugin.MapTargetArrows.ArrowScale),
                new (typeof(MapTargetArrowsComponent), "SelectedColor", Plugin.MapTargetArrows.SelectedColor),
                new (typeof(MapTargetArrowsComponent), "ActiveColor", Plugin.MapTargetArrows.ActiveColor),
                new (typeof(MapTargetArrowsComponent), "ShowT", Plugin.MapTargetArrows.ShowT),
            };
            BindingHelper.ApplyBindings(virtualJoystickBindings);

            initialized = true;
            Plugin.Log($"[MTA] Map Target Arrows plugin started !");
        }
    }
}

public class MapTargetArrowsComponent {
    public static float ArrowScale = 0.5f;
    public static Color SelectedColor = Color.white;
    public static Color ActiveColor = Color.green;
    public static bool ShowT = true;

    public static void Update(ref DynamicMap dynamicMap) {
        var combatHUD = UIBindings.Game.GetCombatHUDComponent();
        if (combatHUD != cachedCombatHUD) {
            if (ShowT) {
                text = GameObject.Instantiate(targetTextCache.GetValue(combatHUD), dynamicMap.iconLayer.transform);
                text.color = ActiveColor;
                text.text = "T";
                text.raycastTarget = false;
                text.enabled = false;
            }
            arrows.Clear();
            cachedCombatHUD = combatHUD;
        }

        var aircraft = combatHUD.aircraft;
        if (aircraft == null)
            return;
        var iconLookup = iconLookupCache.GetValue(dynamicMap);
        if (aircraft != cachedAircraft) {
            cachedAircraftIcon = iconLookup[aircraft];
            cachedAircraft = aircraft;
        }
        var aircraftIconTransform = cachedAircraftIcon.transform;

        List<UnitMapIcon> toDelete = new ();
        foreach ((var icon, var arrow) in arrows)
            if (icon == null) {
                toDelete.Add(icon);
                arrow.enabled = false;
            }
        foreach (var icon in toDelete)
            arrows.Remove(icon);

        var mapRectTransform = mapRectTransformCache.GetValue(dynamicMap);
        var mapImageTransform = dynamicMap.mapImage.transform;

        var insidePosition = DynamicMap.mapMaximized ? mapRectTransform.position : aircraftIconTransform.position;
        Rect? rect;
        if (DynamicMap.mapMaximized) {
            float width = mapRectTransform.rect.width * mapRectTransform.lossyScale.x;
            float height = mapRectTransform.rect.height * mapRectTransform.lossyScale.y;
            rect = new Rect (insidePosition.x - 0.5f * width, insidePosition.y - 0.5f * height, width, height);
        }
        else {
          float width = mapRectTransform.lossyScale.x * mapRectTransform.sizeDelta.x, height = mapRectTransform.lossyScale.y * mapRectTransform.sizeDelta.y;
          var offset = 5;
          rect = new Rect (offset, offset, width, height);
        }

        Unit activeTarget = null;
        var targetList = targetListCache.GetValue(combatHUD);
        if (targetList.Count > 0)
            activeTarget = targetList[0];

        if (ShowT)
            if (activeTarget != null)
                text.enabled = true;
            else
                text.enabled = false;
        else if (text != null)
            text.enabled = false;

        foreach (var icon in iconLookup.Values) {
            if (!arrows.TryGetValue(icon, out var arrow)) {
                arrow = GameObject.Instantiate(targetArrowCache.GetValue(combatHUD), dynamicMap.iconLayer.transform);
                arrows[icon] = arrow;
                arrow.color = SelectedColor;
                arrow.raycastTarget = false;
                arrow.enabled = false;
            }

            if ((bool)isSelectedInfo.GetValue(icon) == false) {
                arrow.enabled = false;
                continue;
            }

            var targetMarker = (TargetMarker)targetMarkerInfo.GetValue(icon);

            var outsidePosition = icon.iconImage.transform.position;
            var outsidePositionClipped = GetRectLineIntersection((Rect)rect, insidePosition, outsidePosition);
            if (outsidePositionClipped != outsidePosition) {
                var direction = outsidePositionClipped - insidePosition;
                float z = -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                if (!DynamicMap.mapMaximized)
                    z = (z + aircraftIconTransform.localEulerAngles.z) % 360f;
                arrow.transform.localEulerAngles = new Vector3 (0, 0, z);
                arrow.transform.position = outsidePositionClipped;
                //makes arrow size fixed regardless of zoom
                arrow.transform.localScale = Vector3.one / mapImageTransform.localScale.x * ArrowScale;
                arrow.enabled = true;

                if (icon.unit == activeTarget) {
                    targetMarker.color = ActiveColor;
                    targetMarker.markerImg.color = ActiveColor;
                    arrow.color = ActiveColor;
                    if (ShowT) {
                        text.transform.localScale = Vector3.one / dynamicMap.mapImage.transform.localScale.x * ArrowScale;
                        text.transform.eulerAngles = Vector3.zero;
                        var offset = text.rectTransform.sizeDelta.y * text.rectTransform.lossyScale.y +
                            arrow.rectTransform.sizeDelta.y * arrow.rectTransform.lossyScale.y;
                        text.transform.position = outsidePositionClipped + (insidePosition - outsidePositionClipped).normalized * offset;
                    }
                }
                else {
                    arrow.color = SelectedColor;
                    targetMarker.color = SelectedColor;
                    targetMarker.markerImg.color = SelectedColor;
                }
            }
            else {
                arrow.enabled = false;
                if (icon.unit == activeTarget) {
                    if (ShowT)
                        text.enabled = false;
                    targetMarker.color = ActiveColor;
                    targetMarker.markerImg.color = ActiveColor;
                }
                else {
                    targetMarker.color = SelectedColor;
                    targetMarker.markerImg.color = SelectedColor;
                }
            }
        }
    }

    [HarmonyPatch(typeof(DynamicMap), "UpdateIcons")]
    public class OnDynamicMapUpdateIcons {
        public static void Postfix(ref DynamicMap __instance) {
            Update(ref __instance);
        }
    }

    private static Vector3 GetRectLineIntersection(Rect rect, Vector3 inside, Vector3 outside) {
        Vector3 direction = outside - inside;
        float tClosest = float.MaxValue;
        float xMin = rect.x, xMax = rect.x + rect.width, yMin = rect.y, yMax = rect.y + rect.height;

        void check_x(float t) {
            if (t > 0 && t < tClosest) {
                float y = inside.y + t * direction.y;
                if (y >= yMin && y <= yMax)
                    tClosest = t;
            }
        }

        void check_y(float t) {
            if (t > 0 && t < tClosest) {
                float x = inside.x + t * direction.x;
                if (x >= xMin && x <= xMax)
                    tClosest = t;
            }
        }

        if (Mathf.Abs(direction.x) > Mathf.Epsilon) {
            // Check intersection with left edge (x = xMin)
            float t = (xMin - inside.x) / direction.x;
            check_x(t);

            // Check intersection with right edge (x = xMax)
            t = (xMax - inside.x) / direction.x;
            check_x(t);
        }

        if (Mathf.Abs(direction.y) > Mathf.Epsilon) {
            // Check intersection with bottom edge (y = yMin)
            float t = (yMin - inside.y) / direction.y;
            check_y(t);

            // Check intersection with top edge (y = yMax)
            t = (yMax - inside.y) / direction.y;
            check_y(t);
        }

        var tClosestClamped = Mathf.Min(tClosest, 1.0f);
        var result = tClosestClamped == 1.0f ? outside : inside + tClosestClamped * direction;
        return result;
    }

    private static TraverseCache<DynamicMap, RectTransform> mapRectTransformCache = new ("mapRectTransform");
    private static TraverseCache<DynamicMap, Dictionary<Unit, UnitMapIcon>> iconLookupCache = new ("iconLookup");
    private static TraverseCache<CombatHUD, Image> targetArrowCache = new ("targetArrow");
    private static TraverseCache<CombatHUD, Text> targetTextCache = new ("targetText");
    private static TraverseCache<CombatHUD, List<Unit>> targetListCache = new ("targetList");
    private static FieldInfo isSelectedInfo = AccessTools.Field(typeof(MapIcon), "isSelected");
    private static FieldInfo targetMarkerInfo = AccessTools.Field(typeof(UnitMapIcon), "targetMarker");

    private static Unit cachedAircraft;
    private static CombatHUD cachedCombatHUD;
    private static UnitMapIcon cachedAircraftIcon;
    private static Dictionary<UnitMapIcon, Image> arrows = new ();
    private static Text text;
}
