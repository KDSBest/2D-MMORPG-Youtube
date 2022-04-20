using Common;
using Common.Protocol.Character;
using Common.Protocol.Map.Interfaces;
using System.Collections.Concurrent;

namespace CommonServer.WorldManagement
{

	public interface IPlayerWorldManagement
	{
		ConcurrentDictionary<string, CharacterInformation> Characters { get; }
		ConcurrentDictionary<string, IMapStateMessage> LastState { get; }
		ConcurrentDictionary<string, Vector2Int> LastStatePartition { get; }

		void Initialize();
		void Initialize(bool registerForEnemyStates, bool registerForSkillCasts);
	}
}
