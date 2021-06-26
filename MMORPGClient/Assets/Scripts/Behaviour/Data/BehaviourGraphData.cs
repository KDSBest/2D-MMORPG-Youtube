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
		public List<CommentBlockData> Comments { get; set; } = new List<CommentBlockData>();
		public Dictionary<Guid, DialogNodeData> Nodes { get; set; } = new Dictionary<Guid, DialogNodeData>();
	}
}
