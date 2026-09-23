using HarmonyLib;
using UnityEngine;
using NO_Tactitools.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace NO_Tactitools.UI.MFD;

[HarmonyPatch(typeof(MainMenu), "Start")]
class DeliveryCheckerPlugin {
    private static bool initialized = false;
    static void Postfix() {
        if (!initialized) {
            Plugin.Log("[DV] DeliveryChecker plugin starting !");

            Plugin.harmony.PatchAll(typeof(DeliveryCheckerComponent.OnMissileStart));
            Plugin.harmony.PatchAll(typeof(DeliveryCheckerComponent.OnMissileRpcDetonate));
            Plugin.harmony.PatchAll(typeof(DeliveryCheckerComponent.OnPlatformUpdate));
            Plugin.harmony.PatchAll(typeof(DeliveryCheckerComponent.OnPlatformStart));

            var bindings = new BindingHelper.Binding[] {
                new (typeof(DeliveryCheckerComponent.InternalState), "ShowTTI", Plugin.deliveryCheckerShowTTI),
            };
            BindingHelper.ApplyBindings(bindings);

            initialized = true;

            Plugin.Log("[DV] DeliveryChecker plugin succesfully started !");
        }
    }
}

public class DeliveryCheckerComponent {
    // LOGIC ENGINE, INTERNAL STATE, DISPLAY ENGINE

    static class LogicEngine {
        static public void Init() {
            InternalState.deliveryChecker?.Destroy();
            InternalState.deliveryChecker = null;
            //deliveries container is not cleared to retain player-owned deliverables across respawns
            //...Updated fields are reset to true to force label updates after respawn
            InternalState.missileCountUpdated = true;
            InternalState.bombCountUpdated = true;
            InternalState.missileTTIUpdated = true;
            InternalState.bombTTIUpdated = true;
        }

        static public void Update() {
            if (GameBindings.Player.Aircraft.GetAircraft() == null
                || UIBindings.Game.GetTargetScreenTransform(true) == null
                || UIBindings.Game.GetTacScreenTransform(true) == null) {
                return; // no aircraft or no targeting screen
            }

            List<Missile> deliveriesToRemove = [];
            foreach ((var missile, var deliveryInfo) in InternalState.deliveries) {
                switch (deliveryInfo.Status) {
                   case InternalState.DeliveryStatus.InFlight:
                       // check if the delivery has been in flight for more than 120 seconds
                       if (Time.time - deliveryInfo.startTime > 120f) {
                           deliveryInfo.Status = InternalState.DeliveryStatus.Missed;
                           deliveryInfo.hitTime = Time.time;
                           deliveryInfo.timeToImpact = -1f;
                           UpdateCountUpdated(deliveryInfo.Type);
                       }
                       else {
                           deliveryInfo.timeToImpact = ComputeTTI(missile);
                       }
                       break;
                   case InternalState.DeliveryStatus.Hit:
                   case InternalState.DeliveryStatus.Missed:
                       // check if the delivery has been in hit/missed status for more than 3 seconds
                       if (Time.time - deliveryInfo.hitTime > 3f) {
                           // add the delivery to the list of deliveries to remove
                           deliveriesToRemove.Add(missile);
                           UpdateCountUpdated(deliveryInfo.Type);
                       }
                       break;
                }
            }

            // remove the deliveries that have been in hit/missed status for more than 3 seconds
            foreach (Missile missile in deliveriesToRemove) {
                InternalState.deliveries.Remove(missile);
            }

            UpdateTTIs();
        }

