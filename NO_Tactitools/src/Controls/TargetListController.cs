using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using NO_Tactitools.Core;
using NO_Tactitools.UI.MFD;
using NO_Tactitools.UI.HMD;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NuclearOption.SceneLoading;
using NuclearOption.Networking;
using NuclearOption.UIStyleSystem;
using Unity.Properties;

namespace NO_Tactitools.Controls;

[HarmonyPatch(typeof(MainMenu), "Start")]
class TargetListControllerPlugin {
    public static bool switchCurrentTarget = true;
    public static bool initialized = false;

    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[TC] Target List Controller plugin starting !");
            Plugin.harmony.PatchAll(typeof(TargetListControllerComponent.OnPlatformStart));
            Plugin.harmony.PatchAll(typeof(TargetListControllerComponent.OnPlatformUpdate));
            Plugin.harmony.PatchAll(typeof(TargetListControllerComponent.OnTargetMarkerExtraSetup));
            Plugin.harmony.PatchAll(typeof(TargetListControllerComponent.OnTargetMarkerDynamicHide));
            Plugin.harmony.PatchAll(typeof(TargetListControllerComponent.OnTargetMarkerMask));
            Plugin.harmony.PatchAll(typeof(TargetListControllerComponent.OnTargetMarkerShow));
            Plugin.harmony.PatchAll(typeof(TargetListControllerComponent.OnDynamicMapMinimize));

            if (Plugin.tlcExtraEntryCheckboxFunctions.Value)
                Plugin.harmony.PatchAll(typeof(TargetListControllerComponent.OnToggleOnPointerClick));

            if (Plugin.tlcHighlightActiveEntry.Value)
                Plugin.harmony.PatchAll(typeof(TargetListControllerComponent.OnTargetListSelectorUpdate));

            var unitRecallListsNum = Plugin.MFDNavExtraKeys.Count + 1;
            TargetListControllerComponent.InternalState.unitRecallLists = new List<Unit> [unitRecallListsNum];

            TargetListControllerComponent.InternalState.mtsSavedTargets = new List<Unit> ();

