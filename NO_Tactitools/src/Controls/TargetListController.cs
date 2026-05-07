using HarmonyLib;
using System.Collections.Generic;
using NO_Tactitools.Core;
using NO_Tactitools.UI.MFD;
using UnityEngine.UI;
using UnityEngine;
using NuclearOption.SceneLoading;
using Unity.Properties;
using System.Linq;

namespace NO_Tactitools.Controls;

[HarmonyPatch(typeof(MainMenu), "Start")]
class TargetListControllerPlugin {
    public static bool switchCurrentTarget = true;
    private static bool initialized = false;

    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[TC] Target List Controller plugin starting !");
            Plugin.harmony.PatchAll(typeof(TargetListControllerComponent.OnPlatformStart));
            Plugin.harmony.PatchAll(typeof(TargetListControllerComponent.OnPlatformUpdate));

            var unitRecallListsNum = Plugin.MFDNavExtraKeys.Count + 1;
            TargetListControllerComponent.InternalState.unitRecallLists = new List<Unit> [unitRecallListsNum];

            InputCatcher.RegisterNewInput(
                Plugin.MFDNavUp,
                0.2f,
                onRelease: () => RecallTargets(0),
                onLongPress: () => RememberTargets(0));
            for (int i = 0; i < Plugin.MFDNavExtraKeys.Count; i++) {
                int j = i + 1;
                InputCatcher.RegisterNewInput(
                    Plugin.MFDNavExtraKeys[i],
                    0.2f,
                    onRelease: () => RecallTargets(j),
                    onLongPress: () => RememberTargets(j));
            }
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavRight,
                0.5f,
                onRelease: NextTarget,
                onLongPress: SortTargetsByDistance);
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavLeft,
                0.5f,
                onRelease: PreviousTarget,
                onLongPress: SortTargetsByName);
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavEnter,
                0.2f,
                onRelease: PopCurrentTarget,
                onLongPress: KeepOnlyCurrentTarget);
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavDown,
                0.2f,
                onRelease: KeepOnlyDataLinkedTargets,
                onLongPress: KeepClosestTargetsBasedOnAmmo);
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavBack,
                0.2f,
                onRelease: KeepUntrackedTargets,
                onLongPress: KeepTrackedTargets);
            initialized = true;
            Plugin.Log("[TC] Target List Controller plugin succesfully started !");
        }
    }

    private static void NextTarget() {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log($"[TC] NextTarget");
        int targetCount = GameBindings.Player.TargetList.GetTargets().Count;
        if (targetCount > 1) {
            if (switchCurrentTarget) {
                List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets();
                currentTargets.Add(currentTargets[0]);
                currentTargets.RemoveAt(0);
                GameBindings.Player.TargetList.DeselectAll();
                GameBindings.Player.TargetList.AddTargets(currentTargets, muteSound: true);
                TargetListControllerComponent.InternalState.targetIndex = 0;
            }
            else {
                TargetListControllerComponent.InternalState.targetIndex = (TargetListControllerComponent.InternalState.targetIndex - 1 + targetCount) % targetCount;
            }
            TargetListControllerComponent.InternalState.updateDisplay = true;
            UIBindings.Sound.PlaySound("beep_scroll");
        }
    }

    private static void PreviousTarget() {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log($"[TC] PreviousTarget");
        int targetCount = GameBindings.Player.TargetList.GetTargets().Count;
        if (targetCount > 1) {
            if (switchCurrentTarget) {
                List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets();
                int lastIndex = targetCount - 1;
                currentTargets.Insert(0, currentTargets[lastIndex]);
                //After inserting new element to the beginning of list the size of list has increased by 1 and index of last element == targetCount
                currentTargets.RemoveAt(targetCount);
                GameBindings.Player.TargetList.DeselectAll();
                GameBindings.Player.TargetList.AddTargets(currentTargets, muteSound: true);
                TargetListControllerComponent.InternalState.targetIndex = 0;
            }
            else {
                TargetListControllerComponent.InternalState.targetIndex = (TargetListControllerComponent.InternalState.targetIndex + 1) % targetCount;
            }
            TargetListControllerComponent.InternalState.updateDisplay = true;
            UIBindings.Sound.PlaySound("beep_scroll");
        }
    }

    private static void PopCurrentTarget() {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log($"[TC] DeselectCurrentTarget");
        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets();
        if (currentTargets.Count > 0 && TargetListControllerComponent.InternalState.targetIndex < currentTargets.Count) {
            Unit targetToDeselect = currentTargets[TargetListControllerComponent.InternalState.targetIndex];
            TargetListControllerComponent.InternalState.targetIndex = Mathf.Clamp(TargetListControllerComponent.InternalState.targetIndex, 0, Mathf.Max(0, currentTargets.Count - 1));
            GameBindings.Player.TargetList.DeselectUnit(targetToDeselect);
            TargetListControllerComponent.InternalState.updateDisplay = true;
        }
    }

    private static void KeepOnlyCurrentTarget() {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log($"[TC] KeepOnlyCurrentTarget");
        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets();
        if (currentTargets.Count > 0 && TargetListControllerComponent.InternalState.targetIndex < currentTargets.Count) {
            Unit targetToKeep = currentTargets[TargetListControllerComponent.InternalState.targetIndex];
            GameBindings.Player.TargetList.DeselectAll();
            GameBindings.Player.TargetList.AddTargets([targetToKeep]);
            TargetListControllerComponent.InternalState.updateDisplay = true;
        }
    }

    private static void KeepOnlyDataLinkedTargets() {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log($"[TC] KeepOnlyDataLinkedTargets");
        if (GameBindings.Player.TargetList.GetTargets().Count == 0) {
            return;
        }
        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets();
        List<Unit> dataLinkedTargets = [];
        foreach (Unit target in currentTargets) {
            if (TargetListControllerComponent.InternalState.playerFactionHQ.IsTargetPositionAccurate(target, 20f)) {
                dataLinkedTargets.Add(target);
            }
        }
        if (dataLinkedTargets.Count >= 0) {
            GameBindings.Player.TargetList.DeselectAll();
            GameBindings.Player.TargetList.AddTargets(dataLinkedTargets);
            if (switchCurrentTarget) {
                TargetListControllerComponent.InternalState.targetIndex = 0;
            }
            else {
                TargetListControllerComponent.InternalState.resetIndex = true;
            }
            TargetListControllerComponent.InternalState.updateDisplay = true;
            UIBindings.Game.DisplayToast($"Kept <b>{dataLinkedTargets.Count.ToString()}</b> data linked target" + (dataLinkedTargets.Count > 1 ? "s" : ""), 3f);
        }
    }

    private static void SortTargetsByDistanceWorker(List<Unit> targets) {
        var playerAircraft = GameBindings.Player.Aircraft.GetAircraft();
        var playerPosition = playerAircraft.transform.position.ToGlobalPosition().AsVector3();
        float calcDistanceTo(Unit unit) {
          Vector3? unitPosition = playerAircraft.NetworkHQ.GetKnownPosition(unit)?.AsVector3();
          if (unitPosition is null)
            return float.PositiveInfinity;
          float distance = Vector3.Distance(playerPosition, (Vector3)unitPosition);
          return distance;
        }
        targets.Sort((a, b) => {
            float distanceA = calcDistanceTo(a);
            float distanceB = calcDistanceTo(b);
            return distanceA.CompareTo(distanceB);
        });
    }

    private static void KeepClosestTargetsBasedOnAmmo() {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log($"[TC] KeepClosestTargetsBasedOnAmmo");
        if (GameBindings.Player.TargetList.GetTargets().Count == 0) {
            return;
        }
        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets();
        List<Unit> sortedTargets = [.. currentTargets];
        SortTargetsByDistanceWorker(sortedTargets);
        int activeStationAmmo = GameBindings.Player.Aircraft.Weapons.GetActiveStationAmmo();
        List<Unit> closestTargets = sortedTargets.GetRange(0, Mathf.Min(activeStationAmmo, sortedTargets.Count));
        //Probably needed to keep the relative order of targets in closestTargets as it is in currentTargets ?
        List<Unit> targetsToKeep = [.. currentTargets.Where(closestTargets.Contains)];

        if (targetsToKeep.Count >= 0) {
            if (switchCurrentTarget) {
                TargetListControllerComponent.InternalState.targetIndex = 0;
            }
            else {
                TargetListControllerComponent.InternalState.resetIndex = true;
            }
            GameBindings.Player.TargetList.DeselectAll();
            GameBindings.Player.TargetList.AddTargets(targetsToKeep);
            TargetListControllerComponent.InternalState.updateDisplay = true;
            UIBindings.Game.DisplayToast($"Kept <b>{targetsToKeep.Count.ToString()}</b> closest targets based on ammo", 3f);
        }
    }

    private static void RememberTargets(int i = 0) {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log(string.Format("[TC] RememberTargets({0})", i));
        if (UIBindings.Game.GetCombatHUDTransform() != null) {
            var targets = GameBindings.Player.TargetList.GetTargets();
            if (targets.Count == 0) {
                return;
            }
            TargetListControllerComponent.InternalState.unitRecallLists[i] = targets;
            string report = string.Format("Saved <b>{0}</b> {1} to list <b>{2}</b>", targets.Count, targets.Count == 1 ? "target" : "targets", i);
            UIBindings.Game.DisplayToast(report, 3f);
            UIBindings.Sound.PlaySound("beep_remember");
        }
    }

    private static void RecallTargets(int i = 0) {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log(string.Format("[TC] RecallTargets({0})", i));
        List<Unit> unitRecallList = TargetListControllerComponent.InternalState.unitRecallLists[i];
        if (unitRecallList != null) {
            if (unitRecallList.Count > 0) {
                GameBindings.Player.TargetList.DeselectAll();
                GameBindings.Player.TargetList.AddTargets(unitRecallList);
                var targets = GameBindings.Player.TargetList.GetTargets();
                string report = string.Format("Recalled <b>{0}</b> {1} from list <b>{2}</b>", targets.Count, targets.Count == 1 ? "target" : "targets", i);
                if (switchCurrentTarget) {
                    TargetListControllerComponent.InternalState.targetIndex = 0;
                }
                else {
                    TargetListControllerComponent.InternalState.resetIndex = true;
                }
                TargetListControllerComponent.InternalState.updateDisplay = true;
                UIBindings.Game.DisplayToast(report, 3f);
            }
        }
    }

    private static void SortTargetsByDistance() {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log($"[TC] SortTargetsByDistance");
        if (GameBindings.Player.TargetList.GetTargets().Count < 2) {
            return;
        }
        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets();
        List<Unit> sortedTargets = [.. currentTargets];
        SortTargetsByDistanceWorker(sortedTargets);
        if (switchCurrentTarget) {
            TargetListControllerComponent.InternalState.targetIndex = 0;
        }
        else {
            sortedTargets.Reverse(); //for behaviour consistency
            TargetListControllerComponent.InternalState.resetIndex = true;
        }
        GameBindings.Player.TargetList.DeselectAll();
        GameBindings.Player.TargetList.AddTargets(sortedTargets, muteSound: true);
        TargetListControllerComponent.InternalState.updateDisplay = true;
        string report = $"Sorted <b>{sortedTargets.Count.ToString()}</b> targets by <b>distance</b>";
        UIBindings.Game.DisplayToast(report, 3f);
        UIBindings.Sound.PlaySound("beep_sort");
    }

    private static void SortTargetsByName() {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log($"[TC] SortTargetsByName");
        if (GameBindings.Player.TargetList.GetTargets().Count < 2) {
            return;
        }
        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets();
        List<Unit> sortedTargets = [.. currentTargets];
        sortedTargets.Sort((a, b) => {
            return a.unitName.CompareTo(b.unitName);
        });
        if (switchCurrentTarget) {
            TargetListControllerComponent.InternalState.targetIndex = 0;
        }
        else {
            sortedTargets.Reverse(); //for behaviour consistency
            TargetListControllerComponent.InternalState.resetIndex = true;
        }
        GameBindings.Player.TargetList.DeselectAll();
        GameBindings.Player.TargetList.AddTargets(sortedTargets, muteSound: true);
        string report = $"Sorted <b>{sortedTargets.Count.ToString()}</b> targets by <b>name</b>";
        TargetListControllerComponent.InternalState.updateDisplay = true;
        UIBindings.Game.DisplayToast(report, 3f);
        UIBindings.Sound.PlaySound("beep_sort");
    }

    private static void KeepTrackedTargets() {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log($"[TC] KeepTrackedTargets");
        var trackedTargets = AmmoConIndicatorComponent.GetTrackedTargets();
        GameBindings.Player.TargetList.DeselectAll();
        GameBindings.Player.TargetList.AddTargets(trackedTargets, muteSound: true);
        string report = $"Kept <b>{trackedTargets.Count.ToString()}</b> tracked targets";
        TargetListControllerComponent.InternalState.updateDisplay = true;
        UIBindings.Game.DisplayToast(report, 3f);
        UIBindings.Sound.PlaySound("beep_sort");
    }

    private static void KeepUntrackedTargets() {
        if (NOAutopilotComponent.InternalState.showMenu) return;
        Plugin.Log($"[TC] KeepTrackedTargets");
        var currentTargets = GameBindings.Player.TargetList.GetTargets();
        var trackedTargets = new HashSet<Unit> (AmmoConIndicatorComponent.GetTrackedTargets());
        currentTargets = currentTargets.FindAll(unit => !trackedTargets.Contains(unit));
        GameBindings.Player.TargetList.DeselectAll();
        GameBindings.Player.TargetList.AddTargets(currentTargets, muteSound: true);
        string report = $"Kept <b>{currentTargets.Count.ToString()}</b> untracked targets";
        TargetListControllerComponent.InternalState.updateDisplay = true;
        UIBindings.Game.DisplayToast(report, 3f);
        UIBindings.Sound.PlaySound("beep_sort");
    }
}

