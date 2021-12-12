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
using System.Linq;
using System.Threading.Tasks;
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
		private NPCScriptEngine scriptEngine = new NPCScriptEngine();

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

			return currentDialogNode.Choices.Where(x =>
			{
				var result = scriptEngine.Execute(x.Condition);
				return result.ValueNumber != 0;
			}).ToList();
		}

		private void OnSelectDialogOption(SelectDialogOption data)
		{
			NextNode(GetAvailableChoices()[data.Option].GuidNext);
		}

		public void OnInteract()
		{
			EntryPointNodeData startNode = graph.Nodes[Guid.Empty] as EntryPointNodeData;
			pubsub.Subscribe<SelectDialogOption>(OnSelectDialogOption, this.GetType().Name);
			NextNode(startNode.Start);
		}

		private void NextNode(Guid guid)
		{
			// End of Dialog Tree means guid is Empty
			if (guid == Guid.Empty)
			{
				pubsub.Publish(new DialogDone());
				pubsub.Unsubscribe<SelectDialogOption>(this.GetType().Name);
				return;
			}

			ProcessNode(graph.Nodes[guid]);
		}

		private void ProcessNode(BaseNodeData node)
		{
			switch (node)
			{
				case BackendCallNodeData backend:
					var result = scriptEngine.Execute(backend.Call);
					if (result.ValueNumber == 0)
					{
						NextNode(backend.NextFalse);
					}
					else
					{
						NextNode(backend.NextTrue);
					}
					break;
				case DialogNodeData dialog:
					this.currentDialogNode = dialog;
					pubsub.Publish<ShowDialog>(new ShowDialog()
					{
						Name = ReplaceTokens(dialog.Name),
						Text = ReplaceTokens(dialog.Text),
						DialogOptions = GetAvailableChoices().ConvertAll<string>(x => x.Text).ToArray()
					});
					break;
			}
		}

		private string ReplaceTokens(string text)
		{
			return text.Replace("[Player]", context.Character.Name)
				.Replace("[Level]", context.Character.Level.ToString());
		}

		public void OnSelected(bool selected)
		{
			SelectionMarker.SetActive(selected);
		}
	}
}