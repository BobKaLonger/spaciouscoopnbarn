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

        public bool checkSVE(IModHelper Helper)
        {
            helper = Helper;
            var compatibilities = ;

            if (!helper.ModRegistry.IsLoaded("FlashShifter.StardewValleyExpandedCP"))
                hasSVE = false;

            if (!helper.ModRegistry.IsLoaded("bobkalonger.gigacoopnbarn"))
                hasBKGCB = false;
            else return hasBKGCB;

            if (!helper.ModRegistry.IsLoaded("jenf1.megacoopbarn"))
                hasJMCB = false;
            else return hasJMCB;

            if (!helper.ModRegistry.IsLoaded("UncleArya.ResourceChickens"))
                hasUARC = false;
            else return hasUARC;
        }
    }
}