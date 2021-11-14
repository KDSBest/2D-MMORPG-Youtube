using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Protocol.Inventory
{
	public class Inventory
	{
		public Dictionary<string, int> Items = new Dictionary<string, int>();

		public int GetAmount(string itemId)
		{
			if (!Items.ContainsKey(itemId))
				return 0;

			return Items[itemId];
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
