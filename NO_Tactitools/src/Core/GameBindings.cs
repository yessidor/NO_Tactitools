using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace NO_Tactitools.Core;

[HarmonyPatch(typeof(MainMenu), "Start")]
public class GameBindingsPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log($"[GB] Initializing GameBindings");

            Plugin.harmony.PatchAll(typeof(GameBindings.Player.TargetList.OnTargetListSelectorCheckExclusions));
            Plugin.harmony.PatchAll(typeof(GameBindings.Player.Aircraft.Countermeasures.OnAircraftCountermeasures));

            initialized = true;
            Plugin.Log($"[GB] Initialized GameBindings");
        }
    }
}

public class GameBindings {
    public class Units {
        public static bool IsImperial() {
            try {
                return PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Imperial;
            }
            catch (Exception e) {
                Plugin.Log(e.ToString());
                return false;
            }
        }

        public static float ConvertAltitude_ToDisplay(float meters) {
            return IsImperial() ? meters * 3.28084f : meters;
        }

        public static float ConvertVerticalSpeed_ToDisplay(float metersPerSecond) {
            return IsImperial() ? metersPerSecond * 196.850394f : metersPerSecond;
        }

        public static float ConvertSpeed_ToDisplay(float metersPerSecond) {
            return IsImperial() ? metersPerSecond * 1.94384f : metersPerSecond * 3.6f;
        }

        public static float ConvertSpeed_FromDisplay(float displayValue) {
            return IsImperial() ? displayValue / 1.94384f : displayValue / 3.6f;
        }

        public static string GetAltitudeUnit() {
            return IsImperial() ? "ft" : "m";
        }

        public static string GetVerticalSpeedUnit() {
            return IsImperial() ? "fpm" : "m/s";
        }

        public static string GetSpeedUnit() {
            return IsImperial() ? "kts" : "km/h";
        }

        public enum DistanceUnits { m, km, ft, mi };

        public static float ConvertToMeters(float distance, DistanceUnits unit) {
            switch (unit) {
                case DistanceUnits.m:
                    return distance;
                case DistanceUnits.km:
                    return distance * 1000f;
                case DistanceUnits.ft:
                    return distance * 0.3048f;
                case DistanceUnits.mi:
                    return distance * 1609.344f;
                default:
                    throw new ArgumentException("Unsupported distance unit");
            }
        }
    }

    public class GameState {
        private static readonly TraverseCache<MessageUI, ChatBox> _chatBoxCache = new("chat");
        public static bool IsGamePaused() {
            try {
                return GameplayUI.GameIsPaused;
            }
            catch (NullReferenceException e) { Plugin.Log(e.ToString()); return false; }
        }

        public static FactionHQ GetCurrentFactionHQ() {
            try {
                return Player.Aircraft.GetAircraft()?.NetworkHQ;
            }
            catch (NullReferenceException e) { Plugin.Log(e.ToString()); return null; }
        }

        public static FactionMode GetFactionMode(Unit unit) {
            if (unit == null)
                return FactionMode.NoFaction;
            var unitHQ = unit.NetworkHQ;
            if (unitHQ == null)
                return FactionMode.NoFaction;
            return DynamicMap.GetFactionMode(unitHQ);
        }

        public static bool IsChatboxActive() {
            try {
                return _chatBoxCache.GetValue(SceneSingleton<MessageUI>.i).isActiveAndEnabled;
            }
            catch (NullReferenceException) { return false; }
        }
    }

    public class Player { 
        public class Aircraft {
            private static readonly TraverseCache<global::Aircraft, Radar> _radarCache = new("radar");
            
            public static global::Aircraft GetAircraft(bool silent = false) {
                try {
                    var combatHUD = UIBindings.Game.GetCombatHUDComponent();
                    if (combatHUD == null)
                        return null;
                    var aircraft = combatHUD.aircraft;
                    return aircraft != null ? aircraft : null;
                }
                catch (NullReferenceException e) {
                    if (!silent)
                        Plugin.Log(e.ToString());
                    return null;
                }
            }

            public static string GetPlatformName() {
                try {
                    return GetAircraft()?.GetAircraftParameters().aircraftName ?? "Unknown";
                }
                catch (NullReferenceException e) { Plugin.Log(e.ToString()); return "Unknown"; }
            }

