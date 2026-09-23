using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using NO_Tactitools.Core;
using NO_Tactitools.Controls;
using NO_Tactitools.UI.HMD;
using NO_Tactitools.UI.MFD;

namespace NO_Tactitools.Controls;

public class MultiLevelMenuComponent {
    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class OnMainMenuStart {
        static void Postfix() {
            if (initialized)
                return;

            Plugin.Log($"[MLM] Initializing MultiLevelMenu component");

            MultiLevelMenu.Init();
            MultiLevelMenuAction.Init();

            int Group1ToInt(Match m) {
                return int.Parse(m.Groups[1].Value);
            }

            StringAndMakers[] stringAndMakers = new StringAndMakers[] {
                new (@"RememberHUDOptions\((\d+)\)",
                    (Match m) => { var i = Group1ToInt(m); return () => HUDOptionsPresetComponent.Remember(i); },
                    null,
                    null,
                    (Match m) => $"Remember HUD Options {m.Groups[1].Value}",
                    (Match m) => $"RmH{m.Groups[1].Value}"),
                new (@"RecallHUDOptions\((\d+)\)",
                    (Match m) => { var i = Group1ToInt(m); return () => HUDOptionsPresetComponent.Recall(i); },
                    (Match m) => { var i = Group1ToInt(m); return () => { if (ShowPreview) HUDOptionsPresetComponent.Preview(i); }; },
                    null,
                    (Match m) => $"Recall HUD Options {m.Groups[1].Value}",
                    (Match m) => $"RcH{m.Groups[1].Value}"),

                new (@"RememberFilter\((\d+)\)",
                    (Match m) => { var i = Group1ToInt(m); return () => TargetFilterPresetComponent.Remember(i); },
                    null,
                    null,
                    (Match m) => $"Remember Filter {m.Groups[1].Value}",
                    (Match m) => $"RmF{m.Groups[1].Value}"),
                new (@"RecallFilter\((\d+)\)",
                    (Match m) => { var i = Group1ToInt(m); return () => TargetFilterPresetComponent.Recall(i); },
                    (Match m) => { var i = Group1ToInt(m); return () => { if (ShowPreview) TargetFilterPresetComponent.Preview(i); }; },
                    null,
                    (Match m) => $"Recall Filter {m.Groups[1].Value}",
                    (Match m) => $"RcF{m.Groups[1].Value}"),

                new (@"RememberTargets\((\d+)\)",
                    (Match m) => { var i = Group1ToInt(m); return () => TargetListControllerPlugin.RememberTargets(i); },
                    null,
                    null,
                    (Match m) => $"Remember Targets {m.Groups[1].Value}",
                    (Match m) => $"RmT{m.Groups[1].Value}"),
                new (@"RecallTargets\((\d+)\)",
                    (Match m) => { var i = Group1ToInt(m); return () => TargetListControllerPlugin.RecallTargets(i); },
                    (Match m) => { var i = Group1ToInt(m); return () => { if (ShowPreview) TargetListControllerPlugin.PreviewTargets(i); }; },
                    null,
                    (Match m) => $"Recall Targets {m.Groups[1].Value}",
                    (Match m) => $"RcT{m.Groups[1].Value}"),
                new (@"PopTarget", TargetListControllerPlugin.PopCurrentTarget, null, null, "Pop Target", "PT"),
                new (@"KeepTarget", TargetListControllerPlugin.KeepOnlyCurrentTarget, null, null, "Keep Target", "KT"),
                new (@"NextTarget", TargetListControllerPlugin.NextTarget, null, null, "Next Target", "NextT"),
                new (@"PrevTarget", TargetListControllerPlugin.PreviousTarget, null, null, "Prev Target", "PrevT"),
                new (@"KeepDatalinked", TargetListControllerPlugin.KeepOnlyDataLinkedTargets, null, null, "Keep data linked", "KD"),
                new (@"KeepByAmmo", TargetListControllerPlugin.KeepClosestTargetsBasedOnAmmo, null, null, "Keep by ammo", "KA"),
                new (@"SortName", TargetListControllerPlugin.SortTargetsByName, null, null, "Sort by name", "SN"),
                new (@"SortDist", TargetListControllerPlugin.SortTargetsByDistance, null, null, "Sort by distance", "SD"),
                new (@"KeepTracked", TargetListControllerPlugin.KeepTrackedTargets, null, null, "Keep tracked", "KTr"),
                new (@"PopTracked", TargetListControllerPlugin.KeepUntrackedTargets, null, null, "Pop tracked", "PTr"),
                new (@"KeepSameName", () => TargetListControllerPlugin.SelectTargetsByUnitName(asCurrentTarget: true), null, null, "Keep with same name", "KN"),
                new (@"PopSameName", () => TargetListControllerPlugin.SelectTargetsByUnitName(asCurrentTarget: false), null, null, "Pop with same name", "PN"),
                new (@"KeepLased", () => TargetListControllerPlugin.SelectTargetsByLasedStatus(lased: true), null, null, "Keep lased", "KL"),
                new (@"PopLased", () => TargetListControllerPlugin.SelectTargetsByLasedStatus(lased: false), null, null, "Pop lased", "PL"),
                new (@"SelectClosestTarget", () => TargetListControllerPlugin.SelectClosestTargets(oneTarget: true), null, null, "Select closest target", "SC"),
                new (@"SelectClosestTargets", () => TargetListControllerPlugin.SelectClosestTargets(oneTarget: false), null, null, "Select closest targets", "SCs"),
                new (@"MTSToggle", TargetListControllerPlugin.MtsToggle, null, null, "MTS Toggle", "MTST"),
                new (@"MTSAutoSelect", () => TargetListControllerPlugin.MtsSetAutoSelectMode(null), null, null, "MTS Auto Select", "MTSA"),

                new (@"MiniMapZoomUp", () => MiniMapZoomComponent.CycleZoom(up: true), null, null, "MiniMap Zoom Up", "MMZU"),
                new (@"MiniMapZoomDown", () => MiniMapZoomComponent.CycleZoom(up: false), null, null, "MiniMap Zoom Down", "MMZD"),
                new (@"MiniMapZoomReset", MiniMapZoomComponent.ResetZoom, null, null, "MiniMap Zoom Reset", "MMZR"),

                new (@"HMDMarkerDistanceUp", () => HMDDeclutterComponent.CycleDistance(up: true), null, null, "HMD Marker Distance Up", "HMDU"),
                new (@"HMDMarkerDistanceDown", () => HMDDeclutterComponent.CycleDistance(up: false), null, null, "HMD Marker Distance Down", "HMDD"),

                new (@"TargetCamModeToggle", () => TargetCamModePlugin.State = !TargetCamModePlugin.State, null, null, "Target Cam Mode Toggle", "TCMT"),
            };

            foreach (var sm in stringAndMakers) {
                var i = sm.str.IndexOf(@"\(");
                var s = i == -1 ? sm.str : sm.str.Substring(0, i);
                ActionMaker maker = i == -1 ? new StringActionMaker (sm) : new RegexActionMaker (sm);
                actionMakers[s] = maker;
            }

            var bindings = new List<BindingHelper.Binding> ();

            var levelsMainChangedCallback = (object obj) => { levelsMainChanged = true; };
            for (int i = 0; i < Plugin.MultiLevelMenu.NumMain.Value; ++i) {
                var cfg = Plugin.MultiLevelMenu.Main[i];
                levelsMain.Add("");
                bindings.Add(
                    new BindingHelper.Binding (
                        typeof(MultiLevelMenuComponent),
                        "levelsMain",
                        i,
                        Plugin.MultiLevelMenu.Main[i],
                        settingChangedCallback: levelsMainChangedCallback));
            }

            var levelsWeaponsChangedCallback = (object obj) => { levelsWeaponsChanged = true; };
            for (int i = 0; i < Plugin.MultiLevelMenu.NumWeapons.Value; ++i) {
                var cfg = Plugin.MultiLevelMenu.Weapons[i];
                levelsWeapons.Add("");
                bindings.Add(
                    new BindingHelper.Binding (
                        typeof(MultiLevelMenuComponent),
                        "levelsWeapons",
                        i,
                        Plugin.MultiLevelMenu.Weapons[i],
                        settingChangedCallback: levelsWeaponsChangedCallback));
            }

            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenuComponent), "ShowPreview", Plugin.MultiLevelMenu.ShowPreview));
            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenu), "MenuDelay", Plugin.MultiLevelMenu.MenuDelay));
            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenu), "ZPerLevel", Plugin.MultiLevelMenu.ZPerLevel));
            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenu), "ResetLevel", Plugin.MultiLevelMenu.ResetLevel));
            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenu), "OpenOnPress", Plugin.MultiLevelMenu.OpenOnPress));
            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenu), "Toggle", Plugin.MultiLevelMenu.Toggle));
            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenu), "SwitchMode", Plugin.MultiLevelMenu.SwitchMode));
            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenu), "LevelSwitchPeriod", Plugin.MultiLevelMenu.LevelSwitchPeriod));
            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenuAction), "DefaultColor", Plugin.MultiLevelMenu.DefaultColor));
            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenuAction), "SelectedColor", Plugin.MultiLevelMenu.SelectedColor));
            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenuAction), "BackgroundColorActive", Plugin.MultiLevelMenu.BackgroundColorActive));
            bindings.Add(new BindingHelper.Binding (typeof(MultiLevelMenuAction), "BackgroundColorInactive", Plugin.MultiLevelMenu.BackgroundColorInactive));

            BindingHelper.ApplyBindings(bindings);

            Plugin.harmony.PatchAll(typeof(OnRadialMenuMainOpenMenu));

            initialized = true;

            Plugin.Log($"[MLM] Initialized MultiLevelMenu component");
        }
    }

    public static bool ShowPreview = true;

    private static bool initialized = false;

    private static List<string> levelsMain = new List<string> ();
    private static List<string> levelsWeapons = new List<string> ();
    private static bool levelsMainChanged = true;
    private static bool levelsWeaponsChanged = true;

    class StringAndMakers {
        public string str;
        public Func<Match, Action> triggerCommandMaker;
        public Func<Match, string> displayNameMaker;
        public Func<Match, string> ammoTextMaker;
        public Func<Match, Action> hoverCommandMaker;
        public Func<Match, Action> unhoverCommandMaker;

        public StringAndMakers(
            string str, Func<Match,
            Action> triggerCommandMaker, Func<Match, Action> hoverCommandMaker, Func<Match, Action> unhoverCommandMaker,
            Func<Match, string> displayNameMaker, Func<Match, string> ammoTextMaker)
        {
            this.str = str;
            this.triggerCommandMaker = triggerCommandMaker;
            this.hoverCommandMaker = hoverCommandMaker;
            this.unhoverCommandMaker = unhoverCommandMaker;
            this.displayNameMaker = displayNameMaker;
            this.ammoTextMaker = ammoTextMaker;
        }

        public StringAndMakers(string str, Action triggerCommand, Action hoverCommand, Action unhoverCommand, string displayName, string ammoText) {
            this.str = str;
            this.triggerCommandMaker = (Match m) => triggerCommand;
            this.hoverCommandMaker = (Match m) => hoverCommand;
            this.unhoverCommandMaker = (Match m) => unhoverCommand;
            this.displayNameMaker = (Match m) => displayName;
            this.ammoTextMaker = (Match m) => ammoText;
        }
    }

    abstract class ActionMaker {
        public abstract RadialMenuAction MakeAction(string str);
    }

    class StringActionMaker : ActionMaker {
        public override RadialMenuAction MakeAction(string str) {
            if (str != sm.str)
                return null;
            var action = CallbackMultiLevelMenuAction.Create(sm.triggerCommandMaker(null), sm.displayNameMaker(null), sm.ammoTextMaker(null));
            if (sm.hoverCommandMaker != null)
                action.OnHover += sm.hoverCommandMaker(null);
            if (sm.unhoverCommandMaker != null)
                action.OnUnHover += sm.unhoverCommandMaker(null);
            return action;
        }

        public StringActionMaker(StringAndMakers sm) {
            this.sm = sm;
        }

        protected StringAndMakers sm;
    }

    class RegexActionMaker : StringActionMaker {
        public override RadialMenuAction MakeAction(string str) {
            var match = regex.Match(str);
            if (!match.Success)
                return null;
            var action = CallbackMultiLevelMenuAction.Create(sm.triggerCommandMaker(match), sm.displayNameMaker(match), sm.ammoTextMaker(match));
            if (sm.hoverCommandMaker != null)
                action.OnHover += sm.hoverCommandMaker(match);
            if (sm.unhoverCommandMaker != null)
                action.OnUnHover += sm.unhoverCommandMaker(match);
            return action;
        }

        public RegexActionMaker(StringAndMakers sm) : base(sm) {
            this.regex = new Regex(sm.str);
        }

        private Regex regex;
    }

    private static List<List<RadialMenuAction>> MakeActionLayers(List<string> stringLayers) {
        List<List<RadialMenuAction>> actionLayers = new ();
        foreach (string stringLayer in stringLayers) {
            string[] stringLayerCommands = stringLayer.Split(";");
            if (stringLayerCommands.Length > 0) {
                List<RadialMenuAction> actionLayer = null;
                if (stringLayerCommands[0] != "Default") {
                    actionLayer = new ();
                    foreach (string stringLayerCommand in stringLayerCommands) {
                        if (stringLayerCommand.Length == 0)
                            continue;
                        var i = stringLayerCommand.IndexOf(@"(");
                        var s = i == -1 ? stringLayerCommand : stringLayerCommand.Substring(0, i);
                        if (actionMakers.TryGetValue(s, out var maker)) {
                            var action = maker.MakeAction(stringLayerCommand);
                            if (action != null) {
                                actionLayer.Add(action);
                            }
                            else
                                Plugin.Log($"[MLM] Cannot make action out of {stringLayerCommand}");
                        }
                        else
                            Plugin.Log($"[MLM] Cannot find action maker for {stringLayerCommand}");
                    }
                }
                actionLayers.Add(actionLayer);
            }
        }
        return actionLayers;
    }

    private static Dictionary<string, ActionMaker> actionMakers = new ();

    [HarmonyPatch(typeof(RadialMenuMain), "OpenMenu")]
    public class OnRadialMenuMainOpenMenu {
        public static void Prefix() {
            if (levelsMainChanged) {
                var actions = MakeActionLayers(levelsMain);
                MultiLevelMenu.SetActionsLayers(actions, RadialMenuMain.RadialMenuType.Main);
                Plugin.Log($"[MLM] Main actions have changed; made {actions.Count} action levels from {levelsMain.Count} string levels");
                levelsMainChanged = false;
            }
            if (levelsWeaponsChanged) {
                var actions = MakeActionLayers(levelsWeapons);
                MultiLevelMenu.SetActionsLayers(actions, RadialMenuMain.RadialMenuType.Weapons);
                Plugin.Log($"[MLM] Weapons actions have changed; made {actions.Count} action levels from {levelsWeapons.Count} string levels");
                levelsWeaponsChanged = false;
            }
        }
    }
}

