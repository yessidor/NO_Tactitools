using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NuclearOption.UI;
using NuclearOption.UIStyleSystem;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

public class EMWSComponent {
    public static void Init() {
        if (!initialized) {
            Plugin.Log("[EMWS] Initializing Early Missile Warning System component");

            Plugin.harmony.PatchAll(typeof(OnHUDUnitMarkerSetNew));
            Plugin.harmony.PatchAll(typeof(OnHUDUnitMarkerRemoveIcon));
            Plugin.harmony.PatchAll(typeof(OnHUDUnitMarkerUpdateColor));
            Plugin.harmony.PatchAll(typeof(OnHUDUnitMarkerUpdateMaximized));
            Plugin.harmony.PatchAll(typeof(OnUnitMapIcon_UpdateColor));
            Plugin.harmony.PatchAll(typeof(OnUnitMapIconUpdateIcon));
            Plugin.harmony.PatchAll(typeof(OnPlatformStart));
            Plugin.harmony.PatchAll(typeof(OnPlatformStop));
            Plugin.harmony.PatchAll(typeof(OnPlatformUpdate));

            var bindings = new List<BindingHelper.Binding> ();

            bindings.Add(new BindingHelper.Binding (typeof(EMWSComponent), "DistanceUnit", Plugin.EMWS.DistanceUnit));
            bindings.Add(new BindingHelper.Binding (typeof(EMWSComponent), "ProcessOnlyEnemyMissiles", Plugin.EMWS.ProcessOnlyEnemyMissiles));

            foreach (var missileConfigData in Plugin.EMWS.MissileConfigDatum) {
                var missileData = new MissileData ();
                missileDatum.Add(missileData);

                bindings.Add(new BindingHelper.Binding (missileData.SeekerTypes, "Entries", missileConfigData.SeekerTypes));
                bindings.Add(new BindingHelper.Binding (missileData, "Angle", missileConfigData.Angle));
                bindings.Add(new BindingHelper.Binding (missileData, "Distance", missileConfigData.Distance));
                bindings.Add(new BindingHelper.Binding (missileData, "ItemColor", missileConfigData.ItemColor));
                bindings.Add(new BindingHelper.Binding (missileData, "FlashMarker", missileConfigData.FlashMarker));
                bindings.Add(new BindingHelper.Binding (missileData, "ShowNotchLine", missileConfigData.ShowNotchLine));
                bindings.Add(new BindingHelper.Binding (missileData, "ShowVectorLine", missileConfigData.ShowVectorLine));
                bindings.Add(new BindingHelper.Binding (missileData, "ShowNotchIndicator", missileConfigData.ShowNotchIndicator));
                bindings.Add(new BindingHelper.Binding (missileData, "ShowText", missileConfigData.ShowText));
                bindings.Add(new BindingHelper.Binding (missileData, "HMDMarkerScale", missileConfigData.HMDMarkerScale));
                bindings.Add(new BindingHelper.Binding (missileData, "MapIconScale", missileConfigData.MapIconScale));
            }

            BindingHelper.ApplyBindings(bindings);

            initialized = true;

            Plugin.Log("[EMWS] Initialized Early Missile Warning System component");
        }
    }

    public static bool ProcessingMarker(HUDUnitMarker marker) {
        return marker.unit is Missile missile && threatItems.TryGetValue(missile, out var threatItemData) && threatItemData.Active;
    }

    public static GameBindings.Units.DistanceUnits DistanceUnit {
        get;
        set {
            field = value;
            foreach (var missileData in missileDatum)
                missileData.distanceInMeters = GameBindings.Units.ConvertToMeters(missileData.Distance, value);
        }
    } = GameBindings.Units.DistanceUnits.m;

    public static bool ProcessOnlyEnemyMissiles = true;

    private static bool initialized = false;
    private static MissileWarning missileWarningSystem = null;

    private class MissileData {
        public RegexEntries SeekerTypes = new ();
        public float Angle { get; set { field = value; dotThreshold = Mathf.Cos(0.5f * value * Mathf.Deg2Rad); } } = 30f;
        public float Distance { get; set { field = value; distanceInMeters = GameBindings.Units.ConvertToMeters(value, DistanceUnit); } } = 50000f;

