using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;

namespace spaciouscoopnbarn
{
    public class Compatibility
    {
        public string name;
        public string uniqueID;
    }

    internal class CompatibilityModulator
    {
        IModHelper helper;

        bool hasSVE = true;
        bool hasBKGCB = true;
        bool hasJMCB = true;
        bool hasUARC = true;

        public bool checkCompatibilities(IModHelper Helper)
        {
            helper = Helper;
            var compatibilities = helper.Data.ReadJsonFile<Dictionary<string, Compatibility>>(PathUtilities.NormalizePath("assets/Compatibilies.json"));
            if (compatibilities is null)
            {
                Log.Error("Compatibility list file not found!");
                return false;
            }
            foreach (var compatibility in compatibilities)
            {
                if (!helper.ModRegistry.IsLoaded("FlashShifter.StardewValleyExpandedCP"))
                {
                    Log.Trace($"Stardew Valley Expanded is not installed, skipping compatibility!");
                    hasSVE = false;
                }

                if (!helper.ModRegistry.IsLoaded("bobkalonger.gigacoopnbarn"))
                {
                    Log.Trace($"Gigantic Coop and Barn is not installed, skipping compatibility!");
                    hasBKGCB = false;
                }
            
                if (!helper.ModRegistry.IsLoaded("jenf1.megacoopbarn"))
                {
                    Log.Trace($"Jen's Mega Coop and Barn is not installed, skipping compatibility!");
                    hasJMCB = false;
                }

                if (!helper.ModRegistry.IsLoaded("UncleArya.ResourceChickens"))
                {
                    Log.Trace($"Resource Chickens is not installed, skipping compatibility!");
                    hasUARC = false;
                }
            }
        }
    }
}