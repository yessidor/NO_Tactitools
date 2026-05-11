using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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
  public static Color FriendlyColor = new Color (0.0f, 0.0f, 1.0f, 1.0f);
  public static Color EnemyColor = new Color (1.0f, 1.0f, 0.0f, 1.0f);
  public static Color NeutralColor = Color.grey;

  [HarmonyPatch(typeof(HUDUnitMarker), "UpdateColor")]
  public class OnHUDUnitMarkerUpdateColor {
      public static void Postfix(ref HUDUnitMarker __instance, ref Unit ___unit, ref Color ___color, ref Image ___image) {
          if (__instance.selected)
              return;

          Color? color = null;
          switch (DynamicMap.GetFactionMode(___unit.NetworkHQ))
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
          ___image.color = (Color)color;
      }
  }
}
