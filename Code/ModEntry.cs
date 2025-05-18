using HarmonyLib;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Enumerable = System.Linq.Enumerable;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.LocationContexts;
using StardewValley.TokenizableStrings;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

using xTile.Dimensions;
using xTile.ObjectModel;

namespace spaciouscoopnbarn
{
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

            TouchActionProperties.Enable(helper, Monitor);

            var harmony = new Harmony(this.ModManifest.UniqueID);

            ActionProperties.ApplyPatch(harmony, Monitor);
            HarmonyPatch_TMXLLoadMapFacingDirection.ApplyPatch(harmony, Monitor);

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(Building), nameof(Building.doesTileHaveProperty))]
        public static class BuildingDeluxeBarnDoorCursorPatch
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
            }
        }

        [HarmonyPatch(typeof(Building), nameof(Building.doAction))]
        public static class BuildingDeluxeBarnDoorPatch
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
            }
        }
        [HarmonyPatch(typeof(Building), nameof(Building.updateInteriorWarps))]
        public static class BuildingDeluxeBarnWarpPatch
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