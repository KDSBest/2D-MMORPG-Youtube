using Common.Protocol.Character;

namespace Assets.Scripts.Character
{
	public class CurrentContext : ICurrentContext
	{
		public string Token { get; set; } = string.Empty;

		public CharacterInformation Character { get; set; }
	}
}
