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
        public static void HarmonyCrops(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(Plant), "get_DyingBecauseExposedToLight"), null,
                new HarmonyMethod(typeof(HarmonyDwarves), nameof(get_DyingBecauseExposedToLight_PostFix)));
            harmony.Patch(AccessTools.Method(typeof(Plant), "get_GrowthRate"), null,
                new HarmonyMethod(typeof(HarmonyDwarves), nameof(get_GrowthRate_PostFix)));
        }

        //Reduces growth rate to one fourth
        public static void get_GrowthRate_PostFix(Plant __instance, ref float __result)
        {
            if (__instance.def.defName == "LotRD_PlantEarthBreadRoot")
            {
                if (__instance.Map.glowGrid.GameGlowAt(__instance.Position, true) > 0f)
                {
                    __result *= 0.5f;
                }
            }
        }

        //While Earthbread prefers caves, it does not die when exposed to sunlight
        public static void get_DyingBecauseExposedToLight_PostFix(Plant __instance, ref bool __result)
        {
            if (__instance.def.defName == "LotRD_PlantEarthBreadRoot")
            {
                __result = false;
            }
        }

    }
}