        public Color ItemColor = Color.grey;
        public bool FlashMarker = true;

        public bool ShowNotchLine = true;
        public bool ShowVectorLine = true;
        public bool ShowNotchIndicator = true;
        public bool ShowText = true;

        public float HMDMarkerScale = 1f;
        public float MapIconScale = 1f;

        public float dotThreshold = 0f;
        public float distanceInMeters = 0f;
    }

    private static List<MissileData> missileDatum = new ();

    private class ThreatItemData {
        public MissileData missileData = null;
        public Missile missile = null;
        public ThreatItem threatItem = null;
        public Text text = null;
        public GameObject notchLine = null;
        public Image notchLineImage = null;
        public GameObject vectorLine = null;
        public Image vectorLineImage = null;
        public GameObject notchIndicator = null;
        public Image notchIndicatorBox = null;
        public Text notchIndicatorLabel = null;
        public HUDUnitMarker hmdMarker = null;
        public UnitMapIcon mapIcon = null;
        public Color hmdMarkerColor = Color.white;
        public Color hmdMarkerImageColor = Color.white;
        public bool hmdMarkerFlashing = false;
        public bool hmdMarkerAlwaysMaximized = false;
        public float hmdMarkerScale = 1.0f;
        public Color mapIconColor = Color.black;
        public bool mapIconFlashing = false;
        public float mapIconScale = 1.0f;

        public ThreatItemData(Missile missile, MissileData missileData) {
            this.missileData = missileData;
            this.missile = missile;

            var combatHUD = UIBindings.Game.GetCombatHUDComponent();
            var threatList = threatListCache.GetValue(combatHUD);
            var transform = threatList.gameObject.transform;
            var threatItemPrefab = threatItemPrefabCache.GetValue(threatList);
            threatItem = UnityEngine.Object.Instantiate(threatItemPrefab, transform).GetComponent<ThreatItem>();

            threatItem.SetItem(missile.persistentID);
            //FoundIcon() initializes threatItem
            if (!threatItem.FoundIcon()) {
                DestroySelf();
                return;
            }

            text = (Text)textInfo.GetValue(threatItem);

            notchLine = (GameObject)notchLineInfo.GetValue(threatItem);
            if (notchLine == null) {
                notchLine = UIBindings.Game.GetDynamicMapComponent().ShowNotchLine();
                notchLineInfo.SetValue(threatItem, notchLine);
            }
            notchLineImage = (Image)notchLine.GetComponent<Image>();

            vectorLine = (GameObject)vectorLineInfo.GetValue(threatItem);
            vectorLineImage = (Image)vectorLine.GetComponent<Image>();

            notchIndicator = (GameObject)notchIndicatorInfo.GetValue(threatItem);
            if (notchIndicator == null) {
                notchIndicator = UIBindings.Game.GetCombatHUDComponent().ShowNotchIndicator();
                notchIndicatorInfo.SetValue(threatItem, notchIndicator);
            }
            notchIndicatorBox = (Image)notchIndicator.GetComponent<Image>();
            notchIndicatorBoxInfo.SetValue(threatItem, notchIndicatorBox);
            notchIndicatorLabel = (Text)notchIndicator.GetComponentInChildren<Text>();
            notchIndicatorLabelInfo.SetValue(threatItem, notchIndicatorLabel);

            //deferring initialization of hmdMarker and mapIcon

            Active = false;
        }

        public void DestroySelf() {
            UnityEngine.Object.Destroy(threatItem.gameObject);
            threatItem = null;
        }