            InputCatcher.RegisterNewInput(
                Plugin.MFDNavUp,
                Plugin.PressDelay.Value,
                onRelease: () => RecallTargets(0),
                onLongPress: () => RememberTargets(0));
            for (int i = 0; i < Plugin.MFDNavExtraKeys.Count; i++) {
                int j = i + 1;
                InputCatcher.RegisterNewInput(
                    Plugin.MFDNavExtraKeys[i],
                    Plugin.PressDelay.Value,
                    onRelease: () => RecallTargets(j),
                    onLongPress: () => RememberTargets(j));
            }
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavRight,
                Plugin.PressDelay.Value,
                onRelease: NextTarget,
                onLongPress: SortTargetsByDistance);
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavLeft,
                Plugin.PressDelay.Value,
                onRelease: PreviousTarget,
                onLongPress: SortTargetsByName);
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavEnter,
                Plugin.PressDelay.Value,
                onRelease: PopCurrentTarget,
                onLongPress: KeepOnlyCurrentTarget);
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavDown,
                Plugin.PressDelay.Value,
                onRelease: KeepOnlyDataLinkedTargets,
                onLongPress: KeepClosestTargetsBasedOnAmmo);
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavBack,
                Plugin.PressDelay.Value,
                onRelease: KeepUntrackedTargets,
                onLongPress: KeepTrackedTargets);
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavSelectByUnitName,
                Plugin.PressDelay.Value,
                onRelease: () => SelectTargetsByUnitName(asCurrentTarget: false),
                onLongPress: () => SelectTargetsByUnitName(asCurrentTarget: true));
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavSelectLased,
                Plugin.PressDelay.Value,
                onRelease: () => SelectTargetsByLasedStatus(lased: false),
                onLongPress: () => SelectTargetsByLasedStatus(lased: true));
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavSelectClosest,
                Plugin.PressDelay.Value,
                onRelease: () => SelectClosestTargets(oneTarget: true),
                onLongPress: () => SelectClosestTargets(oneTarget: false));
            InputCatcher.RegisterNewInput(
                Plugin.MFDNavMissileTargetingSystem,
                Plugin.PressDelay.Value,
                onRelease: MtsToggle,
                onLongPress: () => MtsSetAutoSelectMode(null));

            var bindings = new BindingHelper.Binding[] {
                new (typeof(TargetListControllerComponent.InternalState), "mtsSelectOnlyInterceptableMissiles", Plugin.tlcMTSSelectOnlyInterceptableMissiles),
                new (typeof(TargetListControllerComponent.InternalState), "mtsAngleThreshold", Plugin.tlcMTSAngleThreshold),
                new (typeof(TargetListControllerComponent.InternalState), "respectTargetingDistances", Plugin.tlcRespectTargetingDistances),
                new (typeof(TargetListControllerComponent.InternalState), "selectUnitsToReload", Plugin.tlcSelectUnitsToReload),
                new (typeof(TargetListControllerComponent.InternalState), "selectClosestTargetsByAmmo", Plugin.tlcSelectClosestTargetsByAmmo),
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;
            Plugin.Log("[TC] Target List Controller plugin succesfully started !");
        }
    }

    //TODO Move callbacks into TargetListControllerComponent
    public static void NextTarget() {
        Plugin.Log($"[TC] NextTarget");

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        int targetCount = GameBindings.Player.TargetList.GetTargetCount();
        if (targetCount > 1) {
            if (switchCurrentTarget) {
                List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets(copy: false);
                currentTargets.Add(currentTargets[0]);
                currentTargets.RemoveAt(0);
                TargetListControllerComponent.InternalState.targetIndex = 0;
            }
            else {
                TargetListControllerComponent.InternalState.targetIndex = (TargetListControllerComponent.InternalState.targetIndex - 1 + targetCount) % targetCount;
            }
            TargetListControllerComponent.InternalState.updateDisplay = true;
            UIBindings.Sound.PlaySound("beep_scroll");
        }
    }

    public static void PreviousTarget() {
        Plugin.Log($"[TC] PreviousTarget");

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        int targetCount = GameBindings.Player.TargetList.GetTargetCount();
        if (targetCount > 1) {
            if (switchCurrentTarget) {
                List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets(copy: false);
                int lastIndex = targetCount - 1;
                currentTargets.Insert(0, currentTargets[lastIndex]);
                //After inserting new element to the beginning of list the size of list has increased by 1 and index of last element == targetCount
                currentTargets.RemoveAt(targetCount);
                TargetListControllerComponent.InternalState.targetIndex = 0;
            }
            else {
                TargetListControllerComponent.InternalState.targetIndex = (TargetListControllerComponent.InternalState.targetIndex + 1) % targetCount;
            }
            TargetListControllerComponent.InternalState.updateDisplay = true;
            UIBindings.Sound.PlaySound("beep_scroll");
        }
    }

    public static void PopCurrentTarget() {
        Plugin.Log($"[TC] DeselectCurrentTarget");

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets(copy: false);
        if (currentTargets.Count > 0 && TargetListControllerComponent.InternalState.targetIndex < currentTargets.Count) {
            Unit targetToDeselect = currentTargets[TargetListControllerComponent.InternalState.targetIndex];
            TargetListControllerComponent.InternalState.targetIndex = Mathf.Clamp(TargetListControllerComponent.InternalState.targetIndex, 0, Mathf.Max(0, currentTargets.Count - 1));
            GameBindings.Player.TargetList.DeselectUnit(targetToDeselect);
            TargetListControllerComponent.InternalState.updateDisplay = true;
        }
    }

    public static void KeepOnlyCurrentTarget() {
        Plugin.Log($"[TC] KeepOnlyCurrentTarget");

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets(copy: false);
        if (currentTargets.Count > 0 && TargetListControllerComponent.InternalState.targetIndex < currentTargets.Count) {
            Unit targetToKeep = currentTargets[TargetListControllerComponent.InternalState.targetIndex];
            GameBindings.Player.TargetList.DeselectAll();
            GameBindings.Player.TargetList.AddTargets([targetToKeep]);
            TargetListControllerComponent.InternalState.updateDisplay = true;
        }
    }

    public static void KeepOnlyDataLinkedTargets() {
        Plugin.Log($"[TC] KeepOnlyDataLinkedTargets");

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        if (GameBindings.Player.TargetList.GetTargetCount() == 0) {
            return;
        }
        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets(copy: false);
        List<Unit> dataLinkedTargets = currentTargets.Where(unit => TargetListControllerComponent.InternalState.playerFactionHQ.IsTargetPositionAccurate(unit, 20f)).ToList();
        if (dataLinkedTargets.Count >= 0) {
            replaceTargets(dataLinkedTargets);
            string report = string.Format("Kept <b>{0}</b> data linked {1}", dataLinkedTargets.Count, singularOrPlural(dataLinkedTargets.Count, "target", "targets"));
            UIBindings.Game.DisplayToast(report, 3f);
        }
    }

    public static void KeepClosestTargetsBasedOnAmmo() {
        Plugin.Log($"[TC] KeepClosestTargetsBasedOnAmmo");

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        bool selected = false;
        if (TargetListControllerComponent.InternalState.selectClosestTargetsByAmmo) {
            SelectClosestTargets(oneTarget: false, silent: true);
            selected = true;
        }
        else if (GameBindings.Player.TargetList.GetTargetCount() == 0)
            return;

        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets(copy: false);
        List<Unit> sortedTargets = [.. currentTargets];
        sortTargetsByDistance(sortedTargets);
        int activeStationAmmo = GameBindings.Player.Aircraft.Weapons.GetActiveStationAmmo();
        List<Unit> closestTargets = sortedTargets.GetRange(0, Mathf.Min(activeStationAmmo, sortedTargets.Count));
        //Probably needed to keep the relative order of targets in closestTargets as it is in currentTargets ?
        List<Unit> targetsToKeep = [.. currentTargets.Where(closestTargets.Contains)];

        if (targetsToKeep.Count >= 0) {
            replaceTargets(targetsToKeep);
            string report = $"{(selected ? "Selected" : "Kept")} <b>{targetsToKeep.Count}</b> closest {(singularOrPlural(targetsToKeep.Count, "target", "targets"))} based on ammo";
            UIBindings.Game.DisplayToast(report, 3f);
        }
    }

    public static void RememberTargets(int i = 0) {
        Plugin.Log(string.Format("[TC] RememberTargets({0})", i));

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        if (UIBindings.Game.GetCombatHUDTransform() != null) {
            if (GameBindings.Player.TargetList.GetTargetCount() == 0) {
                return;
            }
            var targets = GameBindings.Player.TargetList.GetTargets();
            TargetListControllerComponent.InternalState.unitRecallLists[i] = targets;
            string report = string.Format("Saved <b>{0}</b> {1} to list <b>{2}</b>", targets.Count, singularOrPlural(targets.Count, "target", "targets"), i);
            UIBindings.Game.DisplayToast(report, 3f);
            UIBindings.Sound.PlaySound("beep_remember");
        }
    }

    public static void RecallTargets(int i = 0) {
        Plugin.Log(string.Format("[TC] RecallTargets({0})", i));

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        List<Unit> unitRecallList = TargetListControllerComponent.InternalState.unitRecallLists[i];
        if (unitRecallList != null) {
            if (unitRecallList.Count > 0) {
                int targetCount = replaceTargets(unitRecallList, muteSound : false);
                string report = string.Format("Recalled <b>{0}</b> {1} from list <b>{2}</b>", targetCount, singularOrPlural(targetCount, "target", "targets"), i);
                UIBindings.Game.DisplayToast(report, 3f);
            }
        }
    }

    public static void PreviewTargets(int i = 0) {
        Plugin.Log(string.Format("[TC] PreviewTargets({0})", i));

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        string report = null;
        List<Unit> unitRecallList = TargetListControllerComponent.InternalState.unitRecallLists[i];
        if (unitRecallList != null && unitRecallList.Count > 0) {
            Dictionary<string, int> targetCounts = new ();
            foreach (var unit in unitRecallList) {
                if (unit == null)
                    continue;
                int count = 1;
                var code = unit.definition.code;
                if (targetCounts.TryGetValue(code, out var prevCount))
                    count += prevCount;
                targetCounts[code] = count;
            }
            var temp = targetCounts.ToList();
            temp.Sort((a, b) => a.Key.CompareTo(b.Key));
            IEnumerable<string> makeString() {
                foreach ((var key, var val) in temp) {
                    yield return $"{key}: {val}";
                }
            };
            report = $"Target list {i}: <b>{(String.Join("; ", makeString()))}</b>";
        }
        else
            report = $"Target list {i} not found";

        UIBindings.Game.DisplayToast(report, 3f);
    }

    public static void SortTargetsByDistance() {
        Plugin.Log($"[TC] SortTargetsByDistance");

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        if (GameBindings.Player.TargetList.GetTargetCount() < 2) {
            return;
        }
        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets(copy: false);
        List<Unit> sortedTargets = [.. currentTargets];
        sortTargetsByDistance(sortedTargets);
        int targetCount = replaceTargets(sortedTargets, muteSound: true);
        string report = string.Format("Sorted <b>{0}</b> {1} by <b>distance</b>", targetCount, singularOrPlural(targetCount, "target", "targets"));
        UIBindings.Game.DisplayToast(report, 3f);
        UIBindings.Sound.PlaySound("beep_sort");
    }

    public static void SortTargetsByName() {
        Plugin.Log($"[TC] SortTargetsByName");

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        if (GameBindings.Player.TargetList.GetTargetCount() < 2) {
            return;
        }
        List<Unit> currentTargets = GameBindings.Player.TargetList.GetTargets();
        List<Unit> sortedTargets = [.. currentTargets];
        sortedTargets.Sort((a, b) => {
            return a.unitName.CompareTo(b.unitName);
        });
        int targetCount = replaceTargets(sortedTargets, muteSound: true);
        string report = string.Format("Sorted <b>{0}</b> {1} by <b>name</b>", targetCount, singularOrPlural(targetCount, "target", "targets"));
        UIBindings.Game.DisplayToast(report, 3f);
        UIBindings.Sound.PlaySound("beep_sort");
    }

    public static void KeepTrackedTargets() {
        Plugin.Log($"[TC] KeepTrackedTargets");

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        var trackedTargets = AmmoConIndicatorComponent.GetTrackedTargets();
        int targetCount = replaceTargets(trackedTargets, muteSound: true);
        string report = string.Format("Kept <b>{0}</b> tracked {1}", targetCount, singularOrPlural(targetCount, "target", "targets"));
        UIBindings.Game.DisplayToast(report, 3f);
        UIBindings.Sound.PlaySound("beep_sort");
    }

    public static void KeepUntrackedTargets() {
        Plugin.Log($"[TC] KeepTrackedTargets");

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        var currentTargets = GameBindings.Player.TargetList.GetTargets(copy: false);
        var trackedTargets = new HashSet<Unit> (AmmoConIndicatorComponent.GetTrackedTargets());
        currentTargets = currentTargets.FindAll(unit => !trackedTargets.Contains(unit));
        int targetCount = replaceTargets(currentTargets, muteSound: true);
        string report = string.Format("Kept <b>{0}</b> untracked {1}", targetCount, singularOrPlural(targetCount, "target", "targets"));
        UIBindings.Game.DisplayToast(report, 3f);
        UIBindings.Sound.PlaySound("beep_sort");
    }

    public static void SelectTargetsByUnitName(bool asCurrentTarget) {
        Plugin.Log($"[TC] SelectUnitsWithSameName");

        if (NOAutopilotComponent.InternalState.showMenu) return;

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        string getName(Unit target) => (target is Aircraft) ? target.definition.unitName : target.unitName;
        if (GameBindings.Player.TargetList.GetTargetCount() == 0)
            return;
        List<Unit> targets = GameBindings.Player.TargetList.GetTargets();
        Unit target = targets[TargetListControllerComponent.InternalState.targetIndex];
        int targetCountBefore = targets.Count;
        string targetName = getName(target);
        targets.RemoveAll(unit => { bool r = getName(unit) == targetName; if (asCurrentTarget) r = !r; return r; });
        int targetCountAfter = replaceTargets(targets, muteSound: true);
        int removedCount = targetCountBefore - targetCountAfter;
        int targetCount = asCurrentTarget ? targetCountAfter : removedCount;
        string report = string.Format(
            "{0} <b>{1}</b> {2} named <b>{3}</b>",
            asCurrentTarget ? "Kept" : "Removed",
            targetCount,
            singularOrPlural(targetCount, "target", "targets"),
            targetName);
        UIBindings.Game.DisplayToast(report, 3f);
        UIBindings.Sound.PlaySound("beep_sort");
    }

    //lased = true - pop unlased targets, keep lased
    //lased = false - pop lased targets, keep unlased
    public static void SelectTargetsByLasedStatus(bool lased) {
        Plugin.Log($"[TC] SelectTargetsByLasedStatus");

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        if (GameBindings.Player.TargetList.GetTargetCount() == 0)
            return;

        List<Unit> targets = GameBindings.Player.TargetList.GetTargets();
        var hq = GameBindings.GameState.GetCurrentFactionHQ();
        targets.RemoveAll(unit => { bool r = hq.IsTargetLased(unit); if (lased) r = !r; return r; });
        int targetCount = replaceTargets(targets);
        string report = string.Format(
            "Kept <b>{0}</b> {1} {2}",
            targetCount,
            lased ? "lased" : "unlased",
            singularOrPlural(targetCount, "target", "targets"));
        UIBindings.Game.DisplayToast(report, 3f);
        UIBindings.Sound.PlaySound("beep_sort");
    }

    public static void SelectClosestTargets(bool oneTarget, bool silent = false) {
        Plugin.Log($"[TC] SelectClosestTargets");

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        bool selectUnitsToReload = TargetListControllerComponent.InternalState.selectUnitsToReload &&
            SceneSingleton<MapOptions>.i != null &&
            SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Ammo;
        int targetCount = 0;

        CombatHUD combatHUD = UIBindings.Game.GetCombatHUDComponent();
        if (combatHUD != null) {
            var distanceSquared = float.PositiveInfinity;
            Aircraft aircraft = null;
            FactionHQ hq = null;
            GlobalPosition cameraPosition = new ();
            if (TargetListControllerComponent.InternalState.respectTargetingDistances) {
                var hmdDeclutterDistance = HMDDeclutterComponent.GetCurrentDistance();
                if (hmdDeclutterDistance <= 0)
                    hmdDeclutterDistance = float.PositiveInfinity;
                var altTargetSelectionDistance = AltTargetSelectionComponent.MaxDistance;
                if (altTargetSelectionDistance <= 0)
                    altTargetSelectionDistance = float.PositiveInfinity;
                distanceSquared = Mathf.Pow(Mathf.Min(hmdDeclutterDistance, altTargetSelectionDistance), 2);
                var camera = UIBindings.Game.GetCameraStateManager()?.mainCamera;
                if (camera == null) {
                    Plugin.Log($"[TC] SelectClosestTargets: camera is null");
                    return;
                }
                cameraPosition = camera.transform.position.ToGlobalPosition();
                aircraft = combatHUD.aircraft;
                if (aircraft == null) {
                    Plugin.Log($"[TC] SelectClosestTargets: aircraft is null");
                    return;
                }
                hq = aircraft.NetworkHQ;
                if (hq == null) {
                    Plugin.Log($"[TC] SelectClosestTargets: hq is null");
                    return;
                }
            }

            bool PassesDistanceCheck(Unit unit) {
                if (TargetListControllerComponent.InternalState.respectTargetingDistances && distanceSquared != float.PositiveInfinity) {
                    if (!hq.TryGetKnownPosition(unit, out var unitPosition))
                        return false;
                    var currentDistanceSquared = FastMath.SquareDistance(cameraPosition, unitPosition);
                    if (currentDistanceSquared > distanceSquared)
                        return false;
                }
                return true;
            }

            List<Unit> targets = new ();

            if (selectUnitsToReload) {
                foreach (Unit unit in TargetListControllerComponent.InternalState.playerFactionHQ.RearmMissionController.UnitsNeedingRearm) {
                    if (unit != aircraft && PassesDistanceCheck(unit))
                        targets.Add(unit);
                }
            }
            else {
                var markers = TargetListControllerComponent.InternalState.markersCache.GetValue(combatHUD);
                foreach (var marker in markers) {
                    var unit = marker.unit;
                    if (PassesDistanceCheck(unit) && !GameBindings.Player.TargetFilter.CheckExclusions(unit))
                        targets.Add(unit);
                }
            }

            if (targets.Count > 0) {
                sortTargetsByDistance(targets);
                if (oneTarget)
                    targets = [ targets[0] ];
                targetCount = replaceTargets(targets, muteSound: true);
            }
        }

        if (!silent) {
            string report = targetCount == 0 ?
                "Did not select closest targets" :
                $"Selected <b>{targetCount}</b> closest {(singularOrPlural(targetCount, "target", "targets"))}";
            if (selectUnitsToReload)
                report += " to reload";
            UIBindings.Game.DisplayToast(report, 3f);
            UIBindings.Sound.PlaySound("beep_sort");
        }
    }

    public static void MtsSetAutoSelectMode(bool? m = null) {
        Plugin.Log($"[TC] MtsSetAutoSelectMode");

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        if (m == null)
            TargetListControllerComponent.InternalState.mtsAutoActive = !TargetListControllerComponent.InternalState.mtsAutoActive;
        else
            TargetListControllerComponent.InternalState.mtsAutoActive = (bool)m;

        string report = string.Format("Missile autoselection: <b>{0}</b>", TargetListControllerComponent.InternalState.mtsAutoActive ? "on" : "off");
        UIBindings.Game.DisplayToast(report, 3f);

        if (TargetListControllerComponent.InternalState.mtsAutoActive == true) {
            TargetListControllerComponent.SelectIncomingMissiles();
        }
    }

    public static void MtsToggle() {
        Plugin.Log($"[TC] MtsToggle");

        if (!TargetListControllerPlugin.initialized) {
            Plugin.Log("[TC] Not initialized");
            return;
        }

        if (TargetListControllerComponent.InternalState.mtsActive)
            TargetListControllerComponent.DeselectIncomingMissiles();
        else
            TargetListControllerComponent.SelectIncomingMissiles();
    }

    //Have to make following methods public because TargetListControllerComponent uses them
    public static void sortTargetsByDistance(List<Unit> targets) {
        var playerAircraft = GameBindings.Player.Aircraft.GetAircraft();
        var playerPosition = playerAircraft.transform.position.ToGlobalPosition().AsVector3();
        float calcDistanceTo(Unit unit) {
            var knownPosition = playerAircraft.NetworkHQ.GetKnownPosition(unit);
            if (knownPosition is null)
                return float.PositiveInfinity;
            Vector3 unitPosition = ((GlobalPosition)knownPosition).AsVector3();
            float distance = Vector3.Distance(playerPosition, (Vector3)unitPosition);
            return distance;
        }
        targets.Sort((a, b) => {
            float distanceA = calcDistanceTo(a);
            float distanceB = calcDistanceTo(b);
            return distanceA.CompareTo(distanceB);
        });
    }

    public static int replaceTargets(List<Unit> targets, bool muteSound = false) {
        GameBindings.Player.TargetList.DeselectAll();
        GameBindings.Player.TargetList.AddTargets(targets, muteSound : muteSound);
        if (switchCurrentTarget) {
            TargetListControllerComponent.InternalState.targetIndex = 0;
        }
        else {
            TargetListControllerComponent.InternalState.resetIndex = true;
        }
        TargetListControllerComponent.InternalState.updateDisplay = true;
        return GameBindings.Player.TargetList.GetTargetCount();
    }

    public static int sortAndReplaceTargets(List<Unit> targets, bool muteSound = false) {
        sortTargetsByDistance(targets);
        if (!switchCurrentTarget)
            targets.Reverse(); //for behaviour consistency
        return replaceTargets(targets, muteSound);
    }

    public static string singularOrPlural(int i, string singular, string plural) {
        return (i % 10 == 1 && i % 100 != 11) ? singular : plural;
    }
}

