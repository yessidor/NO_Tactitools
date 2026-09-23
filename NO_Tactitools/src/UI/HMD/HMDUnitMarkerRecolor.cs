using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

[HarmonyPatch(typeof(MainMenu), "Start")]
public class HMDUnitMarkerRecolorPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[HMR] HMD Marker Recolor plugin starting !");
            Plugin.harmony.PatchAll(typeof(HMDUnitMarkerRecolorComponent.OnHUDUnitMarkerUpdateColor));

            var bindings = new BindingHelper.Binding[] {
                new (typeof(HMDUnitMarkerRecolorComponent), "FriendlyColor", Plugin.HMDUnitMarkerRecolor.FriendlyColor),
                new (typeof(HMDUnitMarkerRecolorComponent), "EnemyColor", Plugin.HMDUnitMarkerRecolor.EnemyColor),
                new (typeof(HMDUnitMarkerRecolorComponent), "NeutralColor", Plugin.HMDUnitMarkerRecolor.NeutralColor)
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log($"[HMR] HMD Marker Recolor plugin started !");
        }
    }
}

class HMDUnitMarkerRecolorComponent {
  public static Color FriendlyColor { set { field = value; UpdateMarkers(); } get; } = new Color (0.0f, 0.0f, 1.0f, 1.0f);
  public static Color EnemyColor { set { field = value; UpdateMarkers(); } get; } = new Color (1.0f, 1.0f, 0.0f, 1.0f);
  public static Color NeutralColor { set { field = value; UpdateMarkers(); } get; } = Color.grey;

  private static MethodInfo updateColorInfo = AccessTools.Method(typeof(HUDUnitMarker), "UpdateColor");
  private static TraverseCache<CombatHUD, List<HUDUnitMarker>> markersCache = new ("markers");

  private static void UpdateMarkers() {
      var combatHUD = UIBindings.Game.GetCombatHUDComponent();
      if (combatHUD == null)
          return;
      var markers = markersCache.GetValue(combatHUD);
      foreach (var marker in markers) {
          if (marker != null)
              updateColorInfo.Invoke(marker, null);
      }
  }

  [HarmonyPatch(typeof(HUDUnitMarker), "UpdateColor")]
  public class OnHUDUnitMarkerUpdateColor {
      public static void Postfix(ref HUDUnitMarker __instance, ref Color ___color) {
          if (__instance.selected || HMDDeclutterComponent.IsSettingMarkerColor(__instance) || EMWSComponent.ProcessingMarker(__instance))
              return;

          Color? color = null;
          switch (DynamicMap.GetFactionMode(__instance.unit.NetworkHQ))
          {
              case FactionMode.NoFaction:
                  color = NeutralColor;
                  break;
              case FactionMode.Friendly:
                  color = FriendlyColor;
                  break;
              case FactionMode.Enemy:
                  color = EnemyColor;
                  break;
          }
          ___color = (Color)color;
          __instance.image.color = (Color)color;
      }
  }
}