        public bool Active {
            get {
                field = Valid && threatItem.gameObject.activeSelf;
                return field;
            }
            set {
                if (!Valid)
                    return;

                threatItem.gameObject.SetActive(value);
                text.gameObject.SetActive(value && missileData.ShowText);
                notchLine.gameObject.SetActive(value && missileData.ShowNotchLine);
                vectorLine.gameObject.SetActive(value && missileData.ShowVectorLine);
                bool notchIndicatorActive = value && missileData.ShowNotchIndicator;
                notchIndicator.gameObject.SetActive(notchIndicatorActive);
                notchIndicatorBox.gameObject.SetActive(notchIndicatorActive);
                notchIndicatorLabel.gameObject.SetActive(notchIndicatorActive);

                if (field != value) {
                    field = value;

                    if (hmdMarker == null && UIBindings.Game.GetCombatHUDComponent().TryGetMarker((Unit)missile, out var marker)) {
                        hmdMarker = marker;
                        SaveHMDMarkerProps();
                    }

                    if (mapIcon == null && DynamicMap.TryGetMapIcon((Unit)missile, out var icon)) {
                        mapIcon = icon;
                        SaveMapIconProps();
                    }

                    if (value) {
                        SetHMDMarkerColor();
                        SetHMDMarkerScale();
                        SetMapIconColor();
                        SetMapIconScale();
                    }
                    else {
                        RestoreHMDMarkerProps();
                        RestoreMapIconProps();
                    }
                }
            }
        } = false;

        public bool Valid {
            get {
                field = threatItem != null;
                return field;
            }
            private set;
        }

        public void Animate() {
            if (!Valid || !Active)
                return;

            threatItem.AnimateItem();

            var color = missileData.ItemColor;

            if (missileData.ShowText) {
                if (!text.gameObject.activeSelf)
                    text.gameObject.SetActive(true);
                text.color = color;
            }
            else if (text.gameObject.activeSelf)
                text.gameObject.SetActive(false);

            if (missileData.ShowNotchIndicator) {
                if (!notchIndicator.activeSelf) {
                    notchIndicator.SetActive(true);
                }
                notchIndicatorBox.color = color;
                notchIndicatorLabel.color = color;
            }
            else if (notchIndicator.activeSelf)
                notchIndicator.SetActive(false);

            if (missileData.ShowNotchLine) {
                if (!notchLine.activeSelf)
                    notchLine.SetActive(true);
                notchLineImage.color = color;
            }
            else if (notchLine.activeSelf)
                notchLine.SetActive(false);

            if (missileData.ShowVectorLine) {
                //assuming that if vector line was enabled, it was also aligned
                if (!vectorLine.activeSelf) {
                    vectorLine.SetActive(true);
                    alignVectorLineInfo.Invoke(threatItem, null);
                }
                vectorLineImage.color = color;
            }
            else if (vectorLine.activeSelf)
                vectorLine.SetActive(false);

            var combatHUD = UIBindings.Game.GetCombatHUDComponent();
            if (hmdMarker == null && combatHUD.TryGetMarker((Unit)missile, out var marker)) {
                hmdMarker = marker;
                SaveHMDMarkerProps();
                SetHMDMarkerColor();
                SetHMDMarkerScale();
            }

            if (mapIcon == null && DynamicMap.TryGetMapIcon((Unit)missile, out var icon)) {
                mapIcon = icon;
                SaveMapIconProps();
                SetMapIconColor();
                SetMapIconScale();
            }
        }

        //HMD marker

        public void SaveHMDMarkerProps() {
            if (hmdMarker == null)
                return;

            hmdMarkerColor = (Color)hudumColorInfo.GetValue(hmdMarker);
            hmdMarkerImageColor = hmdMarker.image.color;
            hmdMarkerFlashing = (bool)hudumFlashingInfo.GetValue(hmdMarker);

            hmdMarkerScale = hmdMarker.image.transform.localScale.x;
            hmdMarkerAlwaysMaximized = hmdMarker.alwaysMaximized;
        }

        public void RestoreHMDMarkerProps() {
            if (MissileManagedByThreatList() || hmdMarker == null)
                return;

            hudumColorInfo.SetValue(hmdMarker, hmdMarkerColor);
            hmdMarker.image.color = hmdMarkerImageColor;
            if (missileData.FlashMarker)
                hmdMarker.SetFlashing(hmdMarkerFlashing);

            var transform = hmdMarker.image.transform;
            transform.localScale = Vector3.one * hmdMarkerScale;
            hmdMarker.alwaysMaximized = hmdMarkerAlwaysMaximized;
        }

