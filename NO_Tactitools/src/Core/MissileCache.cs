using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace NO_Tactitools.Core;

public class MissileData {
    public virtual void Update() {}

    public MissileData(Missile missile, Unit target) {
        this.missile = missile;
        this.target = target;
    }

    public Missile missile;
    public Unit target;
}

public class PlayerMissileData : MissileData {
    public float tti;

    public override void Update() {
        this.tti = GameBindings.Helpers.ComputeMissileTTI(this.missile);
    }

    public PlayerMissileData(Missile missile, Unit target = null) : base(missile, target) {
        this.tti = GameBindings.Helpers.ComputeMissileTTI(this.missile);
    }
}

public abstract class IMissileCache {
    public abstract void StartMisile(Missile missile);

    public abstract void RpcDetonate(Missile missile, Unit relativeUnit, bool hitArmor, bool hitTerrain);

    public abstract void UnitDisabled(Missile missile, bool oldState, bool newState);

    public abstract void TargetIDChanged(Missile missile, PersistentID oldValue, PersistentID newValue);

    public abstract void Update();

}

public class MissileCache<MD> : IMissileCache where MD : MissileData {
    public override void StartMisile(Missile missile) {
        if (!ShouldProcessMissile(missile))
            return;

        if (!UnitRegistry.TryGetUnit(missile.targetID, out var target))
            target = null;

        MD missileData = MakeMissileData(missile, target);

        Missiles[missile] = missileData;

        if (target != null) {
            missileData.target = target;
            if (Targets.TryGetValue(target, out var missileDatum))
                missileDatum.Add(missileData);
            else
                Targets[target] = new List<MD> { missileData };
        }
    }

    public override void RpcDetonate(Missile missile, Unit relativeUnit, bool hitArmor, bool hitTerrain) {
        RemoveMissile(missile);
    }

    public override void UnitDisabled(Missile missile, bool oldState, bool newState) {
        RemoveMissile(missile);
    }

    public override void TargetIDChanged(Missile missile, PersistentID oldValue, PersistentID newValue) {
        if (!ShouldProcessMissile(missile))
            return;

        if (!Missiles.TryGetValue(missile, out var missileData))
            return;

        if (UnitRegistry.TryGetUnit(oldValue, out var oldTarget) && Targets.TryGetValue(oldTarget, out var oldMissileDatum)) {
            oldMissileDatum.Remove(missileData);
            if (oldMissileDatum.Count == 0)
                Targets.Remove(oldTarget);
        }

        if (UnitRegistry.TryGetUnit(newValue, out var newTarget)) {
            missileData.target = newTarget;
            if (Targets.TryGetValue(newTarget, out var newMissileDatum))
                newMissileDatum.Add(missileData);
            else
                Targets[newTarget] = new List<MD> { missileData };
        }
        else
            missileData.target = null;
    }

    public override void Update() {
        missilesToRemove.Clear();
        foreach ((var missile, var missileData) in Missiles) {
            if (missile == null || missile.disabled) {
                missilesToRemove.Add(missile);
                var target = missileData.target;
                if (target is null) {
                    //noop
                }
                else if (target == null) {
                    Targets.Remove(target);
                }
                else {
                    var missileDatum = Targets[target];
                    missileDatum.Remove(missileData);
                    if (missileDatum.Count == 0)
                        Targets.Remove(target);
                }
            } }
        foreach (var missileToRemove in missilesToRemove)
            Missiles.Remove(missileToRemove);

        foreach (var missileData in Missiles.Values)
            missileData.Update();
    }

    public Predicate<Missile> ShouldProcessMissile;
    public Func<Missile, Unit, MD> MakeMissileData;

    public Dictionary<Unit, List<MD>> Targets = new ();
    public Dictionary<Missile, MD> Missiles = new ();

    public MissileCache(Predicate<Missile> shouldProcessMissile, Func<Missile, Unit, MD> makeMissileData) {
        this.ShouldProcessMissile = shouldProcessMissile;
        this.MakeMissileData = makeMissileData;
    }

