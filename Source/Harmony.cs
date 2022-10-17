using HarmonyLib;
using Verse;

namespace Dwarves
{
    [StaticConstructorOnStartup]
    public static partial class HarmonyDwarves
    {
        static HarmonyDwarves()
        {
            var harmony = new Harmony("rimworld.lotr.dwarves");
            HarmonyCrops(harmony);
            HarmonyFactions(harmony);
            HarmonyFood(harmony);
            HarmonyMisc(harmony);
        }
    }
}