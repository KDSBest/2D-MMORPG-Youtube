using Assets.Scripts.Behaviour.Data;
using Assets.Scripts.Behaviour.Data.Nodes;
using Assets.Scripts.Character;
using Assets.Scripts.PubSubEvents.Dialog;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using Common.PublishSubscribe;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.NPC
{
	public class NPCController : MonoBehaviour
	{
		public string Name;
		public GameObject SelectionMarker;
		private BehaviourGraphData graph;
		private DialogNodeData currentDialogNode;
		private IPubSub pubsub;
		private ICurrentContext context;

		public void Awake()
		{
			string path = Path.Combine(Application.streamingAssetsPath, $"{Name}.behaviour");
			graph = JsonConvert.DeserializeObject<BehaviourGraphData>(File.ReadAllText(path), new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.Auto
			});
		}

		public void OnEnable()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			context = DI.Instance.Resolve<ICurrentContext>();

			pubsub.Subscribe<SelectDialogOption>(OnSelectDialogOption, this.GetType().Name);
		}

		public void OnDisable()
		{
			pubsub.Unsubscribe<SelectDialogOption>(this.GetType().Name);
		}

		private List<ChoiceData> GetAvailableChoices()
		{
			if (currentDialogNode.Choices == null || currentDialogNode.Choices.Count == 0)
			{
				return new List<ChoiceData>
				{
					new ChoiceData()
					{
						Condition = "true",
						GuidNext = Guid.Empty,
						Text = "Exit"
					}
				};
			}
			return currentDialogNode.Choices;
		}

		private void OnSelectDialogOption(SelectDialogOption data)
		{
			NextNode(GetAvailableChoices()[data.Option].GuidNext);
		}

		public void OnInteract()
		{
			EntryPointNodeData startNode = graph.Nodes[Guid.Empty] as EntryPointNodeData;
			NextNode(startNode.Start);
		}

		private void NextNode(Guid guid)
		{
			if (guid == Guid.Empty)
			{
				pubsub.Publish(new DialogDone());
				return;
			}

			ProcessNode(graph.Nodes[guid]);
		}

		private void ProcessNode(BaseNodeData node)
		{
			switch (node)
			{
				case BackendCallNodeData backend:
					NextNode(backend.NextTrue);
					break;
				case DialogNodeData dialog:
					this.currentDialogNode = dialog;
					pubsub.Publish<ShowDialog>(new ShowDialog()
					{
						Name = dialog.Name.Replace("[Player]", context.Character.Name),
						Text = dialog.Text,
						DialogOptions = GetAvailableChoices().ConvertAll<string>(x => x.Text).ToArray()
					});
					break;
			}
		}

		public void OnSelected(bool selected)
		{
			SelectionMarker.SetActive(selected);
		}
	}
}