using Assets.Scripts.Behaviour.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Behaviour.Editor
{

	public class BehaviourGraphView : GraphView
	{
		private BehaviourGraphData graph;
		private const string ENTRYPOINT = "ENTRYPOINT";
		private bool isUpdateGraph = false;

		public BehaviourGraphView(BehaviourGraphData graph)
		{
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

			this.AddManipulator(new ClickSelector());
			this.AddManipulator(new EdgeManipulator());
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			this.AddManipulator(new FreehandSelector());

			OnGraphChanged(graph);
		}

		public void OnGraphChanged(BehaviourGraphData graph)
		{
			isUpdateGraph = true;
			var allElements = this.graphElements.ToList();

			if(allElements.Count > 0)
			{
				this.DeleteElements(allElements);
			}

			if (graph.Nodes.Count == 0)
			{
				AddElement(GetEntryPointNodeInstance());
			}
			else
			{
				Dictionary<Guid, DialogNode> nodes = new Dictionary<Guid, DialogNode>();

				var entryPoint = GetEntryPointNodeInstance();
				entryPoint.SetPosition(new Rect(new Vector2(graph.Nodes[Guid.Empty].Position.X, graph.Nodes[Guid.Empty].Position.Y), entryPoint.GetPosition().size));
				AddElement(entryPoint);
				nodes.Add(Guid.Empty, entryPoint);

				foreach (var nodeData in graph.Nodes.Values.Where(x => x.Guid != null && x.Guid != Guid.Empty))
				{
					var node = CreateNode(new Vector2(nodeData.Position.X, nodeData.Position.Y));
					node.Guid = nodeData.Guid;
					SetNodeText(node, nodeData.Text);

					foreach (var nodeChoiceData in nodeData.Choices)
					{
						AddPort(node, nodeChoiceData.Text);
					}

					nodes.Add(node.Guid, node);
					AddElement(node);
				}

				foreach (var nodeData in graph.Nodes.Values)
				{
					var outputNode = nodes[nodeData.Guid];

					foreach(var choice in nodeData.Choices)
					{
						if (choice.GuidNext == Guid.Empty)
							continue;

						var outputPort = outputNode.OutputPort.First(x => x.portName == choice.Text);
						var edge = outputPort.ConnectTo(nodes[choice.GuidNext].InputPort);
						AddElement(edge);
					}
				}

				foreach (var cData in graph.Comments)
				{
					var comment = CreateCommentBlock(new Vector2(cData.Position.X, cData.Position.Y));
					comment.title = cData.Title;
					foreach(var subNode in cData.ChildNodes)
					{
						comment.AddElement(nodes[subNode]);
					}
				}
			}

			this.graph = graph;
			isUpdateGraph = false;
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List<Port>();
			var startPortView = startPort;

			ports.ForEach((port) =>
			{
				var portView = port;
				if (startPortView != portView && startPortView.node != portView.node)
					compatiblePorts.Add(port);
			});

			return compatiblePorts;
		}

		public void UpdateGraph()
		{
			if (isUpdateGraph)
				return;

			var allElements = this.graphElements.ToList();

			var dialogNodes = allElements.Where(x => x is DialogNode).Cast<DialogNode>().ToList();
			graph.Nodes.Clear();

			foreach (var dNode in dialogNodes)
			{
				var pos = dNode.GetPosition().position;
				var nodeData = new DialogNodeData()
				{
					Guid = dNode.Guid,
					Position = new System.Numerics.Vector2(pos.x, pos.y),
					Text = dNode.Text
				};

				foreach (var port in dNode.outputContainer.Children().Where(x => x is Port).Cast<Port>())
				{
					var choice = new ChoiceData();
					choice.Text = port.portName;
					choice.GuidNext = Guid.Empty;
					if (port.connected)
					{
						var otherNode = port.connections.First().input.node as DialogNode;
						if (otherNode != null)
						{
							choice.GuidNext = otherNode.Guid;
						}
					}
					nodeData.Choices.Add(choice);
				}

				graph.Nodes.Add(dNode.Guid, nodeData);
			}

			var comments = allElements.Where(x => x is CommentGroup).Cast<CommentGroup>().ToList();
			graph.Comments.Clear();

			foreach (var comment in comments)
			{
				var cData = new CommentBlockData();
				cData.Title = comment.title;
				var pos = comment.GetPosition().position;
				cData.Position = new System.Numerics.Vector2(pos.x, pos.y);

				foreach (var child in comment.containedElements.Where(x => x is DialogNode).Cast<DialogNode>())
				{
					cData.ChildNodes.Add(child.Guid);
				}
				graph.Comments.Add(cData);
			}

		}

		public CommentGroup CreateCommentBlock(Vector2 position)
		{
			var group = new CommentGroup
			{
				autoUpdateGeometry = true,
				title = "Comment"
			};
			group.SetPosition(new Rect(position, new Vector2(300, 200)));
			AddElement(group);

			UpdateGraph();

			return group;
		}

		public void CreateNewDialogueNode(Vector2 position)
		{
			AddElement(CreateNode(position));
			UpdateGraph();
		}

		public DialogNode CreateNode(Vector2 position)
		{
			var node = new DialogNode()
			{
				title = "Node",
				Text = "Node",
				Guid = Guid.NewGuid()
			};

			Port inputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogNode));
			inputPort.portName = "Input";
			node.InputPort = inputPort;
			node.inputContainer.Add(inputPort);

			node.RefreshExpandedState();
			node.RefreshPorts();
			node.SetPosition(new Rect(position, new Vector2(200, 150)));

			var textField = new TextField("");
			textField.multiline = true;
			textField.RegisterValueChangedCallback(evt =>
			{
				SetNodeText(node, evt.newValue);
			});
			textField.SetValueWithoutNotify(node.title);
			node.TextField = textField;

			node.mainContainer.Add(textField);

			var button = new Button(() => { AddPort(node); })
			{
				text = "Add Choice"
			};
			node.titleButtonContainer.Add(button);

			return node;
		}

		private static void SetNodeText(DialogNode node, string text)
		{
			node.Text = text;
			node.title = text.Split('\r', '\n')[0];
			node.TextField.SetValueWithoutNotify(text);
		}

		public void AddPort(DialogNode node, string name = null)
		{
			var newPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogNode));

			var outputPortCount = node.outputContainer.Children().Where(x => x is Port).Count();
			var outputPortName = string.IsNullOrEmpty(name) ? $"Choice {outputPortCount + 1}" : name;

			newPort.portName = outputPortName;

			var textField = new TextField()
			{
				name = string.Empty,
				value = outputPortName
			};
			textField.RegisterValueChangedCallback(evt => newPort.portName = evt.newValue);
			newPort.contentContainer.Add(textField);

			var deleteButton = new Button(() => RemovePort(node, newPort))
			{
				text = "Remove Choice"
			};
			newPort.contentContainer.Add(deleteButton);

			node.outputContainer.Add(newPort);
			node.OutputPort.Add(newPort);

			node.RefreshPorts();
			node.RefreshExpandedState();
			UpdateGraph();
		}

		private void RemovePort(DialogNode node, Port port)
		{
			var edgeToRemove = edges.ToList().FirstOrDefault(x => x.output.portName == port.portName && x.output.node == port.node);

			if (edgeToRemove != null)
			{
				edgeToRemove.input.Disconnect(edgeToRemove);
				RemoveElement(edgeToRemove);
			}

			node.outputContainer.Remove(port);
			node.OutputPort.Remove(port);

			node.RefreshPorts();
			node.RefreshExpandedState();

			UpdateGraph();
		}

		private DialogNode GetEntryPointNodeInstance()
		{
			var node = new DialogNode()
			{
				title = ENTRYPOINT,
				Guid = Guid.Empty,
				Text = ENTRYPOINT
			};

			var outputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogNode));
			outputPort.portName = "Next";
			node.outputContainer.Add(outputPort);
			node.OutputPort.Add(outputPort);

			node.capabilities &= ~Capabilities.Deletable;

			node.RefreshExpandedState();
			node.RefreshPorts();
			node.SetPosition(new Rect(100, 400, 100, 150));
			return node;
		}
	}
}
