using System.Numerics;

namespace Common.Protocol.Map.Interfaces
{
	public interface IMapStateMessage : IUdpPackage
	{
		string Name { get; set; }
		Vector2 Position { get; set; }
		long ServerTime { get; set; }
	}

	public interface IMapStateMessage<T> : IMapStateMessage
	{
		bool HasNoVisibleDifference(T state);
	}
}