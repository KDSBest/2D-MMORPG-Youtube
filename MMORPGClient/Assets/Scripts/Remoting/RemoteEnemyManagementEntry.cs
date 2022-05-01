using Common.GameDesign;
using System;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	[Serializable]
	public class RemoteEnemyManagementEntry
	{
		public EnemyType Type;
		public GameObject Prefab;
	}
}