        static public void OnMissileStart(Missile missile) {
            var aircraft = GameBindings.Player.Aircraft.GetAircraft();
            if (aircraft == null)
                return;

            if (UIBindings.Game.GetTargetScreenTransform(true) == null ||
                UIBindings.Game.GetTacScreenTransform(true) == null) {
                return;
            }

            //adding only missiles launched by players aircraft
            if (missile.owner == aircraft) {
                var type = missile.GetWeaponInfo().bomb ? InternalState.DeliveryType.Bomb : InternalState.DeliveryType.Missile;
                var deliveryInfo = new InternalState.DeliveryInfo() {
                    Type = type,
                    Status = InternalState.DeliveryStatus.InFlight,
                    startTime = Time.time,
                    hitTime = -1f,
                    timeToImpact = -1f
                };
                InternalState.deliveries[missile] = deliveryInfo;
                deliveryInfo.timeToImpact = ComputeTTI(missile);
                UpdateCountUpdated(type);
            }
        }

        static public void OnMissileRpcDetonate(Missile missile, Unit target, bool hitArmor, bool hitTerrain) {
            var aircraft = GameBindings.Player.Aircraft.GetAircraft();
            if (aircraft == null)
                return;

            if (UIBindings.Game.GetTargetScreenTransform(true) == null ||
                UIBindings.Game.GetTacScreenTransform(true) == null) {
                return;
            }

            //accounting for deliverables launched before respawn
            if (IsPlayersMissile(missile) && InternalState.deliveries.TryGetValue(missile, out var deliveryInfo)) {
                deliveryInfo.Status =
                    (hitArmor || (target != null && (target.GlobalPosition() - missile.GlobalPosition()).magnitude <= target.maxRadius)) ?
                    InternalState.DeliveryStatus.Hit :
                    InternalState.DeliveryStatus.Missed;
                deliveryInfo.hitTime = Time.time;
                deliveryInfo.timeToImpact = -1;
                UpdateCountUpdated(deliveryInfo.Type);
            }
        }

        static private bool IsPlayersMissile(Missile missile) {
            if (UnitRegistry.TryGetPersistentUnit(missile.ownerID, out var missilePersistentUnit) &&
                GameManager.GetLocalPlayer<NuclearOption.Networking.BasePlayer>(out var localPlayer))
                return missilePersistentUnit.player == localPlayer;
            else
                return false;
        }

        static private void UpdateCountUpdated(InternalState.DeliveryType type) {
            switch (type) {
                case InternalState.DeliveryType.Bomb:
                    InternalState.bombCountUpdated = true;
                    break;
                case InternalState.DeliveryType.Missile:
                    InternalState.missileCountUpdated = true;
                    break;
            }
        }

        static private float ComputeTTI(Missile missile) {
            return !InternalState.ShowTTI ? -1f : GameBindings.Helpers.ComputeMissileTTI(missile);
        }

        static private void UpdateTTIs() {
            if (!InternalState.ShowTTI)
                return;

            int i = 0;
            for (i = 0; i < InternalState.missileTTIs.Length; ++i) {
                InternalState.missileTTIUpdated |= InternalState.missileTTIs[i] != -1;
                InternalState.missileTTIs[i] = -1;
            }
            var missileTTIs = InternalState.deliveries
                .Where(p => p.Value.Type == InternalState.DeliveryType.Missile && p.Value.Status == InternalState.DeliveryStatus.InFlight && p.Value.timeToImpact > 0)
                .Select(p => p.Value.timeToImpact)
                .OrderBy(timeToImpact => timeToImpact)
                .Take(InternalState.missileTTIs.Length);
            i = 0;
            foreach (var missileTTI in missileTTIs)
                InternalState.missileTTIs[i++] = missileTTI;
            InternalState.missileTTIUpdated |= i != 0;
            i = 0;
            for (i = 0; i < InternalState.bombTTIs.Length; ++i) {
                InternalState.bombTTIUpdated |= InternalState.bombTTIs[i] != -1;
                InternalState.bombTTIs[i] = -1;
            }
            var bombTTIs = InternalState.deliveries
                .Where(p => p.Value.Type == InternalState.DeliveryType.Bomb && p.Value.Status == InternalState.DeliveryStatus.InFlight && p.Value.timeToImpact > 0)
                .Select(p => p.Value.timeToImpact)
                .OrderBy(timeToImpact => timeToImpact)
                .Take(InternalState.bombTTIs.Length);
            i = 0;
            foreach (var bombTTI in bombTTIs)
                InternalState.bombTTIs[i++] = bombTTI;
            InternalState.bombTTIUpdated |= i != 0;
        }
    }

