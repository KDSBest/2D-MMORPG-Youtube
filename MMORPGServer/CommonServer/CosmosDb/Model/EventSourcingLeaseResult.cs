using Newtonsoft.Json;
using System;

namespace CommonServer.CosmosDb.Model
{
	public class EventSourcingLeaseResult
	{
		public string Id { get; set; }
		public bool Acquired { get; set; }
		public DateTime LatestEvent { get; set; }
		public string AcquiredEtag { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
