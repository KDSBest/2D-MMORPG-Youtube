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
					userData = new DialogNode()
				},
				new SearchTreeEntry(new GUIContent("Comment Block",icon))
				{
					level = 1,
					userData = new CommentGroup()
				}
			};

			return tree;
		}

		public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
		{
			var mousePosition = window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent, context.screenMousePosition - window.position.position);
			var graphMousePosition = graphView.contentViewContainer.WorldToLocal(mousePosition);

			switch (SearchTreeEntry.userData)
			{
				case DialogNode dialogueNode:
					graphView.CreateNewDialogueNode(graphMousePosition);
					return true;
				case CommentGroup group:
					graphView.CreateCommentBlock(graphMousePosition);
					return true;
			}

			return false;
		}
	}
}
