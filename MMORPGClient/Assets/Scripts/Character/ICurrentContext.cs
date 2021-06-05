using Common.Client.Interfaces;

namespace Assets.Scripts.Character
{

	public interface ICurrentContext : ITokenProvider
	{
		string Name { get; set; }
	}
}