public class MultiLevelMenu {
    public static float MenuDelay = 1.0f;
    public static float ZPerLevel = 0.0f;
    public static bool ResetLevel = true;
    public static bool OpenOnPress = true;
    public static bool Toggle = false;
    public enum SwitchModes { Step, Continuous };
    public static SwitchModes SwitchMode {
        get;
        set { field = value; z = 0.0f; }
    } = SwitchModes.Step;
    public static float LevelSwitchPeriod = 0.25f;

    public static void SetActionsLayers(List<List<RadialMenuAction>> actionLayers, RadialMenuMain.RadialMenuType radialMenuType = RadialMenuMain.RadialMenuType.Main) {
        var md = menuDatum[(int)radialMenuType];
        md.actionLayers = actionLayers;
        md.needsUpdate = NeedsUpdate.Partial;
    }

    public static void Init() {
        if (!initialized) {
            Plugin.harmony.PatchAll(typeof(OnRadialMenuMainOpenMenu));
            Plugin.harmony.PatchAll(typeof(OnRadialMenuMainCloseMenu));
            Plugin.harmony.PatchAll(typeof(OnRadialMenuMainUpdate));
            Plugin.harmony.PatchAll(typeof(OnRadialMenuMainCheckAction));
            Plugin.harmony.PatchAll(typeof(OnPlayerGetAxis));
            Plugin.harmony.PatchAll(typeof(OnPlayerGetButton));
            Plugin.harmony.PatchAll(typeof(OnPlayerGetButtonTimedPressDown));

            initialized = true;
        }
    }

