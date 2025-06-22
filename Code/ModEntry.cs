using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Reflection;
using StardewValley.Buildings;
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
        internal const string SpaciousCP = "bobkalonger.spaciouscoopnbarnCP_";
        internal const string SVExpandCP = "FlashShifter.StardewValleyExpandedCP_";
        internal const string SpaciousBarn = $"{SpaciousCP}SpaciousBarn";
        internal const string SpaciousCoop = $"{SpaciousCP}SpaciousCoop";
        private const string spaciousPremiumCoop = $"{SVExpandCP}PremiumCoop";
        private const string spaciousPremiumBarn = $"{SVExpandCP}PremiumBarn";

        public override void Entry(IModHelper helper)
        {
            modInstance = this;

            I18n.Init(Helper.Translation);

            var mi = Helper.ModRegistry.Get("bobkalonger.spaciouscoopnbarnCP");
            cpPack = mi.GetType().GetProperty("ContentPack")?.GetValue(mi) as IContentPack;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            helper.Events.Player.Warped += PlayerOnWarped;

            var harmony = new Harmony(ModManifest.UniqueID);

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
            {
                return Array.Empty<string>();
            }
            return new[] {ComputeSpaciousMode()};
        }
        private string ComputeSpaciousMode()
        {
            bool hasSVE = Helper.ModRegistry.IsLoaded("FlashShifter.StardewValleyExpandedCP");
            bool hasBKGCB = Helper.ModRegistry.IsLoaded("bobkalonger.gigacoopnbarn");
            bool hasJMCB = Helper.ModRegistry.IsLoaded("jenf1.megacoopbarn");
            bool hasUARC = Helper.ModRegistry.IsLoaded("UncleArya.ResourceChickens");

            // Determine the spaciousMode
            if (hasSVE)
            {
                return "SVE";
            }
            else if (hasBKGCB)
            {
                return "BKGCB";
            }
            else if (hasJMCB)
            {
                if (hasUARC)
                {
                    return "Both";
                }
                else
                {
                    return "JMCB";
                }
            }
            else if (hasUARC)
            {
                //By the time the logic gets here, we already know JMCB is false
                return "UARC";
            }
            else
            {
                return "Vanilla";
            }
        }

        private void PlayerOnWarped(object sender, WarpedEventArgs e)
        {
            RemoveCustomlights(e.OldLocation);

            foreach (var b in e.NewLocation.buildings)
            {
                if (b.buildingType.Value == SpaciousBarn)
                {
                    var spaciousLightBL = new Point(b.tileX.Value + 3, b.tileY.Value + 3);
                    var ll = new LightSource($"{SpaciousCP}BarnLight_{b.tileX.Value}_{b.tileY.Value}_L", 4, spaciousLightBL.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                    Game1.currentLightSources.Add(ll.Id, ll);

                    var spaciousLightBR = new Point(b.tileX.Value + 8, b.tileY.Value + 3);
                    var lr = new LightSource($"{SpaciousCP}BarnLight_{b.tileX.Value}_{b.tileY.Value}_R", 4, spaciousLightBR.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                    Game1.currentLightSources.Add(lr.Id, lr);

                    /*if (b.daysUntilUpgrade.Value > 0)
                    {
                        var signTile = new Rectangle(16, 80, 16, 32);
                        float signX = b.tileX.Value + 4.5f;
                        float signY = b.tileY.Value + 0.5f;
                        var signPosition = new Vector2(signX, signY) * Game1.tileSize;

                        Helper.Events.Display.RenderedWorld += (s, args) =>
                        {
                            // Only draw if still under construction and player is on the same location
                            if (b.daysUntilUpgrade.Value > 0 && Game1.currentLocation == e.NewLocation)
                            {
                                // Use the default buildings texture
                                var texture = Game1.mouseCursors;
                                args.SpriteBatch.Draw(
                                    texture,
                                    Game1.GlobalToLocal(signPosition),
                                    signTile,
                                    Color.White,
                                    0f,
                                    Vector2.Zero,
                                    Game1.pixelZoom,
                                    Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
                                    (signPosition.Y + 32) / 10000f // draw above floor
                                );
                            }
                        };
                    }*/
                }

                if (b.buildingType.Value == SpaciousCoop)
                {
                    var spaciousLightC = new Point(b.tileX.Value + 6, b.tileY.Value + 2);
                    var lc = new LightSource($"{SpaciousCP}CoopLight_{b.tileX.Value}_{b.tileY.Value}", 4, spaciousLightC.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                    Game1.currentLightSources.Add(lc.Id, lc);

                    /* if (b.daysUntilUpgrade.Value > 0)
                    {
                        var signTile = new Rectangle(16, 80, 16, 32);
                        // Use floats for more precise placement
                        float signX = b.tileX.Value + 0.5f;
                        float signY = b.tileY.Value + 2.5f;
                        var signPosition = new Vector2(signX, signY) * Game1.tileSize;

                        Helper.Events.Display.RenderedWorld += (s, args) =>
                        {
                            // Only draw if still under construction and player is on the same location
                            if (b.daysUntilUpgrade.Value > 0 && Game1.currentLocation == e.NewLocation)
                            {
                                // Use the default buildings texture
                                var texture = Game1.mouseCursors;
                                args.SpriteBatch.Draw(
                                    texture,
                                    Game1.GlobalToLocal(signPosition),
                                    signTile,
                                    Color.White,
                                    0f,
                                    Vector2.Zero,
                                    Game1.pixelZoom,
                                    Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
                                    (signPosition.Y + 32) / 10000f // draw above floor
                                );
                            }
                        };
                    } */

                    if (b.buildingType.Value == spaciousPremiumCoop)
                    {
                        var spaciousLightPCP = new Point(b.tileX.Value + 6, b.tileY.Value + 2);
                        var lp = new LightSource($"{SpaciousCP}PremiumCoopLight_patch_{b.tileX.Value}_{b.tileY.Value}", 4, spaciousLightPCP.ToVector2() * Game1.tileSize, 1f, Color.Black, LightSource.LightContext.None);
                        Game1.currentLightSources.Add(lp.Id, lp);
                    }
                }
            }
        }

        private static void RemoveCustomlights(GameLocation location)
        {
            if (location == null || !Context.IsWorldReady)
                return;

            foreach (var light in Game1.currentLightSources.Keys)
            {
                if (light.StartsWith("{SpaciousCP}"))
                {
                    Game1.currentLightSources.Remove(light);
                }
            }
        }



        [HarmonyPatch(typeof(Building), nameof(Building.doesTileHaveProperty))]
        public static class SpaciousCursorPatch
        {
            public static void Postfix(Building __instance, int tile_x, int tile_y, string property_name, ref string property_value, ref bool __result)
            {
                if (__instance.buildingType.Value == SpaciousBarn && __instance.daysUntilUpgrade.Value <= 0)
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
                if (__instance.buildingType.Value == SpaciousCoop && __instance.daysUntilUpgrade.Value <= 0)
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
        public static class SpaciousDoorPatch
        {
            public static void Postfix(Building __instance, Vector2 tileLocation, Farmer who, ref bool __result)
            {
                if (who.ActiveObject != null && who.ActiveObject.IsFloorPathItem() && who.currentLocation != null && !who.currentLocation.terrainFeatures.ContainsKey(tileLocation))
                {
                    return;
                }

                if (__instance.buildingType.Value == SpaciousBarn && __instance.daysUntilUpgrade.Value <= 0)
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
                if (__instance.buildingType.Value == SpaciousCoop && __instance.daysUntilUpgrade.Value <= 0)
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
                if (__instance.buildingType.Value != SpaciousBarn)
                    return;
                if (interior == null || interior.warps.Count == 0)
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
                if (__instance.buildingType.Value != SpaciousCoop)
                    return;
                if (interior == null || interior.warps.Count == 0)
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
                string toCheck = null;
                if (buildingId == "Coop" || buildingId == "Big Coop" || buildingId == "Deluxe Coop" || buildingId == spaciousPremiumCoop)
                {
                    toCheck = SpaciousCoop;
                }
                else if (buildingId == "Barn" || buildingId == "Big Barn" || buildingId == "Deluxe Barn" || buildingId == spaciousPremiumBarn)
                {
                    toCheck = SpaciousBarn;
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

        [HarmonyPatch(typeof(Building), nameof(Building.InitializeIndoor))]
        public static class BuildingAutoGrabberFix
        {
            public static void Postfix(Building __instance, bool forUpgrade)
            {
                if (!forUpgrade)
                    return;
                if (__instance.buildingType.Value != SpaciousCoop &&
                    __instance.buildingType.Value != SpaciousBarn)
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
}