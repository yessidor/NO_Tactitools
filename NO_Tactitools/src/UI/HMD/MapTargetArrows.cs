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
                if (text != null)
                    UnityEngine.Object.Destroy(text.gameObject);
                text = GameObject.Instantiate(targetTextCache.GetValue(combatHUD), dynamicMap.iconLayer.transform);
                text.color = ActiveColor;
                text.text = "T";
                text.raycastTarget = false;
                text.enabled = false;
            }
            ClearAllArrows();
            cachedCombatHUD = combatHUD;
        }

        if (combatHUD == null) {
            ClearAllArrows();
            return;
        }
        var aircraft = combatHUD.aircraft;
        if (aircraft == null) {
            ClearAllArrows();
            return;
        }
        var iconLookup = iconLookupCache.GetValue(dynamicMap);
        if (aircraft != cachedAircraft) {
            ClearAllArrows();
            cachedAircraftIcon = iconLookup[aircraft];
            cachedAircraft = aircraft;
        }
        var aircraftIconTransform = cachedAircraftIcon.transform;

        ClearInactiveArrows();

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
            var outsidePositionClipped = MathUtils.GetRectLineIntersection((Rect)rect, insidePosition, outsidePosition);
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
                targetMarker.markerImg.enabled = false;

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
                targetMarker.markerImg.enabled = true;

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

    private static void ClearAllArrows() {
        foreach ((var icon, var arrow) in arrows) {
            if (arrow != null) {
                arrow.enabled = false;
                UnityEngine.Object.Destroy(arrow.gameObject);
            }
        }
        arrows.Clear();
    }

    private static void ClearInactiveArrows() {
        List<UnitMapIcon> toDelete = new ();
        foreach ((var icon, var arrow) in arrows)
            if (icon == null) {
                toDelete.Add(icon);
                arrow.enabled = false;
                UnityEngine.Object.Destroy(arrow.gameObject);
            }
        foreach (var icon in toDelete)
            arrows.Remove(icon);
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
