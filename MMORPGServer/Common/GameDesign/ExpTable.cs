using System;
using System.Collections.Generic;
using System.Text;

namespace Common.GameDesign
{
	public static class ExpTable
	{
		public static Dictionary<EnemyType, int> Prop = new Dictionary<EnemyType, int>()
		{
			{ EnemyType.Flower, 5 }
		};
	}
}
