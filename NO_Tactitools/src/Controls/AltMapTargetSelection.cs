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
        DynamicMap dynamicMap = SceneSingleton<DynamicMap>.i;
        if (dynamicMap == null)
            return;

        var iconLookup = (Dictionary<Unit, UnitMapIcon>)dynamicMapIconLookupInfo.GetValue(dynamicMap);
        UnitMapIcon unselectedMapIcon = null, selectedMapIcon = null;
        float squareSelectionRadiusUnselected = SelectionRadius * SelectionRadius;
        float squareSelectionRadiusSelected = squareSelectionRadiusUnselected;
        Vector3 mousePosition = Input.mousePosition;

        foreach (UnitMapIcon icon in iconLookup.Values) {
            if (icon.gameObject.activeSelf) {
                float squareDistance = FastMath.SquareDistance(mousePosition, icon.transform.position);
                bool selected = !icon.iconImage.raycastTarget;
                if (!selected && squareDistance <= squareSelectionRadiusUnselected && !SceneSingleton<TargetListSelector>.i.CheckExclusions(icon.unit)) {
                    if (paint) {
                        icon.ClickIcon(MapIcon.ClickSource.Controller);
                    }
                    else {
                        squareSelectionRadiusUnselected = squareDistance;
                        unselectedMapIcon = icon;
                    }
                }
                else if (PickActive && selected && !paint && squareDistance <= squareSelectionRadiusSelected) {
                    squareSelectionRadiusSelected = squareDistance;
                    selectedMapIcon = icon;
                }
            }
        }
        if (!paint && unselectedMapIcon != null) {
            unselectedMapIcon.ClickIcon(MapIcon.ClickSource.Controller);
        }
        else if (selectedMapIcon != null) {
            var unit = selectedMapIcon.unit;
            GameBindings.Player.TargetList.DeselectUnit(unit);
            GameBindings.Player.TargetList.AddTarget(unit);
        }
    }

    private static FieldInfo dynamicMapIconLookupInfo = AccessTools.Field(typeof(DynamicMap), "iconLookup");
    private static FieldInfo dynamicMapPlayerInfo = AccessTools.Field(typeof(DynamicMap), "player");

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
        public static bool Prefix() {
            DynamicMap dynamicMap = SceneSingleton<DynamicMap>.i;
            if (dynamicMap == null)
                return false;

            var player = (Rewired.Player)dynamicMapPlayerInfo.GetValue(dynamicMap);
            if (player.GetButtonTimedPressUp("Select", 0f, PlayerSettings.clickDelay)) {
                SelectFromMap(paint: false);
                return false;
            }
            else if (player.GetButtonTimedPressDown("Select", PlayerSettings.pressDelay)) {
                SelectFromMap(paint: true);
                return false;
            }
            else
                return true;
        }
    }
}
