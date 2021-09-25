using System;
using System.Collections.Generic;
using System.Text;

namespace Common.GameDesign
{
	public class SkillProgressionTable : Dictionary<int, SkillStats>
	{
		public SkillStats GetStats(int level)
		{
			// TODO: Linear interpolate between empty levels
			return this[level];
		}
	}
}
