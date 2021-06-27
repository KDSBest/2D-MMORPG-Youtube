using System;
using System.Collections.Generic;
using System.Numerics;

namespace Assets.Scripts.Behaviour.Data.Nodes
{
	[Serializable]
	public class GroupData
	{
		public List<Guid> ChildNodes { get; set; } = new List<Guid>();
		public Vector2 Position { get; set; }
		public string Title { get; set; }
	}
}