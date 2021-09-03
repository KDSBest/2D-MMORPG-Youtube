namespace Common.Protocol.Map.Interfaces
{

	public interface IMapStateMessage : IPartitionMessage
	{
		string Name { get; set; }
		long ServerTime { get; set; }
	}

	public interface IMapStateMessage<T> : IMapStateMessage
	{
		bool HasNoVisibleDifference(T state);
	}
}