            public static void ToggleAutoControl() {
                try {
                    UIBindings.Game.GetCombatHUDComponent().ToggleAutoControl();
                }
                catch (NullReferenceException e) { Plugin.Log(e.ToString()); }
            }

            public static bool IsRadarJammed() {
                try {
                    global::Aircraft aircraft = GetAircraft();
                    if (aircraft == null) return false;
                    Radar radar = _radarCache.GetValue(aircraft);
                    return radar != null && radar.IsJammed();
                }
                catch (Exception e) {
                    Plugin.Log(e.ToString());
                    return false;
                }
            }

            public class Countermeasures {
                private static readonly TraverseCache<CountermeasureManager, IList> _countermeasureStationsCache = new("countermeasureStations");
                private static readonly TraverseCache<RadarJammer, PowerSupply> _powerSupplyCache = new("powerSupply");
                private static readonly TraverseCache<object, int> _irStationAmmoCache = new("ammo");

                private static global::Aircraft _aircraft = null;
                private class StationData {
                    public TraverseCache<object, IList> countermeasuresCache = new ("countermeasures");
                    public object station;
                    public int index;

                    public StationData (object station, int index) {
                        this.station = station;
                        this.index = index;
                    }

                    public object GetFirstCountermeasure() {
                        var countermeasuresList = countermeasuresCache.GetValue(station);
                        return countermeasuresList[0];
                    }
                };
                private static Dictionary<Type, StationData> _stationData = new ();

