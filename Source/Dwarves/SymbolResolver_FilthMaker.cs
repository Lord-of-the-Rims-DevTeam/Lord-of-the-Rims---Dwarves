using System;
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
        
		//Original code from merthsoft. [[merthsoft's Designator Shapes (url: https://ludeon.com/forums/index.php?topic=37723.0)]]

	    #region MyRegion
		private static void swap<T>(ref T item1, ref T item2)
		{
			T t = item1;
			item1 = item2;
			item2 = t;
		}
		public static IEnumerable<IntVec3> GetEllipse(int x1, int y1, int z1, int x2, int y2, int z2, bool fill = false)
		{
			if (x2 < x1)
			{
				swap<int>(ref x1, ref x2);
			}
			if (z2 < z1)
			{
				swap<int>(ref z1, ref z2);
			}
			int num = (x2 - x1) / 2;
			int num2 = (z2 - z1) / 2;
			int x3 = x1 + num;
			int y3 = z1 + num2;
			return DrawEllipseUsingRadius(x3, y3, num, num2, fill);
		}
		public static IEnumerable<IntVec3> DrawEllipseUsingRadius(int x, int y, int xRadius, int yRadius, bool fill = false)
		{
			HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
			int num = 0;
			int num2 = yRadius;
			int num3 = xRadius * xRadius;
			int num4 = yRadius * yRadius;
			int num5 = -(num3 / 4 + xRadius % 2 + num4);
			int num6 = -(num4 / 4 + yRadius % 2 + num3);
			int num7 = -(num4 / 4 + yRadius % 2);
			int num8 = -num3 * num2;
			int num9 = 2 * num4 * num;
			int num10 = -2 * num3 * num2;
			int num11 = 2 * num4;
			int num12 = 2 * num3;
			while (num2 >= 0 && num <= xRadius)
			{
				circlePlot(x, y, hashSet, num, num2);
				if (num8 + num4 * num <= num5 || num8 + num3 * num2 <= num7)
				{
					incrementX(ref num, ref num9, ref num11, ref num8);
				}
				else if (num8 - num3 * num2 > num6)
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

		public static IEnumerable<IntVec3> Fill(IEnumerable<IntVec3> outLine)
		{
			HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
			foreach (IGrouping<int, IntVec3> grouping in from vec in outLine
				group vec by vec.z)
			{
				if (grouping.Count<IntVec3>() == 1)
				{
					hashSet.Add(grouping.First<IntVec3>());
				}
				else
				{
					grouping.OrderBy((IntVec3 v) => v.x);
					IntVec3 intVec = grouping.First<IntVec3>();
					IntVec3 intVec2 = grouping.Last<IntVec3>();
					hashSet.AddRange(DrawHorizontalLine(intVec.x, intVec2.x, intVec.y, grouping.Key).ToList());
				}
			}
			return hashSet;
		}
		
		public static IEnumerable<IntVec3> DrawHorizontalLine(int x1, int x2, int y, int z)
		{
			if (x1 > x2)
			{
				swap<int>(ref x1, ref x2);
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
			if (plotX != 0 && plotY != 0)
			{
				ret.Add(new IntVec3(x + plotX, 0, y - plotY));
				ret.Add(new IntVec3(x - plotX, 0, y + plotY));
			}
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
	    #endregion merthsoft
        
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            TerrainGrid terrainGrid = map.terrainGrid;
            //TerrainDef newTerr = rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction, false);
	        if (rp.streetHorizontal != null && rp.streetHorizontal.Value == true)
	        {
		        var ellipse = GetEllipse(rp.rect.minX, 0, rp.rect.minZ, rp.rect.maxX, 0, rp.rect.maxZ, true);
		        foreach (var c in ellipse)
		        {
			        if (rp.chanceToSkipFloor == null || !Rand.Chance(rp.chanceToSkipFloor.Value))
			        {
				        //terrainGrid.SetTerrain(iterator.Current, newTerr);
				        if (rp.filthDef != null)
				        {
					        FilthMaker.MakeFilth(c, map, rp.filthDef, (rp.filthDensity == null) ? 1 : Mathf.RoundToInt(rp.filthDensity.Value.RandomInRange));
				        }
			        }
		        }   
	        }
	        else
	        {
		        CellRect.CellRectIterator iterator = rp.rect.GetIterator();

		        while (!iterator.Done())
		        {
			        if (rp.chanceToSkipFloor == null || !Rand.Chance(rp.chanceToSkipFloor.Value))
			        {
				        //terrainGrid.SetTerrain(iterator.Current, newTerr);
				        if (rp.filthDef != null)
				        {
					        FilthMaker.MakeFilth(iterator.Current, map, rp.filthDef, (rp.filthDensity == null) ? 1 : Mathf.RoundToInt(rp.filthDensity.Value.RandomInRange));
				        }
			        }
			        iterator.MoveNext();
		        }   
	        }
        }
    }
}
