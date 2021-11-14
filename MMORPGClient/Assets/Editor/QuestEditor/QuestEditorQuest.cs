using Common.QuestSystem;
using UnityEngine;

public class QuestEditorQuest : ScriptableObject
{
	public string Name;

	[Range(1, 99)]
	public int Level;
	public string PreQuest;

	public QuestEditorAbstractTask Task;
}
