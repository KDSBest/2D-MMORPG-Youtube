using Common.Client.Interfaces;
using Common.Protocol.Character;

namespace Assets.Scripts.Character
{

	public interface ICurrentContext : ITokenProvider
	{
		CharacterInformation Character { get; set; }
	}
}
