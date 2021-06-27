using Assets.Scripts.Behaviour.Data.Nodes;
using Assets.Scripts.Behaviour.Editor.Nodes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Behaviour.Editor.Nodes
{

	public class DialogNode : BaseNode, ITextfieldNode
	{
		public string Text { get; set; }

		public TextField TextField { get; set; }
		public List<Port> OutputPort { get; set; } = new List<Port>();

		private GraphView graphView;

		public DialogNode(GraphView graphView) : base()
		{
			this.graphView = graphView;
		}

		public DialogNode(GraphView graphView, Vector2 position) : this(graphView)
		{
			this.title = "Node";
			this.Text = "Node";
			this.Guid = Guid.NewGuid();

			Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(BaseNode));
			inputPort.portName = "Input";
			this.InputPort = inputPort;
			this.inputContainer.Add(inputPort);

			this.RefreshExpandedState();
			this.RefreshPorts();
			this.SetPosition(new Rect(position, new Vector2(200, 150)));

			var textField = new TextField("");
			textField.multiline = true;
			textField.RegisterValueChangedCallback(evt =>
			{
				NodeHelper.SetNodeText(this, evt.newValue);
			});
			textField.SetValueWithoutNotify(this.title);
			this.TextField = textField;

			this.mainContainer.Add(textField);

			var button = new Button(() => { AddPort(); })
			{
				text = "Add Choice"
			};
			this.titleButtonContainer.Add(button);
		}

		public void AddPort(string name = null)
		{
			var newPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BaseNode));

			var outputPortCount = this.outputContainer.Children().Where(x => x is Port).Count();
			var outputPortName = string.IsNullOrEmpty(name) ? $"Choice {outputPortCount + 1}" : name;

			newPort.portName = outputPortName;

			var textField = new TextField()
			{
				name = string.Empty,
				value = outputPortName
			};
			textField.RegisterValueChangedCallback(evt => newPort.portName = evt.newValue);
			newPort.contentContainer.Add(textField);

			var deleteButton = new Button(() => RemovePort(newPort))
			{
				text = "Remove Choice"
			};
			newPort.contentContainer.Add(deleteButton);

			this.outputContainer.Add(newPort);
			this.OutputPort.Add(newPort);

			this.RefreshPorts();
			this.RefreshExpandedState();
		}

		private void RemovePort(Port port)
		{
			var edgeToRemove = graphView.edges.ToList().FirstOrDefault(x => x.output.portName == port.portName && x.output.node == port.node);

			if (edgeToRemove != null)
			{
				edgeToRemove.input.Disconnect(edgeToRemove);
				graphView.RemoveElement(edgeToRemove);
			}

			this.outputContainer.Remove(port);
			this.OutputPort.Remove(port);

			this.RefreshPorts();
			this.RefreshExpandedState();
		}

		public DialogNodeData GetData()
		{
			var pos = this.GetPosition().position;
			var nodeData = new DialogNodeData()
			{
				Guid = this.Guid,
				Position = new System.Numerics.Vector2(pos.x, pos.y),
				Text = this.Text
			};

			foreach (var port in OutputPort)
			{
				var choice = new ChoiceData();
				choice.Text = port.portName;
				choice.GuidNext = Guid.Empty;
				if (port.connected)
				{
					var otherNode = port.connections.First().input.node as BaseNode;
					if (otherNode != null)
					{
						choice.GuidNext = otherNode.Guid;
					}
				}
				nodeData.Choices.Add(choice);
			}

			return nodeData;
		}
	}
}
