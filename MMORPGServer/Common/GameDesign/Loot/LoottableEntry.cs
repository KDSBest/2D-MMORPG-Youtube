using System.Collections.Generic;

namespace Common.GameDesign.Loot
{
	public class LoottableEntry
	{
		public int Weight = 100;

		public List<LoottableEntryItemDrop> Loots = new List<LoottableEntryItemDrop>();
	}
}
