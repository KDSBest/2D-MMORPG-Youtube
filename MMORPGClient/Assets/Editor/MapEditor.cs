using Assets.Scripts.NPC;
using Common.GameDesign;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("SaveMap"))
		{
			var path = EditorUtility.SaveFilePanel("Save Map", string.Empty, string.Empty, "map");

			if (path.Length != 0)
			{
				var map = (Map)target;
				var npcs = map.GetComponentsInChildren<NPCController>().ToList();

				var mapData = new MapData();
				mapData.Name = map.Name;
				mapData.NPCs = npcs.ConvertAll(x => new MapNPCData()
				{
					IsTeleporter = x.IsTeleporter,
					Name = x.Name,
					Position = new System.Numerics.Vector2(x.transform.position.x, x.transform.position.y)
				});

				File.WriteAllText(path, JsonConvert.SerializeObject(mapData, new JsonSerializerSettings()
				{
					TypeNameHandling = TypeNameHandling.Auto,
					Formatting = Formatting.Indented
				}));
			}
		}
	}
}