    private class MenuLayer {
        public List<RadialMenuAction> actions = new ();
        public List<GameObject> objects = new ();
        public float degreesPerAction = 0f;
    }

    /* Sorts child gameobjects so that background sectors (which don't have "Text" child)
       move to beginning of the list, and will be reparented (and, subsequently, drawn) first. */
    private static void SortChildren(List<GameObject> children) {
        int HasTextChild(GameObject obj) {
            return obj.transform.Find("Text") != null ? 1 : 0;
        }
        int CompareChildren(GameObject a, GameObject b) {
            return HasTextChild(a) - HasTextChild(b);
        }
        children.Sort(CompareChildren);
    }

    private static MenuLayer CreateMenuLayerWorker(List<RadialMenuAction> actions, RadialMenuMain radialMenu, RadialMenuMain.RadialMenuType radialMenuType = RadialMenuMain.RadialMenuType.Main) {
        var acc = accessors[(int)radialMenuType];

        RadialMenuAction[] actionsArray = new RadialMenuAction [actions.Count];
        actions.CopyTo(actionsArray);
        acc.actionsInfo.SetValue(radialMenu, actionsArray);

        var actionObjects = (List<GameObject>)acc.actionObjectsInfo.GetValue(radialMenu);
        actionObjects.Clear(); //to prevent RadialMenuMain.Setup...() destroying previously created objects

        acc.setupInfo.Invoke(radialMenu, null);

        var allowedActions = (List<RadialMenuAction>)acc.allowedActionsInfo.GetValue(radialMenu);
        var allowedActionsCopy = new List<RadialMenuAction> (allowedActions);

        var actionObjectsCopy = new List<GameObject> (actionObjects);
        actionObjects.Clear(); //to prevent RadialMenuMain.Setup...() destroying created objects

        var degreesPerAction = (float)acc.degreesPerActionInfo.GetValue(radialMenu);

        MenuLayer menuLayer = new MenuLayer {
            actions = allowedActionsCopy,
            objects = actionObjectsCopy,
            degreesPerAction = degreesPerAction
        };

        return menuLayer;
    }

