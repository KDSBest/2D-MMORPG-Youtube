using Common.QuestSystem;
using System.Collections.Generic;
using UnityEngine;

public class QuestEditorQuest : ScriptableObject
{
	public string Name;

	[Range(1, 99)]
	public int Level;
	public string PreQuest;

	public List<QuestReward> Rewards = new List<QuestReward>();

	public QuestEditorAbstractTask Task;
}