        public void SetHMDMarkerColor() {
            if (hmdMarker == null)
                return;

            var color = missileData.ItemColor;
            hudumColorInfo.SetValue(hmdMarker, color);
            hmdMarker.image.color = color;
            if (missileData.FlashMarker)
                hmdMarker.SetFlashing(true);
        }

        public void SetHMDMarkerScale() {
            if (hmdMarker == null)
                return;

            var transform = hmdMarker.image.transform;
            transform.localScale = Vector3.one * missileData.HMDMarkerScale;
            hmdMarker.alwaysMaximized = true;
        }

        //Map icon

        public void SaveMapIconProps() {
            if (mapIcon == null)
                return;

            mapIconColor = mapIcon.iconImage.color;
            mapIconFlashing = (bool)mapIconFlashingInfo.GetValue(mapIcon);
            mapIconScale = (float)mapIconUnitSizeFactorInfo.GetValue(mapIcon);
        }

        public void RestoreMapIconProps() {
            if (mapIcon == null)
                return;

            if (!MissileManagedByThreatList()) {
                //works ok without call to UnitMapIcon_UpdateColor()
                //mapIcon.UnitMapIcon_UpdateColor();
                mapIcon.iconImage.color = mapIconColor;
                if (missileData.FlashMarker)
                    mapIconFlashingInfo.SetValue(mapIcon, mapIconFlashing);
            }

            //unitSizeFactor should be restored even if missile is managed by ThreatList
            mapIconUnitSizeFactorInfo.SetValue(mapIcon, mapIconScale);
        }

        public void SetMapIconColor() {
            if (mapIcon == null)
                return;

            mapIcon.iconImage.color = missileData.ItemColor;
            if (missileData.FlashMarker)
                mapIconFlashingInfo.SetValue(mapIcon, true);
        }

        public void SetMapIconScale() {
            if (mapIcon == null)
                return;

            var unitSizeFactor = (float)mapIconUnitSizeFactorInfo.GetValue(mapIcon);
            unitSizeFactor = missileData.MapIconScale;
            mapIconUnitSizeFactorInfo.SetValue(mapIcon, unitSizeFactor);
        }

        public bool MissileManagedByThreatList() {
            var combatHUD = UIBindings.Game.GetCombatHUDComponent();
            var threatList = threatListCache.GetValue(combatHUD);
            var itemLookup = threatListItemLookupCache.GetValue(threatList);
            return itemLookup.ContainsKey(missile.persistentID);
        }
    }

    private static Aircraft currentAircraft = null;
    private static Dictionary<Missile, ThreatItemData> threatItems = new ();
    private static List<Missile> toRemove = new ();

    private static TraverseCache<CombatHUD, ThreatList> threatListCache = new ("threatList");
    private static TraverseCache<ThreatList, GameObject> threatItemPrefabCache = new ("threatItemPrefab");
    private static TraverseCache<ThreatList, Dictionary<PersistentID, ThreatItem>> threatListItemLookupCache = new ("itemLookup");
    private static FieldInfo textInfo = AccessTools.Field(typeof(ThreatItem), "text");
    private static FieldInfo notchIndicatorInfo = AccessTools.Field(typeof(ThreatItem), "notchIndicator");
    private static FieldInfo notchIndicatorBoxInfo = AccessTools.Field(typeof(ThreatItem), "notchIndicatorBox");
    private static FieldInfo notchIndicatorLabelInfo = AccessTools.Field(typeof(ThreatItem), "notchIndicatorLabel");
    private static FieldInfo notchLineInfo = AccessTools.Field(typeof(ThreatItem), "notchLine");
    private static FieldInfo vectorLineInfo = AccessTools.Field(typeof(ThreatItem), "vectorLine");
    private static MethodInfo alignVectorLineInfo = AccessTools.Method(typeof(ThreatItem), "AlignVectorLine");
    private static FieldInfo hudumColorInfo = AccessTools.Field(typeof(HUDUnitMarker), "color");
    private static FieldInfo hudumFlashingInfo = AccessTools.Field(typeof(HUDUnitMarker), "flashing");
    private static FieldInfo mapIconUnitSizeFactorInfo = AccessTools.Field(typeof(UnitMapIcon), "unitSizeFactor");
    private static FieldInfo mapIconFlashingInfo = AccessTools.Field(typeof(UnitMapIcon), "flashing");
    private static FieldInfo mapIconSelectedInfo = AccessTools.Field(typeof(UnitMapIcon), "isSelected");

