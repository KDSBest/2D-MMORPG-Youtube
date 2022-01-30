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
	public class Map : MonoBehaviour
	{
		public string Name = "town";
	}
}