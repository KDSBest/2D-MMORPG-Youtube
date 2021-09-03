using System.Numerics;

namespace Common.Protocol.Map.Interfaces
{
	public interface IPartitionMessage : IUdpPackage
	{
		Vector2 Position { get; set; }
	}
}