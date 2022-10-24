using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Dwarves
{
    public static partial class HarmonyDwarves
    {
        public static void HarmonyMisc(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(Pawn_GeneTracker), "get_CanHaveBeard"), null,
                new HarmonyMethod(typeof(HarmonyDwarves), nameof(get_FemaleDwarvesCanHaveBeards_PostFix)));

            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), name: "DrawHeadHair"),
                null, null,
                transpiler: new HarmonyMethod(typeof(HarmonyDwarves), nameof(BeardCheckTranspiler)));
        }

        //Dwarves should always have their beards visible no matter what
        public static IEnumerable<CodeInstruction> BeardCheckTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            FieldInfo fullHeadInfo = AccessTools.Field(typeof(BodyPartGroupDefOf), nameof(BodyPartGroupDefOf.FullHead));
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                if (i > 0 && instructionList[i - 1].OperandIs(fullHeadInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(PawnRenderer), name: "pawn"));
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(HarmonyDwarves), nameof(NotADwarf)));
                    yield return new CodeInstruction(OpCodes.And);
                }
            }
        }
        private static bool NotADwarf(Pawn pawn)
        {
            return pawn.def.defName != "LotRD_DwarfStandardRace";
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