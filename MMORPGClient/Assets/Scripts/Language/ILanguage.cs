namespace Assets.Scripts.Language
{
	public interface ILanguage
	{
		string Starting { get; }
		string ConnectToLogin { get; }
		string ConnectToChat { get; }
		string ConnectionFailed { get; }
		string EncryptionHandshake { get; }
	}
}