    public static class InternalState {
        public enum DeliveryType {
            Missile,
            Bomb
        }
        public enum DeliveryStatus {
            InFlight,
            Hit,
            Missed
        }
        public class DeliveryInfo {
            public DeliveryType Type;
            public DeliveryStatus Status;
            public float startTime;
            public float hitTime;
            public float timeToImpact;
        }
        static public bool ShowTTI = true;
        static public Dictionary<Missile, DeliveryInfo> deliveries = [];
        static public DeliveryChecker deliveryChecker;
        static public float[] bombTTIs = new float[4];
        static public float[] missileTTIs = new float[4];
        static public bool missileCountUpdated = true;
        static public bool bombCountUpdated = true;
        static public bool missileTTIUpdated = false;
        static public bool bombTTIUpdated = false;
    }

    static class DisplayEngine {
        static public void Init() {
            Plugin.Log("[DC] Initializing Delivery Checker for Target Screen");
            InternalState.deliveryChecker = new DeliveryChecker();
            Plugin.Log("[DC] Delivery Checker for Target Screen initialized");
        }
        static public void Update() {
            if (GameBindings.Player.Aircraft.GetAircraft() == null
                || UIBindings.Game.GetTargetScreenTransform(true) == null
                || UIBindings.Game.GetTacScreenTransform(true) == null) {
                return; // no aircraft or no targeting screen
            }
            if (InternalState.deliveryChecker == null) {
                Init();
            }
            // skip ui
            if (!(InternalState.missileCountUpdated || InternalState.bombCountUpdated || InternalState.missileTTIUpdated || InternalState.bombTTIUpdated))
                return;

            // do update work
            int missilesInFlight = 0;
            int bombsInFlight = 0;
            int missilesHit = 0;
            int bombsHit = 0;
            int missilesMissed = 0;
            int bombsMissed = 0;

            foreach (var deliveryInfo in InternalState.deliveries.Values) {
                switch (deliveryInfo.Type) {
                    case InternalState.DeliveryType.Missile:
                        switch (deliveryInfo.Status) {
                            case InternalState.DeliveryStatus.InFlight:
                                ++missilesInFlight;
                                break;
                            case InternalState.DeliveryStatus.Hit:
                                ++missilesHit;
                                break;
                            case InternalState.DeliveryStatus.Missed:
                                ++missilesMissed;
                                break;
                            default:
                                Plugin.Log($"[DC] Unsupported delivery status {deliveryInfo.Status} for delivery type: {deliveryInfo.Type}");
                                break;
                        }
                        break;
                    case InternalState.DeliveryType.Bomb:
                        switch (deliveryInfo.Status) {
                            case InternalState.DeliveryStatus.InFlight:
                                ++bombsInFlight;
                                break;
                            case InternalState.DeliveryStatus.Hit:
                                ++bombsHit;
                                break;
                            case InternalState.DeliveryStatus.Missed:
                                ++bombsMissed;
                                break;
                            default:
                                Plugin.Log($"[DC] Unsupported delivery status {deliveryInfo.Status} for delivery type: {deliveryInfo.Type}");
                                break;
                        }
                        break;
                    default:
                        Plugin.Log($"[DC] Unsupported delivery type: {deliveryInfo.Type}");
                        break;
                }
            }

            var dc = InternalState.deliveryChecker;

            if (InternalState.missileCountUpdated) {
                // Toggle visibility of M category labels based on whether there are any deliveries
                dc.missileLabel.GetGameObject().SetActive(missilesInFlight + missilesHit + missilesMissed > 0);
                // Update missile in flight label
                dc.missileInFlightLabel.SetText(missilesInFlight > 0 ? missilesInFlight.ToString() : "");
                dc.missileInFlightLabel.GetGameObject().SetActive(missilesInFlight > 0);
                //TODO Remove - position is fixed
                dc.SetLabelPos(dc.missileInFlightLabel, dc.xOffsetMissile, 0);
                // Update missile hit label
                var missilesInFlightT = missilesInFlight > 0 ? 1 : 0;
                dc.missileHitLabel.SetText(missilesHit > 0 ? missilesHit.ToString() : "");
                dc.missileHitLabel.GetGameObject().SetActive(missilesHit > 0);
                dc.SetLabelPos(dc.missileHitLabel, dc.xOffsetMissile, missilesInFlightT);
                // Update missile missed label
                var missilesHitT = missilesHit > 0 ? 1 : 0;
                dc.missileMissedLabel.SetText(missilesMissed > 0 ? missilesMissed.ToString() : "");
                dc.missileMissedLabel.GetGameObject().SetActive(missilesMissed > 0);
                dc.SetLabelPos(dc.missileMissedLabel, dc.xOffsetMissile, missilesInFlightT + missilesHitT);

                InternalState.missileCountUpdated = false;
            }

            if (InternalState.bombCountUpdated) {
                // Toggle visibility of B category labels based on whether there are any deliveries
                dc.bombLabel.GetGameObject().SetActive(bombsInFlight + bombsHit + bombsMissed > 0);
                // Update bomb in flight label
                dc.bombInFlightLabel.SetText(bombsInFlight > 0 ? bombsInFlight.ToString() : "");
                dc.bombInFlightLabel.GetGameObject().SetActive(bombsInFlight > 0);
                //TODO Remove - position is fixed
                dc.SetLabelPos(dc.bombInFlightLabel, dc.xOffsetBomb, 0);
                // Update bomb hit label
                var bombsInFlightT = bombsInFlight > 0 ? 1 : 0;
                dc.bombHitLabel.SetText(bombsHit > 0 ? bombsHit.ToString() : "");
                dc.bombHitLabel.GetGameObject().SetActive(bombsHit > 0);
                dc.SetLabelPos(dc.bombHitLabel, dc.xOffsetBomb, bombsInFlightT);
                // Update bomb missed label
                var bombsHitT = bombsHit > 0 ? 1 : 0;
                dc.bombMissedLabel.SetText(bombsMissed > 0 ? bombsMissed.ToString() : "");
                dc.bombMissedLabel.GetGameObject().SetActive(bombsMissed > 0);
                dc.SetLabelPos(dc.bombMissedLabel, dc.xOffsetBomb, bombsInFlightT + bombsHitT);

                InternalState.bombCountUpdated = false;
            }

            if (InternalState.ShowTTI) {
                if (InternalState.missileTTIUpdated) {
                    //Update visibility and contents of missile TTI labels
                    for (int i = 0; i < dc.missileTTILabels.Length; ++i) {
                        var missileTTI = InternalState.missileTTIs[i];
                        var missileTTILabel = dc.missileTTILabels[i];
                        var ttiValid = missileTTI > 0;
                        missileTTILabel.GetGameObject().SetActive(ttiValid);
                        if (ttiValid) {
                            missileTTILabel.SetText(missileTTI > 999 ? "999" : ((int)missileTTI).ToString());
                        }
                    }

                    InternalState.missileTTIUpdated = false;
                }

                if (InternalState.bombTTIUpdated) {
                    //Update visibility and contents of bomb TTI labels
                    for (int i = 0; i < dc.bombTTILabels.Length; ++i) {
                        var bombTTI = InternalState.bombTTIs[i];
                        var bombTTILabel = dc.bombTTILabels[i];
                        var ttiValid = bombTTI > 0;
                        bombTTILabel.GetGameObject().SetActive(ttiValid);
                        if (ttiValid) {
                            bombTTILabel.SetText(((int)bombTTI).ToString());
                        }
                    }

                    InternalState.bombTTIUpdated = false;
                }
            }
        }
    }