                private static void UpdateStations() {
                    var aircraft = GetAircraft();
                    if (aircraft != _aircraft) {
                        _stationData.Clear();
                        _aircraft = aircraft;

                        if (aircraft == null)
                            return;

                        TraverseCache<object, IList> countermeasuresCache = new ("countermeasures");
                        IList stationList = GetStationsList();
                        if (stationList != null) {
                            for (int i = 0; i < stationList.Count; i++) {
                                var station = stationList[i];
                                if (station != null) {
                                    IList countermeasures = countermeasuresCache.GetValue(station);
                                    if (countermeasures.Count > 0) {
                                        var firstCountermeasure = countermeasures[0];
                                        if (firstCountermeasure != null) {
                                            var firstCountermeasureType = firstCountermeasure.GetType();
                                            _stationData[firstCountermeasureType] = new StationData (station, i);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                private static StationData GetStationDataByType(Type type) {
                    UpdateStations();
                    if (_stationData.TryGetValue(type, out var stationData))
                        return stationData;
                    else
                        return null;
                }

                private static CountermeasureManager GetCountermeasureManager() {
                    CountermeasureManager currentManager = GetAircraft()?.countermeasureManager;
                    return currentManager != null ? currentManager : null;
                }

                private static IList GetStationsList() {
                    try {
                        CountermeasureManager currentManager = GetCountermeasureManager();
                        if (currentManager == null)
                            return null;
                        return _countermeasureStationsCache.GetValue(currentManager);
                    }
                    catch (NullReferenceException e) {
                        Plugin.Log(e.ToString());
                        return null;
                    }
                }

                private static object GetStationByType(Type type) {
                    return GetStationDataByType(type)?.station;
                }

                private static int GetStationIndexByType(Type type) {
                    return GetStationDataByType(type)?.index ?? -1;
                }

                public static int GetCurrentIndex() {
                    try {
                        return GetCountermeasureManager()?.activeIndex ?? -1;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return -1; }
                }

                public static void SetCurrentIndex(int i) {
                    try {
                        if (i < 0)
                            throw new Exception ("invalid station index");
                        var aircraft = GameBindings.Player.Aircraft.GetAircraft();
                        if (aircraft == null)
                            return;
                        var countermeasureManager = aircraft.countermeasureManager;
                        if (countermeasureManager == null)
                            return;
                        countermeasureManager.activeIndex = (byte)i;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); }
                }

                public static object GetIRStation() {
                    return GetStationByType(typeof(FlareEjector));
                }

                public static int GetIRStationIndex() {
                    return GetStationIndexByType(typeof(FlareEjector));
                }

                public static object GetJammerStation() {
                    return GetStationByType(typeof(RadarJammer));
                }

                public static int GetJammerStationIndex() {
                    return GetStationIndexByType(typeof(RadarJammer));
                }

                public static object GetChaffStation() {
                    return GetStationByType(typeof(ChaffEjector));
                }

                public static int GetChaffStationIndex() {
                    return GetStationIndexByType(typeof(ChaffEjector));
                }

                public static int GetIRFlareAmmo() {
                    try {
                        object IRStation = GetStationDataByType(typeof(FlareEjector))?.station;
                        if (IRStation == null)
                            return 0;
                        int count = _irStationAmmoCache.GetValue(IRStation);
                        return count;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return 0; }
                }

                public static int GetIRFlareMaxAmmo() {
                    try {
                        var stationData = GetStationDataByType(typeof(FlareEjector));
                        if (stationData != null) {
                            FlareEjector ejectorStation = (FlareEjector)stationData.GetFirstCountermeasure();
                            int maxCount = ejectorStation.GetMaxAmmo();
                            return maxCount;
                        }
                        else
                            return 0;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return 0; }
                }

                public static int GetJammerAmmo() {
                    try {
                        var stationData = GetStationDataByType(typeof(RadarJammer));
                        if (stationData != null) {
                            RadarJammer jammerStation = (RadarJammer)stationData.GetFirstCountermeasure();
                            PowerSupply supply = _powerSupplyCache.GetValue(jammerStation);
                            int charge = (int)(supply.GetCharge() * 100f);
                            return charge;
                        }
                        else
                            return 0;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return 0; }
                }

                public static int GetChaffAmmo() {
                    try {
                        object ChaffStation = GetStationDataByType(typeof(ChaffEjector))?.station;
                        if (ChaffStation == null)
                            return 0;
                        int count = _irStationAmmoCache.GetValue(ChaffStation);
                        return count;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return 0; }
                }

                public static int GetChaffMaxAmmo() {
                    try {
                        var stationData = GetStationDataByType(typeof(ChaffEjector));
                        if (stationData != null) {
                            ChaffEjector ejectorStation = (ChaffEjector)stationData.GetFirstCountermeasure();
                            int maxCount = ejectorStation.GetMaxAmmo();
                            return maxCount;
                        }
                        else
                            return 0;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return 0; }
                }

                public static bool HasIRFlare() {
                    try {
                        return GetIRStation() != null;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return false; }
                }

                public static bool HasJammer() {
                    try {
                        return GetJammerStation() != null;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return false; }
                }

                public static bool HasChaff() {
                    try {
                        return GetChaffStation() != null;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return false; }
                }

                public static bool IsFlareSelected() {
                    try {
                        int IRStationIndex = GetIRStationIndex();
                        return IRStationIndex != -1 && IRStationIndex == GetCurrentIndex();
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return false; }
                }

                public static bool IsJammerSelected() {
                    try {
                        int JammerStationIndex = GetJammerStationIndex();
                        return JammerStationIndex != -1 && JammerStationIndex == GetCurrentIndex();
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return false; }
                }

                public static bool IsChaffSelected() {
                    try {
                        int ChaffStationIndex = GetChaffStationIndex();
                        return ChaffStationIndex != -1 && ChaffStationIndex == GetCurrentIndex();
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return false; }
                }

                public static void SetIRFlare() {
                    try {
                        SetCurrentIndex(GetIRStationIndex());
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); }
                }

                public static void SetJammer() {
                    try {
                        SetCurrentIndex(GetJammerStationIndex());
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); }
                }

                public static void SetChaff() {
                    try {
                        SetCurrentIndex(GetChaffStationIndex());
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); }
                }

                public static bool GetState() {
                    var aircraft = GetAircraft();
                    return aircraft?.countermeasureTrigger ?? false;
                }

                public static void SetState(bool state) {
                    var aircraft = GetAircraft();
                    var currentIndex = GetCurrentIndex();
                    if (aircraft == null || currentIndex == -1 || (state && aircraft.radarAlt < 0.2f))
                        return;
                    if ((state && !aircraft.countermeasureTrigger) || (!state && aircraft.countermeasureTrigger)) {
                        /* SetState(true) sets ignoreCountermeasuresCall to true
                           to avoid turning off countermeasure by Aircraft.Countermeasures() call in PilotPlayerState.PlayerControls() */
                        ignoreCountermeasuresCall = false;
                        aircraft.Countermeasures(active: state, (byte)currentIndex);
                        ignoreCountermeasuresCall = state;
                    }
                }

                private static bool ignoreCountermeasuresCall = false;

                [HarmonyPatch(typeof(global::Aircraft), "Countermeasures")]
                public static class OnAircraftCountermeasures {
                    static bool Prefix() {
                        return !ignoreCountermeasuresCall;
                    }
                }
            }

            public class Weapons {
                private static readonly TraverseCache<WeaponStatus, Image> _weaponImageCache = new("weaponImage");

                public static WeaponStation GetActiveStation() {
                    try {
                        return GetAircraft().weaponManager.currentWeaponStation;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return null; }
                }

                public static string GetActiveStationName() {
                    try {
                        var weaponInfo = GetActiveStation().WeaponInfo;
                        string name = weaponInfo.shortName;
                        return name == "" ? weaponInfo.weaponName : name;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return "Unknown Weapon"; }
                }

                public static int GetActiveStationAmmo() {
                    try {
                        if (GetStationCount() == 0)
                            return 0;
                        else
                            return GetActiveStation().Ammo;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return 0; }
                }

                public static int GetActiveStationMaxAmmo() {
                    try {
                        if (GetStationCount() == 0)
                            return 0;
                        else
                            return GetActiveStation().FullAmmo;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return 0; }
                }

                public static string GetActiveStationAmmoString() {
                    try {
                        if (GetStationCount() == 0)
                            return "0";
                        else
                            return GetActiveStation().GetAmmoReadout();
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return "0"; }
                }

                public static float GetActiveStationReloadProgress() {
                    try {
                        return GetActiveStation().GetReloadStatusMax();
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return 0f; }
                }

                public static Image GetActiveStationImage() {
                    try {
                        WeaponStatus currentWeaponStatus = UIBindings.Game.GetWeaponStatus();
                        Image weaponImage = _weaponImageCache.GetValue(currentWeaponStatus);
                        return weaponImage;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return null; }
                }

                public static WeaponStation GetStationByIndex(int index) {
                    try {
                        return index < GetStationCount() ? GetAircraft().weaponStations[index] : null;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return null; }
                }

                public static string GetStationNameByIndex(int index) {
                    try {
                        if (index < GetStationCount()) {
                            var weaponStation = GetStationByIndex(index);
                            var weaponInfo = weaponStation.WeaponInfo;
                            if (weaponInfo.shortName == "") {
                                return weaponInfo.weaponName;
                            }
                            return weaponInfo.shortName;
                        }
                        else {
                            Plugin.Log("[BD] Station index out of range !");
                            return "Unknown Weapon";
                        }
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return "Unknown Weapon"; }
                }

                public static int GetStationAmmoByIndex(int index) {
                    try {
                        if (index < GetStationCount()) {
                            return GetStationByIndex(index).Ammo;
                        }
                        else {
                            Plugin.Log("[BD] Station index out of range !");
                            return 0;
                        }
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return 0; }
                }

                public static int GetStationMaxAmmoByIndex(int index) {
                    try {
                        if (index < GetStationCount()) {
                            return GetStationByIndex(index).FullAmmo;
                        }
                        else {
                            Plugin.Log("[BD] Station index out of range !");
                            return 0;
                        }
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return 0; }
                }

                public static int GetActiveStationIndex() {
                    try {
                        var aircraft = GetAircraft();
                        if (aircraft == null)
                            return -1;
                        var weaponManager = aircraft.weaponManager;
                        if (weaponManager == null)
                            return -1;
                        return aircraft.weaponStations.IndexOf(weaponManager.currentWeaponStation);
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return -1; }
                }

                public static int GetStationCount() {
                    try {
                        return GetAircraft().weaponStations.Count;
                    }
                    catch (NullReferenceException e) { Plugin.Log(e.ToString()); return 5; }
                }

                public static void SetActiveStation(byte index) {
                    try {
                        global::Aircraft aircraft = GetAircraft();

                        if (aircraft == null)
                            return;

                        if (index >= aircraft.weaponStations.Count) {
                            Plugin.Log($"[BD] Station index {index} out of range!");
                            return;
                        }

                        aircraft.SetActiveStation(index);
                        UIBindings.Game.GetCombatHUDComponent().ShowWeaponStation(aircraft.weaponStations[index]);
                    }
                    catch (NullReferenceException e) {
                        Plugin.Log(e.ToString());
                    }
                }
            }
        }

        public class TargetList {
            private static readonly TraverseCache<CombatHUD, List<Unit>> _targetListCache = new("targetList");
            private static readonly TraverseCache<CombatHUD, Dictionary<Unit, HUDUnitMarker>> _markerLookupCache = new("markerLookup");
            private static readonly TraverseCache<CombatHUD, AudioClip> _selectSoundCache = new("selectSound");
            
            public static void AddTargets(List<Unit> units, bool muteSound = false) {
                try {
                    /* marker.SelectMarker() calls SceneSingleton<DynamicMap>.i.SelectIcon(unit) where unit is marker.unit,
                       which in turn calls SceneSingleton<TargetListSelector>.i.CheckExclusions(unit).
                       So the map icon won't be selected if its unit does not pass current target filters. 
                       So a hook with a flag is added to bypass filters and select map icon anyway. */
                    CombatHUD currentCombatHUD = UIBindings.Game.GetCombatHUDComponent();
                    Dictionary<Unit, HUDUnitMarker> markerLookup = _markerLookupCache.GetValue(currentCombatHUD);
                    List<Unit> currentTargets = [.. units];
                    currentTargets.Reverse();
                    foreach (Unit t_unit in currentTargets) {
                        if (markerLookup.TryGetValue(t_unit, out var marker)) {
                            try {
                                checkExclusionsResult = false;
                                marker.SelectMarker();
                                GameBindings.Player.Aircraft.GetAircraft().weaponManager.AddTargetList(t_unit);
                            }
                            finally { checkExclusionsResult = null; }
                        }
                    }
                    if (!muteSound) {
                        AudioClip selectSound = _selectSoundCache.GetValue(currentCombatHUD);
                        SoundManager.PlayInterfaceOneShot(selectSound);
                    }
                }
                catch (NullReferenceException e) {
                    Plugin.Log("GameBindings.Player.TargetList.AddTargets(): " + e.ToString());
                }
            }

            public static void AddTarget(Unit unit, bool muteSound = false) {
                try {
                    CombatHUD currentCombatHUD = UIBindings.Game.GetCombatHUDComponent();
                    Dictionary<Unit, HUDUnitMarker> markerLookup = _markerLookupCache.GetValue(currentCombatHUD);
                    if (markerLookup.TryGetValue(unit, out var marker)) {
                        try {
                            checkExclusionsResult = false;
                            marker.SelectMarker();
                            GameBindings.Player.Aircraft.GetAircraft().weaponManager.AddTargetList(unit);
                        }
                        finally { checkExclusionsResult = null; }
                    }
                    if (!muteSound) {
                        AudioClip selectSound = _selectSoundCache.GetValue(currentCombatHUD);
                        SoundManager.PlayInterfaceOneShot(selectSound);
                    }
                }
                catch (NullReferenceException e) {
                    Plugin.Log("GameBindings.Player.TargetList.AddTarget(): " + e.ToString());
                }
            }

            public static void DeselectAll() {
                try {
                    UIBindings.Game.GetCombatHUDComponent().DeselectAll(false);
                }
                catch (NullReferenceException e) {
                    Plugin.Log("GameBindings.Player.TargetList.DeselectAll(): " + e.ToString());
                }
            }

            public static void DeselectUnit(Unit unit) {
                try {
                    UIBindings.Game.GetCombatHUDComponent().DeSelectUnit(unit);
                }
                catch (NullReferenceException e) {
                    Plugin.Log("GameBindings.Player.TargetList.DeselectUnit(): " + e.ToString());
                }
            }

            public static List<Unit> GetTargets(bool copy = true) {
                try {
                    CombatHUD currentCombatHUD = UIBindings.Game.GetCombatHUDComponent();
                    List<Unit> targetList = _targetListCache.GetValue(currentCombatHUD);
                    return copy ? [.. targetList] : targetList;
                }
                catch (NullReferenceException e) {
                    Plugin.Log("GameBindings.Player.TargetList.GetTargets(): " + e.ToString());
                    return [];
                }
            }

            public static Unit GetActiveTarget() {
                try {
                    CombatHUD currentCombatHUD = UIBindings.Game.GetCombatHUDComponent();
                    if (currentCombatHUD == null)
                        return null;
                    List<Unit> targetList = _targetListCache.GetValue(currentCombatHUD);
                    return targetList != null && targetList.Count > 0 ? targetList[0] : null;
                }
                catch (NullReferenceException e) {
                    Plugin.Log("GameBindings.Player.TargetList.GetAcitveTarget(): " + e.ToString());
                    return null;
                }
            }

            public static int GetTargetCount() {
                try {
                    CombatHUD currentCombatHUD = UIBindings.Game.GetCombatHUDComponent();
                    if (currentCombatHUD == null)
                        return 0;
                    List<Unit> targetList = _targetListCache.GetValue(currentCombatHUD);
                    if (targetList == null)
                        return 0;
                    return targetList.Count;
                }
                catch (NullReferenceException e) {
                    Plugin.Log("GameBindings.Player.TargetList.GetTargetCount(): " + e.ToString());
                    return 0;
                }
            }

            private static bool? checkExclusionsResult = null;

            [HarmonyPatch(typeof(TargetListSelector), "CheckExclusions")]
            public class OnTargetListSelectorCheckExclusions {
                public static bool Prefix(ref bool __result) {
                    if (checkExclusionsResult != null) {
                        __result = (bool)checkExclusionsResult;
                        return false;
                    }
                    else
                        return true;
                }
            }
        }

        public class TargetFilter {
            public static bool CheckExclusions(Unit unit) {
                var targetListSelector = UIBindings.Game.GetTargetListSelectorComponent();
                return targetListSelector != null ? targetListSelector.CheckExclusions(unit) : false;
            }
        }
    }

