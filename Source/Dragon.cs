using RimWorld;
using Verse;

namespace Dwarves
{
    public class Dragon : Pawn, IObservedThoughtGiver
    {
        public Thought_Memory GiveObservedThought(Pawn pawn)
        {
            Thought_MemoryObservation thought_MemoryObservation = null;
            if (Dead)
            {
                if (ThoughtDef.Named("LotRD_ObservedDragonDead") is { } td)
                {
                    thought_MemoryObservation =
                        (Thought_MemoryObservation) ThoughtMaker.MakeThought(td);
                }
            }
            else if (InAggroMentalState)
            {
                if (ThoughtDef.Named("LotRD_ObservedDragonEnraged") is { } td)
                {
                    thought_MemoryObservation =
                        (Thought_MemoryObservation) ThoughtMaker.MakeThought(td);
                }
            }
            else
            {
                if (ThoughtDef.Named("LotRD_ObservedDragon") is { } td)
                {
                    thought_MemoryObservation =
                        (Thought_MemoryObservation) ThoughtMaker.MakeThought(td);
                }
            }

            if (thought_MemoryObservation == null)
            {
                return null;
            }

            thought_MemoryObservation.Target = this;
            return thought_MemoryObservation;
        }

        public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
        {
            return null;
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            if (dinfo.Def == DamageDefOf.Burn ||
                dinfo.Def == DamageDefOf.Flame)
            {
                Log.Message("Absorbed flame damage");
                absorbed = true;
            }

            if (!InMentalState && dinfo.Instigator is Pawn pawn && pawn.Faction == Faction.OfPlayerSilentFail)
            {
                mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, pawn.Label, true);
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            var hediffsToRemove = health?.hediffSet?.hediffs.FindAll(x =>
                x.def == HediffDefOf.BadBack ||
                x.def == HediffDefOf.Cataract ||
                x.def == HediffDef.Named("HeartArteryBlockage") ||
                x.def == HediffDef.Named("Gunshot") ||
                x.def == HediffDef.Named("HearingLoss"));
            if (hediffsToRemove.NullOrEmpty())
            {
                return;
            }

            if (hediffsToRemove == null)
            {
                return;
            }

            foreach (var hd in hediffsToRemove)
            {
                health?.RemoveHediff(hd);
            }
        }
    }
}