    public class DeliveryChecker {
        public GameObject containerObject;
        public Transform containerTransform;
        public UIBindings.Draw.UIAdvancedRectangleLabeled missileLabel;
        public UIBindings.Draw.UIAdvancedRectangleLabeled bombLabel;
        public UIBindings.Draw.UIAdvancedRectangleLabeled[] missileTTILabels = new UIBindings.Draw.UIAdvancedRectangleLabeled[4];
        public UIBindings.Draw.UIAdvancedRectangleLabeled[] bombTTILabels = new UIBindings.Draw.UIAdvancedRectangleLabeled[4];
        public UIBindings.Draw.UIAdvancedRectangleLabeled missileInFlightLabel;
        public UIBindings.Draw.UIAdvancedRectangleLabeled bombInFlightLabel;
        public UIBindings.Draw.UIAdvancedRectangleLabeled missileHitLabel;
        public UIBindings.Draw.UIAdvancedRectangleLabeled bombHitLabel;
        public UIBindings.Draw.UIAdvancedRectangleLabeled missileMissedLabel;
        public UIBindings.Draw.UIAdvancedRectangleLabeled bombMissedLabel;
        public float xOffsetMissile;
        public float xOffsetBomb;
        public float yOffset;
        public readonly int fontSize = 20;
        public readonly float labelWidth = 36f;
        public readonly float labelHeight = 20f;
        public readonly float labelWidthHalf;
        public readonly float labelHeightHalf;

