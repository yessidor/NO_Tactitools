using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

[HarmonyPatch(typeof(MainMenu), "Start")]
public class AltMapTargetSelectionPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[AMTS] Alternative Map Target Selection plugin starting !");

            Plugin.harmony.PatchAll(typeof(AltMapTargetSelectionComponent.OnDynamicMapSelectFromMap));
            Plugin.harmony.PatchAll(typeof(AltMapTargetSelectionComponent.OnDynamicMapMapControls));
            Plugin.harmony.PatchAll(typeof(AltMapTargetSelectionComponent.OnUnitMapIconClickIcon));

            var bindings = new BindingHelper.Binding[] {
                new (typeof(AltMapTargetSelectionComponent), "SelectionRadius", Plugin.AltMapTargetSelection.SelectionRadius),
                new (typeof(AltMapTargetSelectionComponent), "PickActive", Plugin.AltMapTargetSelection.PickActive),
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log($"[AMTS] Alternative Map Target Selection plugin started !");
        }
    }
}

class AltMapTargetSelectionComponent {
    public static int SelectionRadius { set; get; } = 10;
    public static bool PickActive = true;

    private static void SelectFromMap(bool paint = false) {
        DynamicMap dynamicMap = UIBindings.Game.GetDynamicMapComponent();
        if (dynamicMap == null)
            return;

        var iconLookup = dynamicMapIconLookupCache.GetValue(dynamicMap);
        List<UnitMapIcon> paintedIcons = new ();
        UnitMapIcon unselectedIcon = null, selectedIcon = null;
        float squareSelectionRadiusUnselected = SelectionRadius * SelectionRadius;
        float squareSelectionRadiusSelected = squareSelectionRadiusUnselected;
        Vector3 mousePosition = Input.mousePosition;

        foreach (UnitMapIcon icon in iconLookup.Values) {
            if (icon.gameObject.activeSelf) {
                float squareDistance = FastMath.SquareDistance(mousePosition, icon.transform.position);
                bool selected = !icon.iconImage.raycastTarget;
                if (!selected && squareDistance <= squareSelectionRadiusUnselected && !GameBindings.Player.TargetFilter.CheckExclusions(icon.unit)) {
                    if (paint) {
                        paintedIcons.Add(icon);
                    }
                    else {
                        squareSelectionRadiusUnselected = squareDistance;
                        unselectedIcon = icon;
                    }
                }
                else if (PickActive && selected && !paint && squareDistance <= squareSelectionRadiusSelected) {
                    squareSelectionRadiusSelected = squareDistance;
                    selectedIcon = icon;
                }
            }
        }

        bool disposessed = true;
        var aircraft = GameBindings.Player.Aircraft.GetAircraft();
        if (aircraft != null)
            disposessed = aircraft.disabled;

        if (paint) {
            if (paintedIcons.Count > 0) {
                if (disposessed) {
                    foreach (var icon in paintedIcons)
                        icon.ClickIcon(MapIcon.ClickSource.Controller);
                }
                else {
                    GameBindings.Player.TargetList.AddTargets(paintedIcons.ConvertAll(icon => icon.unit));
                }
            }
        }
        else {
            if (disposessed) {
                var icon = unselectedIcon != null ? unselectedIcon : selectedIcon != null ? selectedIcon : null;
                if (icon != null)
                    icon.ClickIcon(MapIcon.ClickSource.Controller);
            }
            else
                if (unselectedIcon != null) {
                    GameBindings.Player.TargetList.AddTarget(unselectedIcon.unit);
                }
                else if (selectedIcon != null) {
                    var unit = selectedIcon.unit;
                    GameBindings.Player.TargetList.DeselectUnit(unit);
                    GameBindings.Player.TargetList.AddTarget(unit);
                }
        }
    }

    private static TraverseCache<DynamicMap, Dictionary<Unit, UnitMapIcon>> dynamicMapIconLookupCache = new ("iconLookup");
    private static TraverseCache<DynamicMap, Rewired.Player> dynamicMapPlayerCache = new ("player");

    /*An icon is selected by calling ClickIcon() from DynamicMap.SelectFromMap() (which is disabled),
      and from MapIcon.IPointerClickHandler.OnPointerClick(). The latter causes unneeded additional selection.
      Harmony cannot patch it (unable to find method), so patching UnitMapIcon.ClickIcon() instead.
      MapIcon.IPointerClickHandler.OnPointerClick() calls ClickIcon(clickSource: ClickSource.Mouse), and
      DynamicMap.SelectFromMap calls ClickIcon(clickSource: ClickSource.Controller), so using clickSource
      to distinguish these cases. clickSource itself is not used inside ClickIcon. */
    [HarmonyPatch(typeof(UnitMapIcon), "ClickIcon")]
    public class OnUnitMapIconClickIcon {
        public static bool Prefix(MapIcon.ClickSource clickSource) {
            return clickSource == MapIcon.ClickSource.Mouse ? false : true;
        }
    }

    [HarmonyPatch(typeof(DynamicMap), "SelectFromMap")]
    public class OnDynamicMapSelectFromMap {
        public static bool Prefix() {
            return false;
        }
    }

    [HarmonyPatch(typeof(DynamicMap), "MapControls")]
    public class OnDynamicMapMapControls {
        public static bool Prefix(DynamicMap __instance, ref RectTransform ___mapRectTransform) {
            var dynamicMap = __instance;
            var mapRectTransform = ___mapRectTransform;

            if (dynamicMap == null || !DynamicMap.mapMaximized)
                return false;

            var player = dynamicMapPlayerCache.GetValue(dynamicMap);
            if (player.GetButtonTimedPressUp("Select", 0f, PlayerSettings.clickDelay)) {
                SelectFromMap(paint: false);
                return false;
            }
            else if (player.GetButtonTimedPressDown("Select", PlayerSettings.pressDelay)) {
                SelectFromMap(paint: true);
                return false;
            }
            else {
                //FIX Avoids processing any input if mouse cursor is outside maximized map
                var position = mapRectTransform.position;
                float width = mapRectTransform.rect.width * mapRectTransform.lossyScale.x;
                float height = mapRectTransform.rect.height * mapRectTransform.lossyScale.y;
                var rect = new Rect (position.x - 0.5f * width, position.y - 0.5f * height, width, height);
                return MathUtils.IsInsideRect(rect, Input.mousePosition);
            }
        }
    }
}
