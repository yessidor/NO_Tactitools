using HarmonyLib;
using UnityEngine.UI;
using System.Collections.Generic;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

/* Objective marker and text, as well as airbase marker and text, on HMD will be hidden if "OBJ" button in Map settings is off. */

//Assuming that AirbaseOverlay and ObjectiveOverlayManager are essentially singletones.

[HarmonyPatch(typeof(MainMenu), "Start")]
public class HideObjectivesPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[HO] Hide Objectives plugin starting !");
            Plugin.harmony.PatchAll(typeof(HideObjectivesComponent.OnAirbaseOverlayDisplayMarkers));
            Plugin.harmony.PatchAll(typeof(HideObjectivesComponent.OnObjectiveOverlayManagerUpdateOverlays));
            initialized = true;
            Plugin.Log($"[HO] Hide Objectives plugin started !");
        }
    }
}

class HideObjectivesComponent {
  [HarmonyPatch(typeof(AirbaseOverlay), "DisplayMarkers")]
  public class OnAirbaseOverlayDisplayMarkers {
      public static void Postfix(ref Image ___airbaseMarker, ref Text ___airbaseLabel) {
        if (SceneSingleton<MapOptions>.i.showObjectives == false) {
          ___airbaseMarker.enabled = false;
          ___airbaseLabel.enabled = false;
        }
      }
  }

  [HarmonyPatch(typeof(ObjectiveOverlayManager), "UpdateOverlays")]
  public class OnObjectiveOverlayManagerUpdateOverlays {
      public static void Postfix(ref List<ObjectiveOverlay> ___overlays) {
        if (SceneSingleton<MapOptions>.i.showObjectives == false) {
          foreach (ObjectiveOverlay overlay in  ___overlays)
            overlay.HideOverlay();
        }
      }
  }
}