    private static void UpdateMissileWarningSystem() {
        var aircraft = GameBindings.Player.Aircraft.GetAircraft();
        var mws = aircraft != null ? aircraft.GetMissileWarningSystem() : null;
        if (mws != null && mws != missileWarningSystem) {
            mws.onMissileWarning += OnMissileWarning;
            mws.offMissileWarning += OffMissileWarning;
            missileWarningSystem = mws;
        }
    }

    private static void AddThreatItemIfNeeded(Missile missile) {
        //ThreatItem.FoundIcon() will throw exception if either players' aircraft or missile is null
        if (GameBindings.Player.Aircraft.GetAircraft() == null || missile == null)
            return;

        if (ProcessOnlyEnemyMissiles && GameBindings.GameState.GetFactionMode(missile) == FactionMode.Friendly)
            return;

        MissileData missileData = null;
        var seekerType = missile.GetSeekerType();
        foreach (var md in missileDatum) {
            if (md.SeekerTypes.Matches(seekerType)) {
                missileData = md;
                break;
            }
        }
        if (missileData == null) {
            return;
        }

        var threatItemData = new ThreatItemData (missile, missileData);
        if (!threatItemData.Valid) {
            return;
        }

        threatItems[missile] = threatItemData;
    }

    private static void RemoveThreatItemIfNeeded(Missile missile) {
        if (threatItems.TryGetValue(missile, out var threatItemData)) {
            threatItemData.Active = false;
            threatItemData.DestroySelf();
            threatItems.Remove(missile);
        }
    }

    private static void UpdateThreatItems() {
        var aircraft = GameBindings.Player.Aircraft.GetAircraft();
        if (aircraft == null)
            return;
        var hq = aircraft.NetworkHQ;
        if (hq == null)
            return;
        if (aircraft != currentAircraft) {
            currentAircraft = aircraft;
            DestroyThreatItems();
        }

        var aircraftGlobalPos = aircraft.GlobalPosition();
        foreach ((var missile, var threatItemData) in threatItems) {
            if (missile == null) {
                threatItemData.Active = false;
                UnityEngine.Object.Destroy(threatItemData.threatItem.gameObject);
                toRemove.Add(missile);
                continue;
            }

            var shouldAnimate = false;
            if (!threatItemData.MissileManagedByThreatList() && hq.TryGetKnownPosition(missile, out var missileGlobalPos)) {
                (var toAircraftNormalized, var toAircraftMagniture) = MathUtils.CalcNormalizedAndMagnitude(aircraftGlobalPos - missileGlobalPos);
                var distanceInMeters = threatItemData.missileData.distanceInMeters;
                if (distanceInMeters > 0 && toAircraftMagniture < distanceInMeters) {
                    var missileVelocityNormalized = missile.rb.velocity.normalized;
                    if (Vector3.Dot(toAircraftNormalized, missileVelocityNormalized) > threatItemData.missileData.dotThreshold)
                        shouldAnimate = true;
                }
            }

            if (shouldAnimate) {
                if (!threatItemData.Active) {
                    threatItemData.Active = true;
                }

                threatItemData.Animate();

            }
            else if (threatItemData.Active) {
                threatItemData.Active = false;
            }
        }

        foreach (var missile in toRemove)
            threatItems.Remove(missile);
    }

    private static void DestroyThreatItems() {
        foreach (var threatItemData in threatItems.Values) {
            var threatItem = threatItemData.threatItem;
            if (threatItem != null)
                UnityEngine.Object.Destroy(threatItem.gameObject);
        }
        threatItems.Clear();
    }

