namespace MapService.WorldManagement
{

	public interface IPlayerWorldManagement
	{
		void Initialize();
		void OnDisconnectedPlayer(string name);
	}
}
