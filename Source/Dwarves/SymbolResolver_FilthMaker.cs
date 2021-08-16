using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace Dwarves
{
    public class SymbolResolver_FilthMaker : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            var map = BaseGen.globalSettings.map;
            _ = map.terrainGrid;
            //TerrainDef newTerr = rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction, false);
            if (rp.streetHorizontal != null && rp.streetHorizontal.Value)
            {
                var ellipse = GetEllipse(rp.rect.minX, rp.rect.minZ, rp.rect.maxX, rp.rect.maxZ, true);
                foreach (var c in ellipse)
                {
                    if (rp.chanceToSkipFloor != null && Rand.Chance(rp.chanceToSkipFloor.Value))
                    {
                        continue;
                    }

                    //terrainGrid.SetTerrain(iterator.Current, newTerr);
                    if (rp.filthDef != null)
                    {
                        FilthMaker.TryMakeFilth(c, map, rp.filthDef,
                            rp.filthDensity == null ? 1 : Mathf.RoundToInt(rp.filthDensity.Value.RandomInRange));
                    }
                }
            }
            else
            {
                foreach (var rectCell in rp.rect.Cells)
                {
                    if (rp.chanceToSkipFloor != null && Rand.Chance(rp.chanceToSkipFloor.Value))
                    {
                        continue;
                    }

                    //terrainGrid.SetTerrain(iterator.Current, newTerr);
                    if (rp.filthDef != null)
                    {
                        FilthMaker.TryMakeFilth(rectCell, map, rp.filthDef,
                            rp.filthDensity == null ? 1 : Mathf.RoundToInt(rp.filthDensity.Value.RandomInRange));
                    }
                }
            }
        }

        //Original code from merthsoft. [[merthsoft's Designator Shapes (url: https://ludeon.com/forums/index.php?topic=37723.0)]]

        private static void swap<T>(ref T item1, ref T item2)
        {
            var t = item1;
            item1 = item2;
            item2 = t;
        }

        private static IEnumerable<IntVec3> GetEllipse(int x1, int z1, int x2, int z2, bool fill = false)
        {
            if (x2 < x1)
            {
                swap(ref x1, ref x2);
            }

            if (z2 < z1)
            {
                swap(ref z1, ref z2);
            }

            var num = (x2 - x1) / 2;
            var num2 = (z2 - z1) / 2;
            var x3 = x1 + num;
            var y3 = z1 + num2;
            return DrawEllipseUsingRadius(x3, y3, num, num2, fill);
        }

        private static IEnumerable<IntVec3> DrawEllipseUsingRadius(int x, int y, int xRadius, int yRadius,
            bool fill = false)
        {
            var hashSet = new HashSet<IntVec3>();
            var num = 0;
            var num2 = yRadius;
            var num8 = -(xRadius * xRadius) * num2;
            var num9 = 0;
            var num10 = -2 * xRadius * xRadius * num2;
            var num11 = 2 * yRadius * yRadius;
            var num12 = 2 * xRadius * xRadius;
            while (num2 >= 0 && num <= xRadius)
            {
                circlePlot(x, y, hashSet, num, num2);
                if (num8 + (yRadius * yRadius * num) <=
                    -((xRadius * xRadius / 4) + (xRadius % 2) + (yRadius * yRadius)) ||
                    num8 + (xRadius * xRadius * num2) <= -((yRadius * yRadius / 4) + (yRadius % 2)))
                {
                    incrementX(ref num, ref num9, ref num11, ref num8);
                }
                else if (num8 - (xRadius * xRadius * num2) >
                         -((yRadius * yRadius / 4) + (yRadius % 2) + (xRadius * xRadius)))
                {
                    incrementY(ref num2, ref num10, ref num12, ref num8);
                }
                else
                {
                    incrementX(ref num, ref num9, ref num11, ref num8);
                    circlePlot(x, y, hashSet, num, num2);
                    incrementY(ref num2, ref num10, ref num12, ref num8);
                }
            }

            if (fill)
            {
                return Fill(hashSet);
            }

            return hashSet;
        }

        private static IEnumerable<IntVec3> Fill(IEnumerable<IntVec3> outLine)
        {
            var hashSet = new HashSet<IntVec3>();
            foreach (var grouping in from vec in outLine
                group vec by vec.z)
            {
                if (grouping.Count() == 1)
                {
                    hashSet.Add(grouping.First());
                }
                else
                {
                    var intVec = grouping.First();
                    var intVec2 = grouping.Last();
                    hashSet.AddRange(DrawHorizontalLine(intVec.x, intVec2.x, intVec.y, grouping.Key).ToList());
                }
            }

            return hashSet;
        }

        private static IEnumerable<IntVec3> DrawHorizontalLine(int x1, int x2, int y, int z)
        {
            if (x1 > x2)
            {
                swap(ref x1, ref x2);
            }

            return from x in Enumerable.Range(x1, x2 - x1 + 1)
                select new IntVec3(x, y, z);
        }

        private static void circlePlot(int x, int y, HashSet<IntVec3> ret, int plotX, int plotY)
        {
            ret.Add(new IntVec3(x + plotX, 0, y + plotY));
            if (plotX != 0 || plotY != 0)
            {
                ret.Add(new IntVec3(x - plotX, 0, y - plotY));
            }

            if (plotX == 0 || plotY == 0)
            {
                return;
            }

            ret.Add(new IntVec3(x + plotX, 0, y - plotY));
            ret.Add(new IntVec3(x - plotX, 0, y + plotY));
        }

        private static void incrementX(ref int x, ref int dxt, ref int d2xt, ref int t)
        {
            x++;
            dxt += d2xt;
            t += dxt;
        }

        private static void incrementY(ref int y, ref int dyt, ref int d2yt, ref int t)
        {
            y--;
            dyt += d2yt;
            t += dyt;
        }
    }
}