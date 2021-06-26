using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Assets.Scripts.Behaviour.Editor
{
	public class DialogNode : Node
	{
		public string Text { get; set; }

		public Guid Guid { get; set; }

		public Port InputPort { get; set; }
		public List<Port> OutputPort { get; set; } = new List<Port>();
		public TextField TextField { get; set; }
	}
}