        public DeliveryChecker() {
            labelWidthHalf = 0.5f * labelWidth;
            labelHeightHalf = 0.5f * labelHeight;

            Transform parentTransform = UIBindings.Game.GetTargetScreenTransform();
            RectTransform rectTransform = parentTransform as RectTransform;

            if (rectTransform != null) {
                float halfWidth = 0.5f * rectTransform.rect.width;
                float halfHeight = 0.5f * rectTransform.rect.height;
                xOffsetMissile = -halfWidth + labelWidthHalf;
                xOffsetBomb = halfWidth - labelWidthHalf;
                yOffset = -halfHeight + labelHeightHalf + 64f; // 64f is the height of the bearing

            } else {
                xOffsetMissile = -110f;
                xOffsetBomb = 110f;
                yOffset = -110f;
            }

            // Create container GameObject to hold all DeliveryChecker elements
            containerObject = new GameObject("i_dc_DeliveryCheckerContainer");
            containerObject.AddComponent<RectTransform>();
            containerTransform = containerObject.transform;
            containerTransform.SetParent(parentTransform, false);
            // Create a M for missile and B for bomb labels using AdvancedUIRectangleLabel
            missileLabel = new UIBindings.Draw.UIAdvancedRectangleLabeled(
                name: "i_dc_MissileLabel",
                cornerA: new Vector2(xOffsetMissile - labelWidthHalf, yOffset - labelHeightHalf),
                cornerB: new Vector2(xOffsetMissile + labelWidthHalf, yOffset + labelHeightHalf),
                borderColor: Color.clear,
                borderThickness: 0f,
                UIParent: containerTransform,
                fillColor: new Color(0f, 0f, 0f, 0.8f),
                fontStyle: FontStyle.Bold,
                textColor: Color.white,
                fontSize: fontSize
            );
            missileLabel.SetText("M");
            missileLabel.GetGameObject().SetActive(false);
            bombLabel = new UIBindings.Draw.UIAdvancedRectangleLabeled(
                name: "i_dc_BombLabel",
                cornerA: new Vector2(xOffsetBomb - labelWidthHalf, yOffset - labelHeightHalf),
                cornerB: new Vector2(xOffsetBomb + labelWidthHalf, yOffset + labelHeightHalf),
                borderColor: Color.clear,
                borderThickness: 0f,
                UIParent: containerTransform,
                fillColor: new Color(0f, 0f, 0f, 0.8f),
                fontStyle: FontStyle.Bold,
                textColor: Color.white,
                fontSize: fontSize
            );
            bombLabel.SetText("B");
            bombLabel.GetGameObject().SetActive(false);
            missileInFlightLabel = new UIBindings.Draw.UIAdvancedRectangleLabeled(
                name: "i_dc_MissileInFlightLabel",
                cornerA: new Vector2(xOffsetMissile - labelWidthHalf, yOffset - labelHeightHalf),
                cornerB: new Vector2(xOffsetMissile + labelWidthHalf, yOffset + labelHeightHalf),
                borderColor: Color.clear,
                borderThickness: 0f,
                UIParent: containerTransform,
                fillColor: new Color(1f, 1f, 0f, 0.8f),
                fontStyle: FontStyle.Bold,
                textColor: Color.black,
                fontSize: fontSize
            );
            missileInFlightLabel.SetText("");
            missileInFlightLabel.GetGameObject().SetActive(false);
            for (int i = 0; i < missileTTILabels.Length; ++i) {
                missileTTILabels[i] = new UIBindings.Draw.UIAdvancedRectangleLabeled(
                    name: $"i_dc_MissileTTILabel_{i}",
                    cornerA: new Vector2(xOffsetMissile - labelWidthHalf + labelWidth, yOffset - labelHeightHalf + labelHeight * i),
                    cornerB: new Vector2(xOffsetMissile + labelWidthHalf + labelWidth, yOffset + labelHeightHalf + labelHeight * i),
                    borderColor: Color.clear,
                    borderThickness: 0f,
                    UIParent: containerTransform,
                    fillColor: new Color(1f, 1f, 0f, 0.5f),
                    fontStyle: FontStyle.Bold,
                    textColor: Color.black,
                    fontSize: fontSize
                );
                missileTTILabels[i].SetText("");
                missileTTILabels[i].GetGameObject().SetActive(false);
            }
            bombInFlightLabel = new UIBindings.Draw.UIAdvancedRectangleLabeled(
                name: "i_dc_BombInFlightLabel",
                cornerA: new Vector2(xOffsetBomb - labelWidthHalf, yOffset - labelHeightHalf),
                cornerB: new Vector2(xOffsetBomb + labelWidthHalf, yOffset + labelHeightHalf),
                borderColor: Color.clear,
                borderThickness: 0f,
                UIParent: containerTransform,
                fillColor: new Color(1f, 1f, 0f, 0.8f),
                fontStyle: FontStyle.Bold,
                textColor: Color.black,
                fontSize: fontSize
            );
            bombInFlightLabel.SetText("");
            bombInFlightLabel.GetGameObject().SetActive(false);
            for (int i = 0; i < bombTTILabels.Length; ++i) {
                bombTTILabels[i] = new UIBindings.Draw.UIAdvancedRectangleLabeled(
                    name: $"i_dc_BombTTILabel_{i}",
                    cornerA: new Vector2(xOffsetBomb - labelWidthHalf - labelWidth, yOffset - labelHeightHalf + labelHeight * i),
                    cornerB: new Vector2(xOffsetBomb + labelWidthHalf - labelWidth, yOffset + labelHeightHalf + labelHeight * i),
                    borderColor: Color.clear,
                    borderThickness: 0f,
                    UIParent: containerTransform,
                    fillColor: new Color(1f, 1f, 0f, 0.5f),
                    fontStyle: FontStyle.Bold,
                    textColor: Color.black,
                    fontSize: fontSize
                );
                bombTTILabels[i].SetText("");
                bombTTILabels[i].GetGameObject().SetActive(false);
            }
            missileHitLabel = new UIBindings.Draw.UIAdvancedRectangleLabeled(
                name: "i_dc_MissileHitLabel",
                cornerA: new Vector2(xOffsetMissile - labelWidthHalf, yOffset - labelHeightHalf),
                cornerB: new Vector2(xOffsetMissile + labelWidthHalf, yOffset + labelHeightHalf),
                borderColor: Color.clear,
                borderThickness: 0f,
                UIParent: containerTransform,
                fillColor: new Color(0f, 1f, 0f, 0.8f),
                fontStyle: FontStyle.Bold,
                textColor: Color.black,
                fontSize: fontSize
            );
            missileHitLabel.SetText("");
            missileHitLabel.GetGameObject().SetActive(false);
            bombHitLabel = new UIBindings.Draw.UIAdvancedRectangleLabeled(
                name: "i_dc_BombHitLabel",
                cornerA: new Vector2(xOffsetBomb - labelWidthHalf, yOffset - labelHeightHalf),
                cornerB: new Vector2(xOffsetBomb + labelWidthHalf, yOffset + labelHeightHalf),
                borderColor: Color.clear,
                borderThickness: 0f,
                UIParent: containerTransform,
                fillColor: new Color(0f, 1f, 0f, 0.8f),
                fontStyle: FontStyle.Bold,
                textColor: Color.black,
                fontSize: fontSize
            );
            bombHitLabel.SetText("");
            bombHitLabel.GetGameObject().SetActive(false);
            missileMissedLabel = new UIBindings.Draw.UIAdvancedRectangleLabeled(
                name: "i_dc_MissileMissedLabel",
                cornerA: new Vector2(xOffsetMissile - labelWidthHalf, yOffset - labelHeightHalf),
                cornerB: new Vector2(xOffsetMissile + labelWidthHalf, yOffset + labelHeightHalf),
                borderColor: Color.clear,
                borderThickness: 0f,
                UIParent: containerTransform,
                fillColor: new Color(1f, 0f, 0f, 0.8f),
                fontStyle: FontStyle.Bold,
                textColor: Color.black,
                fontSize: fontSize
            );
            missileMissedLabel.SetText("");
            missileMissedLabel.GetGameObject().SetActive(false);
            bombMissedLabel = new UIBindings.Draw.UIAdvancedRectangleLabeled(
                name: "i_dc_BombMissedLabel",
                cornerA: new Vector2(xOffsetBomb - labelWidthHalf, yOffset - labelHeightHalf),
                cornerB: new Vector2(xOffsetBomb + labelWidthHalf, yOffset + labelHeightHalf),
                borderColor: Color.clear,
                borderThickness: 0f,
                UIParent: containerTransform,
                fillColor: new Color(1f, 0f, 0f, 0.8f),
                fontStyle: FontStyle.Normal,
                textColor: Color.black,
                fontSize: fontSize
            );
            bombMissedLabel.SetText("");
            bombMissedLabel.GetGameObject().SetActive(false);
        }

