using Common.Protocol.PlayerEvent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonServer.CosmosDb.Model
{

	public class PlayerEvent
	{
		[JsonProperty(PropertyName = "id")]
		public Guid Id { get; set; }

		[JsonProperty(PropertyName = "playerId")]
		public string PlayerId { get; set; }

		public PlayerEventType Type { get; set; } = PlayerEventType.DailyLogin;

		public DateTime CreationDate { get; set; } = DateTime.UtcNow;

		[JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
		public int? TimeToLive { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
