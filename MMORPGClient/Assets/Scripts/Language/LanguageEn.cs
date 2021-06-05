namespace Assets.Scripts.Language
{
	public class LanguageEn : ILanguage
	{
		public string Starting { get; } = "Starting...";

		public string ConnectToLogin { get; } = "Connect to Login Server.";
		public string ConnectToCharacter { get; } = "Connect to Character Server.";
		public string ConnectToChat { get; } = "Connect to Chat Server.";

		public string ConnectToGame { get; } = "Connect to Game.";

		public string ConnectionFailed { get; } = "Connection failed!";
		public string EncryptionHandshake { get; } = "Make Connection to Backend Secure.";
	}
}