        public void Destroy() {
            GameObject.Destroy(containerObject);
        }

        public void SetLabelPos(UIBindings.Draw.UIAdvancedRectangleLabeled label, float xOffset, int stackPos) {
            label.SetCorners(
                new Vector2(xOffset - labelWidthHalf, yOffset + labelHeightHalf + labelHeight * stackPos),
                new Vector2(xOffset + labelWidthHalf, yOffset + labelHeightHalf + labelHeight * (stackPos + 1))
            );
        }
    }

    // HARMONY PATCHES
    [HarmonyPatch(typeof(Missile), "StartMissile")]
    public static class OnMissileStart {
        static void Postfix(Missile __instance) {
            LogicEngine.OnMissileStart(__instance);
        }
    }

    [HarmonyPatch(typeof(Missile), "RpcDetonate")]
    public static class OnMissileRpcDetonate {
        static void Prefix(Missile __instance, Unit relativeUnit, bool hitArmor, bool hitTerrain) {
            LogicEngine.OnMissileRpcDetonate(__instance, relativeUnit, hitArmor, hitTerrain);
        }
    }

    [HarmonyPatch(typeof(TacScreen), "Initialize")]
    public static class OnPlatformStart {
        static void Postfix() {
            LogicEngine.Init();
            // DisplayEngine.Init();
            // Since there is no target screen at the start of the game, we will initialize the display engine on the first update instead
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
