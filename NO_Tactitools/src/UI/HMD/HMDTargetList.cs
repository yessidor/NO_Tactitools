using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Collections.Generic;
using NuclearOption.UIStyleSystem;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

public class HMDTargetListComponent {
    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class OnMainMenuStart {
        static void Postfix() {
            if (!initialized) {
                Plugin.Log($"[HMDTL] HMD Target List component starting !");

                Plugin.harmony.PatchAll(typeof(OnCombatHUDLateUpdate));

                var bindings = new BindingHelper.Binding[] {
                    new (typeof(HMDTargetListComponent), "Anchor", Plugin.HMDTargetList.Anchor),
                    new (typeof(HMDTargetListComponent), "Position", Plugin.HMDTargetList.Position),
                    new (typeof(HMDTargetListComponent), "Spacing", Plugin.HMDTargetList.Spacing),
                    new (typeof(HMDTargetListComponent), "FontSize", Plugin.HMDTargetList.FontSize),
                    new (typeof(HMDTargetListComponent), "BackgroundColor", Plugin.HMDTargetList.BackgroundColor),
                    new (typeof(HMDTargetListComponent), "ShowMissileTargets", Plugin.HMDTargetList.ShowMissileTargets),
                    new (typeof(HMDTargetListComponent), "MissileTargetsLabelColor", Plugin.HMDTargetList.MissileTargetsLabelColor),
                    new (typeof(HMDTargetListComponent), "FlashColor", Plugin.HMDTargetList.FlashColor),
                    new (typeof(HMDTargetListComponent), "FlashBeforeImpactTime", Plugin.HMDTargetList.FlashBeforeImpactTime),
                    new (typeof(HMDTargetListComponent), "MaxNumEntries", Plugin.HMDTargetList.MaxNumEntries),
                    new (typeof(HMDTargetListComponent), "EntryFormatString", Plugin.HMDTargetList.EntryFormatString),
                    new (typeof(HMDTargetListComponent), "UpdatePeriod", Plugin.HMDTargetList.UpdatePeriod),
                };
                BindingHelper.ApplyBindings(bindings);

                MissileCacheComponent.Init();
                playerMissileCache = MissileCacheManager.GetCache<PlayerMissileData>("player");
                friendlyMissileCache = MissileCacheManager.GetCache<MissileData>("friendly");

                initialized = true;

                Plugin.Log($"[HMDTL] HMD Target List component started !");
            }
        }
    }

    public static UIBindings.Draw.Anchors Anchor {
        get;
        set {
            field = value;
            if (container != null)
                UIBindings.Draw.SetAnchor(container.GetComponent<RectTransform>(), value);
        }
    } = UIBindings.Draw.Anchors.BottomLeft;

    public static Vector2 Position {
        get;
        set {
            field = value;
            if (container != null) {
                var containerTransform = container.GetComponent<RectTransform>();
                if (containerTransform != null)
                    containerTransform.anchoredPosition = value;
            }
        }
    } = Vector2.zero;
    public static int Spacing {
        get;
        set {
            field = value;
            if (container != null) {
                var layout = container.GetComponent<VerticalLayoutGroup>();
                if (layout != null)
                    layout.spacing = value;
            }
        }
    } = 0;
    public static int FontSize {
        get;
        set {
            field = value;
            foreach (var labelData in labelDatum)
                labelData.label.SetFontSize(value);
        }
    } = 20;
    public static Color BackgroundColor {
        get;
        set {
            field = value;
            if (container != null) {
                var background = container.GetComponent<Image>();
                if (background != null)
                    background.color = value;
            }
        }
    } = new Color (0, 0, 0, 0.3f);
    public static bool ShowMissileTargets = true;
    public static Color MissileTargetsLabelColor = Color.grey;
    public static Color FlashColor = Color.yellow;
    public static float FlashBeforeImpactTime = -1;
    public static int MaxNumEntries = -1;
    public static string EntryFormatString = "";
    public static float UpdatePeriod = 0.1f;

