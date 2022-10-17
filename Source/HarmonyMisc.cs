using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Dwarves
{
    public static partial class HarmonyDwarves
    {
        public static void HarmonyMisc(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(Pawn_GeneTracker), "get_CanHaveBeard"), null,
                    new HarmonyMethod(typeof(HarmonyDwarves), nameof(get_FemaleDwarvesCanHaveBeards_PostFix)));
        }

        //Female dwarves should always be able to grow beards
        public static void get_FemaleDwarvesCanHaveBeards_PostFix(Pawn_GeneTracker __instance, ref bool __result)
        {
            if (__instance.pawn?.def?.defName == "LotRD_DwarfStandardRace")
            {
                __result = true;
            }
        }
    }
}