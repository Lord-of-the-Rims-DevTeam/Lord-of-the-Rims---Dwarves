using RimWorld;
using Verse;

namespace Dwarves
{
    public class Dragon : Pawn, IThoughtGiver
    {
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            if (dinfo.Def == DamageDefOf.Burn ||
                dinfo.Def == DamageDefOf.Flame)
            {
                Log.Message("Absorbed flame damage");
                absorbed = true;
            }
            //if (!this.InMentalState && dinfo.Instigator is Pawn p && p?.Faction == Faction.OfPlayerSilentFail)
            //{
            //    this.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, p.Label, true, false,
            //        null);
            //}
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            var hediffsToRemove = this?.health?.hediffSet?.hediffs.FindAll(x =>
                x.def == HediffDefOf.BadBack ||
                x.def == HediffDefOf.Cataract ||
                x.def == HediffDef.Named("HeartArteryBlockage") ||
                x.def == HediffDef.Named("Gunshot") ||
                x.def == HediffDef.Named("HearingLoss"));
            if (!hediffsToRemove.NullOrEmpty())
            {
                if (hediffsToRemove != null)
                    foreach (var hd in hediffsToRemove)
                    {
                        this?.health?.RemoveHediff(hd);
                    }
            }
        }

        public Thought_Memory GiveObservedThought()
        {
           
            Thought_MemoryObservation thought_MemoryObservation = null;
            if (this.Dead)
            {
                if (ThoughtDef.Named("LotRD_ObservedDragonDead") is ThoughtDef td)
                {
                    thought_MemoryObservation =
                        (Thought_MemoryObservation) ThoughtMaker.MakeThought(td);
                }
            }
            else if (this.InAggroMentalState)
            {
                if (ThoughtDef.Named("LotRD_ObservedDragonEnraged") is ThoughtDef td)
                {
                    thought_MemoryObservation =
                        (Thought_MemoryObservation) ThoughtMaker.MakeThought(td);   
                }
            }
            else
            {
                if (ThoughtDef.Named("LotRD_ObservedDragon") is ThoughtDef td)
                {
                    thought_MemoryObservation =
                        (Thought_MemoryObservation) ThoughtMaker.MakeThought(td);
                }
            }
            
            if (thought_MemoryObservation != null)
            {
                thought_MemoryObservation.Target = this;
                return thought_MemoryObservation;
            }

            return null;
        }
    }
}