    private static MenuLayer CreateMenuLayer(List<RadialMenuAction> actions, RadialMenuMain.RadialMenuType radialMenuType = RadialMenuMain.RadialMenuType.Main) {
        var radialMenu = SceneSingleton<RadialMenuMain>.i;
        if (radialMenu == null)
            return new MenuLayer ();

        //Using SetupMain() to create actionObjects, because SetupWeapons() creates actions for weapons, not needed here
        var accessorsMain = accessors[(int)RadialMenuMain.RadialMenuType.Main];

        RadialMenuAction[] oldActions = (RadialMenuAction[])((RadialMenuAction[])accessorsMain.actionsInfo.GetValue(radialMenu)).Clone();
        List<RadialMenuAction> oldAllowedActions = new ((List<RadialMenuAction>)accessorsMain.allowedActionsInfo.GetValue(radialMenu));
        List<GameObject> oldObjects = new ((List<GameObject>)accessorsMain.actionObjectsInfo.GetValue(radialMenu));
        var degreesPerAction = (float)accessorsMain.degreesPerActionInfo.GetValue(radialMenu);

        MenuLayer menuLayer = CreateMenuLayerWorker(actions, radialMenu, radialMenuType);

        if (menuLayer != null) {
            var accessorsActual = accessors[(int)radialMenuType];
            var container = (GameObject)accessorsActual.containerInfo.GetValue(radialMenu);
            /* Sorting ensures correct draw order. */
            SortChildren(menuLayer.objects);
            foreach (var actionObject in menuLayer.objects) {
                actionObject.transform.SetParent(container.transform);
            }
        }

        accessorsMain.degreesPerActionInfo.SetValue(radialMenu, degreesPerAction);
        accessorsMain.actionObjectsInfo.SetValue(radialMenu, oldObjects);
        accessorsMain.allowedActionsInfo.SetValue(radialMenu, oldAllowedActions);
        accessorsMain.actionsInfo.SetValue(radialMenu, oldActions);
        
        return menuLayer;
    }

    private static List<MenuLayer> CreateMenuLayers(List<List<RadialMenuAction>> actionLayers, RadialMenuMain.RadialMenuType radialMenuType = RadialMenuMain.RadialMenuType.Main) {
        var radialMenu = SceneSingleton<RadialMenuMain>.i;
        if (radialMenu == null)
            return new List<MenuLayer> ();

        //Using SetupMain() to create actionObjects
        var accessorsMain = accessors[(int)RadialMenuMain.RadialMenuType.Main];

        var actions = (RadialMenuAction[])accessorsMain.actionsInfo.GetValue(radialMenu);
        var actionsCopy = (RadialMenuAction[])actions.Clone();

        var allowedActions = (List<RadialMenuAction>)accessorsMain.allowedActionsInfo.GetValue(radialMenu);
        var allowedActionsCopy = new List<RadialMenuAction> (allowedActions);

        var actionObjects = (List<GameObject>)accessorsMain.actionObjectsInfo.GetValue(radialMenu);
        var actionObjectsCopy = new List<GameObject> (actionObjects);
        actionObjects.Clear(); //to prevent RadialMenuMain.Setup...() destroying saved objects

        var degreesPerAction = (float)accessorsMain.degreesPerActionInfo.GetValue(radialMenu);

        var accessorsActual = accessors[(int)radialMenuType];
        var container = (GameObject)accessorsActual.containerInfo.GetValue(radialMenu);

        List<MenuLayer> result = new ();

        foreach (var actionLayer in actionLayers) {
            MenuLayer menuLayer = null;
            if (actionLayer == null) {
                /* noop */
            }
            else if (actionLayer.Count == 0) {
                continue;
            }
            else {
                menuLayer = CreateMenuLayerWorker(actionLayer, radialMenu, RadialMenuMain.RadialMenuType.Main);
            }

            result.Add(menuLayer);

            if (menuLayer != null) {
                /* Sorting ensures correct draw order. */
                SortChildren(menuLayer.objects);
                foreach (var actionObject in menuLayer.objects) {
                    actionObject.transform.SetParent(container.transform);
                }
            }
        }

        accessorsMain.degreesPerActionInfo.SetValue(radialMenu, degreesPerAction);
        accessorsMain.actionObjectsInfo.SetValue(radialMenu, actionObjectsCopy);
        accessorsMain.allowedActionsInfo.SetValue(radialMenu, allowedActionsCopy);
        accessorsMain.actionsInfo.SetValue(radialMenu, actionsCopy);

        return result;
    }

    private static void DestroyMenuLayers(MenuData menuData) {
        if (menuData == null || menuData.menuLayers == null) {
            Plugin.Log($"[MLM] MultiLevelMenu.DestroyMenuLayers(): menuData or menuLayers is null, returning");
            return;
        }

        for (int i = 0; i < menuData.menuLayers.Count; ++i) {
            //default menu is preserved on partial reinit
            if (menuData.needsUpdate == NeedsUpdate.Partial && i == menuData.defaultLevel) {
                continue;
            }
            var menuLayer = menuData.menuLayers[i];
            foreach (var obj in menuLayer.objects) {
                if (obj != null)
                    UnityEngine.Object.Destroy(obj);
            }
        }

    }

