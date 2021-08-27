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
		public string DisplayName { get; set; }

		public TextField TextField { get; set; }
		public TextField TextFieldDisplayName { get; set; }
		public List<ConditionPort> OutputPort { get; set; } = new List<ConditionPort>();

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

			this.TextFieldDisplayName = new TextField("");
			this.TextFieldDisplayName.RegisterValueChangedCallback(evt =>
			{
				this.DisplayName = evt.newValue;
				NodeHelper.SetNodeText(this, this.Text, this.DisplayName + ": ");
			});

			this.mainContainer.Add(new Label("Display Name:"));
			this.mainContainer.Add(TextFieldDisplayName);

			this.TextField = new TextField("");
			this.TextField.multiline = true;
			this.TextField.RegisterValueChangedCallback(evt =>
			{
				NodeHelper.SetNodeText(this, evt.newValue, this.DisplayName + ": ");
			});
			this.TextField.SetValueWithoutNotify(this.title);

			this.mainContainer.Add(new Label("Dialog Text:"));
			this.mainContainer.Add(this.TextField);

			var button = new Button(() => { AddPort(); })
			{
				text = "Add Choice"
			};
			this.titleButtonContainer.Add(button);
		}

		public void AddPort(string name = null, string condition = "true")
		{
			var newPort = new ConditionPort()
			{
				Port = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BaseNode))
			};

			var outputPortCount = this.outputContainer.Children().Where(x => x is Port).Count();
			var outputPortName = string.IsNullOrEmpty(name) ? $"Choice {outputPortCount + 1}" : name;

			newPort.Port.portName = outputPortName;

			var textField = new TextField()
			{
				name = string.Empty,
				value = outputPortName
			};
			textField.RegisterValueChangedCallback(evt => newPort.Port.portName = evt.newValue);
			newPort.Port.contentContainer.Add(textField);
			newPort.Port.contentContainer.Add(new Label("Answer:"));

			var conditionTextField = new TextField()
			{
				name = string.Empty,
				value = condition
			};
			conditionTextField.RegisterValueChangedCallback(evt => newPort.Condition = evt.newValue);
			newPort.Port.contentContainer.Add(conditionTextField);
			newPort.Port.contentContainer.Add(new Label("Condition:"));

			var deleteButton = new Button(() => RemovePort(newPort))
			{
				text = "Remove Choice"
			};
			newPort.Port.contentContainer.Add(deleteButton);

			this.outputContainer.Add(newPort.Port);
			this.OutputPort.Add(newPort);

			this.RefreshPorts();
			this.RefreshExpandedState();
		}

		private void RemovePort(ConditionPort port)
		{
			var edgeToRemove = graphView.edges.ToList().FirstOrDefault(x => x.output.portName == port.Port.portName && x.output.node == port.Port.node);

			if (edgeToRemove != null)
			{
				edgeToRemove.input.Disconnect(edgeToRemove);
				graphView.RemoveElement(edgeToRemove);
			}

			this.outputContainer.Remove(port.Port);
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
				Position = new UnityEngine.Vector2(pos.x, pos.y),
				Text = this.Text,
				Name = this.DisplayName
			};

			foreach (var port in OutputPort)
			{
				var choice = new ChoiceData();
				choice.Text = port.Port.portName;
				choice.Condition = port.Condition;
				choice.GuidNext = Guid.Empty;
				if (port.Port.connected)
				{
					var otherNode = port.Port.connections.First().input.node as BaseNode;
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
