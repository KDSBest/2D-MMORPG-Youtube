using Common.Protocol;

namespace MapService.WorldManagement
{
	public class State
    {
        public IUdpPackage Data;
        public int Priority;
        public int CurrentPriority = 0;

        public State(IUdpPackage data, int priority)
        {
            Data = data;
            Priority = priority;
        }
    }
}