    [HarmonyPatch(typeof(CombatHUD), "LateUpdate")]
    class OnCombatHUDLateUpdate {
        static void Postfix(CombatHUD __instance) {
            if (combatHUD != __instance) {
                combatHUD = __instance;

                //reinit
                if (container != null)
                    UnityEngine.Object.Destroy(container);
                labelDatum.Clear();

                container = new GameObject ("HMDTargetList");

                var containerTransform = container.AddComponent<RectTransform>();
                containerTransform.SetParent(combatHUD.transform, true);
                containerTransform.anchoredPosition = Position;
                containerTransform.localScale = Vector3.one;
                //TODO Remove
                //containerTransform.localScale = new Vector3 (1.0f / containerTransform.lossyScale.x, 1.0f / containerTransform.lossyScale.y, 1.0f / containerTransform.lossyScale.z);
                Anchor = Anchor;

                var background = container.AddComponent<Image>();
                background.color = BackgroundColor;

                var layout = container.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.LowerLeft;
                layout.childControlHeight = false;
                layout.childControlWidth = false;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = false;
                layout.spacing = Spacing;

                var fitter = container.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                lastUpdate = Time.timeSinceLevelLoad;
            }

            if (Time.timeSinceLevelLoad >= lastUpdate + UpdatePeriod) {
                lastUpdate = Time.timeSinceLevelLoad;

                var targets = GameBindings.Player.TargetList.GetTargets(false);

                var aircraft = combatHUD.aircraft;
                var hq = aircraft == null ? null : aircraft.NetworkHQ;
                var laserDesignator = aircraft == null ? null : aircraft.GetLaserDesignator();
                var lasedTargets = laserDesignator == null ? null : laserDesignator.GetLasedTargets();

                int i = 0;
                LabelData GetOrCreateLabelData() {
                    LabelData labelData = null;
                    if (i == MaxNumEntries)
                        return null;

                    if (i == labelDatum.Count) {
                        var label = new UIBindings.Draw.UILabel(
                            name: $"label{i}",
                            position: Vector2.zero,
                            fontSize: FontSize,
                            backgroundOpacity: 0.0f,
                            UIParent: container.GetComponent<RectTransform>(),
                            font: UIBindings.Draw.GetNotchIndicatorFont());
                        label.GetRectTransform().localScale = Vector3.one;
                        labelData = new LabelData { label = label };
                        labelDatum.Add(labelData);
                    }
                    else
                        labelData = labelDatum[i];
                    i++;
                    return labelData;
                }

                int nameLength = 0;
                int codeLength = 0;
                void UpdateNameLength(IEnumerable<Unit> units) {
                    foreach (var unit in units) {
                        if (unit == null)
                            continue;
                        var unitNameLength = unit.unitName.Length;
                        if (unitNameLength > nameLength)
                            nameLength = unitNameLength;
                        var unitCodeLength = unit.definition.code.Length;
                        if (unitCodeLength > codeLength)
                            codeLength = unitCodeLength;
                    }
                }

                UpdateNameLength(targets);

                void PopulateLabelData(LabelData labelData, Unit target, List<PlayerMissileData> playerMissileDatum = null, List<MissileData> friendlyMissileDatum = null) {
                    if (labelData == null)
                        throw new NullReferenceException("labelData is null");
                    if (target == null)
                        throw new NullReferenceException("target is null");

                    if (labelData.target != target) {
                        labelData.target = target;

                        labelData.name = target.unitName;
                        labelData.code = target.definition.code.Replace("\n", " ");

                        var color = Color.grey;
                        if (target == GameBindings.Player.TargetList.GetActiveTarget())
                            color = ThemeManager.Active.ColorTheme.HudUnitSelected;
                        else {
                            var factionMode = GameBindings.GameState.GetFactionMode(target);
                            color = factionMode switch {
                                FactionMode.Friendly => ThemeManager.Active.ColorTheme.HudUnitFriendly,
                                FactionMode.Enemy => ThemeManager.Active.ColorTheme.HudUnitHostile,
                                _ => ThemeManager.Active.ColorTheme.HudUnitNeutral,
                            };
                        }

                        labelData.color = color;
                        labelData.label.SetColor(color);
                    }

                    strings["{name}"] = labelData.name.PadRight(nameLength);
                    strings["{code}"] = labelData.code.PadRight(codeLength);

                    if (aircraft != null) {
                        string distanceString = "";
                        if (SceneSingleton<DynamicMap>.i.HQ.TryGetKnownPosition(target, out var knownPosition)) {
                            float distance = FastMath.Distance(aircraft.GlobalPosition(), knownPosition);
                            distanceString = UnitConverter.DistanceReading(distance);
                            strings["{grid}"] = SceneSingleton<DynamicMap>.i.gridLabels.GetGridPosition(knownPosition);
                        }
                        else {
                            distanceString = "?";
                            strings["{grid}"] = "?";
                        }
                        strings["{distance}"] = distanceString.PadRight(6);
                    }
                    else {
                        strings["{distance}"] = "?";
                        strings["{grid}"] = "?";
                    }

                    strings["{lased}"] = hq == null ? "" : (hq.IsTargetLased(target) ? ((lasedTargets != null && lasedTargets.Contains(target)) ? "[L]" : " L ") : "   ");

                    float tti = -1f;
                    if (playerMissileDatum != null || playerMissileCache.Targets.TryGetValue(target, out playerMissileDatum)) {
                        foreach (var missileData in playerMissileDatum) {
                            if (tti == -1f || missileData.tti < tti)
                                tti = missileData.tti;
                        }
                    }

                    strings["{tti}"] =
                        tti < 0 ? "    " :
                        tti > 999f ? "999s" :
                        tti < FlashBeforeImpactTime ? $"{tti, 3:F1}s" :
                        $"{tti, 3:F0}s";

                    if (friendlyMissileDatum == null)
                        friendlyMissileCache.Targets.TryGetValue(target, out friendlyMissileDatum);
                    strings["{missiles}"] =
                        (friendlyMissileDatum == null || friendlyMissileDatum.Count == 0) ? "   " :
                        friendlyMissileDatum.Count > 10 ? "9+M" :
                        $"{friendlyMissileDatum.Count}M".PadRight(3);

                    string MakeString(string format) {
                        var sb = new StringBuilder (format);
                        foreach ((var parameter, var replacement) in strings)
                            sb.Replace(parameter, replacement);
                        return sb.ToString();
                    }

                    labelData.label.SetText(MakeString(EntryFormatString));

                    var flashing = FlashBeforeImpactTime > 0 && tti > 0 && tti < FlashBeforeImpactTime;
                    if (!flashing && labelData.flashing)
                        labelData.label.SetColor(labelData.color);
                    labelData.flashing = flashing;
                }

                foreach (var target in targets) {
                    if (target == null)
                        continue;

                    var targetLabelData = GetOrCreateLabelData();
                    if (targetLabelData == null)
                        break;

                    PopulateLabelData(targetLabelData, target);
                    targetLabelData.label.GetRectTransform().SetAsFirstSibling();
                }

                if (ShowMissileTargets) {
                    var drawMissileTargets = false;
                    foreach (var mt in playerMissileCache.Targets.Keys) {
                        if (!targets.Contains(mt)) {
                            drawMissileTargets = true;
                            break;
                        }
                    }

                    if (drawMissileTargets && i < MaxNumEntries - 1) {
                        var spacerLabelData = GetOrCreateLabelData();
                        if (spacerLabelData != null) {
                            spacerLabelData.label.SetColor(MissileTargetsLabelColor);
                            spacerLabelData.label.SetText("---MISSILE TARGETS---");
                            spacerLabelData.label.GetRectTransform().SetAsLastSibling();
                            spacerLabelData.flashing = false;

                            UpdateNameLength(playerMissileCache.Targets.Keys);
                            foreach ((var missileTarget, var missileDatum) in playerMissileCache.Targets) {
                                if (missileTarget == null || targets.Contains(missileTarget))
                                    continue;
                                var missileTargetLabelData = GetOrCreateLabelData();
                                if (missileTargetLabelData == null)
                                    break;
                                PopulateLabelData(missileTargetLabelData, missileTarget, missileDatum);
                                missileTargetLabelData.label.GetRectTransform().SetAsLastSibling();
                            }
                        }
                    }
                }

                if (i < labelDatum.Count) {
                    for (int j = i; j < labelDatum.Count; j++) {
                        labelDatum[j].label.Destroy();
                    }
                    labelDatum.RemoveRange(i, labelDatum.Count - i);
                }
            } //if (Time.timeSinceLevelLoad >= lastUpdate + UpdatePeriod)

            foreach (var labelData in labelDatum) {
                if (labelData.flashing) {
                    var color = Color.Lerp(labelData.color, FlashColor, Mathf.Sin(Time.timeSinceLevelLoad * 20f) + 0.5f);
                    labelData.label.SetColor(color);
                }
            }
        }
    }

    private static bool initialized = false;
    private static MissileCache<MissileData> friendlyMissileCache = null;
    private static MissileCache<PlayerMissileData> playerMissileCache = null;
    private static float lastUpdate = 0f;
    private static CombatHUD combatHUD = null;
    private static GameObject container = null;
    private class LabelData {
        public UIBindings.Draw.UILabel label;
        public Unit target;
        public string name;
        public string code;
        public Color color = Color.white;
        public bool flashing = false;
    }
    private static List<LabelData> labelDatum = new ();
    private static Dictionary<string, string> strings = new ();
}
