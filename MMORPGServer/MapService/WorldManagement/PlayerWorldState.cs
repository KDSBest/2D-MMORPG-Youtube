using Common;
using ReliableUdp.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MapService.WorldManagement
{

	public class PlayerWorldState
    {
        private ConcurrentDictionary<string, State> statesManagement = new ConcurrentDictionary<string, State>();

        public State GetState(string id)
        {
            if (!statesManagement.ContainsKey(id))
                return null;

            return statesManagement[id];
        }

        public void AddState(string id, State state)
        {
            statesManagement.AddOrUpdate(id, state, (k, v) =>
            {
                v.Data = state.Data;
                v.Priority = state.Priority;
                return v;
            });
        }

        public void RemoveState(string id)
        {
            State notused;
            statesManagement.Remove(id, out notused);
        }

        public UdpDataWriter GetPackage(int maxSizeBytes)
        {
            var states = statesManagement.Values.ToList();
            for (int i = 0; i < states.Count; i++)
            {
                states[i].CurrentPriority += states[i].Priority;
            }

            states = states.OrderByDescending(x => x.CurrentPriority).ToList();

            UdpDataWriter writer = new UdpDataWriter();

            for (int i = 0; i < states.Count && writer.Length < maxSizeBytes; i++)
            {
                states[i].Data.Write(writer);
                states[i].CurrentPriority = 0;
            }

            return writer;
        }

		public List<State> GetState(Vector2Int partition)
		{
            // ToList needed because we want a cloned list
            // TODO: Optimize in own Dictionary so we don't have to traverse a dictionary (that is actually quite slow)
            return statesManagement.Values.Where(x => x.Partition == partition).ToList();
		}
	}
}
