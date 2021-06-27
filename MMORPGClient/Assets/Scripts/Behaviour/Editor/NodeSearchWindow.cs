using Assets.Scripts.Behaviour.Editor.Nodes;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Behaviour.Editor
{

	public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
	{
		private EditorWindow window;
		private BehaviourGraphView graphView;

		private Texture2D icon;

		public void Configure(EditorWindow window, BehaviourGraphView graphView)
		{
			this.window = window;
			this.graphView = graphView;

			icon = new Texture2D(1, 1);
			icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
			icon.Apply();
		}

		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			var tree = new List<SearchTreeEntry>
			{
				new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
				new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),
				new SearchTreeEntry(new GUIContent("Dialogue Node", icon))
				{
					level = 2, 
					userData = NodeCreationType.Dialog
				},
				new SearchTreeEntry(new GUIContent("Backend Call Node", icon))
				{
					level = 2,
					userData = NodeCreationType.BackendCall
				},
				new SearchTreeEntry(new GUIContent("Group",icon))
				{
					level = 1,
					userData = NodeCreationType.Group
				}
			};

			return tree;
		}

		public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
		{
			var mousePosition = window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent, context.screenMousePosition - window.position.position);
			var graphMousePosition = graphView.contentViewContainer.WorldToLocal(mousePosition);

			switch ((NodeCreationType)SearchTreeEntry.userData)
			{
				case NodeCreationType.Dialog:
					graphView.CreateNewDialogueNode(graphMousePosition);
					return true;
				case NodeCreationType.BackendCall:
					graphView.CreateNewBackendCallNode(graphMousePosition);
					return true;
				case NodeCreationType.Group:
					graphView.CreateGroup(graphMousePosition);
					return true;
			}

			return false;
		}
	}
}
