using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Assets.Scripts.Behaviour.Editor.Nodes
{

	public class BaseNode : Node
	{
		public Guid Guid { get; set; }

		public Port InputPort { get; set; }
	}
}