public static class TargetListControllerComponent {
    static class LogicEngine {
        public static void Init() {
            InternalState.previousTargetList = [];
            InternalState.targetIndex = 0;
            InternalState.playerFactionHQ = GameBindings.GameState.GetCurrentFactionHQ();
        }

        public static void Update() {
            var currentTargetList = GameBindings.Player.TargetList.GetTargets();
            int currentCount = currentTargetList.Count;
            int previousCount = InternalState.previousTargetList.Count;
            if (previousCount != currentCount || InternalState.resetIndex) {
                if (TargetListControllerPlugin.switchCurrentTarget) {
                    InternalState.targetIndex = 0;
                }
                else {
                  if (previousCount != currentCount) {
                      if (currentCount <= 1)
                          InternalState.targetIndex = 0;
                      else
                          InternalState.targetIndex += (currentCount > previousCount) ? 1 : -1;
                  }
                  if (InternalState.targetIndex < 0)
                    InternalState.targetIndex = 0;
                  else if (InternalState.targetIndex > currentCount - 1)
                    InternalState.targetIndex = currentCount - 1;
                  if (InternalState.resetIndex) { // don't forget that the list is in reverse order (LIFO), this is why we set to count - 1
                      InternalState.targetIndex = currentCount - 1;
                  }
                }
                InternalState.previousTargetList = currentTargetList;
                InternalState.resetIndex = false;
                InternalState.updateDisplay = true;
            }
        }
    }

