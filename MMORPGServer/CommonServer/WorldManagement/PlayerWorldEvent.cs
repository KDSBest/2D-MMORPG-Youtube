using Common;

namespace CommonServer.WorldManagement
{
	public class PlayerWorldEvent<T>
	{
		public T State;
		public Vector2Int OldPartition;
		public Vector2Int NewPartition;

		public PlayerWorldEvent(T state, Vector2Int newPartition, Vector2Int oldPartition)
		{
			State = state;
			OldPartition = oldPartition;
			NewPartition = newPartition;
		}
	}
}
