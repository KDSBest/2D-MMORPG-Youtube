using Assets.Scripts.NPC;
using Common.Protocol.Character;
using Common.Protocol.Inventory;
using Common.QuestSystem;
using System.Collections.Generic;

namespace Assets.Scripts.Character
{
	public class CurrentContext : ICurrentContext
	{
		public string Token { get; set; } = string.Empty;

		public CharacterInformation Character { get; set; }

		public QuestTracking QuestTracking { get; set; }
		public Inventory Inventory { get; set; }

		public Dictionary<string, NPCController> NPC { get; set; } = new Dictionary<string, NPCController>();
		public PlayerController PlayerController { get; set; }
	}
}