    private static MenuLayer GetMenuLayer(RadialMenuMain.RadialMenuType radialMenuType = RadialMenuMain.RadialMenuType.Main) {
        var radialMenu = SceneSingleton<RadialMenuMain>.i;
        if (radialMenu == null)
            return new MenuLayer ();

        var acc = accessors[(int)radialMenuType];

        MenuLayer menuLayer= new MenuLayer {
            actions = new List<RadialMenuAction> ((List<RadialMenuAction>)acc.allowedActionsInfo.GetValue(radialMenu)),
            objects = new List<GameObject> ((List<GameObject>)acc.actionObjectsInfo.GetValue(radialMenu)),
            degreesPerAction = (float)acc.degreesPerActionInfo.GetValue(radialMenu)
        };

        //Plugin.Log($"[MLM] GetMenuLayer(): radialMenuType:{radialMenuType}; actions:{menuLayer.actions.Count}; objects:{menuLayer.objects.Count}; degreesPerAction:{menuLayer.degreesPerAction}");

        return menuLayer;
    }

    private static void SetMenuLayer(MenuLayer menuLayer, RadialMenuMain.RadialMenuType radialMenuType = RadialMenuMain.RadialMenuType.Main) {
        var radialMenu = SceneSingleton<RadialMenuMain>.i;
        if (radialMenu == null)
            return;

        var acc = accessors[(int)radialMenuType];

        acc.actionObjectsInfo.SetValue(radialMenu, new List<GameObject> (menuLayer.objects));
        acc.allowedActionsInfo.SetValue(radialMenu, new List<RadialMenuAction> (menuLayer.actions));
        acc.degreesPerActionInfo.SetValue(radialMenu, menuLayer.degreesPerAction);
    }

    private enum NeedsUpdate { None, Partial, Full };

    private class MenuData {
        public RadialMenuMain.RadialMenuType radialMenuType;
        public int level;
        public int defaultLevel;
        public List<List<RadialMenuAction>> actionLayers;
        public List<MenuLayer> menuLayers;
        public NeedsUpdate needsUpdate = NeedsUpdate.Full;

        public MenuData(RadialMenuMain.RadialMenuType radialMenuType) {
            this.radialMenuType = radialMenuType;
        }
    }

    private static MenuData[] menuDatum = new MenuData [3] {
        null,
        new (RadialMenuMain.RadialMenuType.Main),
        new (RadialMenuMain.RadialMenuType.Weapons)
    };
    
    private static void InitMenuData(MenuData menuData) {
        var menuLayers = menuData.menuLayers;
        var defaultMenuLayer = (menuData.needsUpdate == NeedsUpdate.Partial && menuLayers != null && menuLayers.Count > 0) ? menuLayers[menuData.defaultLevel] : null;

        DestroyMenuLayers(menuData); //should accept null

        if (menuData.actionLayers != null) {
            menuData.menuLayers = CreateMenuLayers(menuData.actionLayers, menuData.radialMenuType);            
        }

        if (menuData.menuLayers == null) {
            Plugin.Log($"[MLM] MultiLevelMenu.InitMenuData(): menu layers for {menuData.radialMenuType} is null");
            return;
        }

        menuData.defaultLevel = -1;

        for (int i = 0; i < menuData.menuLayers.Count; ++i) {
            var menuLayer = menuData.menuLayers[i];
            if (menuLayer == null) {
                if (menuData.defaultLevel != -1)
                    throw new Exception ("only one default menu enrty is allowed");
                else {
                    menuData.defaultLevel = i;
                }
            }
            else {
                SetObjectsActive(menuLayer.objects, false);
            }
        }

        if (menuData.defaultLevel == -1) {
            menuData.defaultLevel = 0;
            menuData.menuLayers.Insert(0, null);
        }

        menuData.level = menuData.defaultLevel;

        if (defaultMenuLayer != null)  {
            menuData.menuLayers[menuData.defaultLevel] = defaultMenuLayer;
            SetMenuLayer(defaultMenuLayer, menuData.radialMenuType);
        }
        else
            menuData.menuLayers[menuData.defaultLevel] = GetMenuLayer(menuData.radialMenuType);
    }

    private static void SetObjectsActive(List<GameObject> objects, bool active) {
        foreach (var obj in objects)
            obj.SetActive(active);
    }

    [HarmonyPatch(typeof(RadialMenuMain), "OpenMenu")]
    public class OnRadialMenuMainOpenMenu {
        public static void Postfix(ref RadialMenuMain.RadialMenuType ___currentState, ref GameObject ___actionsMainContainer, ref GameObject ___actionsWeaponsContainer, ref bool ___showWeaponWheel) {
            var radialMenuType = ___currentState;

            if (radialMenuType == RadialMenuMain.RadialMenuType.Closed)
                return;

            GameManager.GetLocalAircraft(out var localAircraft);
            if (aircraft != localAircraft) {
                Plugin.Log($"[MLM] MultiLevelMenu.OpenMenu(): aircraft has changed");
                aircraft = localAircraft;
                menuDatum[(int)RadialMenuMain.RadialMenuType.Main].needsUpdate = NeedsUpdate.Full;
                menuDatum[(int)RadialMenuMain.RadialMenuType.Weapons].needsUpdate = NeedsUpdate.Full;
                ___showWeaponWheel = true;
            }

            var mdMain = menuDatum[(int)RadialMenuMain.RadialMenuType.Main];
            if (mdMain.needsUpdate != NeedsUpdate.None) {
                Plugin.Log($"[MLM] MultiLevelMenu.OpenMenu(): initializing Main menu data");
                InitMenuData(mdMain);
                mdMain.needsUpdate = NeedsUpdate.None;
            }
            var mdWeapons = menuDatum[(int)RadialMenuMain.RadialMenuType.Weapons];
            if (mdWeapons.needsUpdate != NeedsUpdate.None) {
                Plugin.Log($"[MLM] MultiLevelMenu.OpenMenu(): initializing Weapons menu data");
                InitMenuData(mdWeapons);
                mdWeapons.needsUpdate = NeedsUpdate.None;
            }

            var menuData = menuDatum[(int)radialMenuType];

            //In case default menu has changed
            if (menuData.level == menuData.defaultLevel) {
                menuData.menuLayers[menuData.level] = GetMenuLayer(radialMenuType);
            }
            else if (ResetLevel) {
                menuData.level = menuData.defaultLevel;
            }

            var currentMenuLayer = menuData.menuLayers[menuData.level];
            SetMenuLayer(currentMenuLayer, radialMenuType);
            SetObjectsActive(currentMenuLayer.objects, true);

            z = 0;
        }
    }

