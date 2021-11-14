using Assets.Scripts.GameDesign;
using Assets.Scripts.ScriptableObjects;
using Common.QuestSystem;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestEditorWindow : EditorWindow
{
    [MenuItem("KDSBest/Quest Editor")]
    public static void ShowWindow()
	{
		var windows = GetWindow<QuestEditorWindow>();
		windows.titleContent = new GUIContent("Quest Editor");
		windows.minSize = new Vector2(800, 600);
	}

	public void OnEnable()
	{
		VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/QuestEditor/QuestEditor.uxml");
		TemplateContainer treeAsset = original.CloneTree();
		rootVisualElement.Add(treeAsset);

		StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/QuestEditor/QuestEditor.uss");
		rootVisualElement.styleSheets.Add(styleSheet);

		CreateItemListView();
	}

	private void CreateItemListView()
	{
		var items = FindAllItems();

		ListView itemList = rootVisualElement.Query<ListView>("quest-list").First();
		itemList.makeItem = () => new Label();
		itemList.bindItem = (element, i) => (element as Label).text = items[i].Name;

		itemList.itemsSource = items;
		itemList.selectionType = SelectionType.Single;

		itemList.onSelectionChange += ItemListOnSelectionChange;
	}

	private void ItemListOnSelectionChange(IEnumerable<object> enumerable)
	{
		foreach(Quest quest in enumerable)
		{
			Box questInfo = rootVisualElement.Query<Box>("quest-info").First();
			questInfo.Clear();

			var editorQuest = new QuestEditorQuest();
			editorQuest.Name = quest.Name;
			editorQuest.Level = quest.Level;
			editorQuest.PreQuest = quest.PreQuest;
			editorQuest.Rewards = quest.Rewards;
			editorQuest.Task = (QuestEditorAbstractTask)Resources.Load($"QuestTasks/{quest.UnityReferenceTask}");

			Toolbar toolbar = new Toolbar();
			toolbar.Add(new ToolbarButton(() => OnSave(quest, editorQuest))
			{
				text = "Save",
				name = "Save"
			});
			questInfo.Add(toolbar);

			SerializedObject serializedQuest = new SerializedObject(editorQuest);
			SerializedProperty itemProperty = serializedQuest.GetIterator();
			itemProperty.Next(true);

			while (itemProperty.NextVisible(false))
			{
				PropertyField prop = new PropertyField(itemProperty);

				prop.Bind(serializedQuest);
				questInfo.Add(prop);
			}
		}
	}

	private void OnSave(Quest quest, QuestEditorQuest editorQuest)
	{
		quest.Name = editorQuest.Name;
		quest.Level = editorQuest.Level;
		quest.PreQuest = editorQuest.PreQuest;
		quest.UnityReferenceTask = editorQuest.Task.name;
		quest.Rewards = editorQuest.Rewards;
		quest.Task = ParseEditorTask(editorQuest.Task);

		string newFileName = $"{quest.Name}.quest";
		string fileName = Path.GetFileName(quest.Path);
		string path = Path.GetDirectoryName(quest.Path);
		if (newFileName != fileName)
		{
			File.Delete(quest.Path);
			quest.Path = Path.Combine(path, newFileName);
		}

		string json = JsonConvert.SerializeObject(quest, new JsonSerializerSettings()
		{
			TypeNameHandling = TypeNameHandling.Auto
		});
		File.WriteAllText(quest.Path, json);

		CreateItemListView();
	}

	private IQuestTask ParseEditorTask(QuestEditorAbstractTask task)
	{
		if (task == null)
			return null;

		switch(task)
		{
			case QuestEditorOrTask orTask:
				return new OrQuestTask()
				{
					A = ParseEditorTask(orTask.A),
					B = ParseEditorTask(orTask.B)
				};
			case QuestEditorAndTask andTask:
				return new AndQuestTask()
				{
					A = ParseEditorTask(andTask.A),
					B = ParseEditorTask(andTask.B)
				};
			case QuestEditorMobKillTask mobTask:
				return new MobKillTask()
				{
					Amount = mobTask.Amount,
					MobId = mobTask.MobId
				};
			case QuestEditorInventoryTask invTask:
				return new InventoryQuestTask()
				{
					Amount = invTask.Amount,
					ItemId = invTask.ItemId
				};
		}

		return null;
	}

	private Quest[] FindAllItems()
	{
		string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "Quests"), "*.quest", SearchOption.TopDirectoryOnly);
		Quest[] quests = new Quest[files.Length];

		for(int i = 0; i < files.Length; i++)
		{
			string json = File.ReadAllText(files[i]);
			quests[i] = JsonConvert.DeserializeObject<Quest>(json, new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.Auto
			});
			quests[i].Path = files[i];
		}

		return quests;
	}
}
