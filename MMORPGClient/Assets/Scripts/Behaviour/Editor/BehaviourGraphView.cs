using Assets.Scripts.Behaviour.Data;
using Assets.Scripts.Behaviour.Data.Nodes;
using Assets.Scripts.Behaviour.Editor.Nodes;
using Assets.Scripts.Behaviour.Editor.Nodes.Interfaces;
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

			if (allElements.Count > 0)
			{
				this.DeleteElements(allElements);
			}

			if (graph.Nodes.Count == 0)
			{
				AddElement(new EntryPointNode());
			}
			else
			{
				Dictionary<Guid, BaseNode> nodes = new Dictionary<Guid, BaseNode>();

				var entryPoint = new EntryPointNode(new Vector2(graph.Nodes[Guid.Empty].Position.X, graph.Nodes[Guid.Empty].Position.Y));
				AddElement(entryPoint);
				nodes.Add(Guid.Empty, entryPoint);

				// nodes
				foreach (var nodeData in graph.Nodes.Values.Where(x => x.Guid != null && x.Guid != Guid.Empty))
				{
					switch (nodeData)
					{
						case DialogNodeData dNodeData:
							CreateDialogNodeFromData(nodes, nodeData, dNodeData);
							break;
						case BackendCallNodeData bNodeData:
							CreateBackendCallNodeFromData(nodes, nodeData, bNodeData);
							break;
					}
				}

				// edges
				foreach (var nodeData in graph.Nodes.Values)
				{
					switch (nodeData)
					{
						case EntryPointNodeData eNodeData:
							Connect(nodes, eNodeData);
							break;
						case DialogNodeData dNodeData:
							Connect(nodes, dNodeData);
							break;
						case BackendCallNodeData bNodeData:
							Connect(nodes, bNodeData);
							break;
					}
				}

				// comments
				foreach (var cData in graph.Groups)
				{
					var comment = CreateGroup(new Vector2(cData.Position.X, cData.Position.Y));
					comment.title = cData.Title;
					foreach (var subNode in cData.ChildNodes)
					{
						comment.AddElement(nodes[subNode]);
					}
				}
			}

			this.graph = graph;
			isUpdateGraph = false;
		}

		private void Connect(Dictionary<Guid, BaseNode> nodes, BackendCallNodeData nodeData)
		{
			var outputNode = nodes[nodeData.Guid] as BackendCallNode;

			ConnectNodes(nodes, outputNode.TruePort, nodeData.NextTrue);
			ConnectNodes(nodes, outputNode.FalsePort, nodeData.NextFalse);
		}

		private void Connect(Dictionary<Guid, BaseNode> nodes, EntryPointNodeData nodeData)
		{
			var outputNode = nodes[nodeData.Guid] as EntryPointNode;
			ConnectNodes(nodes, outputNode.Start, nodeData.Start);
		}

		private void Connect(Dictionary<Guid, BaseNode> nodes, DialogNodeData nodeData)
		{
			var outputNode = nodes[nodeData.Guid] as DialogNode;

			foreach (var choice in nodeData.Choices)
			{
				var outputPort = outputNode.OutputPort.First(x => x.Port.portName == choice.Text);
				ConnectNodes(nodes, outputPort.Port, choice.GuidNext);
			}
		}

		private void ConnectNodes(Dictionary<Guid, BaseNode> nodes, Port outputPort, Guid nextNode)
		{
			if (nextNode == Guid.Empty)
				return;

			var edge = outputPort.ConnectTo(nodes[nextNode].InputPort);
			AddElement(edge);
		}

		private void CreateDialogNodeFromData(Dictionary<Guid, BaseNode> nodes, BaseNodeData nodeData, DialogNodeData dNodeData)
		{
			var node = new DialogNode(this, new Vector2(nodeData.Position.X, nodeData.Position.Y));
			node.Guid = nodeData.Guid;
			node.DisplayName = dNodeData.Name;
			node.TextFieldDisplayName.SetValueWithoutNotify(node.DisplayName);

			NodeHelper.SetNodeText(node, dNodeData.Text, node.DisplayName + ": ");

			foreach (var nodeChoiceData in dNodeData.Choices)
			{
				node.AddPort(nodeChoiceData.Text, nodeChoiceData.Condition);
			}

			nodes.Add(node.Guid, node);
			AddElement(node);
		}

		private void CreateBackendCallNodeFromData(Dictionary<Guid, BaseNode> nodes, BaseNodeData nodeData, BackendCallNodeData bNodeData)
		{
			var bNode = new BackendCallNode(new Vector2(nodeData.Position.X, nodeData.Position.Y));
			bNode.Guid = nodeData.Guid;
			NodeHelper.SetNodeText(bNode, bNodeData.Call);

			nodes.Add(bNode.Guid, bNode);
			AddElement(bNode);
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

			graph.Nodes.Clear();

			UpdateEntryPointNodes(allElements);
			UpdateGraphDialogNodes(allElements);
			UpdateGraphBackendCallNodes(allElements);

			graph.Groups.Clear();
			UpdateGraphComments(allElements);
		}

		private void UpdateGraphComments(List<GraphElement> allElements)
		{
			var comments = allElements.Where(x => x is CommentGroup).Cast<CommentGroup>().ToList();

			foreach (var comment in comments)
			{
				graph.Groups.Add(comment.GetData());
			}
		}

		private void UpdateEntryPointNodes(List<GraphElement> allElements)
		{
			var dialogNodes = allElements.Where(x => x is EntryPointNode).Cast<EntryPointNode>().ToList();

			foreach (var node in dialogNodes)
			{
				graph.Nodes.Add(node.Guid, node.GetData());
			}
		}

		private void UpdateGraphBackendCallNodes(List<GraphElement> allElements)
		{
			var dialogNodes = allElements.Where(x => x is BackendCallNode).Cast<BackendCallNode>().ToList();

			foreach (var node in dialogNodes)
			{
				graph.Nodes.Add(node.Guid, node.GetData());
			}
		}

		private void UpdateGraphDialogNodes(List<GraphElement> allElements)
		{
			var dialogNodes = allElements.Where(x => x is DialogNode).Cast<DialogNode>().ToList();

			foreach (var node in dialogNodes)
			{
				graph.Nodes.Add(node.Guid, node.GetData());
			}
		}

		public CommentGroup CreateGroup(Vector2 position)
		{
			var group = new CommentGroup
			{
				autoUpdateGeometry = true,
				title = "Comment"
			};
			group.SetPosition(new Rect(position, new Vector2(300, 200)));
			AddElement(group);

			return group;
		}

		public void CreateNewBackendCallNode(Vector2 position)
		{
			AddElement(new BackendCallNode(position));
		}

		public void CreateNewDialogueNode(Vector2 position)
		{
			AddElement(new DialogNode(this, position));
		}
	}
}