    [HarmonyPatch(typeof(RadialMenuMain), "CloseMenu")]
    public class OnRadialMenuMainCloseMenu {
        public static void Prefix(ref RadialMenuMain.RadialMenuType ___currentState) {
            Plugin.Log("[MLM] MultiLevelMenu: closing menu");

            var radialMenuType = ___currentState;

            if (radialMenuType == RadialMenuMain.RadialMenuType.Closed)
                return;

            var menuData = menuDatum[(int)radialMenuType];

            var currentMenuLayer = GetMenuLayer(radialMenuType);

            SetObjectsActive(currentMenuLayer.objects, false);

            //In case default menu has changed
            if (menuData.level == menuData.defaultLevel) {
                menuData.menuLayers[menuData.level] = currentMenuLayer;
            }
            else if (ResetLevel) {
                Plugin.Log($"[MLM] MultiLevelMenu.CloseMenu(): restoring default menu items");
                var defaultMenuLayer = menuData.menuLayers[menuData.defaultLevel];
                SetObjectsActive(defaultMenuLayer.objects, false);
                SetMenuLayer(defaultMenuLayer, radialMenuType);
                menuData.level = menuData.defaultLevel;
            }
        }
    }

    [HarmonyPatch(typeof(RadialMenuMain), "Update")]
    public class OnRadialMenuMainUpdate {
        public static void Prefix() {
            inRadialMenuMainUpdate = true;
        }

        public static void Postfix(
            RadialMenuMain __instance, Player ___playerInput, ref RadialMenuMain.RadialMenuType ___currentState, ref Vector3 ___mouseDelta,
            ref List<RadialMenuAction> ___allowedActionsMain, ref List<GameObject> ___actionObjectsMain,
            ref List<RadialMenuAction> ___allowedActionsWeapons, ref List<GameObject> ___actionObjectsWeapons) {

            var radialMenu = __instance;
            var playerInput = ___playerInput;
            var menuData = menuDatum[(int)___currentState];
            if (menuData == null)
                return;
            var newLevel = menuData.level;

            var buttonAction = ___currentState == RadialMenuMain.RadialMenuType.Main ? "Radial Menu" : "Weapon Wheel";
            if (playerInput.GetButtonDoublePressDown(buttonAction, PlayerSettings.pressDelay)) {
                menuData.level = -1;
            }

            if (___currentState == RadialMenuMain.RadialMenuType.Closed)
                return;

            if (menuData.level == -1) {
                newLevel = menuData.defaultLevel;
            }
            else {
                var currentTime = Time.realtimeSinceStartup;
                if (currentTime - lastLevelSwitch > LevelSwitchPeriod) {
                    var newZ = playerInput.GetAxis("Zoom View");
                    switch (SwitchMode) {
                        case SwitchModes.Continuous:
                            if (Mathf.Sign(newZ) != Mathf.Sign(z))
                                z = 0.0f;
                            z += newZ;
                            if (Mathf.Abs(z) > ZPerLevel) {
                                newLevel += (int)Mathf.Sign(z);
                                z = 0.0f;
                                lastLevelSwitch = currentTime;
                            }
                            break;
                        case SwitchModes.Step:
                            if (newZ != 0 && z == 0) {
                                newLevel += (newZ > 0 ? 1 : -1);
                                lastLevelSwitch = currentTime;
                            }
                            z = newZ;
                            break;
                        default:
                            throw new Exception ($"Invalid switch mode: {SwitchMode}");
                    }
                }
            }

            if (newLevel != menuData.level && newLevel >= 0 && newLevel < menuData.menuLayers.Count) {
                ___mouseDelta = Vector3.zero;
                menuData.level = newLevel;

                var actionObjects = ___currentState == RadialMenuMain.RadialMenuType.Main ? ___actionObjectsMain : ___actionObjectsWeapons;
                var allowedActions = ___currentState == RadialMenuMain.RadialMenuType.Main ? ___allowedActionsMain : ___allowedActionsWeapons;
                SetObjectsActive(actionObjects, false);
                foreach (var action in allowedActions) {
                    if (action is MultiLevelMenuAction multiLevelMenuAction)
                        multiLevelMenuAction.Disable();
                }

                var menuLayer = menuData.menuLayers[menuData.level];
                SetMenuLayer(menuLayer, ___currentState);

                actionObjects = ___currentState == RadialMenuMain.RadialMenuType.Main ? ___actionObjectsMain : ___actionObjectsWeapons;
                allowedActions = ___currentState == RadialMenuMain.RadialMenuType.Main ? ___allowedActionsMain : ___allowedActionsWeapons;
                SetObjectsActive(actionObjects, true);
                foreach (var action in allowedActions) {
                    if (action is MultiLevelMenuAction multiLevelMenuAction)
                        multiLevelMenuAction.Enable();
                }

                //Plugin.Log($"[MLM] MultiLevelMenu.Update(): set level to {menuData.level}");
            }
        }

        public static void Finalizer() {
            inRadialMenuMainUpdate = false;
        }
    }

