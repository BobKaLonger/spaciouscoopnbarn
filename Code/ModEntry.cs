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
using System.IO;
using Newtonsoft.Json;

namespace spaciouscoopnbarn
{
    //Holds config info
    public class ModConfig
    {
        public bool HasSVE { get; set; }
        public bool HasBKGCB { get; set; }
        public bool HasJMCB { get; set; }
        public bool HasUARC { get; set; }
        public string SpaciousMode { get; set; }
    }
    public class ModEntry : Mod
    {
        public static Mod modInstance;
        public static IContentPack cpPack;

        public override void Entry(IModHelper helper)
        {
            ModEntry.modInstance = this;

            I18n.Init(Helper.Translation);

            var mi = Helper.ModRegistry.Get("bobkalonger.spaciouscoopnbarnCP");
            cpPack = mi.GetType().GetProperty("ContentPack")?.GetValue(mi) as IContentPack;

            //Check for compatible mods
            bool hasSVE = helper.ModRegistry.IsLoaded("FlashShifter.StardewValleyExpandedCP");
            bool hasBKGCB = helper.ModRegistry.IsLoaded("bobkalonger.gigacoopnbarn");
            bool hasJMCB = helper.ModRegistry.IsLoaded("jenf1.megacoopbarn");
            bool hasUARC = helper.ModRegistry.IsLoaded("UncleArya.ResourceChickens");

            // Determine the spaciousMode
            string spaciousMode;
            if (hasSVE)
            {
                spaciousMode = "SVE";
            }
            else if (hasBKGCB)
            {
                spaciousMode = "BKGCB";
            }
            else if (hasJMCB)
            {
                if (hasUARC)
                {
                    spaciousMode = "Both";
                }
                else
                {
                    spaciousMode = "JMCB";
                }
            }
            else if (hasUARC)
            {
                //By the time the logic gets here, we already know JMCB is false
                spaciousMode = "UARC";
            }
            else
            {
                spaciousMode = "Vanilla";
            }

            //Let's make that config now
            ModConfig config = new ModConfig
            {
                HasSVE = hasSVE,
                HasBKGCB = hasBKGCB,
                HasJMCB = hasJMCB,
                HasUARC = hasUARC,
                SpaciousMode = spaciousMode
            };

            //Path to the content patcher config file
            string spaciousFolder = Path.GetFullPath(Path.Combine(helper.DirectoryPath, ".."));
            string configPath = Path.Combine(spaciousFolder, "[CP] Spacious Coop and Barn", "config.json");
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(configPath, json);

            helper.Events.Player.Warped += PlayerOnWarped;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void PlayerOnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation == null)
                return;

