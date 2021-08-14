using Common.Protocol.PlayerEvent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonServer.CosmosDb.Model
{

	public class InventoryEvent
	{
		[JsonProperty(PropertyName = "id")]
		public Guid Id { get; set; }

		[JsonProperty(PropertyName = "playerId")]
		public string PlayerId { get; set; }

		public PlayerEventType Type { get; set; } = PlayerEventType.DailyLogin;

		public Dictionary<string, int> Add { get; set; } = new Dictionary<string, int>();
		public Dictionary<string, int> Remove { get; set; } = new Dictionary<string, int>();

		public DateTime CreationDate { get; set; } = DateTime.UtcNow;

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
