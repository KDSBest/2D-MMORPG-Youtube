using Common;
using Common.Protocol;

namespace MapService.WorldManagement
{
	public class State
    {
        public IUdpPackage Data;
        public int Priority;
        public int CurrentPriority = 0;
        public Vector2Int Partition;
        public string Id;

        public State(string id, IUdpPackage data, int priority, Vector2Int partition)
        {
            Id = id;
            Data = data;
            Priority = priority;
			Partition = partition;
		}
    }
}
