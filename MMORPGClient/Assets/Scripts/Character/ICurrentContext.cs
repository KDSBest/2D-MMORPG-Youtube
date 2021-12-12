using Common.Client.Interfaces;
using Common.Protocol.Character;
using Common.QuestSystem;

namespace Assets.Scripts.Character
{

	public interface ICurrentContext : ITokenProvider
	{
		CharacterInformation Character { get; set; }
		QuestTracking QuestTracking { get; set; }
	}
}
