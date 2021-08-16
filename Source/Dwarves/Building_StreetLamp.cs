using RimWorld;
using Verse;

namespace Dwarves
{
    internal class Building_StreetLamp : Building
    {
        private readonly ThingDef glowerDef = ThingDef.Named("LotRD_GasLampGlower");
        private CompBreakdownable compBreakdownable;
        private ThingWithComps_Glower glower;

        private void SpawnGlower()
        {
            var thing = ThingMaker.MakeThing(glowerDef);
            var position = Position + GenAdj.CardinalDirections[0]
                                    + GenAdj.CardinalDirections[0];
            GenPlace.TryPlaceThing(thing, position, Map, ThingPlaceMode.Near);
            glower = thing as ThingWithComps_Glower;
            if (glower != null)
            {
                glower.master = this;
            }
        }

        private void DespawnGlower()
        {
            glower.master = null;
            glower.DeSpawn();
            glower = null;
        }

        private void ResolveGlower()
        {
            if (compBreakdownable == null)
            {
                return;
            }

            if (compBreakdownable.BrokenDown)
            {
                if (glower != null)
                {
                    DespawnGlower();
                }

                return;
            }

            if (glower == null)
            {
                SpawnGlower();
            }
        }

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map, bla);
            compBreakdownable = this.TryGetComp<CompBreakdownable>();
        }

        public override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(60))
            {
                ResolveGlower();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref glower, "glower");
        }
    }
}