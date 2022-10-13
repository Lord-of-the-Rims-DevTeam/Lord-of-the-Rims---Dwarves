using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Dwarves
{
    [StaticConstructorOnStartup]
    public class Building_FermentingMeadBarrel : Building
    {
        public const int MaxCapacity = 20;

        private const int BaseFermentationDuration = 600000;

        public const float MinIdealTemperature = 7f;

        private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

        private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);

        private static readonly Color BarFermentedColor = new Color(0.9f, 0.85f, 0.2f);

        private static readonly Material BarUnfilledMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

        private Material barFilledCachedMat;

        private float progressInt;

        private int wortCount;

        private float Progress
        {
            get => progressInt;
            set
            {
                if (value == progressInt)
                {
                    return;
                }

                progressInt = value;
                barFilledCachedMat = null;
            }
        }

        private Material BarFilledMat
        {
            get
            {
                if (barFilledCachedMat == null)
                {
                    barFilledCachedMat =
                        SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(BarZeroProgressColor, BarFermentedColor,
                            Progress));
                }

                return barFilledCachedMat;
            }
        }

        public int SpaceLeftForWort
        {
            get
            {
                if (Fermented)
                {
                    return 0;
                }

                return 20 - wortCount;
            }
        }

        private bool Empty => wortCount <= 0;

        public bool Fermented => !Empty && Progress >= 1f;

        private float CurrentTempProgressSpeedFactor
        {
            get
            {
                var compProperties = def.GetCompProperties<CompProperties_TemperatureRuinable>();
                var ambientTemperature = AmbientTemperature;
                if (ambientTemperature < compProperties.minSafeTemperature)
                {
                    return 0.1f;
                }

                if (ambientTemperature < 7f)
                {
                    return GenMath.LerpDouble(compProperties.minSafeTemperature, 7f, 0.1f, 1f, ambientTemperature);
                }

                return 1f;
            }
        }

        private float ProgressPerTickAtCurrentTemp => 1.66666666E-06f * CurrentTempProgressSpeedFactor;

        private int EstimatedTicksLeft =>
            Mathf.Max(Mathf.RoundToInt((1f - Progress) / ProgressPerTickAtCurrentTemp), 0);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref wortCount, "wortCount");
            Scribe_Values.Look(ref progressInt, "progress");
        }

        public override void TickRare()
        {
            base.TickRare();
            if (!Empty)
            {
                Progress = Mathf.Min(Progress + (200f * ProgressPerTickAtCurrentTemp), 1f);
            }
        }

        private void AddWort(int count)
        {
            GetComp<CompTemperatureRuinable>().Reset();
            if (Fermented)
            {
                Log.Warning("Tried to add mead wort to a barrel full of mead. Colonists should take the mead first.");
                return;
            }

            var num = Mathf.Min(count, 20 - wortCount);
            if (num <= 0)
            {
                return;
            }

            Progress = GenMath.WeightedAverage(0f, num, Progress, wortCount);
            wortCount += num;
        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "RuinedByTemperature")
            {
                Reset();
            }
        }

        private void Reset()
        {
            wortCount = 0;
            Progress = 0f;
        }

        public void AddWort(Thing wort)
        {
            var num = Mathf.Min(wort.stackCount, 20 - wortCount);
            if (num <= 0)
            {
                return;
            }

            AddWort(num);
            wort.SplitOff(num).Destroy();
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }

            var comp = GetComp<CompTemperatureRuinable>();
            if (!Empty && !comp.Ruined)
            {
                stringBuilder.AppendLine(Fermented
                    ? "LotRD_ContainsMead".Translate(wortCount, 20)
                    : "LotRD_ContainsWortMead".Translate(wortCount, 20));
            }

            if (!Empty)
            {
                if (Fermented)
                {
                    stringBuilder.AppendLine("Fermented".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("FermentationProgress".Translate(Progress.ToStringPercent(),
                        EstimatedTicksLeft.ToStringTicksToPeriod()));
                    if (CurrentTempProgressSpeedFactor != 1f)
                    {
                        stringBuilder.AppendLine(
                            "FermentationBarrelOutOfIdealTemperature".Translate(CurrentTempProgressSpeedFactor
                                .ToStringPercent()));
                    }
                }
            }

            stringBuilder.AppendLine("Temperature".Translate() + ": " + AmbientTemperature.ToStringTemperature("F0"));
            stringBuilder.AppendLine(string.Concat("IdealFermentingTemperature".Translate(), ": ",
                7f.ToStringTemperature("F0"), " ~ ", comp.Props.maxSafeTemperature.ToStringTemperature("F0")));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public Thing TakeOutMead()
        {
            if (!Fermented)
            {
                Log.Warning("Tried to get beer but it's not yet fermented.");
                return null;
            }

            var thing = ThingMaker.MakeThing(ThingDef.Named("LotRD_Mead"));
            thing.stackCount = wortCount;
            Reset();
            return thing;
        }

        public override void Draw()
        {
            base.Draw();
            if (Empty)
            {
                return;
            }

            var drawPos = DrawPos;
            drawPos.y += 0.046875f;
            drawPos.z += 0.25f;
            GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
            {
                center = drawPos,
                size = BarSize,
                fillPercent = wortCount / 20f,
                filledMat = BarFilledMat,
                unfilledMat = BarUnfilledMat,
                margin = 0.1f,
                rotation = Rot4.North
            });
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (Prefs.DevMode && !Empty)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to 1",
                    action = delegate { Progress = 1f; }
                };
            }
        }
    }
}