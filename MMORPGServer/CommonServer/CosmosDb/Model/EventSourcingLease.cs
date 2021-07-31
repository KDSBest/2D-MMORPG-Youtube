using Newtonsoft.Json;
using System;

namespace CommonServer.CosmosDb.Model
{
	public class EventSourcingLease
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		public DateTime LeasedUntil { get; set; }

		public DateTime LastCreatedEventTimestamp { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
