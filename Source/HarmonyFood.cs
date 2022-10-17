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
        public static void HarmonyFood(Harmony harmony)
        {
            //Dwarven raw food ideo moodlets
            harmony.Patch(
                original: AccessTools.Method(type: typeof(FoodUtility), name: nameof(FoodUtility.ThoughtsFromIngesting)), prefix: null,
                postfix: new HarmonyMethod(methodType: typeof(HarmonyDwarves), methodName: nameof(ThoughtsFromIngestingDwarvenFood_PostFix)));
            
            //Dwarven cooked food ideo moodlets
            harmony.Patch(original: AccessTools.Method(type: typeof(FoodUtility), name: "AddIngestThoughtsFromIngredient"), prefix: null,
                postfix: new HarmonyMethod(methodType: typeof(HarmonyDwarves), methodName: nameof(AddIngestThoughtsFromDwarvenIngredient_PostFix)));
            
        }
        
        public static bool IsDwarvenFood(ThingDef foodDef)
        {
          
            //Otherwise...
            switch (foodDef.defName)
            {
                case "LotRD_RawEarthBread":
                case "LotRD_PlantEarthBreadRoot":
                case "LotRD_RawHoneyroot":
                case "LotRD_PlantHoneyroot":
                case "LotRD_Mead": 
                case "Meat_Pig": //Delicious pork!
                case "Meat_Goat": //Mountain animal
                case "Meat_Ibex": //Mountain animal
                case "Meat_Yak": //Mountain animal
                    return true;
                default:
                    return false;
            }
            

        }

        //Eating raw Dwarven food is appreciated by those with the precept
        public static void ThoughtsFromIngestingDwarvenFood_PostFix(ref List<FoodUtility.ThoughtFromIngesting> __result,
            Pawn ingester, Thing foodSource, ThingDef foodDef)
        {
            if (ModsConfig.IdeologyActive && foodDef.thingCategories != null)
            {
                var meatSourceCategory = foodSource.def.IsCorpse
                    ? FoodUtility.GetMeatSourceCategoryFromCorpse(thing: foodSource)
                    : FoodUtility.GetMeatSourceCategory(source: foodDef);

                //Is it raw plant food?
                if (foodDef.thingCategories.Contains(item: ThingCategoryDefOf.PlantFoodRaw) && !IsDwarvenFood(foodDef: foodDef))
                {
                    AccessTools.Method(type: typeof(FoodUtility), name: "AddThoughtsFromIdeo").Invoke(obj: null,
                        parameters: new object[]
                        {
                            DefDatabase<HistoryEventDef>.GetNamed(defName: "LotRD_AteNonDwarvenFoodPlant"), ingester,
                            foodDef, meatSourceCategory
                        });
                }

                //Raw meat is not Dwarven food...
                if (foodDef?.thingCategories?.FirstOrDefault(predicate: x => x == ThingCategoryDefOf.MeatRaw) !=
                    null)
                    return;
                
                //Is it Dwarven food?
                if (IsDwarvenFood(foodDef: foodDef))
                {
               
                    //Pass it on to ideology for buffs/debuffs
                    AccessTools.Method(type: typeof(FoodUtility), name: "AddThoughtsFromIdeo").Invoke(obj: null,
                        parameters: new object[]
                        {
                            DefDatabase<HistoryEventDef>.GetNamed(defName: "LotRD_AteDwarvenFood"), ingester, foodDef,
                            meatSourceCategory
                        });
                }
            }

            __result = (List<FoodUtility.ThoughtFromIngesting>)AccessTools.Field(type: typeof(FoodUtility), name: "ingestThoughts")
                .GetValue(obj: null);
        }

        //Eating cooked Dwarven food is appreciated by those with the precept
        private static void AddIngestThoughtsFromDwarvenIngredient_PostFix(ThingDef ingredient, Pawn ingester,
            List<FoodUtility.ThoughtFromIngesting> ingestThoughts)
        {
            if (ingredient.ingestible == null)
            {
                return;
            }

            MeatSourceCategory meatSourceCategory = FoodUtility.GetMeatSourceCategory(source: ingredient);

            //Is it a raw plant?
            if (ModsConfig.IdeologyActive && ingredient.thingCategories != null &&
                ingredient.thingCategories.Contains(item: ThingCategoryDefOf.PlantFoodRaw) && !IsDwarvenFood(foodDef: ingredient))
            {
                AccessTools.Method(type: typeof(FoodUtility), name: "AddThoughtsFromIdeo").Invoke(obj: null,
                    parameters: new object[]
                    {
                        DefDatabase<HistoryEventDef>.GetNamed(defName: "LotRD_AteNonDwarvenFoodMealWithPlants"), ingester, ingredient,
                        meatSourceCategory
                    });
                return;
            }
            
            //Is it Dwarven food?
            if (IsDwarvenFood(foodDef: ingredient))
            {
                //Pass it on to ideology for buffs/debuffs
                AccessTools.Method(type: typeof(FoodUtility), name: "AddThoughtsFromIdeo").Invoke(obj: null,
                    parameters: new object[]
                    {
                        DefDatabase<HistoryEventDef>.GetNamed(defName: "LotRD_AteDwarvenFoodAsIngredient"), ingester, ingredient,
                        meatSourceCategory
                    });
            }
        }
    }
}