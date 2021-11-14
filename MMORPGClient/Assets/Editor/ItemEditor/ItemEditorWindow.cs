using Assets.Scripts.GameDesign;
using Assets.Scripts.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemEditorWindow : EditorWindow
{
    [MenuItem("KDSBest/Item Editor")]
    public static void ShowWindow()
	{
		var windows = GetWindow<ItemEditorWindow>();
		windows.titleContent = new GUIContent("Item Editor");
		windows.minSize = new Vector2(800, 600);
	}

	public void OnEnable()
	{
		VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ItemEditor/ItemEditor.uxml");
		TemplateContainer treeAsset = original.CloneTree();
		rootVisualElement.Add(treeAsset);

		StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/ItemEditor/ItemEditor.uss");
		rootVisualElement.styleSheets.Add(styleSheet);

		CreateItemListView();
	}

	private void CreateItemListView()
	{
		var items = FindAllItems();

		ListView itemList = rootVisualElement.Query<ListView>("item-list").First();
		itemList.makeItem = () => new Label();
		itemList.bindItem = (element, i) => (element as Label).text = items[i].name;

		itemList.itemsSource = items;
		itemList.selectionType = SelectionType.Single;

		itemList.onSelectionChange += ItemListOnSelectionChange;
	}

	private void ItemListOnSelectionChange(IEnumerable<object> enumerable)
	{
		foreach(ItemData item in enumerable)
		{
			Box itemInfo = rootVisualElement.Query<Box>("item-info").First();
			itemInfo.Clear();

			SerializedObject serializedItem = new SerializedObject(item);
			SerializedProperty itemProperty = serializedItem.GetIterator();
			itemProperty.Next(true);

			while(itemProperty.NextVisible(false))
			{
				PropertyField prop = new PropertyField(itemProperty);

				prop.SetEnabled(itemProperty.name != "m_Script");
				prop.Bind(serializedItem);
				itemInfo.Add(prop);
			}
		}
	}

	private ItemData[] FindAllItems()
	{
		ItemProvider itemProvider = new ItemProvider();
		return itemProvider.Initialize();
	}
}