    private static void OnMissileWarning(MissileWarning.OnMissileWarning e) {
        if (threatItems.TryGetValue(e.missile, out var threatItemData)) {
            threatItemData.Active = false;
        }
    }

    private static void OffMissileWarning(MissileWarning.OffMissileWarning e) {
        if (threatItems.TryGetValue(e.missile, out var threatItemData)) {
            threatItemData.Active = true;
        }
    }

    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class OnMainMenuStart {
        static void Postfix() {
            Init();
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "SetNew")]
    public class OnHUDUnitMarkerSetNew {
        public static void Postfix(ref HUDUnitMarker __instance) {
            if (__instance.unit is Missile missile) {
                AddThreatItemIfNeeded(missile);
            }
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "RemoveIcon")]
    public class OnHUDUnitMarkerRemoveIcon {
        public static void Prefix(ref HUDUnitMarker __instance) {
            if (__instance.unit is Missile missile) {
                RemoveThreatItemIfNeeded(missile);
            }
        }
    }

    [HarmonyPatch(typeof(HUDUnitMarker), "UpdateColor")]
    public class OnHUDUnitMarkerUpdateColor {
        public static void Postfix(ref HUDUnitMarker __instance) {
            if (__instance.selected)
               return;
            var missile = __instance.unit as Missile;
            if (missile == null)
                return;
            if (!threatItems.TryGetValue(missile, out var threatItemData))
                return;
            if (!threatItemData.Active)
                return;

            threatItemData.SetHMDMarkerColor();
        }
    }

    //Overriding scale set by UpdateMaximized()
    [HarmonyPatch(typeof(HUDUnitMarker), "UpdateMaximized")]
    public class OnHUDUnitMarkerUpdateMaximized {
        public static void Postfix(ref HUDUnitMarker __instance) {
            if (__instance.selected)
               return;
            var missile = __instance.unit as Missile;
            if (missile == null)
                return;
            if (!threatItems.TryGetValue(missile, out var threatItemData))
                return;
            if (!threatItemData.Active)
                return;

            threatItemData.SetHMDMarkerScale();
        }
    }

    [HarmonyPatch(typeof(UnitMapIcon), "UnitMapIcon_UpdateColor")]
    public class OnUnitMapIcon_UpdateColor {
        public static void Postfix(ref UnitMapIcon __instance, ref bool ___isSelected) {
            if (___isSelected)
               return;
            var missile = __instance.unit as Missile;
            if (missile == null)
                return;
            if (!threatItems.TryGetValue(missile, out var threatItemData))
                return;
            if (!threatItemData.Active)
                return;

            threatItemData.SetMapIconColor();
        }
    }

    //Overriding colors of flashing map icon
    [HarmonyPatch(typeof(UnitMapIcon), "UpdateIcon")]
    public class OnUnitMapIconUpdateIcon {
        public static void Postfix(ref UnitMapIcon __instance, ref bool ___flashing) {
            if (!___flashing)
                return;
            var missile = __instance.unit as Missile;
            if (missile == null)
                return;
            if (!threatItems.TryGetValue(missile, out var threatItemData))
                return;
            if (!threatItemData.Active)
                return;

            float t = Mathf.Sin(Time.realtimeSinceStartup * 20f) * 0.5f + 0.5f;
            __instance.iconImage.color = Color.Lerp(threatItemData.missileData.ItemColor, ThemeManager.Active.ColorTheme.MapIconHostileSelected, t);
        }
    }

    [HarmonyPatch(typeof(TacScreen), "Initialize")]
    public static class OnPlatformStart {
        static void Postfix() {
            UpdateMissileWarningSystem();
        }
    }

    [HarmonyPatch(typeof(TacScreen), "OnDestroy")]
    public static class OnPlatformStop {
        static void Prefix() {
            DestroyThreatItems();
        }
    }

    [HarmonyPatch(typeof(TacScreen), "Update")]
    public static class OnPlatformUpdate {
        static void Postfix() {
            UpdateThreatItems();
        }
    }
};
