using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace spaciouscoopnbarn
{
    internal static class Log
    {
        internal static IMonitor Monitor { get; private set; } = null;
        internal static void Error(string msg) => Monitor.Log(msg, LogLevel.Error);
        internal static void Trace(string msg) => Monitor.Log(msg, LogLevel.Trace);
    }

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
        bool hasVanilla = true;

        public static void AutoCheck(IModHelper Helper, IMonitor Monitor)
        {
            if (new spaciouscoopnbarn.CompatibilityModulator().checkCompatibilities(Helper, Monitor))
                Monitor.Log(Helper.Translation.Get("compatibilitymodulator.success"), LogLevel.Info);
        }

        public bool checkCompatibilities(IModHelper Helper, IMonitor Monitor)
        {
            helper = Helper;
            var compatibilities = helper.Data.ReadJsonFile<Dictionary<string, Compatibility>>(PathUtilities.NormalizePath("assets/Compatibilities.json"));

            foreach (var compatibility in compatibilities.Values)
            {
                if ($"{compatibility.uniqueID}" == "FlashShifter.StardewValleyExpandedCP" && !helper.ModRegistry.IsLoaded("FlashShifter.StardewValleyExpandedCP"))
                {
                    hasSVE = false;
                }
                else
                {
                    return hasSVE;
                }
                /*if ($"{compatibility.uniqueID}" == "bobkalonger.gigacoopnbarn" && !helper.ModRegistry.IsLoaded("bobkalonger.gigacoopnbarn"))
                    hasBKGCB = false;

                if ($"{compatibility.uniqueID}" == "jenf1.megacoopbarn" && !helper.ModRegistry.IsLoaded("jenf1.megacoopbarn"))
                    hasJMCB = false;

                if ($"{compatibility.uniqueID}" == "UncleArya.ResourceChickens" && !helper.ModRegistry.IsLoaded("UncleArya.ResourceChickens"))
                    hasUARC = false;*/
            }

            /*if (hasSVE)
                return hasSVE;

            if (!hasSVE && hasBKGCB)
                return hasBKGCB;

            if (!hasSVE && !hasBKGCB && hasJMCB)
            {
                if (hasUARC)
                    return hasJMCB && hasUARC;

                else
                    return hasJMCB;
            }

            if (!hasSVE && !hasBKGCB && !hasJMCB)
            {
                if (hasUARC)
                    return hasUARC;
            }
            else
            {
                return hasVanilla;
            }*/
        }
    }
}