    public static class InternalState {
        public static List<Unit>[] unitRecallLists;
        public static FactionHQ playerFactionHQ;
        public static List<Unit> previousTargetList;
        public static Color mainColor = Color.green;
        public static bool updateDisplay = false;
        public static bool resetIndex = false;
        public static int targetIndex = 0;
        public static readonly TraverseCache<TargetScreenUI, FactionHQ> _hqCache = new("hq");
        public static readonly TraverseCache<TargetScreenUI, Text> _typeTextCache = new("typeText");
        public static readonly TraverseCache<TargetScreenUI, Text> _headingCache = new("heading");
        public static readonly TraverseCache<TargetScreenUI, Text> _altitudeCache = new("altitude");
        public static readonly TraverseCache<TargetScreenUI, Text> _relAltitudeCache = new("rel_altitude");
        public static readonly TraverseCache<TargetScreenUI, Text> _speedCache = new("speed");
        public static readonly TraverseCache<TargetScreenUI, Text> _relSpeedCache = new("rel_speed");
        public static readonly TraverseCache<TargetScreenUI, Text> _pilotTextCache = new("pilotText");
        public static readonly TraverseCache<TargetScreenUI, Text> _distanceCache = new("distance");
        public static readonly TraverseCache<TargetScreenUI, List<Image>> _targetBoxesCache = new("targetBoxes");
    }
    static class DisplayEngine {