    private void RemoveMissile(Missile missile) {
        if (!Missiles.TryGetValue(missile, out var missileData))
            return;

        Missiles.Remove(missile);

        var target = missileData.target;

        if (target != null) {
            if (Targets.TryGetValue(target, out var missileDatum) && missileDatum.Remove(missileData) && missileDatum.Count == 0)
                Targets.Remove(missileData.target);
        }
        else {
            List<Unit> targetsToRemove = new ();
            foreach ((var tempTarget, var missileDatum) in Targets) {
                if (missileDatum.Remove(missileData) && missileDatum.Count == 0)
                    targetsToRemove.Add(tempTarget);
            }
            foreach (var targetToRemove in targetsToRemove)
                Targets.Remove(targetToRemove);
        }
    }

    private List<Missile> missilesToRemove = new ();
}

public static class MissileCacheManager {
    public static void AddCache(string name, IMissileCache missileCache) {
        missileCaches[name] = missileCache;
    }

    public static IMissileCache GetCache(string name) {
        return missileCaches[name];
    }

    public static MissileCache<MD> GetCache<MD>(string name) where MD : MissileData {
        if (missileCaches[name] is MissileCache<MD> missileCache)
            return missileCache;
        else
            return null;
    }

    public static void Init() {
        if (!initialized) {
            Plugin.Log("[MCM] Missile Cache Manager starting !");

            Plugin.harmony.PatchAll(typeof(OnMissileStartMissile));
            Plugin.harmony.PatchAll(typeof(OnMissileRpcDetonate));
            Plugin.harmony.PatchAll(typeof(OnMissileUnitDisabled));
            Plugin.harmony.PatchAll(typeof(OnMissileTargetIDChanged));
            Plugin.harmony.PatchAll(typeof(OnDynamicMapUpdate));

            initialized = true;

            Plugin.Log("[MCM] Missile Cache Manager started !");
        }
    }

    [HarmonyPatch(typeof(Missile), "StartMissile")]
    public static class OnMissileStartMissile {
        static void Postfix(Missile __instance) {
            foreach (var missileCache in missileCaches.Values)
                missileCache.StartMisile(__instance);
        }
    }

    [HarmonyPatch(typeof(Missile), "RpcDetonate")]
    public static class OnMissileRpcDetonate {
        static void Prefix(Missile __instance, Unit relativeUnit, bool hitArmor, bool hitTerrain) {
            foreach (var missileCache in missileCaches.Values)
                missileCache.RpcDetonate(__instance, relativeUnit, hitArmor, hitTerrain);
        }
    }

    [HarmonyPatch(typeof(Missile), "UnitDisabled")]
    public static class OnMissileUnitDisabled {
        static void Prefix(Missile __instance, bool oldState, bool newState) {
            foreach (var missileCache in missileCaches.Values)
                missileCache.UnitDisabled(__instance, oldState, newState);
        }
    }

    [HarmonyPatch(typeof(Missile), "TargetIDChanged")]
    public static class OnMissileTargetIDChanged {
        static void Prefix(Missile __instance, PersistentID oldValue, PersistentID newValue) {
            foreach (var missileCache in missileCaches.Values)
                missileCache.TargetIDChanged(__instance, oldValue, newValue);
        }
    }

    [HarmonyPatch(typeof(DynamicMap), "Update")]
    public static class OnDynamicMapUpdate {
        static void Postfix() {
            foreach (var missileCache in missileCaches.Values)
                missileCache.Update();
        }
    }

    private static Dictionary<string, IMissileCache> missileCaches = new ();
    private static bool initialized = false;
}

public static class MissileCacheComponent {
    public static bool IsPlayersMissile(Missile missile) {
        if (UnitRegistry.TryGetPersistentUnit(missile.ownerID, out var missilePersistentUnit) &&
            GameManager.GetLocalPlayer<NuclearOption.Networking.BasePlayer>(out var localPlayer))
            return missilePersistentUnit.player == localPlayer;
        else
            return false;
    }

    public static bool IsFriendlyMissile(Missile missile) {
        return GameBindings.GameState.GetFactionMode(missile) == FactionMode.Friendly;
    }

    public static void Init() {
        if (initialized)
            return;

        MissileCacheManager.Init();
        MissileCacheManager.AddCache("player", new MissileCache<PlayerMissileData>(IsPlayersMissile, (Missile missile, Unit target) => new PlayerMissileData(missile, target)));
        MissileCacheManager.AddCache("friendly", new MissileCache<MissileData>(IsFriendlyMissile, (Missile missile, Unit target) => new MissileData(missile, target)));

        initialized = true;
    }

    private static bool initialized = false;
}
