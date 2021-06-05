using Common.Client.Interfaces;

namespace Common.Client
{
	public class TokenProvider : ITokenProvider
	{
		public string Token { get; set; }
	}
}
