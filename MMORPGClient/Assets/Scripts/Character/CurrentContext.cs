using Common.Protocol.Character;
using Common.QuestSystem;

namespace Assets.Scripts.Character
{
	public class CurrentContext : ICurrentContext
	{
		public string Token { get; set; } = string.Empty;

		public CharacterInformation Character { get; set; }

		public QuestTracking QuestTracking { get; set; }
	}
}
