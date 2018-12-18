using RimWorld;
using Verse;

namespace Dwarves
{
    class ThingWithComps_Glower : ThingWithComps
    {
        public Building_StreetLamp master = null;

        public override void Draw()
        {
        }

        public override void Tick()
        {
            base.Tick();
            CheckNeedsDestruction();
            CheckNeedsFlick();
        }

        public void CheckNeedsDestruction()
        {
            if (master != null && this.Spawned)
            {
                if (!master.Spawned)
                {
                    this.Destroy(0);
                    return;
                }

            }
        }

        public void CheckNeedsFlick()
        {
            if (master == null) return;
            CompFlickable masterflickable = master.TryGetComp<CompFlickable>();
            CompFlickable flickable = this.TryGetComp<CompFlickable>();

            if (masterflickable.SwitchIsOn != flickable.SwitchIsOn)
            {
                flickable.DoFlick();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Building_StreetLamp>(ref this.master, "master", false);
        }
    }
}
