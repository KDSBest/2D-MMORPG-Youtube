using Assets.Scripts.Behaviour.Data.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Behaviour.Data
{
	public class BehaviourGraphData
	{
		public List<IBehaviourGraphProperty> Properties { get; set; } = new List<IBehaviourGraphProperty>();
		public List<GroupData> Groups { get; set; } = new List<GroupData>();
		public Dictionary<Guid, BaseNodeData> Nodes { get; set; } = new Dictionary<Guid, BaseNodeData>();
	}
}
