using Assets.Scripts.NPC;
using Common.Client.Interfaces;
using Common.Protocol.Character;
using Common.Protocol.Inventory;
using Common.QuestSystem;
using System.Collections.Generic;

namespace Assets.Scripts.Character
{

	public interface ICurrentContext : ITokenProvider
	{
		CharacterInformation Character { get; set; }
		QuestTracking QuestTracking { get; set; }
		Inventory Inventory { get; set; }
		Dictionary<string, NPCController> NPC { get; set; }
		PlayerController PlayerController { get; set; }
	}
}