    public class Helpers {
        static public float ComputeMissileTTI(Missile missile) {
            float timeToImpact = -1f;

            if (missile != null) {
                GlobalPosition targetPosition = new ();
                var target = (Unit)targetInfo.GetValue(missile);
                if (target == null && missile.targetID.TryGetUnit(out var target2)) {
                    target = target2;
                }
                if (target != null) {
                    targetPosition = target.GlobalPosition();
                }
                else {
                    var seeker = (MissileSeeker)seekerInfo.GetValue(missile);
                    if (seeker != null) {
                       target = (Unit)targetUnitInfo.GetValue(seeker);
                       if (target != null)
                           targetPosition = target.GlobalPosition();
                    }
                }
                if (target != null) {
                    var distanceVector = targetPosition - missile.GlobalPosition();
                    var velocityVector = missile.rb.velocity;
                    (var distanceVectorNormalized, var distanceVectorMagnitude) = MathUtils.CalcNormalizedAndMagnitude(distanceVector);
                    var projectedVelocity = Vector3.Dot(velocityVector, distanceVectorNormalized);
                    timeToImpact = projectedVelocity > 0.001f ? distanceVectorMagnitude / projectedVelocity : -1f;
                }
            }

            return timeToImpact;
        }

        static private FieldInfo targetInfo = AccessTools.Field(typeof(Missile), "target");
        static private FieldInfo seekerInfo = AccessTools.Field(typeof(Missile), "seeker");
        static private FieldInfo targetUnitInfo = AccessTools.Field(typeof(MissileSeeker), "targetUnit");
    }
}

public class TransformUtils {
    public static string ToString(Transform transform) {
        return $"localPosition: {transform.localPosition}; localEulerAngles: {transform.localEulerAngles}; localScale:{transform.localScale}; position: {transform.position}; eulerAngles: {transform.eulerAngles}; lossyScale: {transform.lossyScale}";
    }

    public static string ToString(RectTransform transform) {
        return $"{ToString((Transform)transform)}; anchoredPosition: {transform.anchoredPosition}; anchoredPosition3D: {transform.anchoredPosition3D}; anchorMax: {transform.anchorMax}; anchorMin: {transform.anchorMin}; offsetMax: {transform.offsetMax}; offsetMin: {transform.offsetMin}; pivot: {transform.pivot}; rect: {transform.rect}; sizeDelta: {transform.sizeDelta}";
    }
}
