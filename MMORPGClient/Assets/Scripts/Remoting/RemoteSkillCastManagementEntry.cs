using Common.GameDesign;
using System;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	[Serializable]
	public class RemoteSkillCastManagementEntry
	{
		public SkillCastType Type;
		public GameObject Prefab;
	}
}
