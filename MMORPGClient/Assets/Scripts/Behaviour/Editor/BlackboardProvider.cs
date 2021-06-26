using Assets.Scripts.Behaviour.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Behaviour.Editor
{
	public class BlackboardProvider
	{
		private BehaviourGraphData graph;
		public Blackboard Blackboard { get; private set; }
		private readonly BlackboardSection blackboardSection;
		private readonly Dictionary<Guid, BlackboardRow> propertyRows = new Dictionary<Guid, BlackboardRow>();
		private readonly Texture2D icon;

		public BlackboardProvider(BehaviourGraphData graph)
		{
			icon = new Texture2D(1, 1);
			icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
			icon.Apply();

			Blackboard = new Blackboard();
			Blackboard.Add(new BlackboardSection { title = "Variables" });

			Blackboard.addItemRequested = AddItemRequested;
			Blackboard.editTextRequested = EditTextRequested;
			Blackboard.moveItemRequested = MoveItemRequested;

			Blackboard.SetPosition(new Rect(10, 30, 200, 300));

			blackboardSection = new BlackboardSection { headerVisible = false };

			OnGraphChanged(graph);

			Blackboard.Add(blackboardSection);
		}

		public void OnGraphChanged(BehaviourGraphData graph)
		{
			blackboardSection.Clear();
			this.propertyRows.Clear();

			foreach (var property in graph.Properties)
			{
				AddProperty(property);
			}

			this.graph = graph;
		}

		public void UpdateGraph()
		{
			graph.Properties.Clear();
			var properties = GraphHelper.SearchForUserData<IBehaviourGraphProperty>(this.blackboardSection);
			graph.Properties.AddRange(properties);
		}

		private void AddItemRequested(Blackboard blackboard)
		{
			var gm = new GenericMenu();
			gm.AddItem(new GUIContent("TextPlaceholder"), false, () => AddProperty(new TextBehaviourGraphProperty(), true));
			gm.ShowAsContext();
		}

		private void EditTextRequested(Blackboard blackboard, VisualElement visualElement, string newText)
		{
			var field = (BlackboardField)visualElement;
			var property = (IBehaviourGraphProperty)field.userData;
			if (!string.IsNullOrEmpty(newText) && newText != property.Name)
			{
				property.Name = newText;
				field.text = newText;
			}

			UpdateGraph();
		}

		private void MoveItemRequested(Blackboard blackboard, int newIndex, VisualElement visualElement)
		{
			var property = visualElement.userData as IBehaviourGraphProperty;
			if (property == null)
				return;

			this.graph.Properties.Remove(property);

			if (newIndex > this.graph.Properties.Count - 1)
				this.graph.Properties.Add(property);
			else
				this.graph.Properties.Insert(newIndex, property);

			OnGraphChanged(this.graph);
		}

		private void AddProperty(IBehaviourGraphProperty property, bool create = false)
		{
			if(create)
			{
				property.Guid = Guid.NewGuid();
				property.Name = property.Guid.ToString();
			}

			if (this.propertyRows.ContainsKey(property.Guid))
				return;

			var field = new BlackboardField(icon, property.Name, property.Type.ToString()) { userData = property };

			VisualElement propertyVisual = null;

			switch (property)
			{
				case TextBehaviourGraphProperty textProperty:
					var textfield = new TextField("Default Value:");
					textfield.RegisterValueChangedCallback(evt =>
					{
						textProperty.DefaultValue = evt.newValue;
					});
					textfield.SetValueWithoutNotify(textProperty.DefaultValue);
					propertyVisual = textfield;
					break;
			}

			var row = new BlackboardRow(field, propertyVisual);
			row.userData = property;
			blackboardSection.Add(row);

			this.propertyRows[property.Guid] = row;

			if (create)
			{
				row.expanded = true;
				field.OpenTextEditor();
				UpdateGraph();
			}
		}
	}
}
