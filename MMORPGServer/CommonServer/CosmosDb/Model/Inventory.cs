using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CommonServer.CosmosDb.Model
{
	public class Inventory
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		public Dictionary<string, int> Items = new Dictionary<string, int>();

		public DateTime LatestEventTimestamp { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