    [HarmonyPatch(typeof(RadialMenuMain), "CheckAction")]
    public class OnRadialMenuMainCheckAction {
        public static void Postfix(ref float ___lastSelection, ref RadialMenuAction ___selectedAction) {
            if (lastSelection == ___lastSelection) {
                if (updateLastSelection) {
                    ___lastSelection += MenuDelay - 0.05f;
                    lastSelection = ___lastSelection;
                    updateLastSelection = false;
                }
            }
            else {
                lastSelection = ___lastSelection;
                updateLastSelection = true;
            }
        }
    }

    [HarmonyAfter(["yessidor.no_tactitools_plus.key_axes"])]
    [HarmonyPatch(typeof(Player), "GetAxis", typeof(string))]
    public class OnPlayerGetAxis {
        public static void Postfix(ref float __result, ref string actionName) {
            if (!DynamicMap.mapMaximized && SceneSingleton<RadialMenuMain>.i != null && RadialMenuMain.IsInUse() && !inRadialMenuMainUpdate && actionName == "Zoom View") {
                __result = 0;
            }
        }
    }

    [HarmonyPatch(typeof(Player), "GetButtonTimedPressDown", typeof(string), typeof(float))]
    public class OnPlayerGetButtonTimedPressDown {
        public static void Postfix(Player __instance, ref bool __result, ref string actionName) {
            if (OpenOnPress && inRadialMenuMainUpdate && (actionName == "Radial Menu" || actionName == "Weapon Wheel")) {
                __result = __instance.GetButtonDown(actionName);
            }
            if (Toggle && __result) {
                prevButtonState = true;
            }
        }
    }

    [HarmonyPatch(typeof(Player), "GetButton", typeof(string))]
    public class OnPlayerGetButton {
        public static void Postfix(Player __instance, ref bool __result, ref string actionName) {
            if (Toggle && inRadialMenuMainUpdate && (actionName == "Radial Menu" || actionName == "Weapon Wheel")) {
                /* Cannot call Player.GetButtonDown() from GetButton(), because the former always returns false. So have to emulate it.
                   result == false when button has just been pressed. NB: prevButtonState || !result is less obvious. */
                var result = !(!prevButtonState && __result);
                prevButtonState = __result;
                __result = result;
            }
        }
    }

    private class Accessors {
        //RadialMenuAction[]
        public FieldInfo actionsInfo;
        //List<RadialMenuAction>
        public FieldInfo allowedActionsInfo;
        //List<GameObject>
        public FieldInfo actionObjectsInfo;
        //GameObject
        public FieldInfo containerInfo;
        //float
        public FieldInfo degreesPerActionInfo;
        //void ()
        public MethodInfo setupInfo;
    }

    private static Accessors[] accessors = new Accessors [3] {
        null,
        new Accessors {
            actionsInfo = AccessTools.Field(typeof(RadialMenuMain), "actionsMain"),
            allowedActionsInfo = AccessTools.Field(typeof(RadialMenuMain), "allowedActionsMain"),
            actionObjectsInfo = AccessTools.Field(typeof(RadialMenuMain), "actionObjectsMain"),
            degreesPerActionInfo = AccessTools.Field(typeof(RadialMenuMain), "degreesPerActionMain"),
            containerInfo = AccessTools.Field(typeof(RadialMenuMain), "actionsMainContainer"),
            setupInfo = AccessTools.Method(typeof(RadialMenuMain), "SetupMain"),
        },
        new Accessors {
            actionsInfo = AccessTools.Field(typeof(RadialMenuMain), "actionsWeapons"),
            allowedActionsInfo = AccessTools.Field(typeof(RadialMenuMain), "allowedActionsWeapons"),
            actionObjectsInfo = AccessTools.Field(typeof(RadialMenuMain), "actionObjectsWeapons"),
            containerInfo = AccessTools.Field(typeof(RadialMenuMain), "actionsWeaponsContainer"),
            degreesPerActionInfo = AccessTools.Field(typeof(RadialMenuMain), "degreesPerActionWeapons"),
            setupInfo = null, //SetupWeapons() should not be used because it initializes actionObjects for weapon stations
        },
    };

    private static FieldInfo playerInputInfo = AccessTools.Field(typeof(RadialMenuMain), "playerInput");

    private static bool initialized = false;
    private static bool inRadialMenuMainUpdate = false;
    private static bool prevButtonState = false;
    private static Aircraft aircraft = null;
    private static float z = 0f;
    private static float lastSelection = 0;
    private static bool updateLastSelection = false;
    private static float lastLevelSwitch = 0;
}


public class MultiLevelMenuAction : RadialMenuAction {
    public static Color DefaultColor = Color.gray;
    public static Color SelectedColor = Color.green;
    public static Color BackgroundColorActive = new Color (0, 0, 0, 0.8f);
    public static Color BackgroundColorInactive = new Color (0, 0, 0, 0.5f);

    public static void Init() {
        if (!initialized) {
            Plugin.harmony.PatchAll(typeof(MultiLevelMenuAction.OnRadialMenuActionGetActionType));
            Plugin.harmony.PatchAll(typeof(MultiLevelMenuAction.OnRadialMenuActionAllowedOnAircraft));
            Plugin.harmony.PatchAll(typeof(MultiLevelMenuAction.OnRadialMenuActionTriggerAction));
            Plugin.harmony.PatchAll(typeof(MultiLevelMenuAction.OnRadialMenuActionHover));
            Plugin.harmony.PatchAll(typeof(MultiLevelMenuAction.OnRadialMenuActionUnHover));
            Plugin.harmony.PatchAll(typeof(MultiLevelMenuAction.OnRadialMenuActionSetup));

            initialized = true;
        }
    }

	public MultiLevelMenuAction() {
		actionTypeInfo.SetValue(this, ActionType.NavLights);
	}

	public new virtual bool AllowedOnAircraft(Aircraft aircraft) => true;
	public new virtual void TriggerAction(Aircraft aircraft) { }
	public new virtual void Hover() { }
	public new virtual void UnHover() { }

