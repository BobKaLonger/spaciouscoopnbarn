using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Reflection;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace spaciouscoopnbarn
{
    public interface IContentPatcherAPI
    {
        void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>> getValue);
    }
    public class ModEntry : Mod
    {
        public static Mod modInstance;
        public static IContentPack cpPack;
        private const string ModDataKey = "bobkalonger.BKSCB_code/SpaciousMode";
        internal const string SpaciousBarn = "bobkalonger.spaciouscoopnbarnCP_SpaciousBarn";
        internal const string SpaciousCoop = "bobkalonger.spaciouscoopnbarnCP_SpaciousCoop";
        private const string SVECoopType = "FlashShifter.StardewValleyExpandedCP_PremiumCoop";

        public override void Entry(IModHelper helper)
        {
            modInstance = this;
            I18n.Init(Helper.Translation);

            var mi = Helper.ModRegistry.Get("bobkalonger.spaciouscoopnbarnCP");
            cpPack = mi.GetType().GetProperty("ContentPack")?.GetValue(mi) as IContentPack;
            if (cpPack == null)
            {
                Monitor.Log("Failed to get content pack for spaciouscoopnbarnCP.", LogLevel.Error);
                return;
            }

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Player.Warped += PlayerOnWarped;

            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var cp = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            if (cp is null)
            {
                Monitor.Log("Content Patcher not found; dynamic token will not be available.", LogLevel.Warn);
                return;
            }
            cp.RegisterToken(ModManifest, "SpaciousMode", GetCurrentSpaciousMode);
        }

        private IEnumerable<string> GetCurrentSpaciousMode()
        {
            if (!Context.IsWorldReady)
                return Array.Empty<string>();
            return new[] { ComputeSpaciousMode() };
        }

        private string ComputeSpaciousMode()
        {
            bool hasSVE = Helper.ModRegistry.IsLoaded("FlashShifter.StardewValleyExpandedCP");
            bool hasBKGCB = Helper.ModRegistry.IsLoaded("bobkalonger.gigacoopnbarn");
            bool hasJMCB = Helper.ModRegistry.IsLoaded("jenf1.megacoopbarn");
            bool hasUARC = Helper.ModRegistry.IsLoaded("UncleArya.ResourceChickens");

            if (hasSVE) return "SVE";
            if (hasBKGCB) return "BKGCB";
            if (hasJMCB) return hasUARC ? "Both" : "JMCB";
            if (hasUARC) return "UARC";
            return "Vanilla";
        }

        private void PlayerOnWarped(object sender, WarpedEventArgs e)
        {
            foreach (var b in e.NewLocation.buildings)
            {
                if (b.buildingType.Value == SpaciousBarn)
                {
                    var tileLoc1 = new Point(b.tileX.Value + 3, b.tileY.Value + 3);
                    var l1 = new LightSource($"spacious_SpaciousBarnLight_{b.tileX.Value}_{b.tileY.Value}_1", 4, tileLoc1.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                    Game1.currentLightSources.Add(l1.Id, l1);

                    var tileLoc2 = new Point(b.tileX.Value + 8, b.tileY.Value + 3);
                    var l2 = new LightSource($"spacious_SpaciousBarnLight_{b.tileX.Value}_{b.tileY.Value}_2", 4, tileLoc2.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                    Game1.currentLightSources.Add(l2.Id, l2);
                }
                else if (b.buildingType.Value == SpaciousCoop)
                {
                    var tileLoc = new Point(b.tileX.Value + 6, b.tileY.Value + 2);
                    var l = new LightSource($"spacious_SpaciousCoopLight_{b.tileX.Value}_{b.tileY.Value}_1", 4, tileLoc.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                    Game1.currentLightSources.Add(l.Id, l);
                }
                else if (b.buildingType.Value == SVECoopType)
                {
                    var tileLoc = new Point(b.tileX.Value + 6, b.tileY.Value + 2);
                    var l = new LightSource($"SVE_PremiumCoopLight_{b.tileX.Value}_{b.tileY.Value}_1", 4, tileLoc.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                    Game1.currentLightSources.Add(l.Id, l);
                }
            }
        }

        [HarmonyPatch(typeof(Building), nameof(Building.doesTileHaveProperty))]
        public static class SpaciousCursorPatch
        {
            public static void Postfix(Building __instance, int tile_x, int tile_y, string property_name, string layer_name, ref string property_value, ref bool __result)
            {
                // Barn
                if (__instance.buildingType.Value == SpaciousBarn && __instance.daysOfConstructionLeft.Value <= 0)
                {
                    var interior = __instance.GetIndoors();
                    if (tile_x == __instance.tileX.Value + __instance.humanDoor.X + 8 &&
                        tile_y == __instance.tileY.Value + __instance.humanDoor.Y &&
                        interior != null && property_name == "Action")
                    {
                        property_value = "meow";
                        __result = true;
                        return;
                    }
                }
                // Coop
                if (__instance.buildingType.Value == SpaciousCoop && __instance.daysOfConstructionLeft.Value <= 0)
                {
                    var interior = __instance.GetIndoors();
                    if (tile_x == __instance.tileX.Value + __instance.humanDoor.X - 2 &&
                        tile_y == __instance.tileY.Value + __instance.humanDoor.Y - 2 &&
                        interior != null && property_name == "Action")
                    {
                        property_value = "meow";
                        __result = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Building), nameof(Building.doAction))]
        public static class SpaciousDoorPatch
        {
            public static void Postfix(Building __instance, Vector2 tileLocation, Farmer who, ref bool __result)
            {
                if (who.ActiveObject != null && who.ActiveObject.IsFloorPathItem() && who.currentLocation != null && !who.currentLocation.terrainFeatures.ContainsKey(tileLocation))
                    return;

                // Barn
                if (__instance.buildingType.Value == SpaciousBarn && __instance.daysOfConstructionLeft.Value <= 0)
                {
                    var interior = __instance.GetIndoors();
                    if (tileLocation.X == __instance.tileX.Value + __instance.humanDoor.X + 8 &&
                        tileLocation.Y == __instance.tileY.Value + __instance.humanDoor.Y &&
                        interior != null)
                    {
                        if (who.mount != null)
                        {
                            Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:DismountBeforeEntering"));
                            __result = false;
                            return;
                        }
                        if (who.team.demolishLock.IsLocked())
                        {
                            Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:CantEnter"));
                            __result = false;
                            return;
                        }
                        if (__instance.OnUseHumanDoor(who))
                        {
                            who.currentLocation.playSound("doorClose", tileLocation);
                            bool isStructure = __instance.indoors.Value != null;
                            Game1.warpFarmer(interior.NameOrUniqueName, interior.warps[1].X, interior.warps[1].Y - 1, Game1.player.FacingDirection, isStructure);
                        }
                        __result = true;
                        return;
                    }
                }
                // Coop
                if (__instance.buildingType.Value == SpaciousCoop && __instance.daysOfConstructionLeft.Value <= 0)
                {
                    var interior = __instance.GetIndoors();
                    if (tileLocation.X == __instance.tileX.Value + __instance.humanDoor.X - 2 &&
                        tileLocation.Y == __instance.tileY.Value + __instance.humanDoor.Y - 2 &&
                        interior != null)
                    {
                        if (who.mount != null)
                        {
                            Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:DismountBeforeEntering"));
                            __result = false;
                            return;
                        }
                        if (who.team.demolishLock.IsLocked())
                        {
                            Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:CantEnter"));
                            __result = false;
                            return;
                        }
                        if (__instance.OnUseHumanDoor(who))
                        {
                            who.currentLocation.playSound("doorClose", tileLocation);
                            bool isStructure = __instance.indoors.Value != null;
                            Game1.warpFarmer(interior.NameOrUniqueName, interior.warps[1].X - 1, interior.warps[1].Y, Game1.player.FacingDirection, isStructure);
                        }
                        __result = true;
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Building), nameof(Building.updateInteriorWarps))]
        public static class SpaciousBarnWarpPatch
        {
            public static void Postfix(Building __instance, GameLocation interior)
            {
                if (__instance.buildingType.Value != SpaciousBarn || interior == null || interior.warps.Count == 0)
                    return;

                var w = interior.warps[1];
                interior.warps[1] = new(w.X, w.Y, w.TargetName, w.TargetX + 8, w.TargetY, w.flipFarmer.Value, w.npcOnly.Value);
            }
        }

        [HarmonyPatch(typeof(Building), nameof(Building.updateInteriorWarps))]
        public static class SpaciousCoopWarpPatch
        {
            public static void Postfix(Building __instance, GameLocation interior)
            {
                if (__instance.buildingType.Value != SpaciousCoop || interior == null || interior.warps.Count == 0)
                    return;

                var w = interior.warps[1];
                interior.warps[1] = new(w.X, w.Y, w.TargetName, w.TargetX - 1, w.TargetY - 3, w.flipFarmer.Value, w.npcOnly.Value);
            }
        }

        [HarmonyPatch(typeof(Utility), "_HasBuildingOrUpgrade")]
        public static class UtilityHasCoopBarnPatch
        {
            public static void Postfix(GameLocation location, string buildingId, ref bool __result)
            {
                string toCheck = buildingId switch
                {
                    "Coop" or "Deluxe Coop" or "Big Coop" or "FlashShifter.StardewValleyExpandedCP_PremiumCoop" => SpaciousCoop,
                    "Barn" or "Deluxe Barn" or "Big Barn" or "FlashShifter.StardewValleyExpandedCP_PremiumBarn" => SpaciousBarn,
                    _ => null
                };

                if (!__result && toCheck != null && location.getNumberBuildingsConstructed(toCheck) > 0)
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Building), nameof(Building.InitializeIndoor))]
    public static class BuildingAutoGrabberFix
    {
        public static void Postfix(Building __instance, BuildingData data, bool forConstruction, bool forUpgrade)
        {
            if (!forUpgrade)
                return;
            if (__instance.buildingType.Value != ModEntry.SpaciousCoop &&
                __instance.buildingType.Value != ModEntry.SpaciousBarn)
                return;

            foreach (var obj in __instance.indoors.Value.Objects.Values)
            {
                if (obj.QualifiedItemId == "(BC)165" && obj.heldObject.Value == null)
                {
                    obj.heldObject.Value = new Chest();
                }
            }
        }
    }
}