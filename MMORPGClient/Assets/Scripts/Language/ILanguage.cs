namespace Assets.Scripts.Language
{
	public interface ILanguage
	{
		string Starting { get; }
		string ConnectToLogin { get; }
		string ConnectToChat { get; }
		string ConnectToCombat { get; }
		string ConnectToPlayerEvent { get; }
		string ConnectToInventory { get; }
		string ConnectToCharacter { get; }
		string ConnectToGame { get; }
		string ConnectionFailed { get; }

	}
}