    public virtual void Enable() {
        var backgroundImage = (Image)backgroundImageInfo.GetValue(this);
        if (backgroundImage == null) {
            Plugin.Log("[MLM] MultiLevelMenuAction.Enable(): backgroundImage is null");
            return;
        }
        backgroundImage.enabled = true;
        backgroundImage.gameObject.SetActive(true);
    }

    public virtual void Disable() {
        var backgroundImage = (Image)backgroundImageInfo.GetValue(this);
        if (backgroundImage == null) {
            Plugin.Log("[MLM] MultiLevelMenuActio.Enable(): backgroundImage is null");
            return;
        }
        backgroundImage.enabled = false;
        backgroundImage.gameObject.SetActive(false);
    }

    public string AmmoText {
        set {
            field = value;
            var ammoText = (Text)ammoTextInfo.GetValue(this);
            if (ammoText != null) {
                ammoText.text = value;
                ammoText.enabled = true;
            }
        }
        get {
            return field;
        }
    } = "";

    protected Sprite sprite = null;

	[HarmonyPatch(typeof(RadialMenuAction), "GetActionType")]
    public class OnRadialMenuActionGetActionType {
        private static bool Prefix(RadialMenuAction __instance, ref RadialMenuAction.ActionType __result) {
            if (__instance is not MultiLevelMenuAction multiLevelMenuAction) return true;
            __result = RadialMenuAction.ActionType.NavLights;
            return false;
        }
    }

	[HarmonyPatch(typeof(RadialMenuAction), "AllowedOnAircraft")]
    public class OnRadialMenuActionAllowedOnAircraft {
        private static bool Prefix(RadialMenuAction __instance, Aircraft aircraft, ref bool __result) {
            if (__instance is not MultiLevelMenuAction multiLevelMenuAction) return true;
            __result = multiLevelMenuAction.AllowedOnAircraft(aircraft);
            return false;
        }
    }

	[HarmonyPatch(typeof(RadialMenuAction), "TriggerAction")]
    public class OnRadialMenuActionTriggerAction {
        private static bool Prefix(RadialMenuAction __instance, Aircraft aircraft) {
            if (__instance is not MultiLevelMenuAction multiLevelMenuAction) return true;
            multiLevelMenuAction.TriggerAction(aircraft);
            return false;
        }
    }

	[HarmonyPatch(typeof(RadialMenuAction), "Hover")]
    public class OnRadialMenuActionHover {
        private static void Postfix(RadialMenuAction __instance) {
            if (__instance is MultiLevelMenuAction multiLevelMenuAction)
                multiLevelMenuAction.Hover();
        }
    }

	[HarmonyPatch(typeof(RadialMenuAction), "UnHover")]
    public class OnRadialMenuActionUnHover {
        private static void Postfix(RadialMenuAction __instance) {
            if (__instance is MultiLevelMenuAction multiLevelMenuAction)
                multiLevelMenuAction.UnHover();
        }
    }

    [HarmonyPatch(typeof(RadialMenuAction), "Setup")]
    public class OnRadialMenuActionSetup {
        private static void Prefix(RadialMenuAction __instance, Image backgroundImage, ref Image ___iconImage, ref Image ___backgroundImage, ref Sprite __state) {
            __state = backgroundImage.sprite;
        }

        private static void Postfix(
            RadialMenuAction __instance, Image backgroundImage, ref Image ___iconImage, ref Image ___backgroundImage, ref Sprite __state,
            ref Color ___backgroundColorActive, ref Color ___backgroundColorInactive, ref Color ___selectedColor, ref Color ___defaultColor)
        {
            if (__instance is not MultiLevelMenuAction multiLevelMenuAction) return;
            multiLevelMenuAction.AmmoText = multiLevelMenuAction.AmmoText; //applies AmmoText
            if (multiLevelMenuAction.sprite != null) {
                ___iconImage.enabled = true;
                ___iconImage.sprite = multiLevelMenuAction.sprite;
            }
            else {
                ___iconImage.enabled = false;
                var ammoText = (Text)ammoTextInfo.GetValue(multiLevelMenuAction);
                if (ammoText != null) {
                    ammoText.gameObject.transform.localPosition = Vector3.zero;
                    ammoText.alignment = TextAnchor.MiddleCenter;
                    ammoText.resizeTextMinSize = 40;
                    ammoText.resizeTextMaxSize = 40;
                    ammoText.fontSize = 40;
                    ammoText.fontStyle = FontStyle.Bold;
                }
            }

            ___selectedColor = SelectedColor;
            ___defaultColor = DefaultColor;
            ___backgroundImage.sprite = __state;
            ___backgroundColorActive = BackgroundColorActive;
            ___backgroundColorInactive = BackgroundColorInactive;
            ___backgroundImage.color = ___backgroundColorInactive;
        }
    }

    private static FieldInfo actionTypeInfo = AccessTools.Field(typeof(RadialMenuAction), "actionType");
    private static FieldInfo ammoTextInfo = AccessTools.Field(typeof(RadialMenuAction), "ammoText");
    private static FieldInfo backgroundImageInfo = AccessTools.Field(typeof(RadialMenuAction), "backgroundImage");

    private static bool initialized = false;
}

public class CallbackMultiLevelMenuAction : MultiLevelMenuAction {
    public static CallbackMultiLevelMenuAction Create(Action callback, string displayName, string ammoText, Sprite sprite = null) {
        var result = ScriptableObject.CreateInstance<CallbackMultiLevelMenuAction> ();
        result.DisplayName = displayName;
        result.AmmoText = ammoText;
        result.sprite = sprite;
        result.OnTriggerAction += callback;
        return result;
    }

    public override void TriggerAction(Aircraft aircraft) {
        OnTriggerAction?.Invoke();
    }

    public override void Hover() {
        OnHover?.Invoke();
    }

    public override void UnHover() {
        OnUnHover?.Invoke();
    }

    public event Action OnTriggerAction;
    public event Action OnHover;
    public event Action OnUnHover;
}
