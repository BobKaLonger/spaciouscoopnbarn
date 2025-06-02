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
            
        }
    }
}