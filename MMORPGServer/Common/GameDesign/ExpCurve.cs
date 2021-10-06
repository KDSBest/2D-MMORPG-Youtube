using System;
using System.Collections.Generic;
using System.Text;

namespace Common.GameDesign
{
	public static class ExpCurve
	{
		public static int MaxLevel = 50;

		public static int CurveMod = 32;

		public static Dictionary<int, int> DiffExp = new Dictionary<int, int>();
		public static Dictionary<int, int> FullExp = new Dictionary<int, int>();

		static ExpCurve()
		{
			for (int i = 0; i < MaxLevel; i++)
			{
				int level = i + 1;
				int expNeededTillNow = i == 0 ? 0 : FullExp[level - 1];
				int expForCurrentLevel = CurveMod * level * level;
				DiffExp.Add(level, expForCurrentLevel);
				FullExp.Add(level, expNeededTillNow + expForCurrentLevel);
			}
		}
	}
}