        public static void Init() {
        }
        public static void Update() {
            static void UpdateTargetTexts() {
                TargetScreenUI targetScreen = UIBindings.Game.GetTargetScreenUIComponent();
                if (targetScreen == null) return;

                List<Unit> targets = GameBindings.Player.TargetList.GetTargets();
                int index = InternalState.targetIndex;
                if (index >= targets.Count) return;

                Unit unit = targets[index];
                FactionHQ hq = InternalState._hqCache.GetValue(targetScreen);

                Text typeText = InternalState._typeTextCache.GetValue(targetScreen);
                Text heading = InternalState._headingCache.GetValue(targetScreen);
                Text altitude = InternalState._altitudeCache.GetValue(targetScreen);
                Text rel_altitude = InternalState._relAltitudeCache.GetValue(targetScreen);
                Text speed = InternalState._speedCache.GetValue(targetScreen);
                Text rel_speed = InternalState._relSpeedCache.GetValue(targetScreen);
                Text pilotText = InternalState._pilotTextCache.GetValue(targetScreen);

                Text distance = InternalState._distanceCache.GetValue(targetScreen);
                /* Text bearingText = traverse.Field("bearingText").GetValue<Text>();
                Image bearingImg = traverse.Field("bearingImg").GetValue<Image>(); */

                bool isAirOrMissile = unit is Aircraft || unit is Missile;

                if (unit.NetworkHQ == null) {
                    typeText.color = Color.white;
                }
                else {
                    typeText.color = (unit.NetworkHQ == hq) ? GameAssets.i.HUDFriendly : GameAssets.i.HUDHostile;
                }

                if (isAirOrMissile) {
                    Aircraft aircraft = unit as Aircraft;
                    if (aircraft != null && aircraft.pilots[0].player != null) {
                        pilotText.gameObject.SetActive(true);
                        pilotText.text = "Pilot : " + aircraft.pilots[0].player.PlayerName;
                        pilotText.color = typeText.color;
                    }
                    else {
                        pilotText.gameObject.SetActive(false);
                    }
                }
                else {
                    pilotText.gameObject.SetActive(false);
                }

                if (hq.IsTargetPositionAccurate(unit, 20f) && isAirOrMissile) {
                    GlobalPosition globalPos = unit.GlobalPosition();
                    Vector3 relPos = globalPos - GameBindings.Player.Aircraft.GetAircraft().GlobalPosition();

                    heading.text = string.Format("HDG {0:F0}°", unit.transform.eulerAngles.y);
                    altitude.text = "ALT " + UnitConverter.AltitudeReading(globalPos.y);
                    rel_altitude.text = "REL " + UnitConverter.AltitudeReading(relPos.y);
                    speed.text = "SPD " + UnitConverter.SpeedReading(unit.speed);
                    rel_speed.text = "REL " + UnitConverter.SpeedReading(Vector3.Dot(GameBindings.Player.Aircraft.GetAircraft().rb.velocity, relPos.normalized) - Vector3.Dot(unit.rb.velocity, relPos.normalized));
                }
                else {
                    heading.text = "HDG -";
                    altitude.text = "ALT -";
                    rel_altitude.text = "REL -";
                    speed.text = "SPD -";
                    rel_speed.text = "REL -";
                }
                distance.text = "RNG " + UnitConverter.DistanceReading(Vector3.Distance(GameBindings.Player.Aircraft.GetAircraft().transform.position, unit.transform.position));
                var countText = TargetListControllerPlugin.switchCurrentTarget ?
                    string.Format("[{0}] ", targets.Count) :
                    string.Format("[{0}/{1}] ", targets.Count - index, targets.Count); 
                var unitName = (unit is Aircraft) ? unit.definition.unitName : unit.unitName;
                typeText.text = countText + unitName;
            }
            // PROPER UPDATE START
            if (UIBindings.Game.GetCombatHUDTransform() == null ||
                UIBindings.Game.GetTargetScreenTransform(silent: true) == null) {
                return;
            }

            List<Unit> targets = GameBindings.Player.TargetList.GetTargets();
            if (targets.Count == 0) return;

            if (InternalState.updateDisplay) {
                TargetScreenUI targetScreen = UIBindings.Game.GetTargetScreenUIComponent();
                List<Image> targetIcons = InternalState._targetBoxesCache.GetValue(targetScreen);
                // Wait until the UI has instantiated the boxes for the new targets
                if (targetIcons.Count < targets.Count) {
                    return;
                }
                for (int i = 0; i < targetIcons.Count; i++) {

                    Rect rect = targetIcons[i].rectTransform.rect;
                    Vector2 size = rect.size + new Vector2(4f, 4f);
                    Vector2 halfSize = size / 2f;

                    UIBindings.Draw.UIAdvancedRectangle selectionRect = new(
                        "SelectionOutline",
                        -halfSize,
                        halfSize,
                        InternalState.mainColor,
                        4f,
                        targetIcons[i].transform,
                        Color.clear
                    );
                    selectionRect.GetImageComponent().raycastTarget = false;

                    if (i == InternalState.targetIndex) {
                        selectionRect.GetGameObject().SetActive(true);
                    }
                    else {
                        selectionRect.GetGameObject().SetActive(false);
                    }
                }
                InternalState.updateDisplay = false;
            }

            if (targets.Count > 1) {
                UpdateTargetTexts();
            }
        }

    }

    [HarmonyPatch(typeof(TacScreen), "Initialize")]
    public static class OnPlatformStart {
        static void Postfix() {
            LogicEngine.Init();
            DisplayEngine.Init();
        }
    }

    [HarmonyPatch(typeof(TacScreen), "Update")]
    public static class OnPlatformUpdate {
        static void Postfix() {
            LogicEngine.Update();
            DisplayEngine.Update();
        }
    }
}


