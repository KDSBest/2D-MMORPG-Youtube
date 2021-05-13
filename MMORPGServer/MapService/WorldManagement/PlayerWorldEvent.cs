namespace MapService.WorldManagement
{
	public class PlayerWorldEvent<T>
	{
		public T State;
		public MapPartition OldPartition;
		public MapPartition NewPartition;

		public PlayerWorldEvent(T state, MapPartition newPartition, MapPartition oldPartition)
		{
			State = state;
			OldPartition = oldPartition;
			NewPartition = newPartition;
		}
	}
}
