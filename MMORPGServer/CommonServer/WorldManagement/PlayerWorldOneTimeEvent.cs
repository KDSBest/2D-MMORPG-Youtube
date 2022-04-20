using Common;

namespace CommonServer.WorldManagement
{
	public class PlayerWorldOneTimeEvent<T>
	{
		public T State;
		public Vector2Int Partition;

		public PlayerWorldOneTimeEvent(T state, Vector2Int partition)
		{
			State = state;
			Partition = partition;
		}
	}
}
