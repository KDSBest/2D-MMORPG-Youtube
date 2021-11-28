using Assets.Scripts.Behaviour.Data.Nodes;
using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Assets.Scripts.Behaviour.Editor.Nodes
{
	public class EntryPointNode : BaseNode
	{
		public Port Start { get; set; }

		public EntryPointNode() : this(new Vector2(100, 400))
		{

		}

		public EntryPointNode(Vector2 position)
		{
			this.title = "ENTRYPOINT";
			this.Guid = Guid.Empty;

			Start = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BaseNode));
			Start.portName = "Next";
			this.outputContainer.Add(Start);

			this.capabilities &= ~Capabilities.Deletable;

			this.RefreshExpandedState();
			this.RefreshPorts();
			this.SetPosition(new Rect(position.x, position.y, 100, 150));
		}

		public EntryPointNodeData GetData()
		{
			var pos = this.GetPosition().position;
			return new EntryPointNodeData()
			{
				Guid = this.Guid,
				Position = new System.Numerics.Vector2(pos.x, pos.y),
				Start = Start.connected ? (Start.connections.First().input.node as BaseNode).Guid : Guid.Empty
			};
		}
	}
}
