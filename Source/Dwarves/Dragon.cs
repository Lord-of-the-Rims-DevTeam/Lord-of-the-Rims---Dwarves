using RimWorld;
using Verse;

namespace Dwarves
{
    public class Dragon : Pawn
    {
        public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(dinfo, out absorbed);
            if (dinfo.Def == DamageDefOf.Burn ||
                dinfo.Def == DamageDefOf.Flame)
            {
                Log.Message("Absorbed flame damage");
                absorbed = true;
            }
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
    }
}