            foreach (var b in e.NewLocation.buildings)
            {
                if (b.buildingType.Value == "bobkalonger.spaciouscoopnbarnCP_SpaciousBarn")
                {
                    Point tileLoc = new(b.tileX.Value + 3, b.tileY.Value + 3);
                    var l = new LightSource($"spacious_SpaciousBarnLight_{b.tileX.Value}_{b.tileY.Value}_1", 4, tileLoc.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                    Game1.currentLightSources.Add(l.Id, l);

                    tileLoc = new(b.tileX.Value + 8, b.tileY.Value + 3);
                    l = new LightSource($"spacious_SpaciousBarnLight_{b.tileX.Value}_{b.tileY.Value}_2", 4, tileLoc.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                    Game1.currentLightSources.Add(l.Id, l);
                }

                if (b.buildingType.Value == "bobkalonger.spaciouscoopnbarnCP_SpaciousCoop")
                {
                    Point tileLoc = new(b.tileX.Value + 6, b.tileY.Value + 2);
                    var l = new LightSource($"spacious_SpaciousCoopLight_{b.tileX.Value}_{b.tileY.Value}_1", 4, tileLoc.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                    Game1.currentLightSources.Add(l.Id, l);
                }

                if (b.buildingType.Value == "FlashShifter.StardewValleyExpandedCP_PremiumCoop")
                {
                    Point tileLoc = new(b.tileX.Value + 6, b.tileY.Value + 2);
                    var l = new LightSource($"SVE_PremiumCoopLight_{b.tileX.Value}_{b.tileY.Value}_1", 4, tileLoc.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                    Game1.currentLightSources.Add(l.Id, l);
                }
            }
        }

        [HarmonyPatch(typeof(Building), nameof(Building.doesTileHaveProperty))]
        public static class SpaciousBuildingsCursorPatch
        {
            public static void Postfix(Building __instance, int tile_x, int tile_y, string property_name, string layer_name, ref string property_value, ref bool __result)
            {
                if (__instance.buildingType.Value == "bobkalonger.spaciouscoopnbarnCP_SpaciousBarn" && __instance.daysOfConstructionLeft.Value <= 0)
                {
                    var interior = __instance.GetIndoors();
                    if (tile_x == __instance.tileX.Value + __instance.humanDoor.X + 8 &&
                        tile_y == __instance.tileY.Value + __instance.humanDoor.Y &&
                        interior != null)
                    {
                        if (property_name == "Action")
                        {
                            property_value = "meow";
                            __result = true;
                        }
                    }
                }

                if (__instance.buildingType.Value == "bobkalonger.spaciouscoopnbarnCP_SpaciousCoop" && __instance.daysOfConstructionLeft.Value <= 0)
                {
                    var interior = __instance.GetIndoors();
                    if (tile_x == __instance.tileX.Value + __instance.humanDoor.X - 2 &&
                        tile_y == __instance.tileY.Value + __instance.humanDoor.Y - 2 &&
                        interior != null)
                    {
                        if (property_name == "Action")
                        {
                            property_value = "meow";
                            __result = true;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Building), nameof(Building.doAction))]
        public static class SpaciousBuildingsDoorPatch
        {
            public static void Postfix(Building __instance, Vector2 tileLocation, Farmer who, ref bool __result)
            {
                if (who.ActiveObject != null && who.ActiveObject.IsFloorPathItem() && who.currentLocation != null && !who.currentLocation.terrainFeatures.ContainsKey(tileLocation))
                {
                    return;
                }

                if (__instance.buildingType.Value == "bobkalonger.spaciouscoopnbarnCP_SpaciousBarn" && __instance.daysOfConstructionLeft.Value <= 0)
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

                if (__instance.buildingType.Value == "bobkalonger.spaciouscoopnbarnCP_SpaciousCoop" && __instance.daysOfConstructionLeft.Value <= 0)
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
                            Game1.warpFarmer(interior.NameOrUniqueName, interior.warps[1].X + 1, interior.warps[1].Y, Game1.player.FacingDirection, isStructure);
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
                if (__instance.buildingType.Value != "bobkalonger.spaciouscoopnbarnCP_SpaciousBarn")
                    return;
                if (interior == null || interior.warps.Count == 0)
                    return;

                var w = interior.warps[1];
                interior.warps[1] = new(w.X, w.Y, w.TargetName, w.TargetX + 8, w.TargetY, w.flipFarmer.Value, w.npcOnly.Value);
            }
        }

        public static class SpaciousCoopWarpPatch
        {
            public static void Postfix(Building __instance, GameLocation interior)
            {
                if (__instance.buildingType.Value != "bobkalonger.spaciouscoopnbarnCP_SpaciousBarn")
                    return;
                if (interior == null || interior.warps.Count == 0)
                    return;

                var w = interior.warps[1];
                interior.warps[1] = new(w.X, w.Y, w.TargetName, w.TargetX - 2, w.TargetY - 2, w.flipFarmer.Value, w.npcOnly.Value);
            }
        }

        [HarmonyPatch(typeof(Utility), "_HasBuildingOrUpgrade")]
        public static class UtilityHasCoopBarnPatch
        {
            public static void Postfix(GameLocation location, string buildingId, ref bool __result)
            {
                string toCheck = null;
                if (buildingId == "Coop" || buildingId == "Deluxe Coop" || buildingId == "Big Coop" || buildingId == "FlashShifter.StardewValleyExpandedCP_PremiumCoop")
                {
                    toCheck = "bobkalonger.spaciouscoopnbarnCP_SpaciousCoop";
                }
                else if (buildingId == "Barn" || buildingId == "Deluxe Barn" || buildingId == "Big Barn" || buildingId == "FlashShifter.StardewValleyExpandedCP_PremiumBarn")
                {
                    toCheck = "bobkalonger.spaciouscoopnbarnCP_SpaciousBarn";
                }

                if (!__result && toCheck != null)
                {
                    if (location.getNumberBuildingsConstructed(toCheck) > 0)
                    {
                        __result = true;
                    }
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
            if (__instance.buildingType.Value != "bobkalonger.spaciouscoopnbarnCP_SpaciousCoop" &&
                __instance.buildingType.Value != "bobkalonger.spaciouscoopnbarnCP_SpaciousBarn")
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