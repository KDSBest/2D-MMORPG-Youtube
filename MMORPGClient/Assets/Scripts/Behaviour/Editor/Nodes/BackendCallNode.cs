using Assets.Scripts.Behaviour.Data.Nodes;
using Assets.Scripts.Behaviour.Editor.Nodes.Interfaces;
using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Behaviour.Editor.Nodes
{
	public class BackendCallNode : BaseNode, ITextfieldNode
	{
		public string Text { get; set; }

		public TextField TextField { get; set; }
		public Port TruePort { get; set; }
		public Port FalsePort { get; set; }

		public BackendCallNodeData GetData()
		{
			var pos = this.GetPosition().position;
			return new BackendCallNodeData()
			{
				Guid = this.Guid,
				Position = new System.Numerics.Vector2(pos.x, pos.y),
				Call = this.Text,
				NextFalse = FalsePort.connected ? (FalsePort.connections.First().input.node as BaseNode).Guid : Guid.Empty,
				NextTrue = TruePort.connected ? (TruePort.connections.First().input.node as BaseNode).Guid : Guid.Empty
			};
		}

		public BackendCallNode() : base()
		{

		}

		public BackendCallNode(Vector2 position) : this()
		{
			this.title = "Node";
			this.Text = "Node";
			this.Guid = Guid.NewGuid();

			Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(BaseNode));
			inputPort.portName = "Input";
			this.InputPort = inputPort;
			this.inputContainer.Add(inputPort);

			Port trueOutputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BaseNode));
			trueOutputPort.portName = "True";
			this.TruePort = trueOutputPort;
			this.outputContainer.Add(trueOutputPort);

			Port falseOutputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BaseNode));
			falseOutputPort.portName = "False";
			this.FalsePort = falseOutputPort;
			this.outputContainer.Add(falseOutputPort);

			this.RefreshExpandedState();
			this.RefreshPorts();
			this.SetPosition(new Rect(position, new Vector2(200, 150)));

			var textField = new TextField("");
			textField.RegisterValueChangedCallback(evt =>
			{
				NodeHelper.SetNodeText(this, evt.newValue);
			});
			textField.SetValueWithoutNotify(this.title);
			this.TextField = textField;

			this.mainContainer.Add(textField);
		}

	}
}