public static class TargetListControllerComponent {
    static class LogicEngine {
        public static void Init() {
            InternalState.previousTargetList = [];
            InternalState.targetIndex = 0;
            InternalState.playerFactionHQ = GameBindings.GameState.GetCurrentFactionHQ();
            TargetListControllerComponent.UpdateMissileWarningSystem();
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

            if (InternalState.mtsSelectOnlyInterceptableMissiles && InternalState.mtsActive) {
                var activeStationIndex = GameBindings.Player.Aircraft.Weapons.GetActiveStationIndex();
                if (activeStationIndex != InternalState.activeStationIndex) {
                    SelectIncomingMissiles(mute : true);
                    InternalState.activeStationIndex = activeStationIndex;
                }
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
        public static List<Unit> mtsSavedTargets;
        public static bool mtsActive = false;
        public static bool mtsAutoActive = false;
        public static bool mtsSelectOnlyInterceptableMissiles = true;
        public static float mtsAngleThreshold { set { mtsCosAngleThreshold = Mathf.Cos(0.5f * Mathf.Deg2Rad * value); field = value; } get; } = 180f;
        public static float mtsCosAngleThreshold = 0f;
        public static bool respectTargetingDistances = true;
        public static bool selectUnitsToReload = true;
        public static bool selectClosestTargetsByAmmo = true;
        public static int activeStationIndex = -1;
        public static TargetListSelector_UnitItem activeItem = null;
        public static MissileWarning missileWarningSystem = null;
        public static readonly TraverseCache<TargetScreenUI, FactionHQ> _hqCache = new("hq");
        public static readonly TraverseCache<TargetScreenUI, TextMeshProUGUI> _typeTextCache = new("typeText");
        public static readonly TraverseCache<TargetScreenUI, TextMeshProUGUI> _headingCache = new("heading");
        public static readonly TraverseCache<TargetScreenUI, TextMeshProUGUI> _altitudeCache = new("altitude");
        public static readonly TraverseCache<TargetScreenUI, TextMeshProUGUI> _relAltitudeCache = new("rel_altitude");
        public static readonly TraverseCache<TargetScreenUI, TextMeshProUGUI> _speedCache = new("speed");
        public static readonly TraverseCache<TargetScreenUI, TextMeshProUGUI> _relSpeedCache = new("rel_speed");
        public static readonly TraverseCache<TargetScreenUI, TextMeshProUGUI> _pilotTextCache = new("pilotText");
        public static readonly TraverseCache<TargetScreenUI, TextMeshProUGUI> _distanceCache = new("distance");
        public static readonly TraverseCache<TargetScreenUI, List<Image>> _targetBoxesCache = new("targetBoxes");
        public static readonly TraverseCache<CombatHUD, List<HUDUnitMarker>> markersCache = new("markers");
    }

    static class DisplayEngine {
        public static void Init() {
        }
        public static void Update() {
            //TODO Move in DisplayEngine
            static void UpdateTargetTexts() {
                TargetScreenUI targetScreen = UIBindings.Game.GetTargetScreenUIComponent();
                if (targetScreen == null) return;

                List<Unit> targets = GameBindings.Player.TargetList.GetTargets(copy: false);
                int index = InternalState.targetIndex;
                if (index >= targets.Count) return;

                Unit unit = targets[index];
                FactionHQ hq = InternalState._hqCache.GetValue(targetScreen);

                var typeText = InternalState._typeTextCache.GetValue(targetScreen);
                var heading = InternalState._headingCache.GetValue(targetScreen);
                var altitude = InternalState._altitudeCache.GetValue(targetScreen);
                var rel_altitude = InternalState._relAltitudeCache.GetValue(targetScreen);
                var speed = InternalState._speedCache.GetValue(targetScreen);
                var rel_speed = InternalState._relSpeedCache.GetValue(targetScreen);
                var pilotText = InternalState._pilotTextCache.GetValue(targetScreen);

                var distance = InternalState._distanceCache.GetValue(targetScreen);
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
                        pilotText.text = "Pilot : " + aircraft.pilots[0].player.GetDisplayName(PlayerNameContext.Other);
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
                if (SceneSingleton<MapOptions>.i.tooltipType == MapOptions.TooltipType.Ammo && unit.NetworkHQ == hq)
                    unitName += $" (ammo: {(int)(unit.GetAmmoLevel() * 100)}%)";
                typeText.text = countText + unitName;
            }
            // PROPER UPDATE START
            if (UIBindings.Game.GetCombatHUDTransform() == null ||
                UIBindings.Game.GetTargetScreenTransform(silent: true) == null) {
                return;
            }

            var targetCount = GameBindings.Player.TargetList.GetTargetCount();
            if (targetCount == 0) return;

            if (InternalState.updateDisplay) {
                TargetScreenUI targetScreen = UIBindings.Game.GetTargetScreenUIComponent();
                List<Image> targetIcons = InternalState._targetBoxesCache.GetValue(targetScreen);
                // Wait until the UI has instantiated the boxes for the new targets
                if (targetIcons.Count < targetCount) {
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

            UpdateTargetTexts();
        }
    }

    public static int SelectIncomingMissiles(bool mute = false) {
        Plugin.Log("SelectIncomingMissiles");

        var missileWarningSystem = TargetListControllerComponent.InternalState.missileWarningSystem;
        if (missileWarningSystem == null)
            return 0;
        var knownMissiles = missileWarningSystem.knownMissiles;
        if (knownMissiles == null)
            return 0;

        List<Unit> targets = new ();
        if (TargetListControllerComponent.InternalState.mtsSelectOnlyInterceptableMissiles) {
            var aircraft = GameBindings.Player.Aircraft.GetAircraft();
            if (aircraft == null)
                return 0;
            var hq = aircraft.NetworkHQ;
            if (hq == null)
                return 0;
            var aircraftGlobalPosition = aircraft.GlobalPosition();
            var aircraftForward = aircraft.transform.forward;

            var station = GameBindings.Player.Aircraft.Weapons.GetActiveStation();
            if (station == null)
                return 0;
            var weaponInfo = station.WeaponInfo;

            foreach (var m in knownMissiles) {
                var seekerType = m.GetSeekerType();
                if (weaponInfo.jammer) {
                   if (!(seekerType == "ARH" || seekerType == "SARH"))
                       continue;
                }
                else if (weaponInfo.missile || weaponInfo.energy) {
                   if (!hq.TryGetKnownPosition(m, out var missileGlobalPosition))
                       continue;
                   var toMissile = (missileGlobalPosition - aircraftGlobalPosition).normalized;
                   if (Vector3.Dot(toMissile, aircraftForward) < TargetListControllerComponent.InternalState.mtsCosAngleThreshold)
                       continue;
                }
                targets.Add((Unit)m);
            }

            TargetListControllerComponent.InternalState.activeStationIndex = GameBindings.Player.Aircraft.Weapons.GetActiveStationIndex();
        }
        else
            targets = knownMissiles.ConvertAll(m => (Unit)m);

        if (targets == null || targets.Count == 0)
            return 0;

        if (!TargetListControllerComponent.InternalState.mtsActive) {
            TargetListControllerComponent.InternalState.mtsSavedTargets = GameBindings.Player.TargetList.GetTargets();
            TargetListControllerComponent.InternalState.mtsActive = true;
        }
        
        TargetListControllerPlugin.sortAndReplaceTargets(targets, muteSound : mute);

        if (!mute) {
            string report = string.Format("Selected <b>{0}</b> {1}", targets.Count, TargetListControllerPlugin.singularOrPlural(targets.Count, "missile", "missiles"));
            UIBindings.Game.DisplayToast(report, 3f);
        }

        return targets.Count;
    }

    public static void DeselectIncomingMissiles(bool mute = false) {
        Plugin.Log("DeselectIncomingMissiles");

        if (!TargetListControllerComponent.InternalState.mtsActive)
            return;
        TargetListControllerComponent.InternalState.mtsActive = false;

        var targets = TargetListControllerComponent.InternalState.mtsSavedTargets;
        int targetCount = TargetListControllerPlugin.replaceTargets(targets, muteSound : false);

        if (!mute) {
            string report = string.Format("Recalled <b>{0}</b> {1}", targetCount, TargetListControllerPlugin.singularOrPlural(targetCount, "target", "targets"));
            UIBindings.Game.DisplayToast(report, 3f);
        }
    }

    private static void UpdateMissileWarningSystem() {
        var aircraft = GameBindings.Player.Aircraft.GetAircraft();
        var missileWarningSystem = aircraft != null ? aircraft.GetMissileWarningSystem() : null;
        if (missileWarningSystem != null && missileWarningSystem != TargetListControllerComponent.InternalState.missileWarningSystem) {
            missileWarningSystem.onMissileWarning += TargetListController_OnMissileWarning;
            missileWarningSystem.offMissileWarning += TargetListController_OffMissileWarning;
            InternalState.missileWarningSystem = missileWarningSystem;
        }
    }

    private static void TargetListController_OnMissileWarning(MissileWarning.OnMissileWarning e) {
        if (InternalState.mtsActive || InternalState.mtsAutoActive) {
            bool mute = !InternalState.mtsActive && InternalState.mtsAutoActive;
            SelectIncomingMissiles(mute : mute);
        }
    }

    private static void TargetListController_OffMissileWarning(MissileWarning.OffMissileWarning e) {
        if (!InternalState.mtsActive)
            return;
        var missileCount = SelectIncomingMissiles(mute : true);
        if (missileCount == 0)
            DeselectIncomingMissiles();
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

    private static void TargetMarkerExtraSetup(TargetMarker targetMarker) {
       targetMarker.Show(value: false);
       targetMarker.markerImg.enabled = true;
       float num = (DynamicMap.mapMaximized ? 1f : 0.5f) / UIBindings.Game.GetDynamicMapComponent().mapImage.transform.localScale.x;
       targetMarker.transform.localScale = Vector3.one * num;
    }

    //FIX: This patch removes brief flashing of selected target info and marker on minimap when loading target list
    [HarmonyPatch(typeof(TargetMarker), "ExtraSetup")]
    public static class OnTargetMarkerExtraSetup {
        static void Prefix(ref TargetMarker __instance) {
           TargetMarkerExtraSetup(__instance);
        }

        //0.33.4: Added Postfix()
        static void Postfix(ref TargetMarker __instance) {
           TargetMarkerExtraSetup(__instance);
        }
    }

    //FIX: 0.33.4: This patch highlights target marker for active target and masks target markers for other targets
    [HarmonyPatch(typeof(TargetMarker), "DynamicHide")]
    public static class OnTargetMarkerDynamicHide {
        static bool Prefix(ref TargetMarker __instance) {
           //GetActiveTarget() returns null when there are no targets or there is no target list at all
           var activeTarget = GameBindings.Player.TargetList.GetActiveTarget();
           bool show = UIBindings.Game.GetMapOptionsComponent().showTargetInfo && DynamicMap.mapMaximized;
           if (activeTarget == null || __instance.GetUnit() == activeTarget) {
               __instance.Show(value: show);
           }
           else {
               if (show)
                   __instance.Mask();
               else
                   __instance.Show(false);
           }
           return false;
        }
    }

    /* Save and restore markerImg.color and markerImg.enabled in order to maintain values of these fields,
       because Show() modifies both and  Mask() modifies .color */
    [HarmonyPatch(typeof(TargetMarker), "Show")]
    public static class OnTargetMarkerShow {
        struct ShowState {
            public bool enabled;
            public Color color;
        };

        static bool Prefix(bool value, ref TargetMarker __instance, ref ShowState __state) {
            if (__instance == null || __instance.markerImg == null)
                return false;
            __state.color = __instance.markerImg.color;
            __state.enabled = __instance.markerImg.enabled;
            return true;
        }

        static void Postfix(ref TargetMarker __instance, ref ShowState __state) {
            if (__instance == null || __instance.markerImg == null)
                return;
            __instance.markerImg.color = __state.color;
            __instance.markerImg.enabled = __state.enabled;
        }
    }

    [HarmonyPatch(typeof(TargetMarker), "Mask")]
    public static class OnTargetMarkerMask {
        static void Prefix(ref TargetMarker __instance, ref Color __state) {
            __state = __instance.markerImg.color;
        }

        static void Postfix(ref TargetMarker __instance, ref Color __state) {
            __instance.markerImg.color = __state;
        }
    }

    [HarmonyPatch(typeof(DynamicMap), "Minimize")]
    public static class OnDynamicMapMinimize {
        private static FieldInfo targetMarkerInfo = AccessTools.Field(typeof(UnitMapIcon), "targetMarker");

        static void Postfix(ref DynamicMap __instance) {
            foreach (var icon in __instance.selectedIcons) {
                var unitMapIcon = icon as UnitMapIcon;
                if (unitMapIcon == null)
                    continue;
                var targetMarker = targetMarkerInfo.GetValue(unitMapIcon) as TargetMarker;
                if (targetMarker == null)
                    continue;
                targetMarker.Show(value: false);
                targetMarker.markerImg.enabled = true;
                float scale = (DynamicMap.mapMaximized ? 1f : 0.5f) / __instance.mapImage.transform.localScale.x;
                targetMarker.transform.localScale = Vector3.one * scale;
            }
        }
    }

    //This patch handles clicking on item checkmark in target list
    [HarmonyPatch(typeof(Toggle), "OnPointerClick")]
    public static class OnToggleOnPointerClick {
        static void Postfix(ref Toggle __instance, UnityEngine.EventSystems.PointerEventData eventData) {
            var unitItemComponent = __instance.GetComponentInParent<TargetListSelector_UnitItem>();
            if (unitItemComponent != null) {
                var unit = unitItemComponent.unit;
                /* If either of shift keys is held,
                   left click removes items with the same unit name as for item which checkbox was clicked,
                   right click removes items with unit names that differ */
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    bool asUnit;
                    switch (eventData.button) {
                        case UnityEngine.EventSystems.PointerEventData.InputButton.Left:
                            asUnit = true;
                            break;
                        case UnityEngine.EventSystems.PointerEventData.InputButton.Right:
                            asUnit = false;
                            break;
                        default:
                            return;
                    }
                    string getName(Unit unit) => (unit is Aircraft) ? unit.definition.unitName : unit.unitName;
                    if (GameBindings.Player.TargetList.GetTargetCount() != 0) {
                        var targets = GameBindings.Player.TargetList.GetTargets();
                        string unitName = getName(unit);
                        targets.RemoveAll(unit => { bool r = getName(unit) == unitName; if (!asUnit) r = !r; return r; });
                        GameBindings.Player.TargetList.DeselectAll();
                        GameBindings.Player.TargetList.AddTargets(targets, muteSound : true);
                    }
                }
                else if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Left) {
                    //Removing unit from target list by left-clicking checkbox is already done by game
                    GameBindings.Player.TargetList.DeselectUnit(unit);
                }
                else if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right) {
                    GameBindings.Player.TargetList.DeselectAll();
                    GameBindings.Player.TargetList.AddTarget(unit, muteSound : true);
                }
            }
        }
    }

    //This patch highlights target list item belonging to active target
    [HarmonyPatch(typeof(TargetListSelector), "Update")]
    public static class OnTargetListSelectorUpdate {
        private static MethodInfo onThemeGroupChangedInfo = AccessTools.Method(typeof(TargetListSelector_UnitItem), "TargetListSelector_UnitItem_OnThemeGroupChanged");
        private static FieldInfo unitNameInfo = AccessTools.Field(typeof(TargetListSelector_UnitItem), "unitName");
        private static FieldInfo unitDistInfo = AccessTools.Field(typeof(TargetListSelector_UnitItem), "unitDist");
        private static FieldInfo unitIconInfo = AccessTools.Field(typeof(TargetListSelector_UnitItem), "unitIcon");

        static void Postfix(List<TargetListSelector_UnitItem> ___listItems) {
            var activeTarget = GameBindings.Player.TargetList.GetActiveTarget();
            if (InternalState.activeItem == null || InternalState.activeItem.unit != activeTarget) {
                if (InternalState.activeItem != null) {
                    //Restores item text fields color, preserving unit icon color
                    var unitIcon = ((Image)unitIconInfo.GetValue(InternalState.activeItem));
                    var unitIconColor = unitIcon.color;
                    onThemeGroupChangedInfo.Invoke(InternalState.activeItem, null);
                    unitIcon.color = unitIconColor;
                }
                if (activeTarget != null) {
                    InternalState.activeItem = ___listItems.Find((TargetListSelector_UnitItem item) => item.unit == activeTarget);
                    //Should not be null, but check anyway
                    if (InternalState.activeItem != null) {
                        Color allClear = ThemeManager.Active.ColorTheme.AllClear;
                        ((TextMeshProUGUI)unitNameInfo.GetValue(InternalState.activeItem)).color = allClear;
                        ((TextMeshProUGUI)unitDistInfo.GetValue(InternalState.activeItem)).color = allClear;
                    }
                }
            }
        }
    }
}
