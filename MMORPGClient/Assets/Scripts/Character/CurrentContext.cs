namespace Assets.Scripts.Character
{
	public class CurrentContext : ICurrentContext
	{
		public string Name { get; set; } = string.Empty;
		public string Token { get; set; } = string.Empty;
	}
}
